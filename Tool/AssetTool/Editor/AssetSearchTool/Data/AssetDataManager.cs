using System.Collections.Generic;
using System.IO;
using Kuroha.Tool.Release;
using Kuroha.Util.RunTime;
using UnityEditor;

namespace Kuroha.Tool.Editor.AssetSearchTool.Data
{
    /// <summary>
    /// 继承自: 资源处理器
    /// 主要是为了实现当资源全部加载完之后触发事件
    /// </summary>
    public class AssetDirty : AssetPostprocessor
    {
        /// <summary>
        /// 记录下全部发生了改动的资源
        /// </summary>
        /// <param name="importedAssets">本次导入过程中: 新增的资源</param>
        /// <param name="deletedAssets">本次导入过程中: 删除的资源</param>
        /// <param name="movedAssets">本次导入过程中: 仅移动了位置的资源</param>
        /// <param name="movedFromAssetPaths">本次导入过程中: 仅移动了位置的 Assets 目录下的资源</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var importedAsset in importedAssets) {
                AssetDataManager.SetDirty(importedAsset);
            }

            foreach (var importedAsset in deletedAssets) {
                AssetDataManager.SetDirty(importedAsset);
            }

            foreach (var movedAsset in movedAssets) {
                AssetDataManager.SetDirty(movedAsset);
            }
        }
    }
    
    public static class AssetDataManager
    {
        /// <summary>
        /// 标志: 是否已经进行了第一次查询
        /// </summary>
        private static bool isNotFirst;

        /// <summary>
        /// 搜索条件
        /// </summary>
        private const string FILTER_STR = "t:Prefab t:Material t:ScriptableObject t:Scene t:AnimatorController t:TextAsset";
        
        /// <summary>
        /// 保存了近期全部的资源改动 (增加, 删除, 改动)
        /// 适用于新增或删除的那部分资源进行快速的引用查找
        /// </summary>
        private static readonly List<string> assetsDirty = new List<string>();
        
        /// <summary>
        /// 保存所有的 AssetData
        /// </summary>
        private static readonly Dictionary<string, AssetData> assetDataDictionary = new Dictionary<string, AssetData>();

        /// <summary>
        /// 设置脏标识
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        public static void SetDirty(string assetPath) {
            if (assetPath.StartsWith("Assets")) {
                assetsDirty.Add(assetPath);
            }
        }

        /// <summary>
        /// 创建本次查询的任务
        /// </summary>
        private static void CreateTask()
        {
            #region 根据不同的情况设置字典的值 (即设置本次查询需要读取哪些资源)

            if (isNotFirst)
            {
                if (assetDataDictionary.IsNotNullAndEmpty()) {
                    AfterSearch();
                } else {
                    FirstSearch();
                }
            }
            else {
                FirstSearch();
            }

            #endregion

            // 使用多线程启动任务, 并等待多线程执行完毕
            var tasks = assetDataDictionary.Values;
            var threadPool = new ThreadPoolTool(tasks);
            while (threadPool.IsDone == false)
            {
                var com = threadPool.completedTaskCount;
                var all = threadPool.taskCount;
                Kuroha.GUI.Editor.ProgressBar.DisplayProgressBar("引用分析工具", $"初始化资源中: {com}/{all}", com, all);
            }
        }

        /// <summary>
        /// 类的启动入口
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<AssetData> GetRawDataDictionary() {
            CreateTask();
            return assetDataDictionary.Values;
        }

        /// <summary>
        /// 第一次查询, 读取全部的资源
        /// </summary>
        private static void FirstSearch()
        {
            // 确保清空数据, 防止存在垃圾数据
            assetDataDictionary.Clear();
            
            // 查询项目中全部的资源
            var guids = AssetDatabase.FindAssets(FILTER_STR, new [] {"Assets"});
            
            // 为每一个资源创建 AssetData, 并添加到字典中
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var filePath = System.IO.Path.GetFullPath(assetPath);
                var assetData = new AssetData(assetPath, filePath);

                assetDataDictionary[assetPath] = assetData;
            }

            // 设置标志位
            isNotFirst = true;
        }

        /// <summary>
        /// 第一次之后的查询, 仅读取改动过的资源
        /// </summary>
        private static void AfterSearch()
        {
            // assetsDirty 保存了近期全部的资源改动 (增加, 删除, 改动)
            foreach (var assetPath in assetsDirty)
            {
                var filePath = System.IO.Path.GetFullPath(assetPath);
                
                if (File.Exists(filePath))
                {
                    var assetData = new AssetData(assetPath, filePath);
                    assetDataDictionary[assetPath] = assetData;
                }
                else
                {
                    if (assetDataDictionary.ContainsKey(assetPath))
                    {
                        assetDataDictionary.Remove(assetPath);
                    }
                }
            }
        }
    }
}
