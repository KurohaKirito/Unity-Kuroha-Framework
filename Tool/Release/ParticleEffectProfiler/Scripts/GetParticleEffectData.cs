#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 处理特效整体相关的数据
/// </summary>
public static class GetParticleEffectData
{
    private static int maxDrawCall;

    /// <summary>
    /// 计算运行时内存占用
    /// </summary>
    /// <param name="go"></param>
    /// <param name="textureCount">返回贴图数量</param>
    /// <returns></returns>
    private static int GetRuntimeMemorySize(GameObject go, out int textureCount)
    {
        textureCount = 0;
        
        var memorySize = 0;
        var textures = new List<Texture>();
        var meshRendererList = go.GetComponentsInChildren<ParticleSystemRenderer>(true);

        foreach (var renderer in meshRendererList)
        {
            if (renderer.sharedMaterial)
            {
                var texture = renderer.sharedMaterial.mainTexture;
                if (texture && textures.Contains(texture) == false)
                {
                    textures.Add(texture);
                    textureCount++;
                    memorySize += GetStorageMemorySize(texture);
                }
            }
        }
        
        return memorySize;
    }

    /// <summary>
    /// 计算硬盘占用
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    private static int GetStorageMemorySize(Texture texture)
    {
        return (int)InvokeInternalAPI("UnityEditor.TextureUtil", "GetStorageMemorySize", texture);
    }

    /// <summary>
    /// 运行 Unity 内部私有 API
    /// </summary>
    /// <param name="type"></param>
    /// <param name="method"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static object InvokeInternalAPI(string type, string method, params object[] parameters)
    {
        var assembly = typeof(AssetDatabase).Assembly;
        var custom = assembly.GetType(type);
        var methodInfo = custom.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
        return methodInfo != null ? methodInfo.Invoke(null, parameters) : 0;
    }

    /// <summary>
    /// 运行时内存占用
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static string GetTextRuntimeMemorySize(GameObject go)
    {
        const int MAX_TEXTURE_COUNT = 5;
        const int MAX_MEMORY_SIZE = 1000 * 1024;
        
        var memorySize = GetRuntimeMemorySize(go, out var textureCount);
        var memorySizeStr = EditorUtility.FormatBytes(memorySize);
        var maxMemorySizeStr = EditorUtility.FormatBytes(MAX_MEMORY_SIZE);

        memorySizeStr = MAX_MEMORY_SIZE > memorySize
            ? $"<color=green>{memorySizeStr}</color>"
            : $"<color=red>{memorySizeStr}</color>";

        return $"贴图所占用的内存: {memorySizeStr}  建议: <{maxMemorySizeStr}\n贴图数量: {FormatColorMax(textureCount, MAX_TEXTURE_COUNT)}  建议: <{MAX_TEXTURE_COUNT}";
    }

    /// <summary>
    /// 向外返回 粒子系统组件 数量
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static string GetParticleSystemCount(GameObject go)
    {
        const int MAX = 5;
        var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        return $"特效中所有粒子系统组件数量: {FormatColorMax(particleSystems.Length, MAX)}  建议: <{MAX}";
    }

    public static int GetOnlyParticleEffectDrawCall()
    {
        // 因为 Camera 实际上渲染了两次，一次用作取样，一次用作显示。
        // 狂飙这里给出了详细的说明：https://networm.me/2019/07/28/unity-particle-effect-profiler/#drawcall-%E6%95%B0%E5%80%BC%E4%B8%BA%E4%BB%80%E4%B9%88%E6%AF%94%E5%AE%9E%E9%99%85%E5%A4%A7-2-%E5%80%8D
        var drawCall = UnityEditor.UnityStats.batches / 2;
        if (maxDrawCall < drawCall)
        {
            maxDrawCall = drawCall;
        }
        return drawCall;
    }

    public static string GetOnlyParticleEffectDrawCallStr()
    {
        const int MAX_DRAW_CALL = 10;
        return $"DrawCall: {FormatColorMax(GetOnlyParticleEffectDrawCall(), MAX_DRAW_CALL)} 最高: {FormatColorMax(maxDrawCall, MAX_DRAW_CALL)} 建议: <{MAX_DRAW_CALL}";
    }

    public static string GetPixDrawAverageStr(ParticleEffectScript particleEffectGo)
    {
        //index = 0：默认按高品质的算，这里你可以根本你们项目的品质进行修改。
        EffectEvlaData[] effectEvlaData = particleEffectGo.GetEffectEvlaData();
        int pixDrawAverage = effectEvlaData[0].GetPixDrawAverage();
        return string.Format("特效原填充像素点：{0}", FormatColorValue(pixDrawAverage));
    }

    public static string GetPixActualDrawAverageStr(ParticleEffectScript particleEffectGo)
    {
        EffectEvlaData[] effectEvlaData = particleEffectGo.GetEffectEvlaData();
        int pixActualDrawAverage = effectEvlaData[0].GetPixActualDrawAverage();
        return string.Format("特效实际填充像素点：{0}", FormatColorValue(pixActualDrawAverage));
    }

    public static string GetPixRateStr(ParticleEffectScript particleEffectGo)
    {
        int max = 4;
        EffectEvlaData[] effectEvlaData = particleEffectGo.GetEffectEvlaData();
        int pixRate = effectEvlaData[0].GetPixRate();
        return string.Format("平均每像素overdraw率：{0}   建议：<{1}", FormatColorMax(pixRate, max), max);
    }

    public static string GetParticleCountStr(ParticleEffectScript particleEffectGo)
    {
        int max = 50;
        return string.Format("粒子数量：{0}   最高：{1}   建议：<{2}", FormatColorMax(particleEffectGo.GetParticleCount(), max), FormatColorMax(particleEffectGo.GetMaxParticleCount(), max), max);
    }

    public static string GetCullingSupportedString(GameObject go)
    {
        var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        string text = "";
        foreach (ParticleSystem item in particleSystems)
        {
            string str = CheckCulling(item);
            if (!string.IsNullOrEmpty(str))
            {
                text += item.gameObject.name + ":" + str + "\n\n";
            }
        }
        return text;
    }

    private static string CheckCulling(ParticleSystem particleSystem)
    {
        var text = "";
        
        if (particleSystem.collision.enabled)
        {
            text += "\n勾选了 Collision";
        }

        if (particleSystem.emission.enabled)
        {
            if (particleSystem.emission.rateOverDistance.curveMultiplier != 0)
            {
                text += "\nEmission 使用了 Current(非线性运算)";
            }
        }

        if (particleSystem.externalForces.enabled)
        {
            text += "\n勾选了 External Forces";
        }

        if (particleSystem.forceOverLifetime.enabled)
        {
            if (GetIsRandomized(particleSystem.forceOverLifetime.x)
                || GetIsRandomized(particleSystem.forceOverLifetime.y)
                || GetIsRandomized(particleSystem.forceOverLifetime.z)
                || particleSystem.forceOverLifetime.randomized)
            {
                text += "\nForce Over Lifetime 使用了 Current(非线性运算)";
            }
        } 
        if (particleSystem.inheritVelocity.enabled)
        {
            if (GetIsRandomized(particleSystem.inheritVelocity.curve))
            {
                text += "\nInherit Velocity 使用了 Current(非线性运算)";
            }
        } 
        if (particleSystem.noise.enabled)
        {
            text += "\n勾选了 Noise";
        } 
        if (particleSystem.rotationBySpeed.enabled)
        {
            text += "\n勾选了 Rotation By Speed";
        }
        if (particleSystem.rotationOverLifetime.enabled)
        {
            if (GetIsRandomized(particleSystem.rotationOverLifetime.x)
                || GetIsRandomized(particleSystem.rotationOverLifetime.y)
                || GetIsRandomized(particleSystem.rotationOverLifetime.z))
            {
                text += "\nRotation Over Lifetime 使用了 Current(非线性运算)";
            }
        } 
        if (particleSystem.shape.enabled)
        {
            var shapeType = particleSystem.shape.shapeType;
            switch (shapeType)
            {
                case ParticleSystemShapeType.Cone:
                case ParticleSystemShapeType.ConeVolume:
                case ParticleSystemShapeType.Donut:
                case ParticleSystemShapeType.Circle:
                    if(particleSystem.shape.arcMode != ParticleSystemShapeMultiModeValue.Random)
                    {
                        text += "\nShape 的 Circle-Arc 使用了 Random 模式";
                    }
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    if (particleSystem.shape.radiusMode != ParticleSystemShapeMultiModeValue.Random)
                    {
                        text += "\nShape 的 Edge-Radius 使用了 Random 模式";
                    }
                    break;
                default:
                    break;
            }
        } 
        if (particleSystem.subEmitters.enabled)
        {
            text += "\n勾选了 SubEmitters";
        } 
        if (particleSystem.trails.enabled)
        {
            text += "\n勾选了 Trails";
        } 
        if (particleSystem.trigger.enabled)
        {
            text += "\n勾选了 Trigger";
        }
        if (particleSystem.velocityOverLifetime.enabled)
        {
            if (GetIsRandomized(particleSystem.velocityOverLifetime.x)
                || GetIsRandomized(particleSystem.velocityOverLifetime.y)
                || GetIsRandomized(particleSystem.velocityOverLifetime.z))
            {
                text += "\nVelocity Over Lifetime 使用了 Current(非线性运算)";
            }
        }
        if (particleSystem.limitVelocityOverLifetime.enabled)
        {
            text += "\n勾选了 Limit Velocity Over Lifetime";
        }
        if (particleSystem.main.simulationSpace != ParticleSystemSimulationSpace.Local)
        {
            text += "\nSimulationSpace 不等于 Local";
        }
        if (particleSystem.main.gravityModifierMultiplier != 0)
        {
            text += "\nGravityModifier 不等于 0";
        }
        return text;
    }

    private static bool GetIsRandomized(ParticleSystem.MinMaxCurve minMaxCurve)
    {
        var flag = AnimationCurveSupportsProcedural(minMaxCurve.curveMax);

        bool result;
        if (minMaxCurve.mode != ParticleSystemCurveMode.TwoCurves && minMaxCurve.mode != ParticleSystemCurveMode.TwoConstants)
        {
            result = flag;
        }
        else
        {
            var flag2 = AnimationCurveSupportsProcedural(minMaxCurve.curveMin);
            result = (flag && flag2);
        }

        return result;
    }

    private static bool AnimationCurveSupportsProcedural(AnimationCurve curve)
    {
        // switch (AnimationUtility.IsValidPolynomialCurve(curve)) //保护级别，无法访问，靠
        // {
        //     case AnimationUtility.PolynomialValid.Valid:
        //         return true;
        //     case AnimationUtility.PolynomialValid.InvalidPreWrapMode:
        //         break;
        //     case AnimationUtility.PolynomialValid.InvalidPostWrapMode:
        //         break;
        //     case AnimationUtility.PolynomialValid.TooManySegments:
        //         break;
        // }
        
        return false; //只能默认返回false了
    }

    private static string FormatColorValue(int value)
    {
        return $"<color=green>{value}</color>";
    }

    private static string FormatColorMax(int value, int max)
    {
        return value < max
            ? $"<color=green>{value}</color>"
            : $"<color=red>{value}</color>";
    }
}
#endif