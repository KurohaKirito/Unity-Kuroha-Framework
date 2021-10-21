using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kuroha.Tool.Release;

namespace Kuroha.Tool.Editor.AssetSearchTool.Data
{
    public class MatchTask : ThreadPoolTool.ITask
    {
        /// <summary>
        /// 需要匹配的关键字
        /// </summary>
        private readonly string[] keyWords;

        /// <summary>
        /// 资源数据
        /// </summary>
        private readonly AssetData assetData;

        /// <summary>
        /// 存储当前查询任务的匹配结果
        /// </summary>
        public readonly Dictionary<string, string> results = new Dictionary<string, string>();

        /// <summary>
        /// 任务标志: 是否完成
        /// </summary>
        private bool done;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="keyWordArray"></param>
        /// <param name="data"></param>
        public MatchTask(string[] keyWordArray, AssetData data) {
            keyWords = keyWordArray;
            assetData = data;
        }

        /// <summary>
        /// 任务是否结束
        /// </summary>
        /// <returns></returns>
        public bool IsDone() {
            return done;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            foreach (var str in keyWords)
            {
                // 将 搜索字符串 和 文件内容 匹配
                if (Regex.IsMatch(assetData.fileContentText, str))
                {
                    results.Add(str, assetData.relativePath);
                }
            }

            done = true;
        }
    }
}