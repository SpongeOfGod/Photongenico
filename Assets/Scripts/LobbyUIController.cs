using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI CurrentPlayersOnRoom;
    [SerializeField] TextMeshProUGUI StateType;
    [SerializeField] Button StartGameButton;
    [SerializeField] RoomOptions RoomOptions;
    public GameObject HUD;
    [HideInInspector] public PhotonView photonView;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        StartGameButton.onClick.AddListener(CallRPCStarGame);
        StartGameButton.gameObject.SetActive(false);
    }
    void Update()
    {
        if (PhotonNetwork.CurrentRoom != null) 
        {
            var currentNumberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
            CurrentPlayersOnRoom.text = $"Players: {currentNumberOfPlayers} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2;

            if (!StartGameButton.gameObject.activeSelf)
                StartGameButton.gameObject.SetActive(true);

            StateType.text = PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers ? "Max number of players reached" : "Waiting for players";
        }
        else
        {
            StateType.text = "Waiting for Master to start";
        }

    }
    private void CallRPCStarGame() 
    {
        photonView.RPC("StartGame", RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void StartGame() 
    {
        if (PhotonNetwork.CurrentRoom.IsOpen)
            PhotonNetwork.CurrentRoom.IsOpen = false;
        StartGameButton.transform.parent.gameObject.SetActive(false);
        GameManager.Instance.ChangeState(GameManager.GameStates.InRound);
        SoundManager.Instance.SetRoundMusic();
        HUD.SetActive(true);
    }

    [PunRPC]
    private void ReturnToStartup()
    {
        if (!PhotonNetwork.CurrentRoom.IsOpen)
            PhotonNetwork.CurrentRoom.IsOpen = true;

        if (PhotonNetwork.IsMasterClient)
            StartGameButton.transform.parent.gameObject.SetActive(true);

        SoundManager.Instance.SetStartmusic();
    }
}
