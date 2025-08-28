using UnityEngine;

public class SimplePause : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }
    // --- Chiama questo da un pulsante UI per mettere in pausa ---
    public void PauseGame()
    {
        gameManager.PauseGame();
    }

    // --- Chiama questo da un pulsante UI per riprendere il gioco ---
    public void ResumeGame()
    {
        gameManager.ResumeGame();
    }
}
