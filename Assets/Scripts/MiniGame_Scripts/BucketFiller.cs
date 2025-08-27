using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFiller : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isFilled = false;
    private BucketFillTrigger visual; // 비주얼 관리 스크립트

    private void Awake()
    {
        visual = GetComponent<BucketFillTrigger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFilled) return;

        if (other.CompareTag("Oasis"))
        {
            isFilled = true;

            // 버킷 색상/머티리얼 적용
            if (visual != null)
                visual.ApplyFilledVisual();

            // 미니게임 매니저 알림
            MiniGameManager_Bear.Instance.OnBucketFilled();

            Debug.Log("버킷에 물이 채워졌습니다.");
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }
}
