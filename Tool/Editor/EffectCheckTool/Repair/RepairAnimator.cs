using System;
using Kuroha.Tool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.Editor.EffectCheckTool.Report;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.Repair
{
    public static class RepairAnimator
    {
        /// <summary>
        /// 自动修复问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">问题项</param>
        public static void Repair(EffectCheckReportInfo effectCheckReportInfo)
        {
            var modeType = (CheckAnimator.CheckOptions)effectCheckReportInfo.modeType;

            switch (modeType)
            {
                case CheckAnimator.CheckOptions.CullMode:
                    RepairCullMode(effectCheckReportInfo);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 修复动画状态机的剔除模式
        /// </summary>
        /// <param name="effectCheckReportInfo"></param>
        private static void RepairCullMode(EffectCheckReportInfo effectCheckReportInfo)
        {
            var cullMode = Convert.ToInt32(effectCheckReportInfo.parameter);
            if (effectCheckReportInfo.asset is GameObject assetGameObject)
            {
                if (assetGameObject.TryGetComponent<Animator>(out var animator))
                {
                    animator.cullingMode = (AnimatorCullingMode)cullMode;
                    EditorUtility.SetDirty(effectCheckReportInfo.asset);
                    EffectCheckReport.reportInfos.Remove(effectCheckReportInfo);
                }
            }
        }
    }
}