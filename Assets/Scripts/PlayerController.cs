using UnityEngine;
using Photon.Pun;
using OculusSampleFramework;


public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject firePoint;
    public GameObject seedPrefab;

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
            Debug.Log("localVRCam: " + (localVRCam == null ? "null" : localVRCam.name));

            characterController = localVRCam?.GetComponent<CharacterController>();
            Debug.Log("characterController: " + (characterController == null ? "null" : "있음"));

            leftHand = GameObject.Find("LeftHandAnchor")?.GetComponentInChildren<OVRHand>();
            rightHand = GameObject.Find("RightHandAnchor")?.GetComponentInChildren<OVRHand>();
            Debug.Log("leftHand: " + (leftHand == null ? "null" : leftHand.name));
            Debug.Log("rightHand: " + (rightHand == null ? "null" : rightHand.name));
        }
    }


    void Update()
    {
        if (!photonView.IsMine || localVRCam == null || characterController == null) return;

        // ✅ 이동 처리 (조이스틱)
        Vector2 move = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 direction = new Vector3(move.x, 0, move.y);
        direction = Camera.main.transform.TransformDirection(direction);
        direction.y = 0;

        float moveSpeed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ? runSpeed : speed;
        characterController.SimpleMove(direction * moveSpeed);

        // ✅ 본체 위치 카메라 따라가기
        transform.position = localVRCam.transform.position + new Vector3(0, -1.8f, 0);
        transform.rotation = localVRCam.transform.rotation;

        // ✅ 씨앗 발사 (오른손 트리거 또는 양손 핀치)
        if (
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) ||
            (rightHand != null && rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index)) ||
            (leftHand != null && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        )
        {
            FireSeed();
        }
    }

    void FireSeed()
    {
        if (firePoint == null || seedPrefab == null)
        {
            Debug.LogWarning("FirePoint 또는 SeedPrefab이 연결되지 않았습니다.");
            return;
        }

        GameObject seed = PhotonNetwork.Instantiate("SeedProjectile", firePoint.transform.position, firePoint.transform.rotation);

        Rigidbody rb = seed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.transform.forward * 15f; // ← 여기가 핵심
            Debug.Log(" 씨앗 속도: " + rb.velocity);
        }

        Debug.Log(" 씨앗 발사 완료!");
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
}
