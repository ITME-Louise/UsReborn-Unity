using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MiniGameManager_Fox : MonoBehaviour
{
    public static MiniGameManager_Fox Instance;

    [Header("UI")]
    public GameObject successUI;
    public GameObject failUI;
    public Button retryButton;
    public Text timerText;

    [Header("Tutorial / Start UI (자동 타이머)")]
    public GameObject tutorialPanel;
    public GameObject gameStartPanel;
    public float tutorialShowDelay = 10f;
    public float tutorialDuration = 8f;
    public float gameStartDuration = 2f;

    [Header("Panels (스테이지)")]
    [Tooltip("Stage 1")] public GameObject stoneCanvas;
    [Tooltip("Stage 2")] public GameObject stoneCanvas2;
    [Tooltip("Stage 3")] public GameObject stoneCanvas3;
    [Tooltip("점수/HUD/안내 등(게임 시작 후 켜짐)")]
    public GameObject extraPanel;

    [Header("Timer")]
    public bool useTimer = true;
    public float timeLimit = 60f;
    private float timer;
    private bool isTimerRunning;

    [Header("승리 조건")]
    public int finalStage = 3;                      // 마지막 스테이지 번호
    public ReefStageCounter finalStageCounter;      // 스테이지3 타겟 카운터(필수)

    // 내부 상태
    private bool gameEnded = false;
    private int currentStage = 0;  // 0: 미시작
    private bool gameStarted = false;

    // ========= Unity =========
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SetActiveSafe(successUI, false);
        SetActiveSafe(failUI, false);
        SetActiveSafe(stoneCanvas, false);
        SetActiveSafe(stoneCanvas2, false);
        SetActiveSafe(stoneCanvas3, false);
        SetActiveSafe(extraPanel, false);

        SetActiveSafe(tutorialPanel, false);
        SetActiveSafe(gameStartPanel, false);
    }

    private void Start()
    {
        if (retryButton) retryButton.onClick.AddListener(RestartGame);

        timer = timeLimit;
        if (useTimer) UpdateTimerUI();

        StartCoroutine(TutorialFlow());
    }

    private IEnumerator TutorialFlow()
    {
        if (tutorialShowDelay > 0f) yield return new WaitForSeconds(tutorialShowDelay);

        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (tutorialDuration > 0f) yield return new WaitForSeconds(tutorialDuration);
        if (tutorialPanel) tutorialPanel.SetActive(false);

        if (gameStartPanel)
        {
            gameStartPanel.SetActive(true);
            if (gameStartDuration > 0f) yield return new WaitForSeconds(gameStartDuration);
            gameStartPanel.SetActive(false);
        }

        StartGame();
    }

    private void Update()
    {
        if (useTimer && gameStarted && !gameEnded && isTimerRunning)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (timer <= 0f)
            {
                timer = 0f;
                if (!gameEnded)
                {
                    if (CheckWinCondition()) OnSuccess();
                    else OnFail();
                }
            }
        }

        var kb = Keyboard.current;
        if (kb != null && kb.f6Key.wasPressedThisFrame && gameStarted && !gameEnded)
        {
            if (stoneCanvas3 && stoneCanvas3.activeInHierarchy) stoneCanvas3.SetActive(!stoneCanvas3.activeSelf);
            else if (stoneCanvas2 && stoneCanvas2.activeInHierarchy) stoneCanvas2.SetActive(!stoneCanvas2.activeSelf);
            else if (stoneCanvas && stoneCanvas.activeInHierarchy) stoneCanvas.SetActive(!stoneCanvas.activeSelf);
            else if (extraPanel && extraPanel.activeInHierarchy) extraPanel.SetActive(!extraPanel.activeSelf);
        }
    }

    // ========= 게임 시작 =========
    public void StartGame()
    {
        if (gameEnded) return;

        gameStarted = true;
        SetStage(1);
        if (extraPanel) extraPanel.SetActive(true);

        ReefMissionManager.Instance?.ResetScore();

        if (useTimer)
        {
            timer = timeLimit;
            isTimerRunning = true;
            UpdateTimerUI();
        }
    }

    // === 득점 이벤트(이제 점수는 ScoringZone만 처리하도록 비활성화해도 됨) ===
    public void OnTargetHit(int points = 1)
    {
        if (!gameStarted || gameEnded) return;
        // 점수 처리는 ReefScoringZone에서만 수행. 여기선 승리조건만 체크 가능.
        if (CheckWinCondition()) OnSuccess();
    }

    // === ScoringZone에서 득점 시 알림 ===
    public void OnZoneScored(ReefStageCounter zone)
    {
        if (!gameStarted || gameEnded) return;
        if (CheckWinCondition()) OnSuccess();
    }

    // ========= 스테이지 이동 =========
    public void ForceStage(int stage)
    {
        if (!gameStarted || gameEnded) return;
        SetStage(stage);
        if (CheckWinCondition()) OnSuccess();
    }

    public void OnTargetZoneHit(int stageToForce, int pointsIgnored = 1)
    {
        if (!gameStarted || gameEnded) return;
        // 점수는 ScoringZone이 전담 -> 여기선 순수 스테이지 전환만
        ForceStage(stageToForce);
    }

    private void SetStage(int stage)
    {
        currentStage = stage;

        if (stoneCanvas) stoneCanvas.SetActive(stage == 1);
        if (stoneCanvas2) stoneCanvas2.SetActive(stage == 2);
        if (stoneCanvas3) stoneCanvas3.SetActive(stage == 3);
    }

    // ========= 성공/실패/재시작 =========
    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        SetStage(0);
        if (extraPanel) extraPanel.SetActive(false);
        if (successUI) successUI.SetActive(true);
    }

    public void OnFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        SetStage(0);
        if (extraPanel) extraPanel.SetActive(false);
        if (failUI) failUI.SetActive(true);
    }

    public void RestartGame()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    // ========= 타이머 UI =========
    private void UpdateTimerUI()
    {
        if (!timerText) return;
        int t = Mathf.CeilToInt(Mathf.Max(0f, timer));
        timerText.text = $"Time: {t}";
    }

    // ========= 복합 승리 조건 =========
    private bool CheckWinCondition()
    {
        bool reachedFinalStage = (currentStage == finalStage);
        bool counterOk = (finalStageCounter && finalStageCounter.IsComplete);
        return reachedFinalStage && counterOk;
    }

    // ========= Helper =========
    private static void SetActiveSafe(GameObject go, bool on)
    {
        if (go && go.activeSelf != on) go.SetActive(on);
    }
}
