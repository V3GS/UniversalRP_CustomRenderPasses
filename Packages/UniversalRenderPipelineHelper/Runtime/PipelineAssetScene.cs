using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class PipelineAssetScene
{
    [SerializeField] private SceneAsset m_SceneAsset;
    [SerializeField] private UniversalRenderPipelineAsset m_PipelineAsset;

    public SceneAsset SceneAsset { get => m_SceneAsset; set => m_SceneAsset = value; }
    public UniversalRenderPipelineAsset PipelineAsset { get => m_PipelineAsset; set => m_PipelineAsset = value; }
}
