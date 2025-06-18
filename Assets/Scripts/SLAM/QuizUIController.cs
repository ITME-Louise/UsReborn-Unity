using UnityEngine;
using TMPro;

public class QuizUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text questionText;
    private QuizManager quizManager;

    private string correctAnswer;
    private string currentClassName;
    private Vector3 spawnPosition;

    public void Initialize(QuizManager manager)
    {
        quizManager = manager;
    }

    public void ShowQuiz(string className, string question, string answer, Vector3 worldPos)
    {
        currentClassName = className;
        correctAnswer = answer;
        spawnPosition = worldPos;

        questionText.text = question;
        panel.SetActive(true);
    }

    public void OnOButtonPressed()
    {
        Submit("O");
    }

    public void OnXButtonPressed()
    {
        Submit("X");
    }

    private void Submit(string userAnswer)
    {
        panel.SetActive(false);
        quizManager.OnAnswerSubmitted(userAnswer, correctAnswer, currentClassName, spawnPosition);
    }
}
