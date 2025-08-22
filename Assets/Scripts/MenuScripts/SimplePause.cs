using UnityEngine;

public class SimplePause : MonoBehaviour
{
    // --- Chiama questo da un pulsante UI per mettere in pausa ---
    public void PauseGame()
    {
        Time.timeScale = 0f;   // ferma il gioco
    }

    // --- Chiama questo da un pulsante UI per riprendere il gioco ---
    public void ResumeGame()
    {
        Time.timeScale = 1f;   // riprende il gioco
    }
}
