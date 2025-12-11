using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReefScoringZone : MonoBehaviour
{
    [Header("Center (null이면 자기 Transform)")]
    public Transform center;

    [Header("정지(쌓임) 판정")]
    public float settleSpeedThreshold = 0.05f; // m/s 이하
    public float settleTime = 0.7f;            // s 이상 유지

    [Header("즉시 득점")]
    [Tooltip("켜면 존에 들어오면 곧바로 점수(+1) 처리, 정지 판정 생략")]
    public bool scoreOnEnter = true;

    [Header("Final Stage Counter (명시 연결 권장)")]
    public ReefStageCounter targetCounter; // 스테이지3의 카운터 컴포넌트를 지정

    // 내부 상태
    readonly Dictionary<Rock, Coroutine> running = new();
    readonly HashSet<int> scoredBodies = new(); // Rigidbody 기준 1회만 득점
    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!targetCounter) targetCounter = GetComponent<ReefStageCounter>();
        if (!targetCounter) targetCounter = GetComponentInParent<ReefStageCounter>();
    }

    void Start()
    {
        if (!center) center = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        var rock = GetRock(other);
        if (rock == null) return;

        var rb = other.attachedRigidbody;
        if (rb == null) return;

        // 이미 득점한 리지드바디면 무시(콜라이더 중복 방지)
        if (scoredBodies.Contains(rb.GetInstanceID())) return;

        if (scoreOnEnter)
        {
            ScoreNow(rock, rb);
            return;
        }

        // 정지 판정 코루틴
        if (running.ContainsKey(rock)) return;
        running[rock] = StartCoroutine(WaitSettleAndScore(rock, rb));
    }

    void OnTriggerExit(Collider other)
    {
        var rock = GetRock(other);
        if (rock == null) return;

        if (running.TryGetValue(rock, out var co))
        {
            StopCoroutine(co);
            running.Remove(rock);
        }
    }

    Rock GetRock(Collider c)
    {
        if (!c) return null;
        var rb = c.attachedRigidbody;
        if (rb)
        {
            var r = rb.GetComponent<Rock>();
            if (r) return r;
        }
        return c.GetComponent<Rock>();
    }

    IEnumerator WaitSettleAndScore(Rock rock, Rigidbody rb)
    {
        float held = 0f;
        while (true)
        {
            if (!rock || !rb) yield break;

            if (!col.bounds.Contains(rock.transform.position))
            {
                Cleanup(rock);
                yield break;
            }

            if (rb.velocity.magnitude <= settleSpeedThreshold)
            {
                held += Time.deltaTime;
                if (held >= settleTime)
                {
                    ScoreNow(rock, rb);
                    Cleanup(rock);
                    yield break;
                }
            }
            else
            {
                held = 0f;
            }
            yield return null;
        }
    }

    // 무조건 +1, 한 리지드바디당 1회만
    void ScoreNow(Rock rock, Rigidbody rb)
    {
        if (!rock || !rb) return;
        if (rock.Scored) return;

        int id = rb.GetInstanceID();
        if (scoredBodies.Contains(id)) return;

        if (rock.TryMarkScored())
        {
            scoredBodies.Add(id);
            ReefMissionManager.Instance?.AddScore(1);              // +1 점수 (가산점 없음)
            if (targetCounter) targetCounter.OnScoredOne();         // 카운터 +1
            MiniGameManager_Fox.Instance?.OnZoneScored(targetCounter);
            // Debug.Log($"[ScoringZone] Scored: {id}, counter={targetCounter?.IsComplete}");
        }
    }

    void Cleanup(Rock rock)
    {
        if (running.TryGetValue(rock, out var co))
        {
            StopCoroutine(co);
            running.Remove(rock);
        }
    }
}
