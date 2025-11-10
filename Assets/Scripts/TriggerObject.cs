using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObject : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool Inside;
    public float LastTimeInside;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(Inside);
        else if (stream.IsReading)
            Inside = (bool)stream.ReceiveNext();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            LastTimeInside = Time.time;
            Inside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Inside = false;
    }
}
