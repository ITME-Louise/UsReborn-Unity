using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFiller : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isFilled = false;
    private BucketFillTrigger visual;

    private void Awake()
    {
        visual = GetComponent<BucketFillTrigger>();
        if (visual == null)
            Debug.LogWarning("BucketFillTrigger가 같은 오브젝트에 없습니다. 비주얼 교체가 실행되지 않습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFilled) return;

        if (other.CompareTag("Oasis"))
        {
            isFilled = true;

            // 머티리얼/색 교체 (비주얼 전용 스크립트 호출)
            visual?.ApplyFilledVisual();

            // 미니게임 매니저 알림
            MiniGameManager_Bear.Instance.OnBucketFilled();

            Debug.Log("[BucketFiller] 버킷에 물이 채워졌습니다.");
        }
    }

    public bool IsFilled() => isFilled;
}
