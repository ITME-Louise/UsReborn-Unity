using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGame_UITrigger : MonoBehaviour
{
    public GameObject dialogueCanvas;
    public TMP_Text dialogueText;
    public Button nextButton;

    private OVRGrabbable grabbable;
    private bool isDialogueActive = false;
    private int dialogueIndex = 0;

    private string[] dialogueLines = new string[]
    {
        "어! 날 구하러 와줬네 고마워",
        "" // 두 번째 대사는 나중에 채울 예정
    };

    void Start()
    {
        //  런타임에 자동 연결
        if (dialogueCanvas == null)
            dialogueCanvas = GameObject.Find("Canvas");

        if (dialogueText == null)
            dialogueText = GameObject.Find("Text (TMP)").GetComponent<TMP_Text>();

        if (nextButton == null)
            nextButton = GameObject.Find("Button").GetComponent<Button>();

        grabbable = GetComponent<OVRGrabbable>();
        dialogueCanvas?.SetActive(false);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextDialogue);
        }
        else
        {
            Debug.LogWarning("Next Button이 연결되지 않았습니다!");
        }
    }

    void Update()
    {
        if (grabbable == null) return;
        if (!isDialogueActive && grabbable.isGrabbed)
        {
            ShowDialogue();
        }
        else if (isDialogueActive && !grabbable.isGrabbed)
        {
            HideDialogue();
        }
    }

    void ShowDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        dialogueCanvas.SetActive(true);
        dialogueText.text = dialogueLines[dialogueIndex];
    }

    void ShowNextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueLines.Length && !string.IsNullOrEmpty(dialogueLines[dialogueIndex]))
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
        dialogueCanvas.SetActive(false);
        isDialogueActive = false;
    }
}
