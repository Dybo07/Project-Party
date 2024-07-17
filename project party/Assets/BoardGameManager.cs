using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Unity.Netcode;
using UnityEngine;

public class BoardGameManager : NetworkBehaviour
{
    public enum GAMESTATE
    {
        BeginBoardGame,
        StartTurn,
        Moving,
        EndTurn
    }
    public GAMESTATE state;
    public List<Transform> spawnPlaces;
    public Transform cameraSpawnPlace;
    public Transform cam;
    public List<Transform> players;
    public int playerIndex;
    public GameObject dicePrefab;

    public float height;
    private void Start()
    {
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Player").Length; i++)
        {
            GameObject.FindGameObjectsWithTag("Player")[i].transform.position = spawnPlaces[i].position;
            players.Add(GameObject.FindGameObjectsWithTag("Player")[i].transform);
        }

        cam.position = cameraSpawnPlace.position;
        SwitchStates(GAMESTATE.StartTurn);
    }

    public void SwitchStates(GAMESTATE gameState)
    {
        state = gameState;

        switch(state)
        {
            case GAMESTATE.StartTurn:
                if (!IsServer) return;
                StartCoroutine(StartTurn());
                break;
        }
    }

    private IEnumerator StartTurn()
    {
        yield return new WaitForSeconds(2);
        GameObject clonedDice = Instantiate(dicePrefab, players[playerIndex].transform.position + new Vector3(0, height, 0), Quaternion.identity);
        clonedDice.GetComponent<NetworkObject>().Spawn();

        cam.position = players[playerIndex].GetChild(0).position;
        cam.rotation = players[playerIndex].GetChild(0).rotation;

        players[playerIndex].GetComponent<PlayerTurnScript>().isTurn = true;


    }

    


}
