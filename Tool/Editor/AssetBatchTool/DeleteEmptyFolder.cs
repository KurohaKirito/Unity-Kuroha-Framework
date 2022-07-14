using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public class DeleteEmptyFolder {
        /// <summary>
        /// 整合了需要批量删除的资源所在路径的文件
        /// </summary>
        private static string selectFolder = string.Empty;

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
        /// 绘制界面
        /// </summary>
        public static void OnGUI() {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int)AssetBatchToolGUI.BatchType.DeleteEmptyFolder], true);
            if (foldout) {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    #region 绘制标题
                
                    EditorGUILayout.LabelField("1. 请选择整合了需要批量删除的资源所在路径的文件.");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("注: 文件中的路径必须以 Assets 开头或者绝对路径.");
                    EditorGUI.indentLevel--;

                    #endregion
                
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical(GUILayout.Width(UI_BUTTON_WIDTH + 60));
                        {
                            #region 目录选择器

                            GUILayout.BeginHorizontal("Box");
                            if (GUILayout.Button("Select Folder", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                                selectFolder = EditorUtility.OpenFolderPanel("Select Folder", selectFolder, "");
                            }
                            GUILayout.EndHorizontal();

                            #endregion
                            
                            GUILayout.Space(UI_DEFAULT_MARGIN);
                            
                            #region 执行按钮

                            GUILayout.Label("2. 点击按钮, 执行检测.");
                            GUILayout.BeginHorizontal("Box");
                            if (GUILayout.Button("Delete", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                                EmptyFolder();
                            }
                            GUILayout.EndHorizontal();

                            #endregion
                        }
                        GUILayout.EndVertical();

                        #region 文件夹显示

                        GUILayout.BeginVertical();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(selectFolder)) {
                            selectFolder = "请选择文件夹...";
                        }
                        GUILayout.Label(selectFolder, "WordWrapLabel", GUILayout.Width(200));
                        GUILayout.EndVertical();

                        #endregion
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }
        
        private static void EmptyFolder() {
            var counter = 0;
            
            // 创建根目录
            var dirList = new List<DirectoryInfo>();
            var tempDir = new DirectoryInfo(selectFolder);
            dirList.Add(tempDir);
            
            // 根据选择的目录构建目录链表
            for (var index = 0; index < dirList.Count; index++) {
                var tempDirs = dirList[index].GetDirectories();
                dirList.AddRange(tempDirs);
            }
            
            DebugUtil.Log($"一共检测了 {dirList.Count} 个目录!", null, "#58D2BC");
            
            // 倒序遍历全部的目录, 单个目录如果是空的, 则可以删除
            for (var index = dirList.Count - 1; index >= 0; index--) {
                var total = dirList.Count - 1;
                var current = total - index;
                ProgressBar.DisplayProgressBar("目录检测中", $"进度: {current}/{total}", current, total);
                if (dirList[index].FullName.Contains(".git") || dirList[index].FullName.Contains("AutoCreat\\Temporary") || dirList[index].FullName.Contains("Assets\\Packages")) {
                    continue;
                }
                var dirCount = dirList[index].GetDirectories().Length;
                var files = dirList[index].GetFiles("*.*", SearchOption.TopDirectoryOnly);
                var metaCount = files.Count(f => f.Name.EndsWith(".meta"));
                var fileCount = files.Count(f => f.Name.EndsWith(".meta") == false);
                if (dirCount == 0 && fileCount == 0 && metaCount == 0) {
                    DebugUtil.Log($"目录 {dirList[index].FullName} 是空的!", null, "yellow");
                    AssetDatabase.DeleteAsset(PathUtil.GetAssetPath(dirList[index].FullName));
                    counter++;
                }
            }
            
            DebugUtil.Log($"一共移除了 {counter} 个空目录!", null, "green");
        }
    }
}
