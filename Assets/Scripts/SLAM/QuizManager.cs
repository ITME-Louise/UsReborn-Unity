using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private QuizUIController quizUI;
    [SerializeField] private PotSpawner potSpawner;
    [SerializeField] private DetectionVisualizer detectionVisualizer;

    private Dictionary<string, List<(string, string)>> quizData = new Dictionary<string, List<(string, string)>>()
    {
        { "paper", new List<(string, string)>{
            ("젖은 종이는 종이류로 버려도 된다.", "X"),
            ("스프링 노트는 철심을 제거하고 버려야 한다.", "O"),
            ("감열지 영수증은 종이류로 분리 배출해야 한다.", "X")
        }},
        { "pack", new List<(string, string)>{
            ("종이팩과 종이류는 같은 재활용 공정을 거친다.", "X"),
            ("종이팩은 헹군 후 전용 수거함에 배출한다.", "O")
        }},
        { "can", new List<(string, string)>{
            ("금속캔을 버릴 때는 내용물을 비우고 헹군다.", "O"),
            ("부탄가스는 그냥 캔류로 배출해도 된다.", "X")
        }},
        { "glass", new List<(string, string)>{
            ("소주병은 보증금을 돌려받을 수 있다.", "O"),
            ("깨진 유리는 유리병으로 배출한다.", "X")
        }},
        { "pet", new List<(string, string)>{
            ("페트병은 라벨을 제거하고 배출해야 한다.", "O")
        }},
        { "plastic", new List<(string, string)>{
            ("플라스틱 용기는 내용물을 비우고 헹군 후 배출한다.", "O")
        }},
        { "vinyl", new List<(string, string)>{
            ("이물질이 묻은 비닐도 배출해도 된다.", "X")
        }}
    };

    private void Start()
    {
        quizUI.Initialize(this);
    }

    public void StartQuiz(string className, Vector3 worldPos)
    {
        if (!quizData.ContainsKey(className)) return;

        var quizList = quizData[className];
        var selected = quizList[Random.Range(0, quizList.Count)];
        string question = selected.Item1;
        string answer = selected.Item2;

        quizUI.ShowQuiz(className, question, answer, worldPos);
    }

    public void OnAnswerSubmitted(string userAnswer, string correctAnswer, string className, Vector3 worldPos)
    {
        if (detectionVisualizer != null)
        {
            detectionVisualizer.OnQuizCompleted();
        }
        
        if (userAnswer == correctAnswer)
        {
            Debug.Log("정답입니다!");
            potSpawner.SpawnPot(worldPos, className);
        }
        else
        {
            Debug.Log("오답입니다!");
        }
    }
}