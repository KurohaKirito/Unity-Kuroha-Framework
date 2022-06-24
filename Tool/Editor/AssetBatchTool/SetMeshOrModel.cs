using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Util.Unity;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool
{
    public class SetMeshOrModel
    {
        private enum FixMeshRWType
        {
            Mesh,
            Model
        }

        private enum RWStatus
        {
            开启读写,
            关闭读写
        }

        private enum FixModelMaterialType
        {
            关闭导入,
            开启导入并移除
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

        private static FixMeshRWType fixMeshRwType;
        private static string filter = "^(.(?!Collider))*$";
        private static Regex regex;
        private static RWStatus flagRW;
        private static bool rwSwitch;
        private static FixModelMaterialType fixModelMaterialType;

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
                {
                    GUILayout.BeginVertical("Box");

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("读写修复工具");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    EditorGUILayout.LabelField("1. 选择修复类型.");
                    GUILayout.BeginVertical("Box");
                    fixMeshRwType = (FixMeshRWType) EditorGUILayout.EnumPopup("Input Fix Type", fixMeshRwType, GUILayout.Width(260));
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
                        switch (fixMeshRwType)
                        {
                            case FixMeshRWType.Mesh:
                                await FixMesh();
                                break;
                            case FixMeshRWType.Model:
                                FixModel();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();

                    GUILayout.EndVertical();
                }
                GUILayout.Space(UI_DEFAULT_MARGIN);
                {
                    GUILayout.BeginVertical("Box");
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("模型内嵌材质球清除工具");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    EditorGUILayout.LabelField("1. 选择修复目标类型.");
                    GUILayout.BeginVertical("Box");
                    fixModelMaterialType = (FixModelMaterialType) EditorGUILayout.EnumPopup("Input Fix Type", fixModelMaterialType, GUILayout.Width(260));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("2. 输入 Model 所在路径.");
                    GUILayout.BeginVertical("Box");
                    checkPath = EditorGUILayout.TextField("Input Path To Detect", checkPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("3. 输入文件 (包含路径) 的正则剔除规则");
                    GUILayout.BeginVertical("Box");
                    filter = EditorGUILayout.TextField("Input Filter Rule", filter, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();
                    
                    EditorGUILayout.LabelField("4. 点击按钮, 开始修复.");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(checkPath) == false;
                    if (GUILayout.Button("Fix", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        RemoveMaterials();
                    }
                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();
                    
                    GUILayout.EndVertical();
                }
            }
        }

        private static void InitRegex()
        {
            regex = new Regex(filter);
            switch (flagRW)
            {
                case RWStatus.开启读写:
                    rwSwitch = true;
                    break;
                case RWStatus.关闭读写:
                    rwSwitch = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static List<ModelImporter> GetModelList(bool isRW)
        {
            var models = new List<ModelImporter>();
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
                        if (isRW)
                        {
                            if (modelImporter.isReadable != rwSwitch)
                            {
                                models.Add(modelImporter);
                            }
                        }
                        else
                        {
                            models.Add(modelImporter);
                        }
                    }
                }
            }

            return models;
        }

        private static List<Mesh> GetMeshList(bool isRW)
        {
            var meshes = new List<Mesh>();
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

                if (path.IndexOf(".asset", StringComparison.OrdinalIgnoreCase) > 0 || path.IndexOf(".mesh", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    try
                    {
                        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if (isRW)
                        {
                            if (mesh.isReadable != rwSwitch)
                            {
                                meshes.Add(mesh);
                            }
                        }
                        else
                        {
                            meshes.Add(mesh);
                        }
                    } catch
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
            var models = GetModelList(true);

            for (var index = 0; index < models.Count; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Model 修复中: {index + 1}/{models.Count}", index + 1, models.Count))
                {
                    return;
                }

                models[index].isReadable = rwSwitch;
                models[index].SaveAndReimport();
                counter++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"一共修复了 {counter} 个 Model");
        }

        private static async Task FixMesh()
        {
            var counter = 0;
            var meshes = GetMeshList(true);

            for (var index = 0; index < meshes.Count; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Mesh 修复中: {index + 1}/{meshes.Count}", index + 1, meshes.Count))
                {
                    return;
                }

                await meshes[index].SetReadable(rwSwitch);
                AssetDatabase.Refresh();
                counter++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"一共修复了 {counter} 个 Mesh");
        }

        private static void RemoveMaterials()
        {
            var counter = 0;
            var models = GetModelList(false);

            for (var index = 0; index < models.Count; index++)
            {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"Model 材质球导入设置中: {index + 1}/{models.Count}", index + 1, models.Count))
                {
                    return;
                }

                switch (fixModelMaterialType)
                {
                    case FixModelMaterialType.关闭导入:
                        if (models[index].materialImportMode != ModelImporterMaterialImportMode.None)
                        {
                            models[index].materialImportMode = ModelImporterMaterialImportMode.None;
                            models[index].SaveAndReimport();
                        }
                        break;
                    
                    case FixModelMaterialType.开启导入并移除:
                        if (models[index].materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
                        {
                            // 开启材质导入, 提取出模型的内嵌材质到 Materials 文件夹
                            models[index].materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                            models[index].SaveAndReimport();

                            // 删除提取出来的材质球
                            var path = AssetDatabase.GetAssetPath(models[index]);
                            var materialPath = path.Substring(0, path.LastIndexOf('/')) + "/Materials";
                            Debug.Log($"materialPath:  {materialPath}");
                            AssetDatabase.DeleteAsset(materialPath);

                            // 修改模型材质引用类型为内嵌材质
                            models[index].materialLocation = ModelImporterMaterialLocation.InPrefab;
                            models[index].SaveAndReimport();
                        }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                counter++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"一共修复了 {counter} 个 Model");
        }
    }
}
