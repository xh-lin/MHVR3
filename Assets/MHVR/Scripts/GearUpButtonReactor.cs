using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK.Controllables;
using VRTK;

public class GearUpButtonReactor : MonoBehaviour
{
    public VRTK_BaseControllable controllable;
    public GameObject equipPrefab;
    public Text displayText;

    private Avatar avatar;

    void Start()
    {
        avatar = FindObjectsOfType<Avatar>()[0];
        controllable = (controllable == null ? GetComponent<VRTK_BaseControllable>() : controllable);
        controllable.MaxLimitReached += MaxLimitReached;
    }

    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if (avatar.equip == null)
        {
            Equip();
        }
        else
        {
            Unequip();
        }
    }

    private void Equip()
    {
        GameObject equipInstance = Instantiate(equipPrefab);
        avatar.equip = equipInstance;
        equipInstance.transform.SetParent(avatar.backpack.transform);
        equipInstance.transform.localPosition = Vector3.zero;
        equipInstance.transform.localRotation = Quaternion.identity;

        if (displayText != null)
        {
            displayText.text = "1";
        }
    }

    private void Unequip()
    {
        VRTK_SnapDropZone[] snapDropZones = avatar.equip.GetComponentsInChildren<VRTK_SnapDropZone>();
        foreach (var sdz in snapDropZones)
        {
            var dsiObj = sdz.defaultSnappedInteractableObject;
            if (dsiObj != null)
            {
                if (dsiObj.IsGrabbed())
                {
                    dsiObj.GetGrabbingObject().GetComponent<VRTK_InteractGrab>().ForceRelease();
                }
                Destroy(dsiObj.gameObject);
            }
        }
        Destroy(avatar.equip);

        if (displayText != null)
        {
            displayText.text = "0";
        }
    }
}
