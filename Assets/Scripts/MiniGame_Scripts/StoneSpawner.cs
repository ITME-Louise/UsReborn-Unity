using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject stonePrefab;
    public Transform spawnPoint;
    public Transform[] stoneSnapTargets; // Snap 위치들 (5개)

    private int currentStoneIndex = 0;

    void Start()
    {
        SpawnNextStone();
    }

    public void SpawnNextStone()
    {
        if (currentStoneIndex >= stoneSnapTargets.Length)
            return;

        GameObject stone = Instantiate(stonePrefab, spawnPoint.position, spawnPoint.rotation);
        stone.name = "Stone_" + (currentStoneIndex + 1);

        StoneSnapper snapper = stone.GetComponent<StoneSnapper>();
        if (snapper != null)
        {
            snapper.snapTarget = stoneSnapTargets[currentStoneIndex];
            snapper.spawner = this;
        }

        currentStoneIndex++;
    }
}
