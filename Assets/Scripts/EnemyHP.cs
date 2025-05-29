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
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!photonView.IsMine) return;

        currentHp -= amount;
        currentHp = Mathf.Max(currentHp, 0f);

        if (hpSlider != null) hpSlider.value = currentHp;

        CheckAttackPhase();

        if (currentHp <= 0f) Die();
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

    [PunRPC]
    void AttackAllPlayers()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerHP hp = player.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage((int)10f);
            }
        }
    }

    void Die()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
