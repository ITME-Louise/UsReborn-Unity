using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFillTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("URP Lit 머티리얼 할당")]
    [SerializeField] private Material filledMaterial; // URP → Lit 로 생성한 머티리얼

    private MeshRenderer[] renderers;

    private void Awake()
    {
        // 자기 자신 + 자식까지 안전하게 커버
        renderers = GetComponentsInChildren<MeshRenderer>(true);

        if (renderers == null || renderers.Length == 0)
            Debug.LogError("[BucketFillTrigger] MeshRenderer가 이 오브젝트(또는 자식)에 없습니다.");
    }

    /// <summary>버킷이 채워졌을 때 비주얼 반영</summary>
    public void ApplyFilledVisual()
    {
        if (renderers == null || renderers.Length == 0) return;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            if (filledMaterial != null)
            {
                // 단일 머티리얼 구성 가정(일반적인 케이스)
                r.material = filledMaterial;
            }
            else
            {
                // 머티리얼 미할당 시 대비책
                var mat = r.material;       // 인스턴스화된 머티리얼
                mat.color = Color.blue;     // 색상만 변경
                r.material = mat;
                Debug.LogWarning("[BucketFillTrigger] filledMaterial 미할당. 파란색으로만 변경합니다.");
            }
        }

        Debug.Log("[BucketFillTrigger] 머티리얼 교체 완료.");
    }
}
