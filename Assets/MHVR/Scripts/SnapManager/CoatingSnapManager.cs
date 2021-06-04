using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CoatingSnapManager : MonoBehaviour
{
    public VRTK_SnapDropZone VRTK_SnapDropZone;
    // for the notch on the bow
    public BowAim aim;

    private Collider objectCollider;
    private Coating coating;

    public void Snap()
    {
        objectCollider = VRTK_SnapDropZone.GetCurrentSnappedObject().GetComponent<MeshCollider>();
        objectCollider.isTrigger = true;
    }

    public void Unsnap()
    {
        objectCollider.isTrigger = false;
    }

    public void Apply()
    {
        Snap();
        coating = VRTK_SnapDropZone.GetCurrentSnappedObject().GetComponent<Coating>();
        coating.PlayApplySound(0.3f);
        aim.coating = coating;
    }

    public void Remove()
    {
        Unsnap();
        coating.PlayRemoveSound(0.3f);
        aim.coating = null;
    }
}
