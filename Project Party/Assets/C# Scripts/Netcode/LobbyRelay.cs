using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyRelay : NetworkBehaviour
{
    public static LobbyRelay Instance;
    public LobbyUIMananager lobbyUIMananager;

    public float lobbySearchInterval = 2;
    private bool inSearchLobbyScreen;

    public TMP_InputField lobbyNameField;
    public TMP_InputField maxPlayersField;

    public TMP_InputField searchedlobbyNameField;

    public string _lobbyId;

    private RelayHostData _hostData;
    private RelayJoinData _joinData;


    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(SearchLobbiesTimer());
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await SignInAnonymouslyAsync();
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.ActiveSceneSynchronizationEnabled = true;
    }

    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            AuthenticationService.Instance.SignOut(true);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
        }
        catch (Exception ex)
        {
            print(ex);
        }
    }


    public void SyncMenuState(bool _inSearchLobbyScreen)
    {
        inSearchLobbyScreen = _inSearchLobbyScreen;
    }
    private IEnumerator SearchLobbiesTimer()
    {
        while (true)
        {
            yield return new WaitUntil(() => inSearchLobbyScreen == true);
            FindLobbies();
            yield return new WaitForSeconds(lobbySearchInterval);
        }
    }
    public async void FindLobbies()
    {
        try
        {
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                    {
                        // Only include open lobbies in the pages
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "-1")
                    },
                Order = new List<QueryOrder>
                    {
                        // Show the newest lobbies first
                        new QueryOrder(false, QueryOrder.FieldOptions.Created),
                    }
            };
            if (searchedlobbyNameField != null && !string.IsNullOrEmpty(searchedlobbyNameField.text))   //FIX THIS, REMOVE !=null!!!
            {
                queryOptions.Filters.Add(new QueryFilter(
                            field: QueryFilter.FieldOptions.Name,
                            op: QueryFilter.OpOptions.CONTAINS,
                            value: searchedlobbyNameField.text));
            }

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            List<Lobby> lobbies = response.Results;


            lobbyUIMananager.CreateLobbyUI(response.Results); 
        }
        catch (LobbyServiceException e)
        {
            print(e);
        }
    }

    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);


            string joinCode = lobby.Data["joinCode"].Value;
            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);


            _joinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _joinData.IPv4Address,
                _joinData.Port,
                _joinData.AllocationIDBytes,
                _joinData.Key,
                _joinData.ConnectionData,
                _joinData.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch(LobbyServiceException e)
        {
            print(e);
            throw;
        }
    }


    public async void CreateLobby()
    {
        int maxPlayers = 4;
        if (!string.IsNullOrEmpty(maxPlayersField.text))
        {
            maxPlayers = Mathf.Clamp(int.Parse(maxPlayersField.text),1,15);
        }

        try
        {
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxPlayers - 1);
            _hostData = new RelayHostData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                IPv4Address = allocation.RelayServer.IpV4
            };

            _hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            string lobbyName = "NewLobby";
            if (!string.IsNullOrEmpty(lobbyNameField.text))
            {
                lobbyName = lobbyNameField.text;
            }

            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;

            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "joinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: _hostData.JoinCode)
                },
            };

            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            _lobbyId = lobby.Id;

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                _hostData.IPv4Address,
                _hostData.Port,
                _hostData.AllocationIDBytes,
                _hostData.Key,
                _hostData.ConnectionData);

            NetworkManager.Singleton.StartHost();
            NetworkManager.SceneManager.LoadScene("UNO", LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            print(e);
        }
    }



    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public override void OnDestroy()
    {
        if (Application.isPlaying == true)
        {
            Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
        }
    }
    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }
    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }
}