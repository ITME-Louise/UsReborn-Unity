using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class EnemyHP : MonoBehaviourPun
{
    public float maxHp = 100f;
    private float currentHp;
    public Slider hpSlider;

    private bool triggered70 = false;
    private bool triggered50 = false;
    private bool triggered30 = false;

    void Start()
    {
        currentHp = maxHp;
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[EnemyHP] 충돌 감지: {other.name}, 태그: {other.tag}");

        if (other.CompareTag("PlayerHand"))
        {
            TakeDamage(10f);
        }
    }


    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        hpSlider.value = currentHp;

        CheckAttackPhase();

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    void CheckAttackPhase()
    {
        if (currentHp <= 70f && !triggered70)
        {
            photonView.RPC("AttackAllPlayers", RpcTarget.All);
            triggered70 = true;
        }
        else if (currentHp <= 50f && !triggered50)
        {
            photonView.RPC("AttackAllPlayers", RpcTarget.All);
            triggered50 = true;
        }
        else if (currentHp <= 30f && !triggered30)
        {
            photonView.RPC("AttackAllPlayers", RpcTarget.All);
            triggered30 = true;
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    [PunRPC]
    void AttackAllPlayers()
    {
        // 여기에 플레이어 전체 데미지 주는 RPC 호출 있음
        Debug.Log("Enemy triggered RPC attack to all players!");
    }
}
