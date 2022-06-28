using System;
using Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool;
using Script.Effect.Editor.AssetTool.Util.Editor;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.MeshAnalysisTool
{
    public class MeshAnalysisToolGUI : UnityEditor.Editor
    {
        /// <summary>
        /// [GUI] 折叠狂
        /// </summary>
        private static bool trisVertsFoldout = true;
        
        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 560;

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
        /// 待检测目录
        /// </summary>
        private static string detectPath = "Assets/Scenes/Models/CombatIsland/DinoLand";
        
        /// <summary>
        /// 待检测游戏物体
        /// </summary>
        private static GameObject detectGameObject;

        /// <summary>
        /// 检测类型
        /// </summary>
        private static MeshAnalysisData.DetectType detectType = MeshAnalysisData.DetectType.Path;
        
        /// <summary>
        /// 检测类型
        /// </summary>
        private static MeshAnalysisData.DetectMeshType detectMeshType = MeshAnalysisData.DetectMeshType.RendererMesh;
        
        /// <summary>
        /// 检测类型
        /// </summary>
        private static MeshAnalysisData.DetectTypeAtPath detectTypeAtPath = MeshAnalysisData.DetectTypeAtPath.Prefabs;

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
                    EditorGUILayout.LabelField("1. 当前支持 4 种检查类型");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("(1) Path:        \t检查指定目录, 目录必须是 Assets/ 的相对目录.");
                    EditorGUILayout.LabelField("    Ⅰ  Textures:\t检查指定目录下的全部 Mesh.");
                    EditorGUILayout.LabelField("    Ⅱ  Prefabs: \t检查指定目录下的全部预制体所引用的 Mesh.");
                    EditorGUILayout.LabelField("(2) Scene:       \t检查当前场景中所引用的 Mesh.");
                    EditorGUILayout.LabelField("(3) Game Object: \t检查指定游戏物体中所引用的 Mesh.");
                    EditorGUI.indentLevel--;

                    GUILayout.BeginVertical("Box");
                    {
                        detectType = (MeshAnalysisData.DetectType) EditorGUILayout.EnumPopup("选择检查类型", detectType, GUILayout.Width(280));
                    }
                    GUILayout.EndVertical();

                    switch (detectType)
                    {
                        case MeshAnalysisData.DetectType.Scene:
                            break;
                        case MeshAnalysisData.DetectType.Path:
                        {
                            GUILayout.BeginVertical("Box");
                            {
                                detectTypeAtPath = (MeshAnalysisData.DetectTypeAtPath) EditorGUILayout.EnumPopup("选择资源类型", detectTypeAtPath, GUILayout.Width(280));
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical("Box");
                            {
                                detectPath = EditorGUILayout.TextField("输入待检查目录: ", detectPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                            }
                            GUILayout.EndVertical();
                        }
                            break;
                        case MeshAnalysisData.DetectType.GameObject:
                        {
                            GUILayout.BeginVertical("Box");
                            {
                                detectGameObject = EditorGUILayout.ObjectField("选择检测的游戏物体: ", detectGameObject, typeof(GameObject), true, GUILayout.Width(UI_INPUT_AREA_WIDTH)) as GameObject;
                            }
                            GUILayout.EndVertical();
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (detectType != MeshAnalysisData.DetectType.Path || detectTypeAtPath != MeshAnalysisData.DetectTypeAtPath.Meshes)
                    {
                        GUILayout.BeginVertical("Box");
                        detectMeshType = (MeshAnalysisData.DetectMeshType) EditorGUILayout.EnumPopup("选择 Mesh 类型", detectMeshType, GUILayout.Width(280));
                        GUILayout.EndVertical();
                    }

                    GUILayout.Space(UI_DEFAULT_MARGIN);
                    GUILayout.Label("2. 点击按钮, 开始分析.");
                    GUILayout.BeginVertical("Box");
                    {
                        if (GUILayout.Button("统计顶点面数", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            SceneAnalysisTableWindow.Open(detectType, detectTypeAtPath, detectMeshType, detectGameObject, detectPath);
                        }
                    }
                    GUILayout.EndVertical();

                    #region 计算内存占用
                    
                    GUILayout.BeginVertical("Box");
                    {
                        UnityEngine.GUI.enabled = detectGameObject == null;
                        if (GUILayout.Button("计算内存占用", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            PrefabUtil.CountMemoryOfPrefab(detectGameObject);
                        }
                    
                        UnityEngine.GUI.enabled = true;
                    }
                    GUILayout.EndVertical();
                    
                    #endregion
                }
                GUILayout.EndVertical();
            }
        }
    }
}
