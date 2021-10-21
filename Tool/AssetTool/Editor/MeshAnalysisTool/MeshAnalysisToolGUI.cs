using System;
using Kuroha.Tool.Editor.ModelAnalysisTool;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.MeshAnalysisTool
{
    public class MeshAnalysisToolGUI : UnityEditor.Editor
    {
        private static GameObject trisVertsObject;

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
                    trisVertsObject = EditorGUILayout.ObjectField("选择待检查的预制体", trisVertsObject, typeof(GameObject), true, GUILayout.Width(UI_SELECTION_WIDTH)) as GameObject;
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = ReferenceEquals(trisVertsObject, null) == false;
                    if (GUILayout.Button("Count Tris Verts", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        CountTrisVert(detectType, trisVertsObject);
                    }
                    UnityEngine.GUI.enabled = true;
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
                    ModelAnalysisTableWindow.Open(false, asset, false);
                    break;

                case MeshAnalysisData.DetectType.ColliderMesh:
                    ModelAnalysisTableWindow.Open(true, asset, false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}