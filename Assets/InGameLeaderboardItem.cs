using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameLeaderboardItem : MonoBehaviourPunCallbacks, IPunObservable
{
    public int score = 0;
    public SimpleCarController carController;
    public CarHealth health;

    public TextMeshProUGUI NameTMPro;
    public TextMeshProUGUI LifeTMPro;
    public TextMeshProUGUI ScoreTMPro;

    private void Update()
    {
        if (carController != null)
        {
            if (health == null)
            {
                health = carController.gameObject.GetComponent<CarHealth>();
            }
            NameTMPro.text = carController._pv.Owner.NickName;
            LifeTMPro.text = health.CurrentHealth.ToString();
            ScoreTMPro.text = score.ToString();
        }
        else
            gameObject.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(NameTMPro.text);
            stream.SendNext(LifeTMPro.text);
            stream.SendNext(ScoreTMPro.text);
            stream.SendNext(score);
            stream.SendNext(transform.GetSiblingIndex());
        }
        else if (stream.IsReading) 
        {
            NameTMPro.text = (string)stream.ReceiveNext();
            LifeTMPro.text = (string)stream.ReceiveNext();
            ScoreTMPro.text = (string)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
            transform.SetSiblingIndex((int)stream.ReceiveNext());
        }
    }
}
