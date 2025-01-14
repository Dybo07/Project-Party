using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class MiniGameManager : NetworkBehaviour
{
    public static MiniGameManager Instance;
    private void Awake()
    {
        Instance = this;
    }



    public UnityEvent OnStartMinigameEvent;


    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += OnNetworkSceneLoadComplete;
    }

    private void OnNetworkSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName.EndsWith("Minigame") && clientsCompleted.Count == NetworkManager.ConnectedClientsIds.Count)
        {
            OnStartMinigameEvent.Invoke();
            OnStartMinigameEvent.RemoveAllListeners();
        }
    }
}
