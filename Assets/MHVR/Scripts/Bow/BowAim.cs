// Mofidied from VRTK.Examples.Archery.BowAim

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BowAim : MonoBehaviour
{
    // aim
    public float powerMultiplier = 30.0f;
    public float pullMultiplier = 1.0f;
    public float pullOffset = 0.214f;
    public float maxPullDistance = 1.0f;
    public float bowVibration = 0.062f;
    public float stringVibration = 0.087f;
    public GameObject arrowPrefab;
    public GameObject snapHandle;   // for calculating bow aiming rotation

    // shake
    [Range(0f, 30f)]
    public float shakeSpeed = 10f;
    [Range(0f, 0.1f)]
    public float shakeMultiplier = 0.01f;

    // spread shot
    [Range(0f, 1f)] // 90 ~ 0 degree
    public float spreadShotThreshold = 0.85f; // 0.85f ~> 30 degree 

    private GameObject currentArrow;
    private BowHandle handle;

    private VRTK_InteractableObject interact;
    private VRTK_InteractGrab holdControl;
    private VRTK_InteractGrab stringControl;

    // aim
    private Quaternion releaseRotation;
    private Quaternion baseRotation;
    private bool isFired;
    private float fireOffset;
    private float currentPull;
    private float previousPull;

    // charge threshold
    private const float chargeThreshold = 0.75f;
    private const float chargeTwoThreshold = 0.84f;
    private const float chargeMaxThreshold = 0.97f;
    private const float chargeInterpolation = 0.4f;

    // charge pull
    private float chargePull;
    private float startChargePull;
    private float previousChargePull;
    private float lerpVal;
    private int chargeLevel;

    // shake
    private float arrowSeedX;
    private float arrowSeedY;
    private float bowSeedX;
    private float bowSeedY;
    private bool isShaking;

    [HideInInspector]
    public Bow bow;
    [HideInInspector]
    public Coating coating;

    private Color coatingColor;

    private void Start()
    {
        handle = GetComponentInChildren<BowHandle>();
        interact = GetComponent<VRTK_InteractableObject>();
        interact.InteractableObjectGrabbed += new InteractableObjectEventHandler(DoObjectGrab);

        bow = GetComponent<Bow>();
        startChargePull = chargeThreshold;
        chargeLevel = 1;

        arrowSeedX = Random.value * 10f;
        arrowSeedY = Random.value * 10f;
        bowSeedX = Random.value * 10f;
        bowSeedY = Random.value * 10f;
        isShaking = false;
    }

    private void Update()
    {
        // holding bow with arrow loaded
        if (currentArrow != null && IsHeld())
        {
            AimArrow();
            AimBow();
            PullString();
            // release arrow
            if (!stringControl.IsGrabButtonPressed())
            {
                coatingColor = (coating == null) ? Color.clear : coating.Consume();
                currentArrow.GetComponent<Arrow>().Fired(coatingColor);
                isFired = true;
                releaseRotation = transform.localRotation;
                Release();
            }
        }
        // recover bow rotation after releasing arrow
        else if (IsHeld())
        {
            if (isFired)
            {
                isFired = false;
                fireOffset = Time.time;
            }
            if (releaseRotation != baseRotation)
                transform.localRotation = Quaternion.Lerp(releaseRotation, baseRotation, 
                    (Time.time - fireOffset) * 8);
        }

        // drop bow while arrow loaded
        if (!IsHeld())
        {
            if (currentArrow != null)
                Release();
        }
    }

    public bool IsHeld()
    {
        return interact.IsGrabbed();
    }

    public bool HasArrow()
    {
        return currentArrow != null;
    }

    public void SetArrow(GameObject arrow)
    {
        currentArrow = arrow;
        bow.PlaySetArrowSound(0.5f);
    }

    private void DoObjectGrab(object sender, InteractableObjectEventArgs e)
    {
        if (VRTK_DeviceFinder.IsControllerLeftHand(e.interactingObject))
        {
            holdControl = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_InteractGrab>();
            stringControl = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_InteractGrab>();
        }
        else
        {
            stringControl = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_InteractGrab>();
            holdControl = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_InteractGrab>();
        }
        StartCoroutine(nameof(GetBaseRotation));
    }

    private IEnumerator GetBaseRotation()
    {
        yield return new WaitForEndOfFrame();
        baseRotation = transform.localRotation;
    }

    private void Release()
    {
        bow.SetPullAnimation(0);

        currentArrow.transform.SetParent(null);
        Collider[] arrowCols = currentArrow.GetComponentsInChildren<Collider>();
        Collider[] BowCols = GetComponentsInChildren<Collider>();
        foreach (var ac in arrowCols)
        {
            ac.enabled = true;
            foreach (var bc in BowCols)
                Physics.IgnoreCollision(ac, bc);
        }

        currentArrow.GetComponent<Rigidbody>().isKinematic = false;
        currentArrow.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        currentArrow.GetComponent<Rigidbody>().velocity = currentPull * powerMultiplier * 
            currentArrow.transform.TransformDirection(Vector3.forward);

        currentArrow.GetComponent<Arrow>().InFlight();
        currentArrow.GetComponent<Arrow>().StopSound();

        Vector3 bowUp = holdControl.transform.TransformDirection(Vector3.forward);
        if (Vector3.ProjectOnPlane(bowUp, Vector3.up).magnitude > spreadShotThreshold)
        {
            // spread shot, if the bow is holding horizontally
            DuplicateArrow(new Vector3(0.1f, 0f, 1f));
            DuplicateArrow(new Vector3(-0.1f, 0f, 1f));
            currentArrow.GetComponent<Arrow>().PlaySpreadShotSound(chargeLevel, 0.2f);
        }
        else
        {
            // charge shot
            switch (chargeLevel)
            {
                case 2:
                    DuplicateArrow(new Vector3(0f, -0.01f, 1f));
                    break;
                case 3:
                    DuplicateArrow(new Vector3(0.01f, -0.01f, 1f));
                    DuplicateArrow(new Vector3(-0.01f, -0.01f, 1f));
                    break;
            }
            currentArrow.GetComponent<Arrow>().PlayShotSound(chargeLevel, 0.2f);
        }

        currentArrow = null;

        // reset pull
        currentPull = 0.0f;
        previousPull = 0.0f;
        startChargePull = chargeThreshold;
        chargePull = 0.0f;
        lerpVal = 0.0f;
        chargeLevel = 1;
        isShaking = false;

        ReleaseArrow();

        bow.StopGlow();
        bow.StopSound(); // Stop pull hold sound loop
        bow.PlayShotSound(0.5f);
    }

    private void DuplicateArrow(Vector3 direction)
    {
        GameObject newArrowNotch = Instantiate(arrowPrefab, currentArrow.transform.position, 
            currentArrow.transform.rotation);
        GameObject newArrow = newArrowNotch.GetComponent<ArrowNotch>().arrow;
        newArrowNotch.GetComponent<ArrowNotch>().CopyNotchToArrow();

        newArrow.transform.SetParent(null);
        Collider[] arrowCols = newArrow.GetComponentsInChildren<Collider>();
        Collider[] BowCols = GetComponentsInChildren<Collider>();
        foreach (var ac in arrowCols)
        {
            ac.enabled = true;
            foreach (var bc in BowCols)
                Physics.IgnoreCollision(ac, bc);
        }

        newArrow.GetComponent<Rigidbody>().isKinematic = false;
        newArrow.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        newArrow.GetComponent<Rigidbody>().velocity = currentPull * powerMultiplier * 
            currentArrow.transform.TransformDirection(direction);
        newArrow.GetComponent<Arrow>().InFlight();

        if (coatingColor == Color.clear)
            newArrow.GetComponent<Arrow>().ps.Stop();
        else
            newArrow.GetComponent<Arrow>().Fired(coatingColor);
    }

    private void ReleaseArrow()
    {
        if (stringControl)
            stringControl.ForceRelease();
    }

    private void AimArrow()
    {
        currentArrow.transform.localPosition = Vector3.zero;
        Vector3 lookDir = handle.nockSide.position;

        if (isShaking)
        {
            var x = (Mathf.PerlinNoise(arrowSeedX, Time.time * shakeSpeed) - 0.5f) * shakeMultiplier;
            var y = (Mathf.PerlinNoise(arrowSeedY, Time.time * shakeSpeed) - 0.5f) * shakeMultiplier;
            lookDir += new Vector3(x, y, 0);
        }

        currentArrow.transform.LookAt(lookDir);
    }

    private void AimBow()
    {
        Vector3 lookDir = holdControl.transform.position - stringControl.transform.position;
        Vector3 upDir = holdControl.transform.TransformDirection(
            Quaternion.Euler(-snapHandle.transform.localEulerAngles) * Vector3.up);

        if (isShaking)
        {
            var x = (Mathf.PerlinNoise(bowSeedX, Time.time * shakeSpeed) - 0.5f) * shakeMultiplier;
            var y = (Mathf.PerlinNoise(bowSeedY, Time.time * shakeSpeed) - 0.5f) * shakeMultiplier;
            lookDir += new Vector3(x, y, 0);
        }

        transform.rotation = Quaternion.LookRotation(lookDir, upDir);

        Debug.Log(holdControl.transform.position + ",  " + stringControl.transform.position);
    }

    private void PullString()
    {
        currentPull = Mathf.Clamp(
            (Vector3.Distance(holdControl.transform.position, stringControl.transform.position) - pullOffset) * pullMultiplier, 
            0, 
            maxPullDistance);

        // charge pull
        if (currentPull > chargeThreshold && currentPull > chargePull)
        {
            lerpVal += chargeInterpolation * Time.deltaTime;
            chargePull = Mathf.Lerp(startChargePull, currentPull, lerpVal);
            bow.SetPullAnimation(chargePull);
        } 
        else
        {
            if (currentPull < previousPull)
            {
                lerpVal = 0.0f;
                chargePull = currentPull;
            }
            startChargePull = currentPull;

            bow.SetPullAnimation(currentPull);
        }

        // controller haptic
        if (currentPull.ToString("F2") != previousPull.ToString("F2"))
        {
            VRTK_ControllerHaptics.TriggerHapticPulse(
                VRTK_ControllerReference.GetControllerReference(holdControl.gameObject), 
                bowVibration);
            VRTK_ControllerHaptics.TriggerHapticPulse(
                VRTK_ControllerReference.GetControllerReference(stringControl.gameObject), 
                stringVibration);
        }

        // charge pull sound and glow
        if (previousPull < 0.4f && currentPull > 0.4f)
        {
            bow.PlayStringStretchSound(0.2f);
        }
        else if (previousChargePull < chargeThreshold && chargePull >= chargeThreshold)
        {
            bow.PlayPullSound(chargeLevel, 0.5f, 0.2f);
        }
        else if (previousChargePull < chargeTwoThreshold && chargePull >= chargeTwoThreshold)
        {
            chargeLevel = 2;
            bow.PlayPullSound(chargeLevel, 0.5f, 0.2f);
            bow.GlowPulse(1, 10, 5, false);
            currentArrow.GetComponent<Arrow>().PlayAirConeSound(0.2f);
        }
        else if (previousChargePull < chargeMaxThreshold && chargePull >= chargeMaxThreshold)
        {
            chargeLevel = 3;
            bow.PlayPullSound(chargeLevel, 0.5f, 0.2f);
            bow.GlowPulse(1, 30, 5, true);
            isShaking = true;
        }
        else if (previousPull > chargeThreshold && currentPull <= chargeThreshold)
        {
            chargeLevel = 1;
            bow.StopSound(); // Stop pull hold sound loop
            bow.StopGlow();
            currentArrow.GetComponent<Arrow>().StopSound();
            isShaking = false;
        }

        previousPull = currentPull;
        previousChargePull = chargePull;
    }
}
