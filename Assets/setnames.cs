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

    public float scorenum;

    // Start is called before the first frame update

    public void Oninstance(string name, float number)
    {
        playername.text = name;

        score.text = number.ToString();
        scorenum = number;

        position.text = ScoreManager.Instance.leaderboard.prefabs.Count.ToString();



    }



}
