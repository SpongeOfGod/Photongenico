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
    public enum MenuState { Nickname, ServerInfo}
    [HideInInspector] public MenuState state;

    [Header("Nickname - MenuState")]
    public Transform NicknameWindow;
    public TMP_InputField PlayerName;
    public Button StartButton;

    [Header("Nickname - ServerInfo")]
    public Transform ServerInfoWindow;
    public TMP_InputField Lobby;
    public TextMeshProUGUI PlayingAsName;
    public TextMeshProUGUI MaxPlayersText;
    public Button buttonSustract;
    public Button buttonAddup;
    public Button ConnectButton;
    public Button GoBack;
    public string SceneToload;
    public int MaxNumberOfPlayers = 2;

    [Header("Error Handler")]
    public Transform ErrorTextHolder;
    public TextMeshProUGUI PrefabText;

    private string NicknameKey = "PlayerNickname";
    private string Nickname;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
    }
    void Start()
    {
        Lobby.onValueChanged.AddListener(CheckLobby);
        PlayerName.onValueChanged.AddListener(CheckName);
        StartButton.onClick.AddListener(() => SwitchState(MenuState.ServerInfo));
        ConnectButton.onClick.AddListener(ConnectToLobby);
        GoBack.onClick.AddListener(() => SwitchState(MenuState.Nickname));
    }

    private void CheckName(string name) 
    {
        if (PlayerName.text != string.Empty)
        {
            StartButton.gameObject.SetActive(true);
            Nickname = PlayerName.text;
            PlayingAsName.text = name;
        }
        else
            StartButton.gameObject.SetActive(false);
    }

    private void CheckLobby(string name)
    {
        if (Lobby.text != string.Empty)
        {
            ConnectButton.gameObject.SetActive(true);
        }
        else
            ConnectButton.gameObject.SetActive(false);
    }

    public void ChangeMaxPlayersValue(int value) 
    {
        if (MaxNumberOfPlayers + value >= 2 && MaxNumberOfPlayers + value <= 4)
            MaxNumberOfPlayers += value;

        buttonSustract.gameObject.SetActive(MaxNumberOfPlayers > 2);
        buttonAddup.gameObject.SetActive(MaxNumberOfPlayers < 4);

        MaxPlayersText.text = MaxNumberOfPlayers.ToString();
    }

    public void SwitchState(MenuState newState) 
    {
        state = newState;

        switch (state) 
        {
            case MenuState.Nickname:

                ServerInfoWindow.gameObject.SetActive(false);
                NicknameWindow.gameObject.SetActive(true);
                break;

            case MenuState.ServerInfo:

                NicknameWindow.gameObject.SetActive(false);
                ServerInfoWindow.gameObject.SetActive(true);
                break;
        }
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
