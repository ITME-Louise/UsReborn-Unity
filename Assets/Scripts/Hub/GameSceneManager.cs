using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private string currentLoadedScene = "";

    public const string MAIN_SCENE = "Main_Scene";
    public const string HUB_SCENE = "Hub_Scene";
    public const string FOX_GAME = "MiniGame_Fox_Scene1 1";
    public const string RABBIT_GAME = "MiniGame_Bear_Scene 1";
    public const string SLAM_SCENE = "SLAM_Scene";
    public const string MULTI_SCENE = "Multiplay_Scene";

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameSceneManager] 초기화 완료");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(InitialLoadHub());
    }

    private IEnumerator InitialLoadHub()
    {
        yield return new WaitForSeconds(0.5f);
        //LoadSceneAdditive(HUB_SCENE);
    }

    public void LoadHubSceneFromMain()
    {
        LoadSceneAdditive(HUB_SCENE);
    }

    public void LoadSceneAdditive(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.Log("[GameSceneManager] 이미 씬 전환 중");
            return;
        }

        if (sceneName == currentLoadedScene)
        {
            Debug.Log("[GameSceneManager] 이미 해당 씬이 로드됨: " + sceneName);
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        isTransitioning = true;
        Debug.Log("[GameSceneManager] 씬 로드 시작: " + sceneName);

        // 전환 메시지 표시
        string message = GetTransitionMessage(sceneName);
        if (!string.IsNullOrEmpty(message) && TransitionUI.Instance != null)
        {
            StartCoroutine(TransitionUI.Instance.ShowTransition(message));
            yield return new WaitForSeconds(2f);  // 1.5f → 2f
        }

        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(currentLoadedScene))
        {
            Debug.Log("[GameSceneManager] 이전 씬 언로드: " + currentLoadedScene);
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentLoadedScene);
            while (unloadOp != null && !unloadOp.isDone)
            {
                yield return null;
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        currentLoadedScene = sceneName;
        Debug.Log("[GameSceneManager] 씬 로드 완료: " + sceneName);
        isTransitioning = false;
    }

    private string GetTransitionMessage(string sceneName)
    {
        switch (sceneName)
        {
            case SLAM_SCENE:
                return "주변 환경이 쓰레기로 오염되었습니다.\n쓰레기를 주운 뒤 분리수거 퀴즈에 도전하세요.\n정답을 맞히면 화분이 생성되고 환경이 복구됩니다.";
            case FOX_GAME:
                return "사막의 강풍에 여우의 굴을 지키던 돌이 무너졌습니다.\n주변에 떨어진 돌울 왼쪽 손으로 던져 굴 복원을 도와주세요.\n정해진 위치 세 곳에 정확하게 던지면 임무 성공입니다.";
            case RABBIT_GAME:
                return "사막에서 토끼가 탈진했습니다.\n양동이를 왼쪽 손으로 잡은 후 물을 담아 토끼에게 건네주세요.\n토끼에게 물을 주면 탈진에서 벗어나고 구조가 완료됩니다.";
            case MULTI_SCENE:
                return "오염된 지역에 출현한 '쓰레기 괴물'을 팀원들과 협력해 퇴치하세요.\n몬스터가 등장하면 주먹을 쥐고 손으로 직접 공격하세요.\n제한 시간 내에 괴물을 퇴치하지 못하면 미션은 실패입니다!";
            default:
                return "";
        }
    }

    public void ReturnToHub()
    {
        Debug.Log("[GameSceneManager] Hub로 복귀");
        LoadSceneAdditive(HUB_SCENE);
    }

    public string GetCurrentScene()
    {
        return currentLoadedScene;
    }

    public void QuitGame()
    {
        Debug.Log("[GameSceneManager] 게임 종료");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}