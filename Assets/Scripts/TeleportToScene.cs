using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered) return;

        if (other.name.Contains("Index") || other.name.Contains("Hand") || other.CompareTag("PlayerHand"))
        {
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

            hasTriggered = true;
            SceneManager.LoadScene(targetScene);
        }
    }
}