
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Teleport Trigger] 충돌 감지됨: {other.name}");

        if (other.name.Contains("Index") || other.name.Contains("Hand") || other.CompareTag("PlayerHand"))
        {
            string tag = gameObject.tag;
            string targetScene = "";

            switch (tag)
            {
                case "ToSlam":
                    targetScene = "Slam_Scene";
                    break;
                case "ToOasis":
                    targetScene = "Mini_Oasis";
                    break;
                case "ToRabbit":
                    targetScene = "MiniGame_Rabbit";
                    break;
                case "ToBear":
                    targetScene = "MiniGame_Bear";
                    break;
                case "ToFox":
                    targetScene = "MiniGame_Fox";
                    break;
                case "ToPenguin":
                    targetScene = "MiniGame_Penguin";
                    break;
                case "ToMain":
                    targetScene = "Main_Scene";
                    break;
                default:
                    Debug.LogWarning($"[Teleport] 태그 '{tag}'에 해당하는 씬 없음 (ToMulti 제외됨)");
                    return;
            }

            Debug.Log($"씬 전환 시도 → {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
    }
}
