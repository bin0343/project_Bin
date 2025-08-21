using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public partial class PhotonManager : MonoBehaviourPunCallbacks      //override = puncallback에 있음. 없으면 직접 만든 함수
{
    public PhotonView PV;

    private void Awake()
    {
        PhotonNetwork.GameVersion = "1.0.0";    //게임버전 일치해야함.
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 10;   //통신속도와 연관있음

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)      //연결중에 끊거나, 실패하면 호출
    {
        base.OnDisconnected(cause);
    }

    public override void OnConnectedToMaster()      //제일 처음 유저를 방장으로 처리.
    {
        base.OnConnectedToMaster();

        PhotonNetwork.JoinLobby();

        Debug.Log("OnConnectedToMaster");
    }

    public override void OnJoinedLobby()        //연결되면 로비로 연결
    {
        base.OnJoinedLobby();

        Debug.Log("OnJoinnedLobby");
    }

    public void OnLobby()
    {
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    public void LeaveLobby()        //로비를 나갈때(인원 감소시킴)
    {
        PhotonNetwork.LeaveLobby();
    }
}
