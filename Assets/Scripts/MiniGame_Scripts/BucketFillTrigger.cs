using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketFillTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public Material filledMaterial;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bucket"))
        {
            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null && filledMaterial != null)
            {
                rend.material = filledMaterial;  // 머티리얼 자체를 교체
                Debug.Log("버킷에 물이 채워졌습니다.");

            }
            MiniGameManager_Bear.Instance.OnBucketFilled();
            Debug.Log("버킷에 물이 채워졌습니다.");

        }
    }
}
