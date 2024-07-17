using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawned : NetworkBehaviour
{
    public bool isSubscribed;
    public override void OnNetworkSpawn()
    {
        if (!isSubscribed)
        {
            Debug.Log("Subscribed");
            NetworkManager.Singleton.OnClientConnectedCallback += FindObjectOfType<SpawnPlayersIn>().ClientJoinedServerRpc;
            NetworkManager.Singleton.OnClientDisconnectCallback += FindObjectOfType<SpawnPlayersIn>().HandleDisconnectionServerRpc;
            isSubscribed = true;
        }
        else
        {
            Debug.LogWarning("HEY IDIOT NOO");
        }
        
    }

    public override void OnNetworkDespawn()
    {
        if (isSubscribed)
        {
            Debug.Log("UNSUBSCRIBED AHAHAHAHAHA");
            NetworkManager.Singleton.OnClientConnectedCallback -= FindObjectOfType<SpawnPlayersIn>().ClientJoinedServerRpc;
            NetworkManager.Singleton.OnClientDisconnectCallback -= FindObjectOfType<SpawnPlayersIn>().HandleDisconnectionServerRpc;
            isSubscribed = false;
        }
        
    }
}
