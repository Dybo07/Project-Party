using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FallingTileManager : NetworkBehaviour
{
    public GameObject tilePrefab;

    public Transform gridCenter;
    public Vector2 gridSize;

    public List<Vector3> tileSpawnPositions;

    public float startGameTime;

    public AnimationCurve tileSpawnRate;
    public int tilesPlaced;

    public float tileSize;

    public List<int> tileToSpawnIds;
    public List<float> tileSpawnClockTimes;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SetupGrid();
            StartCoroutine(CalculateNewTile_OnServer());
        }
        StartCoroutine(SpawnTileDelay());
    }
    private void SetupGrid()
    {
        for (int x = 0; x < gridSize.x / tileSize; x++)
        {
            for (int z = 0; z < gridSize.y / tileSize; z++)
            {
                Vector3 worldBottomLeft = gridCenter.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;
                Vector3 _worldPos = worldBottomLeft + Vector3.right * (x * tileSize + tileSize / 2) + Vector3.forward * (z * tileSize + tileSize / 2);

                tileSpawnPositions.Add(_worldPos);
            }
        }
    }



    private IEnumerator CalculateNewTile_OnServer()
    {
        yield return new WaitForSeconds(startGameTime);

        float tileDelay = 0;
        while (true)
        {
            yield return new WaitForSeconds(tileDelay);
            SyncTile_ClientRPC(NetworkManager.ServerTime.TimeAsFloat + 2, Random.Range(0, tileSpawnPositions.Count));

            tileDelay = tileSpawnRate.Evaluate(tilesPlaced);
            tilesPlaced += 1;
            print(tileDelay);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncTile_ClientRPC(float timeToActivateAt, int spawnPointId)
    {
        tileSpawnClockTimes.Add(timeToActivateAt);
        tileToSpawnIds.Add(spawnPointId);
    }
    private IEnumerator SpawnTileDelay()
    {
        while (true)
        {
            yield return new WaitUntil(() => tileSpawnClockTimes.Count != 0);

            yield return new WaitUntil(() => NetworkManager.ServerTime.TimeAsFloat >= tileSpawnClockTimes[0]);

            SpawnTile(tileToSpawnIds[0]);

            tileSpawnClockTimes.RemoveAt(0);
            tileToSpawnIds.RemoveAt(0);
        }
    }
    private void SpawnTile(int tileSpawnId)
    {
        Instantiate(tilePrefab, tileSpawnPositions[tileSpawnId], Quaternion.identity);
    }




    private void OnDrawGizmos()
    {
        if (tileSize == 0)
        {
            return;
        }
        for (int x = 0; x < gridSize.x / tileSize; x++)
        {
            for (int z = 0; z < gridSize.x / tileSize; z++)
            {
                Vector3 worldBottomLeft = gridCenter.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;
                Vector3 _worldPos = worldBottomLeft + Vector3.right * (x * tileSize + tileSize / 2) + Vector3.forward * (z * tileSize + tileSize / 2);

                Gizmos.DrawWireCube(_worldPos, new Vector3(tileSize, 1, tileSize));
            }
        }
    }
}
