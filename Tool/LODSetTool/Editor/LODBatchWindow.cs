using System;
using System.Collections.Generic;
using System.IO;
using Kuroha.GUI.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.LODSetTool.Editor
{
    /// <summary>
    /// 预设数据结构
    /// </summary>
    [Serializable]
    public class LODBatchSet
    {
        public string name;
        public int lodCount;
        public List<float> lodValues;
    }

    public class LODBatchWindow : EditorWindow
    {
        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局输入框的宽度
        /// </summary>
        private const float UI_INPUT_AREA_WIDTH = 400;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 滑动条
        /// </summary>
        private Vector2 scroll;

        /// <summary>
        /// 所有的 LOD Group
        /// </summary>
        private static List<LODGroup> lodGroups;

        /// <summary>
        /// LOD 默认级别数量
        /// </summary>
        private static int lodCount = 3;

        /// <summary>
        /// LOD 的级别百分比设置
        /// </summary>
        private static readonly List<float> lodValues = new List<float>();

        /// <summary>
        /// 另存为模式
        /// </summary>
        private bool isSettingEditMode;

        /// <summary>
        /// 配置名称
        /// </summary>
        private string configName;

        /// <summary>
        /// 配置列表
        /// </summary>
        private static List<LODBatchSet> configList;

        /// <summary>
        /// 当前正在编辑的设置
        /// </summary>
        private int currentEdit;

        public static void Open()
        {
            RefreshSelection();
            var window = GetWindow<LODBatchWindow>("LodGroup 百分比设置");
            window.minSize = new Vector2(440, 400);
            window.maxSize = new Vector2(440, 3000);

            var path = Path.GetFullPath("Assets/Art/Effects/LODToolConfig.json");
            var jsonText = File.ReadAllText(path);
            configList = JsonUtility.FromJson<JsonSerialization<LODBatchSet>>(jsonText).ToList();

            if (configList.Count > 0)
            {
                ReadConfigToWindow(configList[0]);
            }
        }

        private void OnSelectionChange()
        {
            RefreshSelection();
            Repaint();
        }

        public void OnGUI()
        {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            #region 绘制预设按钮

            for (var index = 0; index < configList.Count; ++index)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(UI_DEFAULT_MARGIN * 2);
                    if (GUILayout.Button(configList[index].name, GUILayout.Width(UI_BUTTON_WIDTH * 0.78f)))
                    {
                        ReadConfigToWindow(configList[index]);
                    }

                    GUILayout.Space(UI_DEFAULT_MARGIN * 2);

                    if (++index >= configList.Count)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        break;
                    }

                    if (GUILayout.Button(configList[index].name, GUILayout.Width(UI_BUTTON_WIDTH * 0.78f)))
                    {
                        ReadConfigToWindow(configList[index]);
                    }

                    GUILayout.Space(UI_DEFAULT_MARGIN * 2);

                    if (++index >= configList.Count)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        break;
                    }

                    if (GUILayout.Button(configList[index].name, GUILayout.Width(UI_BUTTON_WIDTH * 0.78f)))
                    {
                        ReadConfigToWindow(configList[index]);
                    }

                    GUILayout.Space(UI_DEFAULT_MARGIN * 2);

                    if (++index >= configList.Count)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        break;
                    }

                    if (GUILayout.Button(configList[index].name, GUILayout.Width(UI_BUTTON_WIDTH * 0.78f)))
                    {
                        ReadConfigToWindow(configList[index]);
                    }

                    GUILayout.Space(UI_DEFAULT_MARGIN * 2);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(UI_DEFAULT_MARGIN);
            }

            #endregion

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            #region 绘制界面

            lodCount = EditorGUILayout.IntField("LOD Count: (1 - 10)", lodCount, GUILayout.Width(UI_INPUT_AREA_WIDTH));
            if (lodCount < 1)
            {
                lodCount = 1;
            }

            if (lodCount > 10)
            {
                lodCount = 10;
            }

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            while (lodCount > lodValues.Count)
            {
                lodValues.Add(0);
            }

            while (lodCount < lodValues.Count)
            {
                lodValues.RemoveAt(0);
            }

            for (var index = 0; index < lodValues.Count; index++)
            {
                if (index == lodValues.Count - 1)
                {
                    lodValues[index] = EditorGUILayout.FloatField("Culled:", lodValues[index], GUILayout.Width(UI_INPUT_AREA_WIDTH));
                }
                else
                {
                    lodValues[index] = EditorGUILayout.FloatField($"LOD {index + 1}:", lodValues[index], GUILayout.Width(UI_INPUT_AREA_WIDTH));
                }

                GUILayout.Space(UI_DEFAULT_MARGIN * 2);
            }

            #endregion

            #region 绘制按钮

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("编辑预设", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                {
                    currentEdit = -1;
                    isSettingEditMode = !isSettingEditMode;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("批量设置", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                {
                    BatchLOD();
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("刷新选择", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                {
                    RefreshSelection();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            #region 预设编辑模式

            if (isSettingEditMode)
            {
                for (var index = 0; index < configList.Count; index++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(UI_DEFAULT_MARGIN * 2);
                        GUILayout.Label(configList[index].name);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("编辑", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2.5f)))
                        {
                            currentEdit = index;
                            ReadConfigToWindow(configList[index]);
                        }

                        GUILayout.Space(UI_DEFAULT_MARGIN * 2);
                        if (GUILayout.Button("删除", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2.5f)))
                        {
                            configList.RemoveAt(index);
                        }

                        GUILayout.Space(UI_DEFAULT_MARGIN * 2);

                        if (currentEdit == index)
                        {
                            if (GUILayout.Button("保存", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2.5f)))
                            {
                                SetWindowToConfig(index);
                                SaveAsConfig();
                                currentEdit = -1;
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("覆盖", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2.5f)))
                            {
                                SetWindowToConfig(index);
                                SaveAsConfig();
                            }
                        }

                        GUILayout.Space(UI_DEFAULT_MARGIN * 2);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(UI_DEFAULT_MARGIN * 2);
                }

                configName = EditorGUILayout.TextField("预设名称", configName);

                GUILayout.Space(UI_DEFAULT_MARGIN * 2);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("新增", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        if (string.IsNullOrEmpty(configName))
                        {
                            Dialog.Display("消息", "填写预设名称", Dialog.DialogType.Message, "OK");
                        }
                        else
                        {
                            configList.Add(new LODBatchSet
                            {
                                name = configName,
                                lodCount = 1,
                                lodValues = new List<float> {0, 0}
                            });
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("保存", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        currentEdit = -1;
                        isSettingEditMode = !isSettingEditMode;
                        SaveAsConfig();
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }

            #endregion

            GUILayout.Space(UI_DEFAULT_MARGIN * 2);

            #region 绘制场景物体

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label($"当前选中了 {lodGroups.Count} 个 LOD Group");
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            scroll = GUILayout.BeginScrollView(scroll);
            for (var index = 0; index < lodGroups.Count; ++index)
            {
                UnityEditor.EditorGUILayout.ObjectField($"{index + 1}", lodGroups[index].gameObject, typeof(GameObject), true);
            }

            GUILayout.EndScrollView();

            #endregion
        }

        private static void BatchLOD()
        {
            RefreshSelection();

            foreach (var lodGroup in lodGroups)
            {
                var originLods = lodGroup.GetLODs();

                #region LOD 级别数相等

                if (originLods.Length == lodValues.Count)
                {
                    for (var i = 0; i < originLods.Length; i++)
                    {
                        originLods[i].screenRelativeTransitionHeight = lodValues[i] / 100.0f;
                    }

                    lodGroup.SetLODs(originLods);
                }

                #endregion

                #region 新的级别数较多

                else if (originLods.Length < lodValues.Count)
                {
                    for (var i = 0; i < originLods.Length; i++)
                    {
                        if (i < originLods.Length - 1)
                        {
                            originLods[i].screenRelativeTransitionHeight = lodValues[i] / 100.0f;
                        }
                        else if (i == originLods.Length - 1)
                        {
                            originLods[i].screenRelativeTransitionHeight = lodValues[lodValues.Count - 1] / 100.0f;
                        }
                    }

                    lodGroup.SetLODs(originLods);
                }

                #endregion

                #region 新的级别数较少

                else if (originLods.Length > lodValues.Count)
                {
                    var obj = lodGroup.gameObject;
                    DebugUtil.LogError($"错误: 游戏物体 {obj.name} 的 LOD 级别数 ({originLods.Length}) 比当前需要设置的级别数 ({lodValues.Count}) 多, 已跳过此物体.", obj);
                }

                #endregion
            }

            Debug.Log("批量设置成功!");
        }

        private static void ReadConfigToWindow(LODBatchSet config)
        {
            lodCount = config.lodCount;

            while (lodCount > lodValues.Count)
            {
                lodValues.Add(0);
            }

            while (lodCount < lodValues.Count)
            {
                lodValues.RemoveAt(0);
            }

            for (var index = 0; index < lodCount; index++)
            {
                lodValues[index] = config.lodValues[index];
            }
        }

        private static void SetWindowToConfig(int i)
        {
            configList[i].lodCount = lodCount;

            while (configList[i].lodValues.Count > lodCount)
            {
                configList[i].lodValues.RemoveAt(0);
            }

            while (configList[i].lodValues.Count < lodCount)
            {
                configList[i].lodValues.Add(0);
            }

            for (var j = 0; j < configList[i].lodValues.Count; j++)
            {
                configList[i].lodValues[j] = lodValues[j];
            }
        }

        private static void SaveAsConfig()
        {
            var jsonText = JsonUtility.ToJson(new JsonSerialization<LODBatchSet>(configList));
            var path = Path.GetFullPath("Assets/Art/Effects/LODToolConfig.json");
            File.WriteAllText(path, jsonText);
        }

        private static void RefreshSelection()
        {
            if (lodGroups == null)
            {
                lodGroups = new List<LODGroup>();
            }
            else
            {
                lodGroups.Clear();
            }

            var selections = Selection.GetTransforms(SelectionMode.Unfiltered);
            foreach (var trans in selections)
            {
                if (trans.TryGetComponent<LODGroup>(out var lod))
                {
                    if (ReferenceEquals(lod, null) == false)
                    {
                        lodGroups.Add(lod);
                    }
                }
            }
        }
    }
}
