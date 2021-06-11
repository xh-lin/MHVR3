using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK.Controllables;

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
        if (avatar.equip == null) {
            GameObject equipInstance = Instantiate(equipPrefab);
            avatar.equip = equipInstance;
            equipInstance.transform.SetParent(avatar.backpack.transform);
            equipInstance.transform.localPosition = Vector3.zero;

            if (displayText != null) {
                displayText.text = "1";
            }
        }
    }
}
