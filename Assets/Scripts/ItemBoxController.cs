using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBoxController : MonoBehaviour
{
    PhotonView photonView;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Player"))
        {
            photonView.RPC("ItemReaction", RpcTarget.All);
        }
    }
    [PunRPC]
    private void ItemReaction() 
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameManager.GameStates.InRound)
            photonView.RPC("ItemReaction", RpcTarget.All);
    }
}
