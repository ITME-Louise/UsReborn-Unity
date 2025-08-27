using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearDrinkTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bucket"))
        {
            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null && rend.material.color == Color.blue)
            {
                // 곰 만족 처리
                MiniGameManager_Bear.Instance.OnBearSatisfied();

                // 버킷 제거 (한 번만 사용 가능)
                Destroy(other.gameObject);
            }
        }
    }
}
