using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;

public class LobbyUIMananager : MonoBehaviour
{
    public LobbyUIPanel[] lobbyUISlots;
    public int activeLobbyUISlots;


    public void CreateLobbyUI(List<Lobby> lobbies)
    {
        for (int i = 0; i < activeLobbyUISlots; i++)
        {
            lobbyUISlots[i].mainUI.SetActive(false);
        }

        activeLobbyUISlots = lobbies.Count;
        for (int i = 0; i < lobbies.Count; i++)
        {
            lobbyUISlots[i].mainUI.SetActive(true);
            lobbyUISlots[i].lobbyName.text = lobbies[i].Name;
            lobbyUISlots[i].lobbyId = lobbies[i].Id;

            int maxPlayers = lobbies[i].MaxPlayers;
            bool full = lobbies[i].AvailableSlots == 0;

            lobbyUISlots[i].amountOfPlayersInLobby.text = (maxPlayers - lobbies[i].AvailableSlots).ToString() + "/" + maxPlayers.ToString() + (full ? "Full!" : "");
            lobbyUISlots[i].Full = full;
        }
    }
}



