using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    
    void Start()
    {
        Debug.Log("PlayerSpawner.Start() 실행됨");
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("PhotonNetwork 아직 연결 안 됨 → ConnectUsingSettings() 실행");
            PhotonNetwork.AutomaticallySyncScene = true;
            
            //  추가: Region 고정 + GameVersion 설정
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
            PhotonNetwork.GameVersion = "v1.0";
            
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("PhotonNetwork 연결 완료 → 즉시 스폰");
            SpawnPlayer();
        }
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log($" 마스터 서버 연결 완료 (Region: {PhotonNetwork.CloudRegion}) → 로비 참가 시도");
        PhotonNetwork.JoinLobby();
    }
    
    public override void OnJoinedLobby()
    {
        Debug.Log(" 로비 입장 완료 → 방 참가 또는 생성 시도");
        PhotonNetwork.JoinOrCreateRoom("DefaultRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }
    
    public override void OnJoinedRoom()
    {
        // 더 자세한 로그
        Debug.Log($" 방 입장 완료! Room: {PhotonNetwork.CurrentRoom.Name}, Players: {PhotonNetwork.CurrentRoom.PlayerCount}, Region: {PhotonNetwork.CloudRegion} ");
        
        //  모든 플레이어 ID 출력
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"  - Player: {player.NickName} (ID: {player.ActorNumber})");
        }
        
        SpawnPlayer();
    }
    
    void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0f,
            UnityEngine.Random.Range(-1f, 1f)
        );
        Debug.Log("생성 위치: " + spawnPos);
        
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        
        if (player != null)
        {
            Debug.Log($" 플레이어 프리팹 생성 완료: {player.name} (IsMine: {player.GetComponent<PhotonView>().IsMine})");
        }
        else
        {
            Debug.Log("플레이어 생성 실패");
        }
    }
}