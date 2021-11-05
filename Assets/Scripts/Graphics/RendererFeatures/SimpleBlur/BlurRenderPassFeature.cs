using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
 
        public Material blurMaterial = null;
        [Range(0.0f, 0.015f)]
        public float blurX;
        [Range(0.0f, 0.015f)]
        public float blurY;
        public bool grayscale = false;
    }
    
    private BlurPass m_BlurPass;
    [SerializeField]
    private Settings m_passSettings = new Settings();

    public Settings PassSettings
    {
        set { m_passSettings = value; }
        get { return m_passSettings; }
    }

    public override void Create()
    {
        m_BlurPass = new BlurPass(m_passSettings, name);
    }
 
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!m_passSettings.isEnabled) return;

        if (m_passSettings.blurMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        if (!renderingData.cameraData.isPreviewCamera)
        {
            m_BlurPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_BlurPass);
        }
    }
}


