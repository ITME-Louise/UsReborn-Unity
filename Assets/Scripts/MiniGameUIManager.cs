using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameUIManager : MonoBehaviour
{
    public TMP_Text dialogueText;
    public Button nextButton;

    private OVRGrabbable grabbable;
    private bool isDialogueActive = false;
    private int dialogueIndex = 0;

    private string[] dialogueLines = new string[]
    {
        "어! 날 구하러 와줬네 고마워",
        ""
    };

    void Start()
    {
        grabbable = GetComponent<OVRGrabbable>();

        if (dialogueText == null)
            dialogueText = GameObject.Find("Text (TMP)").GetComponent<TMP_Text>();

        if (nextButton == null)
            nextButton = GameObject.Find("Button").GetComponent<Button>();

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextDialogue);

        // 테스트용: 시작하자마자 UI 띄우기
        ShowDialogue();

        // 원래는 시작 시 UI를 숨김
        // dialogueText.text = "";
        // nextButton.gameObject.SetActive(false);
    }

    void Update()
    {
        // 원래는 잡았을 때 UI를 띄우고, 놓으면 끔
        /*
        if (grabbable == null) return;

        if (!isDialogueActive && grabbable.isGrabbed)
        {
            ShowDialogue();
        }
        else if (isDialogueActive && !grabbable.isGrabbed)
        {
            HideDialogue();
        }
        */
    }

    void ShowDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        dialogueText.text = dialogueLines[dialogueIndex];
        nextButton.gameObject.SetActive(true);
    }

    void ShowNextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[dialogueIndex];
        }
        else
        {
            HideDialogue();
        }
    }

    void HideDialogue()
    {
        isDialogueActive = false;
        dialogueText.text = "";
        nextButton.gameObject.SetActive(false);
    }
}
