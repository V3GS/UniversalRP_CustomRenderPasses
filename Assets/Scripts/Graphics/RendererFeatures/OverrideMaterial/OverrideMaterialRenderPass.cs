using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class OverrideMaterialRenderPass : ScriptableRenderPass
{
    private string m_ProfilerTag;
    private Material m_OverrideMaterial;
    
    private FilteringSettings m_FilteringSettings;
    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    public OverrideMaterialRenderPass(OverrideMaterialRenderPassFeature.Settings settings, string tag)
    {
        renderPassEvent = settings.renderPassEvent;
        m_ProfilerTag = tag;

        m_OverrideMaterial = settings.overrideMaterial;
        
        m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);
        
        m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
        m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
        m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
    }
    
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        
        RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        opaqueDescriptor.depthBufferBits = 0;
        
        using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
        {
            var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
            var drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);
            drawSettings.perObjectData = PerObjectData.None;
            
            drawSettings.overrideMaterial = m_OverrideMaterial;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
        }
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
    
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }
}
