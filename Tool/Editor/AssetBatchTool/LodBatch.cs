using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using Script.Effect.Editor.AssetTool.Util.Unity;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public static class LodBatch {
        [MenuItem("GameObject/LODTool", false, 12)]
        public static void Batch() {
            LodWindow.Open();
        }
    }

    public class LodWindow : EditorWindow {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 400;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 所有的 LOD Group
        /// </summary>
        private static List<LODGroup> lodGroups;

        /// <summary>
        /// LOD 默认级别数量
        /// </summary>
        private int lodCount = 3;

        /// <summary>
        /// LOD 的级别百分比设置
        /// </summary>
        private float[] lodValues;

        public static void Open() {
            RefreshSelection();
            GetWindow<LodWindow>("LodGroup 百分比设置");
        }

        private void OnSelectionChange() {
            RefreshSelection();
            Repaint();
        }

        public void OnGUI() {
            #region 绘制界面

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            lodCount = EditorGUILayout.IntField("LOD Count: (1 - 10)", lodCount, GUILayout.Width(UI_INPUT_AREA_WIDTH));

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            if (lodValues == null) {
                lodValues = new float[3];
            }

            if (lodCount > 0 && lodCount <= 10 && lodCount != lodValues.Length) {
                lodValues = new float[lodCount];
            }

            for (var index = 0; index < lodValues.Length; index++) {
                if (index == lodValues.Length - 1) {
                    lodValues[index] = EditorGUILayout.FloatField("Culled:", lodValues[index], GUILayout.Width(UI_INPUT_AREA_WIDTH));
                } else {
                    lodValues[index] = EditorGUILayout.FloatField($"LOD {index + 1}:", lodValues[index], GUILayout.Width(UI_INPUT_AREA_WIDTH));
                }

                GUILayout.Space(UI_DEFAULT_MARGIN * 2);
            }

            #endregion

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("批量设置", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                BatchLOD();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("强制刷新选择", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH))) {
                RefreshSelection();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"当前选中的 LOD Group 个数: {lodGroups.Count}");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void BatchLOD() {
            RefreshSelection();

            foreach (var lodGroup in lodGroups) {
                var originLods = lodGroup.GetLODs();

                #region LOD 级别数相等

                if (originLods.Length == lodValues.Length) {
                    for (var i = 0; i < originLods.Length; i++) {
                        originLods[i].screenRelativeTransitionHeight = lodValues[i] / 100.0f;
                    }

                    lodGroup.SetLODs(originLods);
                }

                #endregion

                #region 新的级别数较多

                else if (originLods.Length < lodValues.Length) {
                    for (var i = 0; i < originLods.Length; i++) {
                        if (i < originLods.Length - 1) {
                            originLods[i].screenRelativeTransitionHeight = lodValues[i] / 100.0f;
                        } else if (i == originLods.Length - 1) {
                            originLods[i].screenRelativeTransitionHeight = lodValues[lodValues.Length - 1] / 100.0f;
                        }
                    }

                    lodGroup.SetLODs(originLods);
                }

                #endregion

                #region 新的级别数较少

                else if (originLods.Length > lodValues.Length) {
                    var obj = lodGroup.gameObject;
                    DebugUtil.LogError($"错误: 游戏物体 {obj.name} 的 LOD 级别数 ({originLods.Length}) 比当前需要设置的级别数 ({lodValues.Length}) 多, 已跳过此物体.", obj);
                }

                #endregion
            }

            Debug.Log("批量设置成功!");
        }

        private static void RefreshSelection() {
            if (lodGroups == null) {
                lodGroups = new List<LODGroup>();
            } else {
                lodGroups.Clear();
            }

            var selections = Selection.GetTransforms(SelectionMode.Unfiltered);
            foreach (var trans in selections) {
                if (trans.TryGetComponent<LODGroup>(out var lod)) {
                    if (ReferenceEquals(lod, null) == false) {
                        lodGroups.Add(lod);
                    }
                }
            }
        }
    }
}