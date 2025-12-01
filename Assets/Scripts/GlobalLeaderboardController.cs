using LootLocker.Requests;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GlobalLeaderboardController : MonoBehaviour
{
    [SerializeField] string key = "global_leaderboard";
    [SerializeField] int count = 10;
    [SerializeField] GameObject LeaderboardPrefab;
    [SerializeField] Transform Holder;
    Dictionary<GameObject, (TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI)> PlayersLeaderboard = new();

    private void Awake()
    {
        for (int i = 0; i < count; i++)
        {
            var instancedObject = Instantiate(LeaderboardPrefab, Holder);

            instancedObject.transform.GetChild(0).gameObject.TryGetComponent<TextMeshProUGUI>(out var Rank);
            instancedObject.transform.GetChild(1).gameObject.TryGetComponent<TextMeshProUGUI>(out var Name);
            instancedObject.transform.GetChild(2).gameObject.TryGetComponent<TextMeshProUGUI>(out var Score);

            instancedObject.gameObject.SetActive(false);
            PlayersLeaderboard.Add(instancedObject, (Rank, Name, Score));
        }
    }
    public void Refresh() 
    {
        if (!SessionLootLocker.SessionInitialized) 
        {
            return;
        }

        LootLockerSDKManager.GetScoreList(key, count, 0, response =>
        {
            if (response.success)
            {
                var items = response.items;

                for (int i = 0; i < items.Length; i++)
                {
                    if (i + 1 < items.Length && items[i + 1].score > items[i].score)
                        (items[i + 1], items[i]) = (items[i], items[i + 1]);

                    if (i - 1 >= 0 && items[i -1].score < items[i].score)
                        (items[i - 1], items[i]) = (items[i], items[i - 1]);
                }

                var keys = PlayersLeaderboard.Keys.ToList();

                for (int i = 0; i < keys.Count; i++)
                {
                    if (i < items.Length)
                    {
                        keys[i].gameObject.SetActive(true);
                        PlayersLeaderboard[keys[i]].Item1.text = $"# {i + 1}";
                        PlayersLeaderboard[keys[i]].Item2.text = $"{items[i].player.name}";
                        PlayersLeaderboard[keys[i]].Item3.text = $"{items[i].score}";
                    }
                    else 
                    {
                        keys[i].gameObject.SetActive(false);
                    }
                }
            }
        });
    }
}
