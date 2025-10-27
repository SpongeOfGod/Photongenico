using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public HUD_Controller HUD_Controller;

    public GameObject leaderUi;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] ladderboard leaderboard;
    [SerializeField] ChaseCamera chaseCamera;
    [SerializeField] LobbyUIController LobbyUIController;
    [SerializeField] Transform[] spawnpositions;
    [SerializeField] public CrashInfo crashInfo; 
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

    public Vector3 GetRandomSpawn() => spawnpositions[Random.Range(0, spawnpositions.Length - 1)].position;

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
    public override void OnDisconnected(DisconnectCause cause)
    {
        CrashInfo crash = Instantiate(crashInfo);

        crash.SetMessage(cause.ToString());

        DontDestroyOnLoad(crash);
    }
    public void InstantiatePlayerCar(GameObject prefab)
    {
        CreateNewPlayer(prefab);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"Player: {newPlayer.NickName} entered the room!");
    }

    public void CreateNewPlayer(GameObject prefab)
    {
        GameObject player = PhotonNetwork.Instantiate(prefab.name, spawnpositions[PhotonNetwork.CurrentRoom.PlayerCount -1].position, Quaternion.identity);
        var playername = player.GetComponent<PhotonView>().Owner.NickName;

        ScoreManager.Instance.createscoreprefab(playername, 10);
        player.GetComponent<Playerseeleaderbord>().Leaderboard = leaderUi;

        player.TryGetComponent(out CarNameSync NameSync);
        if (player != null)
        {
            chaseCamera.TargetObject = player;

            if (NameSync != null)
                NameSync.PhotonView.RPC("ChangeUsername", RpcTarget.AllBuffered, null);
        }
    }
}
