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
    [SerializeField] CinemachineVirtualCamera VirtualCamera;

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

        player.TryGetComponent(out CarController controller);
        if (controller != null) 
        {
            controller.AssignCameraTarget(VirtualCamera);
            controller.PhotonView.RPC("ChangeUsername", RpcTarget.AllBuffered, null);
        }
    }
}
