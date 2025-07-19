using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fades between two horizontal skybox materials by extracting their textures
/// and blending via a custom blend material (Custom/Skybox/PanoramicBlend_2D).
/// </summary>
public class SkyboxMaterialFader : MonoBehaviour
{
    [Tooltip("Original Hub Skybox Material")]
    public Material skyboxMaterialA;
    [Tooltip("Cleanroom Skybox Material")]
    public Material skyboxMaterialB;
    [Tooltip("Blend Material with Custom/Skybox/PanoramicBlend_2D shader")]
    public Material blendSkyboxMaterial;
    [Tooltip("Duration of the fade in seconds")]
    public float fadeDuration = 2f;

    private float timer;
    private bool fading;
    private float startBlend;
    private float targetBlend;

    void Start()
    {
        if (skyboxMaterialA == null || skyboxMaterialB == null || blendSkyboxMaterial == null)
        {
            Debug.LogError("SkyboxMaterialFader: Assign skyboxMaterialA, skyboxMaterialB, and blendSkyboxMaterial.");
            enabled = false;
            return;
        }

        // At start, use the original Hub skybox
        RenderSettings.skybox = skyboxMaterialA;
        DynamicGI.UpdateEnvironment();
    }

    void Update()
    {
        if (!fading) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeDuration);
        float blend = Mathf.Lerp(startBlend, targetBlend, t);
        blendSkyboxMaterial.SetFloat("_Blend", blend);

        if (t >= 1f)
        {
            fading = false;
            // Ensure final blend
            blendSkyboxMaterial.SetFloat("_Blend", targetBlend);
            DynamicGI.UpdateEnvironment();

            // If we faded back to 0, restore the original material
            if (Mathf.Approximately(targetBlend, 0f))
            {
                RenderSettings.skybox = skyboxMaterialA;
                DynamicGI.UpdateEnvironment();
            }
        }
    }

    /// <summary>Fade from Hub (A) to Cleanroom (B).</summary>
    public void FadeToCleanroom()
    {
        StartFade(1f);
    }

    /// <summary>Fade from Cleanroom (B) back to Hub (A).</summary>
    public void FadeToHub()
    {
        StartFade(0f);
    }

    private void StartFade(float toBlend)
    {
        // On first fade, initialize blend material textures and switch skybox
        if (RenderSettings.skybox != blendSkyboxMaterial)
        {
            // Extract textures from A and B
            Texture texA = skyboxMaterialA.GetTexture("_MainTex") ?? skyboxMaterialA.GetTexture("_Tex");
            Texture texB = skyboxMaterialB.GetTexture("_MainTex") ?? skyboxMaterialB.GetTexture("_Tex");
            blendSkyboxMaterial.SetTexture("_MainTex1", texA);
            blendSkyboxMaterial.SetTexture("_MainTex2", texB);
            blendSkyboxMaterial.SetFloat("_Blend", startBlend = RenderSettings.skybox == skyboxMaterialA ? 0f : 1f);

            RenderSettings.skybox = blendSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        startBlend = blendSkyboxMaterial.GetFloat("_Blend");
        targetBlend = toBlend;
        timer = 0f;
        fading = true;
    }
}
