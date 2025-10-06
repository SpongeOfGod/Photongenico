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
    public List<GameObject> scorelist;

    [SerializeField] GameObject scorepreefab;
 
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void AddplayerToLeaderboard(String Name, float score)
    {
        var newscore = Instantiate(scorepreefab);
        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name.ToString();
        transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.ToString();

        scorelist.Add(scorepreefab);

        transform.Find("Position").GetComponent<TextMeshProUGUI>().text = scorelist.Count.ToString();


    }
}
