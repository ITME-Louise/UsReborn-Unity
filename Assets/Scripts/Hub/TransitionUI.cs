using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionUI : MonoBehaviour
{
    public static TransitionUI Instance { get; private set; }

    public Canvas transitionCanvas;
    public Image fadeImage;
    public TextMeshProUGUI transitionText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator ShowTransition(string message)
    {
        gameObject.SetActive(true);

        // 페이드 인
        fadeImage.color = new Color(0, 0, 0, 0);
        transitionText.alpha = 0;
        transitionText.text = message;

        float timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0, 0.9f, timer / 1f));
            yield return null;
        }

        // 텍스트 페이드 인
        timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            transitionText.alpha = Mathf.Lerp(0, 1, timer / 1f);
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        // 페이드 아웃
        timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / 1f);
            fadeImage.color = new Color(0, 0, 0, alpha * 0.9f);
            transitionText.alpha = alpha;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}