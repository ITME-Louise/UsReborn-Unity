using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISample : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData)
    {
        // UI Ŭ�� ���� (Debug.Log ���)
        Debug.Log($"[{gameObject.name}] UI�� Ŭ���Ǿ����ϴ�.");
    }
}
