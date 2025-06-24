using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamQuiizUIController : MonoBehaviour
{
    [SerializeField] private GameObject quizPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void HideQuiz()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false); // 실제 퀴즈 패널을 꺼야 함
        }
    }
}
