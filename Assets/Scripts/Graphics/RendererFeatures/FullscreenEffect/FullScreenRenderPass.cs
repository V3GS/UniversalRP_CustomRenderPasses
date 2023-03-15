using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenRenderPass : ScriptableRenderPass
{
    string m_ProfilerTag;
    Material m_BlitMaterial = null;
    bool m_GenerateTemporaryColorTexture = false;

    RTHandle m_Source;
    RTHandle m_TemporaryColorTexture;

    public FullScreenRenderPass(FullscreenRendererFeature.Settings settings, string tag)
    {
        renderPassEvent = settings.renderPassEvent;
        m_ProfilerTag = tag;
        m_GenerateTemporaryColorTexture = settings.GenerateTemporaryColorTexture;

        m_BlitMaterial = settings.blitMaterial;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_Source);
    }

    public void Setup(RTHandle destinationColor, in RenderingData renderingData)
    {
        m_Source = destinationColor;

        if (m_GenerateTemporaryColorTexture)
        {
            var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref m_TemporaryColorTexture, colorCopyDescriptor, name: "_TemporaryColorTexture");
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        var cameraData = renderingData.cameraData;

        using (new ProfilingScope(cmd, new ProfilingSampler(m_ProfilerTag)))
        {
            if (m_GenerateTemporaryColorTexture)
            {
                Blitter.BlitCameraTexture(cmd, m_Source, m_TemporaryColorTexture, m_BlitMaterial, 0);
                Blitter.BlitCameraTexture(cmd, m_TemporaryColorTexture, m_Source);
            }
            else
            {
                Blitter.BlitCameraTexture(cmd, m_Source, m_Source, m_BlitMaterial, 0);
            }
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        m_Source?.Release();
        m_TemporaryColorTexture?.Release();
    }
}
