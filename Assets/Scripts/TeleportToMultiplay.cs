using UnityEngine;
using Photon.Pun;


public class TeleportToMultiplay : MonoBehaviour
{
    [SerializeField] private string multiplayerSceneName = "Multiplay_Scene";

    // 손가락이 트리거에 닿았을 때 호출
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Teleport Trigger] 충돌 감지됨: {other.name}");

        // 손 트래킹 오브젝트인지 확인 (필요에 따라 태그 or 이름 확인)
        if (other.name.Contains("Index") || other.name.Contains("Hand") || other.CompareTag("PlayerHand"))
        {
            Debug.Log("손가락 충돌 감지 → 멀티플레이 씬으로 전환 시도");
            PhotonNetwork.LoadLevel(multiplayerSceneName);
        }
    }
}
