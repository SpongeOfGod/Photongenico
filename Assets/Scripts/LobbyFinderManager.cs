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
    [Header("Connection")]
    public TMP_InputField Lobby;
    public TMP_InputField PlayerName;
    public Button ConnectButton;

    [Header("Error Handler")]
    public Transform ErrorTextHolder;
    public TextMeshProUGUI PrefabText;

    private string NicknameKey = "PlayerNickname";
    private string Nickname;

    public string SceneToload;
    public int MaxNumberOfPlayers = 2;
    public TextMeshProUGUI MaxPlayersText;
    public Button buttonSustract;
    public Button buttonAddup;


    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
    }
    void Start()
    {
        Lobby.onValueChanged.AddListener(CheckLobbyAndName);
        PlayerName.onValueChanged.AddListener(CheckLobbyAndName);
        ConnectButton.onClick.AddListener(ConnectToLobby);
    }

    public void ChangeMaxPlayersValue(int value) 
    {
        if (MaxNumberOfPlayers + value >= 2 && MaxNumberOfPlayers + value <= 4)
            MaxNumberOfPlayers += value;

        buttonSustract.gameObject.SetActive(MaxNumberOfPlayers > 2);
        buttonAddup.gameObject.SetActive(MaxNumberOfPlayers < 4);

        MaxPlayersText.text = MaxNumberOfPlayers.ToString();
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

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        else
            JoinOrCreateRoom();
    }

    public override void OnConnectedToMaster()
    {
        JoinOrCreateRoom();
    }

    private void JoinOrCreateRoom()
    {
        if (Lobby.text != "") 
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = MaxNumberOfPlayers;
            PhotonNetwork.JoinOrCreateRoom(Lobby.text, roomOptions, TypedLobby.Default);
            roomOptions.IsVisible = false;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene(SceneToload);
    }

    private void HandleErrorMessage(string message) 
    {
        var prefab = Instantiate(PrefabText, ErrorTextHolder);
        prefab.text = message;
        prefab.gameObject.SetActive(true);

        ConnectButton.interactable = true;
        Lobby.interactable = true;
        PlayerName.interactable = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");

        string newMessage = message + $"\n error code: {returnCode}";

        HandleErrorMessage(newMessage);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");

        string newMessage = message + $"\n error code: {returnCode}";

        HandleErrorMessage(newMessage);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");

        string newMessage = message + $"\n error code: {returnCode}";

        HandleErrorMessage(newMessage);
    }
}
