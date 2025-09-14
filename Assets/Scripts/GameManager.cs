using Adrenak.Tork.Demo;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Vector3 InitialPosition;
    [SerializeField] SmoothFollow smoothFollow;
    [SerializeField] LobbyUIController LobbyUIController;
    [HideInInspector] public GameStates GameState { get; private set; }
    public enum GameStates 
    {
        Startup, Interlude, InRound
    }

    public void ChangeState(GameStates newState) 
    {
        GameState = newState;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            OnJoinedRoom();
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount < 2 && GameState != GameStates.Startup && PhotonNetwork.IsMasterClient) 
        {
            ChangeState(GameStates.Startup);
            LobbyUIController.photonView.RPC("ReturnToStartup", RpcTarget.All);
        }

    }

    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom(this);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    public override void OnJoinedRoom()
    {
        CreateNewPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        Debug.Log($"Player: {newPlayer.NickName} entered the room!");
    }

    private void CreateNewPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, InitialPosition, Quaternion.identity);

        player.TryGetComponent(out CarNameSync NameSync);
        if (player != null) 
        {
            smoothFollow.target = player.transform;
            if (NameSync != null)
            NameSync.PhotonView.RPC("ChangeUsername", RpcTarget.AllBuffered, null);
        }
    }
}
