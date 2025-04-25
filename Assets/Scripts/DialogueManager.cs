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
        "오, 드디어 일어났네? 첫 출근에 늦잠 잘까 봐 걱정했어.",
        "난 레온 대리! 앞으로 네 멘토가 될 사람이지.",
        "여긴 우주 환경 복구국 US-Reborn.",
        "우주에 퍼진 오염을 정화하고 생태계를 복구하는 곳이야.",
        "먼저, 사원증을 만들어 볼까?",
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
