using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class CarHealth : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float minImpactSpeed = 5f;
    [SerializeField] private float maxImpactSpeed = 30f;
    [SerializeField] private float maxDamage = 50f;
    [SerializeField] private float frontalBonus = 1.5f;

    [Header("Visuals")]
    [SerializeField] private Material hitMaterial;
    [SerializeField] private float hitDuration = 0.2f;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Debug")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private float lastImpactSpeed;

    private float currentHealth;
    private Rigidbody rb;
    private CarWeaponController weaponController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        weaponController = GetComponent<CarWeaponController>();
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine) 
        {
            GameManager.Instance.HUD_Controller.RefreshHealth(currentHealth);
            currentSpeed = rb.velocity.magnitude;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!photonView.IsMine) return;

        currentHealth -= amount;

        currentHealth = Mathf.Max(currentHealth, 0f);
        GameManager.Instance.HUD_Controller.RefreshHealth(currentHealth);

        photonView.RPC("RPC_FlashHit", RpcTarget.All);

        Debug.Log($"{gameObject.name} recibió {amount:F1} de daño. Vida actual: {currentHealth:F1}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    [PunRPC]
    private void RPC_FlashHit()
    {
        StartCoroutine(FlashHitMaterial());
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

        yield return new WaitForSeconds(hitDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials = originalMaterials[i];
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ha sido destruido");
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || GameManager.Instance.GameState != GameManager.GameStates.InRound) return;

        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null || otherRb == rb) return;

        float selfSpeed = rb.velocity.magnitude;
        float otherSpeed = otherRb.velocity.magnitude;

        if (selfSpeed >= otherSpeed) return;

        Vector3 relativeVelocity = otherRb.velocity - rb.velocity;
        float impactSpeed = relativeVelocity.magnitude;
        lastImpactSpeed = impactSpeed;

        if (impactSpeed < minImpactSpeed) return;

        Vector3 collisionDir = (rb.position - otherRb.position).normalized;
        float frontalFactor = Vector3.Dot(rb.velocity.normalized, collisionDir);
        frontalFactor = Mathf.Clamp01(Mathf.Abs(frontalFactor)) * frontalBonus;

        float speedFactor = Mathf.Clamp01((impactSpeed - minImpactSpeed) / (maxImpactSpeed - minImpactSpeed));
        float damage = speedFactor * maxDamage * frontalFactor;

        if (weaponController.CurrentWeapon != null)
            damage = (damage + weaponController.CurrentWeapon.DamageAmount) * weaponController.CurrentWeapon.DamageMultiplier;

        TakeDamage(damage);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHealth);
            stream.SendNext(currentSpeed);
            stream.SendNext(lastImpactSpeed);
        }
        else
        {
            currentHealth = (float)stream.ReceiveNext();
            currentSpeed = (float)stream.ReceiveNext();
            lastImpactSpeed = (float)stream.ReceiveNext();
        }
    }
}
