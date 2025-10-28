using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CarScore : MonoBehaviour
{
    private float score = 0f;

    public float Score 
	{
		get {return score; }
		set {score = value; }
	}
    // Start is called before the first frame update


   public  void calculatescore( bool killedpalyer)
    {
        if (!killedpalyer)
        {
            Score++;
        }
        else
        {
            Score += 2;
        }

        var name = this.GetComponent<PhotonView>().Owner.NickName;
        ScoreManager.Instance.photonView.RPC("updatescores",RpcTarget.All,name,Score);
    }

}
