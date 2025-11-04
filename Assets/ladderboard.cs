using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
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
    public List<GameObject> ScorePrefabs;
     PhotonView view;

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    public void Intanceincanvas()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount - 1;
    for(int i = 0; i <= playerCount; i++)
        {
             var gObject = PhotonNetwork.Instantiate(ScorePrefabs[i].name, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
        gObject.transform.SetParent(transform);

        gObject.name = ScorePrefabs[i].GetComponent<setnames>().playname;
        }
       
    }
    [PunRPC]
    public void shortlist()
    {
        int n = ScorePrefabs.Count - 1;
        bool swapped = false;

        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (ScorePrefabs[j].GetComponent<setnames>().scorenum > ScorePrefabs[j + 1].GetComponent<setnames>().scorenum)
                {
                    var tempposition = ScorePrefabs[j].transform.position;
                    var temp = ScorePrefabs[j];
                    ScorePrefabs[j] = ScorePrefabs[j + 1];
                    ScorePrefabs[j].transform.position = ScorePrefabs[j + 1].transform.position;
                    ScorePrefabs[j + 1].transform.position = tempposition;

                    swapped = true;
                }
            }
              if (swapped == false)
                break;
        }
    }

 [PunRPC]
    public void updatescores(string name, float score)
    {
        int i = 0;
        foreach( var scores in ScorePrefabs)
        {

            if (ScorePrefabs[i].name != name)
            {
                i++;
            }
            else
            {
                ScorePrefabs[i].GetComponent<setnames>().setscore(score);
            }
        }
           view.RPC("shortlist", RpcTarget.AllBuffered,null);
    }
}


