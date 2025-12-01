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
        // Host/Owner만 실제 데미지 적용
        if (!photonView.IsMine) return;
        if (enemyHP == null) return;

        float damage = purifyAmount * damagePerSecond;
        enemyHP.TakeDamage(damage);
    }
}
