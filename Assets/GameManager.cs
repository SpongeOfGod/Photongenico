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
        CreateNewPlayer(PhotonNetwork.CurrentRoom.Players[1]);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //CreateNewPlayer(newPlayer);
    }

    private void CreateNewPlayer(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        newPlayer.NickName = PlayerPrefab.name + PhotonNetwork.CurrentRoom.Players.Count;
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, InitialPosition, Quaternion.identity);

        Debug.Log( newPlayer.NickName);
        player.TryGetComponent(out PlayerController component);

        if (component != null)
            component.ChangeUsername(newPlayer.NickName);
    }
}
