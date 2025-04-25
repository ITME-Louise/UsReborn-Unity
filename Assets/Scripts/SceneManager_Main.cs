using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Main : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnMouseDown()
    {
        Debug.Log("ø¿∫Í¡ß∆Æ ≈¨∏Øµ !");
        SceneManager.LoadScene("Slam_Scene"); // ø¯«œ¥¬ æ¿ ¿Ã∏ß¿∏∑Œ πŸ≤„¡‡!
    }
}
