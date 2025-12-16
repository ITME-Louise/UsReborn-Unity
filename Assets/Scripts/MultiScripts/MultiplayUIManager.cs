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
    [SerializeField] private GameObject tipBook;
    [SerializeField] private Button retryButton;
    
    [Header("Sound")]
    [SerializeField] private AudioSource clearSound;
    [SerializeField] private AudioSource glowSound;
    [SerializeField] private AudioSource failSound;
    
    [Header("Timer UI")]
    [SerializeField] private Text timerText;
    
    [Header("Debug UI")]
    [SerializeField] private Text debugText;
    
    [Header("Config")]
    [SerializeField] private int timeLimitSeconds = 180;
    [SerializeField] private float missionCardDuration = 2f;
    [SerializeField] private float tuto1Duration = 2f;
    [SerializeField] private float successHoldSeconds = 2f;
    [SerializeField] private float tipBookDuration = 5f;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    
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
        SetActive(tipBook, false);
        SetActive(tuto1, false);
        SetActive(tuto2, false);
        SetActive(missionCard, true);
        SetActive(retryButton?.gameObject, false);
        finished = false;
        running = false;
        remain = timeLimitSeconds;
        UpdateTimer(remain);
    }
    
    IEnumerator RunFlow()
    {
        // 미션카드 페이드 인
        yield return StartCoroutine(FadeIn(missionCard, fadeDuration));
        yield return new WaitForSeconds(missionCardDuration);
        
        // 미션카드 페이드 아웃 → 튜토리얼 페이드 인
        yield return StartCoroutine(FadeOut(missionCard, fadeDuration));
        SetActive(missionCard, false);
        
        SetActive(tuto1, true);
        SetActive(tuto2, true);
        StartCoroutine(FadeIn(tuto1, fadeDuration));
        yield return StartCoroutine(FadeIn(tuto2, fadeDuration));
        
        yield return new WaitForSeconds(tuto1Duration);
        
        // tuto1만 페이드 아웃
        yield return StartCoroutine(FadeOut(tuto1, fadeDuration));
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
        StartCoroutine(SuccessFlow());
    }
    
    IEnumerator SuccessFlow()
    {
        // tuto2 페이드 아웃
        yield return StartCoroutine(FadeOut(tuto2, fadeDuration));
        SetActive(tuto2, false);
        
        // Clear 사운드 재생
        if (clearSound != null) clearSound.Play();
        
        // Success UI 페이드 인
        SetActive(successUI, true);
        yield return StartCoroutine(FadeIn(successUI, fadeDuration));
        yield return new WaitForSeconds(successHoldSeconds);
        
        // Success UI 페이드 아웃
        yield return StartCoroutine(FadeOut(successUI, fadeDuration));
        SetActive(successUI, false);
        
        // TipBook 사운드 재생
        if (glowSound != null) glowSound.Play();
        
        // TipBook 페이드 인
        SetActive(tipBook, true);
        yield return StartCoroutine(FadeIn(tipBook, fadeDuration));
        
        yield return new WaitForSeconds(tipBookDuration);
        
        // TipBook 페이드 아웃
        yield return StartCoroutine(FadeOut(tipBook, fadeDuration));
        SetActive(tipBook, false);
    }
    
    public void Fail()
    {
        if (finished) return;
        finished = true;
        running = false;
        
        // Fail 사운드 재생
        if (failSound != null) failSound.Play();
        
        StartCoroutine(FailFlow());
    }
    
    IEnumerator FailFlow()
    {
        SetActive(failUI, true);
        yield return StartCoroutine(FadeIn(failUI, fadeDuration));
        
        SetActive(retryButton?.gameObject, true);
        if (retryButton != null)
        {
            yield return StartCoroutine(FadeIn(retryButton.gameObject, fadeDuration));
        }
    }
    
    public void Restart()
    {
        if (flowCo != null) StopCoroutine(flowCo);
        SetupInitialState();
        flowCo = StartCoroutine(RunFlow());
    }
    
    // ==================== 페이드 효과 ====================
    
    IEnumerator FadeIn(GameObject obj, float duration)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(obj);
        cg.alpha = 0f;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }
    
    IEnumerator FadeOut(GameObject obj, float duration)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(obj);
        cg.alpha = 1f;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 0f;
    }
    
    CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        if (obj == null) return null;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }
    
    static void SetActive(GameObject go, bool v)
    {
        if (go) go.SetActive(v);
    }
}