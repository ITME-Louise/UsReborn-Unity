using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RockTargetAdvance : MonoBehaviour
{
    [Tooltip("이 타겟에 돌이 들어오면 전환할 스테이지 (RockTarget_01=2, RockTarget_02=3)")]
    public int stageOnHit = 2;

    [Tooltip("점수도 1점 줄까요? (권장: false, 점수는 ScoringZone 전담)")]
    public bool awardPoint = false;   // ✅ 기본값 false

    [Tooltip("같은 돌로 중복 발동 방지 (Rigidbody 기준)")]
    public bool oneShotPerRock = true;

    [Header("정지(쌓임) 판정")]
    public float settleSpeedThreshold = 0.05f;
    public float settleTime = 0.7f;
    public float settleMaxWait = 3f;

    [Header("고정 옵션")]
    public bool freezeRockOnSettle = true;
    public bool disableRockColliderAfterSettle = true;

    private readonly HashSet<int> _firedBodyIds = new();

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb == null) return;

        int id = rb.GetInstanceID();
        if (oneShotPerRock && _firedBodyIds.Contains(id)) return;
        _firedBodyIds.Add(id);

        // 스테이지 전환(점수는 ScoringZone이 담당)
        if (awardPoint)
            MiniGameManager_Fox.Instance?.OnTargetZoneHit(stageOnHit, 1);
        else
            MiniGameManager_Fox.Instance?.ForceStage(stageOnHit);

        StartCoroutine(SettleAndFreeze(rb));
    }

    private IEnumerator SettleAndFreeze(Rigidbody rb)
    {
        if (rb == null) yield break;

        float stableTimer = 0f;
        float totalTimer = 0f;
        rb.useGravity = true;

        while (totalTimer < settleMaxWait)
        {
            float speed = rb.velocity.magnitude;

            if (speed <= settleSpeedThreshold) stableTimer += Time.deltaTime;
            else stableTimer = 0f;

            if (stableTimer >= settleTime) break;

            totalTimer += Time.deltaTime;
            yield return null;
        }

        if (freezeRockOnSettle)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            // 대안) rb.isKinematic = true;
        }

        if (disableRockColliderAfterSettle)
        {
            var colls = rb.GetComponentsInChildren<Collider>();
            foreach (var col in colls) col.enabled = false;
        }
    }
}
