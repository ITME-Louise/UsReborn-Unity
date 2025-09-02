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

    [Header("즉시 득점 옵션")]
    [Tooltip("켜면 존에 들어오는 즉시 점수 처리(정지판정 생략)")]
    public bool scoreOnEnter = true;

    // 내부 상태
    private readonly Dictionary<Rock, Coroutine> running = new();
    private readonly HashSet<int> scoredBodies = new(); // Rigidbody 기준 1회만 득점
    private Collider col;
    private ReefStageCounter cachedCounter; // 같은 오브젝트에 붙는 카운터(선택)

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        cachedCounter = GetComponent<ReefStageCounter>(); // 있으면 캐시
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

        if (scoredBodies.Contains(rb.GetInstanceID())) return;

        if (scoreOnEnter)
        {
            ScoreNow(rock, rb);
            return;
        }

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

    // 무조건 +1, 한 리지드바디당 1회
    void ScoreNow(Rock rock, Rigidbody rb)
    {
        if (!rock || !rb) return;
        if (rock.Scored) return;

        int id = rb.GetInstanceID();
        if (scoredBodies.Contains(id)) return;

        if (rock.TryMarkScored())
        {
            scoredBodies.Add(id);
            ReefMissionManager.Instance?.AddScore(1);          // +1
            if (cachedCounter) cachedCounter.OnScoredOne();     // 카운터 증가
            MiniGameManager_Fox.Instance?.OnZoneScored(cachedCounter);
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
