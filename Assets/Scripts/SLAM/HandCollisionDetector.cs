using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask trashLayerMask; // trash
    private bool isHoldingTrash = false;
    private GameObject currentTrash = null;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & trashLayerMask) != 0 && !isHoldingTrash)
        {
            isHoldingTrash = true;
            currentTrash = other.gameObject;
            Debug.Log("손-쓰레기 충돌 인식 " + currentTrash.name);

            // ML 인식 트리거
            QuizManager.Instance?.TriggerMLRecognition(currentTrash);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentTrash)
        {
            isHoldingTrash = false;
            currentTrash = null;
            Debug.Log("손에서 쓰레기 물체 분리됨");
        }
    }
}