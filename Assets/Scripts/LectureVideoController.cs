using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class LectureVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource professorAudio;
    [SerializeField] private float fmBatteryDiesAt = 45f; // Seconds into video
    [SerializeField] private GameObject confusionUI;

    void OnEnable()
    {
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
    }

    void OnDisable()
    {
        GameStateManager.Instance.OnStateChanged.RemoveListener(OnStateChanged);
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.WatchingVideo)
            StartCoroutine(RunLecture());
    }

    // TODO: Implement FM battery death and confusion moment 
    IEnumerator RunLecture()
    {
        yield return new WaitForSeconds(5f); // Brief pause before video starts
        Debug.Log("Video played");
        // videoPlayer.Play();
        // professorAudio.Play();

        // Wait for FM battery death moment
        // yield return new WaitForSeconds(fmBatteryDiesAt);
        // KillFMBattery();

        // Wait for video to finish
        // yield return new WaitUntil(() => !videoPlayer.isPlaying);

        GameStateManager.Instance.SetState(GameState.TakeQuiz);
        Debug.Log("TakeQuiz state");
    }

    // void KillFMBattery()
    // {
    //     GameStateManager.Instance.SetState(GameState.FMBatteryDead);

    //     StartCoroutine(FadeOutAudio(professorAudio, 1.5f));

    //     if (confusionUI != null) confusionUI.SetActive(true);

    //     StartCoroutine(ResumeWatching());
    // }

    // IEnumerator ResumeWatching()
    // {
    //     yield return new WaitForSeconds(2f);
    //     GameStateManager.Instance.SetState(GameState.WatchingVideo);
    // }

    // IEnumerator FadeOutAudio(AudioSource source, float duration)
    // {
    //     float startVolume = source.volume;
    //     float elapsed = 0f;
    //     while (elapsed < duration)
    //     {
    //         source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }
    //     source.Stop();
    // }
}