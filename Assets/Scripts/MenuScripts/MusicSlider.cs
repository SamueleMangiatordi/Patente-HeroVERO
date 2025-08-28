using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MusicSlider : MonoBehaviour
{
    public MusicType musicType; // Tipo di musica da controllare
    public AudioSource musicSource; // Trascina qui l'AudioSource
    public Slider slider;           // Trascina qui lo Slider

    private VideoPlayer videoPlayer;
    private List<AudioSource> audioSources = new List<AudioSource>();

    void Start()
    {
        videoPlayer = GameObject.FindAnyObjectByType<VideoPlayer>();

        // Imposta valore iniziale
        slider.value = musicSource.volume;

        // Collega evento
        slider.onValueChanged.AddListener(ChangeVolume);

        musicSource.volume = slider.value;

        GameObject[] sources;

        if(musicType == MusicType.SoundEffect)
        {
            sources = GameObject.FindGameObjectsWithTag("soundEffect");
        }
        else {  
            sources = GameObject.FindGameObjectsWithTag("soundBackgroundMusic");
        }

        foreach (GameObject source in sources)
            {

                audioSources.Add(source.GetComponent<AudioSource>());

            }
    }

    void ChangeVolume(float v)
    {
        musicSource.volume = v; // 0 = muto, 1 = max

        foreach (AudioSource source in audioSources)
        {
            source.volume = v;
        }
        //videoPlayer.SetDirectAudioVolume(0, v);
    }
}

public enum MusicType
{
    SoundEffect,
    BackgroundMusic
}
