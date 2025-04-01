using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OverrideMaterialRenderPassFeature : ScriptableRendererFeature
{
    // Pass settings
    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material overrideMaterial = null;
        
        public LayerMask layerMask;
    }
    
    public Settings PassSettings
    {
        set { m_passSettings = value; }
        get { return m_passSettings; }
    }
    
    OverrideMaterialRenderPass m_ReplaceMaterialPass;
    [SerializeField]
    private Settings m_passSettings = new Settings();

    public override void Create()
    {
        m_ReplaceMaterialPass = new OverrideMaterialRenderPass(m_passSettings);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // If the pass is not enabled
        if (!m_passSettings.isEnabled) return;
        
        // If the Material is not correctly setup, it won't be enqueue the pass 
        if (m_passSettings.overrideMaterial == null)
        {
            Debug.LogWarningFormat("Missing override material. {0} the example pass will not be executed. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }
        
        // By checking the following, the ScriptableRendererFeature will only rendered in the GameView
        if (!renderingData.cameraData.isSceneViewCamera && !renderingData.cameraData.isPreviewCamera)
        {
            renderer.EnqueuePass(m_ReplaceMaterialPass);
        }
    }
}


