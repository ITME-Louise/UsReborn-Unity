using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject nicknamePanel;
    public Animator characterAnimator;

    public GameObject usRebornPanel;
    public GameObject topNoticePanel;
    public GameObject topNicknameNoticePanel;

    private string nickname = "";

    private string[] dialogues = new string[5]
    {
        "오, 드디어 일어났네? 첫 출근에 늦잠 잘까 봐 걱정했어.",
        "난 레온 대리! 앞으로 네 멘토가 될 사람이지.",
        "여긴 우주 환경 복구국 US-Reborn.",
        "우주에 퍼진 오염을 정화하고 생태계를 복구하는 곳이야.",
        "먼저, 사원증을 만들어 볼까?",
    };

    private string[] secondDialogues = new string[5]
    {
        "좋아, 이제부터 넌 정식 US-Reborn의 인턴이야, {0}!",
        "하지만 아직 배울 게 많지.",
        "여긴 네 개인 우주선 방이야.",
        "앞으로 임무를 선택하고 출발하는 거점이 될 거야.",
        "준비되면, 가고 싶은 임무를 직접 골라봐."
    };

    private int currentIndex = 0;
    private bool isSecondPhase = false;
    private bool isDialogueActive = true;

    void Start()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogues[currentIndex];
        nicknamePanel.SetActive(false);

        usRebornPanel.SetActive(false);
        topNoticePanel.SetActive(true);
        topNicknameNoticePanel.SetActive(false);

        StartCoroutine(AutoAdvanceDialogue());
    }

    IEnumerator AutoAdvanceDialogue()
    {
        while (isDialogueActive)
        {
            yield return new WaitForSeconds(3f);
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        currentIndex++;

        if (!isSecondPhase)
        {
            if (currentIndex < dialogues.Length)
            {
                dialogueText.text = dialogues[currentIndex];

                usRebornPanel.SetActive(currentIndex == 2 || currentIndex == 3);
                if (currentIndex == 1)
                    topNoticePanel.SetActive(false);
            }
            else
            {
                isDialogueActive = false;
                dialoguePanel.SetActive(false);
                nicknamePanel.SetActive(true);
                topNicknameNoticePanel.SetActive(true);

                StartCoroutine(DelayNicknameComplete());
            }
        }
        else
        {
            if (currentIndex < secondDialogues.Length)
            {
                dialogueText.text = string.Format(secondDialogues[currentIndex], nickname);
            }
            else
            {
                isDialogueActive = false;
                dialoguePanel.SetActive(false);
                Debug.Log("두 번째 대사까지 모두 완료!");
                characterAnimator.SetTrigger("Walk");

                StartCoroutine(LoadHubSceneAfterDelay());
            }
        }
    }

    IEnumerator DelayNicknameComplete()
    {
        yield return new WaitForSeconds(3f);

        nickname = "인턴";
        PlayerPrefs.SetString("PlayerNickname", nickname);

        nicknamePanel.SetActive(false);
        dialoguePanel.SetActive(true);

        currentIndex = 0;
        isSecondPhase = true;
        dialogueText.text = string.Format(secondDialogues[currentIndex], nickname);

        topNicknameNoticePanel.SetActive(false);

        isDialogueActive = true;
        StartCoroutine(AutoAdvanceDialogue());
    }

    IEnumerator LoadHubSceneAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadHubSceneFromMain();
        }
        else
        {
            Debug.LogError("GameSceneManager를 찾을 수 없습니다.");
        }
    }
}
