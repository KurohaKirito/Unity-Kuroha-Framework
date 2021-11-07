using System.Collections.Generic;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView
{
    public class EffectCheckItemViewWindow : EditorWindow
    {
        /// <summary>
        /// 刷新标志位
        /// </summary>
        public static bool isRefresh = true;

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
        /// GUID 检查项序号风格
        /// </summary>
        private static GUIStyle itemIdGUIStyle;

        /// <summary>
        /// 检查项 GUI 风格
        /// </summary>
        private static GUIStyle checkItemGUIStyle;
        
        /// <summary>
        /// 标题风格
        /// </summary>
        private static GUIStyle titleStyle;

        /// <summary>
        /// 滑动条位置
        /// </summary>
        private static Vector2 vector2ScrollView;
        
        /// <summary>
        /// 打开窗口
        /// </summary>
        public static void Open()
        {
            var window = GetWindow<EffectCheckItemViewWindow>("检查项列表");
            window.minSize = new Vector2Int(800, 600);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            isRefresh = true;
            
            itemIdGUIStyle = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleRight,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };

            checkItemGUIStyle = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };
            
            titleStyle = new GUIStyle
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            if (isRefresh)
            {
                Refresh();
            }

            GUILayout.Label("检查项列表", titleStyle);
            GUILayout.Space(UI_DEFAULT_MARGIN);

            #region 列表

            GUILayout.BeginVertical("Box");

            OnGUI_ShowTitle();

            vector2ScrollView = GUILayout.BeginScrollView(vector2ScrollView);
            foreach (var checkItem in EffectCheckItemView.CheckItemInfoList)
            {
                OnGUI_ShowItem(checkItem);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            #endregion

            OnGUI_Buttons(EffectCheckItemView.CheckItemInfoList);
        }

        /// <summary>
        /// 显示列表标题
        /// </summary>
        private static void OnGUI_ShowTitle()
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("序号", itemIdGUIStyle);
            GUILayout.Space(24);
            GUILayout.Label("CICD", checkItemGUIStyle);
            GUILayout.Space(24);
            GUILayout.Label("Effect", checkItemGUIStyle);
            GUILayout.Space(140);
            GUILayout.Label("标题", checkItemGUIStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示一项检查项
        /// </summary>
        /// <param name="info">检查项</param>
        private static void OnGUI_ShowItem(CheckItemInfo info)
        {
            GUILayout.BeginHorizontal("Box");

            // GUID
            GUILayout.Label(info.guid, itemIdGUIStyle, GUILayout.Width(36), GUILayout.Height(UI_BUTTON_HEIGHT));
            GUILayout.Space(15);

            #region Auto Check Icon

            var autoCheckIcon = EditorGUIUtility.IconContent(info.cicdEnable
                ? "sv_icon_dot11_pix16_gizmo"
                : "sv_icon_dot8_pix16_gizmo");
            if (GUILayout.Button(autoCheckIcon, GUILayout.Width(32), GUILayout.Height(32)))
            {
                info.cicdEnable = !info.cicdEnable;
            }

            #endregion

            GUILayout.Space(30);

            #region Effect Check Icon

            var effectCheckIcon =
                EditorGUIUtility.IconContent(info.effectEnable
                    ? "sv_icon_dot3_pix16_gizmo"
                    : "sv_icon_dot0_pix16_gizmo");
            if (GUILayout.Button(effectCheckIcon, GUILayout.Width(32), GUILayout.Height(32)))
            {
                info.effectEnable = !info.effectEnable;
            }
            
            #endregion
            
            GUILayout.Space(30);
            
            // 标题
            EditorGUILayout.LabelField($"{info.title}", checkItemGUIStyle, GUILayout.Height(UI_BUTTON_HEIGHT));
            GUILayout.FlexibleSpace();

            // 编辑按钮
            if (GUILayout.Button("Edit", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2)))
            {
                EffectCheckItemSetViewWindow.Open(info);
            }

            // 删除按钮
            if (GUILayout.Button("Delete", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2)))
            {
                if (EditorUtility.DisplayDialog("标题", $"是否删除检查项:\n\n{info.title}", "确认", "取消"))
                {
                    EffectCheckItemView.Remove(info);
                }
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// GUI 保存
        /// </summary>
        private void OnGUI_Buttons(List<CheckItemInfo> infos)
        {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            GUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();

            #region 全选 (CI)

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("全选 (CI)", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
            {
                foreach (var info in infos)
                {
                    info.cicdEnable = true;
                }
            }
            GUILayout.EndVertical();

            #endregion
            
            GUILayout.FlexibleSpace();

            #region 全不选 (CI)

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("全不选 (CI)", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
            {
                foreach (var info in infos)
                {
                    info.cicdEnable = false;
                }
            }
            GUILayout.EndVertical();

            #endregion
            
            GUILayout.FlexibleSpace();

            #region 保存

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("保存", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
            {
                EffectCheckItemSetView.SaveConfig(EffectCheckItemView.CheckItemInfoList, "保存成功!");
                Close();
            }
            GUILayout.EndVertical();

            #endregion
            
            GUILayout.FlexibleSpace();
            
            #region 全选 (特效)

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("全选 (特效)", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
            {
                foreach (var info in infos)
                {
                    info.effectEnable = true;
                }
            }
            GUILayout.EndVertical();

            #endregion
            
            GUILayout.FlexibleSpace();

            #region 全不选 (特效)

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("全不选 (特效)", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
            {
                foreach (var info in infos)
                {
                    info.effectEnable = false;
                }
            }
            GUILayout.EndVertical();

            #endregion
            
            GUILayout.EndHorizontal();

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        private static void Refresh()
        {
            if (EffectCheckItemView.CheckItemInfoList != null)
            {
                EffectCheckItemView.CheckItemInfoList.Clear();
                EffectCheckItemView.CheckItemInfoList = null;
            }

            EffectCheckItemView.CheckItemInfoList = EffectCheckItemSetView.LoadConfig();
            isRefresh = false;
        }
    }
}