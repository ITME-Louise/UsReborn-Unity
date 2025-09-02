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
    float timer;
    bool isTimerRunning;

    [Header("승리 조건")]
    public int finalStage = 3;                 // 마지막 스테이지 번호
    public ReefStageCounter finalStageCounter; // 스테이지3 타겟 카운터 (씬에서 연결 필수)

    [Header("성공 UI 보장 옵션")]
    public bool bringSuccessUIToFront = true;  // 성공시 최상단으로 보이게 강제
    public int successSortingOrder = 1000;     // 성공 패널 정렬 우선순위

    // 내부 상태
    bool gameEnded = false;
    int currentStage = 0; // 0: 미시작
    bool gameStarted = false;

    void Awake()
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

    void Start()
    {
        if (retryButton) retryButton.onClick.AddListener(RestartGame);

        timer = timeLimit;
        if (useTimer) UpdateTimerUI();

        StartCoroutine(TutorialFlow());
    }

    IEnumerator TutorialFlow()
    {
        if (tutorialShowDelay > 0f) yield return new WaitForSeconds(tutorialShowDelay);

        if (tutorialPanel) { tutorialPanel.SetActive(true); }
        if (tutorialDuration > 0f) yield return new WaitForSeconds(tutorialDuration);
        if (tutorialPanel) { tutorialPanel.SetActive(false); }

        if (gameStartPanel)
        {
            gameStartPanel.SetActive(true);
            if (gameStartDuration > 0f) yield return new WaitForSeconds(gameStartDuration);
            gameStartPanel.SetActive(false);
        }

        StartGame();
    }

    void Update()
    {
        // 타이머 동작
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

        // 조건 만족 즉시 성공(타이머와 무관)
        if (gameStarted && !gameEnded && CheckWinCondition())
            OnSuccess();

        // F6 디버그 토글
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

        // 점수 0으로
        ReefMissionManager.Instance?.ResetScore();
        // 카운터도 0으로
        if (finalStageCounter) finalStageCounter.ResetCount();

        if (useTimer)
        {
            timer = timeLimit;
            isTimerRunning = true;
            UpdateTimerUI();
        }
    }

    // ========= 스테이지 이동 =========
    public void ForceStage(int stage)
    {
        if (!gameStarted || gameEnded) return;
        SetStage(stage);
    }

    public void OnTargetZoneHit(int stageToForce)
    {
        if (!gameStarted || gameEnded) return;
        ForceStage(stageToForce);
    }

    void SetStage(int stage)
    {
        currentStage = stage;
        if (stoneCanvas) stoneCanvas.SetActive(stage == 1);
        if (stoneCanvas2) stoneCanvas2.SetActive(stage == 2);
        if (stoneCanvas3) stoneCanvas3.SetActive(stage == 3);
    }

    // ========= ScoringZone에서 득점 시 알림 =========
    public void OnZoneScored(ReefStageCounter counter)
    {
        if (!gameStarted || gameEnded) return;
        if (CheckWinCondition()) OnSuccess();
    }

    // ========= 성공/실패/재시작 =========
    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        // 성공 시 스테이지 3을 유지 (0으로 초기화하지 않음)
        SetStage(finalStage);

        if (extraPanel) extraPanel.SetActive(false);
        ShowSuccessUI();
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
    void UpdateTimerUI()
    {
        if (!timerText) return;
        int t = Mathf.CeilToInt(Mathf.Max(0f, timer));
        timerText.text = $"Time: {t}";
    }

    // ========= 복합 승리 조건 =========
    bool CheckWinCondition()
    {
        bool reachedFinalStage = (currentStage == finalStage);
        int totalScore = ReefMissionManager.Instance ? ReefMissionManager.Instance.Score : 0;
        bool scoreOk = totalScore >= 3;  // 전체 점수 합계 기준

        Debug.Log($"[CheckWin] stage={currentStage}, score={totalScore}, ok={reachedFinalStage && scoreOk}");
        return reachedFinalStage && scoreOk;
    }

    // ========= Helper =========
    static void SetActiveSafe(GameObject go, bool on)
    {
        if (go && go.activeSelf != on) go.SetActive(on);
    }

    void ShowSuccessUI()
    {
        if (!successUI)
        {
            Debug.LogWarning("[MiniGameManager_Fox] successUI is not assigned.");
            return;
        }

        // 부모 체인 활성화 보장
        Transform t = successUI.transform;
        while (t != null)
        {
            t.gameObject.SetActive(true);
            t = t.parent;
        }

        // 최상단 오버레이로 노출 보장(가려짐 방지)
        if (bringSuccessUIToFront)
        {
            var root = successUI;
            var canvas = root.GetComponent<Canvas>();
            if (!canvas) canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = successSortingOrder;

            var gr = root.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (!gr) root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var cg = root.GetComponent<CanvasGroup>();
            if (!cg) cg = root.AddComponent<CanvasGroup>();
            cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;

            successUI.transform.SetAsLastSibling();
        }

        successUI.SetActive(true);
        Debug.Log("[MiniGameManager_Fox] Success UI shown.");
    }
}

