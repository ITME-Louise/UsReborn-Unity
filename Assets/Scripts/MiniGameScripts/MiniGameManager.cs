using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    private bool plankSnapped = false;
    private bool rabbitPlaced = false;
    private bool gameEnded = false;

    private bool isTimerRunning = false;

    public GameObject successUI;
    public GameObject failUI;
    public Button retryButton;
    public float timeLimit = 30f;
    private float timer;
    public Text timerText;

    void Start()
    {
        successUI.SetActive(false);
        failUI.SetActive(false);

        timer = timeLimit;
        retryButton.onClick.AddListener(RestartGame);
    }
    void Update()
    {
        if (!gameEnded && isTimerRunning)
        {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

            if (timer <= 0f)
            {
                OnRescueFail();
            }
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnPlankSnapped()
    {
        plankSnapped = true;
        CheckRescueCondition();
    }

    public void OnRabbitPlaced()
    {
        rabbitPlaced = true;
        CheckRescueCondition();
    }

    void CheckRescueCondition()
    {
        if (plankSnapped && rabbitPlaced)
        {
            FindObjectOfType<PlankMover>().StartMoving();
        }
    }

    public void OnRescueSuccess()
    {
        if (gameEnded) return;
        gameEnded = true;
        successUI.SetActive(true);
    }

    public void OnRescueFail()
    {
        if (gameEnded) return;
        gameEnded = true;
        failUI.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnRabbitFellInWater()
    {
        OnRescueFail();
    }

    public void StartTimer()
    {
        timer = timeLimit;
        isTimerRunning = true;
    }
}
