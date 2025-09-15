using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWeaponController : MonoBehaviour
{
    public Transform WeaponParent;
    public Vector3 Position;

    public Weapon CurrentWeapon;
    private void Update()
    {
        if (CurrentWeapon != null) 
        {
            if (CurrentWeapon.transform.parent != WeaponParent) 
            {
                CurrentWeapon.transform.parent = WeaponParent;
                CurrentWeapon.transform.eulerAngles = Vector3.zero;
                CurrentWeapon.transform.localEulerAngles = Vector3.zero;
                CurrentWeapon.transform.localPosition = Position;
            }
        }
    }
}
