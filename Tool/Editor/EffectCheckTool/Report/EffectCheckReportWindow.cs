using System;
using System.Collections.Generic;
using Kuroha.Tool.Editor.EffectCheckTool.ItemSetView;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.EffectCheckTool.Report
{
    public class EffectCheckReportWindow : EditorWindow
    {
        /// <summary>
        /// [GUI] 全选标志位
        /// </summary>
        private bool isSelectAll = true;

        /// <summary>
        /// [GUI] 滑动条
        /// </summary>
        private Vector2 scrollPos;

        /// <summary>
        /// [GUI] 每页显示的结果数量
        /// </summary>
        private const int COUNT_PER_PAGE = 20;

        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// [GUI] 分页管理器: 当前页索引
        /// </summary>
        private int curPageIndex;

        /// <summary>
        /// [GUI] 分页管理器: 当前页中数据的开始索引
        /// </summary>
        private int indexBegin;

        /// <summary>
        /// [GUI] 分页管理器: 当前页中数据的结束索引
        /// </summary>
        private int indexEnd;

        /// <summary>
        /// [GUI] 分页管理器: 总页数
        /// </summary>
        private int pageCount;
        
        /// <summary>
        /// 标题风格
        /// </summary>
        private GUIStyle titleStyle;

        /// <summary>
        /// 开启页面
        /// </summary>
        /// <param name="results">检测结果</param>
        public static void Open(List<EffectCheckReportInfo> results)
        {
            EffectCheckReport.reportInfos = results;
            GetWindow<EffectCheckReportWindow>("检测结果").minSize = new Vector2(1000, 650);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            titleStyle = new GUIStyle
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
        }

        /// <summary>
        /// 绘制页面
        /// </summary>
        private void OnGUI()
        {
            if (EffectCheckReport.reportInfos == null)
            {
                return;
            }

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            GUILayout.Label($"待修复问题: {EffectCheckReport.reportInfos.Count} 个", titleStyle);
            GUILayout.BeginHorizontal();

            #region 全选 与 全不选

            var selectAllStr = isSelectAll ? "全不选" : "全选";
            UnityEngine.GUI.enabled = EffectCheckReport.reportInfos.Count > 0;
            if (GUILayout.Button(selectAllStr, GUILayout.Width(UI_BUTTON_WIDTH),
                GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                isSelectAll = !isSelectAll;
                foreach (var reportInfo in EffectCheckReport.reportInfos)
                {
                    reportInfo.isEnable = isSelectAll;
                }
            }

            UnityEngine.GUI.enabled = true;

            #endregion

            #region 一键修复

            if (GUILayout.Button("一键修复", GUILayout.Width(UI_BUTTON_WIDTH),
                GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                EffectCheckReport.AllRepair();
            }

            #endregion

            GUILayout.EndHorizontal();

            #region 全部问题列表 分页显示

            OnGUI_PageManager();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
            {
                if (indexBegin >= 0 && indexBegin <= EffectCheckReport.reportInfos.Count &&
                    indexEnd >= 0 && indexEnd <= EffectCheckReport.reportInfos.Count &&
                    indexBegin <= indexEnd)
                {
                    for (var index = indexBegin; index <= indexEnd; index++)
                    {
                        GUILayout.BeginHorizontal("Box");
                        OnGUI_ShowItemReport(EffectCheckReport.reportInfos[index]);
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndScrollView();

            #endregion
        }

        /// <summary>
        /// 显示一个问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">待显示问题项</param>
        private static void OnGUI_ShowItemReport(EffectCheckReportInfo effectCheckReportInfo)
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            effectCheckReportInfo.isEnable = EditorGUILayout.Toggle(effectCheckReportInfo.isEnable, GUILayout.Width(20));

            UnityEngine.GUI.skin.label.normal.textColor = effectCheckReportInfo.dangerLevel == 0 ? Color.yellow : Color.red;
            GUILayout.Label(EffectCheckItemSetView.dangerLevelOptions[effectCheckReportInfo.dangerLevel], GUILayout.Width(40));
            UnityEngine.GUI.skin.label.normal.textColor = Color.black;

            GUILayout.Label(effectCheckReportInfo.content);

            GUILayout.FlexibleSpace();

            if (ReferenceEquals(effectCheckReportInfo.asset, null) == false)
            {
                // 判断是否支持自动修复
                var isCanRepair = EffectCheckReport.RepairOrSelect(effectCheckReportInfo.effectCheckReportType);

                if (isCanRepair)
                {
                    if (GUILayout.Button("修复", GUILayout.Width(UI_BUTTON_WIDTH / 2)))
                    {
                        EffectCheckReport.Repair(effectCheckReportInfo);
                    }
                }

                if (GUILayout.Button("选中", GUILayout.Width(UI_BUTTON_WIDTH / 2)))
                {
                    EffectCheckReport.Ping(effectCheckReportInfo);
                }
            }
        }

        /// <summary>
        /// 分页管理器
        /// </summary>
        private void OnGUI_PageManager()
        {
            var count = EffectCheckReport.reportInfos.Count;

            pageCount = count / COUNT_PER_PAGE;
            if (count % COUNT_PER_PAGE != 0)
            {
                pageCount++;
            }

            if (curPageIndex < 0)
            {
                curPageIndex = 0;
            }

            if (curPageIndex > pageCount - 1)
            {
                curPageIndex = pageCount - 1;
            }

            indexBegin = curPageIndex * COUNT_PER_PAGE;

            if (curPageIndex + 1 < pageCount)
            {
                indexEnd = indexBegin + COUNT_PER_PAGE - 1;
            }
            else
            {
                var remainder = count % COUNT_PER_PAGE;
                indexEnd = remainder == 0
                    ? indexBegin + COUNT_PER_PAGE - 1
                    : indexBegin + remainder - 1;
            }

            GUILayout.BeginHorizontal();
            UnityEngine.GUI.enabled = curPageIndex > 0;
            if (GUILayout.Button("首页", GUILayout.Width(UI_BUTTON_WIDTH), GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                curPageIndex = 0;
            }

            if (GUILayout.Button("上一页", GUILayout.Width(UI_BUTTON_WIDTH), GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                curPageIndex--;
                if (curPageIndex < 0)
                {
                    curPageIndex = 0;
                }
            }

            UnityEngine.GUI.enabled = curPageIndex < pageCount - 1;
            if (GUILayout.Button("下一页", GUILayout.Width(UI_BUTTON_WIDTH),
                GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                curPageIndex++;
                if (curPageIndex > pageCount - 1)
                {
                    curPageIndex = pageCount - 1;
                }
            }

            if (GUILayout.Button("末页", GUILayout.Width(UI_BUTTON_WIDTH),
                GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                curPageIndex = pageCount - 1;
            }

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            GUILayout.Label(pageCount > 0 ? $"第 {curPageIndex + 1} 页 / 共 {pageCount} 页" : "无数据",
                GUILayout.Height(UI_BUTTON_HEIGHT));

            UnityEngine.GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}