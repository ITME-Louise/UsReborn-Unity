using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotSpawner : MonoBehaviour
{
    public GameObject flowerVaseStage1; // 꽃 없음
    public GameObject flowerVaseStage2; // 꽃 1송이
    public GameObject flowerVaseStage3; // 꽃 2송이
    public GameObject flowerVaseStage4; // 꽃 3송이

    public Transform spawnPoint;     // 스폰 위치

    private GameObject currentPot;
    private bool isSpawning = false;

    public void SpawnPot()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnVaseSequence());
            isSpawning = true;
        }


    }
    private IEnumerator SpawnVaseSequence()
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;

        // 1단계
        currentPot = Instantiate(flowerVaseStage1, spawnPosition, Quaternion.identity);
        Debug.Log("1단계 화분 생성");
        yield return new WaitForSeconds(10f);

        // 2단계
        Destroy(currentPot);
        currentPot = Instantiate(flowerVaseStage2, spawnPosition, Quaternion.identity);
        Debug.Log("2단계 화분 생성");
        yield return new WaitForSeconds(10f);

        // 3단계
        Destroy(currentPot);
        currentPot = Instantiate(flowerVaseStage3, spawnPosition, Quaternion.identity);
        Debug.Log("3단계 화분 생성");
        yield return new WaitForSeconds(10f);

        // 4단계 (마지막, 파괴 없음)
        Destroy(currentPot);
        Instantiate(flowerVaseStage4, spawnPosition, Quaternion.identity);
        Debug.Log("최종단계 화분 생성 (3송이)");
    }

}
