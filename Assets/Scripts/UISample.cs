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
        // UI 클릭 감지 (Debug.Log 출력)
        Debug.Log($"[{gameObject.name}] UI가 클릭되었습니다.");
    }
}
