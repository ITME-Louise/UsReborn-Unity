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

    public float timeLimit = 5f;
    private float timer;
    private bool isTimerRunning = false;
    private bool gameEnded = false;

    private int filledBucketCount = 0;
    private int satisfiedBearCount = 0;
    public int totalBucketsToFill = 3;
    public int totalBears = 3;

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
            timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

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

    public void OnBucketFilled()
    {
        filledBucketCount++;
        CheckForSuccess();
    }

    public void OnBearSatisfied()
    {
        satisfiedBearCount++;
        CheckForSuccess();
    }
    private void CheckForSuccess()
    {
        if (filledBucketCount >= totalBucketsToFill && satisfiedBearCount >= totalBears)
        {
            OnSuccess();
        }
    }

    public void OnSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        successUI.SetActive(true);
    }

    public void OnFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        failUI.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
