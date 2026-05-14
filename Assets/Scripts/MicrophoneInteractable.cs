using UnityEngine;

public class MicrophoneInteractable : MonoBehaviour
{
    // [SerializeField] private DialoguePlayer dialoguePlayer;

    public void OnPlayerInteract()
    {
        if (GameStateManager.Instance.GetState() != GameState.GiveMicrophone) return;
        Debug.Log("Prof received microphone");

        // dialoguePlayer.PlayDialogue("Sure, I will put it on.", OnDialogueFinished);
    }

    void OnDialogueFinished()
    {
        GameStateManager.Instance.SetState(GameState.SitDown);
        Debug.Log("SitDown state");
    }
}