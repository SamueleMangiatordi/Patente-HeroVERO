// GameManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int _pauseCount = 0;

    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource buttonClickAudioSource;
    [SerializeField] private AudioSource endLevelAudioSource;
    [SerializeField] private AudioSource checkpointReachedSound;


    [SerializeField] private GameObject endLevelPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        buttonClickAudioSource = buttonClickAudioSource ?? GameObject.Find("audio e video").transform.Find("ClickButtonSounds").GetComponent<AudioSource>();
        backgroundMusic = backgroundMusic ?? GameObject.Find("audio e video").transform.Find("MusicaSottofondoLivello").GetComponent<AudioSource>();
        endLevelAudioSource = endLevelAudioSource ?? GameObject.Find("audio e video").transform.Find("EndLevelSound").GetComponent<AudioSource>();
        checkpointReachedSound = checkpointReachedSound ?? GameObject.Find("audio e video").transform.Find("CheckpointReachedSound").GetComponent<AudioSource>();


        // Cerca tutti i Button nella scena, inclusi quelli inattivi.
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button button in allButtons)
        {
            // Aggiungi il listener per i pulsanti specifici
            // Sostituisci questo con la logica corretta per ogni pulsante
            button.onClick.AddListener(() => { buttonClickAudioSource.Play(); });
        }
    }

    public void PauseGame()
    {
        if (_pauseCount == 0)
        {
            Time.timeScale = 0f;
        }
        _pauseCount++;
    }

    public void ResumeGame()
    {
        if (_pauseCount > 0)
        {
            _pauseCount--;
            if (_pauseCount == 0)
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void EndLevel()
    {
        if(endLevelPanel == null)
        {
            Debug.LogWarning("End Level Panel is not assigned in the GameManager.");
            return;
        }

        PauseGame();
        endLevelPanel.SetActive(true);
        
        endLevelAudioSource.Play();
        StartCoroutine(WaitToRaiseBgMusicVolume(backgroundMusic.volume * 0.2f, 2f, 5f));
    }

    public IEnumerator WaitToPause(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PauseGame();
    }

    public IEnumerator WaitToResume(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        ResumeGame();
    }

    public IEnumerator WaitToRaiseBgMusicVolume(float targetVolume, float delaySeconds, float duration)
    {
        float startVolume = backgroundMusic.volume;
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            backgroundMusic.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale changes
            yield return null;
        }
        backgroundMusic.volume = targetVolume; // Ensure it ends exactly at target volume

        yield return new WaitForSecondsRealtime(delaySeconds);
        
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            backgroundMusic.volume = Mathf.Lerp(targetVolume, startVolume, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to ignore time scale changes
            yield return null;
        }
        backgroundMusic.volume = startVolume; // Ensure it ends exactly at target volume

    }

}