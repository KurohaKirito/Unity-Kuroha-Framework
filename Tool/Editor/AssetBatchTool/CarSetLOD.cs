using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.GUI.Editor;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool {
    public class CarSetLOD : UnityEditor.EditorWindow {
        private static string path = "Assets/ToBundle/Skin/Cars";
        private static readonly List<GameObject> prefabs = new List<GameObject>();

        public static void Open() {
            GetWindow<CarSetLOD>();
        }

        private void OnGUI() {
            path = EditorGUILayout.TextField("检测路径", path);

            for (var i = 0; i < prefabs.Count; i++) {
                prefabs[i] = EditorGUILayout.ObjectField("车辆预制", prefabs[i], typeof(GameObject), true) as GameObject;
            }

            if (GUILayout.Button("搜寻预制")) {
                SearchPrefab();
            }

            if (GUILayout.Button("设置 LOD")) {
                SetLOD();
            }
        }

        private static void SearchPrefab() {
            var assetGuids = AssetDatabase.FindAssets("t:Prefab", new[] {
                path
            });

            for (var index = 0; index < assetGuids.Length; index++) {
                ProgressBar.DisplayProgressBar("特效检测工具", $"Prefab 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);

                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (asset != null) {
                    prefabs.Add(asset);
                }
            }
        }

        private static void SetLOD() {
            // FileUtil.GetProjectRelativePath();
            foreach (var asset in prefabs)
            {
                if (asset != null)
                {
                    asset.hideFlags = HideFlags.None;
                    var oldLodGroup = asset.GetComponent<LODGroup>();

                    if (oldLodGroup != null) {
                        DebugUtil.LogError($"预制体 {AssetDatabase.GetAssetPath(asset)} 的根物体已经有 LOD Group 了!", asset, "red");
                        var lodArray = oldLodGroup.GetLODs();
                        lodArray[0].screenRelativeTransitionHeight = 0.01f;
                        oldLodGroup.SetLODs(lodArray);
                        oldLodGroup.RecalculateBounds();
                    }
                    else
                    {
                        var lodGroup = asset.AddComponent<LODGroup>();
                        var lodArray = new LOD[1];
                        var rendererArray = asset.GetComponentsInChildren<Renderer>(true);
                        lodArray[0] = new LOD(0.01f, rendererArray);
                        lodGroup.SetLODs(lodArray);
                        lodGroup.RecalculateBounds();
                    }

                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}