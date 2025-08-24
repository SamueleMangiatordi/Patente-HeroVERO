using System.Collections;
using UnityEngine;

public class CameraFader : MonoBehaviour
{
    // Il CanvasGroup che conterrà l'overlay nero
    [Tooltip("Il CanvasGroup dell'overlay di dissolvenza.")]
    public CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        if (fadeCanvasGroup != null)
        {
            // Assicura che l'alpha sia 0 all'inizio
            fadeCanvasGroup.alpha = 0f;
            // Assicura che l'overlay non blocchi i raycast
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    // Coroutine helper per la dissolvenza a nero
    public IEnumerator FadeToBlack(float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("Fade Canvas Group non assegnato, impossibile eseguire il fade a nero.");
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    // Coroutine helper per la dissolvenza da nero
    public IEnumerator FadeFromBlack(float duration)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("Fade Canvas Group non assegnato, impossibile eseguire il fade da nero.");
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
    }
}