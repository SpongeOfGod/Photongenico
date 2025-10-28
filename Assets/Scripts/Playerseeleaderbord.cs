                using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerseeleaderbord : MonoBehaviour
{
    public GameObject Leaderboard;
    private bool seeingleadeboard = false;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Tab))
        {
            seeingleadeboard = !seeingleadeboard;
        }

        if (Leaderboard != null)
            Leaderboard.SetActive(seeingleadeboard);
    }
}
