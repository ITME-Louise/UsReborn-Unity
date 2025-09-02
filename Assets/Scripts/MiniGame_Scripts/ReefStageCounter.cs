using UnityEngine;
using UnityEngine.UI;

public class ReefStageCounter : MonoBehaviour
{
    [Header("필요한 돌 개수(Goal)")]
    public int goal = 3;

    [Header("UI (선택)")]
    public Text progressText;   // "2/3" 이런 식으로 표시하고 싶으면 연결

    [SerializeField, Tooltip("현재 쌓인 개수(디버그)")]
    private int current = 0;

    public bool IsComplete => current >= goal;

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

    void OnEnable() => UpdateUI();

    void UpdateUI()
    {
        if (progressText) progressText.text = $"{current}/{goal}";
    }
}
