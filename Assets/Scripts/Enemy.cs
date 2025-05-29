using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;

    private float lastAttackTime;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    PlayerHP playerHP = hit.GetComponent<PlayerHP>();
                    if (playerHP != null)
                    {
                        playerHP.TakeDamage((int)attackDamage);
                        lastAttackTime = Time.time;
                        break;
                    }
                }
            }
        }
    }
}
