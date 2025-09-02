using UnityEngine;
using UnityEngine.UI;

public class ReefStageCounter : MonoBehaviour
{
    [Header("필요한 돌 개수")]
    public int goal = 3;                    // 인스펙터에 보이는 Goal

    [Header("UI (선택)")]
    public Text progressText;               // 인스펙터에 보이는 Progress Text

    [SerializeField, Tooltip("현재 쌓인 개수(디버그 표시)")]
    private int current = 0;

    // MiniGameManager_Fox가 참조하는 승리 판정용 프로퍼티
    public bool IsComplete => current >= goal;

    // ReefScoringZone에서 득점 시 호출
    public void OnScoredOne()
    {
        current = Mathf.Max(0, current + 1);
        UpdateUI();
    }

    public void ResetCount()
    {
        current = 0;
        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (progressText) progressText.text = $"{current}/{goal}";
    }
}
