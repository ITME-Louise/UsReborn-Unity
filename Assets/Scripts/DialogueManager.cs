using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField nicknameInput;
    public GameObject nicknamePanel;
    public Animator characterAnimator;

    public Transform playerTransform;

    public GameObject usRebornPanel;
    public GameObject topNoticePanel;
    public GameObject topNicknameNoticePanel;

    public TextMeshProUGUI nicknameDisplayText; 
    public GameObject okButton; 

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
        "하지만 아직 멀었어.",
        "일단 기초부터 배우자고!",
        "여긴 네 개인 우주선 방이야. 개인 기지라고 생각하면 돼.",
        "우선, 창 밖으로 보이는 EARTH에 가보도록 할까?"
    };

    private int currentIndex = 0;
    private bool isSecondPhase = false;

    void Start()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogues[currentIndex];
        nicknamePanel.SetActive(false);

        usRebornPanel.SetActive(false);
        topNoticePanel.SetActive(true);
        topNicknameNoticePanel.SetActive(false);

        nicknameDisplayText.gameObject.SetActive(false); 
    }

    public void OnDialogueClick()
    {
        currentIndex++;

        if (!isSecondPhase)
        {
            if (currentIndex < dialogues.Length)
            {
                dialogueText.text = dialogues[currentIndex];

                if (currentIndex == 2 || currentIndex == 3)
                {
                    usRebornPanel.SetActive(true);
                }
                else
                {
                    usRebornPanel.SetActive(false);
                }

                if (currentIndex == 1)
                {
                    topNoticePanel.SetActive(false);
                }
            }
            else
            {
                dialoguePanel.SetActive(false);
                nicknamePanel.SetActive(true);

                topNicknameNoticePanel.SetActive(true);

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
                dialoguePanel.SetActive(false);
                Debug.Log("두 번째 대사까지 모두 완료!");


                characterAnimator.SetTrigger("Walk");
                StartCoroutine(WalkForward());
            }
        }
    }

    public void OnNicknameSubmit()
    {
        string input = nicknameInput.text;

        if (string.IsNullOrWhiteSpace(input))
        {
            Debug.LogWarning("닉네임을 입력해주세요.");
            return;
        }

        nickname = input;
        PlayerPrefs.SetString("PlayerNickname", nickname);
        Debug.Log("닉네임 저장됨: " + nickname);

       
        nicknameInput.gameObject.SetActive(false);
        okButton.SetActive(false);

       
        nicknameDisplayText.text = nickname;
        nicknameDisplayText.gameObject.SetActive(true);

       
        StartCoroutine(DelayNicknameComplete());
    }

    IEnumerator DelayNicknameComplete()
    {
        yield return new WaitForSeconds(2f);

        nicknamePanel.SetActive(false);
        dialoguePanel.SetActive(true);

        currentIndex = 0;
        isSecondPhase = true;
        dialogueText.text = string.Format(secondDialogues[currentIndex], nickname);

        topNicknameNoticePanel.SetActive(false);
    }

    IEnumerator WalkForward()
    {
        float walkTime = 0f;

        while (walkTime < 3f)
        {
            playerTransform.position += playerTransform.forward * 2f * Time.deltaTime;
            walkTime += Time.deltaTime;
            yield return null;
        }
    }
}
