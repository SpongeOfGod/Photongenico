using LootLocker.Requests;
using UnityEngine;

public class GlobalScoreSubmitter : MonoBehaviour
{
    public static void SubmitScore(int score, string key, System.Action<bool> onDone = null) 
    {
        LootLockerSDKManager.SubmitScore(SessionLootLocker.identifier, score, key, response =>
        {
            if (response.success)
            {
                Debug.Log("Score successfully submitted");
                onDone.Invoke(true);
            }
            else
            {
                Debug.LogError("Error while trying to submit score.");
                onDone.Invoke(false);
            }
        });
    }
}
