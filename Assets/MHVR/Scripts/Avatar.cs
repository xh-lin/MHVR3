using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Avatar : MonoBehaviour
{
    public VRTK.VRTK_SDKManager VRTK_SDKManager;
    public GameObject head;

    private void Start()
    {
        Invoke(nameof(LateStart), 0.1f);
    }

    private void LateStart()
    {
        Transform rig = VRTK_SDKManager.loadedSetup.actualBoundaries.transform;
        ConstraintSource sourceRig = new ConstraintSource
        {
            sourceTransform = rig,
            weight = 1f
        };
        GetComponent<ParentConstraint>().AddSource(sourceRig);

        Transform neck = VRTK_SDKManager.loadedSetup.actualHeadset.transform.parent;
        ConstraintSource sourceNeck = new ConstraintSource
        {
            sourceTransform = neck,
            weight = 1f
        };
        head.GetComponent<RotationConstraint>().AddSource(sourceNeck);
    }

}
