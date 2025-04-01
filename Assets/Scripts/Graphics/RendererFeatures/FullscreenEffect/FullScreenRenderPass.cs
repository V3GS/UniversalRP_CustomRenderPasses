using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class FullScreenRenderPass : ScriptableRenderPass
{
    const string k_PassName = "FullScreenPass";
    Material m_BlitMaterial = null;

    public FullScreenRenderPass(FullscreenRendererFeature.Settings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        m_BlitMaterial = settings.blitMaterial;

        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        VolumeStack stack = VolumeManager.instance.stack;
        FullScreenVolumeComponent customVolume = stack.GetComponent<FullScreenVolumeComponent>();

        if (!customVolume.IsActive()) return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError($"Skipping render pass. FullScreenRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input.");
            return;
        }

        // Get the source texture
        var source = resourceData.activeColorTexture;

        // Create the destination texture
        var destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = $"CameraColor-{k_PassName}";
        destinationDesc.clearBuffer = false;

        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        // Create blit parameters and Blit
        RenderGraphUtils.BlitMaterialParameters para = new RenderGraphUtils.BlitMaterialParameters(source, destination, m_BlitMaterial, 0);
        renderGraph.AddBlitPass(para, passName: k_PassName);

        // Asign to the color texture the destination value after the blit
        resourceData.cameraColor = destination;
    }
}
