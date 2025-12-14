using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;  // ← 추가

public class MultiplayUIManager : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject missionCard;
    [SerializeField] private GameObject tuto1;
    [SerializeField] private GameObject tuto2;
    [SerializeField] private GameObject successUI;
    [SerializeField] private GameObject failUI;
    [SerializeField] private Button retryButton;
    
    [Header("Timer UI")]
    [SerializeField] private Text timerText; // CanvasOverlay/TimeText
    
    [Header("Debug UI")]  // ← 추가
    [SerializeField] private Text debugText; // ← 추가 (Inspector에서 연결)
    
    [Header("Config")]
    [SerializeField] private int timeLimitSeconds = 180;
    [SerializeField] private float missionCardDuration = 2f;
    [SerializeField] private float tuto1Duration = 2f;
    [SerializeField] private float successHoldSeconds = 2f;
    
    float remain;
    bool running;
    bool finished;
    Coroutine flowCo;
    
    void Start()
    {
        if (retryButton)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(Restart);
        }
        SetupInitialState();
        flowCo = StartCoroutine(RunFlow());
    }

    // ← 추가
    void Update()
    {
        if (debugText)
        {
            if (PhotonNetwork.InRoom)
            {
                debugText.text = $"★ Room: {PhotonNetwork.CurrentRoom.Name}\n★ Players: {PhotonNetwork.CurrentRoom.PlayerCount}";
            }
            else
            {
                debugText.text = "Not connected to room";
            }
        }
    }
    
    void SetupInitialState()
    {
        SetActive(successUI, false);
        SetActive(failUI, false);
        SetActive(tuto1, false);
        SetActive(tuto2, false);
        SetActive(missionCard, true);
        finished = false;
        running = false;
        remain = timeLimitSeconds;
        UpdateTimer(remain);
    }

    IEnumerator RunFlow()
    {
        yield return new WaitForSeconds(missionCardDuration);
        SetActive(missionCard, false);

        SetActive(tuto1, true);
        SetActive(tuto2, true);
        yield return new WaitForSeconds(tuto1Duration);
        SetActive(tuto1, false);

        running = true;
        while (running && !finished)
        {
            remain -= Time.deltaTime;
            if (remain <= 0f)
            {
                remain = 0f;
                UpdateTimer(remain);
                Fail();
                yield break;
            }
            UpdateTimer(remain);
            yield return null;
        }
    }

    void UpdateTimer(float seconds)
    {
        if (!timerText) return;
        int s = Mathf.CeilToInt(seconds);
        int m = s / 60;
        int ss = s % 60;
        timerText.text = $"{m:00}:{ss:00}";
    }

    public void OnMonsterDead()
    {
        if (finished) return;
        finished = true;
        running = false;

        SetActive(tuto2, false);
        SetActive(successUI, true);
        StartCoroutine(HideSuccessLater());
    }

    IEnumerator HideSuccessLater()
    {
        yield return new WaitForSeconds(successHoldSeconds);
        SetActive(successUI, false);
    }

    public void Fail()
    {
        if (finished) return;
        finished = true;
        running = false;
        SetActive(failUI, true);
    }

    public void Restart()
    {
        if (flowCo != null) StopCoroutine(flowCo);
        SetupInitialState();
        flowCo = StartCoroutine(RunFlow());
    }

    static void SetActive(GameObject go, bool v)
    {
        if (go) go.SetActive(v);
    }
}
