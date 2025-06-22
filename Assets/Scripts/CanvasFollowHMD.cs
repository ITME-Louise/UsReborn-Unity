using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFollowHMD : MonoBehaviour
{
    public Transform centerEyeAnchor;  // OVR 카메라 기준 (HMD)

    public float distanceFromCamera = 1.3f;  // 캔버스와 HMD 거리
    public float smoothSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        if (centerEyeAnchor == null)
        {
            GameObject cameraRig = GameObject.Find("OVRCameraRig");
            if (cameraRig != null)
            {
                centerEyeAnchor = cameraRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            }
        }
    }
    void LateUpdate()
    {
        if (centerEyeAnchor == null) return;

        // 시선 앞에 고정
        Vector3 targetPosition = centerEyeAnchor.position + centerEyeAnchor.forward * distanceFromCamera;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // 항상 카메라 쪽을 바라보게
        transform.rotation = Quaternion.LookRotation(transform.position - centerEyeAnchor.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
