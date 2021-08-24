using UnityEngine;

namespace Kuroha.GUI.Editor
{
    /// <summary>
    /// 分页管理器
    /// </summary>
    public static class PageManager
    {
        private const int UI_SPACE = 10;

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="dataCount">数据总数量</param>
        /// <param name="countPerPage">每页多少行数据</param>
        /// <param name="currentPage">当前为第几页</param>
        /// <param name="beginIndex">当前页开始下标</param>
        /// <param name="endIndex">当前页结束下标</param>
        public static void Pager(int dataCount, int countPerPage, ref int currentPage, out int beginIndex, out int endIndex)
        {
            if (dataCount <= 0 || countPerPage <= 0 || currentPage <= 0)
            {
                beginIndex = 0;
                endIndex = 0;
                return;
            }

            var pageCount = dataCount / countPerPage;
            if (dataCount % countPerPage != 0)
            {
                pageCount++;
            }

            if (currentPage > pageCount)
            {
                currentPage = pageCount;
            }

            beginIndex = (currentPage - 1) * countPerPage;

            if (currentPage < pageCount)
            {
                endIndex = beginIndex + countPerPage - 1;
            }
            else
            {
                var remainder = dataCount % countPerPage;

                endIndex = remainder == 0
                    ? beginIndex + countPerPage - 1
                    : beginIndex + remainder - 1;
            }

            GUILayout.BeginHorizontal();
            UnityEngine.GUI.enabled = currentPage > 1;
            if (GUILayout.Button("首页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage = 1;
            }

            if (GUILayout.Button("上一页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage--;
                if (currentPage < 1)
                {
                    currentPage = 1;
                }
            }

            UnityEngine.GUI.enabled = currentPage < pageCount;
            if (GUILayout.Button("下一页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage++;
                if (currentPage > pageCount)
                {
                    currentPage = pageCount;
                }
            }

            if (GUILayout.Button("末页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage = pageCount;
            }

            GUILayout.Space(UI_SPACE);
            GUILayout.Label(pageCount > 0 ? $"第 {currentPage} 页 / 共 {pageCount} 页" : "无数据", GUILayout.Height(24));

            UnityEngine.GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}