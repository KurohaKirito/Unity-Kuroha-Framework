using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using System.Reflection;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QTagLayerComponent: QHierarchyBaseComponent
    {
        // PRIVATE
        private GUIStyle labelStyle;
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
        private Rect tagRect = new Rect();
        private Rect layerRect = new Rect();
        private bool needDrawTag;
        private bool needDrawLayer;
        private int layer;
        private string tag;

        // CONSTRUCTOR
        public QTagLayerComponent()
        {
            labelStyle = new GUIStyle();
            labelStyle.fontSize = 8;
            labelStyle.clipping = TextClipping.Clip;  
            labelStyle.alignment = TextAnchor.MiddleLeft;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeShowType       , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerType               , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeValueType      , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeValuePixel     , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerSizeValuePercent   , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerLabelSize          , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerShow               , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerShowDuringPlayMode , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerTagLabelColor      , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerLayerLabelColor    , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerAlignment          , settingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TagAndLayerLabelAlpha         , settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            showAlways  = QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerType) == (int)EM_QHierarchyTagAndLayerType.Always;
            showType    = (EM_QHierarchyTagAndLayerShowType)QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerSizeShowType);
            sizeIsPixel = QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerSizeValueType) == (int)EM_QHierarchyTagAndLayerSizeType.Pixel;
            pixelSize   = QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerSizeValuePixel);
            percentSize = QSettings.Instance().Get<float>(EM_QHierarchySettings.TagAndLayerSizeValuePercent);
            labelSize   = (EM_QHierarchyTagAndLayerLabelSize)QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerLabelSize);
            enabled     = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TagAndLayerShow);
            tagColor    = QSettings.Instance().GetColor(EM_QHierarchySettings.TagAndLayerTagLabelColor);
            layerColor  = QSettings.Instance().GetColor(EM_QHierarchySettings.TagAndLayerLayerLabelColor);
            labelAlpha  = QSettings.Instance().Get<float>(EM_QHierarchySettings.TagAndLayerLabelAlpha);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TagAndLayerShowDuringPlayMode);

            EM_QHierarchyTagAndLayerAlignment alignment = (EM_QHierarchyTagAndLayerAlignment)QSettings.Instance().Get<int>(EM_QHierarchySettings.TagAndLayerAlignment);
            switch (alignment)
            {
                case EM_QHierarchyTagAndLayerAlignment.Left  : labelStyle.alignment = TextAnchor.MiddleLeft;   break;
                case EM_QHierarchyTagAndLayerAlignment.Center: labelStyle.alignment = TextAnchor.MiddleCenter; break;
                case EM_QHierarchyTagAndLayerAlignment.Right : labelStyle.alignment = TextAnchor.MiddleRight;  break;
            }
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            float textWidth = sizeIsPixel ? pixelSize : percentSize * rect.x;
            rect.width = textWidth + 4;

            if (maxWidth < rect.width)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= rect.width + 2;
                rect.x = curRect.x;
                rect.y = curRect.y;
                rect.y += (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
                //rect.height = EditorGUIUtility.singleLineHeight;

                layer  = gameObject.layer; 
                tag = getTagName(gameObject);             
                
                needDrawTag   = (showType != EM_QHierarchyTagAndLayerShowType.Layer) && ((showAlways || tag   != "Untagged"));
                needDrawLayer = (showType != EM_QHierarchyTagAndLayerShowType.Tag  ) && ((showAlways || layer != 0         ));

                #if UNITY_2019_1_OR_NEWER
                    if (labelSize == EM_QHierarchyTagAndLayerLabelSize.Big || (labelSize == EM_QHierarchyTagAndLayerLabelSize.BigIfSpecifiedOnlyTagOrLayer && needDrawTag != needDrawLayer)) 
                        labelStyle.fontSize = 8;
                    else 
                        labelStyle.fontSize = 7;
                #else
                    if (labelSize == EM_QHierarchyTagAndLayerLabelSize.Big || (labelSize == EM_QHierarchyTagAndLayerLabelSize.BigIfSpecifiedOnlyTagOrLayer && needDrawTag != needDrawLayer)) 
                        labelStyle.fontSize = 9;
                    else 
                        labelStyle.fontSize = 8;
                #endif

                if (needDrawTag) tagRect.Set(rect.x, rect.y - (needDrawLayer ? 4 : 0), rect.width, rect.height);                                                   
                if (needDrawLayer) layerRect.Set(rect.x, rect.y + (needDrawTag ? 4 : 0), rect.width, rect.height);                    

                return EM_QLayoutStatus.Success;
            }
        }

        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            if (needDrawTag ) 
            {
                tagColor.a = (tag == "Untagged" ? labelAlpha : 1.0f);
                labelStyle.normal.textColor = tagColor;
                EditorGUI.LabelField(tagRect, tag, labelStyle);
            }

            if (needDrawLayer) 
            {
                layerColor.a = (layer == 0 ? labelAlpha : 1.0f);
                labelStyle.normal.textColor = layerColor;
                EditorGUI.LabelField(layerRect, getLayerName(layer), labelStyle);
            }
        }

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

                if (needDrawTag && tagRect.Contains(Event.current.mousePosition))
                {
                    gameObjects = Selection.Contains(gameObject) ? Selection.gameObjects : new GameObject[] { gameObject };
                    showTagsContextMenu(tag);
                    Event.current.Use();
                }
                else if (needDrawLayer && layerRect.Contains(Event.current.mousePosition))
                {
                    gameObjects = Selection.Contains(gameObject) ? Selection.gameObjects : new GameObject[] { gameObject };
                    showLayersContextMenu(LayerMask.LayerToName(layer));
                    Event.current.Use();
                }
            }
        }

        private string getTagName(GameObject gameObject)
        {
            string tag = "Undefined";
            try { tag = gameObject.tag; }
            catch {}
            return tag;
        }

        public string getLayerName(int layer)
        {
            string layerName = LayerMask.LayerToName(layer);
            if (layerName.Equals("")) layerName = "Undefined";
            return layerName;
        }

        // PRIVATE
        private void showTagsContextMenu(string tag)
        {
            List<string> tags = new List<string>(UnityEditorInternal.InternalEditorUtility.tags);
            
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Untagged"  ), false, tagChangedHandler, "Untagged");
            
            for (int i = 0, n = tags.Count; i < n; i++)
            {
                string curTag = tags[i];
                menu.AddItem(new GUIContent(curTag), tag == curTag, tagChangedHandler, curTag);
            }
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Add Tag..."  ), false, addTagOrLayerHandler, "Tags");
            menu.ShowAsContext();
        }

        private void showLayersContextMenu(string layer)
        {
            List<string> layers = new List<string>(UnityEditorInternal.InternalEditorUtility.layers);
            
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Default"  ), false, layerChangedHandler, "Default");
            
            for (int i = 0, n = layers.Count; i < n; i++)
            {
                string curLayer = layers[i];
                menu.AddItem(new GUIContent(curLayer), layer == curLayer, layerChangedHandler, curLayer);
            }
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Add Layer..."  ), false, addTagOrLayerHandler, "Layers");
            menu.ShowAsContext();
        }

        private void tagChangedHandler(object newTag)
        {
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                Undo.RecordObject(gameObject, "Change Tag");
                gameObject.tag = (string)newTag;
                EditorUtility.SetDirty(gameObject);
            }
        }

        private void layerChangedHandler(object newLayer)
        {
            int newLayerId = LayerMask.NameToLayer((string)newLayer);
            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                Undo.RecordObject(gameObject, "Change Layer");
                gameObject.layer = newLayerId;
                EditorUtility.SetDirty(gameObject);
            }
        }

        private void addTagOrLayerHandler(object value)
        {
            PropertyInfo propertyInfo = typeof(EditorApplication).GetProperty("tagManager", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty);
            UnityEngine.Object obj = (UnityEngine.Object)(propertyInfo.GetValue(null, null));
            obj.GetType().GetField("m_DefaultExpandedFoldout").SetValue(obj, value);
            Selection.activeObject = obj;
        }
    }
}
