using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animalPrefabs;  // 프리팹 4개를 인스펙터에서 연결
    public int spawnCount = 4;          // 몇 개 생성할지
    public Vector3 spawnAreaMin = new Vector3(-5, 0.5f, -5);  // 스폰 범위 최소
    public Vector3 spawnAreaMax = new Vector3(5, 0.5f, 5);    // 스폰 범위 최대

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPos = new Vector3(
                UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                spawnAreaMin.y,
                UnityEngine.Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );

            int prefabIndex = UnityEngine.Random.Range(0, animalPrefabs.Length);
            Instantiate(animalPrefabs[prefabIndex], randomPos, Quaternion.identity);
        }
    }
}
