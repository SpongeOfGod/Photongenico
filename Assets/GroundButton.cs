using UnityEngine;
using Photon.Pun;
public class GroundButton : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum PressMode {notPressed, pressed }
    public PressMode pressMode;
    public float initialY = 0.2f;
    public float endY = 0.02f;
    public float pressSpeed = 0.5f;
    private float timeSinceLastPressed;
    private float timerButtonOff = 1f; 
    private bool press = false;
    public bool debug;
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


                if (timeSinceLastPressed < Time.time - timerButtonOff && press == false)
                    pressMode = PressMode.notPressed;
                break;
        }

        if (Input.GetKeyDown(KeyCode.Space) && debug) 
        {
            pressMode = PressMode.pressed;
            timeSinceLastPressed = Time.time;
            press = true;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            pressMode = PressMode.pressed;
            press = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            timeSinceLastPressed = Time.time;
            press = false;
        }
    }
}
