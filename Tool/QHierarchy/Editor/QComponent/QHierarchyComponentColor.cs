using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.RunTime;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentColor : QHierarchyBaseComponent
    {
        private Rect colorRect;
        private Color inactiveColor;
        private readonly Texture2D colorTexture;

        private const float COLOR_RECT_SPACE = 1;
        private const float COMPONENT_WIDTH = 8;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentColor()
        {
            colorTexture = QResources.Instance().GetTexture(QTexture.QColorButton);

            rect.width = 8;
            rect.height = 16;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ColorShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ColorShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 设置更改
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ColorShow);

            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ColorShowDuringPlayMode);

            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        /// <summary>
        /// 进行布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < COMPONENT_WIDTH)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= COMPONENT_WIDTH;
                rect.x = curRect.x;
                rect.y = curRect.y;
                return EM_QLayoutStatus.Success;
            }
        }

        /// <summary>
        /// 进行绘制
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            if (hierarchyObjectList != null)
            {
                if (hierarchyObjectList.gameObjectColor.TryGetValue(gameObject, out var newColor))
                {
                    colorRect.x = rect.x + COLOR_RECT_SPACE;
                    colorRect.y = rect.y + COLOR_RECT_SPACE * 2;
                    colorRect.width = COMPONENT_WIDTH - COLOR_RECT_SPACE * 2;
                    colorRect.height = rect.height - COLOR_RECT_SPACE * 3f;

                    EditorGUI.DrawRect(colorRect, newColor);

                    return;
                }
            }

            QHierarchyColorUtils.SetColor(inactiveColor);

            UnityEngine.GUI.DrawTexture(rect, colorTexture, ScaleMode.StretchToFill, true, 1);

            QHierarchyColorUtils.ClearColor();
        }

        /// <summary>
        /// 鼠标单击事件
        /// </summary>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    try
                    {
                        var obj = Selection.Contains(gameObject)
                            ? Selection.gameObjects
                            : new[]
                            {
                                gameObject
                            };
                        var newPopupWindow = new QHierarchyColorPaletteWindow(obj, ColorSelectedHandler, ColorRemovedHandler);
                        PopupWindow.Show(rect, newPopupWindow);
                    }
                    catch
                    {
                        // 忽略 UnityEngine.ExitGUIException 异常
                        // 按照 Unity 官方的说法, 此异常无毒无害, 可忽略
                    }
                }

                currentEvent.Use();
            }
        }

        /// <summary>
        /// 选中颜色事件
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <param name="color"></param>
        private static void ColorSelectedHandler(GameObject[] gameObjects, Color color)
        {
            for (var i = gameObjects.Length - 1; i >= 0; i--)
            {
                var gameObject = gameObjects[i];
                var objectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjects[i]);
                Undo.RecordObject(objectList, "Color Changed");
                if (objectList.gameObjectColor.ContainsKey(gameObject))
                {
                    objectList.gameObjectColor[gameObject] = color;
                }
                else
                {
                    objectList.gameObjectColor.Add(gameObject, color);
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// 清除颜色事件
        /// </summary>
        /// <param name="gameObjects"></param>
        private static void ColorRemovedHandler(GameObject[] gameObjects)
        {
            for (var index = gameObjects.Length - 1; index >= 0; index--)
            {
                var gameObject = gameObjects[index];

                var objectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjects[index]);

                if (objectList.gameObjectColor.ContainsKey(gameObject))
                {
                    Undo.RecordObject(objectList, "Color Changed");
                    objectList.gameObjectColor.Remove(gameObject);
                }
            }

            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
