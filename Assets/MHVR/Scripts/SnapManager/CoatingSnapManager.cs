using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CoatingSnapManager : MonoBehaviour
{
    [Tooltip("For bow's coating notch.")]
    public BowAim aim;

    private VRTK_SnapDropZone snapDropZone;
    private Collider objectCollider;
    private BowCoating coating;

    public void Start()
    {
        snapDropZone = GetComponent<VRTK_SnapDropZone>();

        snapDropZone.ObjectSnappedToDropZone += Snapped;
        snapDropZone.ObjectUnsnappedFromDropZone += Unsnapped;
    }

    protected virtual void Snapped(object sender, SnapDropZoneEventArgs e)
    {
        objectCollider = snapDropZone.GetCurrentSnappedObject().GetComponent<MeshCollider>();
        objectCollider.isTrigger = true;

        coating = snapDropZone.GetCurrentSnappedObject().GetComponent<BowCoating>();
        coating.PlayApplyAudio(0.3f);
        if (aim)
        {
            aim.coating = coating;
        }
    }

    protected virtual void Unsnapped(object sender, SnapDropZoneEventArgs e)
    {
        objectCollider.isTrigger = false;

        coating.PlayRemoveAudio(0.3f);
        if (aim)
        {
            aim.coating = null;
        }
    }
}
