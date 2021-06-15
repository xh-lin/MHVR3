// Mofidied from VRTK.Examples.Archery.ArrowNotch

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ArrowNotch : MonoBehaviour
{
    [HideInInspector]
    public GameObject arrow;
    private VRTK_InteractableObject obj;

    private void Awake()
    {
        obj = GetComponent<VRTK_InteractableObject>();
        arrow = GetComponentInChildren<Arrow>().gameObject;
    }

    private void OnTriggerEnter(Collider collider)
    {
        var handle = collider.GetComponentInParent<BowHandle>();
        NockingArrow(handle);
    }

    public void NockingArrow(BowHandle handle)
    {
        if (handle != null && obj != null && handle.aim.IsHeld() && obj.IsGrabbed() && !handle.aim.bow.IsFolded())
        {
            arrow.transform.SetParent(handle.arrowNockingPoint);    // attach to bow's nocking point
            handle.aim.SetArrow(arrow);

            CopyNotchToArrow();
        }

    }

    public void CopyNotchToArrow()
    {
        arrow.GetComponent<Arrow>().SetArrowHolder(gameObject);
        arrow.GetComponent<Arrow>().OnNock();
    }
}
