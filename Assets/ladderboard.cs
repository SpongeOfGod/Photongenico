using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ladderboard : MonoBehaviour
{
    public List<TextMeshProUGUI> names;
    public List<TextMeshProUGUI> score;
    public List<TextMeshProUGUI> life;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void AddplayerToLeaderboard(String Name, float health)
    {
        names[PhotonNetwork.CurrentRoom.PlayerCount - 1].text = Name;
        score[PhotonNetwork.CurrentRoom.PlayerCount - 1].text = "0";
        life[PhotonNetwork.CurrentRoom.PlayerCount -1].text = health.ToString();

    }
}
