using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MultiplayUIManager : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject missionCard;
    [SerializeField] private GameObject tuto1;
    [SerializeField] private GameObject tuto2;
    [SerializeField] private GameObject successUI;
    [SerializeField] private GameObject failUI;
    [SerializeField] private GameObject tipBook;  // ← 추가
    [SerializeField] private Button retryButton;
    
    [Header("Timer UI")]
    [SerializeField] private Text timerText;
    
    [Header("Debug UI")]
    [SerializeField] private Text debugText;
    
    [Header("Config")]
    [SerializeField] private int timeLimitSeconds = 180;
    [SerializeField] private float missionCardDuration = 2f;
    [SerializeField] private float tuto1Duration = 2f;
    [SerializeField] private float successHoldSeconds = 2f;
    [SerializeField] private float tipBookDuration = 5f;  // ← 추가
    
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
        SetActive(tipBook, false);  // ← 추가
        SetActive(tuto1, false);
        SetActive(tuto2, false);
        SetActive(missionCard, true);
        SetActive(retryButton?.gameObject, false);  // ← 수정: 처음엔 숨김
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
        StartCoroutine(SuccessFlow());  // ← 수정
    }
    
    IEnumerator SuccessFlow()  // ← 수정: TipBook 플로우 추가
    {
        // Success UI 표시
        yield return new WaitForSeconds(successHoldSeconds);
        SetActive(successUI, false);
        
        // TipBook 표시
        SetActive(tipBook, true);
        yield return new WaitForSeconds(tipBookDuration);
        SetActive(tipBook, false);
    }
    
    public void Fail()
    {
        if (finished) return;
        finished = true;
        running = false;
        SetActive(failUI, true);
        SetActive(retryButton?.gameObject, true);  // ← 수정: 실패 시에만 표시
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