using UnityEngine;
using Photon.Pun;


public class TeleportToMultiplay : MonoBehaviour
{
    [SerializeField] private string multiplayerSceneName = "Multiplay_Scene";

    // Unity Inspector에서 직접 연결할 public 메서드 (파라미터 없음)
    public void TriggerTeleport()
    {
        Debug.Log("Ray로 Plane 클릭됨 → 멀티플레이 씬 전환 시도");
        PhotonNetwork.LoadLevel(multiplayerSceneName);
    }
}
