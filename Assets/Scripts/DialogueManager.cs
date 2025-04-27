using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField nicknameInput;
    public GameObject nicknamePanel;
    public Camera_Moving cameraController;
    public Animator characterAnimator;

    public Transform playerTransform;

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
        "����, �������� �� ���� US-Reborn�� �����̾�!",
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

        cameraController.canMove = true;
        cameraController.canWalk = false;

        // Entry �� Talking �ڵ� ����Ǿ� �ִٰ� ����
    }

    public void OnDialogueClick()
    {
        currentIndex++;

        if (!isSecondPhase)
        {
            if (currentIndex < dialogues.Length)
            {
                dialogueText.text = dialogues[currentIndex];
            }
            else
            {
                dialoguePanel.SetActive(false);
                nicknamePanel.SetActive(true);

                cameraController.canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            if (currentIndex < secondDialogues.Length)
            {
                dialogueText.text = secondDialogues[currentIndex];
            }
            else
            {
                dialoguePanel.SetActive(false);
                Debug.Log("�� ��° ������ ��� �Ϸ�!");

                cameraController.canMove = true;
                cameraController.canWalk = true;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //  ��� �������ϱ� �ٷ� �ȱ� ����
                characterAnimator.SetTrigger("Walk"); // Walk Ʈ���� �ߵ�
                StartCoroutine(WalkForward()); // ������ �̵�
            }
        }
    }

    public void OnNicknameSubmit()
    {
        string nickname = nicknameInput.text;

        if (string.IsNullOrWhiteSpace(nickname))
        {
            Debug.LogWarning("�г����� �Է����ּ���.");
            return;
        }

        PlayerPrefs.SetString("PlayerNickname", nickname);
        Debug.Log("�г��� �����: " + nickname);

        nicknamePanel.SetActive(false);
        dialoguePanel.SetActive(true);

        currentIndex = 0;
        isSecondPhase = true;
        dialogueText.text = secondDialogues[currentIndex];

        cameraController.canMove = true;
        cameraController.canWalk = false;
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
