using UnityEditor;
using UnityEngine;

namespace Kuroha.Framework.GUI.Editor
{
    /// <summary>
    /// 分页管理器
    /// </summary>
    public static class PageManager
    {
        /// <summary>
        /// 留白
        /// </summary>
        private const int UI_SPACE = 10;

        /// <summary>
        /// 页数跳转
        /// </summary>
        private static int gotoPage = 1;

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
            #region 分页数据计算

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

            #endregion
            
            GUILayout.BeginHorizontal();
            
            Top(ref currentPage);
            Previous(ref currentPage);
            Next(ref currentPage, pageCount);
            Last(ref currentPage, pageCount);
            
            GUILayout.Space(UI_SPACE);
            
            Info(currentPage, pageCount);
            Goto(ref currentPage, pageCount);
            
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="currentPage"></param>
        private static void Top(ref int currentPage)
        {
            UnityEngine.GUI.enabled = currentPage > 1;
            if (GUILayout.Button("首页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage = 1;
            }
            UnityEngine.GUI.enabled = true;
        }
        
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="currentPage"></param>
        private static void Previous(ref int currentPage)
        {
            UnityEngine.GUI.enabled = currentPage > 1;
            if (GUILayout.Button("上一页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                if (currentPage > 1)
                {
                    currentPage--;
                }
            }
            UnityEngine.GUI.enabled = true;
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageCount"></param>
        private static void Next(ref int currentPage, int pageCount)
        {
            UnityEngine.GUI.enabled = currentPage < pageCount;
            if (GUILayout.Button("下一页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage++;
                if (currentPage > pageCount)
                {
                    currentPage = pageCount;
                }
            }
            UnityEngine.GUI.enabled = true;
        }
        
        /// <summary>
        /// 末页
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageCount"></param>
        private static void Last(ref int currentPage, int pageCount)
        {
            UnityEngine.GUI.enabled = currentPage < pageCount;
            if (GUILayout.Button("末页", GUILayout.Width(100), GUILayout.Height(24)))
            {
                currentPage = pageCount;
            }
            UnityEngine.GUI.enabled = true;
        }
        
        /// <summary>
        /// 页数信息
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageCount"></param>
        private static void Info(int currentPage, int pageCount)
        {
            if (pageCount > 0)
            {
                EditorGUILayout.LabelField($"第 {currentPage} 页 / 共 {pageCount} 页", GUILayout.Width(110),GUILayout.Height(24));
            }
            else
            {
                EditorGUILayout.LabelField("无数据", GUILayout.Width(50),GUILayout.Height(24));
            }
        }

        /// <summary>
        /// 跳转
        /// </summary>
        private static void Goto(ref int currentPage, int pageCount)
        {
            UnityEngine.GUI.enabled = pageCount > 0 && gotoPage >= 1 && gotoPage <= pageCount;
            
            if (GUILayout.Button("跳转", GUILayout.Width(100), GUILayout.Height(24)))
            {
                if (gotoPage >= 1 && gotoPage <= pageCount)
                {
                    currentPage = gotoPage;
                }
            }

            UnityEngine.GUI.enabled = true;
            
            var oldAlignment = UnityEngine.GUI.skin.textField.alignment;
            var fontSize = UnityEngine.GUI.skin.textField.fontSize;
            
            UnityEngine.GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
            UnityEngine.GUI.skin.textField.fontSize = 14;
            gotoPage = EditorGUILayout.IntField(gotoPage, GUILayout.Width(60), GUILayout.Height(24));
            UnityEngine.GUI.skin.textField.alignment = oldAlignment;
            UnityEngine.GUI.skin.textField.fontSize = fontSize;
        }
    }
}