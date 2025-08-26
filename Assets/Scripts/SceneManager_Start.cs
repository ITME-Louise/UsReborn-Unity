using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Start : MonoBehaviour
{
    public string sceneToLoad;
    public string subSceneToLoad;
    // Start is called before the first frame update
    

    // Update is called once per frame
    public void GoToNextScene()
    {
        //SceneManager.LoadScene("Main_Scene");

        SceneManager.LoadScene(sceneToLoad); // 기본 씬 로드
        SceneManager.LoadScene(subSceneToLoad, LoadSceneMode.Additive); // 부가 씬 추가 로드
    }
}
