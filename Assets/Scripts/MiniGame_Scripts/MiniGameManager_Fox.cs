using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameManager_Fox : MonoBehaviour
{
    // Start is called before the first frame update
    public static MiniGameManager_Fox Instance;

    [Header("UI")]
    public GameObject successUI;
    public GameObject failUI;
    public Button retryButton;
    public Text timerText;
    public GameObject tutorialPanel;

    [Header("Timer Settings")]
    public float timeLimit = 5f;
    private float timer;
    private bool isTimerRunning = false;
    private bool gameEnded = false;

    [Header("Game Progress")]
    public int totalSlotsToFill = 5;
    private int filledSlots = 0;

    [Header("Tutorial Settings")]
    public float tutorialDuration = 5f;

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
        isTimerRunning = false;
        yield return new WaitForSeconds(tutorialDuration);
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
        timer = timeLimit;
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
