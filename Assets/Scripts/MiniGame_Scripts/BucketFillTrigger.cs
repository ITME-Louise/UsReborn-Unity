using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFillTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Material filledMaterial; // 물이 찼을 때 적용할 머티리얼

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    // 외부에서 호출 가능하게 public 메서드 제공
    public void ApplyFilledVisual()
    {
        if (rend != null && filledMaterial != null)
        {
            rend.material = filledMaterial; // 머티리얼 교체
        }
        else if (rend != null)
        {
            rend.material.color = Color.blue; // fallback: 색상만 파란색으로 변경
        }
    }
}
