using UnityEngine;

public class PollutionRayCaster : MonoBehaviour
{
    // ================== [OLD VERSION - 핀치 + 데미지] ==================
    /*
    [Header("Input (Pinch)")]
    [SerializeField] private OVRHand rightHand;   // 실제로 사용할 OVRHand

    [Header("Ray Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask pollutionLayerMask; // Pollution 레이어만 맞게

    [Header("Debug / Effect")]
    [SerializeField] private LineRenderer lineRenderer;  // 옵션
    [SerializeField] private bool showLineWhenFiring = true;   // 맞았을 때 라인 보일지

    private void Awake()
    {
        // 실제로 사용할 OVRHand 자동 할당
        if (rightHand == null)
        {
            rightHand = GetComponentInParent<OVRHand>();
            if (rightHand == null)
            {
                Debug.LogError("[PollutionRayCaster] 부모에서 OVRHand를 찾지 못했습니다.");
            }
        }
    }

    private void Update()
    {
        if (rightHand == null) return;

        // 검지 핀치 입력
        bool isPinching = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        float pinchStrength = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        // 핀치가 아니거나, 힘이 약하면 정화 중단
        if (!isPinching || pinchStrength < 0.7f)
        {
            if (lineRenderer != null && showLineWhenFiring)
            {
                lineRenderer.enabled = false;
            }
            return;
        }

        // 여기까지 왔으면: 레이가 닿아 있고, 파란 원(핀치)까지 뜬 상태
        ShootRayAlongHandPointer();
    }

    private void ShootRayAlongHandPointer()
    {
        Vector3 origin = transform.position;
        Vector3 dir    = transform.forward;

        // 기본은 라인 끄기
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // Pollution 레이어만 Raycast
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, pollutionLayerMask))
        {
            // 맞았을 때만 라인 그리기
            if (lineRenderer != null && showLineWhenFiring)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, hit.point);
            }

            // 몬스터 쪽 PollutionTarget 찾기
            var target = hit.collider.GetComponentInParent<PollutionTarget>();
            if (target != null)
            {
                float purifyAmount = Time.deltaTime;
                target.ApplyPurify(purifyAmount);
            }
        }
    }

    // ===== 예전 ShootRay() 백업 (핀치 입력용) =====
    /*
    private void ShootRay()
    {
        Vector3 origin = transform.position;
        Vector3 dir    = transform.forward;

        Debug.Log("[PollutionRayCaster] Ray Fired!");

        // 시각용 라인
        if (lineRenderer != null && showLineWhenFiring)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, origin + dir * maxDistance);
        }

        // 실제 충돌 판정
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, pollutionLayerMask))
        {
            Debug.Log($"[PollutionRayCaster] Hit: {hit.collider.name}");

            var target = hit.collider.GetComponentInParent<PollutionTarget>();

            if (target != null)
            {
                float purifyAmount = Time.deltaTime * 10f;
                target.ApplyPurify(purifyAmount);
            }
        }
        else
        {
            Debug.Log("[PollutionRayCaster] Raycast hit NOTHING");
        }
    }
    */
    
    // =============================================================


    // ================== [NEW VERSION - 레이 시각화 전용] ==================

    [Header("Ray Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask pollutionLayerMask;

    [Header("Debug / Effect")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private bool showLineWhenFiring = true;

    private void Update()
    {
        DrawRayOnly();
    }

    /// <summary>
    /// HandRayInteractor / PointerPose 방향으로 레이만 쏘고,
    /// 맞은 지점까지만 라인 렌더러를 그려줌 (데미지 계산은 안 함)
    /// </summary>
    private void DrawRayOnly()
    {
        Vector3 origin = transform.position;
        Vector3 dir    = transform.forward;

        // 기본은 라인 비활성화
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // Pollution 레이어만 Raycast
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, pollutionLayerMask))
        {
            if (lineRenderer != null && showLineWhenFiring)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, hit.point);
            }

            //  여기서는 더 이상 PollutionTarget.ApplyPurify 같은 데미지 로직 호출 안 함
            // 데미지는 RayInteractable → PollutionTarget.Start/StopPurify 로 처리
        }
    }
}
