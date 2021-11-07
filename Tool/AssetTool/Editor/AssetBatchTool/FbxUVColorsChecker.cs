using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.GUI.Editor;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

// 检测特定路径下 fbx 模型文件的 mesh 网格中的 uv2 uv3 uv4 colors 信息
namespace Kuroha.Tool.AssetTool.Editor.AssetBatchTool
{
    public static class FbxUVColorsChecker
    {
        /// <summary>
        /// 待检测文件夹
        /// </summary>
        private static string folderPath;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool foldout = true;
        
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

        private static bool uv2;
        private static bool uv3;
        private static bool uv4;
        private static bool colors;
    
        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            var title = AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.FbxUVColorsChecker];
            foldout = EditorGUILayout.Foldout(foldout, title, true);

            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 选择检测路径");
                
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical(GUILayout.Width(UI_BUTTON_WIDTH + 60));
                        {
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Select Folder", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                                {
                                    folderPath = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "Art");
                                }
                            }
                            GUILayout.EndHorizontal();
                        
                            GUILayout.Space(UI_DEFAULT_MARGIN);
                        
                            GUILayout.Label("2. 选择允许存在的属性. (勾选: 允许)");
                            uv2 = EditorGUILayout.ToggleLeft("UV2", uv2);
                            uv3 = EditorGUILayout.ToggleLeft("UV3", uv3);
                            uv4 = EditorGUILayout.ToggleLeft("UV4", uv4);
                            colors = EditorGUILayout.ToggleLeft("Colors", colors);
                            
                            GUILayout.Space(UI_DEFAULT_MARGIN);
                            
                            GUILayout.Label("3. 点击按钮, 开始检测");
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                                {
                                    DebugUtil.Log("开始检测!");
                                    //AssetUtil.DeleteAsset(ref filePath);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(folderPath))
                        {
                            folderPath = "请选择待检测文件夹...";
                        }
                        GUILayout.Label(folderPath, "WordWrapLabel", GUILayout.Width(200));
                        GUILayout.EndVertical();
                    }
                
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndVertical();
            }
        }
    
        /// <summary>
        /// 执行检测
        /// </summary>
        public static void Detect()
        {
            // 获取目录下所有的模型
            var assetPath = PathUtil.GetAssetPath(folderPath);
            var guids = AssetDatabase.FindAssets("t:Model", new[] {assetPath});
            var assetPaths = new List<string>(guids.Select(AssetDatabase.GUIDToAssetPath));

            #region 剥离路径和文件名并打印出来

            var allFBXPath = new List<string>();

            var allFBXName = new List<string>();
            foreach (var file in assetPaths)
            {
                var fileName = file.Split('/').Last();
                fileName = fileName.Split('\\').Last();
                allFBXPath.Add(file);
                allFBXName.Add(fileName);
            }

            File.WriteAllLines(@"C:\PrintVirtual.md", allFBXName);

            #endregion

            DebugUtil.Log($"待检测文件一共有 {allFBXPath.Count} 个!");

            // 开始检测
            var fixedCountUV2 = 0;
            var fixedCountUV3 = 0;
            var fixedCountUV4 = 0;
            var fixedCountColors = 0;
            var result = new List<string>();

            const string PRE = @"C:\Workspace\Sausage\";

            for (var i = 0; i < allFBXPath.Count; i++)
            {
                if (ProgressBar.DisplayProgressBarCancel($"模型 UV 信息分析工具: {allFBXName[i]}", $"分析中: {i + 1} / {allFBXPath.Count}", i + 1, allFBXPath.Count))
                {
                    DebugUtil.Log($"一共检测出 {fixedCountUV2} 个 UV2");
                    DebugUtil.Log($"一共检测出 {fixedCountUV3} 个 UV3");
                    DebugUtil.Log($"一共检测出 {fixedCountUV4} 个 UV4");
                    DebugUtil.Log($"一共检测出 {fixedCountColors} 个 Colors");
                    return;
                }

                var go = AssetDatabase.LoadAssetAtPath<GameObject>(allFBXPath[i].Substring(PRE.Length));

                // 检测 MeshFilter
                var meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
                foreach (var meshFilter in meshFilters)
                {
                    var mesh = meshFilter.sharedMesh;

                    if (mesh.uv2.Length > 0) {
                        ++fixedCountUV2;
                        var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 uv2!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }

                    if (mesh.uv3.Length > 0) {
                        ++fixedCountUV3;
                        var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 uv3!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }

                    if (mesh.uv4.Length > 0) {
                        ++fixedCountUV4;
                        var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 uv4!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }

                    if (mesh.colors.Length > 0) {
                        ++fixedCountColors;
                        var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 colors!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }
                }

                // 检测 SkinnedMeshRenderer
                var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    var mesh = skinnedMeshRenderer.sharedMesh;

                    if (mesh.uv2.Length > 0) {
                        ++fixedCountUV2;
                        var log = $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有uv2!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }

                    if (mesh.uv3.Length > 0) {
                        ++fixedCountUV3;
                        var log = $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有uv3!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }

                    if (mesh.uv4.Length > 0) {
                        ++fixedCountUV4;
                        var log = $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有uv4!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }

                    if (mesh.colors.Length > 0) {
                        ++fixedCountColors;
                        var log =
                            $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有colors!";
                        result.Add(log);
                        DebugUtil.Log(log, mesh);
                    }
                }
            }

            // 统计结果
            DebugUtil.Log($"一共检测出 {fixedCountUV2} 个 UV2");
            DebugUtil.Log($"一共检测出 {fixedCountUV3} 个 UV3");
            DebugUtil.Log($"一共检测出 {fixedCountUV4} 个 UV4");
            DebugUtil.Log($"一共检测出 {fixedCountColors} 个 Colors");

            // 输出结果到文件中
            File.WriteAllLines(@"C:\PrintCollider.md", result);
        }
    }
}
