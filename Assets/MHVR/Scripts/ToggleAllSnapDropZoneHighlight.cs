using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.Controllables;

public class ToggleAllSnapDropZoneHighlight : MonoBehaviour
{
    public VRTK_SnapDropZone[] snapZones;
    public VRTK_BaseControllable controllable;
    public Text displayText;
    public string maxText = "Highligh snap zones: On";
    public string minText = "Highligh snap zones: Off";


    protected virtual void OnEnable()
    {
        snapZones = FindObjectsOfType<VRTK_SnapDropZone>();
        controllable = (controllable == null ? GetComponent<VRTK_BaseControllable>() : controllable);
        if (controllable != null)
        {
            controllable.MaxLimitReached += MaxLimitReached;
            controllable.MinLimitReached += MinLimitReached;
        }
    }

    protected virtual void OnDisable()
    {
        if (controllable != null)
        {
            controllable.MaxLimitReached -= MaxLimitReached;
            controllable.MinLimitReached -= MinLimitReached;
        }
    }

    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        SetOption(true, maxText);
    }

    protected virtual void MinLimitReached(object sender, ControllableEventArgs e)
    {
        SetOption(false, minText);
    }

    protected virtual void SetOption(bool value, string text)
    {
        if (displayText != null)
            displayText.text = text;

        foreach (var zone in snapZones)
            zone.highlightAlwaysActive = value;
    }
}
