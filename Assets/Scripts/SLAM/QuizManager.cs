using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private QuizUIController quizUI;
    [SerializeField] private PotSpawner potSpawner;
    [SerializeField] private DetectionVisualizer detectionVisualizer;

    [SerializeField] private string sceneToLoadAfterTimeout;
    [SerializeField] private float timeLimitInSeconds;

    [SerializeField] private Text timerText;

    private float timer = 0f;
    private bool isTimerRunning = false;

    private Dictionary<string, List<(string, string)>> quizData = new Dictionary<string, List<(string, string)>>()
    {
        { "paper", new List<(string, string)>{
            ("종이는 젖지 않게 배출해야 한다.", "O"),
            ("종이는 반듯하게 펴서 끈으로 묶어 배출한다.", "O"),
            ("스프링이나 테이프 붙은 종이는 그대로 배출해도 된다.", "X"),
            ("영수증(감열지)도 종이류로 배출할 수 있다.", "X"),
            ("금박지, 은박지, 부직포는 종이류로 배출하면 안 된다.", "O"),
            ("플라스틱 합성지는 종이류로 배출해도 무방하다.", "X"),
            ("다른 재질이 섞인 벽지는 종이류로 배출하면 안 된다.", "O"),
            ("종이는 쓰레기봉투에 넣어 배출해야 한다.", "O")
        }},
        { "pack", new List<(string, string)>{
            ("팩은 내용물을 깨끗이 비우고 버려야 한다.", "O"),
            ("팩은 일반 종이와 같은 방법으로 분리 배출한다.", "X"),
            ("종이팩에 붙은 빨대나 비닐은 제거하지 않아도 된다.", "X"),
            ("종이팩은 일반 종이류와 섞어 배출해도 무방하다.", "X"),
            ("종이팩 전용 수거함이 없으면 그냥 일반 종이류 수거함에 버려도 된다.", "X"),
            ("팩은 반드시 납작하게 눌러서 버려야 한다.", "O"),
            ("종이팩은 종이팩 전용 수거함에 배출하는 것이 원칙이다.", "O")
        }},
        { "can", new List<(string, string)>{
            ("금속캔은 내용물을 비우고 물로 헹군 후 배출해야 한다.", "O"),
            ("금속캔에 담배꽁초나 다른 이물질을 넣어도 상관없다.", "X"),
            ("금속캔에 플라스틱 뚜껑이 붙어 있어도 그대로 배출해도 된다.", "O"),
            ("금속캔은 이물질이 섞이지 않도록 깨끗이 분리해서 배출해야 한다.", "X"),
            ("부탄가스 용기는 내용물을 완전히 제거하지 않아도 배출할 수 있다.", "X"),
            ("가스용기는 통풍이 잘 되는 곳에서 노즐을 눌러 내용물을 완전히 제거한 후 배출해야 한다.", "O")
        }},
        { "glass", new List<(string, string)>{
            ("유리병은 내용물을 비우고 물로 헹군 후 배출해야 한다.", "O"),
            ("유리병에 담배꽁초 등 이물질을 넣어도 된다.", "X"),
            ("유리병은 깨지지 않도록 주의해서 배출해야 한다.", "O"),
            ("유리병은 내용물을 비우지 않고 그대로 배출해도 된다.", "X"),
            ("거울, 전구, 깨진 유리, 도자기류도 유리병과 같이 배출한다.", "O"),
            ("유리병은 색상별로 분류하여 배출하는 것이 권장된다.", "O")
        }},
        { "pet", new List<(string, string)>{
            ("페트병에 붙은 라벨과 뚜껑은 분리해서 버린다.", "O"),
            ("페트병은 재활용이 불가능한 플라스틱이다.", "X"),
            ("페트병은 내용물을 비우고 물로 헹군 후 배출해야 한다.", "O"),
            ("페트병은 부착된 상표와 부속품도 함께 배출한다.", "X"),
            ("페트병은 본체와 다른 재질을 제거한 후 배출해야 한다.", "O")
        }},
        { "plastic", new List<(string, string)>{
            ("플라스틱 용기류는 내용물을 비우고 깨끗이 헹군 후 배출해야 한다.", "O"),
            ("플라스틱 용기류는 부착상표나 다른 재질을 제거하지 않아도 된다.", "X"),
            ("플라스틱 용기류 중 물로 헹굴 수 없는 것도 내용물을 비운 후 배출해야 한다.", "O"),
            ("장난감, 옷걸이, 칫솔 등 플라스틱은 재활용품으로 배출한다.", "X")
        }},
        { "vinyl", new List<(string, string)>{
            ("비닐류는 내용물을 제거하고 물로 헹군 후 배출해야 한다.", "O"),
            ("비닐류는 흩날리지 않도록 봉투에 담아 배출한다.", "O"),
            ("오염이 심한 비닐, 식탁보, 고무장갑 등은 비닐류로 배출한다.", "X")
        }}
    };

    private void Start()
    {
        quizUI.Initialize(this);
        StartTimer();
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

    private void StartTimer()
    {
        timer = 0f;
        isTimerRunning = true;
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        timer += Time.deltaTime;

        float timeRemaining = Mathf.Max(0f, timeLimitInSeconds - timer);
        UpdateTimerUI(timeRemaining);

        if (timer >= timeLimitInSeconds)
        {
            isTimerRunning = false;
            Debug.Log("시간 초과");

            // 3초 후 씬 전환
            Invoke(nameof(GoToTimeoutScene), 180f);
        }
    }
    private void GoToTimeoutScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoadAfterTimeout))
        {
            SceneManager.LoadScene(sceneToLoadAfterTimeout);
        }
        else
        {
            Debug.LogWarning("sceneToLoadAfterTimeout 값이 비어 있습니다. 인스펙터에서 설정하세요.");
        }
    }
    private void UpdateTimerUI(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

}