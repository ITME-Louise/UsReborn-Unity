using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cameraTransform;  // OVR Camera

    public float followDistance = 1.5f; // UI가 카메라 앞에 유지될 거리
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        // 카메라 정면 방향
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * followDistance;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // 항상 카메라를 바라보게
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}

