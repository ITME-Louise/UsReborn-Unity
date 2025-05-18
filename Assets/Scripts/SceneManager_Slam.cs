using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Slam : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // 앱 실행 시 Slam_Scene을 Additive로 로드
        SceneManager.LoadScene("Slam_Scene", LoadSceneMode.Additive);
    }
}
