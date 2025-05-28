using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInputTest : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject targetToMove;
    public Transform destination;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ¿ÞÂÊ Å¬¸¯
        {
            if (targetToMove != null && destination != null)
            {
                targetToMove.transform.position = destination.position;
            }
        }
    }
}
