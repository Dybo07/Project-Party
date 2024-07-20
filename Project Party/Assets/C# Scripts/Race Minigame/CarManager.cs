using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CarManager : NetworkBehaviour
{
    public static CarManager Instance;
    private void Awake()
    {
        Instance = this;
    }


    public GameObject carPrefab;
    public Transform[] carStartPoints;

    public int amountOfPlayersReady;
    public double startTime;



    public CarController_Force[] cars;
    public Color[] carColors;

    public override void OnNetworkSpawn()
    {
        cars = new CarController_Force[carColors.Length];

        RequestClientSetup_ServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestClientSetup_ServerRPC()
    {
        RequestClientSetup_ClientRPC();

        amountOfPlayersReady += 1;
        if (amountOfPlayersReady == NetworkManager.ConnectedClientsIds.Count && amountOfPlayersReady > 1)
        {
            StartGame_ClientRPC(NetworkManager.ServerTime.Time, startTime);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void RequestClientSetup_ClientRPC()
    {
        SpawnCar_ServerRPC(NetworkManager.LocalClientId);
    }

    #region Spawn and Setup Car logic, prefab, and teamColor

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCar_ServerRPC(ulong clientId)
    {
        NetworkObject carNetwork;
        if (cars[clientId] == null)
        {
            GameObject carObj = Instantiate(carPrefab, carStartPoints[clientId].position, carStartPoints[clientId].rotation);
            carNetwork = carObj.GetComponent<NetworkObject>();
            carNetwork.SpawnWithOwnership(clientId, true);
        }
        else
        {
            carNetwork = cars[clientId].NetworkObject;
        }

        SetupCar_ClientRPC(carNetwork.NetworkObjectId, clientId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetupCar_ClientRPC(ulong networkObjectId, ulong clientId)
    {
        CarController_Force car = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<CarController_Force>();
        car.TeamColorMaterialColor = carColors[clientId];

        cars[clientId] = car;

        if (NetworkManager.LocalClientId == clientId)
        {
            foreach (Collider collider in car.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }
            car.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
    #endregion


    [ClientRpc(RequireOwnership = false)]
    private void StartGame_ClientRPC(double serverTime, double startDelay)
    {
        double latency = (NetworkManager.ServerTime.Time - serverTime) / 2;

        StartCoroutine(StartGameTimer(serverTime + latency + startDelay));
    }

    private IEnumerator StartGameTimer(double startTime)
    {
        yield return new WaitUntil(() => NetworkManager.ServerTime.Time >= startTime);

        cars[NetworkManager.LocalClientId].gameStarted = true;
    }
}
