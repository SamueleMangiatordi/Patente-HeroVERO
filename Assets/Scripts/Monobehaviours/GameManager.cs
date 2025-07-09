// GameManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGamePaused { get; private set; }

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
        if(!IsGamePaused)
        {
            return;
        }

        Time.timeScale = 1f; // Or your default time scale
        IsGamePaused = false;
        // Optionally, hide the pause UI or tutorial prompt
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