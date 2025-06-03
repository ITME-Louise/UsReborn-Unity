using UnityEngine;
using Photon.Pun;
using OculusSampleFramework;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    private CharacterController characterController;
    private GameObject localVRCam;

    private OVRHand leftHand;
    private OVRHand rightHand;

    public float speed = 3f;
    public float runSpeed = 6f;

    void Start()
    {
        if (photonView.IsMine)
        {
            localVRCam = GameObject.Find("OVRCameraRig");
            characterController = localVRCam?.GetComponent<CharacterController>();

            leftHand = GameObject.Find("LeftHandAnchor")?.GetComponentInChildren<OVRHand>();
            rightHand = GameObject.Find("RightHandAnchor")?.GetComponentInChildren<OVRHand>();
        }
    }

    void Update()
    {
        if (!photonView.IsMine || localVRCam == null || characterController == null) return;

        //  이동 처리 (조이스틱)
        Vector2 move = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 direction = new Vector3(move.x, 0, move.y);
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = 0;

        float moveSpeed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ? runSpeed : speed;
        characterController.SimpleMove(direction * moveSpeed);

        //  본체 위치 카메라 따라가기
        transform.position = localVRCam.transform.position + new Vector3(0, -1.8f, 0);
        transform.rotation = localVRCam.transform.rotation;

        //  씨앗 발사 제거됨
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}
