using Kuroha.Framework.GUI.Editor;
using Kuroha.Framework.Utility.RunTime;
using Kuroha.Util.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.AssetBatchTool.Editor
{
    public static class SetTextureImportSettings
    {
        private static string fileFolder;
        private static string[] guids;
        private static int counter;
        
        /// <summary>
        /// 整合了需要批量移动的资源所在路径的文件
        /// </summary>
        private static string filePath = string.Empty;

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
        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.SetTextureImportSettings], true);

            if (foldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);

                DrawFromFile();

                GUILayout.Space(UI_DEFAULT_MARGIN);

                DrawFromFolder();
            }
        }

        private static void DrawFromFile()
        {
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
                            if (GUILayout.Button("Select File", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                            {
                                filePath = EditorUtility.OpenFilePanel("Select File", filePath, "");
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(UI_DEFAULT_MARGIN);

                        GUILayout.Label("2. 点击按钮, 设置导入.");
                        GUILayout.BeginHorizontal("Box");
                        {
                            if (GUILayout.Button("Set Importer", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                            {
                                counter = 0;
                                var toSets = System.IO.File.ReadAllLines(filePath);

                                for (var i = 0; i < toSets.Length; i++)
                                {
                                    if (ProgressBar.DisplayProgressBarCancel("正在设置导入", $"{i + 1}/{toSets.Length}", i + 1, toSets.Length))
                                    {
                                        break;
                                    }

                                    SetImporterForTexture(toSets[i]);
                                }

                                DebugUtil.Log($"共成功移动了 {counter}/{toSets.Length} 项资源!", null, "green");
                                filePath = "已设置导入!";
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(filePath))
                        {
                            filePath = "请选择文件...";
                        }

                        GUILayout.Label(filePath, "WordWrapLabel", GUILayout.Width(120));
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private static void DrawFromFolder()
        {
            GUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("1. 请选择待设置纹理所在的文件夹.");

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUILayout.Width(UI_BUTTON_WIDTH + 60));
                    {
                        GUILayout.BeginHorizontal("Box");
                        {
                            if (GUILayout.Button("Select Folder", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                            {
                                fileFolder = EditorUtility.OpenFolderPanel("Select Folder", fileFolder, "");
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(UI_DEFAULT_MARGIN);

                        GUILayout.Label("2. 点击按钮, 查找资源.");
                        GUILayout.BeginHorizontal("Box");
                        {
                            if (GUILayout.Button("Search Texture", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                            {
                                if (fileFolder != null)
                                {
                                    fileFolder = PathUtil.GetAssetPath(fileFolder);
                                    guids = AssetDatabase.FindAssets("t:Texture", new[] {fileFolder});
                                    Kuroha.Framework.GUI.Editor.Dialog.Display("消息", $"一共找到了 {guids.Length} 张贴图", Dialog.DialogType.Message, "我知道了!", null, null);
                                    counter = 0;
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(UI_DEFAULT_MARGIN);

                        GUILayout.Label("3. 点击按钮, 设置导入.");
                        GUILayout.BeginHorizontal("Box");
                        {
                            if (GUILayout.Button("Set Importer", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                            {
                                if (guids != null)
                                {
                                    for (var index = 0; index < guids.Length; index++)
                                    {
                                        if (Kuroha.Framework.GUI.Editor.ProgressBar.DisplayProgressBarCancel("贴图处理中", $"{index + 1}/{guids.Length}", index + 1, guids.Length))
                                        {
                                            break;
                                        }

                                        var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                                        SetImporterForTexture(path);
                                    }

                                    Kuroha.Framework.GUI.Editor.Dialog.Display("消息", $"一共设置了 {counter} 张贴图", Dialog.DialogType.Message, "Nice!", null, null);
                                    fileFolder = "已设置导入!";
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        if (string.IsNullOrEmpty(fileFolder))
                        {
                            fileFolder = "请选择路径...";
                        }

                        GUILayout.Label(fileFolder, "WordWrapLabel", GUILayout.Width(120));
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private static void SetImporterForTexture(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                if (importer.GetPlatformTextureSettings("Standalone", out var curSize, out var format))
                {
                    if (curSize <= 512 && (format == TextureImporterFormat.DXT1 || format == TextureImporterFormat.DXT5))
                    {
                        return;
                    }
                }
                
                var newSetting = new TextureImporterPlatformSettings
                {
                    name = "Standalone",
                    overridden = true,
                    maxTextureSize = curSize <= 512 ? curSize : 512,
                    format = importer.DoesSourceTextureHaveAlpha() ? TextureImporterFormat.DXT5 : TextureImporterFormat.DXT1
                };

                if (importer.textureType == TextureImporterType.NormalMap)
                {
                    newSetting.format = TextureImporterFormat.DXT5;
                }

                importer.SetPlatformTextureSettings(newSetting);
                importer.SaveAndReimport();
                counter++;
            }
        }
    }
}
