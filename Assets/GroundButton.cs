using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine.UIElements;
public class GroundButton : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum PressMode {notPressed, pressed }
    public PressMode pressMode;
    public float initialY = 0.2f;
    public float endY = 0.02f;
    public float pressSpeed = 0.5f;
    private float timeSinceLastPressed;
    private float timerButtonOff = 1f; 
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(transform.position);
        else if (stream.IsReading)
            transform.position = (Vector3)stream.ReceiveNext();
    }

    void Update()
    {
        Vector3 newPos = transform.position;
        switch (pressMode) 
        {
            case PressMode.notPressed:
                newPos.y = initialY;
                transform.position = Vector3.Lerp(transform.position, newPos, pressSpeed);
                break;

            case PressMode.pressed:
                newPos.y = endY;
                transform.position = Vector3.Lerp(transform.position, newPos, pressSpeed);


                if (timeSinceLastPressed < Time.time - timerButtonOff)
                    pressMode = PressMode.notPressed;
                break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            timeSinceLastPressed = Time.time;
            pressMode = PressMode.pressed;
        }
    }
}
