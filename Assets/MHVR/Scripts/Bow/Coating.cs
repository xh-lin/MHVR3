using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coating : MonoBehaviour
{
    public enum Type { CloseRange, Power, Poison, Paralysis, Sleep, Blast };

    public Type type;
    [Range(0, 50)]
    public int quantity = 20;
    public SoundBank bowPhysicalSFX;

    private AudioSource audioSource;
    private Renderer[] bowRenderers;
    private Dictionary<Type, Color> colorOfType = new Dictionary<Type, Color>()
    {
        { Type.CloseRange, new Color(1f, 1f, 1f) },
        { Type.Power, new Color(0.8666667f, 0.3019608f, 0.3764706f) },
        { Type.Poison, new Color(0.5921569f, 0.5411765f, 0.8078432f) },
        { Type.Paralysis, new Color(0.8745099f, 0.8980393f, 0.01960784f) },
        { Type.Sleep, new Color(0.3176471f, 0.764706f, 0.8941177f) },
        { Type.Blast, new Color(0.5372549f, 0.7019608f, 0.2823529f) }
    };

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        bowRenderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in bowRenderers)
        {
            renderer.material.SetColor("_coatingColor", colorOfType[type]);
        }
    }

    public Color Consume()
    {
        if (quantity > 0)
        {
            quantity--;
            return colorOfType[type];
        }
        return Color.clear;
    }

    // === Play sound

    public void PlayApplySound(float volumeScale)
    {
        audioSource.PlayOneShot(bowPhysicalSFX.audio[30].clip, volumeScale);
    }

    public void PlayRemoveSound(float volumeScale)
    {
        audioSource.PlayOneShot(bowPhysicalSFX.audio[4].clip, volumeScale);
    }

    // ===
}
