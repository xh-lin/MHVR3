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
    public GameObject arrowPrefab;
    public GameObject snapHandle;   // for calculating bow aiming rotation
    // vibration
    [Range(0f, 1f)]
    public float bowVibration = 0.1f;
    [Range(0f, 1f)]
    public float stringVibration = 0.2f;
    // shake
    [Range(0f, 30f)]
    public float shakeSpeed = 10f;
    [Range(0f, 0.1f)]
    public float shakeMultiplier = 0.01f;
    // spread shot
    [Range(0f, 1f)] // 90 ~ 0 degree
    public float horizontalThreshold = 0.85f; // 0.85 ~> 30°, for determining horizontal bow holding

    private GameObject currentArrow;
    private BowHandle handle;

    private VRTK_InteractableObject interact;
    private VRTK_InteractGrab holdControl;
    private VRTK_InteractGrab stringControl;

    // aim
    private Quaternion releaseRotation;
    private Quaternion baseRotation;
    private bool isFired;
    private bool gotBaseRotation;
    private float fireOffset;
    private float currentPull;
    private float previousPull;

    // charge pull
    private int chargeLevel;
    private const int maxChargeLevel = 3;
    private float chargeTimer;
    private const float chargeTime = 2f;       // time required to charge one level in seconds
    private const float chargeThreshold = 0.2f; // minimum pull distance to start charging
    private bool isVibrating;

    // shaking
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
        bow = GetComponent<Bow>();
        handle = GetComponentInChildren<BowHandle>();
        interact = GetComponent<VRTK_InteractableObject>();
        interact.InteractableObjectGrabbed += new InteractableObjectEventHandler(DoObjectGrab);

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
        if (currentArrow != null && IsHeld()) {
            AimArrow();
            AimBow();
            PullString();
            // release arrow
            if (!stringControl.IsGrabButtonPressed()) {
                coatingColor = (coating == null) ? Color.clear : coating.Consume();
                currentArrow.GetComponent<Arrow>().Fired(coatingColor);
                isFired = true;
                releaseRotation = transform.localRotation;
                Release();
            }
        } else if (IsHeld()) {  // recover bow rotation after releasing arrow
            if (isFired) {
                isFired = false;
                fireOffset = Time.time;
            }
            if (gotBaseRotation && transform.localRotation != baseRotation) {
                transform.localRotation = Quaternion.Lerp(releaseRotation, baseRotation, 
                    (Time.time - fireOffset) * 8);
            }
        }

        // release arrow if drop bow while arrow loaded
        if (!IsHeld()) {
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
        if (VRTK_DeviceFinder.IsControllerLeftHand(e.interactingObject)) {
            holdControl = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_InteractGrab>();
            stringControl = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_InteractGrab>();
        } else {
            stringControl = VRTK_DeviceFinder.GetControllerLeftHand().GetComponent<VRTK_InteractGrab>();
            holdControl = VRTK_DeviceFinder.GetControllerRightHand().GetComponent<VRTK_InteractGrab>();
        }
        StartCoroutine(nameof(GetBaseRotation));
    }

    private IEnumerator GetBaseRotation()
    {
        yield return new WaitForEndOfFrame();
        baseRotation = transform.localRotation;
        gotBaseRotation = true;
    }

    private void Release()
    {
        currentArrow.transform.SetParent(null);

        // let arrow colliders ignore bow colliders
        Collider[] arrowCols = currentArrow.GetComponentsInChildren<Collider>();
        Collider[] BowCols = GetComponentsInChildren<Collider>();
        foreach (var ac in arrowCols) {
            ac.enabled = true;
            foreach (var bc in BowCols)
                Physics.IgnoreCollision(ac, bc);
        }

        // setup arrow collision and move speed
        currentArrow.GetComponent<Rigidbody>().isKinematic = false;
        currentArrow.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        currentArrow.GetComponent<Rigidbody>().velocity = currentPull * powerMultiplier * 
            currentArrow.transform.TransformDirection(Vector3.forward);

        currentArrow.GetComponent<Arrow>().InFlight();
        currentArrow.GetComponent<Arrow>().StopSound(); // stop charging sound

        Vector3 bowUp = transform.TransformDirection(Vector3.up);
        if (Vector3.ProjectOnPlane(bowUp, Vector3.up).magnitude > horizontalThreshold) {
            // spread shot, if the bow is holding horizontally
            DuplicateArrow(new Vector3(0.1f, 0f, 1f));
            DuplicateArrow(new Vector3(-0.1f, 0f, 1f));
            currentArrow.GetComponent<Arrow>().PlaySpreadShotSound(chargeLevel, 0.2f);
        } else {
            // charged shot
            switch (chargeLevel) {
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
        ReleaseArrow();

        // reset pull variables
        currentPull = 0f;
        previousPull = 0f;
        chargeLevel = 1;
        isShaking = false;

        // stop pull effects
        bow.SetPullAnimation(0);
        bow.StopGlow();
        bow.StopSound();
        if (isVibrating) {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }

        bow.PlayShotSound(0.5f);
    }

    private void DuplicateArrow(Vector3 direction)
    {
        // create a new arrow from prefab
        GameObject newArrowNotch = Instantiate(arrowPrefab, currentArrow.transform.position, 
            currentArrow.transform.rotation);
        GameObject newArrow = newArrowNotch.GetComponent<ArrowNotch>().arrow;

        newArrowNotch.GetComponent<ArrowNotch>().CopyNotchToArrow();
        newArrow.transform.SetParent(null);

        // let arrow colliders ignore bow colliders
        Collider[] arrowCols = newArrow.GetComponentsInChildren<Collider>();
        Collider[] BowCols = GetComponentsInChildren<Collider>();
        foreach (var ac in arrowCols) {
            ac.enabled = true;
            foreach (var bc in BowCols)
                Physics.IgnoreCollision(ac, bc);
        }

        // setup arrow collision and move speed
        newArrow.GetComponent<Rigidbody>().isKinematic = false;
        newArrow.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        newArrow.GetComponent<Rigidbody>().velocity = currentPull * powerMultiplier * 
            currentArrow.transform.TransformDirection(direction);

        newArrow.GetComponent<Arrow>().InFlight();

        // apply coating particle effect on duplicated arrow
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

        if (isShaking) {
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

        if (isShaking) {
            var x = (Mathf.PerlinNoise(bowSeedX, Time.time * shakeSpeed) - 0.5f) * shakeMultiplier;
            var y = (Mathf.PerlinNoise(bowSeedY, Time.time * shakeSpeed) - 0.5f) * shakeMultiplier;
            lookDir += new Vector3(x, y, 0);
        }

        transform.rotation = Quaternion.LookRotation(lookDir, upDir);
    }

    private void PullString()
    {
        float controllerDist = Vector3.Distance(holdControl.transform.position, stringControl.transform.position);
        currentPull = Mathf.Clamp((controllerDist - pullOffset) * pullMultiplier, 0, maxPullDistance);

        bow.SetPullAnimation(currentPull);

        // controllers vibrate when pulling
        if (currentPull.ToString("F2") != previousPull.ToString("F2")) {
            if (VRTK_DeviceFinder.IsControllerRightHand(holdControl.gameObject)) {
                OVRInput.SetControllerVibration(.1f, bowVibration, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(.1f, stringVibration, OVRInput.Controller.LTouch);
            } else {
                OVRInput.SetControllerVibration(.1f, stringVibration, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(.1f, bowVibration, OVRInput.Controller.LTouch);
            }
            isVibrating = true;
        } else if (isVibrating) {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }

        // pull and hold to charge
        if (currentPull > chargeThreshold) {
            if (previousPull < chargeThreshold) {   // start charging
                bow.PlayStringStretchSound(0.2f);
                bow.PlayPullSound(chargeLevel, 0.5f, 0.2f);
            }
            if (chargeLevel < maxChargeLevel) {     // continue charging till max
                chargeTimer += Time.deltaTime;
                if (chargeTimer > chargeTime) {
                    chargeTimer = 0f;
                    chargeLevel += 1;
                    bow.PlayPullSound(chargeLevel, 0.5f, 0.2f);
                    // charge level up effects
                    if (chargeLevel == 2) {
                        bow.GlowPulse(1, 10, 5, false);
                        currentArrow.GetComponent<Arrow>().PlayAirConeSound(0.2f);
                    } else {    // chargeLevel == 3
                        bow.GlowPulse(1, 30, 5, true);
                        isShaking = true;
                    }
                }
            }
        } else if (previousPull > chargeThreshold) {    // cancel charging
            chargeTimer = 0f;
            chargeLevel = 1;
            bow.StopGlow();
            bow.StopSound();    // Stop pull hold sound loop
            currentArrow.GetComponent<Arrow>().StopSound();
            isShaking = false;
        }

        previousPull = currentPull;
    }
}
