using System.Collections.Generic;
using System.Linq;
using Kuroha.Tool.Editor.AssetSearchTool.Data;
using Kuroha.Tool.Release;
using Kuroha.Util.RunTime;

namespace Kuroha.Tool.Editor.AssetSearchTool.Searcher
{
    /// <summary>
    /// 字符串匹配器
    /// </summary>
    public static class StringSearcher
    {
        /// <summary>
        /// 存储查询结果
        /// </summary>
        public static readonly Dictionary<string, List<string>> references = new Dictionary<string, List<string>>();

        /// <summary>
        /// 字符串引用查询
        /// </summary>
        /// <param name="keyWords">需要查询的字符串数组</param>
        public static void FindString(string[] keyWords)
        {
            if (keyWords.IsNotNullAndEmpty())
            {
                #region 清空旧的查询结果

                references.Clear();
                foreach (var guid in keyWords)
                {
                    references[guid] = new List<string>();
                }

                #endregion

                // 为了遍历整个项目的资源, 为每个资源都建立查询任务
                var tasks = new List<MatchTask>(20000);
                tasks.AddRange(AssetDataManager.GetRawDataDictionary().Select(task => new MatchTask(keyWords, task)));

                // 使用多线程启动任务, 并等待多线程执行完毕
                var threadPool = new ThreadPoolTool(tasks);
                while (threadPool.IsDone == false)
                {
                    var com = threadPool.completedTaskCount;
                    var all = threadPool.taskCount;
                    Kuroha.GUI.Editor.ProgressBar.DisplayProgressBar("字符串引用分析工具", $"引用分析中: {com}/{all}", com, all);
                }

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
    }
}