using System;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.ProjectExtender {
    public class MeshReadableShow {
        private const float WIDTH = 30f;
        
        /// <summary>
        /// 扩展 Project 布局
        /// </summary>
        // 由于扩展布局的方法无法进行手动调用, 所以使用这个属性进行自动调用
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            // 对 projectWindowItemOnGUI 进行注册
            EditorApplication.projectWindowItemOnGUI += OnGUI;
        }

        private static void OnGUI(string guid, Rect selectionRect) {
            // 定义绘制按钮时需要用到的矩形, selectionRect 原本是当前选中时 Unity 中的蓝色的高亮矩形框.
            var rect = new Rect(
                x: selectionRect.x += selectionRect.width - WIDTH - 3,
                y: selectionRect.y + 1,
                width: WIDTH,
                height: selectionRect.height - 2);
            
            // 获取资源
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.IndexOf(".mesh", StringComparison.OrdinalIgnoreCase) > 0 ||
                assetPath.IndexOf(".asset", StringComparison.OrdinalIgnoreCase) > 0) {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                if (mesh != null) {
                    UnityEngine.GUI.Label(rect, mesh.isReadable ? "ON" : "OFF");
                }
            } else if (assetPath.IndexOf(".fbx", StringComparison.OrdinalIgnoreCase) > 0) {
                if (AssetImporter.GetAtPath(assetPath) is ModelImporter model) {
                    UnityEngine.GUI.Label(rect, model.isReadable ? "ON" : "OFF");
                }
            }
        }
    }
}
