using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    public bool canMove = true; // 카메라 제어 ON/OFF 변수 추가
    public bool canWalk = false; // 이동만 허용 여부

    float rotationX = 0f; // 상하
    float rotationY = 0f; // 좌우
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return; // 전체 조작 막기

        Rotate(); // 회전은 항상 허용

        if (canWalk)
        {
            Move(); // 이동은 조건부
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
