using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomUIInteraction : MonoBehaviour
{
    public OVRHand hand; // 왼손 또는 오른손
    public float pinchThreshold = 0.7f; // Pinch 감도 (70%)

    void Update()
    {
        if (hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) > pinchThreshold)
        {
            TriggerUI();
        }
    }

    void TriggerUI()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
            position = new Vector2(Screen.width / 2, Screen.height / 2)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<Button>())
            {
                result.gameObject.GetComponent<Button>().onClick.Invoke();
            }
        }
    }
}
