using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class InGameLeaderboardManager : MonoBehaviour
{
    public GameObject LeaderBoardPrefab;
    public Transform ScoreHolder;
    public GameObject VisualHolder;
    public Dictionary<string, InGameLeaderboardItem> PlayerScores = new();
    public PhotonView PhotonView;

    public void CreateNewItem(SimpleCarController carController) 
    {
        var gameObj = PhotonNetwork.Instantiate(LeaderBoardPrefab.name, Vector3.zero, Quaternion.identity);
        gameObj.transform.SetParent(ScoreHolder);
        gameObj.TryGetComponent<InGameLeaderboardItem>(out var item);
        gameObj.transform.localScale = Vector3.one;
        item.carController = carController;

        PlayerScores.Add(carController._pv.Owner.NickName, item);

        Debug.Log($"Created item for {carController._pv.Owner.NickName}");
    }

    [PunRPC]
    public void AddScore(int score, string nickname) 
    {
        PlayerScores[nickname].score += score;

        if (PhotonNetwork.LocalPlayer.NickName == nickname)
            GlobalScoreSubmitter.SubmitScore(PlayerScores[nickname].score, "global_leaderboard");
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab) && !VisualHolder.activeSelf)
            VisualHolder.SetActive(true);
        else if (!Input.GetKey(KeyCode.Tab) && VisualHolder.activeSelf)
            VisualHolder.SetActive(false);

        var keys = PlayerScores.Keys.ToArray();
        bool swap = true;

        List<InGameLeaderboardItem> items = new();

        for (int i = 0; i < keys.Length; i++)
            if (PlayerScores[keys[i]].gameObject.activeSelf)
                items.Add(PlayerScores[keys[i]]);

        if (items != null)
            while (swap)
            {
                swap = false;

                for (int i = 0; i < items.Count; i++)
                {
                    if (i + 1 < items.Count)
                        if (items[i].score < items[i + 1].score)
                        {
                            (items[i], items[i + 1]) = (items[i + 1], items[i]);
                            swap = true;
                            continue;
                        }

                    if (i - 1 >= 0)
                        if (items[i].score > items[i - 1].score)
                        {
                            (items[i], items[i - 1]) = (items[i - 1], items[i]);
                            swap = true;
                            continue;
                        }
                }

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].transform.SetSiblingIndex(i);
                }
            }
    }
}
