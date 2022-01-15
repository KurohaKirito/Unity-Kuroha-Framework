using System;
using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.GUI
{
    public static class EffectCheckToolGUI
    {
        /// <summary>
        /// 折叠标志位
        /// </summary>
        private static bool effectCheckToolFoldout = true;

        /// <summary>
        /// 代码检测标志
        /// </summary>
        private static bool codeCheckFlag;

        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            effectCheckToolFoldout = EditorGUILayout.Foldout(effectCheckToolFoldout, "特效检测工具", true);
            if (effectCheckToolFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 添加一个特效的检查项目.");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("目前支持检查的资源类别有: 网格, 纹理, 动画状态机, 粒子系统, 预制体, 模型.");
                    EditorGUI.indentLevel--;
                    GUILayout.BeginVertical("Box");
                    if (GUILayout.Button("Add Check Item", GUILayout.Height(UI_BUTTON_HEIGHT),
                        GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        EffectCheckItemSetViewWindow.Open(null);
                    }

                    GUILayout.EndVertical();

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    EditorGUILayout.LabelField("2. 浏览全部的特效检查项目, 可以为每一个特效检查项设置其是否启用.");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("是否在 CICD 中启用, 同时可以对每一个检查项进行编辑和删除.");
                    EditorGUI.indentLevel--;
                    GUILayout.BeginVertical("Box");
                    if (GUILayout.Button("Show Check Item", GUILayout.Height(UI_BUTTON_HEIGHT),
                        GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        EffectCheckItemViewWindow.Open();
                    }

                    GUILayout.EndVertical();

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    EditorGUILayout.LabelField("3. 点击按钮, 开始检查.");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("只有启用状态下的检查项才会被执行.");
                    EditorGUI.indentLevel--;
                    GUILayout.BeginVertical("Box");
                    if (GUILayout.Button("Start Check", GUILayout.Height(UI_BUTTON_HEIGHT),
                        GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        Detect(false);
                    }

                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 代码检测
        /// </summary>
        private static bool CodeCheck()
        {
            var isError = false;

            // 检查代码中枚举是否匹配
            if (codeCheckFlag == false)
            {
                codeCheckFlag = true;

                var reportEnumCount = Enum.GetNames(typeof(EffectCheckReportInfo.EffectCheckReportType)).Length;

                var checkMeshEnumCount = Enum.GetNames(typeof(CheckMesh.CheckOptions)).Length;
                var checkModelEnumCount = Enum.GetNames(typeof(CheckModel.CheckOptions)).Length;
                var checkParticleEnumCount = Enum.GetNames(typeof(CheckParticleSystem.CheckOptions)).Length;
                var checkPrefabEnumCount = Enum.GetNames(typeof(CheckPrefab.CheckOptions)).Length;
                var checkTextureEnumCount = Enum.GetNames(typeof(CheckTexture.CheckOptions)).Length;
                var checkAssetEnumCount = Enum.GetNames(typeof(CheckAsset.CheckOptions)).Length;

                var sum = checkMeshEnumCount + checkModelEnumCount + checkParticleEnumCount + checkPrefabEnumCount + checkTextureEnumCount + checkAssetEnumCount;

                if (reportEnumCount != sum)
                {
                    Dialog.Display("错误", $"代码错误! 报告枚举值的数量 {reportEnumCount} 和各个检查项的枚举值数量 {sum} 不一致, 请检查代码!", Dialog.DialogType.Error, "OK");
                    isError = true;
                }
            }

            return isError;
        }

        /// <summary>
        /// 执行检测
        /// </summary>
        public static List<EffectCheckReportInfo> Detect(bool isAutoCheck, string checkItemNameFilter = null)
        {
            var reportInfos = new List<EffectCheckReportInfo>();

            // 代码检测
            var isError = CodeCheck();
            if (isError == false)
            {
                // 读取配置
                EffectCheckItemView.CheckItemInfoList = EffectCheckItemSetView.LoadConfig();

                if (EffectCheckItemView.CheckItemInfoList.Count > 0)
                {
                    #region 检测

                    foreach (var checkItemInfo in EffectCheckItemView.CheckItemInfoList)
                    {
                        if (string.IsNullOrEmpty(checkItemNameFilter))
                        {
                            // 自动检测
                            if (isAutoCheck)
                            {
                                if (checkItemInfo.cicdEnable == false)
                                {
                                    continue;
                                }
                            }
                            // 特效检测
                            else
                            {
                                if (checkItemInfo.effectEnable == false)
                                {
                                    continue;
                                }
                            }
                        }
                        // 指定检查项检测
                        else
                        {
                            if (checkItemInfo.title.IndexOf(checkItemNameFilter, StringComparison.Ordinal) < 0)
                            {
                                continue;
                            }
                        }

                        switch (checkItemInfo.assetsType)
                        {
                            case EffectToolData.AssetsType.Model:
                                CheckModel.Check(checkItemInfo, ref reportInfos);
                                break;

                            case EffectToolData.AssetsType.Prefab:
                                CheckPrefab.Check(checkItemInfo, ref reportInfos);
                                break;

                            case EffectToolData.AssetsType.Texture:
                                CheckTexture.Check(checkItemInfo, ref reportInfos);
                                break;

                            case EffectToolData.AssetsType.Mesh:
                                CheckMesh.Check(checkItemInfo, ref reportInfos);
                                break;

                            case EffectToolData.AssetsType.ParticleSystem:
                                CheckParticleSystem.Check(checkItemInfo, ref reportInfos);
                                break;

                            case EffectToolData.AssetsType.Asset:
                                CheckAsset.Check(checkItemInfo, ref reportInfos);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    #endregion

                    if (reportInfos.Count > 0)
                    {
                        // 如果不是自动检测, 则弹窗, 展示检测结果
                        if (isAutoCheck == false)
                        {
                            EffectCheckReportWindow.Open(reportInfos);
                        }
                    }
                    else
                    {
                        DebugUtil.Log("检测结束, 无任何问题!");
                    }
                }
                else
                {
                    Kuroha.GUI.Editor.Dialog.Display("消息", "当前未启用任何检查项, 请先启用检查项!", Dialog.DialogType.Message, "OK");
                }
            }

            // 返回结果
            return reportInfos;
        }
    }
}
