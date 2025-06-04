using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class restartTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("PlayerHand"))
        {
            hasTriggered = true;
            Debug.Log("손과 버튼 충돌됨. 게임 다시 시작!");
            MiniGameManager_Penguin.Instance.RestartGame();
        }
    }
}
