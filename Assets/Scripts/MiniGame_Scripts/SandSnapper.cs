using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandSnapper : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform snapTarget;         // 모래가 들어갈 퍼즐 슬롯 위치
    public float snapThreshold = 0.3f;   // 얼마나 가까이 가야 snap 처리할지
    private bool isSnapped = false;

    void Update()
    {
        if (isSnapped) return;

        float distance = Vector3.Distance(transform.position, snapTarget.position);
        if (distance < snapThreshold)
        {
            transform.position = snapTarget.position;
            transform.rotation = snapTarget.rotation;
            isSnapped = true;

            // 물리 꺼서 고정
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

           // MiniGameManager_Fox.Instance.OnSandSnapped();
        }
    }
}
