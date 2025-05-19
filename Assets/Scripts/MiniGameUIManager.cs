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
        "��! �� ���Ϸ� ����� ����",
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

        // �׽�Ʈ��: �������ڸ��� UI ����
        ShowDialogue();

        // ������ ���� �� UI�� ����
        // dialogueText.text = "";
        // nextButton.gameObject.SetActive(false);
    }

    void Update()
    {
        // ������ ����� �� UI�� ����, ������ ��
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
