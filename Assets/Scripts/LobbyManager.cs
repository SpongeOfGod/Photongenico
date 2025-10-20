using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("LobbyMenu")]
    public LobbyMenu LobbyMenu;

    [Header("Error Handler")]
    public ErrorHandler ErrorHandler;

    [Header("Lobby List")]
    public int MaxNumberOfPlayers = 2;
    public Transform LobbyListContainer;
    public GameObject LobbyEntryPrefab;
    private Dictionary<string, GameObject> activeLobbies = new Dictionary<string, GameObject>();

    private string NicknameKey = "PlayerNickname";
    [HideInInspector] public string LobbyName;
    [HideInInspector] public string Nickname;
    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool isInLobby = false;
    [HideInInspector] public string SceneToload;
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        isConnected = true;

        PhotonNetwork.JoinLobby(TypedLobby.Default);

        LobbyMenu.TurnOnButton();
    }

    public override void OnJoinedLobby() => isInLobby = true;

    public void ConnectToLobby(bool isCreatingLobby)
    {
        PlayerPrefs.SetString(NicknameKey, Nickname);
        PhotonNetwork.NickName = Nickname.ToUpper();

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
        PhotonNetwork.JoinRoom(LobbyName);
    }

    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = MaxNumberOfPlayers;
        roomOptions.IsVisible = true;
        PhotonNetwork.CreateRoom(LobbyName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene(SceneToload);
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
        this.LobbyName = lobbyName;
        LobbyMenu.ChangeTextName(lobbyName);
        ConnectToLobby(false);
    }
}
