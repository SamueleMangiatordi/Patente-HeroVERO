using UnityEngine;
using UnityEngine.UI;

public class MusicSlider : MonoBehaviour
{
    public AudioSource musicSource; // Trascina qui l'AudioSource
    public Slider slider;           // Trascina qui lo Slider

    void Start()
    {
        // Imposta valore iniziale
        slider.value = musicSource.volume;

        // Collega evento
        slider.onValueChanged.AddListener(ChangeVolume);
    }

    void ChangeVolume(float v)
    {
        musicSource.volume = v; // 0 = muto, 1 = max
    }
}
