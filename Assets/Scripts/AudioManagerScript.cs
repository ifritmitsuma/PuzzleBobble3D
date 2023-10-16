using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{

    public string[] names;

    public AudioClip[] clips;

    public GameObject audioMusicSource;

    public GameObject audioSFXSource;

    private AudioSource musicSource;

    private AudioSource sfxSource;

    // Start is called before the first frame update
    void Start()
    {
        musicSource = audioMusicSource.GetComponent<AudioSource>();
        sfxSource = audioSFXSource.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void playMusic(string name)
    {
        if(musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        int index = 0;
        for(int i = 0; i < names.Length; ++i)
        {
            if(names[i].Equals(name))
            {
                index = i;
                break;
            }
        }
        musicSource.clip = clips[index];
        musicSource.Play();
    }

    public void stopMusic()
    {
        musicSource.Stop();
    }

    public void playSFX(string name)
    {
        int index = 0;
        for (int i = 0; i < names.Length; ++i)
        {
            if (names[i].Equals(name))
            {
                index = i;
                break;
            }
        }
        sfxSource.clip = clips[index];
        sfxSource.Play();
    }
}
