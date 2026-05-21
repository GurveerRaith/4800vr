using UnityEngine;

public class SeatTrigger : MonoBehaviour
{
    [SerializeField] private Transform seatedPosition;
    [SerializeField] private Transform xrRig;
    [SerializeField] private MonoBehaviour locomotionSystem;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (GameStateManager.Instance.GetState() != GameState.SitDown) return;

        triggered = true;
        SitPlayer();
    }

    void SitPlayer()
    {
        // Snap rig to seat
        xrRig.position = seatedPosition.position;
        xrRig.rotation = seatedPosition.rotation;

        // Lock movement
        locomotionSystem.enabled = false;

        GameStateManager.Instance.SetState(GameState.WatchingVideo);
        Debug.Log("WatchingVideo state");
    }
}