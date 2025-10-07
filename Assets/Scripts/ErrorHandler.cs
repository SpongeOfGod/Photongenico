using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorHandler : MonoBehaviourPunCallbacks
{
    public static ErrorHandler Instance;
    public LobbyMenu LobbyMenu;
    public Transform ErrorTextHolder;
    public TextMeshProUGUI PrefabText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }
    public void HandleErrorMessage(string message)
    {
        var prefab = Instantiate(PrefabText, ErrorTextHolder);
        prefab.text = message;
        prefab.gameObject.SetActive(true);
        LobbyMenu.ResetMenu();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message + $"\n error code: {returnCode}");
        HandleErrorMessage(message + $"\n error code: {returnCode}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message + $"\n error code: {returnCode}");
        HandleErrorMessage(message + $"\n error code: {returnCode}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message + $"\n error code: {returnCode}");
        HandleErrorMessage(message + $"\n error code: {returnCode}");
    }
}
