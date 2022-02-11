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
            var window = GetWindow<EffectCheckReportWindow>("资源检查结果");
            window.minSize = new Vector2(1200, 685);
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            checkItemGUIStyle = new GUIStyle
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin? Color.white : Color.black
                }
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
            
            var size = UnityEngine.GUI.skin.label.fontSize;
            var alignment = UnityEngine.GUI.skin.label.alignment;
            UnityEngine.GUI.skin.label.fontSize = 24;
            UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            UnityEngine.GUI.skin.label.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUILayout.Label($"Check Result Count : {EffectCheckReport.reportInfos.Count}");
            UnityEngine.GUI.skin.label.fontSize = size;
            UnityEngine.GUI.skin.label.alignment = alignment;
            
            GUILayout.BeginHorizontal();

            #region 全选 与 全不选

            var selectAllStr = isSelectAll ? "全不选" : "全选";
            UnityEngine.GUI.enabled = EffectCheckReport.reportInfos.Count > 0;
            if (GUILayout.Button(selectAllStr, GUILayout.Width(UI_BUTTON_WIDTH), GUILayout.Height(UI_BUTTON_HEIGHT)))
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
            // 留白
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            // 勾选框
            effectCheckReportInfo.isEnable = EditorGUILayout.Toggle(effectCheckReportInfo.isEnable, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_HEIGHT));

            // 定位按钮
            var isSelectable = ReferenceEquals(effectCheckReportInfo.asset, null) == false;
            UnityEngine.GUI.enabled = isSelectable;
            if (GUILayout.Button("定位", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2))) {
                EffectCheckReport.Ping(effectCheckReportInfo);
            }
            UnityEngine.GUI.enabled = true;
            
            // 修复按钮
            var isRepairable = EffectCheckReport.RepairOrSelect(effectCheckReportInfo.effectCheckReportType);
            UnityEngine.GUI.enabled = isRepairable;
            if (GUILayout.Button("修复", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2))) {
                EffectCheckReport.Repair(effectCheckReportInfo);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            UnityEngine.GUI.enabled = true;
            
            // 留白
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
            
            // 危险等级
            checkItemGUIStyle.normal.textColor = effectCheckReportInfo.dangerLevel == 0 ? Color.yellow : Color.red;
            EditorGUILayout.SelectableLabel(EffectCheckItemSetView.dangerLevelOptions[effectCheckReportInfo.dangerLevel], checkItemGUIStyle, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(40));
            checkItemGUIStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            // 错误信息
            EditorGUILayout.SelectableLabel(effectCheckReportInfo.content, checkItemGUIStyle, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(1600));

            // 填满留白
            GUILayout.FlexibleSpace();
        }
    }
}
