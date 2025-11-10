using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MashButtonManager : MonoBehaviour
{
    [Header("References")]
    public Slider battleSlider;
    public TextMeshProUGUI colorIndicatorText;
    public TextMeshProUGUI countdownText;

    private float maxValue = 800f;
    private float basePushAmount = 15f;
    private float momentumGain = 0.2f;
    private float momentumDecay = 1f;
    private float maxMomentum = 10f;
    private float comebackMultiplier = 0.5f;

    private Transform shakeObject;
    private float shakeDuration = 0.1f;
    private float shakeMagnitude = 5f;
    private float countdownDuration = 3f;
    private float pulseScale = 1.5f;
    private float pulseSpeed = 10f;

    private float battleValue = 0f;
    private bool gameActive = false;
    private bool gameStarting = false;
    private float countdownTimer;
    private int lastDisplayedSecond = -1;
    private float momentumPlayer1 = 0f;
    private float momentumPlayer2 = 0f;
    private Image fillImage;
    private Image backgroundImage;
    private float shakeTimer;
    private Vector3 originalCanvasPosition;
    private Vector3 originalCountdownScale;

    public PhotonView photonView;

    [Header("Player Info")]
    public string player1Nickname;
    public string player2Nickname;
    public string loser;

    [Header("Debug")]
    public bool isDebugTesting = false;

    private Color localPlayerColor;
    private static readonly List<Color> playerColors = new List<Color>
  {
    Color.green, Color.red, Color.blue, Color.yellow,
    Color.magenta, Color.cyan, new Color(1f,0.5f,0f), new Color(0.5f,0f,1f)
    };

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        shakeObject = transform;

        if (battleSlider != null)
        {
            battleSlider.minValue = -maxValue;
            battleSlider.maxValue = maxValue;
            battleSlider.value = 0;
            fillImage = battleSlider.fillRect.GetComponent<Image>();

            Transform backgroundTransform = battleSlider.transform.Find("Background");
            if (backgroundTransform != null)
            {
                backgroundImage = backgroundTransform.GetComponent<Image>();
            }
        }

        originalCanvasPosition = shakeObject != null ? shakeObject.localPosition : Vector3.zero;
        originalCountdownScale = countdownText != null ? countdownText.transform.localScale : Vector3.one;

        AssignLocalColor();
        UpdateColorIndicator();
        fillImage.color = localPlayerColor;

        if (countdownText != null)
            countdownText.text = "Esperando...";

        if (PhotonNetwork.LocalPlayer != null || isDebugTesting)
            StartBattle();
    }

    void AssignLocalColor()
    {
        if (isDebugTesting)
        {
            localPlayerColor = Color.green;
            return;
        }
        int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % playerColors.Count;
        localPlayerColor = playerColors[index];
    }

    void Update()
    {
        HandleCountdown();

        if (!gameActive) return;

        HandleShake();
        HandleMomentumAndInput();
        HandleBattleValue();
    }

    void HandleCountdown()
    {
        if (!gameStarting) return;
        if (countdownText == null)
        {
            gameStarting = false;
            gameActive = true;
            return;
        }

        countdownTimer -= Time.deltaTime;
        int seconds = Mathf.CeilToInt(countdownTimer);

        if (seconds != lastDisplayedSecond)
        {
            if (seconds > 0)
            {
                countdownText.text = seconds.ToString();
                countdownText.transform.localScale = originalCountdownScale * pulseScale;
            }
            lastDisplayedSecond = seconds;
        }

        if (countdownText.transform.localScale.x > originalCountdownScale.x)
        {
            countdownText.transform.localScale = Vector3.Lerp(
              countdownText.transform.localScale,
              originalCountdownScale,
              Time.deltaTime * pulseSpeed
            );
        }

        if (seconds <= 0)
        {
            countdownText.text = "¡MASH!";
            countdownText.transform.localScale = originalCountdownScale;
            gameStarting = false;
            gameActive = true;
            lastDisplayedSecond = -1;
        }
    }

    void HandleShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeObject != null)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                shakeObject.localPosition = originalCanvasPosition + new Vector3(x, y, 0f);
            }
        }
        else if (shakeObject != null && shakeObject.localPosition != originalCanvasPosition)
        {
            shakeObject.localPosition = originalCanvasPosition;
        }
    }

    void HandleMomentumAndInput()
    {
        momentumPlayer1 = Mathf.Max(0, momentumPlayer1 - momentumDecay * Time.deltaTime);
        momentumPlayer2 = Mathf.Max(0, momentumPlayer2 - momentumDecay * Time.deltaTime);

        float pushDelta = 0f;
        bool buttonPressed = false;

        if (isDebugTesting)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                momentumPlayer1 = Mathf.Min(momentumPlayer1 + momentumGain, maxMomentum);
                pushDelta = basePushAmount + momentumPlayer1;
                buttonPressed = true;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                momentumPlayer2 = Mathf.Min(momentumPlayer2 + momentumGain, maxMomentum);
                pushDelta = -(basePushAmount + momentumPlayer2);
                buttonPressed = true;
            }
        }
        else if (PhotonNetwork.LocalPlayer != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (PhotonNetwork.LocalPlayer.NickName == player1Nickname)
            {
                momentumPlayer1 = Mathf.Min(momentumPlayer1 + momentumGain, maxMomentum);
                pushDelta = basePushAmount + momentumPlayer1;
            }
            else if (PhotonNetwork.LocalPlayer.NickName == player2Nickname)
            {
                momentumPlayer2 = Mathf.Min(momentumPlayer2 + momentumGain, maxMomentum);
                pushDelta = -(basePushAmount + momentumPlayer2);
            }

            if (pushDelta != 0)
            {
                buttonPressed = true;
            }
        }

        if (buttonPressed)
        {
            StartShake();
            if (!isDebugTesting)
            {
                photonView.RPC(nameof(SyncPush), RpcTarget.All, pushDelta);
                photonView.RPC(nameof(SyncShake), RpcTarget.Others);
            }
        }
    }

    [PunRPC]
    void SyncPush(float pushDelta)
    {
        UpdateLocalBattleValue(pushDelta);
    }
    void HandleBattleValue()
    {
        if (battleValue != 0)
        {
            float autoDecayForce = -Mathf.Sign(battleValue) * Time.deltaTime * (momentumDecay * 0.5f);

            if (isDebugTesting)
                UpdateLocalBattleValue(autoDecayForce);
            else
                photonView.RPC(nameof(UpdateBattleValue), RpcTarget.All, autoDecayForce);
        }

        if (battleSlider != null)
            battleSlider.value = battleValue;
    }

    void UpdateColorIndicator()
    {
        if (colorIndicatorText == null) return;

        string colorName = GetColorName(localPlayerColor);
        colorIndicatorText.text = $"Eres: <color=#{ColorUtility.ToHtmlStringRGB(localPlayerColor)}>{colorName}</color>!";
    }

    string GetColorName(Color color)
    {
        if (color == Color.green) return "Verde";
        if (color == Color.red) return "Rojo";
        if (color == Color.blue) return "Azul";
        if (color == Color.yellow) return "Amarillo";
        if (color == Color.magenta) return "Magenta";
        if (color == Color.cyan) return "Cian";
        if (color == new Color(1f, 0.5f, 0f)) return "Naranja";
        if (color == new Color(0.5f, 0f, 1f)) return "Violeta";
        return "Desconocido";
    }

    void StartCountdown()
    {
        if (countdownText == null) return;
        gameActive = false;
        gameStarting = true;
        countdownTimer = countdownDuration;
        lastDisplayedSecond = -1;
        countdownText.transform.localScale = originalCountdownScale;
    }

    void StartShake() => shakeTimer = shakeDuration;

    [PunRPC] void SyncShake() => StartShake();

    void UpdateLocalBattleValue(float delta)
    {
        battleValue = Mathf.Clamp(battleValue + delta, -maxValue, maxValue);
        CheckVictory();
    }

    [PunRPC]
    void UpdateBattleValue(float delta)
    {
        battleValue = Mathf.Clamp(battleValue + delta, -maxValue, maxValue);
        CheckVictory();
    }

    void CheckVictory()
    {
        if (battleValue >= maxValue)
            EndBattle(player1Nickname, player2Nickname);
        else if (battleValue <= -maxValue)
            EndBattle(player2Nickname, player1Nickname);
    }

    public void StartBattle()
    {
        battleValue = 0;
        momentumPlayer1 = 0;
        momentumPlayer2 = 0;

        if (countdownText != null)
            originalCountdownScale = countdownText.transform.localScale;
        if (shakeObject != null)
            originalCanvasPosition = shakeObject.localPosition;

        if (!isDebugTesting)
            photonView.RPC(nameof(SyncBattleStart), RpcTarget.All, battleValue);
        else
            StartCountdown();
    }

    [PunRPC]
    void SyncBattleStart(float startValue)
    {
        if (battleSlider != null)
            battleSlider.value = startValue;
        StartCountdown();
        momentumPlayer1 = 0;
        momentumPlayer2 = 0;
    }

    [PunRPC]
    void SyncMashFightRolesAndStart(string player1, string player2)
    {
        player1Nickname = player1;
        player2Nickname = player2;

        if (transform.parent != null)
            transform.parent.gameObject.SetActive(true);

        Color player1Color = GetPlayerColorByNickname(player1Nickname);
        Color player2Color = GetPlayerColorByNickname(player2Nickname);

        if (fillImage != null && backgroundImage != null)
        {
            fillImage.color = player1Color;

            backgroundImage.color = player2Color;
        }

        localPlayerColor = GetPlayerColorByNickname(PhotonNetwork.LocalPlayer.NickName);
        UpdateColorIndicator();

        StartBattle();
    }

    Color GetPlayerColorByNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname) || PhotonNetwork.PlayerList == null)
        {
            return Color.gray;
        }

        Photon.Realtime.Player player = System.Linq.Enumerable.FirstOrDefault(PhotonNetwork.PlayerList, p => p.NickName == nickname);

        if (player != null)
        {
            int index = (player.ActorNumber - 1) % playerColors.Count;
            return playerColors[index];
        }

        return Color.gray;
    }

    void EndBattle(string winner, string loser)
    {
        gameActive = false;
        gameStarting = false;

        if (countdownText != null)
        {
            countdownText.text = "FIN";
            countdownText.transform.localScale = originalCountdownScale;
        }

        Debug.Log("Ganó " + winner);

        if (!isDebugTesting)
            photonView.RPC(nameof(AnnounceWinner), RpcTarget.All, winner, loser);
        else
            AnnounceWinner(winner, loser);
    }

    [PunRPC]
    void AnnounceWinner(string winner, string loser)
    {
        gameActive = false;
        gameStarting = false;

        if (countdownText != null)
            countdownText.transform.localScale = originalCountdownScale;

        Debug.Log("Ganó " + winner);
        this.loser = loser;

        if (transform.parent != null)
            transform.parent.gameObject.SetActive(false);
    }
}