using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    public bool canMove = true; // ī�޶� ���� ON/OFF ���� �߰�

    float rotationX = 0f; // ����
    float rotationY = 0f; // �¿�
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        // 1. Ű���� �̵�
        float horizontal = Input.GetAxis("Horizontal"); // A, D
        float vertical = Input.GetAxis("Vertical");     // W, S
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        transform.position += move * moveSpeed * Time.deltaTime;

        // 2. ���콺 ȸ�� (�¿�)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY += mouseX;
        rotationY = Mathf.Clamp(rotationY, -25f, 25f); // �¿� ����
        transform.Rotate(Vector3.up * mouseX);

        // 3. ���콺 ȸ�� (����)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -45f, 45f); // ���� ����
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
