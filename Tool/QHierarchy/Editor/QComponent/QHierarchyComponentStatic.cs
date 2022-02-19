using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentStatic : QHierarchyBaseComponent
    {
        private Color activeColor;
        private Color inactiveColor;
        
        private GameObject[] gameObjects;
        
        private Texture2D staticButton;
        private Color32[] staticButtonColors;
        private StaticEditorFlags staticFlags;
        
        private const int ICON_WIDTH = 11;
        private const int ICON_HEIGHT = 10;
        
        /// <summary>
        /// 构造方法
        /// </summary>
        public QHierarchyComponentStatic()
        {
            rect.width = ICON_WIDTH;
            rect.height = ICON_HEIGHT;

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.StaticShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.StaticShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.StaticShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.StaticShowDuringPlayMode);
            activeColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < ICON_WIDTH + COMPONENT_SPACE)
            {
                return EM_QLayoutStatus.Failed;
            }
            
            curRect.x -= ICON_WIDTH + COMPONENT_SPACE;
            
            rect.x = curRect.x;
            rect.y = curRect.y + (16 - ICON_HEIGHT) / 2f;
            staticFlags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            if (staticButton == null)
            {
                staticButton = new Texture2D(11, 10, TextureFormat.ARGB32, false, true);
                staticButtonColors = new Color32[11 * 10];
            }

            DrawQuad(37, 3, 4, (staticFlags & StaticEditorFlags.ContributeGI) > 0);
            DrawQuad(6, 5, 2, (staticFlags & StaticEditorFlags.OccluderStatic) > 0);
            DrawQuad(33, 3, 4, (staticFlags & StaticEditorFlags.BatchingStatic) > 0);
            DrawQuad(88, 5, 2, (staticFlags & StaticEditorFlags.NavigationStatic) > 0);
            DrawQuad(0, 5, 2, (staticFlags & StaticEditorFlags.OccludeeStatic) > 0);
            DrawQuad(94, 5, 2, (staticFlags & StaticEditorFlags.OffMeshLinkGeneration) > 0);
            DrawQuad(41, 3, 4, (staticFlags & StaticEditorFlags.ReflectionProbeStatic) > 0);

            staticButton.SetPixels32(staticButtonColors);
            staticButton.Apply();
            
            UnityEngine.GUI.DrawTexture(rect, staticButton);
        }

        /// <summary>
        /// 左键单击事件
        /// </summary>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();

                var intStaticFlags = (int)staticFlags;
                gameObjects = Selection.Contains(gameObject)? Selection.gameObjects : new [] { gameObject };

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Nothing"), intStaticFlags == 0, StaticChangeHandler, 0);
                menu.AddItem(new GUIContent("Everything"), intStaticFlags == -1, StaticChangeHandler, -1);
                // menu.AddItem(new GUIContent("Lightmap Static"           ), (intStaticFlags & (int)StaticEditorFlags.LightmapStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.LightmapStatic);
                menu.AddItem(new GUIContent("Lightmap Static"), (intStaticFlags & (int)StaticEditorFlags.ContributeGI) > 0, StaticChangeHandler, (int)StaticEditorFlags.ContributeGI);
                menu.AddItem(new GUIContent("Occluder Static"), (intStaticFlags & (int)StaticEditorFlags.OccluderStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.OccluderStatic);
                menu.AddItem(new GUIContent("Batching Static"), (intStaticFlags & (int)StaticEditorFlags.BatchingStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.BatchingStatic);
                menu.AddItem(new GUIContent("Navigation Static"), (intStaticFlags & (int)StaticEditorFlags.NavigationStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.NavigationStatic);
                menu.AddItem(new GUIContent("Occludee Static"), (intStaticFlags & (int)StaticEditorFlags.OccludeeStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.OccludeeStatic);
                menu.AddItem(new GUIContent("Off Mesh Link Generation"), (intStaticFlags & (int)StaticEditorFlags.OffMeshLinkGeneration) > 0, StaticChangeHandler, (int)StaticEditorFlags.OffMeshLinkGeneration);
                menu.AddItem(new GUIContent("Reflection Probe Static"), (intStaticFlags & (int)StaticEditorFlags.ReflectionProbeStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.ReflectionProbeStatic);
                
                menu.ShowAsContext();
            }
        }

        /// <summary>
        /// 静态标志更改
        /// </summary>
        private void StaticChangeHandler(object result) {
            var intResult = (int)result;
            var resultStaticFlags = (StaticEditorFlags)result;
            if (intResult != 0 && intResult != -1) {
                resultStaticFlags = staticFlags ^ resultStaticFlags;
            }

            for (var i = gameObjects.Length - 1; i >= 0; i--) {
                var gameObject = gameObjects[i];
                Undo.RecordObject(gameObject, "Change Static Flags");
                GameObjectUtility.SetStaticEditorFlags(gameObject, resultStaticFlags);
                EditorUtility.SetDirty(gameObject);
            }
        }

        /// <summary>
        /// 绘制方块
        /// </summary>
        private void DrawQuad(int startPosition, int width, int height, bool isActiveColor)
        {
            var color = (Color32)(isActiveColor? activeColor : inactiveColor);
            
            for (var iy = 0; iy < height; iy++)
            {
                for (var ix = 0; ix < width; ix++)
                {
                    var pos = startPosition + ix + iy * 11;
                    
                    staticButtonColors[pos].r = color.r;
                    staticButtonColors[pos].g = color.g;
                    staticButtonColors[pos].b = color.b;
                    staticButtonColors[pos].a = color.a;
                }
            }
        }
    }
}