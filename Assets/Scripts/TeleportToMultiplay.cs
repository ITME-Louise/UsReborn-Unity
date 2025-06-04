using UnityEngine;
using Photon.Pun;

public class TeleportToMultiplay : MonoBehaviourPun
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
                case "ToMulti":
                    targetScene = "Multiplay_Scene";
                    break;
                case "ToOasis":
                    targetScene = "Mini_Oasis";
                    break;
                case "ToMain":
                    targetScene = "Main_Scene";
                    break;
                // 필요 시 case 추가 가능
                default:
                    Debug.LogWarning($"[Teleport] 태그 '{tag}'에 해당하는 씬 없음");
                    return;
            }

            Debug.Log($"[Teleport] PhotonNetwork.LoadLevel → {targetScene}");
            PhotonNetwork.LoadLevel(targetScene);
        }
    }
}
