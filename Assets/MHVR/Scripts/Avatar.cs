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
        Transform headset = FindObjectsOfType<VRTK_SDKManager>()[0].loadedSetup.actualHeadset.transform;
        ConstraintSource sourceHeadset = new ConstraintSource {
            sourceTransform = headset,
            weight = 1f
        };
        head.GetComponent<ParentConstraint>().AddSource(sourceHeadset);
    }
}
