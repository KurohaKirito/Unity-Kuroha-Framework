using System;
using System.Threading.Tasks;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Util.Unity;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool
{
    public class SetMeshReadWrite
    {
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;

        /// <summary>
        /// 枪械路径
        /// </summary>
        private static string checkPath = "ToBundle/Skin/Items";

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
        private const float UI_INPUT_AREA_WIDTH = 400;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static async void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.GunAttachmentsCloseCastShadows], true);

            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 输入枪械预制体所在路径.");
                    GUILayout.BeginVertical("Box");
                    checkPath = EditorGUILayout.TextField("Input Path To Detect", checkPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("2. 点击按钮, 开始修复.");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(checkPath) == false;
                    if (GUILayout.Button("Fix", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        FixModel();
                        await FixMesh();
                    }

                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        private static void FixModel()
        {
            var counter = 0;
            var guids = AssetDatabase.FindAssets("t:Model", new[]
            {
                checkPath
            });
            for (var index = 0; index < guids.Length; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Model 检测中: {index + 1}/{guids.Length}", index + 1, guids.Length))
                {
                    return;
                }

                var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (path.IndexOf("Collider", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    continue;
                }

                if (path.IndexOf(".fbx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    if (AssetImporter.GetAtPath(path) is ModelImporter modelImporter)
                    {
                        if (modelImporter.isReadable)
                        {
                            modelImporter.isReadable = false;
                            modelImporter.SaveAndReimport();
                            counter++;
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"一共检测到 {guids.Length} 个, 共修复了 {counter} 个!");
        }

        private static async Task FixMesh()
        {
            var counter = 0;
            var guids = AssetDatabase.FindAssets("t:Mesh", new[]
            {
                checkPath
            });
            for (var index = 0; index < guids.Length; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Mesh 检测中: {index + 1}/{guids.Length}", index + 1, guids.Length))
                {
                    return;
                }

                var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (path.IndexOf("Collider", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    continue;
                }

                if (path.IndexOf(".asset", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    try
                    {
                        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if (mesh.isReadable)
                        {
                            await mesh.SetReadable(false);
                            AssetDatabase.Refresh();
                            counter++;
                        }
                    }
                    catch
                    {
                        Debug.Log($"无法读取 Mesh, 请检查 Mesh 是否存在问题! {path}");
                    }
                }
            }

            Debug.Log($"一共检测到 {guids.Length} 个, 共修复了 {counter} 个!");
        }
    }
}
