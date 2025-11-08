using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonolithController : MonoBehaviourPunCallbacks, IPunObservable
{
    public enum MonolithState { grounded, exposed }
    public MonolithState currentState;

    [Header("Visual")]
    [SerializeField] private Material hitMaterial;

    [Header("Settings")]
    [SerializeField] private List<Vector3> Positions;
    [SerializeField] private float currentHealth = 15f;
    [SerializeField] private float minImpactSpeed = 5f; 
    [SerializeField] private float maxImpactSpeed = 30f;
    [SerializeField] private float maxDamage = 5f;
    [SerializeField] private float AmountToHeal = 10f;
    [SerializeField] private float InitialY = -4f;
    [SerializeField] private float endY = 4f;
    [SerializeField] private float TimeToReappear = 16f;

    private bool MonolithAppear = false;
    private bool onCollisionWithPlayer;
    private string localPlayerName = string.Empty;
    private float damage;
    private float timeSinceDissappear;
    private GameObject playerObject;
    private Rigidbody rb; 
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!MonolithAppear && currentState == MonolithState.grounded && GameManager.Instance.GameState == GameManager.GameStates.InRound) 
        {
            float randomHeight = UnityEngine.Random.Range(1, endY);
            float randomEndTime = UnityEngine.Random.Range(0.25f, 1f);
            float randomY = UnityEngine.Random.Range(-360f, 360f);

            StartCoroutine(MonolithMove(randomHeight, randomEndTime, randomY, MonolithState.exposed));
        }
    }

    IEnumerator MonolithMove(float height, float endTime, float DesiredY, MonolithState newState) 
    {
        MonolithAppear = true;

        float elapsedTime = 0;
        float initialY = transform.position.y;
        var InitialRotation = transform.rotation;

        var rotation = new Quaternion();
        rotation.eulerAngles.Set(InitialRotation.eulerAngles.x, DesiredY, InitialRotation.eulerAngles.z);
        

        while (elapsedTime <= endTime) 
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / endTime;
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(initialY, height, t), transform.position.z);
            transform.eulerAngles = Vector3.Slerp(InitialRotation.eulerAngles, rotation.eulerAngles, t);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, height, transform.position.z);
        transform.eulerAngles = rotation.eulerAngles;
        currentState = newState;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || GameManager.Instance.GameState != GameManager.GameStates.InRound && currentState != MonolithState.exposed) return;

        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null || otherRb == rb) return;
        float selfSpeed = rb.velocity.magnitude;
        float otherSpeed = otherRb.velocity.magnitude;

        if (selfSpeed >= otherSpeed) return;

        Vector3 relativeVelocity = otherRb.velocity - rb.velocity;
        float impactSpeed = relativeVelocity.magnitude;
   
        if (impactSpeed < minImpactSpeed) return;

        Vector3 collisionDir = (rb.position - otherRb.position).normalized;
        float frontalFactor = Vector3.Dot(rb.velocity.normalized, collisionDir);
        frontalFactor = Mathf.Clamp01(Mathf.Abs(frontalFactor));

        float speedFactor = Mathf.Clamp01((impactSpeed - minImpactSpeed) / (maxImpactSpeed - minImpactSpeed));
        damage = speedFactor * maxDamage;

        if (collision.gameObject.CompareTag("Player") && damage > 0 && currentHealth >= 0)
        {
            onCollisionWithPlayer = true;
            playerObject = collision.gameObject;
            localPlayerName = PhotonNetwork.LocalPlayer.NickName;
            TakeDamage(damage);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(damage);
            stream.SendNext(currentHealth);
            stream.SendNext(onCollisionWithPlayer);
        }
        else if (stream.IsReading)
        {
            transform.position = (Vector3)stream.ReceiveNext(); 
            transform.rotation = (Quaternion)stream.ReceiveNext();
            damage = (float)stream.ReceiveNext();
            currentHealth = (float)stream.ReceiveNext();
            onCollisionWithPlayer = (bool)stream.ReceiveNext();

            if (currentHealth >= 0)
                if (onCollisionWithPlayer)
                {
                    TakeDamage(damage);
                    onCollisionWithPlayer = false;
                }
        }
    }

    [PunRPC]
    public void TakeDamage(float amount)
    {
        if (!photonView.IsMine) return;

        currentHealth -= amount;

        currentHealth = Mathf.Max(currentHealth, 0f);
        photonView.RPC("FlashHit", RpcTarget.All, null);
    }

    [PunRPC]
    private void FlashHit() => StartCoroutine(FlashHitMaterial());

    private void CheckDeath() 
    {
        if (currentHealth <= 0) 
        {
            if (PhotonNetwork.LocalPlayer.NickName == localPlayerName) 
            {
                playerObject.TryGetComponent<PhotonView>(out var photonv);

                photonv.RPC("HealPlayer", RpcTarget.All, AmountToHeal);
            }

            int rndIndex = UnityEngine.Random.Range(0, Positions.Count);
            transform.position = new Vector3(transform.position.x, InitialY, transform.position.z);
            transform.position = Positions[rndIndex];
            StartCoroutine(MonolithMove(InitialY, 0.5f, 0, MonolithState.exposed));
        }
    }

    private IEnumerator FlashHitMaterial()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        Material[][] originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].materials;
            Material[] newMats = new Material[renderers[i].materials.Length];
            for (int j = 0; j < newMats.Length; j++)
                newMats[j] = hitMaterial;
            renderers[i].materials = newMats;
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].materials = originalMaterials[i];

        CheckDeath();
    }
}
