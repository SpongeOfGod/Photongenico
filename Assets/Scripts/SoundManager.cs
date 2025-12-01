using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    public AudioSource Source;
    public static SoundManager Instance;

    public AudioClip Round; 
    public AudioClip StartUp;
   
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
        Source.Stop();
        Source.clip = Round;
        Source.Play();
    }


    public void SetStartmusic()
    {
        Source.Stop();
        Source.clip = StartUp;
        Source.Play();
    }
}
