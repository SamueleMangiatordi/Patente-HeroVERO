using System;
using System.Collections;
using UnityEngine;

public class AutomaticDisablePanel : MonoBehaviour
{
    [SerializeField] private bool automaticDisablePanel = true;

    [SerializeField] private float panelDuration = 3f;

    [SerializeField] private PanelFader panelFader;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;

    [SerializeField] private AudioSource fadeInSound;

    public Action onPanelEnable;
    public Action onPanelDisable;

    private void OnEnable()
    {
        StartCoroutine(panelFader.FadeToBlack(fadeInDuration));
        fadeInSound?.Play();
        onPanelEnable?.Invoke();

        if (automaticDisablePanel)
            StartCoroutine(DelayDisable(panelDuration));
    }

    private IEnumerator DelayDisable(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        panelFader.onEndFadeFromBlack += () =>
        {
            gameObject.SetActive(false);
            onPanelDisable?.Invoke();
            panelFader.onEndFadeFromBlack = null; // Unsubscribe to avoid multiple calls
        };
        StartCoroutine(panelFader.FadeFromBlack(fadeOutDuration));
    }

    public void DisablePanel(float delay)
    {
        StartCoroutine(DelayDisable(panelDuration));
    }
}
