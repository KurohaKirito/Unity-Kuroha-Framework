using System.Collections.Generic;
using System.Linq;
using Kuroha.Tool.Editor.AssetSearchTool.Data;
using Kuroha.Tool.Editor.AssetSearchTool.GUI;
using Kuroha.Tool.Release;
using Kuroha.Util.Release;
using UnityEditor;

namespace Kuroha.Tool.Editor.AssetSearchTool.Searcher
{
    /// <summary>
    /// 引用匹配器
    /// </summary>
    public static class ReferenceSearcher
    {
        /// <summary>
        /// 存储查询结果
        /// </summary>
        public static readonly Dictionary<string, List<string>> references = new Dictionary<string, List<string>>();

        /// <summary>
        /// 查找指定 guid 物体的引用
        /// </summary>
        /// <param name="guids"></param>
        public static void Find(string[] guids)
        {
            if (guids.IsNotNullAndEmpty())
            {
                // 清空旧的查询结果
                references.Clear();
                
                // 初始化用来保存查询结果的数据
                foreach (var guid in guids)
                {
                    references[guid] = new List<string>();
                }

                // 为了遍历整个项目的资源, 为每个资源都建立查询任务
                var tasks = new List<MatchTask>(20000);
                tasks.AddRange(AssetDataManager.GetRawDataDictionary().Select(task => new MatchTask(guids, task)));

                // 使用多线程启动任务, 并等待多线程执行完毕
                var threadPool = new ThreadPoolTool(tasks);
                while (threadPool.IsDone == false)
                {
                    var com = threadPool.completedTaskCount;
                    var all = threadPool.taskCount;
                    Kuroha.GUI.Editor.ProgressBar.DisplayProgressBar("正在分析", $"{com}/{all}", com, all);
                }
                EditorUtility.ClearProgressBar();

                #region 遍历每一个任务中的查询结果, 并汇总到字典中

                foreach (var task in tasks)
                {
                    foreach (var result in task.results)
                    {
                        if (references.ContainsKey(result.Key) == false)
                        {
                            references[result.Key] = new List<string>();
                        }

                        references[result.Key].Add(result.Value);
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// 查找当前选中物体的引用
        /// </summary>
        public static void OpenWindow()
        {
            if (Selection.assetGUIDs.IsNotNullAndEmpty())
            {
                AssetSearchWindow.Open(1);
                Find(Selection.assetGUIDs);
            }
        }
    }
}