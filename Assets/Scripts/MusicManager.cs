using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    private static MusicManager Instance;
    private AudioSource audioSource;
    public AudioClip backgroundMusic;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if(backgroundMusic != null)
        {
            PlayBackgroundMusic(false, backgroundMusic);
        }
        
    }

    public static void PlayBackgroundMusic(bool resetSong, AudioClip audioClip = null)
    {
        if(audioClip != null)
        {
            Instance.audioSource.clip =  audioClip;

        }
        if( Instance.audioSource.clip != null)
        {
            if(resetSong)
            {
                 Instance.audioSource.Stop();

            }
             Instance.audioSource.Play();
        }
    }

    public static void PauseBackgroundMusic()
    {
         Instance.audioSource.Pause();
    }

 
}
