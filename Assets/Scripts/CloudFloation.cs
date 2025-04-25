using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudFloation : MonoBehaviour
{
    // Start is called before the first frame update
    public float floatStrength = 0.5f;   // 떠오르는 높이
    public float speed = 1f;             // 떠오르는 속도

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * speed) * floatStrength, 0);
    }
}