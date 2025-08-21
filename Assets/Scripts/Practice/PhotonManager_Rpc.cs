using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public partial class PhotonManager : MonoBehaviourPunCallbacks      //override = puncallback에 있음. 없으면 직접 만든 함수
{
    [PunRPC]
    void LobbyRoomEntry(bool _Owner)
    {

    }

    [PunRPC]
    void LobbyRoomInfo(Room _Room)
    {

    }

    void StartInGame()  //파라미터가 같아야함
    {

    }
}
