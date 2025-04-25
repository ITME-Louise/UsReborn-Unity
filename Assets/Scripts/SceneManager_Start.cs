using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Start : MonoBehaviour
{
    public string sceneToLoad = "Main_Scene";
    public string subSceneToLoad = "Example_01";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void GoToMainScene()
    {
        //SceneManager.LoadScene("Main_Scene");

        SceneManager.LoadScene(sceneToLoad); // �⺻ �� �ε�
        SceneManager.LoadScene(subSceneToLoad, LoadSceneMode.Additive); // �ΰ� �� �߰� �ε�
    }
}
