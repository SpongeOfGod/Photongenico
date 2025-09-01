using Photon.Pun;
using Photon.Realtime;
using System;
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

        player.TryGetComponent(out PlayerController component);

        CurrentPlayers.Add(component);

        component.GetComponent<PhotonView>().RPC("ChangeUsername", RpcTarget.AllBuffered, null);
    }
}
