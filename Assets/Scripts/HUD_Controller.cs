using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD_Controller : MonoBehaviour
{
    public TextMeshProUGUI HealthValue;
    public TextMeshProUGUI CurrentWeapon;

    public void RefreshHealth(float value) 
    {
        HealthValue.text = value.ToString();
    }

    public void CurrentWeaponChange(string name) 
    {
        CurrentWeapon.text = name;
    }
}
