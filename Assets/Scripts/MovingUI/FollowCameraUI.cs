using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cameraTransform; // OVRCameraRig/CenterEyeAnchor
    public float followDistance = 2f;
    public float heightOffset = 0f;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 카메라 앞 위치 계산
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * followDistance;
        targetPosition.y += heightOffset;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // 카메라를 바라보게 회전
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}
