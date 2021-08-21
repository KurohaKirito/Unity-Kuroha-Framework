using System;
using Kuroha.Tool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.Repair
{
    public static class RepairParticle
    {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modeType = (CheckParticleSystem.CheckOptions)effectCheckReportInfo.modeType;

            switch (modeType)
            {
                case CheckParticleSystem.CheckOptions.RenderMode:
                    break;
                
                case CheckParticleSystem.CheckOptions.CastShadows:
                    break;
                
                case CheckParticleSystem.CheckOptions.ReceiveShadows:
                    break;
                
                case CheckParticleSystem.CheckOptions.MeshTrisLimit:
                    break;
                
                case CheckParticleSystem.CheckOptions.MeshUV:
                    break;
                
                case CheckParticleSystem.CheckOptions.CollisionAndTrigger:
                    break;
                
                case CheckParticleSystem.CheckOptions.Prewarm:
                    break;

                case CheckParticleSystem.CheckOptions.SubEmittersError:
                    break;
                
                case CheckParticleSystem.CheckOptions.ZeroSurface:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
