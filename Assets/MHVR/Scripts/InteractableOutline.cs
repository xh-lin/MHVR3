// Use QuickOutline because VRTK_OutlineObjectCopyHighlighter doesn't support Skinned Mesh Renderer.

using UnityEngine;
using VRTK;

public class InteractableOutline : MonoBehaviour
{
    public VRTK_InteractableObject interact;
    public Outline outline;

    void Start()
    {
        interact = interact ? interact : GetComponent<VRTK_InteractableObject>();
        outline = outline ? outline : GetComponent<Outline>();
        outline.enabled = false;

        if (interact)
        {
            interact.InteractableObjectTouched += InteractOn;
            interact.InteractableObjectUntouched += InteractOff;
            interact.InteractableObjectGrabbed += InteractOff;
            interact.InteractableObjectUngrabbed += InteractOff;
        }
    }

    protected void InteractOn(object sender, InteractableObjectEventArgs e)
    {
        On();
    }
    protected void InteractOff(object sender, InteractableObjectEventArgs e)
    {
        Off();
    }

    public void On()
    {
        outline.enabled = true;
    }

    public void Off()
    {
        outline.enabled = false;
    }
}
