using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class EnemyHP : MonoBehaviourPun
{
    public float maxHp = 100f;
    public float currentHp;

    public Slider hpSlider;

    // HP 구간 공격 체크
    private bool triggered70 = false;
    private bool triggered50 = false;
    private bool triggered30 = false;

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

    // Master가 처리하는 RPC
    [PunRPC]
    void RPC_TakeDamage(float amount)
    {
        ApplyDamage(amount);
    }

    // HP 처리 전체 로직 (Master만 실행)
    void ApplyDamage(float amount)
    {
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
