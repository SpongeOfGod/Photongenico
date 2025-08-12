using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Vector3 InitialPosition;
    private List<PlayerController> CurrentPlayers = new();

    public override void OnJoinedRoom()
    {
        CreateNewPlayer(PhotonNetwork.CurrentRoom.Players[PhotonNetwork.CurrentRoom.Players.Count]);

        foreach (var item in CurrentPlayers)
        {
            item.TryGetComponent(out PhotonView PV);

            PV.RPC("ChangeUsername", RpcTarget.AllBuffered, item.PlayerName);
        }
    }

    private void CreateNewPlayer(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        newPlayer.NickName = PlayerPrefab.name + PhotonNetwork.CurrentRoom.Players.Count;
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, InitialPosition, Quaternion.identity);

        player.TryGetComponent(out PlayerController component);

        CurrentPlayers.Add(component);

        if (component != null)
            component.ChangeUsername(newPlayer.NickName);
    }
}
