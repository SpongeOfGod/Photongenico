using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public float DamageAmount = 1;
    public float DamageMultiplier = 1;
    public float timeToBeDestroyed = 10;

    protected float InitialTime = 0;
}
