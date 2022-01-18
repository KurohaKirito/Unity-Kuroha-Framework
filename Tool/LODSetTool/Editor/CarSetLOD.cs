using System.Collections.Generic;
using Kuroha.GUI.Editor;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.LODSetTool.Editor
{
    public class CarSetLOD : UnityEditor.EditorWindow
    {
        private static float lodValue;
        private static Vector2 scroll;
        private static string path = "Assets/ToBundle/Skin/Cars";
        private static readonly List<string> paths = new List<string>();
        private static readonly List<GameObject> prefabs = new List<GameObject>();

        public static void Open()
        {
            GetWindow<CarSetLOD>();
        }

        private void OnGUI()
        {
            EditorGUILayout.IntField("预制体数量", prefabs.Count);
            lodValue = EditorGUILayout.FloatField("LOD 剔除占比", lodValue);
            path = EditorGUILayout.TextField("检测路径", path);

            scroll = GUILayout.BeginScrollView(scroll);
            for (var i = 0; i < prefabs.Count; i++)
            {
                prefabs[i] = EditorGUILayout.ObjectField("车辆预制", prefabs[i], typeof(GameObject), true) as GameObject;
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("搜寻预制"))
            {
                SearchPrefab();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("清空预制"))
            {
                paths.Clear();
                prefabs.Clear();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("设置 LOD"))
            {
                SetLOD();
            }

            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }

        private static void SearchPrefab()
        {
            var assetGuids = AssetDatabase.FindAssets("t:Prefab", new[]
            {
                path
            });

            for (var index = 0; index < assetGuids.Length; index++)
            {
                ProgressBar.DisplayProgressBar("特效检测工具", $"Prefab 排查中: {index + 1}/{assetGuids.Length}", index + 1, assetGuids.Length);

                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (asset != null)
                {
                    if (paths.Contains(assetPath) == false)
                    {
                        prefabs.Add(asset);
                        paths.Add(assetPath);
                    }
                }
            }
        }

        private static void SetLOD()
        {
            foreach (var asset in prefabs)
            {
                if (asset != null)
                {
                    asset.hideFlags = HideFlags.None;
                    var oldLodGroup = asset.GetComponent<LODGroup>();

                    if (oldLodGroup != null)
                    {
                        var lodArray = oldLodGroup.GetLODs();
                        lodArray[0].screenRelativeTransitionHeight = lodValue;
                        oldLodGroup.SetLODs(lodArray);
                        oldLodGroup.RecalculateBounds();

                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    // else
                    // {
                    //     var lodGroup = asset.AddComponent<LODGroup>();
                    //     var lodArray = new LOD[1];
                    //     var rendererArray = asset.GetComponentsInChildren<Renderer>(true);
                    //     lodArray[0] = new LOD(0.01f, rendererArray);
                    //     lodGroup.SetLODs(lodArray);
                    //     lodGroup.RecalculateBounds();
                    // }
                }
            }
        }
    }
}
