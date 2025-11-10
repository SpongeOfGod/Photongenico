using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class setnames : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playername;

    public string playname;
    [SerializeField] private TextMeshProUGUI position;

    [SerializeField] private TextMeshProUGUI score;

    public float scorenum;

    // Start is called before the first frame update

    public void Oninstance(string name, float number)
    {
        var temp = 1;
        playername.text = name;

        Debug.Log(playername.text);
        playname = name;
        score.text = number.ToString();
        scorenum = number;
        position.text = temp.ToString();

        temp++;


    }

    public void setscore(float newscore)
    {
        scorenum = newscore;
        score.text = scorenum.ToString();
    }



}
