// GameManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGamePaused { get; private set; }

    [SerializeField] private AudioSource buttonClickAudioSource;
    [SerializeField] private AudioSource backgroundMusic;

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
        if (IsGamePaused)
        {
            return;
        }

        Time.timeScale = 0f;
        IsGamePaused = true;
        // Optionally, show a pause UI or tutorial prompt
    }

    public void ResumeGame()
    {
        if (!IsGamePaused)
        {
            return;
        }

        Time.timeScale = 1f; // Or your default time scale
        IsGamePaused = false;
        // Optionally, hide the pause UI or tutorial prompt
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

}