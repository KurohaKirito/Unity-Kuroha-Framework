using System;
using System.Text.RegularExpressions;
using Kuroha.Tool.AssetTool.AssetBatchTool.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.AssetRenameTool.Editor
{
    public static class AssetRenameTool
    {
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

        private static bool UI_Foldout = true;
        private static Vector2 UI_ScrollView;
        private static ReorderableList UI_List;

        /// <summary>
        /// 重命名步骤设置
        /// </summary>
        private static RenameStepScriptableObject renameSetting;

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            CheckRenameSetting();

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            UI_Foldout = EditorGUILayout.Foldout(UI_Foldout, AssetBatchToolGUI.batches[(int) AssetBatchToolGUI.BatchType.AssetRenameTool], true);
            if (UI_Foldout == false) return;

            GUILayout.Space(UI_DEFAULT_MARGIN);
            GUILayout.BeginVertical("Box");
            {
                CheckList();
                UI_List.DoLayoutList();

                GUILayout.Space(UI_DEFAULT_MARGIN);

                if (GUILayout.Button("Rename Assets", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                {
                    DoRename();
                }

                GUILayout.Space(UI_DEFAULT_MARGIN);

                if (Selection.objects != null)
                {
                    UI_ScrollView = GUILayout.BeginScrollView(UI_ScrollView, GUILayout.Height(527));
                    for (int index = 0; index < Selection.objects.Length; index++)
                    {
                        EditorGUILayout.ObjectField($"序号:   {index + 1}", Selection.objects[index], typeof(UnityEngine.Object), false);
                    }

                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 执行重命名操作
        /// </summary>
        private static void DoRename()
        {
            // 保存重命名步骤
            EditorUtility.SetDirty(renameSetting);

            // 执行重命名步骤
            foreach (var step in renameSetting.steps)
            {
                switch (step.operaType)
                {
                    case OperaType.Delete:
                        Do_Delete(Selection.objects, step.deleteStep);
                        break;
                    case OperaType.Remove:
                        Do_Remove(Selection.objects, step.removeStep);
                        break;
                    case OperaType.Insert:
                        Do_Insert(Selection.objects, step.insertStep);
                        break;
                    case OperaType.Replace:
                        Do_Replace(Selection.objects, step.replaceStep);
                        break;
                    case OperaType.Sort:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // 保存刷新
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 检查设置文件
        /// </summary>
        private static void CheckRenameSetting()
        {
            const string PATH = "Assets/Kuroha/Tool/AssetTool/AssetRenameTool/Editor/RenameStepSetting.Asset";
            if (renameSetting != null)
            {
                return;
            }

            renameSetting = AssetDatabase.LoadAssetAtPath<RenameStepScriptableObject>(PATH);
            if (renameSetting != null)
            {
                return;
            }

            renameSetting = ScriptableObject.CreateInstance<RenameStepScriptableObject>();
            AssetDatabase.CreateAsset(renameSetting, PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 检查 List 创建
        /// </summary>
        private static void CheckList()
        {
            if (UI_List != null)
            {
                return;
            }

            UI_List = new ReorderableList(renameSetting.steps, typeof(RenameStep))
            {
                draggable = true,
                displayAdd = true,
                displayRemove = true,
                drawElementCallback = DrawElement,
                drawHeaderCallback = DrawHeader
            };

            // UI_List.onAddCallback += ReorderableList.defaultBehaviours.DoAddButton;
            // ReorderableList.defaultBehaviours.DoAddButton(itemList);

            // UI_List.onRemoveCallback += RemoveItem;
            // ReorderableList.defaultBehaviours.DoRemoveButton(itemList);
        }

        #region 窗口 GUI 绘制相关方法

        private static void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "重命名步骤");
        }

        private static void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.width = 75;
            EditorGUI.LabelField(rect, $"{index + 1} - 操作类型:");
            DrawElementOpera_Type(rect, renameSetting.steps[index]);
        }

        private static void DrawElementOpera_Type(Rect rect, RenameStep step)
        {
            var curRect = new Rect(rect.x + rect.width + 3, rect.y + 1, 65, rect.height);
            step.operaType = (OperaType) EditorGUI.EnumPopup(curRect, step.operaType);

            curRect.y -= 1;
            switch (step.operaType)
            {
                case OperaType.Delete:
                    DrawElementOpera_Delete(curRect, step);
                    break;
                case OperaType.Remove:
                    DrawElementOpera_Remove(curRect, step);
                    break;
                case OperaType.Insert:
                    DrawElementOpera_Insert(curRect, step);
                    break;
                case OperaType.Replace:
                    DrawElementOpera_Replace(curRect, step);
                    break;
                case OperaType.Sort:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawElementOpera_Delete(Rect rect, RenameStep step)
        {
            var curRect = new Rect(rect.x + rect.width + 5, rect.y, 55, rect.height);
            EditorGUI.LabelField(curRect, "开始索引:");
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 130, rect.height - 4);
            step.deleteStep.beginIndex = EditorGUI.IntField(curRect, step.deleteStep.beginIndex);

            curRect = new Rect(curRect.x + curRect.width + 5, rect.y, 55, rect.height);
            EditorGUI.LabelField(curRect, "删除长度:");
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 130, rect.height - 4);
            step.deleteStep.length = EditorGUI.IntField(curRect, step.deleteStep.length);
        }

        private static void DrawElementOpera_Remove(Rect rect, RenameStep step)
        {
            var curRect = new Rect(rect.x + rect.width + 5, rect.y, 55, rect.height);
            EditorGUI.LabelField(curRect, "正则匹配:");
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 323, rect.height - 4);
            step.removeStep.regex = EditorGUI.TextField(curRect, step.removeStep.regex);
        }

        private static void DrawElementOpera_Replace(Rect rect, RenameStep step)
        {
            var curRect = new Rect(rect.x + rect.width + 5, rect.y, 55, rect.height);
            EditorGUI.LabelField(curRect, "正则匹配:");
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 130, rect.height - 4);
            step.replaceStep.regex = EditorGUI.TextField(curRect, step.replaceStep.regex);

            curRect = new Rect(curRect.x + curRect.width + 5, rect.y, 55, rect.height);
            EditorGUI.LabelField(curRect, "新字符串:");
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 130, rect.height - 4);
            step.replaceStep.newString = EditorGUI.TextField(curRect, step.replaceStep.newString);
        }

        private static void DrawElementOpera_Insert(Rect rect, RenameStep step)
        {
            var curRect = new Rect(rect.x + rect.width + 5, rect.y, 30, rect.height);
            EditorGUI.LabelField(curRect, "位置:");
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 55, rect.height);
            step.insertStep.paramType = (PositionType) EditorGUI.EnumPopup(curRect, step.insertStep.paramType);

            if (step.insertStep.paramType == PositionType.Index)
            {
                curRect = new Rect(curRect.x + curRect.width + 5, rect.y, 55, rect.height);
                EditorGUI.LabelField(curRect, "位置索引:");
                curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, 40, rect.height - 4);
                step.deleteStep.beginIndex = EditorGUI.IntField(curRect, step.deleteStep.beginIndex);
            }

            curRect = new Rect(curRect.x + curRect.width + 5, rect.y, 55, rect.height);
            EditorGUI.LabelField(curRect, "插入内容:");
            var width = step.insertStep.paramType == PositionType.Index ? 127 : 230;
            curRect = new Rect(curRect.x + curRect.width + 3, rect.y + 1, width, rect.height - 4);
            step.insertStep.content = EditorGUI.TextField(curRect, step.insertStep.content);
        }

        #endregion

        #region 重命名操作相关方法

        private static void Do_Delete(in UnityEngine.Object[] objects, DeleteStep info)
        {
            foreach (var asset in objects)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var newName = asset.name.Remove(info.beginIndex, info.length);
                AssetDatabase.RenameAsset(path, newName);
            }
        }

        private static void Do_Remove(in UnityEngine.Object[] objects, RemoveStep info)
        {
            var regex = new Regex(info.regex);

            foreach (var asset in objects)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var newName = regex.Replace(asset.name, "");
                if (newName != asset.name)
                {
                    AssetDatabase.RenameAsset(path, newName);
                }
            }
        }

        private static void Do_Replace(in UnityEngine.Object[] objects, ReplaceStep info)
        {
            var regex = new Regex(info.regex);

            foreach (var asset in objects)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                var newName = regex.Replace(asset.name, info.newString);
                if (newName != asset.name)
                {
                    AssetDatabase.RenameAsset(path, newName);
                }
            }
        }

        private static void Do_Insert(in UnityEngine.Object[] objects, InsertStep info)
        {
            foreach (var asset in objects)
            {
                var path = AssetDatabase.GetAssetPath(asset);

                var newName = info.paramType switch
                {
                    PositionType.Begin => asset.name.Insert(0, info.content),
                    PositionType.End => asset.name.Insert(asset.name.Length, info.content),
                    PositionType.Index => asset.name.Insert(info.index, info.content),
                    _ => throw new ArgumentOutOfRangeException()
                };

                AssetDatabase.RenameAsset(path, newName);
            }
        }

        #endregion
    }
}
