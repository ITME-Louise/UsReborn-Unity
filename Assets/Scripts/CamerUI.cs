using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUI : MonoBehaviour
{
    public Transform cameraTransform;  // OVR Camera

    public float followDistance = 1.5f; // UI가 카메라 앞에 유지될 거리
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        // 카메라 정면 방향
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * followDistance;

      
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // 항상 카메라 앞 위치
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}

