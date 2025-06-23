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
            Debug.LogWarning("Game is already paused, cannot pause again.");
            return;
        }

        Time.timeScale = 0f;
        IsGamePaused = true;
        Debug.Log("Game Paused");
        // Optionally, show a pause UI or tutorial prompt
    }

    public void ResumeGame()
    {
        if(!IsGamePaused)
        {
            Debug.LogWarning("Game is not paused, cannot resume.");
            return;
        }

        Time.timeScale = 1f; // Or your default time scale
        IsGamePaused = false;
        Debug.Log("Game Resumed");
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