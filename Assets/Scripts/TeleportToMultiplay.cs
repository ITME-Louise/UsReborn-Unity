using UnityEngine;
using Photon.Pun;

public class TeleportToMultiplay : MonoBehaviourPun
{
    private bool hasTriggered = false; // 중복 방지

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
                case "ToMulti":
                    targetScene = "Multiplay_Scene";
                    break;
                case "ToOasis":
                    targetScene = "Mini_Oasis";
                    break;
                case "ToMain":
                    targetScene = "Main_Scene";
                    break;
                default:
                    Debug.LogWarning($"[Teleport] 태그 '{tag}'에 해당하는 씬 없음");
                    return;
            }

            Debug.Log($"[Teleport] PhotonNetwork.LoadLevel → {targetScene}");
            hasTriggered = true;
            PhotonNetwork.LoadLevel(targetScene);
        }
    }
} 
