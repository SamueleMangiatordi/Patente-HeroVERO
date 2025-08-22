using UnityEngine;
using System.Collections;

public class FadeInUI : MonoBehaviour
{
    public CanvasGroup targetGroup;  // il CanvasGroup della schermata
    public float fadeTime = 1f;      // durata del fade (in secondi)

    void Awake()
    {
        if (!targetGroup) targetGroup = GetComponent<CanvasGroup>();
    }

    public void StartFadeIn()
    {
        gameObject.SetActive(true);
        targetGroup.alpha = 0f;
        targetGroup.interactable = false;
        targetGroup.blocksRaycasts = false;
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            targetGroup.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        targetGroup.alpha = 1f;
        targetGroup.interactable = true;
        targetGroup.blocksRaycasts = true;
    }
}

