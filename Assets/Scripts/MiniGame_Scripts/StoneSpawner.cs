using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject stonePrefab;
    public Transform[] stoneSpawnPoints;
    public Transform[] stoneSnapTargets;

    void Start()
    {
        SpawnStones();
    }

    void SpawnStones()
    {
        for (int i = 0; i < stoneSpawnPoints.Length; i++)
        {
            GameObject stone = Instantiate(stonePrefab, stoneSpawnPoints[i].position, stoneSpawnPoints[i].rotation);

            stone.name = "Stone_" + (i + 1);

            StoneSnapper snapper = stone.GetComponent<StoneSnapper>();
            if (snapper != null && i < stoneSnapTargets.Length)
            {
                snapper.snapTarget = stoneSnapTargets[i];
            }
        }
    }
}
