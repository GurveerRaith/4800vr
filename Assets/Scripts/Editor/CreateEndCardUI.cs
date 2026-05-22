using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// One-time editor utility that builds the End Card UI ("fade to black + The End")
/// parented to the active scene's MainCamera so it always covers the VR view.
/// Run via `Tools → Create End Card UI`. Supports Ctrl+Z undo.
public static class CreateEndCardUI
{
    [MenuItem("Tools/Create End Card UI")]
    public static void Create()
    {
        // 1. Find the main camera.
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindFirstObjectByType<Camera>();
        }
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Create End Card UI",
                "No camera found in the scene. Open the SampleScene first.", "OK");
            return;
        }

        // 2. Root canvas, parented to the camera so it follows head movement in VR.
        var rootGO = new GameObject("EndCardCanvas", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(rootGO, "Create End Card UI");
        rootGO.transform.SetParent(cam.transform, worldPositionStays: false);
        rootGO.transform.localPosition = new Vector3(0f, 0f, 0.3f); // 30 cm in front of the eye
        rootGO.transform.localRotation = Quaternion.identity;
        rootGO.transform.localScale = Vector3.one * 0.001f;

        var rootRT = rootGO.GetComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(1000f, 1000f); // generous — covers FOV at 30 cm

        var canvas = rootGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 32767; // render on top of other world-space canvases
        rootGO.AddComponent<CanvasScaler>();

        // 3. Black overlay image.
        var bgGO = new GameObject("BlackOverlay", typeof(RectTransform));
        bgGO.transform.SetParent(rootGO.transform, worldPositionStays: false);
        StretchToFill(bgGO);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0f); // starts transparent
        bgImg.raycastTarget = false;

        // 4. "The End" text.
        var textGO = new GameObject("EndText", typeof(RectTransform));
        textGO.transform.SetParent(rootGO.transform, worldPositionStays: false);
        StretchToFill(textGO);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "The End";
        tmp.fontSize = 80f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 1f, 1f, 0f); // starts transparent
        tmp.raycastTarget = false;

        // 5. Controller, wired up.
        var ctrl = rootGO.AddComponent<EndCardController>();
        SetPrivateField(ctrl, "blackOverlay", bgImg);
        SetPrivateField(ctrl, "endText", tmp);

        Selection.activeGameObject = rootGO;
        Debug.Log("[CreateEndCardUI] End Card UI created under " + cam.name +
                  ". Press Play and complete the quiz to see it fade in.");
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

    private static void SetPrivateField(Object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        if (field != null) field.SetValue(target, value);
    }
}
