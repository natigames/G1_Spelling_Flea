using UnityEngine;
using System.Collections;




[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(AudioSource))]
class NewAudio : MonoBehaviour
{
    private AudioSource AudioSource;
    private AudioListener AudioListener;

    private static NewAudio instance = null;
    public static NewAudio Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (NewAudio)FindObjectOfType(typeof(NewAudio));
            }
            return instance;
        }
    }

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }


    private void Start()
    {
        AudioSource = FindObjectOfType<AudioSource>();
        AudioListener = FindObjectOfType<AudioListener>();
    }

    public void PlayOneShot(AudioClip AudioClip)
    {
        AudioSource.PlayOneShot(AudioClip); 
    }

    public void PlayWord(AudioClip AudioClip)
    {
        AudioSource.Stop();
        AudioSource.clip = AudioClip;
        AudioSource.loop = false;
        AudioSource.Play();
    }
      
}