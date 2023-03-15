using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadarRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        
        public Material blitMaterial = null;
        public float WaveDistance = 0.0f;
        public float TrailLenght = 0.185f;
        public Color WaveColor = Color.white;
    }
    
    private RadarRenderPass m_RadarPass;
    [SerializeField]
    private Settings m_passSettings = new Settings();

    public Settings PassSettings
    {
        set { m_passSettings = value; }
        get { return m_passSettings; }
    }

    public override void Create()
    {
        m_RadarPass = new RadarRenderPass(m_passSettings, name);
    }
 
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!m_passSettings.isEnabled) return;

        if (m_passSettings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        // By checking the following, the ScriptableRendererFeature will only rendered in the GameView
        if (!renderingData.cameraData.isSceneViewCamera && !renderingData.cameraData.isPreviewCamera)
        {
            renderer.EnqueuePass(m_RadarPass);
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_RadarPass.Setup(renderer.cameraColorTargetHandle);
    }
}


