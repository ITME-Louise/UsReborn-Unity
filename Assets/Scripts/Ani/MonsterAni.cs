using UnityEngine;

public class MonsterAni : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("[MonsterAni] 시작! Animator: " + (animator != null));
    }

    public void Attack()
    {
        Debug.Log("[MonsterAni] Attack 호출!");
        animator.SetTrigger("Attack");
    }

    public void Hit()
    {
        Debug.Log("[MonsterAni] Hit 호출!");
        animator.SetTrigger("Hit");
    }

    public void Die()
    {
        Debug.Log("[MonsterAni] Die 호출!");
        animator.SetBool("IsDead", true);
    }
}