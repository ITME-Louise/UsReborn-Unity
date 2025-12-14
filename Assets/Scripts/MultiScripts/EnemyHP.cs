using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;

public class EnemyHP : MonoBehaviourPun
{
    public float maxHp = 100f;
    public float currentHp;

    public Slider hpSlider;

    // HP 구간 공격 체크
    private bool triggered70 = false;
    private bool triggered50 = false;
    private bool triggered30 = false;

    // ---------------- 애니메이션 연결 ----------------
    private MonsterAni monsterAni;
    // ------------------------------------------------

    // ---------------- 사운드 관련 ----------------
    public AudioSource idleSound;
    public AudioSource attackSound;
    public AudioSource deathSound;
    // --------------------------------------------

    // ---------------- 협동 정화 관련 추가 필드 ----------------
    private HashSet<int> purifyingPlayers = new HashSet<int>();

    [Tooltip("동시에 둘 이상이 정화할 때 적용할 추가 배율")]
    public float coopMultiplier = 1.5f;
    // -------------------------------------------------------

    void Start()
    {
        currentHp = maxHp;

        // MonsterAni 찾기
        monsterAni = GetComponentInChildren<MonsterAni>();

        // ---------------- Idle 사운드 재생 ----------------
        if (idleSound != null) 
        {
            idleSound.Play();
        }
        // --------------------------------------------------

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

    // ---------------- 협동 정화 등록/해제용 메서드 ----------------
    public void StartPurify(int playerViewId)
    {
        if (!purifyingPlayers.Contains(playerViewId))
        {
            purifyingPlayers.Add(playerViewId);
        }
    }

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
        int activeCount = purifyingPlayers.Count;

        if (activeCount >= 2)
        {
            amount *= coopMultiplier;
        }

        currentHp -= amount;
        currentHp = Mathf.Max(currentHp, 0);

        // ---------------- 디버그 추가 ----------------
        Debug.Log("[EnemyHP] ApplyDamage 호출! monsterAni: " + (monsterAni != null));
        // ---------------------------------------------

        photonView.RPC("RPC_PlayHit", RpcTarget.All);
        photonView.RPC("RPC_UpdateUI", RpcTarget.All, currentHp);

        CheckAttackPhase();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    void RPC_PlayHit()
    {
        if (monsterAni != null)
        {
            monsterAni.Hit();
        }
    }

    // HP 구간 (70 / 50 / 30) 진입 시 공격
    void CheckAttackPhase()
    {
        if (currentHp <= 70f && !triggered70)
        {
            triggered70 = true;
            photonView.RPC("RPC_AttackAllPlayers", RpcTarget.All, 20); // 70% → 20 데미지
        }

        if (currentHp <= 50f && !triggered50)
        {
            triggered50 = true;
            photonView.RPC("RPC_AttackAllPlayers", RpcTarget.All, 30); // 50% → 30 데미지
        }

        if (currentHp <= 30f && !triggered30)
        {
            triggered30 = true;
            photonView.RPC("RPC_AttackAllPlayers", RpcTarget.All, 40); // 30% → 40 데미지
        }
    }

    //  실제 플레이어들에게 데미지 주는 RPC
    [PunRPC]
    void RPC_AttackAllPlayers(int damage)
    {
        // ---------------- 공격 애니메이션 재생 ----------------
        if (monsterAni != null)
        {
            monsterAni.Attack();
        }
        // -----------------------------------------------------

        // ---------------- 공격 사운드 재생 ----------------
        if (attackSound != null)
        {
            if (idleSound != null) idleSound.Pause();
            attackSound.PlayOneShot(attackSound.clip);
        }
        // --------------------------------------------------

        PlayerHP[] players = FindObjectsOfType<PlayerHP>();

        foreach (var player in players)
        {
            // 본인 체력만 깎기 (로컬 전용)
            if (player.photonView != null && player.photonView.IsMine)
            {
                player.TakeDamage(damage); // 구간별 다른 데미지 적용
            }
        }

        // ---------------- 공격 끝나면 Idle 다시 재생 ----------------
        if (idleSound != null && currentHp > 0)
        {
            Invoke("ResumeIdleSound", 1f);
        }
        // -----------------------------------------------------------
    }

    // Idle 사운드 다시 재생
    void ResumeIdleSound()
    {
        if (idleSound != null && currentHp > 0)
        {
            idleSound.UnPause();
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

        // ---------------- 사망 사운드 재생 ----------------
        if (idleSound != null) idleSound.Stop();
        if (deathSound != null) deathSound.PlayOneShot(deathSound.clip);
        // --------------------------------------------------

        // ---------------- 사망 애니메이션 재생 ----------------
        if (monsterAni != null)
        {
            monsterAni.Die();
        }
        // -----------------------------------------------------

        // UI 흐름 처리
        FindObjectOfType<MultiplayUIManager>()?.OnMonsterDead();

        // 몬스터 제거 (전체 동기화) - 애니메이션 보여주려고 2초 딜레이
        Invoke("DestroyMonster", 2f);
    }

    void DestroyMonster()
    {
        photonView.RPC("RPC_DestroyEnemy", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_DestroyEnemy()
    {
        Destroy(gameObject);
    }
}