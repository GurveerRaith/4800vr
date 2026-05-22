using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

/// Drives the lecture video on the big classroom screen.
///
/// Triggers off `ProfessorAnnouncement.OnAnnouncementFinished` — fired by
/// ProfessorAnnouncement the moment the WAV clip ends, regardless of whether the
/// FM battery actually died first. After the event fires:
///   1. Wait `videoStartDelaySeconds` (default 3s).
///   2. Hide the thumbnail/intro image, play the video.
///   3. When the video finishes, set state to TakeQuiz.
public class LectureVideoController : MonoBehaviour
{
    [Header("Video")]
    [Tooltip("The VideoPlayer on the big classroom screen (cc_video_player).")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("The RawImage that displays the video output on the screen (in the screen prefab, " +
             "this is the GameObject called VideoImage). Kept hidden until playback starts, " +
             "then activated so the video is visible.")]
    [FormerlySerializedAs("videoImage")]
    [SerializeField] private GameObject videoDisplay;

    [Header("Timing")]
    [Tooltip("Delay between the professor finishing and the video starting.")]
    [SerializeField] private float videoStartDelaySeconds = 3f;

    private bool hasPlayedVideo = false;
    private bool subscribed = false;

    void Start()
    {
        // Ensure the video doesn't auto-play. We want full control.
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            if (videoPlayer.isPlaying) videoPlayer.Stop();
        }

        // Hide the display surface until the video actually starts.
        if (videoDisplay != null) videoDisplay.SetActive(false);

        Subscribe();
    }

    void OnEnable() => Subscribe();

    void OnDisable()
    {
        if (!subscribed) return;
        ProfessorAnnouncement.OnAnnouncementFinished -= HandleAnnouncementFinished;
        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed) return;
        ProfessorAnnouncement.OnAnnouncementFinished += HandleAnnouncementFinished;
        subscribed = true;
        Debug.Log($"[{nameof(LectureVideoController)}] Subscribed to ProfessorAnnouncement.OnAnnouncementFinished.", this);
    }

    private void HandleAnnouncementFinished()
    {
        if (hasPlayedVideo) return;
        hasPlayedVideo = true;
        Debug.Log($"[{nameof(LectureVideoController)}] Prof announcement done — starting video sequence.", this);
        StartCoroutine(PlayVideoAfterDelay());
    }

    private IEnumerator PlayVideoAfterDelay()
    {
        yield return new WaitForSeconds(videoStartDelaySeconds);

        // Activate the display surface so the player can see the video.
        if (videoDisplay != null) videoDisplay.SetActive(true);

        if (videoPlayer == null)
        {
            Debug.LogError($"[{nameof(LectureVideoController)}] No VideoPlayer wired — cannot play.", this);
            yield break;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
        Debug.Log($"[{nameof(LectureVideoController)}] Video playing.", this);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoFinished;
        Debug.Log($"[{nameof(LectureVideoController)}] Video finished — TakeQuiz state.", this);
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SetState(GameState.TakeQuiz);
    }
}
