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
        if (other.gameObject.CompareTag("Player"))
        {
            CollidingPlayer = other.gameObject;
            RPC_ItemReaction();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CollidingPlayer = collision.gameObject;
            RPC_ItemReaction();
        }
    }
    private void RPC_ItemReaction() 
    {
        if (CollidingPlayer != null) 
        {
            System.Random random = new System.Random();

            int index = random.Next(0, WeaponPrefabs.Count);

            CollidingPlayer.TryGetComponent(out CarWeaponController CarWeaponController);
            
            if (CarWeaponController != null) 
            {
                var weapon = PhotonNetwork.Instantiate(WeaponPrefabs[index].name, Vector2.zero, Quaternion.identity);
                CarWeaponController.CurrentWeapon = weapon.GetComponent<Weapon>();
            }
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameManager.GameStates.InRound && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}
