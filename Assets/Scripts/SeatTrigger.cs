using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

/// Sits on a trigger box collider (e.g. SittingSeat on a desk). When the player enters
/// during the SitDown game state:
///   1. Movement is locked (continuous move, teleport, climb). Turn providers stay enabled.
///   2. The rig smoothly rotates around the player's head to face `frontOfClass`, while
///      also smoothly lowering so the head lands at `seatedHeadWorldY`.
///   3. A SeatedHeadClamp is attached to the rig to keep the head pinned to seated height
///      so the player can't physically stand up out of their virtual seat.
///   4. Game state advances to WatchingVideo.
[RequireComponent(typeof(Collider))]
public class SeatTrigger : MonoBehaviour
{
    [Header("Player references (leave empty to auto-find)")]
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform playerHead;

    [Header("Sit-down motion")]
    [Tooltip("The rig rotates to face this transform when sitting down. Drag in the Teacher, " +
             "a screen, or an empty marker at the front of the room.")]
    [SerializeField] private Transform frontOfClass;
    [Tooltip("Collider whose CENTER (X, Z) and TOP (Y) define where the head ends up. " +
             "Leave empty to use this GameObject's own Collider (the SittingSeat itself).")]
    [SerializeField] private Collider seatAnchor;
    [Tooltip("Extra Y offset added on top of the seat anchor's top Y. Positive = higher.")]
    [SerializeField] private float headYOffset = 0f;
    [Tooltip("Extra X offset added on top of the seat anchor's center X. Positive = right.")]
    [SerializeField] private float headXOffset = 0f;
    [Tooltip("Extra Z offset added on top of the seat anchor's top Y. Positive = higher.")]
    [SerializeField] private float headZOffset = 0f;
    [Tooltip("How long the smooth sit-and-rotate motion takes.")]
    [SerializeField] private float sitDuration = 1.0f;
    [Tooltip("Tick this if the rig ends up facing AWAY from `frontOfClass`.")]
    [SerializeField] private bool flipFacing = false;

    [Header("Head clamp (active after seating)")]
    [Tooltip("How far above seated head height the head may drift before being pushed back down.")]
    [SerializeField] private float clampUpwardSlack = 0.10f;
    [Tooltip("How far below seated head height the head may drift before being pushed back up.")]
    [SerializeField] private float clampDownwardSlack = 0.30f;

    [Header("Locomotion lock")]
    [Tooltip("Explicit list of providers to disable. Leave empty to auto-disable every " +
             "LocomotionProvider that ISN'T a turn provider.")]
    [SerializeField] private MonoBehaviour[] providersToLock;

    private bool triggered = false;
    private SeatedHeadClamp activeClamp;

    void Start()
    {
        AutoFindReferences();
    }

    private void AutoFindReferences()
    {
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (xrOrigin == null && playerHead != null)
        {
            var t = playerHead;
            while (t.parent != null) t = t.parent;
            xrOrigin = t;
        }

        if (seatAnchor == null)
            seatAnchor = GetComponent<Collider>();
    }

    private Vector3 ComputeTargetHeadPosition()
    {
        Bounds b = seatAnchor.bounds;
        return new Vector3(b.center.x + headXOffset, b.max.y + headYOffset, b.center.z + headZOffset);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.GetState() != GameState.SitDown) return;
        if (!IsPlayer(other)) return;
        if (frontOfClass == null)
        {
            Debug.LogWarning($"[{nameof(SeatTrigger)}] frontOfClass is not assigned — cannot rotate. Aborting.");
            return;
        }

        triggered = true;
        StartCoroutine(RunSitDown());
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player")) return true;
        if (xrOrigin != null && other.transform.root == xrOrigin) return true;
        return false;
    }

    private IEnumerator RunSitDown()
    {
        Debug.Log("[SeatTrigger] Sit-down sequence started.");
        LockMovementProviders(true);
        yield return SmoothSitAndFace();
        EnableHeadClamp();
        GameStateManager.Instance.SetState(GameState.WatchingVideo);
        Debug.Log("[SeatTrigger] WatchingVideo state");
    }

    private IEnumerator SmoothSitAndFace()
    {
        if (xrOrigin == null || playerHead == null || seatAnchor == null) yield break;

        // --- Snapshot starting state ---
        Vector3 startHeadPos = playerHead.position;
        Vector3 targetHeadPos = ComputeTargetHeadPosition();

        // --- Compute total yaw delta needed to face the front of class ---
        float totalDelta = ComputeYawDeltaToFace(frontOfClass.position);

        float elapsed = 0f;
        float appliedDelta = 0f;
        while (elapsed < sitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sitDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            // Rotate the rig around the head's vertical axis. The head's world position
            // is the pivot, so it stays fixed during the rotation step.
            float targetApplied = totalDelta * eased;
            float stepDelta = targetApplied - appliedDelta;
            appliedDelta = targetApplied;
            xrOrigin.RotateAround(playerHead.position, Vector3.up, stepDelta);

            // Translate the rig so the head reaches its eased position along (start → target).
            // Translating the rig by Δ moves the head by Δ, so this drops the head exactly where we want.
            Vector3 desiredHeadPos = Vector3.Lerp(startHeadPos, targetHeadPos, eased);
            xrOrigin.position += desiredHeadPos - playerHead.position;

            yield return null;
        }
    }

    private float ComputeYawDeltaToFace(Vector3 worldTarget)
    {
        Vector3 toTarget = worldTarget - playerHead.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return 0f;

        float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        if (flipFacing) targetYaw += 180f;

        Vector3 headForward = playerHead.forward;
        headForward.y = 0f;
        if (headForward.sqrMagnitude < 0.0001f) return 0f;
        float headYaw = Mathf.Atan2(headForward.x, headForward.z) * Mathf.Rad2Deg;

        return Mathf.DeltaAngle(headYaw, targetYaw);
    }

    private void EnableHeadClamp()
    {
        if (xrOrigin == null || playerHead == null || seatAnchor == null) return;

        if (activeClamp == null)
            activeClamp = xrOrigin.gameObject.AddComponent<SeatedHeadClamp>();

        activeClamp.Configure(
            origin: xrOrigin,
            head: playerHead,
            seatedHeadWorldY: ComputeTargetHeadPosition().y,
            upwardSlack: clampUpwardSlack,
            downwardSlack: clampDownwardSlack);
        activeClamp.enabled = true;
    }

    private void LockMovementProviders(bool locked)
    {
        if (providersToLock != null && providersToLock.Length > 0)
        {
            foreach (var p in providersToLock)
                if (p != null) p.enabled = !locked;
            return;
        }

        // Auto: disable every LocomotionProvider EXCEPT turn providers.
        var all = FindObjectsByType<LocomotionProvider>(FindObjectsSortMode.None);
        foreach (var p in all)
        {
            if (IsTurnProvider(p)) continue;
            p.enabled = !locked;
        }
    }

    private static bool IsTurnProvider(LocomotionProvider p)
    {
        // SnapTurnProvider, ContinuousTurnProvider, and any custom *Turn* providers.
        return p.GetType().Name.Contains("Turn");
    }
}
