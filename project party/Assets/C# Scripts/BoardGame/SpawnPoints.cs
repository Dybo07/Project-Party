using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public static SpawnPoints instance;
    public List<Transform> spawnPoints;
    public List<Color> spawnColors;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public Vector3 GetSpawnPosition(int  spawnIndex)
    {
        return spawnPoints[spawnIndex].position;
    }

    public Color GetPlayerColor(int colorIndex)
    {
        return spawnColors[colorIndex];
    }
}
