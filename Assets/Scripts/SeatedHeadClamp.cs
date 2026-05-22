using UnityEngine;

/// Runtime-added component that keeps the player's head pinned around a seated height.
/// In LateUpdate (after headset tracking has updated the camera) it checks the camera's
/// world Y and, if it has drifted outside [seatedY - downwardSlack, seatedY + upwardSlack],
/// translates the XR Origin vertically to push the head back into range.
///
/// This is what stops a physically-standing player from rising up in the virtual world.
public class SeatedHeadClamp : MonoBehaviour
{
    private Transform origin;
    private Transform head;
    private float seatedHeadWorldY;
    private float upwardSlack;
    private float downwardSlack;
    private bool configured;

    public void Configure(Transform origin, Transform head, float seatedHeadWorldY, float upwardSlack, float downwardSlack)
    {
        this.origin = origin;
        this.head = head;
        this.seatedHeadWorldY = seatedHeadWorldY;
        this.upwardSlack = Mathf.Max(0f, upwardSlack);
        this.downwardSlack = Mathf.Max(0f, downwardSlack);
        configured = true;
    }

    void LateUpdate()
    {
        if (!configured || origin == null || head == null) return;

        float headY = head.position.y;
        float maxY = seatedHeadWorldY + upwardSlack;
        float minY = seatedHeadWorldY - downwardSlack;

        if (headY > maxY)
        {
            Vector3 p = origin.position;
            p.y -= (headY - maxY);
            origin.position = p;
        }
        else if (headY < minY)
        {
            Vector3 p = origin.position;
            p.y += (minY - headY);
            origin.position = p;
        }
    }
}
