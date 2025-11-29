using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class QuizUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Image trashImage;

    [Header("퀴즈 UI용 쓰레기 이미지")]
    [SerializeField] private Sprite paperSprite;
    [SerializeField] private Sprite packSprite;
    [SerializeField] private Sprite canSprite;
    [SerializeField] private Sprite glassSprite;
    [SerializeField] private Sprite petSprite;
    [SerializeField] private Sprite plasticSprite;
    [SerializeField] private Sprite vinylSprite;

    private QuizManager quizManager;
    private string correctAnswer;
    private string currentClassName;
    private Vector3 spawnPosition;
    private bool isQuizActive = false;

    private Dictionary<string, Sprite> classImageSprites;

    public void Initialize(QuizManager manager)
    {
        quizManager = manager;

        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            Debug.LogError("QuizUIController: Panel이 할당되지 않았습니다");
        }

        // Dictionary 초기화
        classImageSprites = new Dictionary<string, Sprite>
        {
            { "paper", paperSprite },
            { "pack", packSprite },
            { "can", canSprite },
            { "glass", glassSprite },
            { "pet", petSprite },
            { "plastic", plasticSprite },
            { "vinyl", vinylSprite }
        };
    }

    void Update()
    {
        // 퀴즈가 활성화되어 있지 않으면 입력 무시
        if (!isQuizActive) return;

        // 오른쪽 컨트롤러만 사용함
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) // B버튼 -> O
        {
            OnOButtonPressed();
        }    
        else if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) // A버튼 -> X
        {
            OnXButtonPressed();
        }
    }

    public void ShowQuiz(string className, string question, string answer, Vector3 worldPos)
    {
        currentClassName = className;
        correctAnswer = answer;
        spawnPosition = worldPos;

        // 질문 텍스트 설정
        if (questionText != null)
        {
            questionText.text = question;
        }

        // 쓰레기 이미지 설정
        if (trashImage != null)
        {
            if (classImageSprites.ContainsKey(className) && classImageSprites[className] != null)
            {
                trashImage.sprite = classImageSprites[className];
                trashImage.gameObject.SetActive(true);
            }
            else
            {
                trashImage.gameObject.SetActive(false);
                Debug.LogWarning($"QuizUIController: '{className}'에 대한 이미지가 없습니다.");
            }
        }

        // 패널 활성화
        if (panel != null)
        {
            panel.SetActive(true);
            isQuizActive = true;
        }
    }

    public void HideQuiz()
    {
        isQuizActive = false;

        if (panel != null)
        {
            panel.SetActive(false); // 실제 퀴즈 패널을 꺼야 함
        }
    }

    public void OnOButtonPressed() { Submit("O"); }
    public void OnXButtonPressed() { Submit("X"); }

    private void Submit(string userAnswer)
    {
        isQuizActive = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        quizManager?.OnAnswerSubmitted(userAnswer, correctAnswer, currentClassName, spawnPosition);
    }
}
