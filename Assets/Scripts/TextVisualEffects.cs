using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextVisualEffects : MonoBehaviour
{
    public Vector3 DirectionToGo;
    public float velocity;
    public TextMeshProUGUI Text;
    public Image Image;
    public float TimeToGo = 10f;
    private float TimeSinceCheck = 0;
    private void Awake()
    {
        TimeToGo += Time.time;
        TimeSinceCheck = Time.time;
    }
    void Update()
    {
        Color ObjetiveColor = Text.color;
        ObjetiveColor.a = 0;
        Color imageObjectiveColor = Image.color;
        imageObjectiveColor.a = 0;

        var t = (Time.time - TimeSinceCheck) / TimeToGo;
        Color color = Color.Lerp(Text.color, ObjetiveColor, t);

        Image.color = Color.Lerp(Image.color, imageObjectiveColor, t);
        Text.color = color;

        transform.Translate(DirectionToGo * velocity * Time.deltaTime);

        if (Time.time - TimeSinceCheck >= TimeToGo) 
            Destroy(gameObject);
    }
}
