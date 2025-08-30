using Ezereal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoMenuController : MonoBehaviour
{
    // Assign these in the Inspector
    [Header("Video Settings")]
    [SerializeField] private Button playVideoButton;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private GameObject objectToAppearAfterVideo;
    [SerializeField] private Button skipVideoButton;

    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private AudioSource backgroundMusic;

    [SerializeField] private EzerealCameraController cameraController;
    [SerializeField] private GameManager gameManager;

    [Header("Auto Play Settings")]
    [Tooltip("If true, the video will play automatically on start.")]
    [SerializeField] private bool autoPlayVideo = false; // If true, the video will play automatically on start


    [Header("Camera Fader Settings")]
    [SerializeField] private PanelFader cameraFader; // Reference to the CameraFader script
    [SerializeField] private float fadeToBlackDuration = 1f; // Duration of the fade effect
    [SerializeField] private float fadeFromBlackDuration = 1f; // Duration of the fade effect

    [Header("Events")]
    public UnityEvent onVideoStart; // Optional: UnityEvent to trigger when the video starts
    public UnityEvent onVideoEnd; // Optional: UnityEvent to trigger when the video ends

    private bool isPlaying = false;
    private float startBgMusic = 1f;

    private void Awake()
    {
        videoPlayer.Prepare();
    }
    void Start()
    {
        gameManager = gameManager ?? FindAnyObjectByType<GameManager>();
        backgroundMusic = backgroundMusic ?? GameObject.Find("audio e video").transform.Find("MusicaSottofondoLivello").GetComponent<AudioSource>();
        cameraFader = cameraFader ?? GameObject.Find("Canvas").transform.Find("PanelCameraFader").GetComponent<PanelFader>();
        cameraController = cameraController ?? FindAnyObjectByType<EzerealCameraController>();

        if (objectToAppearAfterVideo != null)
        {
            objectToAppearAfterVideo.SetActive(false);
        }

        // Add a listener to the button's click event
        if (playVideoButton != null)
        {
            playVideoButton.onClick.AddListener(PlayVideo);
        }
        if( skipVideoButton != null)
        {
            skipVideoButton.onClick.AddListener(SkipVideo);
        }

        pauseButton?.onClick.AddListener(PauseVideo);
        resumeButton?.onClick.AddListener(ResumeVideo);

        // Add a listener to the video player's loop point reached event.
        // This event is triggered when the video finishes.
        videoPlayer.loopPointReached += OnVideoEnd;

        if( autoPlayVideo )
        {
            PlayVideo();
        }
    }

    public void PauseVideo()
    {
        if(!isPlaying ) return;

        videoPlayer.Pause();
        skipVideoButton.interactable = false;
    }

    public void ResumeVideo()
    {
        if (!isPlaying) return;

        videoPlayer.Play();
        skipVideoButton.interactable = true;
    }

    private void PlayVideo()
    {
        cameraFader.StartCoroutine(cameraFader.FadeToBlack(fadeToBlackDuration)); // Fade to black over 1 second
        gameManager.PauseGame();

        videoRawImage.gameObject.SetActive(true);
        isPlaying = true;

        startBgMusic = backgroundMusic.volume;
        backgroundMusic.volume = backgroundMusic.volume * 0.25f;

        onVideoStart?.Invoke(); // Trigger the UnityEvent if assigned
        // Start playing the video
        videoPlayer.Play();

        cameraFader?.StartCoroutine(cameraFader.FadeFromBlack(fadeFromBlackDuration)); // Fade from black over 1 second
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        cameraFader.StartCoroutine(cameraFader.FadeToBlack(fadeToBlackDuration)); // Fade to black over 1 second
        gameManager.ResumeGame();
        // When the video ends, hide the video
        videoRawImage.gameObject.SetActive(false);
        isPlaying = false;

        backgroundMusic.volume = startBgMusic;

        Debug.Log("OnVideoEnd");

        onVideoEnd?.Invoke(); // Trigger the UnityEvent if assigned
        cameraController?.ResetCurrentCameraRotation();

        // Show the next game object
        if (objectToAppearAfterVideo != null)
        {
            objectToAppearAfterVideo.SetActive(true);
        }

        cameraFader?.StartCoroutine(cameraFader.FadeFromBlack(fadeFromBlackDuration)); // Fade from black over 1 second
    
    }

    public void SkipVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            OnVideoEnd(videoPlayer);
        }
    }
}