using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform headTransform; // OVRCameraRig.CenterEyeAnchor
    private Vector3 lastPosition;
    public float realSpeed;

    void Start()
    {
        if (headTransform == null)
            headTransform = GameObject.Find("CenterEyeAnchor").transform;

        lastPosition = headTransform.position;
    }

    void Update()
    {
        Vector3 currentPosition = headTransform.position;
        float distance = Vector3.Distance(currentPosition, lastPosition);
        realSpeed = distance / Time.deltaTime; // 속도 = 거리 / 시간

        lastPosition = currentPosition;
    }
}
