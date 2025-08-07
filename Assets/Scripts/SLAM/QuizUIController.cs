using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuizUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text classNameText;

    private QuizManager quizManager;
    private string correctAnswer;
    private string currentClassName;
    private Vector3 spawnPosition;

    private Dictionary<string, string> classNameGameText = new Dictionary<string, string>()
    {
        { "paper", "종이 아이템 발견!" },
        { "pack", "팩 아이템 발견!" },
        { "can", "캔 아이템 발견!" },
        { "glass", "유리 아이템 발견!" },
        { "pet", "PET류 아이템 발견!" },
        { "plastic", "플라스틱 아이템 발견!" },
        { "vinyl", "비닐 아이템 발견!" }
    };

    public void Initialize(QuizManager manager)
    {
        quizManager = manager;
        panel.SetActive(false);
    }

    public void ShowQuiz(string className, string question, string answer, Vector3 worldPos)
    {
        currentClassName = className;
        correctAnswer = answer;
        spawnPosition = worldPos;

        questionText.text = question;
        classNameText.text = classNameGameText.ContainsKey(className) ? classNameGameText[className] : $"🔍 {className} 아이템 발견!";

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
