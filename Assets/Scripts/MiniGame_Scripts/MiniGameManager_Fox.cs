using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameManager_Fox : MonoBehaviour
{
    public static MiniGameManager_Fox Instance;

    [Header("UI")]
    public GameObject successUI;
    public GameObject failUI;
    public Button retryButton;
    public Text timerText;

    [Tooltip("튜토리얼 전체 컨테이너(패널)")]
    public GameObject tutorialPanel;

    [Tooltip("튜토리얼이 끝난 뒤 2초 표시할 '게임스타트' 패널/텍스트")]
    public GameObject gameStartPanel;

    [Header("Timer Settings")]
    public float timeLimit = 5f;   // 카운트다운 총 시간(초)
    private float timer;
    private bool isTimerRunning = false;
    private bool gameEnded = false;

    [Header("Game Progress")]
    public int totalSlotsToFill = 5;
    private int filledSlots = 0;

    [Header("Flow Settings")]
    [Tooltip("씬 시작 후 튜토리얼이 나타나기까지 지연(초)")]
    public float tutorialShowDelay = 10f;     // 요구: 10초 뒤 등장
    [Tooltip("튜토리얼 표시 시간(초)")]
    public float tutorialDuration = 8f;       // 요구: 8초 표시
    [Tooltip("'게임스타트' 표시 시간(초)")]
    public float gameStartDuration = 2f;      // 요구: 2초 표시
    [Tooltip("씬 시작 시각으로부터 타이머를 시작할 절대 시각(초)")]
    public float timerGlobalStartAt = 20f;    // 요구: 20초에 타이머 시작

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 초기 UI 상태
        if (successUI) successUI.SetActive(false);
        if (failUI) failUI.SetActive(false);

        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (gameStartPanel) gameStartPanel.SetActive(false);

        timer = timeLimit;

        if (retryButton != null)
            retryButton.onClick.AddListener(RestartGame);

        // 메인 플로우 시작
        StartCoroutine(GameFlow());
    }

    private IEnumerator GameFlow()
    {
        float sceneStartTime = Time.time;
        isTimerRunning = false;

        // 0) 튜토리얼 등장 전 10초 대기
        if (tutorialShowDelay > 0f)
            yield return new WaitForSeconds(tutorialShowDelay);

        // 1) 튜토리얼 8초 표시
        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (tutorialDuration > 0f)
            yield return new WaitForSeconds(tutorialDuration);
        if (tutorialPanel) tutorialPanel.SetActive(false);

        // 2) '게임스타트' 2초 표시
        if (gameStartPanel)
        {
            gameStartPanel.SetActive(true);
            if (gameStartDuration > 0f)
                yield return new WaitForSeconds(gameStartDuration);
            gameStartPanel.SetActive(false);
        }

        // 3) 씬 시작 기준 20초가 될 때까지 남은 시간만큼 대기
        float elapsed = Time.time - sceneStartTime;
        float remainUntilTimer = timerGlobalStartAt - elapsed;
        if (remainUntilTimer > 0f)
            yield return new WaitForSeconds(remainUntilTimer);

        // 4) 타이머 시작
        StartTimer();
    }

    void Update()
    {
        if (!gameEnded && isTimerRunning)
        {
            timer -= Time.deltaTime;
            if (timerText != null)
                timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

            if (timer <= 0f)
            {
                OnFail();
            }
        }
    }

    public void StartTimer()
    {
        timer = timeLimit;      // 카운트다운 길이
        isTimerRunning = true;
    }

    public void OnSlotFilled()
    {
        filledSlots++;
        if (filledSlots >= totalSlotsToFill)
        {
            OnSuccess();
        }
    }

    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;
        if (successUI) successUI.SetActive(true);
    }

    public void OnFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        isTimerRunning = false;
        if (failUI) failUI.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
