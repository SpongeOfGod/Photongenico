using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretZoneController : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum Mode { Inactive, active}
    public Mode currentMode;
    public float InitialY = -9.95765f;
    public float EndY = -21f;
    public float pressSpeed = 0.5f;
    public List<GroundButton> groundButtons = new List<GroundButton>();
    public List<GameObject> garageDoors = new();
    public List<Collider> colliders = new ();
    public bool SecretActive;
    private GameObject currentGarage;
    void Update()
    {
        switch (currentMode) 
        {
            case Mode.Inactive:

                bool allActive = true;

                foreach (var item in garageDoors)
                {
                    var newPos = item.transform.localPosition;
                    newPos.y = InitialY;
                    item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, newPos, pressSpeed);
                }

                foreach (var item in colliders)
                    item.enabled = true;

                foreach (GroundButton groundButton in groundButtons) 
                    if (groundButton.pressMode != GroundButton.PressMode.pressed)
                        allActive = false;

                if (allActive) 
                {
                    foreach (var item in colliders)
                        item.enabled = false;

                    currentMode = Mode.active;
                }
                break;


            case Mode.active:
                foreach (var item in garageDoors)
                {
                    var newPos = item.transform.localPosition;
                    newPos.y = EndY;
                    item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, newPos, pressSpeed);
                }


                break;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            foreach (var item in garageDoors)
                stream.SendNext(item.transform.localPosition);
        else if (stream.IsReading)
            foreach (var item in garageDoors)
                item.transform.localPosition = (Vector3)stream.ReceiveNext();
    }
}
