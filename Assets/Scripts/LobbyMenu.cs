using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    public enum MenuState { Nickname, ServerInfo, Leaderboard }
    [HideInInspector] public MenuState state;

    [Header("Windows")]
    public Transform ServerInfoWindow;
    public Transform NicknameWindow;

    [Header("TMPro")]
    public TextMeshProUGUI LobbyStatus;
    public TextMeshProUGUI PlayingAsName;
    public TextMeshProUGUI MaxPlayersText;
    public TextMeshProUGUI TextLoadingName;
    public TMP_InputField CreateLobbyField;
    public TMP_InputField JoinLobbyField;
    public TMP_InputField PlayerName;

    [Header("Buttons")]
    public Button buttonSustract;
    public Button buttonAddup;
    public Button StartButton;
    public Button buttonJoinLobby;
    public Button CreateButton;
    public Button GoBack;
    public Button GoBackFromLeaderboard;
    public Button GoToLeaderboard;

    [Header("LevelSelect")]
    public Button ChangeLevelUp;
    public Button ChangeLevelDown;
    public int IndexNames;
    public List<string> PossibleLevels;
    public TextMeshProUGUI LevelSelectedUI;
    public string LevelSelected;

    [Header("LoadingScreen")]
    public GameObject LoadingScreenObject;

    [Header("Error Handler")]
    public ErrorHandler ErrorHandler;

    [Header("Lobby Manager")]
    public LobbyManager LobbyManager;

    [Header("Lootlocker")]
    NameRegister nameRegister;

    [Header("Leaderboard")]
    public Transform Holder;
    public GlobalLeaderboardController LeaderboardController;

    private bool loadingName = false;
    void Start()
    {
        nameRegister = new NameRegister();
        LevelSelectedUI.text = PossibleLevels[IndexNames];
        CreateLobbyField.onValueChanged.AddListener(CheckCreateLobby);
        JoinLobbyField.onValueChanged.AddListener(CheckLoadLobby);
        PlayerName.onValueChanged.AddListener(CheckName);
        StartButton.onClick.AddListener(() => 
        {
            if (!loadingName) 
            {
                loadingName = true;
                LoadingString("Registrando nombre...");
                nameRegister.SetLeaderboardName(LobbyManager.Nickname);
                StartCoroutine(LoadingName());
            }
        } );
        buttonJoinLobby.onClick.AddListener(JoinLobby);
        CreateButton.onClick.AddListener(CreateLobby);
        GoBack.onClick.AddListener(() => SwitchState(MenuState.Nickname));
        GoBackFromLeaderboard.onClick.AddListener(() => SwitchState(MenuState.ServerInfo));
        GoToLeaderboard.onClick.AddListener(() => SwitchState(MenuState.Leaderboard));
    }
    public void ResetMenu()
    {
        CreateButton.gameObject.SetActive(true);
        CreateLobbyField.gameObject.SetActive(true);
        PlayerName.gameObject.SetActive(true);
    }

    IEnumerator LoadingName() 
    {
        while (!NameRegister.NameRegistered)
            yield return null;

        SessionLootLocker.instance.Initialize();

        while (!SessionLootLocker.SessionInitialized)
            yield return null;

        SwitchState(MenuState.ServerInfo);

    }

    public void LoadingScreen() 
    {
        LoadingScreenObject.SetActive(true);
    }

    public void TurnOffMenu()
    {
        CreateButton.gameObject.SetActive(false);
        CreateLobbyField.gameObject.SetActive(false);
        PlayerName.gameObject.SetActive(false);
    }
    public void JoinLobby()
    {
        if (!string.IsNullOrEmpty(JoinLobbyField.text))
        {
            TurnOffMenu();
            LobbyManager.LobbyName = JoinLobbyField.text;
            LobbyManager.ConnectToLobby(false);
        }
    }
    public void CreateLobby()
    {
        if (!string.IsNullOrEmpty(CreateLobbyField.text))
        {
            TurnOffMenu();
            LobbyManager.LobbyName = CreateLobbyField.text;
            LobbyManager.ConnectToLobby(true);
        }
    }

    public void ChangeTextName(string lobbyName)
    {
        JoinLobbyField.text = lobbyName;
    }

    private void CheckCreateLobby(string name)
    {
        if (!string.IsNullOrEmpty(CreateLobbyField.text))
            CreateButton.gameObject.SetActive(true);
        else
        {
            string message = "Room name cannot be empty";
            ErrorHandler.HandleErrorMessage(message + $"\n error code: {0}");
        }
    }

    private void CheckLoadLobby(string name)
    {
        if (!string.IsNullOrEmpty(JoinLobbyField.text))
            buttonJoinLobby.gameObject.SetActive(true);
        else
            buttonJoinLobby.gameObject.SetActive(false);
    }

    public void TurnOnButton() 
    {
        if (!string.IsNullOrEmpty(PlayerName.text))
            StartButton.gameObject.SetActive(true);
    }

    private void CheckName(string name)
    {
        LobbyStatus.text = LobbyManager.isConnected ? "" : "Conectando...";

        if (!string.IsNullOrEmpty(PlayerName.text))
        {
            StartButton.gameObject.SetActive(LobbyManager.isConnected);
            LobbyManager.Nickname = PlayerName.text;
            PlayingAsName.text = name;
        }
        else
        {
            StartButton.gameObject.SetActive(false);
        }
    }

    public void LoadingString(string nameText) 
    {
        TextLoadingName.text = nameText;
    }

    public void SwitchState(MenuState newState)
    {
        state = newState;

        switch (state)
        {
            case MenuState.Nickname:
                loadingName = false;
                ServerInfoWindow.gameObject.SetActive(false);
                Holder.gameObject.SetActive(false);
                NicknameWindow.gameObject.SetActive(true);
                break;

            case MenuState.ServerInfo:
                NicknameWindow.gameObject.SetActive(false);
                Holder.gameObject.SetActive(false);
                ServerInfoWindow.gameObject.SetActive(true);
                break;

            case MenuState.Leaderboard:
                LeaderboardController.Refresh();
                NicknameWindow.gameObject.SetActive(false);
                ServerInfoWindow.gameObject.SetActive(false);
                Holder.gameObject.SetActive(true);
                break;
        }
    }
    public void ChangSelectedLevel(int value)
    {
        if (value > 0 && IndexNames + value < PossibleLevels.Count) 
            IndexNames += value;
        else if (value < 0 && IndexNames - value >= 0)
            IndexNames += value;

        ChangeLevelUp.gameObject.SetActive(IndexNames < PossibleLevels.Count - 1);
        ChangeLevelDown.gameObject.SetActive(IndexNames > 0);

        LevelSelectedUI.text = PossibleLevels[IndexNames];
        LevelSelected = PossibleLevels[IndexNames];
    }

    public void ChangeMaxPlayersValue(int value)
    {
        if (LobbyManager.MaxNumberOfPlayers + value >= 2 && LobbyManager.MaxNumberOfPlayers + value <= 4)
            LobbyManager.MaxNumberOfPlayers += value;

        buttonSustract.gameObject.SetActive(LobbyManager.MaxNumberOfPlayers > 2);
        buttonAddup.gameObject.SetActive(LobbyManager.MaxNumberOfPlayers < 4);

        MaxPlayersText.text = LobbyManager.MaxNumberOfPlayers.ToString();
    }
}
