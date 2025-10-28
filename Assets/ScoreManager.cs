using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public static ScoreManager Instance;

    [SerializeField] private GameObject scoreprefab;
    
    public PhotonView View;
  
    public  ladderboard leaderboard;

    // Start is called before the first frame update

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }


[PunRPC]
    public void createscoreprefab(string name, float number)
    {
        GameObject newprefab = scoreprefab;

        newprefab.name = name;
        leaderboard.ScorePrefabs.Add(newprefab);

        newprefab.GetComponent<setnames>().Oninstance(name, number);
        leaderboard.Intanceincanvas();
    }

    // Update is called once per frame

}
