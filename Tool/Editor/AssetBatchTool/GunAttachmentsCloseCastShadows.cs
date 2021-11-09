using System.IO;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public static class GunAttachmentsCloseCastShadows {
        /// <summary>
        /// 折叠框
        /// </summary>
        private static bool gunPiecesFoldout = true;

        /// <summary>
        /// 枪械路径
        /// </summary>
        private static string gunPiecesPath = "ToBundle/Skin/Items";

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
        public static void OnGUI() {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            gunPiecesFoldout = EditorGUILayout.Foldout(gunPiecesFoldout, AssetBatchToolGUI.batches[(int)AssetBatchToolGUI.BatchType.GunAttachmentsCloseCastShadows], true);

            if (gunPiecesFoldout) {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    EditorGUILayout.LabelField("1. 输入枪械预制体所在路径.");
                    GUILayout.BeginVertical("Box");
                    gunPiecesPath = EditorGUILayout.TextField("Input Path To Detect", gunPiecesPath, GUILayout.Width(UI_INPUT_AREA_WIDTH));
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField("2. 点击按钮, 开始修复.");
                    GUILayout.BeginVertical("Box");
                    UnityEngine.GUI.enabled = string.IsNullOrEmpty(gunPiecesPath) == false;
                    if (GUILayout.Button("Fix", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                        Fix();
                    }

                    UnityEngine.GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 关闭阴影投射
        /// </summary>
        private static void Fix() {
            var fullPath = Path.Combine(Application.dataPath, gunPiecesPath);
            if (!Directory.Exists(fullPath)) {
                DebugUtil.LogError("Please Enter The Correct Path! ex. 'ToBundle/Skin/Items'");
                return;
            }

            var direction = new DirectoryInfo(fullPath);
            var files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (var index = 0; index < files.Length; index++) {
                if (ProgressBar.DisplayProgressBarCancel("批处理工具", $"枪械部件阴影投射自动修复中: {index + 1}/{files.Length}", index + 1, files.Length)) {
                    return;
                }

                if (!files[index].Name.EndsWith(".prefab")) {
                    continue;
                }

                var assetPath = "Assets" + files[index].FullName.Replace(Application.dataPath.Replace("/", "\\"), "");
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                var lodGroups = asset.GetComponentsInChildren<LODGroup>(true);
                if (lodGroups.Length <= 0)
                    continue;

                // 标记资源是否被修改
                var fileEdit = false;
                // 遍历所有的 LODGroup
                foreach (var lodGroup in lodGroups) {
                    var lods = lodGroup.GetLODs();
                    if (lods.Length < 2) {
                        continue;
                    }

                    // 取出 LOD0 的所有模型
                    var renderers0 = lods[0].renderers;
                    // 取出 LOD1 的所有模型
                    var renderers1 = lods[1].renderers;

                    #region 判断是否是部件式模型

                    var isMadeByPieces = false;
                    foreach (var renderer0 in renderers0) {
                        if (isMadeByPieces) {
                            break;
                        }

                        foreach (var renderer1 in renderers1) {
                            if (isMadeByPieces) {
                                break;
                            }

                            if (renderer0 == renderer1) {
                                isMadeByPieces = true;
                            }
                        }
                    }

                    if (!isMadeByPieces) {
                        continue;
                    }

                    #endregion

                    foreach (var renderer0 in renderers0) {
                        #region 判断是否需要修改

                        var toEdit = true;
                        foreach (var renderer1 in renderers1) {
                            if (renderer0 == renderer1) {
                                toEdit = false;
                            }
                        }

                        if (!toEdit || renderer0.shadowCastingMode == ShadowCastingMode.Off) {
                            continue;
                        }

                        #endregion

                        renderer0.shadowCastingMode = ShadowCastingMode.Off;
                        fileEdit = true;
                    }
                }

                // 标记资源被修改
                if (!fileEdit)
                    continue;
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
        }
    }
}