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

    private void Update()
    {
        if (rightHand == null) return;

        // 검지 핀치 입력 (IndexPinchSelector와 같은 제스처)
        bool isPinching = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        float pinchStrength = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        if (isPinching && pinchStrength > 0.7f)
        {
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

            // 태그로 한 번 더 필터링하고 싶으면
            // if (!hit.collider.CompareTag("Pollution")) return;

            var target = hit.collider.GetComponent<PollutionTarget>();
            if (target != null)
            {
                // 한 프레임당 얼마나 정화할지 (추후 조정)
                float purifyAmount = Time.deltaTime * 10f;
                target.ApplyPurify(purifyAmount);
            }
        }
    }
}
