using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

/// Sits on a trigger collider on or under the Teacher. When the FM microphone enters,
/// locks player locomotion, snap-rotates the rig so the player faces the teacher, turns
/// the teacher to face the player, takes the mic out of the player's hand and parents
/// it onto the teacher, then shows a dialogue with an OK button. Pressing OK restores
/// locomotion and advances the game state.
[RequireComponent(typeof(ProfessorSpeechBubble))]
public class ProfessorDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string dialogueLine = "Thanks, I will put this on now.";

    [Header("Scene references (leave empty to auto-find)")]
    [SerializeField] private Transform xrRig;
    [SerializeField] private Transform playerHead;
    [Tooltip("The teacher transform that will rotate to face the player. Leave empty to use this object's root.")]
    [SerializeField] private Transform teacherRoot;
    [Tooltip("When true, the mic is hidden (SetActive false) the moment it's handed off. " +
             "Overrides the snap-to-hold-point behavior.")]
    [SerializeField] private bool hideMicAfterHandoff = true;
    [Tooltip("Where the mic should snap onto the teacher after handoff. Ignored if " +
             "hideMicAfterHandoff is true. If empty AND hide is false, the mic is hidden anyway.")]
    [SerializeField] private Transform micHoldPoint;
    [Tooltip("Tick this if the teacher ends up facing AWAY from the player. Flips her facing 180°.")]
    [SerializeField] private bool flipTeacherFacing = false;

    [Header("Locomotion to lock during the handoff (leave empty to auto-find all LocomotionProviders)")]
    [SerializeField] private MonoBehaviour[] locomotionProviders;

    [Header("Animator (leave empty to auto-find on the teacher)")]
    [Tooltip("Animator to pause during the dialogue and resume on OK. Auto-found from teacherRoot if empty.")]
    [SerializeField] private Animator teacherAnimator;

    private ProfessorSpeechBubble speechBubble;
    private bool hasTriggered = false;

    // Captured before the handoff so we can restore on OK.
    private Vector3 teacherOriginalPosition;
    private Quaternion teacherOriginalRotation;
    private float teacherOriginalAnimatorSpeed = 1f;

    void Awake()
    {
        speechBubble = GetComponent<ProfessorSpeechBubble>();
    }

    void Start()
    {
        AutoFindReferences();
    }

    private void AutoFindReferences()
    {
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        if (xrRig == null && playerHead != null)
        {
            // Walk up the camera's parents looking for the XR Origin root.
            var t = playerHead;
            while (t.parent != null) t = t.parent;
            xrRig = t;
        }

        if (teacherRoot == null)
            teacherRoot = transform.root;

        if (teacherAnimator == null && teacherRoot != null)
            teacherAnimator = teacherRoot.GetComponentInChildren<Animator>();

        if (locomotionProviders == null || locomotionProviders.Length == 0)
        {
            var providers = FindObjectsByType<LocomotionProvider>(FindObjectsSortMode.None);
            var list = new List<MonoBehaviour>(providers.Length);
            foreach (var p in providers) list.Add(p);
            locomotionProviders = list.ToArray();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.GetState() != GameState.GiveMicrophone) return;

        var mic = other.GetComponentInParent<FMMicrophone>();
        if (mic == null) return;

        hasTriggered = true;
        StartCoroutine(RunHandoff(mic));
    }

    private IEnumerator RunHandoff(FMMicrophone mic)
    {
        Debug.Log("[ProfessorDialogueTrigger] Mic handoff started.");

        SetLocomotionEnabled(false);
        CaptureTeacherPose();
        PauseTeacherAnimator();
        FaceRigTowardTeacher();
        FaceTeacherTowardPlayer();
        DetachAndParentMic(mic);

        if (speechBubble != null)
        {
            speechBubble.Show(dialogueLine, OnOkPressed);
        }
        else
        {
            Debug.LogError("[ProfessorDialogueTrigger] No ProfessorSpeechBubble found.");
            OnOkPressed();
        }

        yield break;
    }

    private void FaceRigTowardTeacher()
    {
        if (xrRig == null || playerHead == null || teacherRoot == null) return;

        Vector3 toTeacher = teacherRoot.position - playerHead.position;
        toTeacher.y = 0f;
        if (toTeacher.sqrMagnitude < 0.0001f) return;

        float targetYaw = Mathf.Atan2(toTeacher.x, toTeacher.z) * Mathf.Rad2Deg;

        Vector3 headForward = playerHead.forward;
        headForward.y = 0f;
        if (headForward.sqrMagnitude < 0.0001f) return;
        float headYaw = Mathf.Atan2(headForward.x, headForward.z) * Mathf.Rad2Deg;

        float deltaYaw = Mathf.DeltaAngle(headYaw, targetYaw);

        // Rotate the rig around the head's vertical axis so the head doesn't translate.
        xrRig.RotateAround(playerHead.position, Vector3.up, deltaYaw);
    }

    private void FaceTeacherTowardPlayer()
    {
        if (teacherRoot == null || playerHead == null) return;

        Vector3 toPlayer = playerHead.position - teacherRoot.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Vector3 forward = flipTeacherFacing ? -toPlayer.normalized : toPlayer.normalized;
        teacherRoot.rotation = Quaternion.LookRotation(forward);
    }

    private void DetachAndParentMic(FMMicrophone mic)
    {
        var grab = mic.GetComponent<XRGrabInteractable>();

        if (grab != null && grab.isSelected && grab.interactionManager != null)
        {
            var holders = new List<IXRSelectInteractor>(grab.interactorsSelecting);
            foreach (var interactor in holders)
            {
                grab.interactionManager.SelectExit(interactor, (IXRSelectInteractable)grab);
            }
        }

        if (grab != null) grab.enabled = false;

        var rb = mic.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        foreach (var col in mic.GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (hideMicAfterHandoff || micHoldPoint == null)
        {
            // Just make it disappear.
            mic.gameObject.SetActive(false);
        }
        else
        {
            mic.transform.SetParent(micHoldPoint, worldPositionStays: false);
            mic.transform.localPosition = Vector3.zero;
            mic.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnOkPressed()
    {
        Debug.Log("[ProfessorDialogueTrigger] OK pressed — restoring control.");
        RestoreTeacherPose();
        ResumeTeacherAnimator();
        SetLocomotionEnabled(true);
        GameStateManager.Instance.SetState(GameState.SitDown);
    }

    private void CaptureTeacherPose()
    {
        if (teacherRoot == null) return;
        teacherOriginalPosition = teacherRoot.position;
        teacherOriginalRotation = teacherRoot.rotation;
    }

    private void RestoreTeacherPose()
    {
        if (teacherRoot == null) return;
        teacherRoot.SetPositionAndRotation(teacherOriginalPosition, teacherOriginalRotation);
    }

    private void PauseTeacherAnimator()
    {
        if (teacherAnimator == null) return;
        teacherOriginalAnimatorSpeed = teacherAnimator.speed;
        teacherAnimator.speed = 0f;
    }

    private void ResumeTeacherAnimator()
    {
        if (teacherAnimator == null) return;
        teacherAnimator.speed = teacherOriginalAnimatorSpeed == 0f ? 1f : teacherOriginalAnimatorSpeed;
    }

    private void SetLocomotionEnabled(bool enabled)
    {
        if (locomotionProviders == null) return;
        foreach (var p in locomotionProviders)
            if (p != null) p.enabled = enabled;
    }
}
