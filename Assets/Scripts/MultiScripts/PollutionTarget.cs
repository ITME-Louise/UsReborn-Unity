using UnityEngine;
using Photon.Pun;

public class PollutionTarget : MonoBehaviourPun
{
    [Header("연결")]
    [SerializeField] private EnemyHP enemyHP;

    [Header("정화 데미지 세기")]
    [Tooltip("1초 동안 레이로 계속 쏘고 있을 때 깎일 HP 양")]
    [SerializeField] private float damagePerSecond = 10f;

    private void Awake()
    {
        if (enemyHP == null)
            enemyHP = GetComponent<EnemyHP>();
    }

    /// <summary>
    /// PollutionRayCaster에서 매 프레임 호출해주는 함수
    /// purifyAmount는 Time.deltaTime 기반으로 들어옴
    /// </summary>
    public void ApplyPurify(float purifyAmount)
    {
        // EnemyHP.TakeDamage 안에서 MasterClient 처리까지 해주므로
        // 여기서는 Master 검사 없이 바로 데미지 전달만 한다.
        if (enemyHP == null) return;

        float damage = purifyAmount * damagePerSecond;
        enemyHP.TakeDamage(damage);
    }
}
