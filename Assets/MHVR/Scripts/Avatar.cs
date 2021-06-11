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

    private VRTK_SDKManager VRTK_SDKManager;

    private void Start()
    {
        VRTK_SDKManager = FindObjectsOfType<VRTK_SDKManager>()[0];
        Invoke(nameof(LateStart), 0.1f);
    }

    private void LateStart()
    {
        Transform headset = VRTK_SDKManager.loadedSetup.actualHeadset.transform;
        ConstraintSource sourceHeadset = new ConstraintSource
        {
            sourceTransform = headset,
            weight = 1f
        };
        head.GetComponent<ParentConstraint>().AddSource(sourceHeadset);
    }
}
