using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ladderboard : MonoBehaviour
{
    public List<GameObject> prefabs;
    public void Intanceincanvas()
    {
        Instantiate(prefabs[PhotonNetwork.CurrentRoom.PlayerCount - 1].gameObject,this.transform);
        Debug.Log("se ha instanciado");
    }
 
}


