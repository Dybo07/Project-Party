
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SpawnPlayersIn : NetworkBehaviour
{
    public List<GameObject> playerPrefabs;
    public NetworkVariable<int> playerAmount = new NetworkVariable<int>(0);
    public TMP_Text text;
    public List<GameObject> spawnedObjects = new List<GameObject>();
    public int playerIndex;
    public bool hasSetPosition;
    public int amountOfPlayersNeeded;

    private void Start()
    {
        if (IsServer)
        {
            playerAmount.Value = 0;           
        }
    }

    private void Update()
    {
        if (playerAmount.Value == amountOfPlayersNeeded)
        {
            FindObjectOfType<BoardGameManager>().enabled = true;
            Destroy(text.gameObject);
            this.enabled = false;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void HandleDisconnectionServerRpc(ulong clientId)
    {
        playerAmount.Value--;

        StartCoroutine(CheckForPrefabs());
        Debug.Log(clientId + " disconnected");
        // Optionally remove the player's color or perform other clean-up tasks here
        StartCoroutine(WaitToChangeText());
    }

    
    

    private IEnumerator CheckForPrefabs()
    {

        yield return new WaitForSeconds(0.2f);
            foreach (var clone in spawnedObjects)
            {
                if (clone == null)
                {
                    spawnedObjects.Remove(clone);
                    break; // Exit the inner loop once the clone is found
                }

            }
        
    }
   [ServerRpc(RequireOwnership = false)]
    public void ClientJoinedServerRpc(ulong client)
    {
        Debug.Log("Damn why");
        playerAmount.Value++;
        TryToInitialize(client);
        StartCoroutine(WaitToChangeText());
    }

    private IEnumerator WaitToChangeText()
    {
        yield return new WaitForSeconds(0.5f);
        ChangeTextClientRpc();
    }

    private void TryToInitialize(ulong client)
    {
        playerIndex = spawnedObjects.Count;
        CheckIfObjectExistsAlready(client);  

    }

    private void CheckIfObjectExistsAlready(ulong client)
    {
        bool playerDoesAlreadyExist = false;
        foreach (var clone in spawnedObjects)
        {
            if (playerPrefabs[playerIndex].GetComponent<PlayerIDs>().playerId == clone.GetComponent<PlayerIDs>().playerId)
            {
                playerDoesAlreadyExist = true;
                if (playerIndex >= playerPrefabs.Count - 1)
                {
                    playerIndex = 0;

                }
                else
                {
                    playerIndex++;
                }
            }
        }



        if (playerDoesAlreadyExist)
        {
            CheckIfObjectExistsAlready(client);
        }
        else
        {
            SpawnPlayer(client);
        }
    }
    private void SpawnPlayer(ulong client)
    {

        GameObject playerClone = Instantiate(playerPrefabs[playerIndex], SpawnPoints.instance.GetSpawnPosition(playerIndex), Quaternion.identity);
        spawnedObjects.Add(playerClone);

        playerClone.GetComponent<NetworkObject>().SpawnAsPlayerObject(client);
    }



    [ClientRpc]
    private void ChangeTextClientRpc()
    {
        text.text = "Waiting for Players: " + playerAmount.Value.ToString() + "/4";
        Debug.Log(playerAmount.Value);
    }
}
