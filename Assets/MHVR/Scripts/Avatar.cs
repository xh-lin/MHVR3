using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRTK;

public class Avatar : MonoBehaviour
{
    public GameObject head;
    public GameObject backpack;
    [HideInInspector]
    public GameObject equip;

    private void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        // setup constraint to follow the VR headset
        yield return new WaitForEndOfFrame();
        ConstraintSource sourceHeadset = new ConstraintSource {
            sourceTransform = VRTK_DeviceFinder.HeadsetTransform(),
            weight = 1f
        };
        head.GetComponent<ParentConstraint>().AddSource(sourceHeadset);
    }
}
