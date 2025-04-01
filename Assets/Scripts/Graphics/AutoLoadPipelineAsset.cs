using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class AutoLoadPipelineAsset : MonoBehaviour
{
    public PipelineAssetPerSceneConfig m_PipelineAssetConfig;
    
    private void OnEnable()
    {
        UpdatePipeline();
    }

    void UpdatePipeline()
    {
        if ( m_PipelineAssetConfig )
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            for (int i = 0; i < m_PipelineAssetConfig.PipelineAssetBoundToScene.Count; i++)
            {
                SceneAsset sceneInfo = m_PipelineAssetConfig.PipelineAssetBoundToScene[i].SceneAsset;

                if (currentSceneName == sceneInfo.name)
                {
                    GraphicsSettings.defaultRenderPipeline = m_PipelineAssetConfig.PipelineAssetBoundToScene[i].PipelineAsset;
                }
            }
        }
    }
}
