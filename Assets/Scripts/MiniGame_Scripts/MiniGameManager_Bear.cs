using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameManager_Bear : MonoBehaviour
{
    // Start is called before the first frame update
    public static MiniGameManager_Bear Instance;

    public GameObject successUI;
    public GameObject failUI;
    public GameObject tutorialPanel;
    public Button retryButton;
    public Text timerText;
    public GameObject tipbookPanel;

    public float timeLimit = 180f;
    private float timer;
    private bool isTimerRunning = false;
    private bool gameEnded = false;

    private int filledBucketCount = 0;   // 물 채워진 버킷 개수
    private int satisfiedBearCount = 0;  // 버킷 3개 받은 곰의 수

    public int totalBucketsToFill = 3;   // 버킷 3개 채워야 함
    public int totalBearsToSatisfy = 1;  // 곰 한 마리만 만족시키면 됨

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        successUI.SetActive(false);
        failUI.SetActive(false);
        tutorialPanel.SetActive(true);
        if (tipbookPanel) tipbookPanel.SetActive(false);


        timer = timeLimit;
        retryButton.onClick.AddListener(RestartGame);
        StartCoroutine(StartAfterTutorial());
    }

    IEnumerator StartAfterTutorial()
    {
        yield return new WaitForSeconds(5f);
        tutorialPanel.SetActive(false);
        StartTimer();
    }

    void Update()
    {
        if (!gameEnded && isTimerRunning)
        {
            timer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(timer).ToString();



            if (timer <= 0f)
            {
                OnFail();
            }
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        timer = timeLimit;
    }

    // 버킷이 물로 채워졌을 때
    public void OnBucketFilled()
    {
        filledBucketCount++;
        Debug.Log($"[GameManager] 현재 채워진 버킷 수: {filledBucketCount}");
        CheckForSuccess();
    }

    // 곰이 버킷 3개를 받았을 때
    public void OnBearSatisfied()
    {
        satisfiedBearCount++;
        Debug.Log($"[GameManager] 만족한 곰 수: {satisfiedBearCount}");
        CheckForSuccess();
    }

    private void CheckForSuccess()
    {
        // 버킷 3개 채우고, 곰 한 마리 만족시키면 성공
        if (filledBucketCount >= totalBucketsToFill && satisfiedBearCount >= totalBearsToSatisfy)
        {
            OnSuccess();
        }
    }

    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        successUI.SetActive(true);
        isTimerRunning = false;
        Debug.Log("[GameManager] 게임 성공!");
        StartCoroutine(ShowTipbookAfterDelay(3f));
    }
    IEnumerator ShowTipbookAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tipbookPanel) tipbookPanel.SetActive(true);
    }
    public void OnFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        failUI.SetActive(true);
        isTimerRunning = false;
        Debug.Log("[GameManager] 게임 실패!");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
