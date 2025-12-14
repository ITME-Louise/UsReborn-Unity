using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

public class TeleportToScene : MonoBehaviour
{
    private bool hasTriggered = false;

    // Ray Interactable의 When Select 이벤트에서 호출
    public void OnSelected()
    {
        if (hasTriggered) return;

        string tag = gameObject.tag;
        string targetScene = "";

        switch (tag)
        {
            case "ToMulti":
                targetScene = "Multi_Scene";
                break;
            case "ToSlam":
                targetScene = "Slam_Scene";
                break;
            case "ToFox":
                targetScene = "MiniGame_Fox_Scene";
                break;
            case "ToRabbit":
                targetScene = "MiniGame_Rabbit_Scene";
                break;
            default:
                return;
        }

        Debug.Log($"[TeleportToScene] 씬 전환: {targetScene}");
        hasTriggered = true;
        SceneManager.LoadScene(targetScene);
    }
}