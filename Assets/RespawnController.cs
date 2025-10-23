using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RespawnController : MonoBehaviour
{
    public GameObject HolderUI;
    public TextMeshProUGUI countDownText;
    private float startTime = 3f;
    private PhotonView PhotonView;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
    }

    public void ActivateRespawn(CarHealth carHealth)
    {
        StartCoroutine(RespawnTimer(carHealth));
    }

    IEnumerator RespawnTimer(CarHealth carHealth) 
    {
        countDownText.text = startTime.ToString();
        GameObject gameobject = carHealth.gameObject;
        HolderUI.SetActive(true);
        while (startTime >= 0) 
        {
            startTime -= Time.deltaTime;
            countDownText.text = ((int)startTime).ToString();
            yield return null;
        }

        var position = GameManager.Instance.GetRandomSpawn();
        var rotation = new Vector3(0, 0, 0);
        HolderUI.SetActive(false);
        carHealth.photonView.RPC("Reposition", RpcTarget.All, position, rotation);
        carHealth.photonView.RPC("Initialized", RpcTarget.All, null);

    }
}
