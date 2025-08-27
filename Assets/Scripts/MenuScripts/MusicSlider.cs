using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MusicSlider : MonoBehaviour
{
    public AudioSource musicSource; // Trascina qui l'AudioSource
    public Slider slider;           // Trascina qui lo Slider

    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GameObject.FindAnyObjectByType<VideoPlayer>();

        // Imposta valore iniziale
        slider.value = musicSource.volume;

        // Collega evento
        slider.onValueChanged.AddListener(ChangeVolume);

        musicSource.volume = slider.value;
    }

    void ChangeVolume(float v)
    {
        musicSource.volume = v; // 0 = muto, 1 = max
        //videoPlayer.SetDirectAudioVolume(0, v);
    }
}
