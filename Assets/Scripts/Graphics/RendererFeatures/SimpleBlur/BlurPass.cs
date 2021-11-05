using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurPass : ScriptableRenderPass
{
    string m_ProfilerTag;
    private Material m_BlurMaterial = null;
    private RenderTargetIdentifier m_Source { get; set; }
    RenderTargetHandle m_TemporaryColorTexture;
    
    // Reference to shader properties IDs.
    static readonly int m_GrayscaleProperty = Shader.PropertyToID("_Grayscale");
    static readonly int m_BlurX = Shader.PropertyToID("_BlurX");
    static readonly int m_BlurY = Shader.PropertyToID("_BlurY");

    public BlurPass(BlurRenderPassFeature.Settings settings, string tag)
    {
        renderPassEvent = settings.renderPassEvent;
        m_ProfilerTag = tag;
        m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        
        m_BlurMaterial = settings.blurMaterial;
        
        // Set any material properties based on our pass settings. 
        m_BlurMaterial.SetInt(m_GrayscaleProperty, Convert.ToInt32(settings.grayscale) );
        m_BlurMaterial.SetFloat(m_BlurX, settings.blurX );
        m_BlurMaterial.SetFloat(m_BlurY, settings.blurY );
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

            Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_BlurMaterial);
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
