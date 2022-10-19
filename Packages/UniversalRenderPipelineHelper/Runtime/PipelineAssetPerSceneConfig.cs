using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PipelineAssetPerSceneConfig", menuName = "ScriptableObjects/Pipeline asset associated to scene config", order = 1)]
[System.Serializable]
public class PipelineAssetPerSceneConfig : ScriptableObject
{
    [SerializeField]
    private List<PipelineAssetScene> m_PipelineAssetBoundToScene = new List<PipelineAssetScene>();
    
    public List<PipelineAssetScene> PipelineAssetBoundToScene
    {
        get => m_PipelineAssetBoundToScene;
        set => m_PipelineAssetBoundToScene = value;
    }
}
