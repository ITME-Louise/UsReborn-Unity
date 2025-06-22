using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager_Slam : MonoBehaviour
{
    [SerializeField] private GameObject topNoticePanel;
    [SerializeField] private Text noticeText;

    [Header("자동 대사용 문장")]
    [TextArea]
    [SerializeField]
    private string[] slamDialogues = new string[3]
    {
        "자, 우주 환경 미화원으로서의 첫 임무가 완료됐어!",
        "아주 잘 했는 걸? 수고 많았어.",
        "그럼 이제, Earth의 위험에 처한 동물들을 구하러 가볼까?"
    };

    private string sceneToLoad = "";
    [SerializeField] private float dialogueInterval = 3f;
    [SerializeField] private float lastDelayBeforeScene = 2f;

    private int dialogueIndex = 0;
    private System.Action onNoticeComplete;
    // Start is called before the first frame update
    private void Start()
    {
        if (topNoticePanel != null)
            topNoticePanel.SetActive(false);
    }

    public void ShowTimeoutNoticeAndChangeScene(string message, float duration, string sceneName)
    {
        sceneToLoad = sceneName;
        StartCoroutine(RunAutoDialogueSequence());
    }

    private void ProceedToScene()
    {
        topNoticePanel.SetActive(false);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("sceneToLoad 값이 비어 있습니다.");
        }
    }

    private IEnumerator RunAutoDialogueSequence()
    {
        topNoticePanel.SetActive(true);

        for (dialogueIndex = 0; dialogueIndex < slamDialogues.Length; dialogueIndex++)
        {
            noticeText.text = slamDialogues[dialogueIndex];
            yield return new WaitForSeconds(dialogueInterval);
        }

        yield return new WaitForSeconds(lastDelayBeforeScene);

        LoadNextScene();
    }
    private void LoadNextScene()
    {
        topNoticePanel.SetActive(false);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("전환할 씬 이름이 설정되지 않았습니다.");
        }
    }

}
