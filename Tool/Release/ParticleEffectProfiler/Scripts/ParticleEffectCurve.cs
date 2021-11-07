#if UNITY_EDITOR
using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Tool.Release.ParticleEffectProfiler.Scripts
{
    /// <summary>
    /// 特效曲线
    /// </summary>
    public class ParticleEffectCurve
    {
        private readonly AnimationCurve animationCurve = new AnimationCurve();
    
        public const int FPS = 30;

        // 打点的数量: 默认 90 个
        private int valueCount = 3 * FPS;

        private readonly List<int> values = new List<int>();

        public AnimationCurve Update(int value, bool loop, int second)
        {
            valueCount = second * FPS;

            if (animationCurve.length > valueCount)
            {
                for (var index = animationCurve.length - 1; index >= valueCount; index--)
                {
                    DebugUtil.Log(index.ToString());
                    animationCurve.RemoveKey(index);
                    if (index <= values.Count)
                    {
                        values.RemoveAt(index);
                    }
                }
            }

            if (loop)
            {
                if (values.Count >= valueCount)
                {
                    values.RemoveAt(0);
                }

                values.Add(value);
                for (var index = 0; index < values.Count; index++)
                {
                    if (animationCurve.length > index)
                    {
                        animationCurve.RemoveKey(index);
                    }
                    animationCurve.AddKey(index, values[index]);
                }
            }
            else
            {
                if (animationCurve.length < valueCount)
                {
                    animationCurve.AddKey(animationCurve.length, value);
                }
            }

            return animationCurve;
        }

    }
}
#endif