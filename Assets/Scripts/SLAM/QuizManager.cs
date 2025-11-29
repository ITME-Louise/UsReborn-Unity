using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [SerializeField] private QuizUIController quizUI;
    [SerializeField] private PotSpawner potSpawner;
    [SerializeField] private DetectionVisualizer detectionVisualizer;

    [Header("타이머 설정")]
    [SerializeField] private string sceneToLoadAfterTimeout;
    [SerializeField] private float timeLimitInSeconds = 300f;
    [SerializeField] private Text timerText;

    [SerializeField] private DialogueManager_Slam dialogueManager;

    private float timer = 0f;
    private bool isTimerRunning = false;
    private bool isQuizEnded = false;

    private int lastDisplayedSeconds = -1;

    private Dictionary<string, List<(string, string)>> quizData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeQuizData();
    }

    private void Start()
    {
        if (quizUI != null)
        {
            quizUI.Initialize(this);
        }
        else
        {
            Debug.LogError("QuizManager: QuizUIController가 할당되지 않았습니다");
        }

        StartTimer();
    }

    private void InitializeQuizData()
    {
        quizData = new Dictionary<string, List<(string, string)>>
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
    }

    // 손 충돌 시 ML 인식 트리거
    public void TriggerMLRecognition(GameObject trashObj)
    {
        if (trashObj == null) return;

        Debug.Log($"ML 인식 트리거: {trashObj.name}");

        // DetectionVisualizer에 대기 설정
        if (detectionVisualizer != null)
        {
            detectionVisualizer.SetWaitingForRecognition(trashObj);
        }

        // TrashDetectorController에 감지 명령
        TrashDetectorController trashDetector = FindObjectOfType<TrashDetectorController>();
        if (trashDetector != null)
        {
            trashDetector.TriggerDetection();
        }
        else
        {
            Debug.LogWarning("QuizManager: TrashDetectorController를 찾을 수 없습니다.");
        }
    }

    public void StartQuiz(string className, Vector3 worldPos)
    {
        if (isQuizEnded) return;

        if (!quizData.ContainsKey(className))
        {
            Debug.LogWarning($"QuizManager: '{className}'에 대한 퀴즈 데이터가 없습니다.");
            return;
        }

        List<(string, string)> quizList = quizData[className];
        if (quizList.Count == 0) return;

        (string question, string answer) selected = quizList[Random.Range(0, quizList.Count)];

        if (quizUI != null)
        {
            quizUI.ShowQuiz(className, selected.question, selected.answer, worldPos);
        }
    }

    public void OnAnswerSubmitted(string userAnswer, string correctAnswer, string className, Vector3 worldPos)
    {
        detectionVisualizer?.OnQuizCompleted();

        if (userAnswer == correctAnswer)
        {
            Debug.Log("정답입니다!");
            potSpawner?.SpawnPot(worldPos, className);
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
        lastDisplayedSeconds = -1;
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        timer += Time.deltaTime;
        float timeRemaining = Mathf.Max(0f, timeLimitInSeconds - timer);

        // 1초마다만 UI 업데이트
        int currentSeconds = Mathf.FloorToInt(timeRemaining);
        if (currentSeconds != lastDisplayedSeconds)
        {
            UpdateTimerUI(timeRemaining);
            lastDisplayedSeconds = currentSeconds;
        }

        // 시간 초과 체크
        if (timer >= timeLimitInSeconds)
        {
            OnTimeOut();
        }
    }

    private void OnTimeOut()
    {
        isTimerRunning = false;
        isQuizEnded = true;

        // 퀴즈 UI 숨기기
        if (quizUI != null)
        {
            quizUI.HideQuiz();
        }

        if (dialogueManager != null)
        {
            dialogueManager.ShowTimeoutNoticeAndChangeScene(
                "시간이 초과되었습니다.\n이제 다음 단계로 이동합니다.",
                5f,
                sceneToLoadAfterTimeout
            );
        }
        else
        {
            Debug.LogWarning("dialogueManager가 설정되지 않았습니다.");
        }
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}