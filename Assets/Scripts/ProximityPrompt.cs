using UnityEngine;
using TMPro;

public class ProximityPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshPro promptLabel;  // World-space TMP, not TMP_UGUI
    [SerializeField] private float showDistance = 1.8f;
    [SerializeField] private Transform playerHead;     // Drag in your XR Camera here

    private void Update()
    {
        // Debug.Log($"PromptLabel null: {promptLabel == null}, PlayerHead null: {playerHead == null}");
        if (promptLabel == null || playerHead == null) return;

        float dist = Vector3.Distance(transform.position, playerHead.position);
        bool shouldShow = dist <= showDistance
                          && MicrophonePickup.IsBeingHeld
                          && GameStateManager.Instance.GetState() == GameState.GiveMicrophone;

        promptLabel.gameObject.SetActive(shouldShow);

        // Always face the player
        if (promptLabel.gameObject.activeSelf)
            promptLabel.transform.LookAt(playerHead);
    }
}