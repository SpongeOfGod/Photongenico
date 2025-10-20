using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class setnames : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playername;
    [SerializeField] private TextMeshProUGUI position;

    [SerializeField] private TextMeshProUGUI score;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Oninstance(string name, float number)
    {
        playername.text = name;
        
        score.text = number.ToString();

        position.text = ScoreManager.Instance.leaderboard.prefabs[PhotonNetwork.CurrentRoom.PlayerCount -1].ToString();

        
        

    }
  
}
