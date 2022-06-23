using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Util.Unity;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool
{
    public class SetMeshReadWrite
    {
        private enum FixType
        {
            Mesh, Model
        }
        
        private enum RWStatus
        {
            开启读写, 关闭读写
        }
    
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;

        /// <summary>
        /// 枪械路径
        /// </summary>
        private static string checkPath = "Assets/Scenes/Models/CombatIsland/DinoLand";

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

        private static FixType fixType;
        private static string filter = "Collider";
        private static Regex regex;
        private static RWStatus flagRW;

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
                    EditorGUILayout.LabelField("1. 选择修复类型.");
                    GUILayout.BeginVertical("Box");
                    fixType = (FixType) EditorGUILayout.EnumPopup("Input Fix Type", fixType, GUILayout.Width(260));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("2. 输入 Mesh 或者 Model 所在路径.");
                    GUILayout.BeginVertical("Box");
                    checkPath = EditorGUILayout.TextField("Input Path To Detect", checkPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("3. 选择正确的读写状态.");
                    GUILayout.BeginVertical("Box");
                    flagRW = (RWStatus) EditorGUILayout.EnumPopup("Select Target RW", flagRW, GUILayout.Width(260));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("4. 输入文件 (包含路径) 的正则剔除规则");
                    GUILayout.BeginVertical("Box");
                    filter = EditorGUILayout.TextField("Input Filter Rule", filter, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("5. 点击按钮, 开始修复.");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(checkPath) == false;
                    if (GUILayout.Button("Fix", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        InitRegex();
                        FixModel();
                        await FixMesh();
                    }

                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        private static void InitRegex()
        {
            regex = new Regex(filter);
        }

        private static IEnumerable<ModelImporter> GetModelList()
        {
            var models = new List<ModelImporter>();
            var rw = flagRW == RWStatus.开启读写;
            var guids = AssetDatabase.FindAssets("t:Model", new[]
            {
                checkPath
            });
            
            Debug.Log($"一共检测到 {guids.Length} 个 Model");
            
            for (var index = 0; index < guids.Length; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"查找 Model 中: {index + 1}/{guids.Length}", index + 1, guids.Length))
                {
                    return models;
                }

                var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (regex.Match(path).Success)
                {
                    continue;
                }

                if (path.IndexOf(".fbx", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    if (AssetImporter.GetAtPath(path) is ModelImporter modelImporter)
                    {
                        if (modelImporter.isReadable != rw)
                        {
                            models.Add(modelImporter);
                        }
                    }
                }
            }

            return models;
        }
        
        private static IEnumerable<Mesh> GetMeshList()
        {
            var meshes = new List<Mesh>();
            var rw = flagRW == RWStatus.开启读写;
            var guids = AssetDatabase.FindAssets("t:Mesh", new[]
            {
                checkPath
            });
            
            Debug.Log($"一共检测到 {guids.Length} 个 Mesh");

            for (var index = 0; index < guids.Length; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"查找 Mesh 中: {index + 1}/{guids.Length}", index + 1, guids.Length))
                {
                    return meshes;
                }

                var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (regex.Match(path).Success)
                {
                    continue;
                }

                if (path.IndexOf(".asset", StringComparison.OrdinalIgnoreCase) > 0 ||
                    path.IndexOf(".mesh", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    try
                    {
                        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if (mesh.isReadable != rw)
                        {
                            meshes.Add(mesh);
                        }
                    }
                    catch
                    {
                        Debug.Log($"无法将资源读取为 Mesh, 请检查 Mesh 是否存在问题! {path}");
                    }
                }
            }

            return meshes;
        }

        private static void FixModel()
        {
            var counter = 0;
            var rw = flagRW == RWStatus.开启读写;
            var models = GetModelList().ToList();
            
            for (var index = 0; index < models.Count; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Model 修复中: {index + 1}/{models.Count}", index + 1, models.Count))
                {
                    return;
                }
                
                models[index].isReadable = rw;
                models[index].SaveAndReimport();
                counter++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"一共修复了 {counter} 个 Model");
        }

        private static async Task FixMesh()
        {
            var counter = 0;
            var rw = flagRW == RWStatus.开启读写;
            var meshes = GetMeshList().ToList();
            
            for (var index = 0; index < meshes.Count; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Mesh 修复中: {index + 1}/{meshes.Count}", index + 1, meshes.Count))
                {
                    return;
                }
                
                await meshes[index].SetReadable(rw);
                AssetDatabase.Refresh();
                counter++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"一共修复了 {counter} 个 Mesh");
        }
    }
}
