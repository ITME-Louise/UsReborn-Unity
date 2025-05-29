using UnityEngine;
using Photon.Pun;

public class PlayerCameraFollow : MonoBehaviourPun
{
    public Transform target; // 따라갈 대상
    public Vector3 offset = new Vector3(0f, 2f, -5f); // 카메라 위치 오프셋

    void LateUpdate()
    {
        if (target == null || !photonView.IsMine) return;

        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}
