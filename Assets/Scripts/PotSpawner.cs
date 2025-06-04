using System.Collections;
using UnityEngine;

public class PotSpawner : MonoBehaviour
{
    public GameObject flowerVaseStage1; // 꽃 없음
    public GameObject flowerVaseStage2; // 꽃 1송이
    public GameObject flowerVaseStage3; // 꽃 2송이
    public GameObject flowerVaseStage4; // 꽃 3송이

    public void SpawnPot(Vector3 position)
    {
        StartCoroutine(SpawnVaseSequence(position));
    }

    private IEnumerator SpawnVaseSequence(Vector3 spawnPosition)
    {
        // 1단계
        var pot1 = Instantiate(flowerVaseStage1, spawnPosition, Quaternion.identity);
        Debug.Log("1단계 화분 생성");
        yield return new WaitForSeconds(10f);

        // 2단계
        Destroy(pot1);
        var pot2 = Instantiate(flowerVaseStage2, spawnPosition, Quaternion.identity);
        Debug.Log("2단계 화분 생성");
        yield return new WaitForSeconds(10f);

        // 3단계
        Destroy(pot2);
        var pot3 = Instantiate(flowerVaseStage3, spawnPosition, Quaternion.identity);
        Debug.Log("3단계 화분 생성");
        yield return new WaitForSeconds(10f);

        // 4단계
        Destroy(pot3);
        Instantiate(flowerVaseStage4, spawnPosition, Quaternion.identity);
        Debug.Log("최종단계 화분 생성 (3송이)");
    }
}
