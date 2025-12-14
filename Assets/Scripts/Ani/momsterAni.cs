using UnityEngine;

public class MonsterAni : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 공격 애니메이션
    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    // 피격 애니메이션
    public void Hit()
    {
        animator.SetTrigger("Hit");
    }

    // 사망 애니메이션
    public void Die()
    {
        animator.SetBool("IsDead", true);
    }
}
