// Use it with VRTK_InteractableObject_UnityEvents.
// Requires QuickOutline script.
// Use QuickOutline because VRTK_OutlineObjectCopyHighlighter doesn't support Skinned Mesh Renderer.

using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    // QuickOutline
    [SerializeField]
    private Outline outline;

    void Start()
    {
        if (outline == null)
            outline = GetComponent<Outline>();
        if (outline == null)
            Debug.LogError("Outline script not found.");
        else
            outline.enabled = false;
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
