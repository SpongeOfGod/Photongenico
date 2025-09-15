using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spear : Weapon
{
    public int DamageAmount = 1;
    public int DamageMultiplier = 1;
    public float timeToBeDestroyed = 10;

    private float InitialTime = 0;

    private void Awake()
    {
        InitialTime = Time.time;
    }
    private void Update()
    {
        if (Time.time - InitialTime >= timeToBeDestroyed)
            Destroy(gameObject);
    }
}
