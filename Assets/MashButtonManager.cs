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
    private float basePushAmount = 2f;
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
    private float shakeTimer;
    private Vector3 originalCanvasPosition;
    private Vector3 originalCountdownScale;
    public PhotonView photonView;

    public string player1Nickname;
    public string player2Nickname;
    public string loser;

    [Header("Debug")]
    public bool isDebugTesting = false;

    private Color localPlayerColor;
    private static readonly List<Color> playerColors = new List<Color>
    {
        Color.green, Color.red, Color.blue, Color.yellow, Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f)
    };

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        shakeObject = transform;
        battleSlider.minValue = -maxValue;
        battleSlider.maxValue = maxValue;
        battleSlider.value = 0;
        fillImage = battleSlider.fillRect.GetComponent<Image>();
        originalCanvasPosition = shakeObject.localPosition;
        originalCountdownScale = countdownText != null ? countdownText.transform.localScale : Vector3.one;

        AssignLocalColor();
        UpdateColorIndicator();

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
        if (gameStarting)
        {
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
                countdownText.transform.localScale = Vector3.Lerp(countdownText.transform.localScale, originalCountdownScale, Time.deltaTime * pulseSpeed);
            }

            if (seconds <= 0)
            {
                countdownText.text = "MASH!";
                countdownText.transform.localScale = originalCountdownScale;
                gameStarting = false;
                gameActive = true;
                lastDisplayedSecond = -1;
            }
            return;
        }

        if (!gameActive) return;

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            shakeObject.localPosition = originalCanvasPosition + new Vector3(x, y, 0f);
        }
        else shakeObject.localPosition = originalCanvasPosition;

        momentumPlayer1 = Mathf.Max(0, momentumPlayer1 - momentumDecay * Time.deltaTime);
        momentumPlayer2 = Mathf.Max(0, momentumPlayer2 - momentumDecay * Time.deltaTime);

        bool buttonPressed = false;

        if (isDebugTesting)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                momentumPlayer1 = Mathf.Min(momentumPlayer1 + momentumGain, maxMomentum);
                buttonPressed = true;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                momentumPlayer2 = Mathf.Min(momentumPlayer2 + momentumGain, maxMomentum);
                buttonPressed = true;
            }
        }
        else if (PhotonNetwork.LocalPlayer != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (PhotonNetwork.LocalPlayer.NickName == player1Nickname)
                momentumPlayer1 = Mathf.Min(momentumPlayer1 + momentumGain, maxMomentum);
            else
                momentumPlayer2 = Mathf.Min(momentumPlayer2 + momentumGain, maxMomentum);
            buttonPressed = true;
        }

        if (buttonPressed)
        {
            StartShake();
            if (!isDebugTesting)
                photonView.RPC(nameof(SyncShake), RpcTarget.Others);
        }

        float bonusPlayer1 = battleValue < 0 ? comebackMultiplier : 0f;
        float bonusPlayer2 = battleValue > 0 ? comebackMultiplier : 0f;
        float forcePlayer1 = basePushAmount + momentumPlayer1 + bonusPlayer1;
        float forcePlayer2 = basePushAmount + momentumPlayer2 + bonusPlayer2;
        float netForce = (forcePlayer1 - forcePlayer2) * (1.5f - (Mathf.Abs(battleValue) / maxValue));

        if (isDebugTesting)
            UpdateLocalBattleValue(netForce);
        else
            photonView.RPC(nameof(UpdateBattleValue), RpcTarget.All, netForce);

        battleSlider.value = battleValue;
        fillImage.color = localPlayerColor;
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
            EndBattle("Jugador Izq", "Jugador Der");
        else if (battleValue <= -maxValue)
            EndBattle("Jugador Der", "Jugador Izq");
    }

    public void StartBattle()
    {
        battleValue = 0;
        momentumPlayer1 = 0;
        momentumPlayer2 = 0;
        photonView.RPC(nameof(SyncBattleStart), RpcTarget.All, battleValue);
    }

    [PunRPC]
    void SyncBattleStart(float startValue)
    {
        battleSlider.value = startValue;
        StartCountdown();
        momentumPlayer1 = 0;
        momentumPlayer2 = 0;
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
        photonView.RPC(nameof(AnnounceWinner), RpcTarget.All, winner, loser);
    }

    [PunRPC]
    void AnnounceWinner(string winner, string loser)
    {
        gameActive = false;
        gameStarting = false;
        countdownText.transform.localScale = originalCountdownScale;
        this.loser = loser;
        transform.parent.gameObject.SetActive(false);
    }
}