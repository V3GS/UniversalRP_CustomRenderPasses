using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadarRenderPass : ScriptableRenderPass
{
    string m_ProfilerTag;
    private Material m_RadarMaterial = null;
    private RenderTargetIdentifier m_Source { get; set; }
    RenderTargetHandle m_TemporaryColorTexture;

    static readonly int m_WaveDistance = Shader.PropertyToID("_WaveDistance");
    static readonly int m_TrailLenght = Shader.PropertyToID("_TrailLenght");
    static readonly int m_WaveColor= Shader.PropertyToID("_WaveColor");
    

    public RadarRenderPass(RadarRendererFeature.Settings settings, string tag)
    {
        renderPassEvent = settings.renderPassEvent;
        m_ProfilerTag = tag;
        m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        
        m_RadarMaterial = settings.blitMaterial;
        
        SetMaterialData(settings);
    }

    private void SetMaterialData(RadarRendererFeature.Settings settings)
    {
        // Set any material properties based on our pass settings. 
        m_RadarMaterial.SetFloat(m_WaveDistance, settings.WaveDistance);
        m_RadarMaterial.SetFloat(m_TrailLenght, settings.TrailLenght);
        m_RadarMaterial.SetColor(m_WaveColor, settings.WaveColor );
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

            Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_RadarMaterial);
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
