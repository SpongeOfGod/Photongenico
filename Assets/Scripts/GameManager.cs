using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public HUD_Controller HUD_Controller;

    [SerializeField] GameObject PlayerPrefab;

    [SerializeField] ChaseCamera chaseCamera;
    [SerializeField] LobbyUIController LobbyUIController;
    [SerializeField] Transform[] spawnpositions;
    [SerializeField] public CrashInfo crashInfo;
    public MashButtonManager mashButtonManager;
    public LeaderboardManager leaderboardManager;
    [HideInInspector] public GameStates GameState { get; private set; }

    public enum GameStates
    {
        Startup, Interlude, InRound
    }

    public void ChangeState(GameStates newState)
    {
        GameState = newState;
    }

    [PunRPC]
    public void InitalizeMashFight(string playerA, string playerB)
    {
        mashButtonManager.transform.parent.gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            int actorA = PhotonNetwork.PlayerList.First(p => p.NickName == playerA).ActorNumber;
            int actorB = PhotonNetwork.PlayerList.First(p => p.NickName == playerB).ActorNumber;


            string p1, p2;

            if (actorA < actorB)
            {
                p1 = playerA;
                p2 = playerB;
            }
            else
            {
                p1 = playerB;
                p2 = playerA;
            }

            mashButtonManager.photonView.RPC(
                "SyncMashFightRolesAndStart",
                RpcTarget.All,
                p1,
                p2
            );
        }
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
        string playername = player.GetComponent<PhotonView>().Owner.NickName;
        float playerscore = player.GetComponent<CarScore>().Score;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.View.RPC("createscoreprefab", RpcTarget.AllBuffered, playername, playerscore);

        player.TryGetComponent(out CarNameSync NameSync);
        if (player != null)
        {
            chaseCamera.TargetObject = player;

            if (NameSync != null)
                NameSync.PhotonView.RPC("ChangeUsername", RpcTarget.AllBuffered, null);
        }
    }
}
