using UnityEngine;

public class PollutionRayCaster : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private OVRHand rightHand;   // RightHand에 붙어있는 OVRHand

    [Header("Ray Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask pollutionLayerMask; // Pollution 레이어만 맞게

    [Header("Debug / Effect")]
    [SerializeField] private LineRenderer lineRenderer;  // 옵션
    [SerializeField] private bool showLineWhenFiring = true;

    private void Awake()
    {
        // Inspector에서 안 넣어도 부모에서 자동으로 찾기
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

        if (isPinching && pinchStrength > 0.7f)
        {
            Debug.Log("[PollutionRayCaster] Pinch detected → ShootRay()");
            ShootRay();
        }
        else
        {
            if (lineRenderer != null && showLineWhenFiring)
            {
                lineRenderer.enabled = false;
            }
        }
    }

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
}
