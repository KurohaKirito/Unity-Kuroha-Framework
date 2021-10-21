using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEditor;

namespace Kuroha.Tool.Editor.AssetSearchTool.Searcher
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
        /// 寻找当前选中物体的依赖
        /// </summary>
        /// <param name="assetGUIDs">Selection.assetGUIDs</param>
        public static void FindSelectionDependencies(string[] assetGUIDs)
        {
            if (assetGUIDs.IsNotNullAndEmpty())
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
    }
}