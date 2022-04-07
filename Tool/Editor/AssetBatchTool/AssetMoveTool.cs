using Script.Effect.Editor.AssetTool.Util.Editor;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public static class AssetMoveTool {
        /// <summary>
        /// 整合了需要批量移动的资源所在路径的文件
        /// </summary>
        private static string filePath = string.Empty;

        /// <summary>
        /// 目标路径
        /// </summary>
        private static string folderPath = "Assets/Art/Effects/Materials/MaterialEditor";

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

        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 300;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI() {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int)AssetBatchToolGUI.BatchType.AssetMoveTool], true);

            if (foldout) {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 请选择整合了需要批量移动目录的资源所在路径的文件.");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("注: 文件中的路径必须以 Assets 开头或者绝对路径.");
                    EditorGUI.indentLevel--;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical(GUILayout.Width(UI_BUTTON_WIDTH + 60));
                        {
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Select File", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                                    filePath = EditorUtility.OpenFilePanel("Select File", filePath, "");
                                }
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(UI_DEFAULT_MARGIN);

                            GUILayout.Label("2. 输入目标路径.");
                            GUILayout.BeginVertical("Box");
                            folderPath = GUILayout.TextField(folderPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                            GUILayout.EndVertical();

                            GUILayout.Label("3. 点击按钮, 移动资源.");
                            GUILayout.BeginHorizontal("Box");
                            {
                                if (GUILayout.Button("Move Assets", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                                    AssetUtil.MoveFileToNewFolder(ref filePath, folderPath);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(filePath)) {
                            filePath = "请选择文件...";
                        }

                        GUILayout.Label(filePath, "WordWrapLabel", GUILayout.Width(120));
                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }
        }
    }
}