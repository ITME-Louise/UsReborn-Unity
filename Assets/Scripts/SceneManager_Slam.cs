using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Slam : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // �� ���� �� Slam_Scene�� Additive�� �ε�
        SceneManager.LoadScene("Slam_Scene", LoadSceneMode.Additive);
    }
}
