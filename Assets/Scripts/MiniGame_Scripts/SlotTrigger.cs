using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sand"))
        {
            SandSnapper snapper = other.GetComponent<SandSnapper>();
            if (snapper != null)
            {
                snapper.snapTarget = transform; // 슬롯 위치로 snapTarget 설정
            }
        }
    }
}
