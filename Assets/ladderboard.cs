using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

public class ladderboard : MonoBehaviourPunCallbacks
{
    public List<GameObject> prefabs;
   
    public void Intanceincanvas()
    {
        Instantiate(prefabs[PhotonNetwork.CurrentRoom.PlayerCount - 1].gameObject,this.transform);
        Debug.Log("se ha instanciado");


    }
    [PunRPC]
    public void shortlist()
    {
        int n = prefabs.Count - 1;
        bool swapped = false;

        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (prefabs[j].GetComponent<setnames>().scorenum > prefabs[j + 1].GetComponent<setnames>().scorenum)
                {
                    var tempposition = prefabs[j].transform.position;
                    var temp = prefabs[j];
                    prefabs[j] = prefabs[j + 1];
                    prefabs[j].transform.position = prefabs[j + 1].transform.position;
                    prefabs[j + 1].transform.position = tempposition;

                    swapped = true;
                }
            }
              if (swapped == false)
                break;
        }
    }

 
    public void updatescores(string name, float score)
    {
        int i = 0;
        foreach( var scores in prefabs)
        {

            if (prefabs[i].name != name)
            {
                i++;
            }
            else
            {
                prefabs[i].GetComponent<setnames>().setscore(score);
            }
        }
        shortlist();
    }
}


