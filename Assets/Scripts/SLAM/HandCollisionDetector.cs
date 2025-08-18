using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionDetector : MonoBehaviour
{
    [SerializeField] private LayerMask trashLayerMask; // trash

    private bool isHoldingTrash = false;
    private GameObject currentTrash = null;

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & trashLayerMask) != 0 && !isHoldingTrash)
        {
            isHoldingTrash = true;
            currentTrash = other.gameObject;
            Debug.Log("손-쓰레기 충돌 인식 " + currentTrash.name);

            TrashRecognitionManager.Instance.OnTrashPicked(currentTrash);
        }
    }
    private void OntriggerExit(Collider other)
    {
        if (other.gameObject == currentTrash)
        {
            isHoldingTrash = false;
            currentTrash = null;
            Debug.Log("손에서 쓰레기 물체 분리됨");
        }
    }
}
