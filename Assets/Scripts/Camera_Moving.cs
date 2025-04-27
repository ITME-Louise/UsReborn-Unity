using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    public bool canMove = true; // ī�޶� ���� ON/OFF ���� �߰�
    public bool canWalk = false; // �̵��� ��� ����

    float rotationX = 0f; // ����
    float rotationY = 0f; // �¿�
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return; // ��ü ���� ����

        Rotate(); // ȸ���� �׻� ���

        if (canWalk)
        {
            Move(); // �̵��� ���Ǻ�
        }
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        // rotationY = Mathf.Clamp(rotationY, -90f, 45f);
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -45f, 45f);

        transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
