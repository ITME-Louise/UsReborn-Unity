using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotSpawner : MonoBehaviour
{
    [System.Serializable]
    public class VaseStages
    {
        public GameObject stage1;
        public GameObject stage2;
        public GameObject stage3;
        public GameObject stage4;
    }

    [Tooltip("클래스 이름과 해당 클래스용 화분 프리팹 세트 매핑")]
    public List<ClassVaseMapping> classVaseMappings;

    private Dictionary<string, VaseStages> vasePrefabsDict;

    [SerializeField] private float spawnDistanceThreshold = 0.5f;
    private float spawnDistanceThresholdSqr;
    private readonly List<Vector3> spawnedPositions = new();

    [System.Serializable]
    public class ClassVaseMapping
    {
        public string className;    // ex) "paper", "pack"
        public VaseStages vaseStages;
    }

    private void Awake()
    {
        spawnDistanceThresholdSqr = spawnDistanceThreshold * spawnDistanceThreshold;
        vasePrefabsDict = new Dictionary<string, VaseStages>();

        foreach (var mapping in classVaseMappings)
        {
            if (!vasePrefabsDict.ContainsKey(mapping.className))
                vasePrefabsDict.Add(mapping.className, mapping.vaseStages);
            else
                Debug.LogWarning($"중복된 클래스 이름 매핑: {mapping.className}");
        }
    }

    public void SpawnPot(Vector3 position, string className)
    {
        if (IsTooCloseToExisting(position))
        {
            Debug.Log($"중복 생성 방지됨 (위치: {position})");
            return;
        }

        spawnedPositions.Add(position);

        if (!vasePrefabsDict.TryGetValue(className, out var vaseStages))
        {
            Debug.LogWarning($"해당 클래스({className})에 대한 화분 프리팹 매핑이 없습니다.");
            return;
        }

        StartCoroutine(SpawnVaseSequence(position, vaseStages));
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

    private IEnumerator SpawnVaseSequence(Vector3 spawnPosition, VaseStages vaseStages)
    {
        GameObject pot1 = Instantiate(vaseStages.stage1, spawnPosition, Quaternion.identity);
        Debug.Log($"1단계 화분 생성 - 요청위치: {spawnPosition}, 실제위치: {pot1.transform.position}");
        yield return new WaitForSeconds(10f);

        Destroy(pot1);
        GameObject pot2 = Instantiate(vaseStages.stage2, spawnPosition, Quaternion.identity);
        Debug.Log($"2단계 화분 생성 - 요청위치: {spawnPosition}, 실제위치: {pot2.transform.position}");
        yield return new WaitForSeconds(10f);

        Destroy(pot2);
        GameObject pot3 = Instantiate(vaseStages.stage3, spawnPosition, Quaternion.identity);
        Debug.Log($"3단계 화분 생성 - 요청위치: {spawnPosition}, 실제위치: {pot3.transform.position}");
        yield return new WaitForSeconds(10f);

        Destroy(pot3);
        GameObject pot4 = Instantiate(vaseStages.stage4, spawnPosition, Quaternion.identity);
        Debug.Log($"최종단계 화분 생성 - 요청위치: {spawnPosition}, 실제위치: {pot4.transform.position}");
    }
}
