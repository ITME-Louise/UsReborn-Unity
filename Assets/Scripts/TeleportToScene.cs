using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    private bool hasTriggered = false; // 중복 방지용

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered) return;

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
                    targetScene = "MiniGame_Rabbit_Scene";
                    break;
                case "ToBear":
                    targetScene = "MiniGame_Bear_Scene";
                    break;
                case "ToFox":
                    targetScene = "MiniGame_Fox_Scene";
                    break;
                case "ToPenguin":
                    targetScene = "MiniGame_Penguin_Scene";
                    break;
                case "ToMain":
                    targetScene = "Main_Scene";
                    break;
                default:
                    Debug.LogWarning($"[Teleport] 태그 '{tag}'에 해당하는 씬 없음 (ToMulti 제외됨)");
                    return;
            }

            Debug.Log($"씬 전환 시도 → {targetScene}");
            hasTriggered = true; // 중복 호출 방지
            SceneManager.LoadScene(targetScene);
        }
    }
}
