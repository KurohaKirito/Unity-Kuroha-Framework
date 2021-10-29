using System;
using System.Collections.Generic;
using Kuroha.Tool.Editor.SceneAnalysisTool;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.MeshAnalysisTool
{
    public class MeshAnalysisToolGUI : UnityEditor.Editor
    {
        /// <summary>
        /// 待检测的预制
        /// </summary>
        private static GameObject prefab;

        /// <summary>
        /// [GUI] 折叠狂
        /// </summary>
        private static bool trisVertsFoldout = true;

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
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_SELECTION_WIDTH = 360;

        /// <summary>
        /// 检测类型
        /// </summary>
        private static MeshAnalysisData.DetectType detectType = MeshAnalysisData.DetectType.RendererMesh;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            trisVertsFoldout = EditorGUILayout.Foldout(trisVertsFoldout, "网格资源统计分析工具", true);
            if (trisVertsFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 当前支持两种检查类型");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("(1) RendererMesh: \t 统计指定预制体中的渲染网格数据.");
                    EditorGUILayout.LabelField("(2) ColliderMesh: \t 统计指定预制体中的碰撞网格数据.");
                    EditorGUI.indentLevel--;

                    GUILayout.BeginVertical("Box");
                    detectType = (MeshAnalysisData.DetectType)EditorGUILayout.EnumPopup("选择检查类型", detectType, GUILayout.Width(UI_SELECTION_WIDTH));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("Box");
                    prefab = EditorGUILayout.ObjectField("选择待检查的预制体", prefab, typeof(GameObject), true, GUILayout.Width(UI_SELECTION_WIDTH)) as GameObject;
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("Box");
                    {
                        UnityEngine.GUI.enabled = ReferenceEquals(prefab, null) == false;
                        if (GUILayout.Button("Count Tris Verts", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CountTrisVert(detectType, prefab);
                        }
                        UnityEngine.GUI.enabled = true;
                    }
                    GUILayout.EndVertical();
                    
                    GUILayout.BeginVertical("Box");
                    {
                        UnityEngine.GUI.enabled = ReferenceEquals(prefab, null) == false;
                        if (GUILayout.Button("计算内存占用", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CountMemory(prefab);
                        }
                        UnityEngine.GUI.enabled = true;
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 统计面数和顶点数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="asset"></param>
        private static void CountTrisVert(MeshAnalysisData.DetectType type, GameObject asset)
        {
            switch (type)
            {
                case MeshAnalysisData.DetectType.RendererMesh:
                    SceneAnalysisTableWindow.Open(false, asset, false);
                    break;

                case MeshAnalysisData.DetectType.ColliderMesh:
                    SceneAnalysisTableWindow.Open(true, asset, false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// 统计实际内存占用
        /// </summary>
        /// <param name="asset"></param>
        private static void CountMemory(GameObject asset)
        {
            var textureGuids = new List<string>();

            // 获取预制中全部的 Renderer
            var renderers = asset.GetComponentsInChildren<Renderer>();

            // 获取预制中全部的 Mesh
            foreach (var renderer in renderers)
            {
                var sharedMaterials = renderer.sharedMaterials;
                foreach (var sharedMaterial in sharedMaterials)
                {
                    Kuroha.Util.Editor.TextureUtil.GetTexturesInMaterial(sharedMaterial, out var textures);
                    for (var k = 0; k < textures.Count; k++)
                    {
                        if (textureGuids.Contains(textures[k].guid) == false)
                        {
                            textureGuids.Add(textures[k].guid);
                            var unit = "KB";
                            var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(textures[k].asset);
                            runTimeSize /= 8192;
                            if (runTimeSize > 1024)
                            {
                                runTimeSize /= 1024;
                                unit = "MB";
                            }
                            Debug.Log(textures[k].asset.name + ":  " + runTimeSize + unit);
                        }
                    }
                }
            }
            
            //var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.TextureUtil");
            //var methodInfo = type.GetMethod("GetStorageMemorySize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            //var import = AssetImporter.GetAtPath(textures[k].path);
            // Debug.Log("内存占用：" + EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(textures[k].asset)));
            // Debug.Log("硬盘占用：" +
            //           EditorUtility.FormatBytes((int)methodInfo?.Invoke(null,
            //               new object[] { textures[k].asset })));
        }
    }
}