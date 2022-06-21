using System;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor.Splitter;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public static class AssetBatchToolGUI {
        /// <summary>
        /// 批处理工具类型
        /// </summary>
        public enum BatchType {
            SetMeshReadWrite,
            SetTextureImportSettings,
            RedundantTextureReferencesCleaner,
            GunAttachmentsCloseCastShadows,
            BundleAssetCounter,
            AssetDeleteTool,
            AssetMoveTool,
            AssetRenameTool,
            MaterialShaderChecker,
            UnusedAssetChecker,
            CheckSubEmitterInAllScene,
            FbxUVColorsChecker,
            AnimationClipCompress,
            AutoCheckTool = 13
        }

        /// <summary>
        /// 批处理工具类型
        /// </summary>
        public static readonly string[] batches = {
            "批量设置网格读写工具",
            "批量修改纹理导入设置工具",
            "材质球冗余纹理引用清除器",
            "关闭枪械配件阴影投射",
            "捆绑包资源数量分析",
            "资源批量删除工具",
            "资源批量移动工具",
            "资源批量重命名工具",
            "材质球的着色器引用检测器",
            "废弃资源检测工具",
            "场景粒子 Sub-Emitter 检测",
            "模型 UV 信息检查器",
            "动画片段压缩工具",
            "自动检测工具"
        };

        /// <summary>
        /// 当前的 Batch
        /// </summary>
        private static BatchType currentBatch;

        /// <summary>
        /// 滑动条
        /// </summary>
        private static Vector2 scrollView;

        /// <summary>
        /// 垂直分割布局
        /// </summary>
        private static VerticalSplitter splitter;

        /// <summary>
        /// 按钮风格
        /// </summary>
        private static GUIStyle buttonStyle;

        /// <summary>
        /// 绘制界面
        /// </summary>
        /// <param name="window"></param>
        public static void OnGUI(in EditorWindow window) {
            if (splitter == null) {
                splitter = new VerticalSplitter(window, 210, 210, false);
            }

            splitter.OnGUI(window.position, MainRect, SubRect);

            if (buttonStyle == null) {
                buttonStyle = new GUIStyle("Button");
            }

            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.normal.textColor = EditorGUIUtility.isProSkin? Color.white : Color.black;
        }

        /// <summary>
        /// 主区域
        /// </summary>
        /// <param name="rect"></param>
        private static void MainRect(Rect rect) {
            GUILayout.BeginArea(rect);
            {
                scrollView = GUILayout.BeginScrollView(scrollView);
                {
                    GUILayout.Space(5);

                    for (var index = 0; index < batches.Length; index++) {
                        var oldColor = UnityEngine.GUI.backgroundColor;
                        if (currentBatch == (BatchType)index) {
                            UnityEngine.GUI.backgroundColor = new Color(92 / 255f, 223 / 255f, 240 / 255f);
                        }

                        if (GUILayout.Button(batches[index], buttonStyle, GUILayout.Width(196), GUILayout.Height(30))) {
                            currentBatch = (BatchType)index;
                        }

                        UnityEngine.GUI.backgroundColor = oldColor;

                        if (index + 1 < batches.Length) {
                            GUILayout.Space(10);
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// 子区域
        /// </summary>
        /// <param name="rect"></param>
        private static void SubRect(Rect rect) {
            GUILayout.BeginArea(rect);
            {
                switch (currentBatch) {
                    case BatchType.SetTextureImportSettings:
                        SetTextureImportSettings.OnGUI();
                        break;
                        
                    case BatchType.RedundantTextureReferencesCleaner:
                        RedundantTextureReferencesCleaner.OnGUI();
                        break;

                    case BatchType.GunAttachmentsCloseCastShadows:
                        GunAttachmentsCloseCastShadows.OnGUI();
                        break;

                    case BatchType.BundleAssetCounter:
                        BundleAssetCounter.OnGUI();
                        break;

                    case BatchType.AssetDeleteTool:
                        AssetDeleteTool.OnGUI();
                        break;

                    case BatchType.AssetMoveTool:
                        AssetMoveTool.OnGUI();
                        break;
                    
                    case BatchType.AssetRenameTool:
                        AssetRenameTool.AssetRenameTool.OnGUI();
                        break;

                    case BatchType.MaterialShaderChecker:
                        ShaderChecker.OnGUI();
                        break;

                    case BatchType.UnusedAssetChecker:
                        UnusedAssetCleaner.OnGUI();
                        break;

                    case BatchType.CheckSubEmitterInAllScene:
                        CheckSubEmitterInAllScene.OnGUI();
                        break;

                    case BatchType.FbxUVColorsChecker:
                        FbxUVColorsChecker.OnGUI();
                        break;

                    case BatchType.AnimationClipCompress:
                        AnimationClipCompress.OnGUI();
                        break;

                    case BatchType.AutoCheckTool:
                        AutoCheckToolGUI.OnGUI();
                        break;

                    case BatchType.SetMeshReadWrite:
                        SetMeshReadWrite.OnGUI();
                        break;
                    
                    default:
                        DebugUtil.LogError("忘记注册 OnGUI 事件了!");
                        throw new ArgumentOutOfRangeException();
                }
            }
            GUILayout.EndArea();
        }
    }
}
