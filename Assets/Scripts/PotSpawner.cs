using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; // Pot 프리팹
    public Transform spawnPoint;     // 스폰 위치

    public void SpawnPot()
    {
        if (prefabToSpawn != null)
        {
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            Debug.Log("Pot 프리팹 생성");
        }
        else
        {
            Debug.LogWarning("프리팹이 할당되지 않았습니다.");
        }
    }
}
