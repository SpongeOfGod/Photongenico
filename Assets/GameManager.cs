using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Vector3 InitialPosition;

    public override void OnJoinedRoom()
    {
        (PhotonView, string) PV = CreateNewPlayer(PhotonNetwork.CurrentRoom.Players[PhotonNetwork.CurrentRoom.Players.Count]);

        PV.Item1.RPC("ChangeUsername", RpcTarget.AllBuffered, PV.Item2);
    }

    private (PhotonView, string) CreateNewPlayer(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        newPlayer.NickName = PlayerPrefab.name + PhotonNetwork.CurrentRoom.Players.Count;
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, InitialPosition, Quaternion.identity);

        player.TryGetComponent(out PlayerController component);

        if (component != null)
            component.ChangeUsername(newPlayer.NickName);

        return (player.GetComponent<PhotonView>(), newPlayer.NickName);
    }
}
