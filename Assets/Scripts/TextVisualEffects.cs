using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextVisualEffects : MonoBehaviour
{
    public Vector3 DirectionToGo;
    public float velocity;
    public TextMeshProUGUI Text;
    public float TimeToGo = 10f;
    private float TimeSinceCheck = 0;
    private void Awake()
    {
        Text = GetComponent<TextMeshProUGUI>();
        TimeToGo += Time.time;
        TimeSinceCheck = Time.time;
    }
    void Update()
    {
        Color ObjetiveColor = Text.color;
        ObjetiveColor.a = 0;
        Color color = Color.Lerp(Text.color, ObjetiveColor, (Time.time - TimeSinceCheck) / TimeToGo);
        Text.color = color;

        transform.Translate(DirectionToGo * velocity * Time.deltaTime);

        if (Time.time - TimeSinceCheck >= TimeToGo)
            Destroy(gameObject);
    }
}
