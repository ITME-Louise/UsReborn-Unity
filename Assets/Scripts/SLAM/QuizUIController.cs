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

    private Dictionary<string, Sprite> classImageSprites = new Dictionary<string, Sprite>();

    public void Initialize(QuizManager manager)
    {
        quizManager = manager;
        panel.SetActive(false);

        classImageSprites["paper"] = paperSprite;
        classImageSprites["pack"] = packSprite;
        classImageSprites["can"] = canSprite;
        classImageSprites["glass"] = glassSprite;
        classImageSprites["pet"] = petSprite;
        classImageSprites["plastic"] = plasticSprite;
        classImageSprites["vinyl"] = vinylSprite;
    }

    public void ShowQuiz(string className, string question, string answer, Vector3 worldPos)
    {
        currentClassName = className;
        correctAnswer = answer;
        spawnPosition = worldPos;

        questionText.text = question;
        
        if (trashImage != null && classImageSprites.ContainsKey(className))
        {
            trashImage.sprite = classImageSprites[className];
            trashImage.gameObject.SetActive(true);
        }
        else if (trashImage != null)
        {
            trashImage.gameObject.SetActive(false);
        }

        panel.SetActive(true);
    }

    public void OnOButtonPressed() { Submit("O"); }
    public void OnXButtonPressed() { Submit("X"); }

    private void Submit(string userAnswer)
    {
        panel.SetActive(false);
        quizManager?.OnAnswerSubmitted(userAnswer, correctAnswer, currentClassName, spawnPosition);
    }
}
