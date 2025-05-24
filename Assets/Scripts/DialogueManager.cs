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
        "��, ���� �Ͼ��? ù ��ٿ� ���� �߱� �� �����߾�.",
        "�� ���� �븮! ������ �� ���䰡 �� �������.",
        "���� ���� ȯ�� ������ US-Reborn.",
        "���ֿ� ���� ������ ��ȭ�ϰ� ���°踦 �����ϴ� ���̾�.",
        "����, ������� ����� ����?",
    };

    private string[] secondDialogues = new string[5]
    {
        "����, �������� �� ���� US-Reborn�� �����̾�, {0}!",
        "������ ���� �־���.",
        "�ϴ� ���ʺ��� ����ڰ�!",
        "���� �� ���� ���ּ� ���̾�. ���� ������� �����ϸ� ��.",
        "�켱, â ������ ���̴� EARTH�� �������� �ұ�?"
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
                Debug.Log("�� ��° ������ ��� �Ϸ�!");


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
            Debug.LogWarning("�г����� �Է����ּ���.");
            return;
        }

        nickname = input;
        PlayerPrefs.SetString("PlayerNickname", nickname);
        Debug.Log("�г��� �����: " + nickname);

       
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
