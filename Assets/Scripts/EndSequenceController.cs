using System.Collections;
using UnityEngine;
using UnityEngine.Playables; // For Timeline cutscene
using TMPro;

public class EndSequenceController : MonoBehaviour
{
    [SerializeField] private PlayableDirector cutsceneDirector; // Cutscene of student grabbing mic and walking out
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private GameObject endTextPanel;
    [SerializeField] private TextMeshProUGUI endText;

    [TextArea(4, 10)]
    [SerializeField]
    private string reflectionText =
        "You missed most of that lecture.\n\n" +
        "Not because you weren't paying attention —\n" +
        "but because the battery in your FM system died.\n\n" +
        "This happens more than you'd think.\n\n" +
        "You didn't ask for help. You didn't want to be a burden.\n\n" +
        "Next time: charge your device the night before.\n" +
        "And know that asking for support is never a weakness.";

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
        if (state == GameState.QuizFailed)
            StartCoroutine(RunEndSequence());
    }

    IEnumerator RunEndSequence()
    {
        GameStateManager.Instance.SetState(GameState.Cutscene);
        Debug.Log("Cutscene state");

        // Play the Timeline cutscene (student grabs mic, walks out)
        cutsceneDirector.Play();
        yield return new WaitUntil(() => cutsceneDirector.state != PlayState.Playing);

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f, 2f));

        GameStateManager.Instance.SetState(GameState.EndScreen);
        Debug.Log("EndScreen state");

        // Show text panel
        endTextPanel.SetActive(true);
        endText.text = "";

        // Type on the reflection text for impact
        yield return StartCoroutine(TypeText(endText, reflectionText, 0.04f));
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeCanvas.alpha = to;
    }

    IEnumerator TypeText(TextMeshProUGUI label, string text, float delay)
    {
        label.text = "";
        foreach (char c in text)
        {
            label.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}