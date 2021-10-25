#if UNITY_EDITOR
using System.Reflection;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 特效性能分析工具的管理类
/// 将此类添加到特效上即可
/// </summary>
public class ParticleEffectManager : MonoBehaviour
{
    // 动画曲线
    public AnimationCurve 粒子数量 = new AnimationCurve();
    public AnimationCurve drawCall = new AnimationCurve();
    public AnimationCurve overdraw = new AnimationCurve();
    
    // 是否循环播放
    public bool 循环;
    
    [Range(1,10)]
    public int 特效运行时间 = 3;

    private EffectOverdraw effectOverdraw;
    ParticleSystem[] m_ParticleSystems;
    MethodInfo m_CalculateEffectUIDataMethod;
    int m_ParticleCount;
    int m_MaxParticleCount;

    private ParticleEffectCurve curveParticleCount;
    private ParticleEffectCurve curveDrawCallCount;
    private ParticleEffectCurve curveOverdraw;

    /// <summary>
    /// 初始化
    /// </summary>
    private void Awake()
    {
        DebugUtil.Log("开始测试单个粒子系统");
        Application.targetFrameRate = ParticleEffectCurve.FPS;

        curveParticleCount = new ParticleEffectCurve();
        curveDrawCallCount = new ParticleEffectCurve();
        curveOverdraw = new ParticleEffectCurve();
        effectOverdraw = new EffectOverdraw(Camera.main);
    }

    void Start()
    {
        m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
#if UNITY_2017_1_OR_NEWER
        m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.Instance | BindingFlags.NonPublic);
#else
        m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CountSubEmitterParticles", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
    }

    private void LateUpdate()
    {
        RecordParticleCoun();
        effectOverdraw.Update();

        UpdateParticleCountCurve();
        UpdateDrawCallCurve();
        UpdateOverdrawCurve();
    }

    public EffectOverdrawData[] GetEffectEvlaData()
    {
        return effectOverdraw.GetEffectEvlaData();
    }

    public void RecordParticleCoun()
    {
        m_ParticleCount = 0;
        foreach (var ps in m_ParticleSystems)
        {
            int count = 0;
#if UNITY_2017_1_OR_NEWER
            object[] invokeArgs = { count, 0.0f, Mathf.Infinity };
            m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
            count = (int)invokeArgs[0];
#else
            object[] invokeArgs = { count };
            m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
            count = (int)invokeArgs[0];
            count += ps.particleCount;
#endif
            m_ParticleCount += count;
        }
        if (m_MaxParticleCount < m_ParticleCount)
        {
            m_MaxParticleCount = m_ParticleCount;
        }
    }

    public int GetParticleCount()
    {
        return m_ParticleCount;
    }
    public int GetMaxParticleCount()
    {
        return m_MaxParticleCount;
    }

    private void UpdateParticleCountCurve()
    {
        粒子数量 = curveParticleCount.Update(m_ParticleCount, 循环, 特效运行时间);
    }

    private void UpdateDrawCallCurve()
    {
        drawCall = curveDrawCallCount.Update(GetParticleEffectData.GetOnlyParticleEffectDrawCall(), 循环, 特效运行时间);
    }

    private void UpdateOverdrawCurve()
    {
        var effectEvlaData = this.GetEffectEvlaData();
        overdraw = curveOverdraw.Update(effectEvlaData[0].GetPixRate(), 循环, 特效运行时间);
    }

	//监听apply事件
    [InitializeOnLoadMethod]
    static void StartInitializeOnLoadMethod()
    {
        PrefabUtility.prefabInstanceUpdated = delegate(GameObject instance)
        {
            var particleEffectScript = instance.GetComponentsInChildren<ParticleEffectManager>(true);

            if (particleEffectScript.Length > 0)
            {
                Debug.LogError("保存前请先删除ParticleEffectScript脚本！");
            }
        };
    }
}
#endif