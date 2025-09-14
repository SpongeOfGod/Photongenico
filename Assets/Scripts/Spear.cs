using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spear : Weapon
{
    public int DamageAmount = 1;
    public int DamageMultiplier = 1;
}
