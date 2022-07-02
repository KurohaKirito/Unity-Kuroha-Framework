using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using UnityEditor;
using UnityEngine;

// 检测特定路径下 fbx 模型文件的 mesh 网格中的 uv2 uv3 uv4 colors 信息
namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public static class FbxUVColorsChecker {
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

        private static bool checkUV2 = true;
        private static bool checkUV3 = true;
        private static bool checkUV4 = true;
        private static bool checkColors = true;

        private static bool clearUV2 = true;
        private static bool clearUV3 = true;
        private static bool clearUV4 = true;
        private static bool clearColors = true;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI() {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            var title = AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.FbxUVColorsChecker];
            foldout = EditorGUILayout.Foldout(foldout, title, true);

            if (foldout) {
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
                                if (GUILayout.Button("Select Folder", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                                    folderPath = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "Art");
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(UI_DEFAULT_MARGIN);

                            GUILayout.Label("2. 选择项目");

                            GUILayout.BeginHorizontal("Box");
                            {
                                GUILayout.Label("UV2\t");
                                GUILayout.Space(UI_DEFAULT_MARGIN);
                                GUILayout.Label("检测");
                                checkUV2 = EditorGUILayout.Toggle(checkUV2, GUILayout.Width(24));
                                GUILayout.Label("清除");
                                clearUV2 = EditorGUILayout.Toggle(clearUV2, GUILayout.Width(24));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal("Box");
                            {
                                GUILayout.Label("UV3\t");
                                GUILayout.Space(UI_DEFAULT_MARGIN);
                                GUILayout.Label("检测");
                                checkUV3 = EditorGUILayout.Toggle(checkUV3, GUILayout.Width(24));
                                GUILayout.Label("清除");
                                clearUV3 = EditorGUILayout.Toggle(clearUV3, GUILayout.Width(24));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal("Box");
                            {
                                GUILayout.Label("UV4\t");
                                GUILayout.Space(UI_DEFAULT_MARGIN);
                                GUILayout.Label("检测");
                                checkUV4 = EditorGUILayout.Toggle(checkUV4, GUILayout.Width(24));
                                GUILayout.Label("清除");
                                clearUV4 = EditorGUILayout.Toggle(clearUV4, GUILayout.Width(24));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal("Box");
                            {
                                GUILayout.Label("Colors\t");
                                GUILayout.Space(UI_DEFAULT_MARGIN);
                                GUILayout.Label("检测");
                                checkColors = EditorGUILayout.Toggle(checkColors, GUILayout.Width(24));
                                GUILayout.Label("清除");
                                clearColors = EditorGUILayout.Toggle(clearColors, GUILayout.Width(24));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(UI_DEFAULT_MARGIN);

                            GUILayout.Label("3. 点击按钮, 开始检测");
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Start", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                                    Detect();
                                    DebugUtil.Log("检测结束!", null, EditorGUIUtility.isProSkin? "yellow" : "black");
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(folderPath)) {
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
        private static void Detect() {
            var uv2Counter = 0;
            var uv3Counter = 0;
            var uv4Counter = 0;
            var colorsCounter = 0;
            var result = new List<string>();

            // 获取目录下所有的模型路径
            var assetPath = PathUtil.GetAssetPath(folderPath);
            var guids = AssetDatabase.FindAssets("t:Mesh", new[] {
                assetPath
            });
            var assetPaths = new List<string>(guids.Select(AssetDatabase.GUIDToAssetPath));

            // 开始检测
            for (var i = 0; i < assetPaths.Count; i++) {
                if (ProgressBar.DisplayProgressBarCancel("UV 分析工具", $"分析中: {i + 1} / {assetPaths.Count}", i + 1, assetPaths.Count)) {
                    LogResult(uv2Counter, uv3Counter, uv4Counter, colorsCounter);
                    return;
                }

                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPaths[i]);
                CheckMesh("Mesh", assetPaths[i], result, mesh, ref uv2Counter, ref uv3Counter, ref uv4Counter, ref colorsCounter);
            }

            // 保存刷新
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 统计结果
            LogResult(uv2Counter, uv3Counter, uv4Counter, colorsCounter);

            // 输出结果到文件中
            File.WriteAllLines(@"C:\PrintCollider.md", result);
        }

        /// <summary>
        /// 检查 Mesh
        /// </summary>
        private static void CheckMesh(string type, string path, in List<string> result, Mesh mesh, ref int uv2, ref int uv3, ref int uv4, ref int colors) {
            if (checkUV2 && mesh.uv2.Length > 0) {
                ++uv2;
                var log = $"{type}: 模型 {path} 中的 {mesh.name} 具有 uv2!";
                result.Add(log);

                if (clearUV2) {
                    mesh.SetUVs(2, (List<Vector2>) null);
                    // mesh.uv2 = null;
                    EditorUtility.SetDirty(mesh);
                }
            }

            if (checkUV3 && mesh.uv3.Length > 0) {
                ++uv3;
                var log = $"{type}: 模型 {path} 中的 {mesh.name} 具有 uv3!";
                result.Add(log);

                if (clearUV3) {
                    mesh.SetUVs(3, (List<Vector2>) null);
                    // mesh.uv3 = null;
                    EditorUtility.SetDirty(mesh);
                }
            }

            if (checkUV4 && mesh.uv4.Length > 0) {
                ++uv4;
                var log = $"{type}: 模型 {path} 中的 {mesh.name} 具有 uv4!";
                result.Add(log);

                if (clearUV4) {
                    mesh.SetUVs(4, (List<Vector2>) null);
                    // mesh.uv4 = null;
                    EditorUtility.SetDirty(mesh);
                }
            }

            if (checkColors && mesh.colors.Length > 0) {
                ++colors;
                var log = $"{type}: 模型 {path} 中的 {mesh.name} 具有 colors!";
                result.Add(log);

                if (clearColors) {
                    mesh.SetColors((List<Color>) null);
                    // mesh.colors = null;
                    EditorUtility.SetDirty(mesh);
                }
            }
        }

        /// <summary>
        /// Log
        /// </summary>
        private static void LogResult(int uv2, int uv3, int uv4, int colors) {
            DebugUtil.Log($"一共检测出 {uv2} 个 UV2");
            DebugUtil.Log($"一共检测出 {uv3} 个 UV3");
            DebugUtil.Log($"一共检测出 {uv4} 个 UV4");
            DebugUtil.Log($"一共检测出 {colors} 个 Colors");
        }
    }
}
