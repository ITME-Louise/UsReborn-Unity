using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem; // F6 디버그 토글용

public class MiniGameManager_Fox : MonoBehaviour
{
    public static MiniGameManager_Fox Instance;

    [Header("UI")]
    public GameObject successUI;
    public GameObject failUI;
    public Button retryButton;
    public Text timerText;

    [Header("Tutorial / Start UI")]
    public GameObject tutorialPanel;   // 10초 후 켜짐
    public GameObject gameStartPanel;  // 8초 경과 후 2초 표시

    [Header("Panels")]
    [Tooltip("20s에 켜질 패널")]
    public GameObject extraPanel;      // (예: ExtraPanel)
    [Tooltip("21s~25s에 보였다가 꺼질 패널")]
    public GameObject stoneCanvas;     // (예: StoneCanvas)
    [Tooltip("25s~28s")]
    public GameObject stoneCanvas2;    // (예: StoneCanvas2)
    [Tooltip("28s~30s")]
    public GameObject stoneCanvas3;    // (예: StoneCanvas3)

    [Header("Timer (옵션)")]
    public float timeLimit = 5f;       // 필요 시 사용
    private float timer;
    private bool isTimerRunning;

    [Header("Flow Flags")]
    private bool gameEnded;

    [Header("Tutorial Durations")]
    public float tutorialShowDelay = 10f; // 0~10s 대기
    public float tutorialDuration = 8f;   // 10~18s 튜토 패널
    public float gameStartDuration = 2f;  // 18~20s 게임스타트

    [Header("Absolute Schedule (sec from scene start)")]
    public float extraOnAt = 20f; // extraPanel ON
    public float stone1OnAt = 21f; // stoneCanvas ON
    public float stone1OffAt = 25f; // stoneCanvas OFF
    public float stone2OnAt = 25f; // stoneCanvas2 ON
    public float stone2OffAt = 28f; // stoneCanvas2 OFF
    public float stone3OnAt = 28f; // stoneCanvas3 ON
    public float stone3OffAt = 30f; // stoneCanvas3 OFF
    public float successAt = 32f;   // 성공 패널 표시
    public float successOffAt = 35f; // NEW: 35초에 성공 패널 OFF

    [Header("Restart After End")]
    [Tooltip("성공/실패 이후 자동 재시작할지 (기본: 꺼짐)")]
    public bool autoRestartOnEnd = false;
    public float restartDelay = 2f;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);

        // 이름 자동 탐색(선택)
        if (!extraPanel) { var f = GameObject.Find("extraPanel") ?? GameObject.Find("ExtraPanel"); if (f) extraPanel = f; }
        if (!stoneCanvas) { var f = GameObject.Find("stoneCanvas") ?? GameObject.Find("StoneCanvas"); if (f) stoneCanvas = f; }
        if (!stoneCanvas2) { var f = GameObject.Find("stoneCanvas2") ?? GameObject.Find("StoneCanvas2"); if (f) stoneCanvas2 = f; }
        if (!stoneCanvas3) { var f = GameObject.Find("stoneCanvas3") ?? GameObject.Find("StoneCanvas3"); if (f) stoneCanvas3 = f; }

        // 시작 즉시 OFF
        if (extraPanel && extraPanel.activeSelf) extraPanel.SetActive(false);
        if (stoneCanvas && stoneCanvas.activeSelf) stoneCanvas.SetActive(false);
        if (stoneCanvas2 && stoneCanvas2.activeSelf) stoneCanvas2.SetActive(false);
        if (stoneCanvas3 && stoneCanvas3.activeSelf) stoneCanvas3.SetActive(false);
        if (successUI) successUI.SetActive(false);
        if (failUI) failUI.SetActive(false);
    }

    private void Start()
    {
        // 튜토/스타트 패널 OFF로 시작
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (gameStartPanel) gameStartPanel.SetActive(false);

        // Canvas 정렬순서만 보정(※ renderMode는 건드리지 않음: 비율깨짐 방지)
        SetupCanvasOrder(extraPanel);
        SetupCanvasOrder(stoneCanvas);
        SetupCanvasOrder(stoneCanvas2);
        SetupCanvasOrder(stoneCanvas3);

        timer = timeLimit;
        if (retryButton) retryButton.onClick.AddListener(RestartGame);

        StartCoroutine(GameFlow());
    }

    private void SetupCanvasOrder(GameObject go)
    {
        if (!go) return;
        var cv = go.GetComponentInParent<Canvas>();
        if (cv) cv.sortingOrder = Mathf.Max(cv.sortingOrder, 500);
    }

    private IEnumerator WaitUntilAbs(float targetSec, float t0)
    {
        float remain = targetSec - (Time.time - t0);
        if (remain > 0f) yield return new WaitForSeconds(remain);
    }

    private IEnumerator GameFlow()
    {
        float t0 = Time.time;
        isTimerRunning = false; // 필요시 panel 타이밍에 맞춰 StartTimer() 호출

        // 0~10s : 대기
        if (tutorialShowDelay > 0f) yield return new WaitForSeconds(tutorialShowDelay);
        if (gameEnded) yield break;

        // 10~18s : 튜토리얼
        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (tutorialDuration > 0f) yield return new WaitForSeconds(tutorialDuration);
        if (gameEnded) yield break;
        if (tutorialPanel) tutorialPanel.SetActive(false);

        // 18~20s : 게임스타트
        if (gameStartPanel)
        {
            gameStartPanel.SetActive(true);
            if (gameStartDuration > 0f) yield return new WaitForSeconds(gameStartDuration);
            if (gameEnded) yield break;
            gameStartPanel.SetActive(false);
        }

        // 20s : extraPanel ON
        yield return WaitUntilAbs(extraOnAt, t0);
        if (gameEnded) yield break;
        if (extraPanel) extraPanel.SetActive(true);
        Debug.Log("[Flow] extraPanel ON (20s)");

        // 21s : stoneCanvas ON
        yield return WaitUntilAbs(stone1OnAt, t0);
        if (gameEnded) yield break;
        if (stoneCanvas) stoneCanvas.SetActive(true);
        Debug.Log("[Flow] StoneCanvas ON (21s)");

        // 25s : stoneCanvas OFF + stoneCanvas2 ON
        yield return WaitUntilAbs(stone1OffAt, t0);
        if (gameEnded) yield break;
        if (stoneCanvas) stoneCanvas.SetActive(false);
        if (stoneCanvas2) stoneCanvas2.SetActive(true);
        Debug.Log("[Flow] StoneCanvas OFF, StoneCanvas2 ON (25s)");

        // 28s : stoneCanvas2 OFF + stoneCanvas3 ON
        yield return WaitUntilAbs(stone2OffAt, t0);
        if (gameEnded) yield break;
        if (stoneCanvas2) stoneCanvas2.SetActive(false);
        if (stoneCanvas3) stoneCanvas3.SetActive(true);
        Debug.Log("[Flow] StoneCanvas2 OFF, StoneCanvas3 ON (28s)");

        // 30s : stoneCanvas3 OFF
        yield return WaitUntilAbs(stone3OffAt, t0);
        if (gameEnded) yield break;
        if (stoneCanvas3) stoneCanvas3.SetActive(false);
        Debug.Log("[Flow] StoneCanvas3 OFF (30s)");

        // 32s : 성공 패널 표시 (자동 재시작 없음)
        yield return WaitUntilAbs(successAt, t0);
        if (gameEnded) yield break;
        OnSuccess(); // gameEnded = true 로 전환
        Debug.Log("[Flow] Success shown (32s)");

        // NEW: 35s 에 성공 패널 끄기(절대시간). gameEnded 여도 동작하도록 별도 코루틴 실행.
        if (successOffAt > successAt)
            StartCoroutine(HideSuccessAtAbs(successOffAt, t0));
    }

    // NEW: 성공 패널을 절대 시각에 끄는 코루틴
    private IEnumerator HideSuccessAtAbs(float targetSec, float t0)
    {
        float remain = targetSec - (Time.time - t0);
        if (remain > 0f) yield return new WaitForSeconds(remain);
        if (successUI && successUI.activeSelf)
        {
            successUI.SetActive(false);
            Debug.Log("[Flow] Success hidden (" + targetSec + "s)");
        }
    }

    private void Update()
    {
        // (옵션) 타이머 사용 시만
        if (!gameEnded && isTimerRunning)
        {
            timer -= Time.deltaTime;
            if (timerText) timerText.text = "Time: " + Mathf.Ceil(timer);
            if (timer <= 0f) OnFail();
        }

        // 디버그: F6 → 현재 켜져있는 패널 토글(우선순위: 3 > 2 > 1 > extra)
        var kb = Keyboard.current;
        if (kb != null && kb.f6Key.wasPressedThisFrame)
        {
            if (stoneCanvas3 && stoneCanvas3.activeInHierarchy) stoneCanvas3.SetActive(!stoneCanvas3.activeSelf);
            else if (stoneCanvas2 && stoneCanvas2.activeInHierarchy) stoneCanvas2.SetActive(!stoneCanvas2.activeSelf);
            else if (stoneCanvas && stoneCanvas.activeInHierarchy) stoneCanvas.SetActive(!stoneCanvas.activeSelf);
            else if (extraPanel && extraPanel.activeInHierarchy) extraPanel.SetActive(!extraPanel.activeSelf);
        }
    }

    // === 공개 API ===
    public void StartTimer()
    {
        timer = timeLimit;
        isTimerRunning = true;
    }

    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        // 진행 중 패널 모두 OFF (성공 UI만 켬)
        if (extraPanel) extraPanel.SetActive(false);
        if (stoneCanvas) stoneCanvas.SetActive(false);
        if (stoneCanvas2) stoneCanvas2.SetActive(false);
        if (stoneCanvas3) stoneCanvas3.SetActive(false);

        if (successUI) successUI.SetActive(true);

        if (autoRestartOnEnd)
            StartCoroutine(RestartGameDelayed(restartDelay)); // 기본값 false이므로 실행 안 됨
    }

    public void OnFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;

        if (extraPanel) extraPanel.SetActive(false);
        if (stoneCanvas) stoneCanvas.SetActive(false);
        if (stoneCanvas2) stoneCanvas2.SetActive(false);
        if (stoneCanvas3) stoneCanvas3.SetActive(false);

        if (failUI) failUI.SetActive(true);

        if (autoRestartOnEnd)
            StartCoroutine(RestartGameDelayed(restartDelay)); // 기본값 false
    }

    private IEnumerator RestartGameDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestartGame();
    }

    public void RestartGame()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
