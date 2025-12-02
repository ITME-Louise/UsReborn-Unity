using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;   // 협동 정화를 위한 컬렉션 사용

public class EnemyHP : MonoBehaviourPun
{
    public float maxHp = 100f;
    public float currentHp;

    public Slider hpSlider;

    // HP 구간 공격 체크
    private bool triggered70 = false;
    private bool triggered50 = false;
    private bool triggered30 = false;

    // ---------------- 협동 정화 관련 추가 필드 ----------------
    // 현재 이 몬스터를 정화하고 있는 플레이어들의 photonViewID 목록
    private HashSet<int> purifyingPlayers = new HashSet<int>();

    // 동시에 여러 명이 정화할 때 적용할 가속 배율 (예: 2명 이상이면 데미지 1.5배)
    [Tooltip("동시에 둘 이상이 정화할 때 적용할 추가 배율")]
    public float coopMultiplier = 1.5f;
    // -------------------------------------------------------

    void Start()
    {
        currentHp = maxHp;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }
    }

    //  광선(PollutionRayCaster)이 호출하는 메서드
    public void TakeDamage(float amount)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Master에게 데미지 처리 요청
            photonView.RPC("RPC_TakeDamage", RpcTarget.MasterClient, amount);
        }
        else
        {
            // Master가 직접 처리
            ApplyDamage(amount);
        }
    }

    // ---------------- 협동 정화 등록/해제용 메서드 추가 ----------------
    // 정화 광선을 맞추기 시작한 플레이어 등록
    public void StartPurify(int playerViewId)
    {
        if (!purifyingPlayers.Contains(playerViewId))
        {
            purifyingPlayers.Add(playerViewId);
        }
    }

    // 정화 광선을 멈춘 플레이어 해제
    public void StopPurify(int playerViewId)
    {
        if (purifyingPlayers.Contains(playerViewId))
        {
            purifyingPlayers.Remove(playerViewId);
        }
    }
    // ----------------------------------------------------------------

    // Master가 처리하는 RPC
    [PunRPC]
    void RPC_TakeDamage(float amount)
    {
        ApplyDamage(amount);
    }

    // HP 처리 전체 로직 (Master만 실행)
    void ApplyDamage(float amount)
    {
        // ---------------- 협동 정화 가속 로직 추가 ----------------
        // 현재 동시에 정화 중인 플레이어 수에 따라 데미지 배율 적용
        int activeCount = purifyingPlayers.Count;

        // 예시: 2명 이상이 동시에 정화 중이면 배율 적용
        if (activeCount >= 2)
        {
            amount *= coopMultiplier;
        }
        // ---------------------------------------------------------

        currentHp -= amount;
        currentHp = Mathf.Max(currentHp, 0);

        // UI 전체 동기화
        photonView.RPC("RPC_UpdateUI", RpcTarget.All, currentHp);

        // HP 구간 공격 체크
        CheckAttackPhase();

        // 사망 처리
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // HP 구간 (70 / 50 / 30) 진입 시 공격
    void CheckAttackPhase()
    {
        if (currentHp <= 70f && !triggered70)
        {
            triggered70 = true;
            photonView.RPC("RPC_AttackAllPlayers", RpcTarget.All);
        }

        if (currentHp <= 50f && !triggered50)
        {
            triggered50 = true;
            photonView.RPC("RPC_AttackAllPlayers", RpcTarget.All);
        }

        if (currentHp <= 30f && !triggered30)
        {
            triggered30 = true;
            photonView.RPC("RPC_AttackAllPlayers", RpcTarget.All);
        }
    }

    //  실제 플레이어들에게 데미지 주는 RPC
    [PunRPC]
    void RPC_AttackAllPlayers()
    {
        PlayerHP[] players = FindObjectsOfType<PlayerHP>();

        foreach (var player in players)
        {
            // 본인 체력만 깎기 (로컬 전용)
            if (player.photonView != null && player.photonView.IsMine)
            {
                player.TakeDamage(10);
            }
        }
    }

    // HP UI 동기화
    [PunRPC]
    void RPC_UpdateUI(float syncedHp)
    {
        currentHp = syncedHp;

        if (hpSlider != null)
            hpSlider.value = currentHp;
    }

    // 몬스터 사망 처리
    void Die()
    {
        Debug.Log("Enemy died!");

        // UI 흐름 처리
        FindObjectOfType<MultiplayUIManager>()?.OnMonsterDead();

        // 몬스터 제거 (전체 동기화)
        photonView.RPC("RPC_DestroyEnemy", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
