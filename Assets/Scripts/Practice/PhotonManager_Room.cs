using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public partial class PhotonManager : MonoBehaviourPunCallbacks     
{
    public void CreateLobbyRoom(string _Room = null)    //방 생성
    {
        if (_Room == null)
            return;

        PhotonNetwork.CreateRoom(_Room);
    }

    public void RandomLobbyRoom()       //방 랜덤 입장
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void JoinLobbyRoom(string _Room = null)
    {
        if (_Room == null)
            return;

        PhotonNetwork.JoinRoom(_Room);
    }

    public void LeaveRoom(bool _Com = true)
    {
        PhotonNetwork.LeaveRoom(_Com);
    }

    public void SecretLobbyRoom(string _Room, byte _Secret, byte _MaxPlayer)
    {
        if (_Room == null)
            return;

        bool Open = _Secret > 0 ? false : true;

        RoomOptions roomoption = new RoomOptions() { IsVisible = Open, MaxPlayers = _MaxPlayer };

        if (roomoption == null)
            return ;

        PhotonNetwork.JoinOrCreateRoom(_Room, roomoption, null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)       //방생성 실패
    {
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)       //방진입 실패
    {
        base.OnJoinRandomFailed(returnCode, message);
    }

    public void SendStartInGame()
    {
        PV.RPC("StartInGame", RpcTarget.All);       //파라미터가 같아야함.
    }
}
