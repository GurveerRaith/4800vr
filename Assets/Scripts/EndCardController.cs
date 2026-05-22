using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Drives the end-of-game "fade to black + 'The End'" sequence.
///
/// Listens for game state == EndScreen, then:
///   1. Fades the black overlay Image from alpha 0 → 1 over `blackFadeSeconds`.
///   2. Waits `holdBeforeTextSeconds`.
///   3. Fades the "The End" TMP text from alpha 0 → 1 over `textFadeSeconds`.
///   4. Stays visible.
///
/// The overlay canvas should be parented to the XR camera and positioned just in
/// front of the near-clip plane so it covers the player's full view in VR.
public class EndCardController : MonoBehaviour
{
    [Header("UI references")]
    [Tooltip("The full-screen black Image. Starts with alpha 0.")]
    [SerializeField] private Image blackOverlay;

    [Tooltip("The 'The End' TMP text. Starts with alpha 0.")]
    [SerializeField] private TextMeshProUGUI endText;

    [Header("Timing")]
    [SerializeField] private float blackFadeSeconds = 2f;
    [SerializeField] private float holdBeforeTextSeconds = 0.5f;
    [SerializeField] private float textFadeSeconds = 1.5f;

    private bool hasFired = false;
    private bool subscribed = false;

    void Start()
    {
        // Initialize to fully transparent.
        if (blackOverlay != null) SetAlpha(blackOverlay, 0f);
        if (endText != null) SetAlpha(endText, 0f);
        TrySubscribe();
    }

    void OnEnable() => TrySubscribe();

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
        if (GameStateManager.Instance == null) return;
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        subscribed = true;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.EndScreen && !hasFired)
        {
            hasFired = true;
            StartCoroutine(RunEndCard());
        }
    }

    private IEnumerator RunEndCard()
    {
        if (blackOverlay != null) yield return FadeGraphic(blackOverlay, 0f, 1f, blackFadeSeconds);
        if (holdBeforeTextSeconds > 0f) yield return new WaitForSeconds(holdBeforeTextSeconds);
        if (endText != null) yield return FadeGraphic(endText, 0f, 1f, textFadeSeconds);
    }

    private IEnumerator FadeGraphic(Graphic g, float from, float to, float duration)
    {
        if (duration <= 0f) { SetAlpha(g, to); yield break; }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(g, Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetAlpha(g, to);
    }

    private static void SetAlpha(Graphic g, float a)
    {
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
}
