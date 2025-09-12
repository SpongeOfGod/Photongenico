using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string RoomName;
    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;
        PhotonNetwork.JoinRoom(RoomName);
        //PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
        //SceneManager.LoadScene(2);
        //roomOptions.IsVisible = false;
        //Debug.Log("hi");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("JoinSuccesful");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");
    }
}
