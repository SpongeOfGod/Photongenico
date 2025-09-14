using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarNameSync : MonoBehaviour
{
    [HideInInspector] public PhotonView PhotonView;
    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void ChangeUsername()
    {
        var text = GetComponentInChildren<TextMeshPro>();
        PhotonView = GetComponent<PhotonView>();

        if (PhotonView.IsMine)
            text.text = PhotonNetwork.NickName;
        else
            text.text = PhotonView.Owner.NickName;
    }
}
