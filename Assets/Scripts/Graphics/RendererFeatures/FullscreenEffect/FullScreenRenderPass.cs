using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenRenderPass : ScriptableRenderPass
{
    string m_ProfilerTag;
    private Material m_BlitMaterial = null;
    private RenderTargetIdentifier m_Source { get; set; }
    RenderTargetHandle m_TemporaryColorTexture;

    public FullScreenRenderPass(FullscreenRendererFeature.Settings settings, string tag)
    {
        renderPassEvent = settings.renderPassEvent;
        m_ProfilerTag = tag;
        m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        
        m_BlitMaterial = settings.blitMaterial;
    }
     
    public void Setup(RenderTargetIdentifier sourceRenderTargetIdentifier)
    {
        this.m_Source = sourceRenderTargetIdentifier;
    }
     
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
        {
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // It can't read and write to same color target, for that reason is necessary a temporary render texture
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc);

            Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_BlitMaterial);
            Blit(cmd, m_TemporaryColorTexture.Identifier(), m_Source);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
         
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
    }
}
