using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyFinderManager : MonoBehaviourPunCallbacks
{ 
    public TMP_InputField Lobby;
    public TMP_InputField PlayerName;
    public Button ConnectButton;
    public PhotonManager PhotonManager;

    private string NicknameKey = "PlayerNickname";
    private string Nickname;

    void Start()
    {
        Lobby.onValueChanged.AddListener(CheckLobbyAndName);
        PlayerName.onValueChanged.AddListener(CheckLobbyAndName);
        ConnectButton.onClick.AddListener(ConnectToLobby);
    }

    private void CheckLobbyAndName(string name) 
    {
        if (Lobby.text != string.Empty && PlayerName.text != string.Empty) 
        {
            ConnectButton.gameObject.SetActive(true);
            Nickname = PlayerName.text;
        }
        else
            ConnectButton.gameObject.SetActive(false);
    }

    public void ConnectToLobby() 
    {
        PlayerPrefs.SetString(NicknameKey, Nickname);
        PhotonNetwork.NickName = Nickname.ToUpper();

        ConnectButton.interactable = false;
        Lobby.interactable = false;
        PlayerName.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;
        SceneManager.LoadScene("SampleScene");
        PhotonNetwork.JoinOrCreateRoom(Lobby.text, roomOptions, TypedLobby.Default);
        roomOptions.IsVisible = false;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
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
