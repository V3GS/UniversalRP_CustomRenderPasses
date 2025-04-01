using Mono.Cecil;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    private const string k_PassName = "FullScreenPass";
    private Material m_BlurMaterial = null;

    private const string k_BlurTextureName = "_BlurTexture";
    private RenderTextureDescriptor m_BlurTextureDescriptor;

    // Reference to shader properties IDs.
    static readonly int m_GrayscaleProperty = Shader.PropertyToID("_Grayscale");
    static readonly int m_BlurXId = Shader.PropertyToID("_BlurX");
    static readonly int m_BlurYId = Shader.PropertyToID("_BlurY");

    public bool m_Grayscale = false;
    private float m_BlurXValue = 0.0f;
    private float m_BlurYValue = 0.0f;

    public BlurRenderPass(BlurRendererFeature.Settings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        m_BlurMaterial = settings.blurMaterial;

        m_Grayscale = settings.grayscale;
        m_BlurXValue = settings.blurX;
        m_BlurYValue = settings.blurY;

        requiresIntermediateTexture = true;

        m_BlurTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle source = resourceData.activeColorTexture;
        TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, m_BlurTextureDescriptor, k_BlurTextureName, false);

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError($"Skipping render pass. BlurRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
            return;
        }

        // Set the blur texture size to be the same as the camera target size.
        m_BlurTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
        m_BlurTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
        m_BlurTextureDescriptor.depthBufferBits = 0;

        UpdateBlurSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!source.IsValid() || !destination.IsValid())
            return;

        // Create blit parameters and Blit
        RenderGraphUtils.BlitMaterialParameters para = new RenderGraphUtils.BlitMaterialParameters(source, destination, m_BlurMaterial, 0);
        renderGraph.AddBlitPass(para, passName: k_PassName);

        // Asign to the color texture the destination value after the blit
        resourceData.cameraColor = destination;
    }

    private void UpdateBlurSettings()
    {
        if (m_BlurMaterial == null) return;

        BlurVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<BlurVolumeComponent>();

        bool isGrayscale = volumeComponent.grayscaleEffect.overrideState ? volumeComponent.grayscaleEffect.value : m_Grayscale;
        float horizontalBlur = volumeComponent.horizontalBlur.overrideState ? volumeComponent.horizontalBlur.value : m_BlurXValue;
        float verticalBlur = volumeComponent.verticalBlur.overrideState ? volumeComponent.verticalBlur.value : m_BlurYValue;

        // Set any material properties based on our pass settings
        m_BlurMaterial.SetInt(m_GrayscaleProperty, Convert.ToInt32(isGrayscale));
        m_BlurMaterial.SetFloat(m_BlurXId, horizontalBlur);
        m_BlurMaterial.SetFloat(m_BlurYId, verticalBlur);
    }
}
