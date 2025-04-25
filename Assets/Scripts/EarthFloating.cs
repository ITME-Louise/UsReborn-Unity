using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthFloating : MonoBehaviour
{
    // Start is called before the first frame update
    public float floatStrength = 0.5f;   // �������� ����
    public float speed = 1f;
    public float rotationSpeed = 20f;    // ȸ�� �ӵ�

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * speed) * floatStrength, 0);

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);     // Z�� ȸ��
    }
}