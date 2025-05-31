using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject stonePrefab;
    public Transform spawnPoint;             // 1개의 생성 위치
    public Transform[] stoneSnapTargets;     // 5개의 스냅 목표 위치

    void Start()
    {
        SpawnStones();
    }

    void SpawnStones()
    {
        for (int i = 0; i < stoneSnapTargets.Length; i++)
        {
            GameObject stone = Instantiate(stonePrefab, spawnPoint.position, spawnPoint.rotation);
            stone.name = "Stone_" + (i + 1);

            StoneSnapper snapper = stone.GetComponent<StoneSnapper>();
            if (snapper != null)
            {
                snapper.snapTarget = stoneSnapTargets[i];
            }
        }
    }
}
