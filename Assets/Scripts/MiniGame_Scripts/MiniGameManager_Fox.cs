using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameManager_Fox : MonoBehaviour
{
    public static MiniGameManager_Fox Instance;

    [Header("UI")]
    public GameObject successUI;
    public GameObject failUI;
    public Button retryButton;
    public UnityEngine.UI.Text timerText;
    public GameObject giftPanel;
    public GameObject tipbookPanel;

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
    public int finalStage = 3;
    public ReefStageCounter finalStageCounter;

    [Header("성공 UI 보장 옵션")]
    public bool bringSuccessUIToFront = true;
    public int successSortingOrder = 1000;

    bool gameEnded = false;
    int currentStage = 0;
    bool gameStarted = false;

    // ✅ 추가: 실패 시 "3초 뒤 게임 종료" 코루틴 핸들
    private Coroutine failTimeoutCo;

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
        SetActiveSafe(giftPanel, false);
        SetActiveSafe(tipbookPanel, false);
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

    void Update()
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

        if (gameStarted && !gameEnded && CheckWinCondition())
            OnSuccess();

        var kb = Keyboard.current;
        if (kb != null && kb.f6Key.wasPressedThisFrame && gameStarted && !gameEnded)
        {
            if (stoneCanvas3 && stoneCanvas3.activeInHierarchy) stoneCanvas3.SetActive(!stoneCanvas3.activeSelf);
            else if (stoneCanvas2 && stoneCanvas2.activeInHierarchy) stoneCanvas2.SetActive(!stoneCanvas2.activeSelf);
            else if (stoneCanvas && stoneCanvas.activeInHierarchy) stoneCanvas.SetActive(!stoneCanvas.activeSelf);
            else if (extraPanel && extraPanel.activeInHierarchy) extraPanel.SetActive(!extraPanel.activeSelf);
        }
    }

    public void StartGame()
    {
        if (gameEnded) return;

        gameStarted = true;
        SetStage(1);
        if (extraPanel) extraPanel.SetActive(true);

        ReefMissionManager.Instance?.ResetScore();
        if (finalStageCounter) finalStageCounter.ResetCount();

        if (useTimer)
        {
            timer = timeLimit;
            isTimerRunning = true;
            UpdateTimerUI();
        }
    }

    public void ForceStage(int stage)
    {
        if (!gameStarted || gameEnded) return;
        SetStage(stage);
    }

    public void OnTargetZoneHit(int stageToForce)
    {
        if (!gameStarted || gameEnded) return;
        // 한 프레임 뒤에 전환(UI/카메라 갱신 보장)
        StartCoroutine(SwitchStageWithDelay(stageToForce));
    }

    // 추가: 전환 지연 코루틴(한 프레임)
    private IEnumerator SwitchStageWithDelay(int nextStage)
    {
        yield return null; // 1 frame
        SetStage(nextStage);
    }

    void SetStage(int stage)
    {
        currentStage = stage;

        if (stoneCanvas) stoneCanvas.SetActive(false);
        if (stoneCanvas2) stoneCanvas2.SetActive(false);
        if (stoneCanvas3) stoneCanvas3.SetActive(false);

        // 해당 스테이지만 켜기
        if (stage == 1 && stoneCanvas) stoneCanvas.SetActive(true);
        else if (stage == 2 && stoneCanvas2) stoneCanvas2.SetActive(true);
        else if (stage == 3 && stoneCanvas3) stoneCanvas3.SetActive(true);
    }

    public void OnZoneScored(ReefStageCounter counter)
    {
        if (!gameStarted || gameEnded) return;
        if (CheckWinCondition()) OnSuccess();
    }

    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        SetStage(finalStage);

        if (extraPanel) extraPanel.SetActive(false);
        if (stoneCanvas3) stoneCanvas3.SetActive(false);

        ShowSuccessUI();

        // 여우 원위치 복귀
        var fox = FindObjectOfType<FoxTeleporter>();
        if (fox != null)
            fox.TeleportBackToOrigin();

        // 성공 패널 3초 뒤 자동 닫기
        if (successUI) StartCoroutine(HideAfterDelay(successUI, 3f));

        // 3초 뒤 여우 애니메이션 + 대화창 실행
        StartCoroutine(PlaySuccessAfterDelay(3f));

        // 8초 뒤 선물 → 팁북 순서 → 게임 종료
        StartCoroutine(ShowGiftAndTipbookSequence());
    }

    public void OnFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        SetStage(0);
        if (extraPanel) extraPanel.SetActive(false);
        if (failUI) failUI.SetActive(true);

        // 추가: Fail UI 뜨면 Retry 버튼을 선택(포커스)해서 컨트롤러 Submit(A)로 눌리게 함
        FocusRetryButton();

        // 변경: 3초 뒤 자동 "재시작" 제거 → 3초 뒤 "게임 종료"로 변경
        if (failTimeoutCo != null) StopCoroutine(failTimeoutCo);
        failTimeoutCo = StartCoroutine(EndAfterDelay(3f));
    }

    // 추가: Retry 버튼 선택(포커스) 함수
    private void FocusRetryButton()
    {
        if (!retryButton) return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning("[MiniGameManager_Fox] EventSystem.current is null. UI Submit won't work.");
            return;
        }

        // Fail UI가 막 켜지는 타이밍이어서 한 프레임 뒤 선택이 안정적
        StartCoroutine(SelectRetryNextFrame());
    }

    //  한 프레임 뒤 선택(활성화 타이밍 안정화)
    private IEnumerator SelectRetryNextFrame()
    {
        yield return null;

        if (!retryButton) yield break;
        if (!retryButton.gameObject.activeInHierarchy) yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }

    // Retry 안 누르면 delay 후 게임 종료
    private IEnumerator EndAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (failUI) failUI.SetActive(false);
        EndGame();
    }

    // (기존) 실패 패널 3초 뒤 사라지고 게임 자동 재시작
    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (failUI) failUI.SetActive(false);
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private IEnumerator HideAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel) panel.SetActive(false);
    }

    public void RestartGame()
    {
        // 3초 뒤 게임 종료 예약이 걸려있으면 취소하고 즉시 재시작
        if (failTimeoutCo != null)
        {
            StopCoroutine(failTimeoutCo);
            failTimeoutCo = null;
        }

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    void UpdateTimerUI()
    {
        if (!timerText) return;
        int t = Mathf.CeilToInt(Mathf.Max(0f, timer));
        timerText.text = $"Time: {t}";
    }

    bool CheckWinCondition()
    {
        bool reachedFinalStage = (currentStage == finalStage);
        int totalScore = ReefMissionManager.Instance ? ReefMissionManager.Instance.Score : 0;
        bool scoreOk = totalScore >= 3;
        Debug.Log($"[CheckWin] stage={currentStage}, score={totalScore}, ok={reachedFinalStage && scoreOk}");
        return reachedFinalStage && scoreOk;
    }

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

        Transform t = successUI.transform;
        while (t != null)
        {
            t.gameObject.SetActive(true);
            t = t.parent;
        }

        if (bringSuccessUIToFront)
        {
            var root = successUI;
            var canvas = root.GetComponent<Canvas>();
            if (!canvas) canvas = root.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            var gr = root.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (!gr) root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var cg = root.GetComponent<CanvasGroup>();
            if (!cg) root.AddComponent<CanvasGroup>();
            cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;
        }

        successUI.SetActive(true);
        Debug.Log("[MiniGameManager_Fox] Success UI shown (WorldSpace in front of camera).");
    }

    // 성공 시 3초 기다린 뒤 여우 애니/대사 실행
    private IEnumerator PlaySuccessAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var foxAni = FindObjectOfType<FoxAnicontroller>();
        if (foxAni != null)
            foxAni.PlaySuccessTalkSequence();
    }

    // 8초 뒤 선물 → 2초 후 팁북 → 5초 후 게임 종료
    private IEnumerator ShowGiftAndTipbookSequence()
    {
        yield return new WaitForSeconds(8f);

        if (giftPanel)
        {
            giftPanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            giftPanel.SetActive(false);
        }

        if (tipbookPanel)
        {
            tipbookPanel.SetActive(true);
            yield return new WaitForSeconds(5f);
            tipbookPanel.SetActive(false);
        }

        EndGame();
    }

    // 게임 종료 처리
    private void EndGame()
    {
        Debug.Log("[MiniGameManager_Fox] 게임 종료됨!");
        // 필요 시 후속 동작 추가
    }
}
