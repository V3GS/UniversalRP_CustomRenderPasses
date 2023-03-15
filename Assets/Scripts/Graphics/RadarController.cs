using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class RadarController : MonoBehaviour
{
    [SerializeField]
    private UniversalRendererData m_RendererData = null;
    [SerializeField]
    private Transform m_PlayerTransform;
    [SerializeField]
    private Material m_EnemyDetectedMaterial;
    [SerializeField]
    private string m_RendererFeatureName = "";
    [SerializeField] private float transitionPeriod = 1;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float m_WaveSpeed = 1;
    [SerializeField]
    private LayerMask m_EnemyMask = -1;
    [SerializeField]
    private string m_EnemyFoundMaskName = "EnemyFound";

    private RadarRendererFeature m_RadarRendererFeatureEffect = null;
    private float m_WaveDistance = 0.0f;
    
    private bool m_Transitioning;
    private float m_EffectStartTime;

    private bool m_StartEnemyLerping = false;
    private Color m_InitialEnemyColor;
    private Color m_CurrentEnemyColor;
    
    void Start()
    {
        if (TryGetFeature(out var m_RadarEffect))
        {
            m_RadarRendererFeatureEffect = m_RadarEffect as RadarRendererFeature;
        }

        m_InitialEnemyColor = m_CurrentEnemyColor = m_EnemyDetectedMaterial.color;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_WaveDistance = 0.0f;
            m_EffectStartTime = Time.timeSinceLevelLoad;
            m_Transitioning = true;
            m_RadarRendererFeatureEffect.PassSettings.isEnabled = true;

            m_EnemyDetectedMaterial.color = m_CurrentEnemyColor = m_InitialEnemyColor;
        }

        if(m_Transitioning)
        {
            if(Time.timeSinceLevelLoad >= m_EffectStartTime + transitionPeriod)
            {
                m_Transitioning = false;
                m_RadarRendererFeatureEffect.PassSettings.isEnabled = false;
                m_WaveDistance = 0.0f;
            }
            else
            {
                UpdateRadarTransition();
                CheckEnemiesAround();
                LerpEnemyColor();
            }
        }
    }
    
    private void UpdateRadarTransition()
    {
        if (m_RadarRendererFeatureEffect == null) return;

        m_WaveDistance += Time.deltaTime * m_WaveSpeed;
        m_RadarRendererFeatureEffect.PassSettings.blitMaterial.SetFloat("_WaveDistance", m_WaveDistance);
    }

    void CheckEnemiesAround()
    {
        Collider[] hitColliders = Physics.OverlapSphere(m_PlayerTransform.position, m_WaveDistance, m_EnemyMask);
        foreach (var hitCollider in hitColliders)
        {
            hitCollider.gameObject.layer = LayerMask.NameToLayer(m_EnemyFoundMaskName);
            
            // As soon as the first enemy is detected, start lerping the alpha color
            if (!m_StartEnemyLerping) m_StartEnemyLerping = true;
        }
    }

    void LerpEnemyColor()
    {
        if (m_StartEnemyLerping)
        {
            StartCoroutine(StartLerpingColor(0.5f));
        }
    }

    IEnumerator StartLerpingColor(float sleepTime)
    {
        yield return new WaitForSeconds(sleepTime);

        m_CurrentEnemyColor.a = Mathf.Lerp(m_CurrentEnemyColor.a, 1.0f, Time.deltaTime);
        m_EnemyDetectedMaterial.color = m_CurrentEnemyColor;

        yield return null;
    }

    private bool TryGetFeature(out ScriptableRendererFeature feature) {
        feature = m_RendererData.rendererFeatures.Where((f) => f.name == m_RendererFeatureName).FirstOrDefault();
        return feature != null;
    }

    private void OnDrawGizmos()
    {
        if (m_PlayerTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.m_PlayerTransform.position, m_WaveDistance);
    }

    private void OnApplicationQuit()
    {
        m_EnemyDetectedMaterial.color = m_InitialEnemyColor;
    }
}
