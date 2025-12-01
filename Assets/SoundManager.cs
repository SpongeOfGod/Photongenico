using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    public AudioSource source;
    public static SoundManager Instance;

    public AudioClip round;

   
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    // Update is called once per frame

    

    public void SetRoundMusic()
    {
        source.clip = round;
        source.Play();
    }
}
