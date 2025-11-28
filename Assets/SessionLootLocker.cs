using UnityEngine;
using LootLocker.Requests;
using System;
using System.Collections;

public class SessionLootLocker : MonoBehaviour
{
    public static SessionLootLocker instance;
    public static bool SessionInitialized { get; private set; }
    [SerializeField] public static string identifier;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this) 
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(StartGuestSession());
    }
    IEnumerator StartGuestSession() 
    {
        bool done = false;

        LootLockerSDKManager.StartGuestSession(identifier,
            response =>
            {
                if (!response.success)
                {
                    Debug.LogError("There was a problem while connecting with the API.");
                    done = true;
                }
                else 
                {
                    SessionInitialized = true;
                    Debug.Log($"Session Initiated! - Identifier: {identifier}");
                    done = true;
                }
            }
            );

        yield return new WaitWhile(() => done == false);
    }
}
