using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auto_TutorialTrigger : MonoBehaviour
{

    public DialogueManager dialogueManager;
    [TextArea(3, 10)]
    // TextArea Attribute�� ����ϸ� �����Ϳ��� ���� �� ���ڿ� �Է��� ����
    // (�ּ� �� ��: 3��, �ִ� �� ��: 10��)
    public string dialogueLines;
    public float delayBetweenLines = 2.5f; // �� ��� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        //dialogueManager.ShowDialogue(dialogueText);

        string prevScene = PlayerPrefs.GetString("LastScene", "None");

        if (prevScene == "Start_Scene")
            dialogueManager.ShowDialogue("�հ��� �����ؿ� �Ĺ��. ���ú��� ����ȯ�� ��ȭ�����μ� ���ϰ� �ǰڳ׿�");
        else
            dialogueManager.ShowDialogue("ȯ���մϴ�!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
