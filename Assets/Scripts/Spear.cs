using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : Weapon
{
    PhotonView PhotonView;
    private void Awake()
    {
        InitialTime = Time.time;
        PhotonView = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if (PhotonView.IsMine)
            GameManager.Instance.HUD_Controller.CurrentWeaponChange("Spear");

        if (Time.time - InitialTime >= timeToBeDestroyed && PhotonView.IsMine) 
        {
            GameManager.Instance.HUD_Controller.CurrentWeaponChange("None");
            PhotonView.RPC("Reaction", RpcTarget.All);
        }
    }

    [PunRPC]
    public void Reaction() 
    {
        Destroy(gameObject);
    }
}
