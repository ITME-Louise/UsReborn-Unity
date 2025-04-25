using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    // Start is called before the first frame update
    void Start()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogues[currentIndex];
    }

    private string[] dialogues = new string[5]
    {
        "��, ���� �Ͼ��? ù ��ٿ� ���� �߱� �� �����߾�.",
        "�� ���� �븮! ������ �� ���䰡 �� �������.",
        "���� ���� ȯ�� ������ US-Reborn.",
        "���ֿ� ���� ������ ��ȭ�ϰ� ���°踦 �����ϴ� ���̾�.",
        "����, ������� ����� ����?",
    };

    private int currentIndex = 0;

    public void OnDialogueClick()
    {
        currentIndex++;
        if (currentIndex < dialogues.Length)
        {
            dialogueText.text = dialogues[currentIndex];
        }
        else
        {
            dialoguePanel.SetActive(false);
        }
    }
}
