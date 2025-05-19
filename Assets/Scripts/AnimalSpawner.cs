using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public GameObject[] animalPrefabs;  // ������ 4���� �ν����Ϳ��� ����
    public int spawnCount = 4;          // �� �� ��������
    public Vector3 spawnAreaMin = new Vector3(-5, 0.5f, -5);  // ���� ���� �ּ�
    public Vector3 spawnAreaMax = new Vector3(5, 0.5f, 5);    // ���� ���� �ִ�

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
