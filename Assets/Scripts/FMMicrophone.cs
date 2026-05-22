using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class FMMicrophone : MonoBehaviour
{
    [Header("Optional: mic-specific snap pose")]
    [SerializeField] private Transform attachTransform;

    [Header("Auto-attach to right hand on start")]
    [SerializeField] private bool autoAttachOnStart = true;
    [Tooltip("Leave empty to auto-find a Direct Interactor whose hierarchy path contains 'Right'.")]
    [SerializeField] private XRBaseInteractor rightHandInteractor;
    [SerializeField] private float attachDelaySeconds = 0.5f;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (attachTransform != null)
            grabInteractable.attachTransform = attachTransform;

        grabInteractable.throwOnDetach = false;

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    IEnumerator Start()
    {
        if (!autoAttachOnStart) yield break;

        yield return new WaitForSeconds(attachDelaySeconds);

        if (rightHandInteractor == null)
            rightHandInteractor = FindRightHandInteractor();

        if (rightHandInteractor == null)
        {
            Debug.LogWarning("[FMMicrophone] Could not find a right-hand interactor. " +
                             "Drag your right-controller Direct Interactor into the inspector field.");
            yield break;
        }

        var manager = grabInteractable.interactionManager;
        if (manager == null)
        {
            Debug.LogWarning("[FMMicrophone] No XR Interaction Manager found.");
            yield break;
        }

        manager.SelectEnter(
            (IXRSelectInteractor)rightHandInteractor,
            (IXRSelectInteractable)grabInteractable);

        Debug.Log($"[FMMicrophone] Auto-attached to '{rightHandInteractor.name}'.");
    }

    private XRBaseInteractor FindRightHandInteractor()
    {
        var candidates = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        XRBaseInteractor bestDirect = null;
        XRBaseInteractor bestAny = null;

        foreach (var c in candidates)
        {
            if (!IsInRightHandHierarchy(c.transform)) continue;
            bestAny ??= c;
            if (c is XRDirectInteractor && bestDirect == null) bestDirect = c;
        }

        return bestDirect != null ? bestDirect : bestAny;
    }

    private static bool IsInRightHandHierarchy(Transform t)
    {
        while (t != null)
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("right")) return true;
            t = t.parent;
        }
        return false;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("[FMMicrophone] Grabbed by controller.");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("[FMMicrophone] Released.");
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
