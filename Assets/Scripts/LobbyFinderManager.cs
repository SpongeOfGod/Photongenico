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
    public enum MenuState { Nickname, ServerInfo }
    [HideInInspector] public MenuState state;

    [Header("Nickname - MenuState")]
    public Transform NicknameWindow;
    public TMP_InputField PlayerName;
    public Button StartButton;

    [Header("Nickname - ServerInfo")]
    public Transform ServerInfoWindow;
    public TMP_InputField CreateLobbyField;
    public TextMeshProUGUI PlayingAsName;
    public TextMeshProUGUI MaxPlayersText;
    public Button buttonSustract;
    public Button buttonAddup;
    public Button CreateButton;
    public Button GoBack;
    public string SceneToload;
    public int MaxNumberOfPlayers = 2;


    public TMP_InputField JoinLobbyField;
    public Button buttonJoinLobby;

    [Header("Error Handler")]
    public Transform ErrorTextHolder;
    public TextMeshProUGUI PrefabText;

    private string NicknameKey = "PlayerNickname";
    private string Nickname;
    private bool isConnected;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
    }
    void Start()
    {
        CreateLobbyField.onValueChanged.AddListener(CheckCreateLobby);
        JoinLobbyField.onValueChanged.AddListener(CheckLoadLobby);
        PlayerName.onValueChanged.AddListener(CheckName);
        StartButton.onClick.AddListener(() => SwitchState(MenuState.ServerInfo));
        buttonJoinLobby.onClick.AddListener(() => ConnectToLobby(false));
        CreateButton.onClick.AddListener(() => ConnectToLobby(true));
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

    private void CheckLoadLobby(string name)
    {
        if (JoinLobbyField.text != string.Empty)
        {
            buttonJoinLobby.interactable = true;
        }
        else
            buttonJoinLobby.interactable = false;
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        isConnected = true;
    }
    private void CheckCreateLobby(string name)
    {
        if (CreateLobbyField.text != string.Empty)
        {
            CreateButton.interactable = true;
        }
        else
            CreateButton.interactable = false;
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

    public void ConnectToLobby(bool isCreatingLobby)
    {
        PlayerPrefs.SetString(NicknameKey, Nickname);
        PhotonNetwork.NickName = Nickname.ToUpper();

        CreateButton.interactable = false;
        CreateLobbyField.interactable = false;
        PlayerName.interactable = false;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        else
            isConnected = true;

        StartCoroutine(WaitCreateAndJoin(isCreatingLobby));
    }

    IEnumerator WaitCreateAndJoin(bool isCreatingLobby)
    {
        while (!isConnected) 
        {
            yield return null;
        }

        if (isCreatingLobby)
            CreateRoom();
        else
            JoinRoom();
    }

    private void JoinRoom()
    {
        if (JoinLobbyField.text != "")
        {
            PhotonNetwork.JoinRoom(JoinLobbyField.text);
        }
    }

    private void CreateRoom()
    {
        if (CreateLobbyField.text != "")
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = MaxNumberOfPlayers;
            PhotonNetwork.CreateRoom(CreateLobbyField.text, roomOptions, TypedLobby.Default);
            roomOptions.IsVisible = true;
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

        CreateButton.interactable = true;
        CreateLobbyField.interactable = true;
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
