using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// Drives the professor's spoken announcement during the WatchingVideo state.
///
/// Flow:
///   1. WatchingVideo state begins.
///   2. Wait `startDelaySeconds` (default 4s) — gives the player a moment to settle in.
///   3. Play the announcement clip on the wired AudioSource, routed through `fmOnGroup`
///      (HearingAidEffects-only chain — clear, FM-mic audio).
///   4. After `fmBatteryLifeSeconds` (default 6s), swap the AudioSource's output to
///      `fmOffGroup` (HearingAidEffects + distance/reverb chain) and transition
///      state to FMBatteryDead.
///   5. Wait for the clip to finish playing.
///   6. Transition state back to WatchingVideo.
public class ProfessorAnnouncement : MonoBehaviour
{
    /// Fires once when the professor's announcement clip has finished playing, just
    /// before state is set back to WatchingVideo. The lecture video controller listens
    /// for this so the video starts regardless of whether the FM battery actually died.
    public static event System.Action OnAnnouncementFinished;


    [Header("Audio")]
    [Tooltip("AudioSource on the Teacher (or a child of her).")]
    [SerializeField] private AudioSource professorAudio;

    [Tooltip("The WAV clip of the professor announcing the 1099 MUX Form video.")]
    [SerializeField] private AudioClip announcementClip;

    [Header("Mixer groups")]
    [Tooltip("Routed while the FM mic is working. Typically a chain with HearingAidEffects only " +
             "(clear, FM-style audio).")]
    [SerializeField] private AudioMixerGroup fmOnGroup;

    [Tooltip("Routed after the FM battery dies. Typically HearingAidEffects + a 'distance' chain " +
             "(lowpass + reverb) so the prof sounds farther away.")]
    [SerializeField] private AudioMixerGroup fmOffGroup;

    [Header("Spatial blend (0 = 2D, 1 = 3D)")]
    [Tooltip("Spatial blend while the FM mic is working. 0 = fully 2D (clear, in-your-ears via the FM).")]
    [Range(0f, 1f)]
    [SerializeField] private float fmOnSpatialBlend = 0f;

    [Tooltip("Spatial blend after the FM battery dies. 1 = fully 3D (distance falloff kicks in, sounds farther away).")]
    [Range(0f, 1f)]
    [SerializeField] private float fmOffSpatialBlend = 1f;

    [Header("Ambient classroom audio")]
    [Tooltip("Optional. The looping classroom ambience AudioSource (e.g. on the Main Camera). " +
             "Fades out before the professor's announcement starts.")]
    [SerializeField] private AudioSource ambientAudio;

    [Tooltip("How long the ambient audio takes to fade out, in seconds.")]
    [SerializeField] private float ambientFadeDurationSeconds = 2f;

    [Header("Timing")]
    [Tooltip("Delay between entering WatchingVideo and the announcement starting. Should be " +
             "≥ ambientFadeDurationSeconds so the ambience is fully gone before the prof speaks.")]
    [SerializeField] private float startDelaySeconds = 4f;

    [Tooltip("How long the FM mic works before the battery dies and the mixer switches.")]
    [SerializeField] private float fmBatteryLifeSeconds = 6f;

    private bool hasFired = false;
    private bool subscribed = false;
    private Coroutine activeRoutine;

    // Subscribe in Start because Unity guarantees ALL Awakes are done before any Start.
    // This avoids a race where ProfessorAnnouncement.OnEnable runs before GameStateManager.Awake
    // and silently fails to attach the listener.
    void Start()
    {
        TrySubscribe();
    }

    void OnEnable()
    {
        // Cover the re-enable case (e.g. user toggles the component off and on).
        TrySubscribe();
    }

    void OnDisable()
    {
        if (subscribed && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged.RemoveListener(OnStateChanged);
            subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning($"[{nameof(ProfessorAnnouncement)}] GameStateManager.Instance is null in TrySubscribe — will retry on next enable.", this);
            return;
        }
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        subscribed = true;
        Debug.Log($"[{nameof(ProfessorAnnouncement)}] Subscribed to OnStateChanged.", this);
    }

    void OnStateChanged(GameState state)
    {
        Debug.Log($"[{nameof(ProfessorAnnouncement)}] OnStateChanged fired: {state} (hasFired={hasFired})", this);

        // Only fire once — when WatchingVideo is reached for the first time.
        // We intentionally don't refire when we set the state back to WatchingVideo at the end.
        if (state == GameState.WatchingVideo && !hasFired)
        {
            hasFired = true;
            activeRoutine = StartCoroutine(RunAnnouncement());
        }
    }

    private IEnumerator RunAnnouncement()
    {
        Debug.Log($"[{nameof(ProfessorAnnouncement)}] RunAnnouncement coroutine started.", this);

        if (professorAudio == null)
        {
            Debug.LogWarning($"[{nameof(ProfessorAnnouncement)}] No AudioSource wired — aborting.", this);
            yield break;
        }
        if (announcementClip == null)
        {
            Debug.LogWarning($"[{nameof(ProfessorAnnouncement)}] No AudioClip wired — aborting.", this);
            yield break;
        }

        // 1. Kick off the ambient fade in parallel with the settle-in delay.
        if (ambientAudio == null)
        {
            Debug.Log($"[{nameof(ProfessorAnnouncement)}] No ambient audio wired — skipping fade.", this);
        }
        else if (!ambientAudio.isPlaying)
        {
            Debug.Log($"[{nameof(ProfessorAnnouncement)}] Ambient audio is wired but not currently playing — skipping fade.", this);
        }
        else
        {
            Debug.Log($"[{nameof(ProfessorAnnouncement)}] Starting ambient fade ({ambientFadeDurationSeconds}s).", this);
            StartCoroutine(FadeOutAmbient(ambientAudio, ambientFadeDurationSeconds));
        }

        yield return new WaitForSeconds(startDelaySeconds);

        // 2. Start with the FM-on mixer chain and spatial blend, then play the clip.
        if (fmOnGroup != null) professorAudio.outputAudioMixerGroup = fmOnGroup;
        professorAudio.spatialBlend = fmOnSpatialBlend;
        professorAudio.clip = announcementClip;
        professorAudio.loop = false;
        professorAudio.Play();
        Debug.Log($"[{nameof(ProfessorAnnouncement)}] Announcement started (FM mic on).");

        // 3. FM mic works for fmBatteryLifeSeconds. If the clip is shorter, end gracefully.
        float endOfFmTime = Time.time + fmBatteryLifeSeconds;
        while (Time.time < endOfFmTime && professorAudio.isPlaying)
            yield return null;

        // 4. If audio is still going, kill the battery: swap mixer + spatial blend, change state.
        if (professorAudio.isPlaying)
        {
            if (fmOffGroup != null) professorAudio.outputAudioMixerGroup = fmOffGroup;
            professorAudio.spatialBlend = fmOffSpatialBlend;
            GameStateManager.Instance.SetState(GameState.FMBatteryDead);
            Debug.Log($"[{nameof(ProfessorAnnouncement)}] FM battery dead — mixer + spatial blend swapped (blend={fmOffSpatialBlend}).");

            // 5. Wait for the clip to finish on the FM-off chain.
            while (professorAudio.isPlaying) yield return null;
        }

        // 6. Notify listeners (e.g. LectureVideoController) that the prof is done talking,
        // then transition back to WatchingVideo.
        OnAnnouncementFinished?.Invoke();
        GameStateManager.Instance.SetState(GameState.WatchingVideo);
        Debug.Log($"[{nameof(ProfessorAnnouncement)}] Announcement finished — back to WatchingVideo.");
    }

    private IEnumerator FadeOutAmbient(AudioSource src, float duration)
    {
        if (duration <= 0f)
        {
            src.Stop();
            yield break;
        }

        float startVolume = src.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            src.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        src.Stop();
        src.volume = startVolume; // restore the field so reverting/re-enabling later is clean
    }
}
