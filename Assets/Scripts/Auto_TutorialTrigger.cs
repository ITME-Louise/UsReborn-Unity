using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auto_TutorialTrigger : MonoBehaviour
{

    public DialogueManager dialogueManager;
    [TextArea(3, 10)]
    // TextArea Attribute을 사용하면 에디터에서 다중 줄 문자열 입력이 가능
    // (최소 줄 수: 3줄, 최대 줄 수: 10줄)
    public string dialogueLines;
    public float delayBetweenLines = 2.5f; // 각 대사 사이 간격

    // Start is called before the first frame update
    void Start()
    {
        //dialogueManager.ShowDialogue(dialogueText);

        string prevScene = PlayerPrefs.GetString("LastScene", "None");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
