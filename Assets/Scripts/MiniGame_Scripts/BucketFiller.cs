using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFiller : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isFilled = false;

    [SerializeField] private GameObject waterVisual; // 물이 찬 시각적 효과 (예: 파란색 물)

    private void OnTriggerEnter(Collider other)
    {
        if (isFilled) return;

        if (other.CompareTag("Oasis"))
        {
            isFilled = true;

            // 물 찬 비주얼 활성화
            if (waterVisual != null)
                waterVisual.SetActive(true);

            // MiniGameManager에 알림
            MiniGameManager_Bear.Instance.OnBucketFilled();
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }
}
