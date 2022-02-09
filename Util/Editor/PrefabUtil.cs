using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Util.Editor
{
    public static class PrefabUtil
    {
        /// <summary>
        /// 获取图片的硬盘空间占用
        /// </summary>
        private static long GetTextureStorageMemorySize(Texture asset)
        {
            // 获取到 UnityEditor 程序集
            var dynamicAssembly = ReflectionUtil.GetAssembly(typeof(EditorWindow));
            
            // 获取到 TextureUtil 类
            var dynamicClass = ReflectionUtil.GetClass(dynamicAssembly, "UnityEditor.TextureUtil");
                
            // public static extern long GetStorageMemorySizeLong(Texture t);
            var dynamicMethod = ReflectionUtil.GetMethod(dynamicClass, "GetStorageMemorySizeLong", BindingFlags.Public | BindingFlags.Static);

            // 参数
            var paramsArray = new object[] { asset };
            
            // 调用
            var result = ReflectionUtil.CallMethod(dynamicMethod, paramsArray);
            
            return (long)result;
        }

        /// <summary>
        /// 统计实际内存占用
        /// </summary>
        /// <param name="asset"></param>
        public static void CountMemoryOfPrefab(GameObject asset)
        {
            #region 统计模型占用的内存, 内存占用的计算必须去重, 和渲染的计算不同

            var meshFilterList = asset.GetComponentsInChildren<MeshFilter>();
            var skinnedMeshRendererList = asset.GetComponentsInChildren<SkinnedMeshRenderer>();
            var meshColliderList = asset.GetComponentsInChildren<MeshCollider>();

            var meshList = new List<int>();
            foreach (var item in meshFilterList)
            {
                var mesh = item.sharedMesh;
                var meshHashCode = mesh.GetHashCode();
                if (string.IsNullOrEmpty(mesh.name) == false && meshList.Contains(meshHashCode) == false)
                {
                    meshList.Add(meshHashCode);
                    var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
                    var size = EditorUtility.FormatBytes(runTimeSize);
                    DebugUtil.Log($"网格: {mesh.name}: 当前设备的运行内存占用 (Profiler): {size}", mesh, "yellow");
                }
            }

            foreach (var item in skinnedMeshRendererList)
            {
                var mesh = item.sharedMesh;
                var meshHashCode = mesh.GetHashCode();
                if (string.IsNullOrEmpty(mesh.name) == false && meshList.Contains(meshHashCode) == false)
                {
                    meshList.Add(meshHashCode);
                    var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
                    var size = EditorUtility.FormatBytes(runTimeSize);
                    DebugUtil.Log($"网格: {mesh.name}: 当前设备的运行内存占用 (Profiler): {size}", mesh, "yellow");
                }
            }

            foreach (var item in meshColliderList)
            {
                var mesh = item.sharedMesh;
                var meshHashCode = mesh.GetHashCode();
                if (string.IsNullOrEmpty(mesh.name) == false && meshList.Contains(meshHashCode) == false)
                {
                    meshList.Add(meshHashCode);
                    var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
                    var size = EditorUtility.FormatBytes(runTimeSize);
                    DebugUtil.Log($"网格: {mesh.name}: 当前设备的运行内存占用 (Profiler): {size}", mesh, "yellow");
                }
            }

            #endregion

            #region 贴图, 内存占用的计算必须去重, 和渲染的计算不同

            var rendererList = asset.GetComponentsInChildren<Renderer>();

            var textureGuids = new List<string>();

            foreach (var item in rendererList)
            {
                var sharedMaterials = item.sharedMaterials;
                foreach (var sharedMaterial in sharedMaterials)
                {
                    Kuroha.Util.Editor.TextureUtil.GetTexturesInMaterial(sharedMaterial, out var textures);
                    for (var i = 0; i < textures.Count; i++)
                    {
                        if (textureGuids.Contains(textures[i].guid) == false)
                        {
                            textureGuids.Add(textures[i].guid);
                            var runTimeSize = EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(textures[i].asset));
                            var storageSize = EditorUtility.FormatBytes(GetTextureStorageMemorySize(textures[i].asset));
                            DebugUtil.Log($"纹理: {textures[i].asset.name}: 当前设备的运行内存占用 (Profiler): {runTimeSize}", textures[i].asset, "yellow");
                            DebugUtil.Log($"纹理: {textures[i].asset.name}: 当前设备的硬盘空间占用 (Inspector): {storageSize}", textures[i].asset, "yellow");
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// 得到预制体中指定游戏物体的 hierarchy 层级路径
        /// </summary>
        public static string GetHierarchyPath(Transform self, bool hadTop)
        {
            var hierarchy = new List<string>
            {
                self.name
            };

            var parent = self.parent;
            while (ReferenceEquals(parent, null) == false)
            {
                hierarchy.Add(parent.name);
                parent = parent.parent;
            }

            var path = new StringBuilder();
            for (var i = hierarchy.Count - 1; i >= 0; --i)
            {
                if (hadTop == false)
                {
                    if (i == hierarchy.Count - 1)
                    {
                        continue;
                    }
                }

                path.Append(i > 0 ? $"{hierarchy[i]}/" : $"{hierarchy[i]}");
            }

            return path.ToString();
        }
    }
}
