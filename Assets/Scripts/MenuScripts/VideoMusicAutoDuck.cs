using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoMusicAutoDuck : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource musicSource;      // Audio della MUSICA di sottofondo
    public VideoPlayer videoPlayer;      // Il tuo VideoPlayer

    [Header("Settings")]
    [Range(0f, 1f)] public float duckVolume = 0f; // Volume mentre il video suona (0 = muto)
    public float fadeTime = 0.5f;                // Fade in/out secondi

    float savedVolume = 1f;
    bool isDucked = false;
    Coroutine fadeCo;

    void Awake()
    {
        if (videoPlayer != null)
        {
            videoPlayer.started += OnVideoStarted;
            videoPlayer.loopPointReached += OnVideoFinished; // chiamato a fine video
            videoPlayer.errorReceived += OnVideoError;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    void OnVideoStarted(VideoPlayer vp) => Duck();
    void OnVideoFinished(VideoPlayer vp) => Unduck();
    void OnVideoError(VideoPlayer vp, string msg) => Unduck();

    public void Duck()
    {
        if (!musicSource) return;
        if (isDucked) return;

        savedVolume = musicSource.volume;
        StartFade(duckVolume);
        isDucked = true;
    }

    public void Unduck()
    {
        if (!musicSource) return;
        if (!isDucked) return;

        StartFade(savedVolume);
        isDucked = false;
    }

    void StartFade(float target)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeTo(target));
    }

    IEnumerator FadeTo(float target)
    {
        float start = musicSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(start, target, Mathf.Clamp01(t / fadeTime));
            yield return null;
        }
        musicSource.volume = target;
    }
}
