using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Equipment : MonoBehaviour
{
    public VRTK.VRTK_SDKManager VRTK_SDKManager;
    public GameObject head;

    private void Start()
    {
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
