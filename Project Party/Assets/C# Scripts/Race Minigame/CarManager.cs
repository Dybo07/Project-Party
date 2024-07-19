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



    public CarController_Force[] cars;
    public Color[] carColors;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            cars = new CarController_Force[carColors.Length];
        }
        RequestClientSetup_ServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestClientSetup_ServerRPC()
    {
        RequestClientSetup_ClientRPC();
    }
    [ClientRpc(RequireOwnership = false)]
    private void RequestClientSetup_ClientRPC()
    {
        SpawnCar_ServerRPC(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCar_ServerRPC(ulong clientId)
    {
        NetworkObject carNetwork;
        if (cars[clientId] == null)
        {
            GameObject carObj = Instantiate(carPrefab, carStartPoints[clientId].position, carStartPoints[clientId].rotation);
            carNetwork = carObj.GetComponent<NetworkObject>();
            carNetwork.SpawnWithOwnership(clientId, true);

            CarController_Force targetCar = carObj.GetComponent<CarController_Force>();

            cars[clientId] = targetCar;
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

        if (NetworkManager.LocalClientId == clientId)
        {
            foreach (Collider collider in car.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }
            car.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
