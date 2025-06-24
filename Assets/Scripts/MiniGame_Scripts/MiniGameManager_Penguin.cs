using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameManager_Penguin : MonoBehaviour
{
    // Start is called before the first frame update
    public static MiniGameManager_Penguin Instance;

    public GameObject successUI;
    public GameObject failUI;
    public GameObject tutorialPanel;
    public Text timerText;

    public float timeLimit = 5f;
    private float timer;
    private bool isTimerRunning = false;
    private bool gameEnded = false;

    private int snappedStones = 0;
    public int totalStonesToSnap = 5;

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
        StartCoroutine(StartGameAfterTutorial());
    }

    IEnumerator StartGameAfterTutorial()
    {
        yield return new WaitForSeconds(5f);
        tutorialPanel.SetActive(false);
        StartTimer();
    }

    void Update()
    {
        if (isTimerRunning && !gameEnded)
        {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

            if (timer <= 0f)
            {
                OnRescueFail();
            }
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        timer = timeLimit;
    }

    public void OnStoneSnapped()
    {
        snappedStones++;
        if (snappedStones >= totalStonesToSnap)
        {
            FindObjectOfType<PenguinMover>().StartMoving();
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
        SceneManager.LoadScene("MiniGame_Penguin_Scene");
    }
}
