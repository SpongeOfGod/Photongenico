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
    public List<GameObject> moveprefabs;
  
    public void Intanceincanvas()
    {
        Instantiate(moveprefabs[PhotonNetwork.CurrentRoom.PlayerCount - 1].gameObject,this.transform);
        Debug.Log("se ha instanciado")


    }


    [PunRPC]
    public void shortlist()
    {
        int n = moveprefabs.Count - 1;
        bool swapped = false;

        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (moveprefabs[j].GetComponent<setnames>().scorenum > moveprefabs[j + 1].GetComponent<setnames>().scorenum)
                {
                    var tempposition = moveprefabs[j].transform.position;
                    var temp = moveprefabs[j];
                     moveprefabs[j] = moveprefabs[j + 1];
                    moveprefabs[j].transform.position = moveprefabs[j + 1].transform.position;
                    moveprefabs[j + 1].transform.position = tempposition;

                    swapped = true;
                }
            }
        }
    }
    
    public void updatescores()
    {
     
    }
}


