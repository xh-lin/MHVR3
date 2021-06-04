using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BowSnapManager : MonoBehaviour
{
    public VRTK_SnapDropZone VRTK_SnapDropZone;

    private Animator animator;
    private Bow bow;
    private Collider[] objectColliders;

    public void Snap()
    {
        var go = VRTK_SnapDropZone.GetCurrentSnappedObject();
        animator = go.GetComponent<Animator>();
        bow = go.GetComponent<Bow>();
        objectColliders = go.GetComponentsInChildren<MeshCollider>();

        animator.enabled = true; // set true here because Bow.Ungrab() setted it false
        if (!bow.IsFolded())
            animator.SetBool("isFolded", true);
        bow.PlaySheathSound(0.3f);
        foreach (var c in objectColliders)
            c.isTrigger = true;
    }

    public void Unsnap()
    {
        animator.SetBool("isFolded", false);
        foreach (var c in objectColliders)
            c.isTrigger = false;
    }
}
