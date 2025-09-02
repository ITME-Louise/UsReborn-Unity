using UnityEngine;
using UnityEngine.UI;

public class ReefMissionManager : MonoBehaviour
{
    public static ReefMissionManager Instance;

    [Header("UI")]
    public Text scoreText;

    private int _score;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        Refresh();
    }

    public void ResetScore()
    {
        _score = 0;
        Refresh();
    }

    public void AddScore(int p)
    {
        _score += p;
        if (_score < 0) _score = 0;
        Refresh();
    }

    private void Refresh()
    {
        if (scoreText) scoreText.text = $"Score : {_score}";
    }

    public int Score => _score;
}
