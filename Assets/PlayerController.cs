using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PhotonView photonView;
    PhotonRigidbodyView photonRigidbody;
    Rigidbody rb;
    [SerializeField] private TextMeshPro text;
    public float speed = 10f;
    [HideInInspector] public string PlayerName;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonRigidbody = GetComponent<PhotonRigidbodyView>();
        rb = GetComponent<Rigidbody>();
        text = GetComponentInChildren<TextMeshPro>();
    }

    void Update()
    {
        if (photonView.IsMine) 
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");

            Vector3 movement = new Vector3(x, 0, y);
            //transform.LookAt(transform.position + movement);

            if (movement != Vector3.zero)
                rb.AddForce(movement.normalized * speed * Time.deltaTime);
            else
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        //if (text != null)
        //    text.transform.LookAt(Camera.main.transform);
    }

    [PunRPC]
    public void ChangeUsername(string username) 
    {
        PlayerName = username;
        text = GetComponentInChildren<TextMeshPro>();
        text.text = username;

        Debug.Log($"{PlayerName} has joined.");
    }
}
