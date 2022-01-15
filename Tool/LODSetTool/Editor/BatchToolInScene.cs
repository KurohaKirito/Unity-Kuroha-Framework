using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.LODSetTool.Editor
{
    public static class BatchToolInScene
    {
        public static void PickUpAllCollider()
        {
            // 获取到预制体 (当前选中的第 1 个物体)
            var rootGameObject = Selection.activeGameObject;

            // 创建到场景中
            var root = Object.Instantiate(rootGameObject).transform;

            // ----------------------------------- 创建所有渲染器的父物体 ----------------------------------------------
            var parent = new GameObject("Mesh Renderer Parent").transform;
            parent.SetParent(root, true);

            // 获得所有的 Mesh Renderer
            var renderers = root.gameObject.GetComponentsInChildren<MeshRenderer>(true);

            // 更改层级
            foreach (var renderer in renderers)
            {
                renderer.transform.SetParent(parent, true);
            }

            // ----------------------------------- 创建所有碰撞器的父物体 ----------------------------------------------
            parent = new GameObject("Mesh Collider Parent").transform;
            parent.SetParent(root, true);

            // 获得所有的 Mesh Collider
            var colliders = root.gameObject.GetComponentsInChildren<MeshCollider>(true);

            // 更改层级
            foreach (var collider in colliders)
            {
                collider.transform.SetParent(parent, true);
            }
        }
    }
}
