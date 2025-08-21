using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoMenuController : MonoBehaviour
{
    // Assign these in the Inspector
    [SerializeField] private Button playVideoButton;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private GameObject objectToAppearAfterVideo;

    public UnityEvent onVideoStart; // Optional: UnityEvent to trigger when the video starts
    public UnityEvent onVideoEnd; // Optional: UnityEvent to trigger when the video ends

    void Start()
    {
        // Initially, hide the video and the object to appear.
        videoRawImage.gameObject.SetActive(false);
        if (objectToAppearAfterVideo != null)
        {
            objectToAppearAfterVideo.SetActive(false);
        }

        // Add a listener to the button's click event
        if (playVideoButton != null)
        {
            playVideoButton.onClick.AddListener(PlayVideo);
        }
        else
        {
            Debug.LogError("Play Video Button is not assigned!");
        }

        // Add a listener to the video player's loop point reached event.
        // This event is triggered when the video finishes.
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void PlayVideo()
    {
        videoRawImage.gameObject.SetActive(true);

        onVideoStart?.Invoke(); // Trigger the UnityEvent if assigned
        // Start playing the video
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        // When the video ends, hide the video
        videoRawImage.gameObject.SetActive(false);

        onVideoEnd?.Invoke(); // Trigger the UnityEvent if assigned

        // Show the next game object
        if (objectToAppearAfterVideo != null)
        {
            objectToAppearAfterVideo.SetActive(true);
        }
    }
}