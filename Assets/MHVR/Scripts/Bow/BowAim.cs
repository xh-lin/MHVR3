using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class BowAim : MonoBehaviour
{
    // pulling position
    public float pullOffset = 0.214f;
    public float pullMultiplier = 1.0f;

    public Image progressRingImage;
    public ArrowSpawner nockingPointArrowSpawner;
    public GameObject snapPoint;                // for calculating aiming rotation
    public GameObject arrowProjectilePrefab;    // should contains P_Arrow script

    [HideInInspector]
    public Bow bow;
    [HideInInspector]
    public BowCoating coating;

    private VRTK_InteractGrab holdControl;
    private VRTK_InteractGrab stringControl;
    private VRTK_InteractableObject interact;
    private BowHandle handle;
    private GameObject loadedArrowGO;
    private ArrowObject loadedArrowObject;

    // aiming
    private Quaternion releaseRotation;
    private Quaternion baseRotation;
    private bool gotBaseRotation;
    private bool isShooted;
    private float shotOffset;

    // shaking
    private const float kShakeSpeed = 10f;
    private const float kShakeMagnitude = 0.01f;
    private float arrowSeedX;
    private float arrowSeedY;
    private float bowSeedX;
    private float bowSeedY;
    private bool isShaking = false;

    // pull & charge
    private const int kMaxChargeLevel = 3;
    private int chargeLevel = 1;
    private const float kChargeHoldTime = 1f;    // time required to charge a level when holding
    private float chargeHoldTimer;
    private const float kRapidTime = 3f;        // time window to pull again after shooted
    private float rapidTimer;
    private const float kMinPullDist = 0.2f;    // minimum pull distance
    private float currentPull;
    private float previousPull;

    // shooting
    private const float horDeg = 30;         // minimum degree determining horizontal bow holding
    private float horProj;
    private const float arrowSpeedMult = 40f;
    private readonly float[] kShootLife = { 0.7f, 0.7f, 0.7f };
    private readonly float[] kShootSpeed = { 1f, 1f, 1.3f };
    private readonly float[] kPowerShotLife = { 0.4f, 0.4f, 0.3f };
    private readonly float[] kPowerShotSpeed = { 0.6f, 1f, 1f };

    // vibration
    private const float kBowVibration = 0.1f;       // vibration of controller grabbing the bow
    private const float kStringVibration = 0.2f;    // vibration of controller grabbing the arrow
    private bool isVibrating = false;
    private readonly Vector3[][] kShootDir = {      // shoot arrow spawn directions
        new Vector3[] {
            new Vector3(0f, 0f, 1f)
        },
        new Vector3[] {
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, -0.01f, 1f)
        },
        new Vector3[] {
            new Vector3(0f, -0.005f, 1f),
            new Vector3(0.01f, 0.005f, 1f),
            new Vector3(-0.01f, 0.005f, 1f)
        }
    };
    private readonly Vector3[][] kPowerShotDir = {  // power shot arrow spawn directions
        new Vector3[] {
            new Vector3(0f, 0f, 1f),
            new Vector3(0.1f, 0f, 1f),
            new Vector3(-0.1f, 0f, 1f)
        },
        new Vector3[] {
            new Vector3(0.02f, 0f, 1f),
            new Vector3(-0.02f, 0f, 1f),
            new Vector3(0.13f, 0f, 1f),
            new Vector3(-0.13f, 0f, 1f),
            new Vector3(0.25f, 0f, 1f),
            new Vector3(-0.25f, 0f, 1f),
        }
    };

    private void Start()
    {
        bow = GetComponent<Bow>();
        handle = GetComponentInChildren<BowHandle>();
        interact = GetComponent<VRTK_InteractableObject>();

        interact.InteractableObjectGrabbed += new InteractableObjectEventHandler(DoObjectGrab);
        arrowSeedX = Random.value * 10f;
        arrowSeedY = Random.value * 10f;
        bowSeedX = Random.value * 10f;
        bowSeedY = Random.value * 10f;
        horProj = Mathf.Cos(Mathf.Deg2Rad * horDeg);
    }

    /// <summary>
    /// For determining which controller is grabbing the bow/arrow.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
        gotBaseRotation = true;
    }

    private void Update()
    {
        // holding bow with arrow loaded
        if (loadedArrowGO != null && IsHeld())
        {
            AimArrow();
            AimBow();
            PullString();
            // release arrow
            if (!stringControl.IsGrabButtonPressed())
            {
                isShooted = true;
                releaseRotation = transform.localRotation;
                if (currentPull < kMinPullDist)
                {
                    UnloadArrow();
                }
                else
                {
                    Release();
                }
            }
        }
        else if (IsHeld())  // recover bow rotation after releasing arrow
        {
            if (isShooted)
            {
                isShooted = false;
                shotOffset = Time.time;
            }
            if (gotBaseRotation && transform.localRotation != baseRotation)
            {
                transform.localRotation = Quaternion.Lerp(releaseRotation, baseRotation, (Time.time - shotOffset) * 8);
            }
        }

        // release arrow if drop bow while arrow loaded
        if (!IsHeld() && loadedArrowGO != null)
        {
            UnloadArrow();
        }

        // can pull again to shoot without nocking an arrow within a time window
        if (rapidTimer >= 0)
        {
            if (!nockingPointArrowSpawner.enabled)
            {
                nockingPointArrowSpawner.enabled = true;
            }
            rapidTimer -= Time.deltaTime;
            progressRingImage.fillAmount = rapidTimer / kRapidTime; // update the progress ring
        }
        else if (nockingPointArrowSpawner.enabled)
        {
            nockingPointArrowSpawner.enabled = false;
            progressRingImage.fillAmount = 0; // update the progress ring
        }
    }

    public bool IsHeld()
    {
        return interact.IsGrabbed();
    }

    public bool HasArrow()
    {
        return loadedArrowGO != null;
    }

    /// <summary>
    /// Update arrow's rotation when aiming.
    /// </summary>
    private void AimArrow()
    {
        Vector3 lookDir = handle.nockSide.position;

        if (isShaking)
        {
            var x = (Mathf.PerlinNoise(arrowSeedX, Time.time * kShakeSpeed) - 0.5f) * kShakeMagnitude;
            var y = (Mathf.PerlinNoise(arrowSeedY, Time.time * kShakeSpeed) - 0.5f) * kShakeMagnitude;
            lookDir += new Vector3(x, y, 0);
        }

        loadedArrowGO.transform.LookAt(lookDir);
    }

    /// <summary>
    /// Update bow's rotation when aiming.
    /// </summary>
    private void AimBow()
    {
        Vector3 lookDir = holdControl.transform.position - stringControl.transform.position;
        Vector3 upDir = holdControl.transform.TransformDirection(Quaternion.Euler(-snapPoint.transform.localEulerAngles) * Vector3.up);

        if (isShaking)
        {
            var x = (Mathf.PerlinNoise(bowSeedX, Time.time * kShakeSpeed) - 0.5f) * kShakeMagnitude;
            var y = (Mathf.PerlinNoise(bowSeedY, Time.time * kShakeSpeed) - 0.5f) * kShakeMagnitude;
            lookDir += new Vector3(x, y, 0);
        }

        transform.rotation = Quaternion.LookRotation(lookDir, upDir);
    }

    public void SetArrow(GameObject arrow)
    {
        loadedArrowGO = arrow;
        loadedArrowObject = loadedArrowGO.GetComponent<ArrowObject>();
        bow.PlaySetArrowAudio(0.5f);

        // charge level loop if rapid shot
        if (rapidTimer > 0 && chargeLevel != kMaxChargeLevel)
        {
            chargeLevel++;
        }
        else
        {
            chargeLevel = 1;
        }

        // reset rapid shot
        rapidTimer = 0;
    }

    private void PullString()
    {
        float controllerDist = Vector3.Distance(holdControl.transform.position, stringControl.transform.position);
        currentPull = Mathf.Clamp((controllerDist - pullOffset) * pullMultiplier, 0, 1);

        bow.SetPullAnimation(currentPull);

        // if is pulling
        if (currentPull > kMinPullDist)
        {
            // if just start to pull
            if (previousPull < kMinPullDist)
            {
                PlayChargeEffect();
            }

            // continue charging till max
            if (chargeLevel < kMaxChargeLevel)
            {
                if (chargeHoldTimer > kChargeHoldTime)
                {
                    chargeLevel++;
                    PlayChargeEffect();
                    chargeHoldTimer = 0;
                }

                chargeHoldTimer += Time.deltaTime;
            }
        }
        // if canceled pulling
        else if (previousPull > kMinPullDist)
        {
            chargeHoldTimer = 0;
            chargeLevel = 1;
            bow.StopGlow();
            bow.StopAudio();
            loadedArrowObject.StopAudio();
            isShaking = false;
        }

        // controller vibration when pulling
        if (currentPull > kMinPullDist && currentPull.ToString("F2") != previousPull.ToString("F2"))
        {
            if (VRTK_DeviceFinder.IsControllerRightHand(holdControl.gameObject))
            {
                OVRInput.SetControllerVibration(.1f, kBowVibration, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(.1f, kStringVibration, OVRInput.Controller.LTouch);
            }
            else
            {
                OVRInput.SetControllerVibration(.1f, kStringVibration, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(.1f, kBowVibration, OVRInput.Controller.LTouch);
            }
            isVibrating = true;
        }
        // stop vibration when not pulling
        else if (isVibrating)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }

        previousPull = currentPull;
    }

    private void PlayChargeEffect()
    {
        if (chargeLevel <= 0 || chargeLevel > kMaxChargeLevel)
        {
            Debug.LogError("Invalid charge level: " + chargeLevel);
        }

        bow.PlayPullAudio(chargeLevel, 0.5f, 0.2f);

        switch (chargeLevel)
        {
            case 1:
                bow.PlayStringStretchAudio(0.2f);
                break;
            case 2:
                bow.GlowPulse(1, 10, 5, true);
                loadedArrowObject.PlayAirConeAudio(0.2f);
                break;
            default:
                bow.GlowPulse(1, 30, 5, true);
                isShaking = true;
                break;
        }
    }

    private void Release()
    {
        Vector3 bowUp = transform.up;
        Vector3 headUp = VRTK_DeviceFinder.HeadsetTransform().up;

        // if the bow is holding horizontally (degree smaller than horDegree)
        if (Vector3.ProjectOnPlane(bowUp, headUp).magnitude > horProj)
        {
            PowerShot();
        }
        else
        {
            Shoot();
        }

        if (stringControl)
        {
            stringControl.ForceRelease();
        }

        // stop vibration
        if (isVibrating)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }

        // reset variables
        currentPull = 0f;
        previousPull = 0f;
        isShaking = false;
        rapidTimer = kRapidTime;
        chargeHoldTimer = 0;

        // effects
        loadedArrowObject.StopAudio(); // stop air cone audio
        bow.SetPullAnimation(0);
        bow.StopGlow();
        bow.StopAudio();
        bow.PlayShotAudio(0.5f);

        Destroy(loadedArrowGO); // loadedArrowGO and loadedArrowObject will now be null
    }

    private void Shoot()
    {
        Color coatingColor = (coating == null) ? Color.clear : coating.Consume();
        float life = kShootLife[chargeLevel - 1];
        float speed = kShootSpeed[chargeLevel - 1] * arrowSpeedMult;

        loadedArrowObject.PlayShootAudio(chargeLevel, 0.2f);

        switch (chargeLevel)
        {
            case 1:
                SpawnArrowProjectile(kShootDir[0][0], coatingColor, life, speed);
                break;
            case 2:
                SpawnArrowProjectile(kShootDir[1][0], coatingColor, life, speed);
                SpawnArrowProjectile(kShootDir[1][1], coatingColor, life, speed);
                break;
            default:
                SpawnArrowProjectile(kShootDir[2][0], coatingColor, life, speed);
                SpawnArrowProjectile(kShootDir[2][1], coatingColor, life, speed);
                SpawnArrowProjectile(kShootDir[2][2], coatingColor, life, speed);
                break;
        }
    }

    private void PowerShot()
    {
        Color coatingColor = (coating == null) ? Color.clear : coating.Consume();
        float life = kPowerShotLife[chargeLevel - 1];
        float speed = kPowerShotSpeed[chargeLevel - 1] * arrowSpeedMult;

        loadedArrowObject.PlayPowerShotAudio(chargeLevel, 0.2f);

        switch (chargeLevel)
        {
            case 1:
                SpawnArrowProjectile(kPowerShotDir[0][0], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[0][1], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[0][2], coatingColor, life, speed);
                break;
            case 2:
                SpawnArrowProjectile(kPowerShotDir[0][0], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[0][1], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[0][2], coatingColor, life, speed);
                break;
            default:
                SpawnArrowProjectile(kPowerShotDir[1][0], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[1][1], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[1][2], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[1][3], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[1][4], coatingColor, life, speed);
                SpawnArrowProjectile(kPowerShotDir[1][5], coatingColor, life, speed);
                break;
        }
    }

    private void SpawnArrowProjectile(Vector3 direction, Color coatingColor, float life, float speed)
    {
        // create a new arrow projectile from prefab
        GameObject newArrow = Instantiate(arrowProjectilePrefab, loadedArrowGO.transform.position, loadedArrowGO.transform.rotation);

        // let arrow colliders ignore bow colliders
        Collider arrowCol = newArrow.GetComponent<Collider>();
        Collider[] BowCols = GetComponentsInChildren<Collider>();
        foreach (var bc in BowCols)
        {
            Physics.IgnoreCollision(arrowCol, bc);
        }

        // setup arrow collision and move speed
        newArrow.GetComponent<Rigidbody>().isKinematic = false;
        newArrow.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        newArrow.GetComponent<Rigidbody>().velocity = loadedArrowGO.transform.TransformDirection(direction) * speed;

        newArrow.GetComponent<ArrowProjectile>().Shooted(coatingColor, life);
    }

    private void UnloadArrow()
    {
        if (loadedArrowGO)
        {
            if (stringControl)
            {
                stringControl.ForceRelease();
            }

            loadedArrowGO.transform.SetParent(null);
            loadedArrowGO = null;
            loadedArrowObject = null;
        }
    }
}
