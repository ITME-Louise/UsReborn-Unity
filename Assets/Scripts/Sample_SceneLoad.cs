using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sample_SceneLoad : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private string[] scenesToLoad =
    {
        "Scenes/MiniGame_Bear_Scene 1",
        "Scenes/Islands_Demo"
    };

    void Start()
    {
        LoadScenes();
    }

    void LoadScenes()
    {
        // 현재 활성화된 씬(빌드 세팅 첫 번째 씬) 제외하고 나머지 Additive 로드
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            if (!SceneManager.GetSceneByName(scenesToLoad[i]).isLoaded)
            {
                SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
            }
        }
    }
}
