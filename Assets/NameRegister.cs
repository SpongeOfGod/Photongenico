using LootLocker.Requests;
using UnityEngine;

public class NameRegister
{
    public static bool NameRegistered;
    public void SetLeaderboardName(string name)
    {
        NameRegistered = false;
        LootLockerSDKManager.SetPlayerName(name, 
            response =>
            {
                if (!response.success)
                    Debug.LogError("Error while trying to set name.");
                else 
                {
                    Debug.Log($"The name \"{name}\" was succesfully set!");
                    NameRegistered = true;
                    SessionLootLocker.identifier = name;
                }
            });
    } 
}
