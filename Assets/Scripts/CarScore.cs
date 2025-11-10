using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CarScore : MonoBehaviour
{
    private float score = 0f;

    private PhotonView view;

    public float Score
    {
        get { return score; }
        set { score = value; }
    }
    // Start is called before the first frame update

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    
    public  void calculatescore( string PlayerName)
    {

        Score += 2;

        var scores = ScoreManager.Instance.Scores;
      
   
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount -1; i++)
            if (scores[i].name != PlayerName)
            {
                return;
            }
            else
            {
                scores[i].GetComponent<setnames>().setscore(score);
                ScoreManager.Instance.photonView.RPC("shortlist",RpcTarget.All);
            }
        }
            
        

   
  }
    


