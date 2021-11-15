using System.Collections.Generic;
using Script.Effect.Editor.AssetTool.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Util.Editor {
    public static class PrefabUtil {
        /// <summary>
        /// 获取图片的硬盘空间占用
        /// </summary>
        private static long GetTextureStorageMemorySize(Texture asset) {
            // 获取到 UnityEditor 程序集
            var dynamicAssembly = new DynamicAssembly(typeof(EditorWindow));

            // 获取到 TextureUtil 类
            var dynamicClass = dynamicAssembly.GetClass("UnityEditor.TextureUtil");

            // 调用 GetStorageMemorySizeLong 方法
            var result = dynamicClass.CallMethod_PublicStatic("GetStorageMemorySizeLong", asset);

            return (long)result;
        }

        /// <summary>
        /// 统计实际内存占用
        /// </summary>
        /// <param name="asset"></param>
        public static void CountMemoryOfPrefab(GameObject asset) {
            // 初始化
            var meshFilterList = asset.GetComponentsInChildren<MeshFilter>();
            DebugUtil.Log($"{meshFilterList.Length} 个 MeshFilter");
            var skinnedMeshRendererList = asset.GetComponentsInChildren<SkinnedMeshRenderer>();
            DebugUtil.Log($"{skinnedMeshRendererList.Length} 个 SkinnedMeshRenderer");
            var meshColliderList = asset.GetComponentsInChildren<MeshCollider>();
            DebugUtil.Log($"{meshColliderList.Length} 个 MeshCollider");
            var rendererList = asset.GetComponentsInChildren<Renderer>();

            #region 统计模型占用的内存, 内存占用的计算必须去重, 和渲染的计算不同

            var meshList = new List<int>();
            foreach (var item in meshFilterList) {
                var mesh = item.sharedMesh;
                var meshHashCode = mesh.GetHashCode();
                if (string.IsNullOrEmpty(mesh.name) == false && meshList.Contains(meshHashCode) == false) {
                    meshList.Add(meshHashCode);
                    var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
                    var size = EditorUtility.FormatBytes(runTimeSize);
                    DebugUtil.Log($"{mesh.name}: {size}", mesh, "green");
                }
            }

            foreach (var item in skinnedMeshRendererList) {
                var mesh = item.sharedMesh;
                var meshHashCode = mesh.GetHashCode();
                if (string.IsNullOrEmpty(mesh.name) == false && meshList.Contains(meshHashCode) == false) {
                    meshList.Add(meshHashCode);
                    var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
                    var size = EditorUtility.FormatBytes(runTimeSize);
                    DebugUtil.Log($"{mesh.name}: {size}", mesh, "green");
                }
            }

            foreach (var item in meshColliderList) {
                var mesh = item.sharedMesh;
                var meshHashCode = mesh.GetHashCode();
                if (string.IsNullOrEmpty(mesh.name) == false && meshList.Contains(meshHashCode) == false) {
                    meshList.Add(meshHashCode);
                    var runTimeSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
                    var size = EditorUtility.FormatBytes(runTimeSize);
                    DebugUtil.Log($"{mesh.name}: {size}", mesh, "green");
                }
            }

            #endregion

            #region 贴图, 内存占用的计算必须去重, 和渲染的计算不同

            var textureGuids = new List<string>();
            
            foreach (var item in rendererList) {
                var sharedMaterials = item.sharedMaterials;
                foreach (var sharedMaterial in sharedMaterials) {
                    AssetTool.Util.Editor.TextureUtil.GetTexturesInMaterial(sharedMaterial, out var textures);
                    for (var i = 0; i < textures.Count; i++) {
                        if (textureGuids.Contains(textures[i].guid) == false) {
                            textureGuids.Add(textures[i].guid);
                            var runTimeSize = EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(textures[i].asset));
                            var storageSize = EditorUtility.FormatBytes(GetTextureStorageMemorySize(textures[i].asset));
                            DebugUtil.Log($"{textures[i].asset.name}: 当前设备的运行内存占用 (Profiler): {runTimeSize}", textures[i].asset, "yellow");
                            DebugUtil.Log($"{textures[i].asset.name}: 当前设备的硬盘空间占用 (Inspector): {storageSize}", textures[i].asset, "yellow");
                        }
                    }
                }
            }

            #endregion
        }
    }
}