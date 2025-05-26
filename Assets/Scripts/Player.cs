using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem; // 새 Input System용 네임스페이스 추가

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody rgb;
    private float speed = 10f;
    private float jumpForce = 5f;
    public PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (rgb == null)
        {
            rgb = GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (pv != null && pv.IsMine && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("space pressed");
            rgb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (pv != null && pv.IsMine)
        {
            float inputX = Keyboard.current?.aKey.isPressed == true ? -1 :
                           Keyboard.current?.dKey.isPressed == true ? 1 : 0;

            float inputY = Keyboard.current?.sKey.isPressed == true ? -1 :
                           Keyboard.current?.wKey.isPressed == true ? 1 : 0;

            rgb.velocity = new Vector3(inputX * speed, rgb.velocity.y, inputY * speed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // PhotonTransformView가 위치/회전 동기화 중이라면 생략 가능
    }
}
