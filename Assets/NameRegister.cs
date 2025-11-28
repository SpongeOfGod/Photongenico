using LootLocker.Requests;
using UnityEngine;

public class NameRegister
{
    public void SetLeaderboardName(string name) => 
        LootLockerSDKManager.SetPlayerName(name, 
            response =>
            {
                if (!response.success)
                    Debug.LogError("Error while trying to set name.");
                else 
                {
                    Debug.Log($"The name \"{name}\" was succesfully set!");
                    SessionLootLocker.identifier = name;
                }
            });
}
