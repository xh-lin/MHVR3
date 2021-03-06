using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BowSnapManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float vibrationIntensity = 0.2f;
    [Tooltip("In seconds.")]
    public float vibrationDuration = 0.2f;

    private VRTK_SnapDropZone snapDropZone;
    private Collider[] objectColliders;
    private Animator animator;
    private Bow bow;

    public void Start()
    {
        snapDropZone = GetComponent<VRTK_SnapDropZone>();

        snapDropZone.ObjectEnteredSnapDropZone += Entered;
        snapDropZone.ObjectSnappedToDropZone += Snapped;
        snapDropZone.ObjectUnsnappedFromDropZone += Unsnapped;
    }

    protected virtual void Entered(object sender, SnapDropZoneEventArgs e)
    {
        // controller vibrates if entered while holding the bow
        foreach (VRTK_InteractableObject interact in snapDropZone.GetHoveringInteractableObjects())
        {
            if (snapDropZone.defaultSnappedInteractableObject == interact)
            {
                GameObject goGrabbing = interact.GetGrabbingObject();
                if (VRTK_DeviceFinder.IsControllerRightHand(goGrabbing))
                {
                    StartCoroutine(HapticPulse(vibrationIntensity, vibrationDuration, true));
                }
                else if (VRTK_DeviceFinder.IsControllerLeftHand(goGrabbing))
                {
                    StartCoroutine(HapticPulse(vibrationIntensity, vibrationDuration, false));
                }
            }
        }
    }

    private IEnumerator HapticPulse(float intensity, float duration, bool isRHand)
    {
        if (isRHand)
        {
            OVRInput.SetControllerVibration(.1f, intensity, OVRInput.Controller.RTouch);
        }
        else
        {
            OVRInput.SetControllerVibration(.1f, intensity, OVRInput.Controller.LTouch);
        }

        yield return new WaitForSeconds(duration);

        if (isRHand)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        else
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }
    }

    protected virtual void Snapped(object sender, SnapDropZoneEventArgs e)
    {
        var goSnapped = snapDropZone.GetCurrentSnappedObject();
        animator = goSnapped.GetComponent<Animator>();
        bow = goSnapped.GetComponent<Bow>();
        objectColliders = goSnapped.GetComponentsInChildren<MeshCollider>();

        animator.enabled = true; // set true here because Bow.Ungrab() setted it false
        if (!bow.IsFolded())
        {
            animator.SetBool("isFolded", true);
        }
        bow.PlaySheathAudio(0.3f);
        foreach (var c in objectColliders)
        {
            c.isTrigger = true;
        }
    }

    protected virtual void Unsnapped(object sender, SnapDropZoneEventArgs e)
    {
        animator.SetBool("isFolded", false);
        foreach (var c in objectColliders)
        {
            c.isTrigger = false;
        }
    }
}
