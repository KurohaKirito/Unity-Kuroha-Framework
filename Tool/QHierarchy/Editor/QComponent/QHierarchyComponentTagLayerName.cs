using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using System.Reflection;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentTagLayerName : QHierarchyBaseComponent
    {
        private readonly GUIStyle labelStyle;
        private EM_QHierarchyTagAndLayerShowType showType;
        private Color layerColor;
        private Color tagColor;
        private bool showAlways;
        private bool sizeIsPixel;
        private int pixelSize;
        private float percentSize;
        private GameObject[] gameObjects;
        private float labelAlpha;
        private EM_QHierarchyTagAndLayerLabelSize labelSize;
        private Rect tagRect;
        private Rect layerRect;
        private bool needDrawTag;
        private bool needDrawLayer;
        private int layer;
        private string tag;

        /// <summary>
        /// 构造方法
        /// </summary>
        public QHierarchyComponentTagLayerName()
        {
            labelStyle = new GUIStyle
            {
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
            };

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeShowType, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerType, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeValueType, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeValuePixel, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeValuePercent, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerLabelSize, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerTagLabelColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerLayerLabelColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerAlignment, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerLabelAlpha, SettingsChanged);
            
            SettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void SettingsChanged()
        {
            showAlways = QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerType) == (int) EM_QHierarchyTagAndLayerType.总是显示全部名称;
            showType = (EM_QHierarchyTagAndLayerShowType) QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerSizeShowType);
            sizeIsPixel = QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerSizeValueType) == (int) EM_QHierarchyTagAndLayerSizeType.像素值;
            pixelSize = QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerSizeValuePixel);
            percentSize = QSettings.Instance().Get<float>(EM_QHierarchySettings.TagAndLayerSizeValuePercent);
            labelSize = (EM_QHierarchyTagAndLayerLabelSize) QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerLabelSize);
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TagAndLayerShow);
            tagColor = QSettings.Instance().GetColor(EM_QHierarchySettings.TagAndLayerTagLabelColor);
            layerColor = QSettings.Instance().GetColor(EM_QHierarchySettings.TagAndLayerLayerLabelColor);
            labelAlpha = QSettings.Instance().Get<float>(EM_QHierarchySettings.TagAndLayerLabelAlpha);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TagAndLayerShowDuringPlayMode);

            var alignment = (EM_QHierarchyTagAndLayerAlignment) QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerAlignment);
            labelStyle.alignment = alignment switch
            {
                EM_QHierarchyTagAndLayerAlignment.Left => TextAnchor.MiddleLeft,
                EM_QHierarchyTagAndLayerAlignment.Center => TextAnchor.MiddleCenter,
                EM_QHierarchyTagAndLayerAlignment.Right => TextAnchor.MiddleRight,
                _ => labelStyle.alignment
            };
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            var textWidth = sizeIsPixel ? pixelSize : percentSize * rect.x;
            
            rect.width = textWidth + 4;
            
            if (maxWidth < rect.width)
            {
                return EM_QLayoutStatus.Failed;
            }
            
            curRect.x -= rect.width + 2;
            rect.x = curRect.x;
            rect.y = curRect.y;

            layer = gameObject.layer;
            tag = gameObject.tag;

            needDrawTag = showType != EM_QHierarchyTagAndLayerShowType.仅显示层级 && showAlways || tag != "Untagged";
            needDrawLayer = showType != EM_QHierarchyTagAndLayerShowType.仅显示标签 && showAlways || layer != 0;

            labelStyle.fontSize = labelSize switch
            {
                EM_QHierarchyTagAndLayerLabelSize.BigIfOnlyShowTagOrLayer when needDrawTag != needDrawLayer => 14,
                EM_QHierarchyTagAndLayerLabelSize.Big => 10,
                _ => 8
            };

            if (needDrawTag)
            {
                tagRect.Set(rect.x, rect.y - (needDrawLayer ? 4 : 0), rect.width, rect.height);
            }

            if (needDrawLayer)
            {
                layerRect.Set(rect.x, rect.y + (needDrawTag ? 4 : 0), rect.width, rect.height);
            }

            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            if (needDrawTag)
            {
                tagColor.a = tag == "Untagged" ? labelAlpha : 1.0f;
                labelStyle.normal.textColor = tagColor;
                EditorGUI.LabelField(tagRect, tag, labelStyle);
            }

            if (needDrawLayer)
            {
                layerColor.a = layer == 0 ? labelAlpha : 1.0f;
                labelStyle.normal.textColor = layerColor;
                EditorGUI.LabelField(layerRect, GetLayerName(layer), labelStyle);
            }
        }

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (Event.current.isMouse && currentEvent.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (needDrawTag && needDrawLayer)
                {
                    tagRect.height = 8;
                    layerRect.height = 8;
                    tagRect.y += 4;
                    layerRect.y += 4;
                }

                // 显示 Tag 菜单
                if (needDrawTag && tagRect.Contains(Event.current.mousePosition))
                {
                    gameObjects = Selection.Contains(gameObject) ? Selection.gameObjects : new[] { gameObject };
                    ShowTagsContextMenu();
                    Event.current.Use();
                }
                
                // 显示 Layer 菜单
                if (needDrawLayer && layerRect.Contains(Event.current.mousePosition))
                {
                    gameObjects = Selection.Contains(gameObject) ? Selection.gameObjects : new[] { gameObject };
                    var layerName = LayerMask.LayerToName(layer);
                    ShowLayersContextMenu(layerName);
                    Event.current.Use();
                }
            }
        }

        /// <summary>
        /// 将 Layer 从 int 转为 string
        /// </summary>
        private static string GetLayerName(int layer)
        {
            var layerName = LayerMask.LayerToName(layer);
            
            if (layerName.Equals(""))
            {
                layerName = "Undefined";
            }
            
            return layerName;
        }

        private void ShowTagsContextMenu()
        {
            var tags = new List<string>(UnityEditorInternal.InternalEditorUtility.tags);

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Untagged"), false, TagChangedHandler, "Untagged");

            for (int i = 0, n = tags.Count; i < n; i++)
            {
                var curTag = tags[i];
                menu.AddItem(new GUIContent(curTag), tag == curTag, TagChangedHandler, curTag);
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Add Tag..."), false, AddTagOrLayerHandler, "Tags");
            menu.ShowAsContext();
        }

        private void ShowLayersContextMenu(string layerName)
        {
            var layers = new List<string>(UnityEditorInternal.InternalEditorUtility.layers);

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Default"), false, LayerChangedHandler, "Default");

            for (int i = 0, n = layers.Count; i < n; i++)
            {
                var curLayer = layers[i];
                menu.AddItem(new GUIContent(curLayer), layerName == curLayer, LayerChangedHandler, curLayer);
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Add Layer..."), false, AddTagOrLayerHandler, "Layers");
            menu.ShowAsContext();
        }

        private void TagChangedHandler(object newTag)
        {
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                Undo.RecordObject(gameObject, "Change Tag");
                gameObject.tag = (string) newTag;
                EditorUtility.SetDirty(gameObject);
            }
        }

        private void LayerChangedHandler(object newLayer)
        {
            int newLayerId = LayerMask.NameToLayer((string) newLayer);
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                Undo.RecordObject(gameObject, "Change Layer");
                gameObject.layer = newLayerId;
                EditorUtility.SetDirty(gameObject);
            }
        }

        private static void AddTagOrLayerHandler(object value)
        {
            var propertyInfo = typeof(EditorApplication).GetProperty("tagManager", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty);
            
            var obj = (UnityEngine.Object) propertyInfo?.GetValue(null, null);

            if (obj != null)
            {
                obj.GetType().GetField("m_DefaultExpandedFoldout").SetValue(obj, value);
                Selection.activeObject = obj;
            }
        }
    }
}
