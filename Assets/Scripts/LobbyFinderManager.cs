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

    [Header("Lobby List")]
    public Transform LobbyListContainer;
    public GameObject LobbyEntryPrefab;
    private Dictionary<string, GameObject> activeLobbies = new Dictionary<string, GameObject>();

    private string NicknameKey = "PlayerNickname";
    private string Nickname;
    private bool isConnected = false;
    private bool isInLobby = false;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.ConnectUsingSettings();
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
        if (!string.IsNullOrEmpty(PlayerName.text) && isConnected)
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
        if (!string.IsNullOrEmpty(JoinLobbyField.text))
            buttonJoinLobby.gameObject.SetActive(true);
        else
            buttonJoinLobby.gameObject.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        isConnected = true;

        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        isInLobby = true;
    }

    private void CheckCreateLobby(string name)
    {
        if (!string.IsNullOrEmpty(CreateLobbyField.text))
            CreateButton.gameObject.SetActive(true);
        else
            CreateButton.gameObject.SetActive(false);
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

        CreateButton.gameObject.SetActive(false);
        CreateLobbyField.gameObject.SetActive(false);
        PlayerName.gameObject.SetActive(false);

        if (PhotonNetwork.IsConnected)
            isConnected = true;

        StartCoroutine(WaitCreateAndJoin(isCreatingLobby));
    }

    IEnumerator WaitCreateAndJoin(bool isCreatingLobby)
    {
        while (!isConnected)
            yield return null;

        while (!isInLobby)
            yield return null;

        if (isCreatingLobby)
            CreateRoom();
        else
            JoinRoom();
    }

    private void JoinRoom()
    {
        if (!string.IsNullOrEmpty(JoinLobbyField.text))
            PhotonNetwork.JoinRoom(JoinLobbyField.text);
    }

    private void CreateRoom()
    {
        if (!string.IsNullOrEmpty(CreateLobbyField.text))
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = MaxNumberOfPlayers;
            roomOptions.IsVisible = true;
            PhotonNetwork.CreateRoom(CreateLobbyField.text, roomOptions, TypedLobby.Default);
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

        CreateButton.gameObject.SetActive(true);
        CreateLobbyField.gameObject.SetActive(true);
        PlayerName.gameObject.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");
        HandleErrorMessage(message + $"\n error code: {returnCode}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");
        HandleErrorMessage(message + $"\n error code: {returnCode}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message + $"\n error code: {returnCode}");
        HandleErrorMessage(message + $"\n error code: {returnCode}");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var room in new List<string>(activeLobbies.Keys))
        {
            if (!roomList.Exists(r => r.Name == room))
            {
                Destroy(activeLobbies[room]);
                activeLobbies.Remove(room);
            }
        }

        foreach (RoomInfo roomInfo in roomList)
        {
            if (activeLobbies.ContainsKey(roomInfo.Name))
            {
                var text = activeLobbies[roomInfo.Name].GetComponentInChildren<TextMeshProUGUI>();
                text.text = $"{roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})";
            }
            else
            {
                GameObject entry = Instantiate(LobbyEntryPrefab, LobbyListContainer);
                var text = entry.GetComponentInChildren<TextMeshProUGUI>();
                text.text = $"{roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})";

                Button btn = entry.GetComponentInChildren<Button>();
                string lobbyName = roomInfo.Name;
                btn.onClick.AddListener(() => JoinLobbyFromButton(lobbyName));

                activeLobbies.Add(roomInfo.Name, entry);
            }
        }
    }

    private void JoinLobbyFromButton(string lobbyName)
    {
        JoinLobbyField.text = lobbyName;
        ConnectToLobby(false);
    }
}
