using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; //Pun 쓰기 위한 내용

public class MultiGame : MonoBehaviourPunCallbacks //pun과 관련된 통신내용을 상속
{
    string gameVersion = "1"; //게임버전을 넣으라고 했는데, 그냥 아무거나 넣은 것
    public GameObject player; //우리가 만든 플레이어(공 : 나)
    // Start is called before the first frame update
    public override void OnConnectedToMaster() //포톤 어플리케이션 만들어 놓은 것 (웹)
    {
        Debug.Log("OnConnectedToMaster() was called by PUN.");
        //PhotonNetwork. JoinRandomRoom();
        PhotonNetwork.LocalPlayer.NickName = "aaaa"; //접속이름
        //PhotonNetwork. JoinLobby();
        print("마스터 접속 완료");

        //PhotonNetwork.ConnectUsingSettings();
        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions();
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 4; //접속자 수 최대인원
        PhotonNetwork.JoinOrCreateRoom("aaaa", roomOptions, Photon.Realtime.TypedLobby.Default);
        //룸이름 aaaa aaaa라는 룸을 만들거나 있으면 들어간다

    } //여기까지가 웹에 접속
    public override void OnJoinedLobby()
    {
        print("로비 접속 완료");

    }
    public override void OnJoinedRoom() //플레이어를 실제로 만드는 영역
    {
        print("룸 접속 완료");
        Vector3 position = new Vector3(0, 0, 0);
        PhotonNetwork.Instantiate("NetworkedPlayer", position, Quaternion.identity);
    }

    // Start is called before the first frame update
    void Start()
    {
        Connect();
    }
    public void Connect() => PhotonNetwork.ConnectUsingSettings(); //접속하라고 명령

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting)
        //{
        //    Vector3 pos = transform.localPosition;
        //    stream.Serialize(ref pos);
        //}
        //else
        //{
        //    Vector3 pos = Vector3.zero;
        //    stream.Serialize(ref pos);  // pos gets filled-in. must be used somewhere
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
