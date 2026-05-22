using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// One-time editor utility that builds the entire Tablet Quiz UI hierarchy in the
/// current scene with all components, RectTransforms, and the QuestionContent
/// script pre-wired. Run via `Tools → Create Tablet Quiz UI`.
///
/// Safe to run multiple times — each invocation creates a fresh Tablet GameObject;
/// previous ones are untouched. Supports Ctrl+Z undo.
public static class CreateTabletQuizUI
{
    [MenuItem("Tools/Create Tablet Quiz UI")]
    public static void Create()
    {
        // ---------------- Root ----------------
        var tablet = new GameObject("Tablet");
        Undo.RegisterCreatedObjectUndo(tablet, "Create Tablet Quiz UI");
        tablet.transform.position = new Vector3(0f, 1.25f, 0.5f); // adjust in scene afterwards
        tablet.transform.rotation = Quaternion.Euler(-25f, 0f, 0f);

        // ---------------- Canvas ----------------
        var canvasGO = CreateChild(tablet, "Canvas");
        var canvasRT = canvasGO.AddComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(250f, 180f);
        canvasRT.localScale = Vector3.one * 0.001f;
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        // ---------------- Background ----------------
        var bgGO = CreateUIChild(canvasGO, "Background");
        StretchToFill(bgGO);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.13f, 0.17f, 1f); // dark slate

        // ---------------- QuestionText ----------------
        var qtGO = CreateUIChild(canvasGO, "QuestionText");
        var qtRT = qtGO.GetComponent<RectTransform>();
        qtRT.anchorMin = new Vector2(0f, 1f);
        qtRT.anchorMax = new Vector2(1f, 1f);
        qtRT.pivot = new Vector2(0.5f, 1f);
        qtRT.anchoredPosition = new Vector2(0f, -10f);
        qtRT.sizeDelta = new Vector2(-20f, 50f);
        var qtTMP = qtGO.AddComponent<TextMeshProUGUI>();
        qtTMP.text = "Question text appears here.";
        qtTMP.fontSize = 14f;
        qtTMP.alignment = TextAlignmentOptions.Center;
        qtTMP.color = new Color(0.92f, 0.92f, 0.95f);
        qtTMP.enableWordWrapping = true;

        // ---------------- ChoiceButtons container ----------------
        var choicesGO = CreateUIChild(canvasGO, "ChoiceButtons");
        var choicesRT = choicesGO.GetComponent<RectTransform>();
        choicesRT.anchorMin = new Vector2(0.5f, 0.5f);
        choicesRT.anchorMax = new Vector2(0.5f, 0.5f);
        choicesRT.pivot = new Vector2(0.5f, 0.5f);
        choicesRT.anchoredPosition = new Vector2(0f, -10f);
        choicesRT.sizeDelta = new Vector2(220f, 80f);

        // 4 buttons in a 2×2 grid
        var btnA = CreateChoiceButton(choicesGO, "Button_A", "A", new Vector2(-55f, 20f));
        var btnB = CreateChoiceButton(choicesGO, "Button_B", "B", new Vector2(55f, 20f));
        var btnC = CreateChoiceButton(choicesGO, "Button_C", "C", new Vector2(-55f, -20f));
        var btnD = CreateChoiceButton(choicesGO, "Button_D", "D", new Vector2(55f, -20f));

        // ---------------- FeedbackPanel ----------------
        var feedbackPanel = CreateUIChild(canvasGO, "FeedbackPanel");
        StretchToFill(feedbackPanel);
        var feedbackBg = feedbackPanel.AddComponent<Image>();
        feedbackBg.color = new Color(0f, 0f, 0f, 0.78f);
        var feedbackText = CreateUIChild(feedbackPanel, "FeedbackText");
        StretchToFill(feedbackText);
        var feedbackTMP = feedbackText.AddComponent<TextMeshProUGUI>();
        feedbackTMP.text = "Correct";
        feedbackTMP.fontSize = 40f;
        feedbackTMP.alignment = TextAlignmentOptions.Center;
        feedbackTMP.color = Color.green;
        feedbackPanel.SetActive(false);

        // ---------------- ScorePanel ----------------
        var scorePanel = CreateUIChild(canvasGO, "ScorePanel");
        StretchToFill(scorePanel);
        var scoreBg = scorePanel.AddComponent<Image>();
        scoreBg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
        var scoreText = CreateUIChild(scorePanel, "ScoreText");
        StretchToFill(scoreText);
        var scoreTMP = scoreText.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "Score: 0 / 4";
        scoreTMP.fontSize = 28f;
        scoreTMP.alignment = TextAlignmentOptions.Center;
        scoreTMP.color = Color.white;
        scorePanel.SetActive(false);

        // ---------------- QuestionContent script on root, wired up ----------------
        var content = tablet.AddComponent<QuestionContent>();
        content.questionText = qtTMP;
        content.choiceButtons = new Button[] { btnA, btnB, btnC, btnD };
        content.feedbackPanel = feedbackPanel;
        content.feedbackText = feedbackTMP;
        content.feedbackDuration = 1.5f;

        // ---------------- Done ----------------
        Selection.activeGameObject = tablet;
        SceneView.lastActiveSceneView?.FrameSelected();
        Debug.Log("[CreateTabletQuizUI] Tablet created and selected. " +
                  "Reposition it onto the desk in front of the seat, then save as a prefab if you want.");
    }

    // ---------- helpers ----------

    private static GameObject CreateChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, worldPositionStays: false);
        return go;
    }

    private static GameObject CreateUIChild(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, worldPositionStays: false);
        return go;
    }

    private static void StretchToFill(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static Button CreateChoiceButton(GameObject parent, string name, string labelText, Vector2 anchoredPos)
    {
        var go = CreateUIChild(parent, name);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(100f, 30f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.85f, 0.87f, 0.92f, 1f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.85f, 0.87f, 0.92f, 1f);
        colors.highlightedColor = new Color(0.95f, 0.97f, 1f, 1f);
        colors.pressedColor = new Color(0.7f, 0.72f, 0.78f, 1f);
        colors.selectedColor = new Color(0.92f, 0.94f, 0.98f, 1f);
        colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.5f);
        btn.colors = colors;

        // label
        var label = CreateUIChild(go, "Label");
        StretchToFill(label);
        var tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.fontSize = 11f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        return btn;
    }
}
