using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Start : MonoBehaviour
{
    public string sceneToLoad = "Slam_Scene";
    public string subSceneToLoad = "XR_HandTracking_Scene";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void GoToSlamScene()
    {
        //SceneManager.LoadScene("Main_Scene");

        SceneManager.LoadScene(sceneToLoad); // �⺻ �� �ε�
        SceneManager.LoadScene(subSceneToLoad, LoadSceneMode.Additive); // �ΰ� �� �߰� �ε�
    }
}
