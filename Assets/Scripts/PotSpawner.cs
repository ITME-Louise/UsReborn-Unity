using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotSpawner : MonoBehaviour
{
    public GameObject flowerVaseStage1;
    public GameObject flowerVaseStage2;
    public GameObject flowerVaseStage3;
    public GameObject flowerVaseStage4;

    [SerializeField] private float spawnDistanceThreshold = 0.5f;
    private float spawnDistanceThresholdSqr;
    private readonly List<Vector3> spawnedPositions = new();

    private void Awake()
    {
        spawnDistanceThresholdSqr = spawnDistanceThreshold * spawnDistanceThreshold;
    }

    public void SpawnPot(Vector3 position)
    {
        if (IsTooCloseToExisting(position))
        {
            Debug.Log($"중복 생성 방지됨 (위치: {position})");
            return;
        }

        spawnedPositions.Add(position);
        StartCoroutine(SpawnVaseSequence(position));
    }

    private bool IsTooCloseToExisting(Vector3 newPos)
    {
        foreach (var pos in spawnedPositions)
        {
            if ((pos - newPos).sqrMagnitude < spawnDistanceThresholdSqr)
                return true;
        }
        return false;
    }

    private IEnumerator SpawnVaseSequence(Vector3 spawnPosition)
    {
        GameObject pot1 = Instantiate(flowerVaseStage1, spawnPosition, flowerVaseStage1.transform.rotation);
        Debug.Log("1단계 화분 생성");
        yield return new WaitForSeconds(10f);

        Destroy(pot1);
        GameObject pot2 = Instantiate(flowerVaseStage2, spawnPosition, flowerVaseStage2.transform.rotation);
        Debug.Log("2단계 화분 생성");
        yield return new WaitForSeconds(10f);

        Destroy(pot2);
        GameObject pot3 = Instantiate(flowerVaseStage3, spawnPosition, flowerVaseStage3.transform.rotation);
        Debug.Log("3단계 화분 생성");
        yield return new WaitForSeconds(10f);

        Destroy(pot3);
        Instantiate(flowerVaseStage4, spawnPosition, flowerVaseStage4.transform.rotation);
        Debug.Log("최종단계 화분 생성 (3송이)");
    }
}
