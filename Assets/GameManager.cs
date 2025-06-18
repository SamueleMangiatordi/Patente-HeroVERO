// GameManager.cs
using UnityEngine;

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
        Time.timeScale = 0f;
        IsGamePaused = true;
        Debug.Log("Game Paused");
        // Optionally, show a pause UI or tutorial prompt
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Or your default time scale
        IsGamePaused = false;
        Debug.Log("Game Resumed");
        // Optionally, hide the pause UI or tutorial prompt
    }
}