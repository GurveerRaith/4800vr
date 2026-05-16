using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MicrophonePickup : MonoBehaviour
{
    // Any script can check MicrophonePickup.IsBeingHeld without needing a reference
    public static bool IsBeingHeld { get; private set; }

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        IsBeingHeld = true;
        Debug.Log("[Microphone] Picked up");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        IsBeingHeld = false;
        Debug.Log("[Microphone] Released");
    }
}
