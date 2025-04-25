using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Moving : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    float rotationX = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 키보드 이동
        float horizontal = Input.GetAxis("Horizontal"); // A, D
        float vertical = Input.GetAxis("Vertical");     // W, S
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        transform.position += move * moveSpeed * Time.deltaTime;

        // 2. 마우스 회전 (좌우)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // 3. 마우스 회전 (상하)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 위아래 회전 제한
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
