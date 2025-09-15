using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemBoxController : MonoBehaviour
{
    PhotonView photonView;
    public List<Weapon> WeaponPrefabs = new();
    private GameObject CollidingPlayer;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Player"))
        {
            CollidingPlayer = other.gameObject;
            photonView.RPC("ItemReaction", RpcTarget.All, other.gameObject);
        }
    }
    [PunRPC]
    private void ItemReaction() 
    {
        if (CollidingPlayer != null) 
        {
            System.Random random = new System.Random();

            int index = random.Next(0, WeaponPrefabs.Count);

            CollidingPlayer.TryGetComponent(out CarWeaponController CarWeaponController);
            
            if (CarWeaponController != null) 
            {
                var weapon = Instantiate(WeaponPrefabs[index]);

                CarWeaponController.CurrentWeapon = weapon;
            }
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameManager.GameStates.InRound)
            photonView.RPC("ItemReaction", RpcTarget.All, null);
    }
}
