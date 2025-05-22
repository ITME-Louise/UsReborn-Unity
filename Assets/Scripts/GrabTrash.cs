using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTrash : MonoBehaviour
{
    public GameObject panelToShow; // 패널을 Drag & Drop로 할당

    private void Start()
    {
        if (panelToShow != null)
        {
            panelToShow.SetActive(false); // 시작 시 비활성화
        }
    }

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        // 태그로 손인지 판별
        if (other.CompareTag("PlayerHand"))
        {
            if (panelToShow != null)
            {
                panelToShow.SetActive(true); // 손이 닿으면 패널 활성화
                Debug.Log("손과 충돌 - 패널 활성화");
            }
        }
    }
}

