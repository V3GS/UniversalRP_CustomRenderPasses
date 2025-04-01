using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

class OverrideMaterialRenderPass : ScriptableRenderPass
{
    const string k_PassName = "FullScreenPass";
    private Material m_OverrideMaterial;
    private LayerMask m_LayerMask;

    private FilteringSettings m_FilteringSettings;
    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    private class PassData
    {
        public RendererListHandle rendererListHandle;
    }

    public OverrideMaterialRenderPass(OverrideMaterialRenderPassFeature.Settings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        m_LayerMask = settings.layerMask;
        m_OverrideMaterial = settings.overrideMaterial;
    }

    private void InitRendererLists(ContextContainer frameData, ref PassData passData, RenderGraph renderGraph)
    {
        // Access the relevant frame data from the Universal Render Pipeline
        UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalLightData lightData = frameData.Get<UniversalLightData>();

        SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
        RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
        FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

        m_ShaderTagIdList.Clear();

        m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));

        DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, universalRenderingData, cameraData, lightData, sortFlags);

        // Add the override material to the drawing settings
        drawSettings.overrideMaterial = m_OverrideMaterial;

        var param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
        passData.rendererListHandle = renderGraph.CreateRendererList(param);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassName, out var passData))
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            InitRendererLists(frameData, ref passData, renderGraph);

            builder.UseRendererList(passData.rendererListHandle);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));

            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Clear the render target to black
        context.cmd.ClearRenderTarget(false, false, Color.black);

        // Draw the objects in the list
        context.cmd.DrawRendererList(data.rendererListHandle);
    }
}
