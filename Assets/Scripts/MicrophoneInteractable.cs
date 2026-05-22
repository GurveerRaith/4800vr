using System.Collections;
using UnityEngine;

public class MicrophoneInteractable : MonoBehaviour
{
    [Header("Professor Response")]
    [SerializeField] private float fallbackDialogueDuration = 3.5f;

    private bool hasBeenUsed = false;

    public void OnPlayerInteract()
    {
        Debug.Log($"[MicrophoneInteractable] Interact fired. " +
                  $"State: {GameStateManager.Instance.GetState()}, " +
                  $"HoldingMic: {MicrophonePickup.IsBeingHeld}, " +
                  $"AlreadyUsed: {hasBeenUsed}");

        if (hasBeenUsed) return;
        if (GameStateManager.Instance.GetState() != GameState.GiveMicrophone) return;

        // The player must actually be holding the microphone
        if (!MicrophonePickup.IsBeingHeld)
        {
            Debug.Log("[MicrophoneInteractable] Player isn't holding the mic yet — ignoring.");
            return;
        }

        hasBeenUsed = true;
        StartCoroutine(HandleInteraction());
    }

    private IEnumerator HandleInteraction()
    {
        yield return new WaitForSeconds(fallbackDialogueDuration);

        Debug.Log("[MicrophoneInteractable] Advancing to SitDown state.");
        GameStateManager.Instance.SetState(GameState.SitDown);
    }
}