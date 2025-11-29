using UnityEngine;

[System.Serializable]
public class TrashType : MonoBehaviour
{
    [Header("쓰레기 분류")]
    [Tooltip("쓰레기 종류: paper, pack, can, glass, pet, plastic, vinyl")]
    public string className;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogWarning($"TrashType ({gameObject.name}): className이 비어있습니다. 쓰레기 종류를 설정해주세요");
        }
    }
}
