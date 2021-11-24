using System.Collections.Generic;
using Kuroha.GUI.Editor;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Report
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
        private const float UI_BUTTON_WIDTH = 100;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// [GUI] 分页管理器: 当前页
        /// </summary>
        private int curPage = 1;

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
        /// 问题项 GUI 风格
        /// </summary>
        private static GUIStyle checkItemGUIStyle;

        /// <summary>
        /// 开启页面
        /// </summary>
        /// <param name="results">检测结果</param>
        public static void Open(List<EffectCheckReportInfo> results)
        {
            EffectCheckReport.reportInfos = results;
            var window = GetWindow<EffectCheckReportWindow>("特效检测结果");
            window.minSize = new Vector2(1000, 650);
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
            
            var size = UnityEngine.GUI.skin.label.fontSize;
            var alignment = UnityEngine.GUI.skin.label.alignment;
            UnityEngine.GUI.skin.label.fontSize = 20;
            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            UnityEngine.GUI.skin.label.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUILayout.Label($"待修复问题: {EffectCheckReport.reportInfos.Count} 个");
            UnityEngine.GUI.skin.label.fontSize = size;
            UnityEngine.GUI.skin.label.alignment = alignment;
            
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

            UnityEngine.GUI.enabled = CanRepairCount(EffectCheckReport.reportInfos) > 0;
            if (GUILayout.Button("一键修复", GUILayout.Width(UI_BUTTON_WIDTH), GUILayout.Height(UI_BUTTON_HEIGHT)))
            {
                EffectCheckReport.AllRepair();
            }
            UnityEngine.GUI.enabled = true;

            #endregion

            GUILayout.EndHorizontal();

            #region 全部问题列表 分页显示

            PageManager.Pager(EffectCheckReport.reportInfos.Count, COUNT_PER_PAGE, ref curPage, out indexBegin, out indexEnd);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));
            {
                if (indexBegin >= 0 && indexBegin < EffectCheckReport.reportInfos.Count &&
                    indexEnd >= 0 && indexEnd < EffectCheckReport.reportInfos.Count &&
                    indexBegin <= indexEnd)
                {
                    for (var index = indexBegin; index <= indexEnd && index < EffectCheckReport.reportInfos.Count; index++)
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
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            checkItemGUIStyle = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin? Color.white : Color.black
                }
            };
        }

        /// <summary>
        /// 计算得出可以自动修复的问题的数量
        /// </summary>
        /// <returns></returns>
        private static int CanRepairCount(in List<EffectCheckReportInfo> reportInfos)
        {
            var count = 0;
            foreach (var reportInfo in reportInfos)
            {
                if (EffectCheckReport.RepairOrSelect(reportInfo.effectCheckReportType)) {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 显示一个问题项
        /// </summary>
        /// <param name="effectCheckReportInfo">待显示问题项</param>
        private static void OnGUI_ShowItemReport(EffectCheckReportInfo effectCheckReportInfo)
        {
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            // 勾选框
            effectCheckReportInfo.isEnable = EditorGUILayout.Toggle(effectCheckReportInfo.isEnable, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_HEIGHT));

            // 危险等级
            UnityEngine.GUI.skin.label.normal.textColor = effectCheckReportInfo.dangerLevel == 0 ? Color.yellow : Color.red;
            EditorGUILayout.SelectableLabel(EffectCheckItemSetView.dangerLevelOptions[effectCheckReportInfo.dangerLevel], checkItemGUIStyle, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(40));
            UnityEngine.GUI.skin.label.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            // 错误信息
            EditorGUILayout.SelectableLabel(effectCheckReportInfo.content, checkItemGUIStyle, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(1600));

            GUILayout.FlexibleSpace();

            if (ReferenceEquals(effectCheckReportInfo.asset, null) == false)
            {
                // 判断是否支持自动修复
                var isCanRepair = EffectCheckReport.RepairOrSelect(effectCheckReportInfo.effectCheckReportType);

                if (isCanRepair)
                {
                    if (GUILayout.Button("修复", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2)))
                    {
                        EffectCheckReport.Repair(effectCheckReportInfo);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                if (GUILayout.Button("选中", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2)))
                {
                    EffectCheckReport.Ping(effectCheckReportInfo);
                }
            }
        }
    }
}
