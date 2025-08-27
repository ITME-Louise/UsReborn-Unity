using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class FoxSimpleWalker : MonoBehaviour
{
    [Header("Target (미리 배치한 목적지)")]
    [SerializeField] private Transform target;
    [SerializeField] private float stopDistance = 0.20f;     // 목표 앞에서 멈출 거리

    [Header("Delay")]
    [SerializeField] private bool autoStartAfterDelay = true;
    [SerializeField] private float startDelay = 10f;

    [Header("Move (Physics 기반)")]
    [SerializeField] private float moveSpeed = 1.5f;         // m/s
    [SerializeField] private float turnDegPerSec = 360f;     // 초당 회전 각도(도)

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private string walkBoolParam = "IsWalking";  // Idle↔Walk 전환 Bool
    [SerializeField] private string speedParam = "Speed";      // 블렌드트리(선택)
    [SerializeField] private float speedDamp = 0.12f;

    [Header("도착 후 회전")]
    [SerializeField] private bool rotateBackOnArrive = true;   // ★ 기본: 켜둠
    [SerializeField] private float rotateBackDegreesY = -90f;   // ★ 도착 뒤 -90°
    [SerializeField] private float rotateBackDuration = 0.25f;  // 부드러운 회전 시간

    private bool isMoving;
    private Rigidbody rb;

    void Reset()
    {
        anim = GetComponent<Animator>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponent<Animator>();
        anim.applyRootMotion = false; // 위치/회전은 코드·물리로 제어

        rb = GetComponent<Rigidbody>();
        // 물리 이동 세팅(끊김 방지)
        rb.isKinematic = false;                                   // 동적 바디
        rb.interpolation = RigidbodyInterpolation.Interpolate;    // 보간 ON
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // 옆으로 기울 방지
    }

    void Start()
    {
        if (autoStartAfterDelay) StartCoroutine(DelayThenStart());
    }

    System.Collections.IEnumerator DelayThenStart()
    {
        yield return new WaitForSeconds(startDelay);
        StartMove();
    }

    [ContextMenu("Start Move Now")]
    public void StartMove()
    {
        if (!target)
        {
            Debug.LogWarning("[FoxSimpleWalker] target이 비어 있어요. 씬의 목적지 Transform을 연결하세요.");
            return;
        }
        isMoving = true;
    }

    void FixedUpdate()
    {
        if (!isMoving || !target) return;

        // 목표까지의 '평면' 벡터/거리
        Vector3 to = target.position - rb.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist > stopDistance)
        {
            // 1) 회전: 이동(떠나는) 방향을 향해 각속도로 부드럽게
            if (to.sqrMagnitude > 1e-6f)
            {
                Quaternion targetRot = Quaternion.LookRotation(to);
                Quaternion nextRot = Quaternion.RotateTowards(rb.rotation, targetRot, turnDegPerSec * Time.fixedDeltaTime);
                rb.MoveRotation(nextRot);
            }

            // 2) 전진: 오버슈트 금지(남은 거리만큼만 이동)
            float remaining = Mathf.Max(0f, dist - stopDistance);
            float stepLen = Mathf.Min(moveSpeed * Time.fixedDeltaTime, remaining);
            Vector3 step = rb.rotation * Vector3.forward * stepLen; // 현재 바라보는 정면 기준

            rb.MovePosition(rb.position + step);

            // 3) 애니: 걷기 유지(+ 블렌드트리면 Speed를 감쇠로)
            if (anim)
            {
                anim.SetBool(walkBoolParam, true);
                if (HasParam(speedParam))
                {
                    float normalized = (moveSpeed > 1e-6f)
                        ? stepLen / (moveSpeed * Time.fixedDeltaTime)  // 0~1
                        : 0f;
                    anim.SetFloat(speedParam, normalized, speedDamp, Time.fixedDeltaTime);
                }
            }
        }
        else
        {
            // 도착: 멈추고 애니 Idle로
            isMoving = false;
            if (anim)
            {
                anim.SetBool(walkBoolParam, false);
                if (HasParam(speedParam))
                    anim.SetFloat(speedParam, 0f, speedDamp, Time.fixedDeltaTime);
            }

            if (rotateBackOnArrive)
                StartCoroutine(RotateByY_Fixed(rotateBackDegreesY, rotateBackDuration));
        }
    }

    // 물리 프레임에 맞춘 부드러운 회전(-90° 등)
    private System.Collections.IEnumerator RotateByY_Fixed(float degreesY, float duration)
    {
        Quaternion from = rb.rotation;
        Quaternion to = rb.rotation * Quaternion.Euler(0f, degreesY, 0f);

        if (duration <= 0f) { rb.MoveRotation(to); yield break; }

        float t = 0f;
        while (t < duration)
        {
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            Quaternion next = Quaternion.Slerp(from, to, k);
            rb.MoveRotation(next);
        }
        rb.MoveRotation(to);
    }

    private bool HasParam(string p)
    {
        if (string.IsNullOrEmpty(p) || anim == null) return false;
        foreach (var prm in anim.parameters)
            if (prm.name == p) return true;
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!target) return;
        Gizmos.color = Color.yellow;
        Vector3 from = Application.isPlaying && rb ? rb.position : transform.position;
        Gizmos.DrawLine(from, target.position);
        Gizmos.DrawWireSphere(target.position, stopDistance);
    }
#endif
}
