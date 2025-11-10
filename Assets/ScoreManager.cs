using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public static ScoreManager Instance;

    [SerializeField] private GameObject scoreprefab;

    public List<GameObject> Scores;
    [SerializeField] private GameObject panel;
    public PhotonView View;
  


    // Start is called before the first frame update

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

            for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
          var newPlayerscore =  Instantiate(scoreprefab, panel.transform);

            Scores.Add(newPlayerscore);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var temp = 2;
            Scores[1].GetComponent<setnames>().setscore(temp);
            shortlist();
        }
    }

    [PunRPC]
    public void createscoreprefab(string playerName, float number)
    {

        var playersinroom = PhotonNetwork.CurrentRoom.PlayerCount - 1;

        for (int i = 0; i < playersinroom; i++)
        {
            Scores[i].GetComponent<setnames>().Oninstance(playerName, number);

            Scores[i].name = playerName;
        }


    }

    [PunRPC]
    public void shortlist()
    {

        int n = Scores.Count - 1;
        bool swapped = false;
        int[] places = {0, 1, 0, 0};
        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (Scores[j].GetComponent<setnames>().scorenum > Scores[j + 1].GetComponent<setnames>().scorenum)
                {
                    places[i]++;
                 
                }
            }
            if (swapped == false)
                break;
        }
        

        for(int i = 0; i <Scores.Count; i++)
        {
         panel.transform.SetSiblingIndex(places[i]);
        }
    }



}
