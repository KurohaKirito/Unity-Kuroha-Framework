using System;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Util.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEngine;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public static class SetTextureImportSettings {
        private static int counter;
        private static bool foldout = true;
        private const float UI_DEFAULT_MARGIN = 5;
        private const float UI_BUTTON_WIDTH = 120;
        private const float UI_BUTTON_HEIGHT = 25;

        private static readonly string[] arrayMaxSizeText = {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192"
        };

        private static readonly int[] arrayMaxSize = {
            32, 64, 128, 256, 512, 1024, 2048, 4096, 8192
        };

        private static bool pcSetting;
        private static int pcMaxSize = 512;
        private static bool pcUseOriginalSize;
        private static bool pcForceOneType;
        private static TextureResizeAlgorithm pcResizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        private static TextureImporterFormat PC_FORMAT_RGB = TextureImporterFormat.DXT1;
        private static TextureImporterFormat PC_FORMAT_RGBA = TextureImporterFormat.DXT5;

        private static bool iosSetting;
        private static int iosMaxSize = 256;
        private static bool iosUseOriginalSize;
        private static bool iosForceOneType;
        private static TextureResizeAlgorithm iosResizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        private static TextureCompressionQuality iosCompressionQuality = TextureCompressionQuality.Normal;
        private static TextureImporterFormat IOS_FORMAT_RGB = TextureImporterFormat.ASTC_6x6;
        private static TextureImporterFormat IOS_FORMAT_RGBA = TextureImporterFormat.ASTC_6x6;

        private static bool androidSetting;
        private static int androidMaxSize = 256;
        private static bool androidUseOriginalSize;
        private static bool androidForceOneType;
        private static TextureResizeAlgorithm androidResizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        private static TextureCompressionQuality androidCompressionQuality = TextureCompressionQuality.Normal;
        private static TextureImporterFormat ANDROID_FORMAT_RGB = TextureImporterFormat.ETC2_RGB4;
        private static TextureImporterFormat ANDROID_FORMAT_RGBA = TextureImporterFormat.ETC2_RGBA8;

        private static bool setFromFile;
        private static string filePath;

        private static bool setFromFolder;
        private static string fileFolder;
        private static string[] guids;

        private static bool setFromSelect = true;
        private static Vector2 scrollView;
        private static UnityEngine.Object[] objects;

        public static void OnGUI() {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            foldout = EditorGUILayout.Foldout(foldout, AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.SetTextureImportSettings], true);

            if (foldout) {
                GUILayout.Space(UI_DEFAULT_MARGIN);

                ImporterParameter();

                GUILayout.Space(UI_DEFAULT_MARGIN);

                SetFromFile();

                GUILayout.Space(UI_DEFAULT_MARGIN);

                SetFromFolder();

                GUILayout.Space(UI_DEFAULT_MARGIN);

                SetFromSelect();
            }
        }

        private static void ImporterParameter() {
            PCSettings();

            GUILayout.Space(UI_DEFAULT_MARGIN);

            IOSSettings();

            GUILayout.Space(UI_DEFAULT_MARGIN);

            AndroidSettings();
        }

        private static void PCSettings() {
            GUILayout.BeginVertical("Box");
            {
                pcSetting = EditorGUILayout.ToggleLeft("PC Importer Settings", pcSetting);
                if (pcSetting) {
                    pcUseOriginalSize = EditorGUILayout.ToggleLeft("Use Original Size", pcUseOriginalSize);
                    pcForceOneType = EditorGUILayout.ToggleLeft("强制指定类型", pcForceOneType);
                    UnityEngine.GUI.enabled = !pcUseOriginalSize;
                    pcMaxSize = EditorGUILayout.IntPopup("Max Size", pcMaxSize, arrayMaxSizeText, arrayMaxSize);
                    UnityEngine.GUI.enabled = true;
                    pcResizeAlgorithm = (TextureResizeAlgorithm) EditorGUILayout.EnumPopup("Resize Algorithm", pcResizeAlgorithm);
                    if (pcForceOneType) {
                        PC_FORMAT_RGB = (TextureImporterFormat) EditorGUILayout.EnumPopup("强制指定格式", PC_FORMAT_RGB);
                    } else {
                        PC_FORMAT_RGB = (TextureImporterFormat) EditorGUILayout.EnumPopup("RGB Format", PC_FORMAT_RGB);
                        PC_FORMAT_RGBA = (TextureImporterFormat) EditorGUILayout.EnumPopup("RGBA Format", PC_FORMAT_RGBA);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static void IOSSettings() {
            GUILayout.BeginVertical("Box");
            {
                iosSetting = EditorGUILayout.ToggleLeft("IOS Importer Settings", iosSetting);
                if (iosSetting) {
                    iosUseOriginalSize = EditorGUILayout.ToggleLeft("Use Original Size", iosUseOriginalSize);
                    iosForceOneType = EditorGUILayout.ToggleLeft("强制指定类型", iosForceOneType);
                    UnityEngine.GUI.enabled = !iosUseOriginalSize;
                    iosMaxSize = EditorGUILayout.IntPopup("Max Size", iosMaxSize, arrayMaxSizeText, arrayMaxSize);
                    UnityEngine.GUI.enabled = true;
                    iosResizeAlgorithm = (TextureResizeAlgorithm) EditorGUILayout.EnumPopup("Resize Algorithm", iosResizeAlgorithm);
                    iosCompressionQuality = (UnityEditor.TextureCompressionQuality) EditorGUILayout.EnumPopup("Compression Quality", iosCompressionQuality);
                    if (iosForceOneType) {
                        IOS_FORMAT_RGB = (TextureImporterFormat) EditorGUILayout.EnumPopup("强制指定格式", IOS_FORMAT_RGB);
                    } else {
                        IOS_FORMAT_RGB = (TextureImporterFormat) EditorGUILayout.EnumPopup("RGB Format", IOS_FORMAT_RGB);
                        IOS_FORMAT_RGBA = (TextureImporterFormat) EditorGUILayout.EnumPopup("RGBA Format", IOS_FORMAT_RGBA);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static void AndroidSettings() {
            GUILayout.BeginVertical("Box");
            {
                androidSetting = EditorGUILayout.ToggleLeft("Android Importer Settings", androidSetting);
                if (androidSetting) {
                    androidUseOriginalSize = EditorGUILayout.ToggleLeft("Use Original Size", androidUseOriginalSize);
                    androidForceOneType = EditorGUILayout.ToggleLeft("强制指定类型", androidForceOneType);
                    UnityEngine.GUI.enabled = !androidUseOriginalSize;
                    androidMaxSize = EditorGUILayout.IntPopup("Max Size", androidMaxSize, arrayMaxSizeText, arrayMaxSize);
                    UnityEngine.GUI.enabled = true;
                    androidResizeAlgorithm = (TextureResizeAlgorithm) EditorGUILayout.EnumPopup("Resize Algorithm", androidResizeAlgorithm);
                    androidCompressionQuality = (UnityEditor.TextureCompressionQuality) EditorGUILayout.EnumPopup("Compression Quality", androidCompressionQuality);
                    if (androidForceOneType) {
                        ANDROID_FORMAT_RGB = (TextureImporterFormat) EditorGUILayout.EnumPopup("强制指定格式", ANDROID_FORMAT_RGB);
                    } else {
                        ANDROID_FORMAT_RGB = (TextureImporterFormat) EditorGUILayout.EnumPopup("RGB Format", ANDROID_FORMAT_RGB);
                        ANDROID_FORMAT_RGBA = (TextureImporterFormat) EditorGUILayout.EnumPopup("RGBA Format", ANDROID_FORMAT_RGBA);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static void SetFromFile() {
            GUILayout.BeginVertical("Box");
            {
                setFromFile = EditorGUILayout.ToggleLeft("使用方式一:  使用路径整合文件来批量修改设置", setFromFile);
                if (setFromFile) {
                    setFromFolder = false;
                    setFromSelect = false;
                    GUISetFromFile();
                }
            }
            GUILayout.EndVertical();
        }

        private static void GUISetFromFile() {
            GUILayout.Space(UI_DEFAULT_MARGIN);

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

                    GUILayout.Label("2. 点击按钮, 设置导入.");
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("Set Importer", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                            counter = 0;
                            var toSets = System.IO.File.ReadAllLines(filePath);

                            for (var i = 0; i < toSets.Length; i++) {
                                if (ProgressBar.DisplayProgressBarCancel("正在设置导入", $"{i + 1}/{toSets.Length}", i + 1, toSets.Length)) {
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
                    if (string.IsNullOrEmpty(filePath)) {
                        filePath = "请选择文件...";
                    }

                    GUILayout.Label(filePath, "WordWrapLabel", GUILayout.Width(120));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private static void SetFromFolder() {
            GUILayout.BeginVertical("Box");
            {
                setFromFolder = EditorGUILayout.ToggleLeft("使用方式二:  对指定目录下的全部纹理进行批量设置", setFromFolder);
                if (setFromFolder) {
                    setFromFile = false;
                    setFromSelect = false;
                    GUISetFromFolder();
                }
            }
            GUILayout.EndVertical();
        }

        private static void GUISetFromFolder() {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            EditorGUILayout.LabelField("1. 请选择待设置纹理所在的文件夹.");

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(UI_BUTTON_WIDTH + 60));
                {
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("Select Folder", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                            fileFolder = EditorUtility.OpenFolderPanel("Select Folder", fileFolder, "");
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(UI_DEFAULT_MARGIN);

                    GUILayout.Label("2. 点击按钮, 查找资源.");
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("Search Texture", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                            if (fileFolder != null) {
                                fileFolder = PathUtil.GetAssetPath(fileFolder);
                                guids = AssetDatabase.FindAssets("t:Texture", new[] {
                                    fileFolder
                                });
                                GUI.Editor.Dialog.Display($"一共找到了 {guids.Length} 张贴图", Dialog.DialogType.Message, "我知道了!");
                                counter = 0;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(UI_DEFAULT_MARGIN);

                    GUILayout.Label("3. 点击按钮, 设置导入.");
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUILayout.Button("Set Importer", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                            if (guids != null) {
                                for (var index = 0; index < guids.Length; index++) {
                                    if (GUI.Editor.ProgressBar.DisplayProgressBarCancel("贴图处理中", $"{index + 1}/{guids.Length}", index + 1, guids.Length)) {
                                        break;
                                    }

                                    var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                                    SetImporterForTexture(path);
                                }

                                GUI.Editor.Dialog.Display($"一共设置了 {counter} 张贴图", Dialog.DialogType.Message, "Nice!");
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
                    if (string.IsNullOrEmpty(fileFolder)) {
                        fileFolder = "请选择路径...";
                    }

                    GUILayout.Label(fileFolder, "WordWrapLabel", GUILayout.Width(120));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private static void SetFromSelect() {
            GUILayout.BeginVertical("Box");
            {
                setFromSelect = EditorGUILayout.ToggleLeft("使用方式三:  手动选择需要进行批量设置的纹理", setFromSelect);
                if (setFromSelect) {
                    setFromFile = false;
                    setFromFolder = false;
                    GUISetFromSelect();
                }
            }
            GUILayout.EndVertical();
        }

        private static void GUISetFromSelect() {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            EditorGUILayout.LabelField("1. 请手动选择需要进行批量设置的纹理.");

            if (Selection.objects != null) {
                objects = Selection.objects;
                GUILayout.BeginHorizontal("Box");
                {
                    scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.Height(155));
                    for (var index = 0; index < objects.Length; index++) {
                        EditorGUILayout.ObjectField($"序号: {index + 1:00}", objects[index], typeof(UnityEngine.Object), false);
                    }

                    GUILayout.EndScrollView();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(UI_DEFAULT_MARGIN);

            GUILayout.Label("2. 点击按钮, 设置导入.");
            GUILayout.BeginHorizontal("Box");
            {
                if (GUILayout.Button("Set Importer", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                    if (objects != null) {
                        for (var index = 0; index < objects.Length; index++) {
                            if (GUI.Editor.ProgressBar.DisplayProgressBarCancel("贴图处理中", $"{index + 1}/{objects.Length}", index + 1, objects.Length)) {
                                break;
                            }

                            var path = AssetDatabase.GetAssetPath(objects[index]);
                            SetImporterForTexture(path);
                        }

                        GUI.Editor.Dialog.Display($"一共设置了 {counter} 张贴图", Dialog.DialogType.Message, "Nice!");
                        fileFolder = "已设置导入!";
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private static void SetImporterForTexture(string assetPath) {
            if (pcSetting || iosSetting || androidSetting) {
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null) {
                    var flag = false;

                    if (pcSetting) {
                        if (PCSetImporterForTexture(importer)) {
                            flag = true;
                        }
                    }

                    if (iosSetting) {
                        if (IOSSetImporterForTexture(importer)) {
                            flag = true;
                        }
                    }

                    if (androidSetting) {
                        if (AndroidSetImporterForTexture(importer)) {
                            flag = true;
                        }
                    }

                    if (flag) {
                        importer.SaveAndReimport();
                        counter++;
                    }
                }
            }
        }

        private static bool PCSetImporterForTexture(TextureImporter importer) {
            if (pcUseOriginalSize) {
                TextureUtil.GetTextureOriginalSize(importer, out var originWidth, out var originHeight);
                pcMaxSize = Mathf.Max(originWidth, originHeight);
            }

            var hadAlpha = importer.DoesSourceTextureHaveAlpha();
            importer.alphaSource = hadAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;

            if (importer.GetPlatformTextureSettings("Standalone", out var curSize, out var format)) {
                if (curSize <= pcMaxSize) {
                    if (hadAlpha && format == PC_FORMAT_RGBA ||
                        !hadAlpha && format == PC_FORMAT_RGB) {
                        return false;
                    }
                }
            }

            var newSetting = new TextureImporterPlatformSettings {
                name = "Standalone",
                overridden = true,
                maxTextureSize = curSize <= pcMaxSize ? curSize : pcMaxSize,
                resizeAlgorithm = pcResizeAlgorithm,
                format = hadAlpha ? PC_FORMAT_RGBA : PC_FORMAT_RGB
            };

            if (pcForceOneType) {
                newSetting.format = PC_FORMAT_RGB;
            }

            if (importer.textureType == TextureImporterType.NormalMap) {
                newSetting.format = PC_FORMAT_RGBA;
            }

            importer.SetPlatformTextureSettings(newSetting);
            return true;
        }

        private static bool IOSSetImporterForTexture(TextureImporter importer) {
            if (iosUseOriginalSize) {
                TextureUtil.GetTextureOriginalSize(importer, out var originWidth, out var originHeight);
                iosMaxSize = Mathf.Min(originWidth, originHeight);
            }

            var hadAlpha = importer.DoesSourceTextureHaveAlpha();
            importer.alphaSource = hadAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;

            if (importer.GetPlatformTextureSettings("iPhone", out var curSize, out var format)) {
                if (curSize <= iosMaxSize) {
                    if (hadAlpha && format == IOS_FORMAT_RGBA ||
                        !hadAlpha && format == IOS_FORMAT_RGB) {
                        return false;
                    }
                }
            }

            var newSetting = new TextureImporterPlatformSettings {
                name = "iPhone",
                overridden = true,
                maxTextureSize = curSize <= iosMaxSize ? curSize : iosMaxSize,
                resizeAlgorithm = iosResizeAlgorithm,
                compressionQuality = Convert.ToInt32(iosCompressionQuality),
                format = hadAlpha ? IOS_FORMAT_RGBA : IOS_FORMAT_RGB
            };

            if (iosForceOneType) {
                newSetting.format = IOS_FORMAT_RGB;
            }

            importer.SetPlatformTextureSettings(newSetting);
            return true;
        }

        private static bool AndroidSetImporterForTexture(TextureImporter importer) {
            if (androidUseOriginalSize) {
                TextureUtil.GetTextureOriginalSize(importer, out var originWidth, out var originHeight);
                androidMaxSize = Mathf.Min(originWidth, originHeight);
            }

            var hadAlpha = importer.DoesSourceTextureHaveAlpha();
            importer.alphaSource = hadAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;

            if (importer.GetPlatformTextureSettings("Android", out var curSize, out var format)) {
                if (curSize <= androidMaxSize) {
                    if (hadAlpha && format == ANDROID_FORMAT_RGBA ||
                        !hadAlpha && format == ANDROID_FORMAT_RGB) {
                        return false;
                    }
                }
            }

            var newSetting = new TextureImporterPlatformSettings {
                name = "Android",
                overridden = true,
                maxTextureSize = curSize <= androidMaxSize ? curSize : androidMaxSize,
                resizeAlgorithm = androidResizeAlgorithm,
                compressionQuality = Convert.ToInt32(androidCompressionQuality),
                format = hadAlpha ? ANDROID_FORMAT_RGBA : ANDROID_FORMAT_RGB
            };

            if (androidForceOneType) {
                newSetting.format = ANDROID_FORMAT_RGB;
            }

            importer.SetPlatformTextureSettings(newSetting);
            return true;
        }
    }
}
