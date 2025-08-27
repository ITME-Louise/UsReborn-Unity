using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FoxTeleporter : MonoBehaviour
{
    [Header("Target (목적지)")]
    [SerializeField] private Transform target;

    [Header("When (언제 텔레포트?)")]
    [SerializeField] private float teleportDelay = 10f;   // 씬 시작 후 몇 초 뒤 텔레포트

    [Header("Where (어디에 텔레포트?)")]
    [Tooltip("타겟에 딱 붙지 않고 앞에서 멈추려면 켜기")]
    [SerializeField] private bool stopBeforeTarget = true;
    [SerializeField] private float stopDistance = 0.20f;

    [Tooltip("Y 높이: 현재 높이를 유지할지 여부 (끄면 target.y + yOffset)")]
    [SerializeField] private bool keepCurrentY = true;
    [SerializeField] private float yOffset = 0f;

    [Header("지면 스냅(선택)")]
    [Tooltip("지면에 붙이고 싶으면 켜기. 마스크 지정하면 그 레이어만 사용")]
    [SerializeField] private bool snapToGround = false;
    [SerializeField] private LayerMask groundMask = 0;   // 0이면 모든 레이어
    [SerializeField] private float rayStartHeight = 1.0f;
    [SerializeField] private float footClearance = 0.02f;

    private Animator anim;
    private Rigidbody rb;   // 있어도/없어도 동작 (회전은 건드리지 않음)

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        if (anim) anim.applyRootMotion = false; // 루트모션은 사용 안함
    }

    void Start()
    {
        StartCoroutine(TeleportFlow());
    }

    private System.Collections.IEnumerator TeleportFlow()
    {
        yield return new WaitForSeconds(teleportDelay);
        TeleportNow();
    }

    [ContextMenu("Teleport Now")]
    public void TeleportNow()
    {
        if (!target)
        {
            Debug.LogWarning("[FoxTeleporter] target이 비어 있어요.");
            return;
        }

        // 1) 목표 위치 계산(수평 방향)
        Vector3 dir = target.position - transform.position; dir.y = 0f;
        Vector3 goal = target.position;

        if (stopBeforeTarget && dir.sqrMagnitude > 1e-6f)
            goal -= dir.normalized * stopDistance;

        // 2) Y 처리
        goal.y = keepCurrentY ? transform.position.y + yOffset
                              : target.position.y + yOffset;

        // 3) (선택) 지면에 붙이기
        if (snapToGround)
        {
            Vector3 rayOrigin = goal + Vector3.up * rayStartHeight;
            if (groundMask != 0)
            {
                if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayStartHeight + 3f, groundMask))
                    goal.y = hit.point.y + footClearance;
            }
            else
            {
                if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayStartHeight + 3f))
                    goal.y = hit.point.y + footClearance;
            }
        }

        // 4) 위치만 텔레포트 (회전은 절대 변경하지 않음)
        if (rb && !rb.isKinematic)
        {
            rb.position = goal;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = goal;
        }

        // 5) 애니 상태 정리(선택)
        if (anim) anim.SetBool("IsWalking", false);
    }
}
