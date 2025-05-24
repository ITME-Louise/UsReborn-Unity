using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTrash : MonoBehaviour
{
    public GameObject panelToShow;

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
        if (other.CompareTag("PlayerHand"))
        {
            if (panelToShow != null)
            {
                panelToShow.SetActive(true); // 손 충돌 시 패널 활성화
                Debug.Log("패널 활성화");
            }
        }
    }
}

