using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.AssetSearchTool.Searcher
{
    /// <summary>
    /// 资源依赖浏览器
    /// </summary>
    public static class DependenceSearcher
    {
        /// <summary>
        /// 保存物体的 guid 和其依赖的物体的路径
        /// </summary>
        public static readonly Dictionary<string, List<string>> dependencies = new Dictionary<string, List<string>>();

        /// <summary>
        /// 公共依赖分析结果
        /// </summary>
        public static readonly Dictionary<UnityEngine.Object, List<UnityEngine.Object>> publicDependencies = new Dictionary<UnityEngine.Object, List<UnityEngine.Object>>();

        /// <summary>
        /// 寻找当前选中物体的依赖
        /// </summary>
        /// <param name="assetGUIDs">Selection.assetGUIDs</param>
        public static void FindSelectionDependencies(string[] assetGUIDs)
        {
            if (assetGUIDs != null)
            {
                dependencies.Clear();

                foreach (var guid in assetGUIDs)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    // 直接给 指定 Key 赋值, 达到添加键值对的目的
                    dependencies[guid] = new List<string>(AssetDatabase.GetDependencies(path));
                }
            }
        }

        /// <summary>
        /// 分析公共依赖
        /// </summary>
        public static void FindPublicDependencies()
        {
            if (dependencies.Keys.Count > 0)
            {
                publicDependencies.Clear();
                
                foreach (var dKey in dependencies.Keys)
                {
                    var path = AssetDatabase.GUIDToAssetPath(dKey);
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                
                    foreach (var dValue in dependencies[dKey])
                    {
                        var assetD = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dValue);
                        if (assetD != null)
                        {
                            if (publicDependencies.ContainsKey(assetD) == false)
                            {
                                publicDependencies.Add(assetD, new List<Object> { asset });
                            }
                            else
                            {
                                publicDependencies[assetD].Add(asset);
                            }
                        }
                        else
                        {
                            DebugUtil.LogError($"加载不到资源 '{dValue}', 请检查资源是否存在 !");
                        }
                    }
                }
            }
        }
    }
}
