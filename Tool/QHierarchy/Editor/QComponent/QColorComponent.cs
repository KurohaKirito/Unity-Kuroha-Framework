using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QColorComponent : QBaseComponent
    {
        // PRIVATE
        private Color inactiveColor;
        private Texture2D colorTexture;
        private Rect colorRect = new Rect();

        // CONSTRUCTOR
        public QColorComponent()
        {
            colorTexture = QResources.getInstance().getTexture(QTexture.QColorButton);

            rect.width = 8;
            rect.height = 16;

            QSettings.getInstance().addEventListener(EM_QSetting.ColorShow, settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.ColorShowDuringPlayMode, settingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalInactiveColor, settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            enabled = QSettings.getInstance().get<bool>(EM_QSetting.ColorShow);
            showComponentDuringPlayMode = QSettings.getInstance().get<bool>(EM_QSetting.ColorShowDuringPlayMode);
            inactiveColor = QSettings.getInstance().getColor(EM_QSetting.AdditionalInactiveColor);
        }

        // LAYOUT
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 8)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= 8;
                rect.x = curRect.x;
                rect.y = curRect.y;
                return EM_QLayoutStatus.Success;
            }
        }

        // DRAW
        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            if (objectList != null)
            {
                Color newColor;
                if (objectList.gameObjectColor.TryGetValue(gameObject, out newColor))
                {
                    colorRect.Set(rect.x + 1, rect.y + 1, 5, rect.height - 1);
                    EditorGUI.DrawRect(colorRect, newColor);
                    return;
                }
            }

            QColorUtils.setColor(inactiveColor);
            UnityEngine.GUI.DrawTexture(rect, colorTexture, ScaleMode.StretchToFill, true, 1);
            QColorUtils.clearColor();
        }

        // EVENTS
        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    try
                    {
                        PopupWindow.Show(rect, new QColorPickerWindow(Selection.Contains(gameObject) ? Selection.gameObjects : new GameObject[] {gameObject}, colorSelectedHandler, colorRemovedHandler));
                    }
                    catch
                    {
                    }
                }

                currentEvent.Use();
            }
        }

        // PRIVATE
        private void colorSelectedHandler(GameObject[] gameObjects, Color color)
        {
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                QObjectList objectList = QObjectListManager.getInstance().getObjectList(gameObjects[i], true);
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

        private void colorRemovedHandler(GameObject[] gameObjects)
        {
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                QObjectList objectList = QObjectListManager.getInstance().getObjectList(gameObjects[i], true);
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
