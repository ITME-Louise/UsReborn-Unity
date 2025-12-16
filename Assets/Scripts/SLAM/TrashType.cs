using UnityEngine;

[System.Serializable]
public class TrashType : MonoBehaviour
{
    [Header("쓰레기 분류")]
    [Tooltip("쓰레기 종류: paper, pack, can, glass, pet, plastic, vinyl")]
    public string className;

    [Header("화분 생성 위치")]
    [Tooltip("정답 시 화분이 생성될 바닥 좌표")]
    public Vector3 potSpawnPosition = Vector3.zero;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogWarning($"TrashType ({gameObject.name}): className이 비어있습니다.");
        }
    }
}