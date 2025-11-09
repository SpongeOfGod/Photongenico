using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

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
    private Color localColor = Color.green;
    private Color remoteColor = Color.red;

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

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        shakeObject = gameObject.transform;
        battleSlider.minValue = -maxValue;
        battleSlider.maxValue = maxValue;
        battleSlider.value = 0;
        fillImage = battleSlider.fillRect.GetComponent<Image>();

        if (shakeObject != null)
        {
            originalCanvasPosition = shakeObject.localPosition;
        }

        if (countdownText != null)
        {
            originalCountdownScale = countdownText.transform.localScale;
        }

        UpdateColorIndicator();
        if (countdownText != null)
        {
            countdownText.text = "Esperando...";
        }

        if (PhotonNetwork.LocalPlayer != null)
            StartBattle();
        else if (isDebugTesting)
            StartBattle();
    }

    void Update()
    {
        if (gameStarting)
        {
            if (countdownText == null)
            {
                gameStarting = false;
                gameActive = true;
                return;
            }

            countdownTimer -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(countdownTimer);
            Debug.Log(seconds);

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

            return;
        }

        if (!gameActive) return;

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
        else
        if (PhotonNetwork.LocalPlayer != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (PhotonNetwork.LocalPlayer.NickName == player1Nickname)
                momentumPlayer1 = Mathf.Min(momentumPlayer1 + momentumGain, maxMomentum);
            else if (PhotonNetwork.LocalPlayer.NickName == player2Nickname)
                momentumPlayer2 = Mathf.Min(momentumPlayer2 + momentumGain, maxMomentum);

            buttonPressed = true;
        }

        if (buttonPressed)
        {
            StartShake();
            if (!isDebugTesting)
            {
                photonView.RPC(nameof(SyncShake), RpcTarget.Others);
            }
        }

        float bonusPlayer1 = 0f;
        float bonusPlayer2 = 0f;

        if (battleValue > 0)
            bonusPlayer2 = comebackMultiplier;
        else if (battleValue < 0)
            bonusPlayer1 = comebackMultiplier;

        float forcePlayer1 = basePushAmount + momentumPlayer1 + bonusPlayer1;
        float forcePlayer2 = basePushAmount + momentumPlayer2 + bonusPlayer2;
        float netForce = forcePlayer1 - forcePlayer2;

        float distanceFromCenter = Mathf.Abs(battleValue);
        float centerMultiplier = 1.5f - (distanceFromCenter / maxValue);
        netForce *= centerMultiplier;

        if (isDebugTesting)
            UpdateLocalBattleValue(netForce);
        else
            photonView.RPC(nameof(UpdateBattleValue), RpcTarget.All, netForce);

        battleSlider.value = battleValue;

        if (isDebugTesting)
            fillImage.color = localColor;
        else
            fillImage.color = remoteColor;
    }

    void UpdateColorIndicator()
    {
        if (colorIndicatorText == null) return;

        if (isDebugTesting)
        {
            colorIndicatorText.text = $"Eres: <color=#{ColorUtility.ToHtmlStringRGB(localColor)}>VERDE</color>";
        }
        else if (PhotonNetwork.LocalPlayer != null)
        {
            Color playerColor = PhotonNetwork.LocalPlayer.NickName == player1Nickname ? localColor : remoteColor;
            string colorName = PhotonNetwork.LocalPlayer.NickName == player1Nickname ? "VERDE" : "ROJO";
            colorIndicatorText.text = $"Eres: <color=#{ColorUtility.ToHtmlStringRGB(playerColor)}>{colorName}</color>";
        }
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

    void StartShake()
    {
        shakeTimer = shakeDuration;
    }

    [PunRPC]
    void SyncShake()
    {
        StartShake();
    }

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
            EndBattle(isDebugTesting ? "Jugador Arriba" : $"{player1Nickname}", $"{player2Nickname}");
        else if (battleValue <= -maxValue)
            EndBattle(isDebugTesting ? "Jugador Abajo" : $"{player2Nickname}", $"{player1Nickname}");
    }

    public void StartBattle()
    {
        battleValue = 0;
        momentumPlayer1 = 0;
        momentumPlayer2 = 0;

        if (countdownText != null)
        {
            originalCountdownScale = countdownText.transform.localScale;
        }

        if (shakeObject != null)
        {
            originalCanvasPosition = shakeObject.localPosition;
        }

        if (countdownText != null)
        {
            originalCountdownScale = countdownText.transform.localScale;
        }

        if (!isDebugTesting)
            photonView.RPC(nameof(SyncBattleStart), RpcTarget.All, battleValue);
        else
            StartCountdown();
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
        {
            countdownText.transform.localScale = originalCountdownScale;
        }
        Debug.Log("Ganó " + winner);
        this.loser = loser;
        transform.parent.gameObject.SetActive(false);

    }
}