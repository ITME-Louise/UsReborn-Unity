using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPlayerController : MonoBehaviourPun
{
    public float moveSpeed = 3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // 내 것이 아닌 네트워크 오브젝트는 물리 끄기
        if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
    }

    private void Update()
    {
        // 네트워크 접속 중이면, 내 것이 아닌 애는 조작 금지
        if (PhotonNetwork.IsConnected && photonView != null && !photonView.IsMine)
            return;

        // 키보드 입력 (WASD / 방향키)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 targetPos = rb.position + dir * moveSpeed * Time.deltaTime;
            rb.MovePosition(targetPos);
        }
    }
}
