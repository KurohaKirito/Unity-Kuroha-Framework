using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QStaticComponent: QBaseComponent
    {
        // PRIVATE
        private Color activeColor;
        private Color inactiveColor;
        private StaticEditorFlags staticFlags;
        private GameObject[] gameObjects;
        private Texture2D staticButton;
        private Color32[] staticButtonColors;

        // CONSTRUCTOR
        public QStaticComponent()
        {
            rect.width = 11;
            rect.height = 10;

            QSettings.getInstance().addEventListener(EM_QSetting.StaticShow                , SettingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.StaticShowDuringPlayMode  , SettingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalActiveColor     , SettingsChanged);
            QSettings.getInstance().addEventListener(EM_QSetting.AdditionalInactiveColor   , SettingsChanged);

            SettingsChanged();
        }

        // PRIVATE
        private void SettingsChanged()
        {
            enabled                     = QSettings.getInstance().get<bool>(EM_QSetting.StaticShow);
            showComponentDuringPlayMode = QSettings.getInstance().get<bool>(EM_QSetting.StaticShowDuringPlayMode);
            activeColor                 = QSettings.getInstance().getColor(EM_QSetting.AdditionalActiveColor);
            inactiveColor               = QSettings.getInstance().getColor(EM_QSetting.AdditionalInactiveColor);
        }

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < 13)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= 13;
                rect.x = curRect.x;
                rect.y = curRect.y + 4;
                staticFlags = GameObjectUtility.GetStaticEditorFlags(gameObject);
                return EM_QLayoutStatus.Success;
            }
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            if (staticButton == null)
            {
                staticButton = new Texture2D(11, 10, TextureFormat.ARGB32, false, true);
                staticButtonColors = new Color32[11 * 10];
            }
            
            // drawQuad(37, 3, 4, (staticFlags & StaticEditorFlags.LightmapStatic       ) > 0);
            DrawQuad(37, 3, 4, (staticFlags & StaticEditorFlags.ContributeGI         ) > 0);
            DrawQuad(33, 3, 4, (staticFlags & StaticEditorFlags.BatchingStatic       ) > 0);
            DrawQuad(41, 3, 4, (staticFlags & StaticEditorFlags.ReflectionProbeStatic) > 0);
            DrawQuad( 0, 5, 2, (staticFlags & StaticEditorFlags.OccludeeStatic       ) > 0);
            DrawQuad( 6, 5, 2, (staticFlags & StaticEditorFlags.OccluderStatic       ) > 0);
            DrawQuad(88, 5, 2, (staticFlags & StaticEditorFlags.NavigationStatic     ) > 0);
            DrawQuad(94, 5, 2, (staticFlags & StaticEditorFlags.OffMeshLinkGeneration) > 0);

            staticButton.SetPixels32(staticButtonColors);
            staticButton.Apply();
            UnityEngine.GUI.DrawTexture(rect, staticButton);
        }

        public override void EventHandler(GameObject gameObject, QObjectList objectList, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();

                var intStaticFlags = (int)staticFlags;
                gameObjects = Selection.Contains(gameObject) ? Selection.gameObjects : new GameObject[] { gameObject };

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Nothing"                   ), intStaticFlags == 0, StaticChangeHandler, 0);
                menu.AddItem(new GUIContent("Everything"                ), intStaticFlags == -1, StaticChangeHandler, -1);
                // menu.AddItem(new GUIContent("Lightmap Static"           ), (intStaticFlags & (int)StaticEditorFlags.LightmapStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.LightmapStatic);
                menu.AddItem(new GUIContent("Lightmap Static"           ), (intStaticFlags & (int)StaticEditorFlags.ContributeGI) > 0, StaticChangeHandler, (int)StaticEditorFlags.ContributeGI);
                menu.AddItem(new GUIContent("Occluder Static"           ), (intStaticFlags & (int)StaticEditorFlags.OccluderStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.OccluderStatic);
                menu.AddItem(new GUIContent("Batching Static"           ), (intStaticFlags & (int)StaticEditorFlags.BatchingStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.BatchingStatic);
                menu.AddItem(new GUIContent("Navigation Static"         ), (intStaticFlags & (int)StaticEditorFlags.NavigationStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.NavigationStatic);
                menu.AddItem(new GUIContent("Occludee Static"           ), (intStaticFlags & (int)StaticEditorFlags.OccludeeStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.OccludeeStatic);
                menu.AddItem(new GUIContent("Off Mesh Link Generation"  ), (intStaticFlags & (int)StaticEditorFlags.OffMeshLinkGeneration) > 0, StaticChangeHandler, (int)StaticEditorFlags.OffMeshLinkGeneration);
                #if UNITY_4_6 || UNITY_4_7
                #else
                menu.AddItem(new GUIContent("Reflection Probe Static"   ), (intStaticFlags & (int)StaticEditorFlags.ReflectionProbeStatic) > 0, StaticChangeHandler, (int)StaticEditorFlags.ReflectionProbeStatic);
                #endif
                menu.ShowAsContext();
            }
        }

        // PRIVATE
        private void StaticChangeHandler(object result)
        {
            var intResult = (int)result;
            var resultStaticFlags = (StaticEditorFlags)result;
            if (intResult != 0 && intResult != -1)
            {
                resultStaticFlags = staticFlags ^ resultStaticFlags;
            }

            for (int i = gameObjects.Length - 1; i >= 0; i--)
            {
                GameObject gameObject = gameObjects[i];
                Undo.RecordObject(gameObject, "Change Static Flags");            
                GameObjectUtility.SetStaticEditorFlags(gameObject, resultStaticFlags);
                EditorUtility.SetDirty(gameObject);
            }
        }

        private void DrawQuad(int startPosition, int width, int height, bool isActiveColor)
        {
            Color32 color = isActiveColor ? activeColor : inactiveColor;
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

