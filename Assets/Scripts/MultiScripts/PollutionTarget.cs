using UnityEngine;
using Photon.Pun;

public class PollutionTarget : MonoBehaviourPun
{
    [Header("연결")]
    [SerializeField] private EnemyHP enemyHP;

    [Header("정화 데미지 세기")]
    [Tooltip("1초 동안 레이로 계속 쏘고 있을 때 깎일 HP 양")]
    [SerializeField] private float damagePerSecond = 5f;

    // ================== [OLD VERSION - 보관용] ==================
    /*
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
    */
    // ========================================================

    // ▼ 새로운 방식: RayInteractable(파란 원) 선택 상태 기반 정화

    // 현재 이 몬스터가 "선택(핀치)"되어 정화 중인지 여부
    private bool isPurifying = false;

    private void Awake()
    {
        if (enemyHP == null)
            enemyHP = GetComponent<EnemyHP>();
    }

    private void Update()
    {
        // 선택되어 있는 동안에만 HP 감소
        if (!isPurifying) return;

        // 1초 동안 damagePerSecond 만큼 깎이도록
        float purifyAmount = Time.deltaTime;
        ApplyPurify(purifyAmount);
    }

    /// <summary>
    /// 실제로 EnemyHP에 데미지를 전달하는 함수 (내부 로직은 예전과 같음)
    /// </summary>
    public void ApplyPurify(float purifyAmount)
    {
        if (enemyHP == null) return;

        float damage = purifyAmount * damagePerSecond;
        enemyHP.TakeDamage(damage);
    }

    // RayInteractable의 When Select 이벤트에서 호출
    public void StartPurify()
    {
        isPurifying = true;
    }

    // RayInteractable의 When Unselect 이벤트에서 호출
    public void StopPurify()
    {
        isPurifying = false;
    }
}
