using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

/// Drives a world-space speech bubble. Hierarchy-agnostic: as long as the
/// dialogue text and OK button live somewhere under `canvasRoot`, the script
/// finds them and toggles the whole thing on/off as one unit.
public class ProfessorSpeechBubble : MonoBehaviour
{
    [Header("UI References (leave empty to auto-find)")]
    [Tooltip("The root GameObject of the speech bubble UI — usually the SpeechBubbleCanvas. " +
             "This whole GameObject is shown/hidden together, so put everything (panel, text, button) under it.")]
    [FormerlySerializedAs("speechBubbleRoot")]
    [SerializeField] private GameObject canvasRoot;

    [Tooltip("The dialogue text. If empty, auto-found under canvasRoot — preferring a GameObject named " +
             "'SpeechBubbleText', then any text not inside a Button.")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Tooltip("Optional. If the dialogue text isn't named 'SpeechBubbleText', set this to its name " +
             "so auto-find can pick it correctly.")]
    [SerializeField] private string dialogueTextName = "SpeechBubbleText";

    [Tooltip("The OK button. If empty, auto-found under canvasRoot.")]
    [SerializeField] private Button okButton;

    [Header("Billboard")]
    [Tooltip("The transform the bubble should face. Defaults to Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    private System.Action onOkPressed;

    void Awake()
    {
        // If canvasRoot wasn't set, grab the first Canvas in this hierarchy.
        if (canvasRoot == null)
        {
            var canvas = GetComponentInChildren<Canvas>(includeInactive: true);
            if (canvas != null) canvasRoot = canvas.gameObject;
        }

        // Pull text and button from inside canvasRoot if not explicitly assigned.
        if (canvasRoot != null)
        {
            if (dialogueText == null)
                dialogueText = FindDialogueText(canvasRoot);

            if (okButton == null)
                okButton = canvasRoot.GetComponentInChildren<Button>(includeInactive: true);
        }

        if (okButton != null) okButton.onClick.AddListener(HandleOkPressed);

        // Start hidden.
        if (canvasRoot != null) canvasRoot.SetActive(false);

        if (cameraTransform == null)
            cameraTransform = Camera.main?.transform;

        WarnIfMisconfigured();
    }

    void Update()
    {
        if (canvasRoot == null || !canvasRoot.activeSelf || cameraTransform == null) return;

        Vector3 lookDir = canvasRoot.transform.position - cameraTransform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
            canvasRoot.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    public void Show(string text, System.Action onConfirmed)
    {
        if (dialogueText != null) dialogueText.text = text;
        onOkPressed = onConfirmed;
        if (canvasRoot != null) canvasRoot.SetActive(true);
    }

    public void Hide()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    private void HandleOkPressed()
    {
        Hide();
        onOkPressed?.Invoke();
    }

    /// Picks the dialogue text from inside the canvas in a way that won't grab the OK button's label by accident.
    /// Priority:
    ///   1. A GameObject named `dialogueTextName` (default "SpeechBubbleText").
    ///   2. The first text whose ancestors do NOT include a Button (skips button labels).
    ///   3. The first text found, as a last resort.
    private TextMeshProUGUI FindDialogueText(GameObject root)
    {
        var allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        if (allTexts.Length == 0) return null;

        if (!string.IsNullOrEmpty(dialogueTextName))
        {
            foreach (var t in allTexts)
                if (t.gameObject.name == dialogueTextName) return t;
        }

        foreach (var t in allTexts)
            if (t.GetComponentInParent<Button>() == null) return t;

        return allTexts[0];
    }

    private void WarnIfMisconfigured()
    {
        if (canvasRoot == null)
            Debug.LogWarning($"[{nameof(ProfessorSpeechBubble)}] canvasRoot is missing — drag your SpeechBubbleCanvas into the field.", this);
        if (dialogueText == null)
            Debug.LogWarning($"[{nameof(ProfessorSpeechBubble)}] No TextMeshProUGUI found under canvasRoot.", this);
        if (okButton == null)
            Debug.LogWarning($"[{nameof(ProfessorSpeechBubble)}] No Button found under canvasRoot.", this);
    }
}
