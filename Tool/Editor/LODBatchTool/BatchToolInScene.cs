using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.LODBatchTool
{
    public static class BatchToolInScene
    {
        public static void DetachLbpRendererAndRenderer()
        {
            // 获取选中的预制体
            var prefab = Selection.activeGameObject;
            
            // 获取选中的预制体的路径
            var path = AssetDatabase.GetAssetPath(prefab);
            
            // 实例化预制体
            if (PrefabUtility.InstantiatePrefab(prefab) is GameObject prefabInstance)
            {
                // 解包预制体
                PrefabUtility.UnpackPrefabInstance(prefabInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                
                // 得到 LBP 的顶层游戏物体
                var topTransform = prefabInstance.transform;
                var regex = new Regex("_A[0-9]{3}$");
                for (var index = 0; index < topTransform.childCount; index++)
                {
                    var child = topTransform.GetChild(index);
                    var match = regex.Match(child.name);
                    if (match.Success) {
                        topTransform = child;
                        break;
                    }
                }
                
                // 创建 3 个子节点
                var rendererParent = new GameObject("Renderer Parent");
                rendererParent.transform.SetParent(topTransform);
                var colliderParent = new GameObject("Collider Parent");
                colliderParent.transform.SetParent(topTransform);
                var lodGroupParent = new GameObject("LOD Group Parent");
                lodGroupParent.transform.SetParent(topTransform);
                
                var allTransformInLbp = topTransform.GetComponentsInChildren<Transform>();
                
                // 挑选 LOD Group => lodGroupParent
                for (var index = 0; index < topTransform.childCount;)
                {
                    var child = topTransform.GetChild(index);
                    if (child.GetComponent<LODGroup>() != null) {
                        child.SetParent(lodGroupParent.transform);
                    } else {
                        index++;
                    }
                }
                
                // 挑选 Sfx => LBP
                foreach (var transform in allTransformInLbp) {
                    if (transform.name.IndexOf("sfx", StringComparison.OrdinalIgnoreCase) >= 0) {
                        var parent = transform.parent;
                        if (parent.GetComponent<Collider>() != null) {
                            for (var index = 0; index < parent.childCount; index++) {
                                if (parent.GetChild(index).name.Contains("_LBP")) {
                                    transform.SetParent(parent.GetChild(index));
                                    break;
                                }
                            }
                        }
                    }
                }
                
                // 挑选 LBP => rendererParent
                foreach (var transform in allTransformInLbp) {
                    if (transform.name.Contains("_LBP")) {
                        transform.SetParent(rendererParent.transform);
                    }
                }
                
                // 挑选 Collider => colliderParent
                for (var index = 0; index < topTransform.childCount;)
                {
                    var child = topTransform.GetChild(index);
                    if (child.GetComponent<Collider>() != null) {
                        child.SetParent(colliderParent.transform);
                    } else {
                        index++;
                    }
                }
                
                // 覆盖旧的 Prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                
                // 销毁场景中临时创建的预制体实例
                UnityEngine.Object.DestroyImmediate(prefabInstance);
                
                // 刷新
                AssetDatabase.Refresh();
            }
        }
    }
}
