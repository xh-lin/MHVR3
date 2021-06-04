// Mofidied from VRTK.Examples.Archery.ArrowSpawner

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ArrowSpawner : MonoBehaviour
{
    public float spawnDelay = 1f;
    public GameObject arrowPrefab;
    public SoundBank bowPhysicalSFX;
    public Outline outline;

    private BowAim aim;
    private AudioSource source;
    private float spawnDelayTimer;

    private int[] grabArrowSounds;
    private int grabArrowSoundsIdx;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        outline.enabled = false;
        spawnDelayTimer = 0f;
        grabArrowSounds = new int[] { 5, 7, 16, 33 };
        grabArrowSoundsIdx = 0;

        Collider spawnerCol = GetComponent<Collider>();
        Collider[] quiverCols = transform.parent.gameObject.GetComponentsInChildren<Collider>();
        foreach (var c in quiverCols)
        {
            if(spawnerCol != c)
                Physics.IgnoreCollision(spawnerCol, c);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        VRTK_InteractGrab grabbingController = (collider.gameObject.GetComponent<VRTK_InteractGrab>() ? 
            collider.gameObject.GetComponent<VRTK_InteractGrab>() : 
            collider.gameObject.GetComponentInParent<VRTK_InteractGrab>());
        if (CanGrab(grabbingController) && NoArrowNotched(grabbingController.gameObject) && Time.time >= spawnDelayTimer)
        {
            GameObject newArrow = Instantiate(arrowPrefab);
            newArrow.name = "ArrowClone";
            grabbingController.GetComponent<VRTK_InteractTouch>().ForceTouch(newArrow);
            grabbingController.AttemptGrab();
            spawnDelayTimer = Time.time + spawnDelay;

            PlayGrabSound(0.5f);
        }

        if(!outline.enabled && grabbingController && grabbingController.GetGrabbedObject() == null)
        {
            source.PlayOneShot(bowPhysicalSFX.audio[10].clip, 0.3f);
            outline.enabled = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (outline.enabled)
            outline.enabled = false;
    }

    private bool CanGrab(VRTK_InteractGrab grabbingController)
    {
        return (grabbingController && grabbingController.GetGrabbedObject() == null && grabbingController.IsGrabButtonPressed());
    }

    private bool NoArrowNotched(GameObject controller)
    {
        if (VRTK_DeviceFinder.IsControllerLeftHand(controller))
        {
            GameObject controllerRightHand = VRTK_DeviceFinder.GetControllerRightHand(true);
            aim = controllerRightHand.GetComponentInChildren<BowAim>();
            if (aim == null)
            {
                aim = VRTK_DeviceFinder.GetModelAliasController(controllerRightHand).GetComponentInChildren<BowAim>();
            }
        }
        else if (VRTK_DeviceFinder.IsControllerRightHand(controller))
        {
            GameObject controllerLeftHand = VRTK_DeviceFinder.GetControllerLeftHand(true);
            aim = controllerLeftHand.GetComponentInChildren<BowAim>();
            if (aim == null)
            {
                aim = VRTK_DeviceFinder.GetModelAliasController(controllerLeftHand).GetComponentInChildren<BowAim>();
            }
        }

        return (aim == null || !aim.HasArrow());
    }

    // === Play sound

    private void PlayGrabSound(float volumeScale)
    {
        source.PlayOneShot(bowPhysicalSFX.audio[grabArrowSounds[grabArrowSoundsIdx]].clip, volumeScale);
        grabArrowSoundsIdx = (grabArrowSoundsIdx + 1) % grabArrowSounds.Length;
    }

    // ===
}
