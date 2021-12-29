using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QHierarchy
{
    public class QHierarchySettingsWindow : EditorWindow
    {
        #region 常量

        private const float SPACE_DEFAULT = 12;
        private const float MENU_WIDTH = 240;

        #endregion
        
        #region 私有变量

        private bool initFlag;
        private bool isProSkin;

        private bool isOpenComponentsSettings;
        private bool isOpenComponentsOrders;
        private bool isOpenAdditionalSettings;
        
        private int indentLevel;

        /// <summary>
        /// 窗口可绘制界面区域的总宽度
        /// 使用 window.position.width 获取的宽度无法自动适配右侧可能会出现的滑动条, 因此手动控制总宽度
        /// </summary>
        private float totalWidth;
        
        private Rect lastRect; 
        private Vector2 scrollPosition;
        private static QHierarchySettingsWindow window;
        
        private Color yellowColor;
        private Color separatorColor;

        private Texture2D checkBoxChecked;
        private Texture2D checkBoxUnchecked;
        private Texture2D restoreButtonTexture;

        private QComponentsOrderList componentsOrderList;

        #endregion

        /// <summary>
        /// 打开窗口
        /// </summary>
        [MenuItem("Tools/QHierarchy/Settings")]
        public static void Open()
        {
            window = GetWindow<QHierarchySettingsWindow>("QHierarchy");
            window.minSize = new Vector2(500, 75);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            // 未初始化或者编辑器更换了皮肤, 则进行初始化
            if (initFlag == false || isProSkin != EditorGUIUtility.isProSkin)
            {
                initFlag = true;

                isProSkin = EditorGUIUtility.isProSkin;
                separatorColor = isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.59f, 0.59f, 0.59f);
                yellowColor = isProSkin ? new Color(1.00f, 0.90f, 0.40f) : new Color(0.31f, 0.31f, 0.31f);

                checkBoxChecked = QResources.getInstance().getTexture(QTexture.QCheckBoxChecked);
                checkBoxUnchecked = QResources.getInstance().getTexture(QTexture.QCheckBoxUnchecked);
                restoreButtonTexture = QResources.getInstance().getTexture(QTexture.QRestoreButton);
                componentsOrderList = new QComponentsOrderList(this);
            }
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        private void OnGUI()
        {
            // 初始化
            Init();
            
            // 绘制前初始化 indentLevel
            indentLevel = 10;
            
            // 绘制前初始化 lastRect
            lastRect = new Rect(0, 1, 0, 0);

            // 绘制
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                // 获取可绘制区域的宽度, 高度默认是 EditorGUIUtility.singleLineHeight, 即 18
                // 这里不需要告诉 Layout 高度, 所以传 0, 如果不传值会导致使用了 Layout 系统的滑动条的范围高度多出部分像素
                // 另外 Unity 默认会有部分留白, 我们不需要留白, 所以最后再加上部分像素, 覆盖掉留白
                totalWidth = EditorGUILayout.GetControlRect(GUILayout.Height(0)).width + SPACE_DEFAULT;

                // 绘制菜单框
                DrawMenuBox("COMPONENTS SETTINGS", MENU_WIDTH, ref isOpenComponentsSettings);
                var sectionStartY = lastRect.y + lastRect.height;
                if (isOpenComponentsSettings)
                {
                    drawTreeMapComponentSettings();
                    DrawSeparator();
                    drawMonoBehaviourIconComponentSettings();
                    DrawSeparator();
                    drawSeparatorComponentSettings();
                    DrawSeparator();
                    drawVisibilityComponentSettings();
                    DrawSeparator();
                    drawLockComponentSettings();
                    DrawSeparator();
                    drawStaticComponentSettings();
                    DrawSeparator();
                    drawErrorComponentSettings();
                    DrawSeparator();
                    drawRendererComponentSettings();
                    DrawSeparator();
                    drawPrefabComponentSettings();
                    DrawSeparator();
                    drawTagLayerComponentSettings();
                    DrawSeparator();
                    drawColorComponentSettings();
                    DrawSeparator();
                    drawGameObjectIconComponentSettings();
                    DrawSeparator();
                    drawTagIconComponentSettings();
                    DrawSeparator();
                    drawLayerIconComponentSettings();
                    DrawSeparator();
                    drawChildrenCountComponentSettings();
                    DrawSeparator();
                    drawVerticesAndTrianglesCountComponentSettings();
                    DrawSeparator();
                    drawComponentsComponentSettings();
                    DrawLine(sectionStartY, lastRect.y + lastRect.height, separatorColor);
                }

                // 开始绘制
                DrawMenuBox("ORDER OF COMPONENTS", MENU_WIDTH, ref isOpenComponentsOrders);
                sectionStartY = lastRect.y + lastRect.height;
                if (isOpenComponentsOrders)
                {
                    drawSpace(6);
                    drawOrderSettings();
                    drawSpace(6);
                    DrawLine(sectionStartY, lastRect.y + lastRect.height, separatorColor);
                }

                // 开始绘制
                DrawMenuBox("ADDITIONAL SETTINGS", MENU_WIDTH, ref isOpenAdditionalSettings);
                sectionStartY = lastRect.y + lastRect.height;
                if (isOpenAdditionalSettings)
                {
                    drawSpace(3);
                    drawAdditionalSettings();
                    DrawLine(sectionStartY, lastRect.y + lastRect.height + 4, separatorColor);
                }

                indentLevel -= 1;
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 获得一个新的 UI 绘制区域
        /// 自动更新 lastRect
        /// </summary>
        private Rect GetNewRect(float width, float height, float addIndent = 0, float remWidth = 0)
        {
            // 为了使 GUILayout 的滑动条检测生效, 必须有这一句
            EditorGUILayout.GetControlRect(false, height, GUIStyle.none, GUILayout.ExpandWidth(true));
            var rect = new Rect(indentLevel + addIndent, lastRect.y + lastRect.height, width == 0
                ? totalWidth - indentLevel - addIndent - remWidth : width, height);
            lastRect = rect;
            return rect;
        }
        
        /// <summary>
        /// 绘制菜单
        /// </summary>
        private void DrawMenuBox(string sectionTitle, float buttonWidth, ref bool buttonFlag)
        {
            var rect = GetNewRect(0, 24, -3, 0);
            
            // 底色 Box
            rect.x = 0;
            UnityEngine.GUI.Box(rect, string.Empty);

            // 左侧竖线
            DrawLine(rect.y, rect.y + 24, yellowColor);
            rect.x += 10;

            // 折叠图标
            UnityEngine.GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            var autoCheckIcon = EditorGUIUtility.IconContent(buttonFlag ? "sv_icon_dot11_pix16_gizmo" : "sv_icon_dot8_pix16_gizmo");
            if (UnityEngine.GUI.Button(new Rect(rect.x, rect.y, buttonWidth, rect.height), autoCheckIcon))
            {
                buttonFlag = !buttonFlag;
            }

            rect.x += 28;

            // 设置图标
            EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("GameManager Icon"));
            rect.x += 28;

            // 名称
            EditorGUI.LabelField(rect, sectionTitle);
        }

        /// <summary>
        /// 绘制线
        /// </summary>
        private void DrawLine(float fromY, float toY, Color color, float width = 0)
        {
            EditorGUI.DrawRect(new Rect(0, fromY, width == 0 ? indentLevel : width, toY - fromY), color);
        }

        // COMPONENTS
        private void drawTreeMapComponentSettings()
        {
            if (drawComponentCheckBox("Hierarchy Tree", EM_QSetting.TreeMapShow))
            {
                var rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.TreeMapColor);
                    QSettings.getInstance().restore(EM_QSetting.TreeMapEnhanced);
                    QSettings.getInstance().restore(EM_QSetting.TreeMapTransparentBackground);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 3 + 5);
                drawSpace(4);
                drawColorPicker("Tree color", EM_QSetting.TreeMapColor);
                drawCheckBoxRight("Transparent background", EM_QSetting.TreeMapTransparentBackground);
                drawCheckBoxRight("Enhanced (\"Transform Sort\" only)", EM_QSetting.TreeMapEnhanced);
                drawSpace(1);
            }
        }

        private void drawMonoBehaviourIconComponentSettings()
        {
            if (drawComponentCheckBox("MonoBehaviour Icon", EM_QSetting.MonoBehaviourIconShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.MonoBehaviourIconShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.MonoBehaviourIconColor);
                    QSettings.getInstance().restore(EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 3 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.MonoBehaviourIconShowDuringPlayMode);
                drawColorPicker("Icon color", EM_QSetting.MonoBehaviourIconColor);
                drawCheckBoxRight("Ignore Unity MonoBehaviours", EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour);
                drawSpace(1);
            }
        }

        private void drawSeparatorComponentSettings()
        {
            if (drawComponentCheckBox("Separator", EM_QSetting.SeparatorShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.SeparatorColor);
                    QSettings.getInstance().restore(EM_QSetting.SeparatorShowRowShading);
                    QSettings.getInstance().restore(EM_QSetting.SeparatorOddRowShadingColor);
                    QSettings.getInstance().restore(EM_QSetting.SeparatorEvenRowShadingColor);
                }

                bool rowShading = QSettings.getInstance().get<bool>(EM_QSetting.SeparatorShowRowShading);

                drawBackground(rect.x, rect.y, rect.width, 18 * (rowShading ? 4 : 2) + 5);
                drawSpace(4);
                drawColorPicker("Separator Color", EM_QSetting.SeparatorColor);
                drawCheckBoxRight("Row shading", EM_QSetting.SeparatorShowRowShading);
                if (rowShading)
                {
                    drawColorPicker("Even row shading color", EM_QSetting.SeparatorEvenRowShadingColor);
                    drawColorPicker("Odd row shading color", EM_QSetting.SeparatorOddRowShadingColor);
                }

                drawSpace(1);
            }
        }

        private void drawVisibilityComponentSettings()
        {
            if (drawComponentCheckBox("Visibility", EM_QSetting.VisibilityShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.VisibilityShowDuringPlayMode);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.VisibilityShowDuringPlayMode);
                drawSpace(1);
            }
        }

        private void drawLockComponentSettings()
        {
            if (drawComponentCheckBox("Lock", EM_QSetting.LockShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.LockShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.LockPreventSelectionOfLockedObjects);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 2 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.LockShowDuringPlayMode);
                drawCheckBoxRight("Prevent selection of locked objects", EM_QSetting.LockPreventSelectionOfLockedObjects);
                drawSpace(1);
            }
        }

        private void drawStaticComponentSettings()
        {
            if (drawComponentCheckBox("Static", EM_QSetting.StaticShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.StaticShowDuringPlayMode);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.StaticShowDuringPlayMode);
                drawSpace(1);
            }
        }

        private void drawErrorComponentSettings()
        {
            if (drawComponentCheckBox("Error", EM_QSetting.ErrorShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowIconOnParent);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowForDisabledComponents);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowForDisabledGameObjects);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowScriptIsMissing);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowReferenceIsMissing);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowReferenceIsNull);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowStringIsEmpty);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowMissingEventMethod);
                    QSettings.getInstance().restore(EM_QSetting.ErrorShowWhenTagOrLayerIsUndefined);
                    QSettings.getInstance().restore(EM_QSetting.ErrorIgnoreString);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 12 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.ErrorShowDuringPlayMode);
                drawCheckBoxRight("Show error icon up of hierarchy (very slow)", EM_QSetting.ErrorShowIconOnParent);
                drawCheckBoxRight("Show error icon for disabled components", EM_QSetting.ErrorShowForDisabledComponents);
                drawCheckBoxRight("Show error icon for disabled GameObjects", EM_QSetting.ErrorShowForDisabledGameObjects);
                drawLabel("Show error icon for the following:");
                indentLevel += 16;
                drawCheckBoxRight("- script is missing", EM_QSetting.ErrorShowScriptIsMissing);
                drawCheckBoxRight("- reference is missing", EM_QSetting.ErrorShowReferenceIsMissing);
                drawCheckBoxRight("- reference is null", EM_QSetting.ErrorShowReferenceIsNull);
                drawCheckBoxRight("- string is empty", EM_QSetting.ErrorShowStringIsEmpty);
                drawCheckBoxRight("- callback of event is missing (very slow)", EM_QSetting.ErrorShowMissingEventMethod);
                drawCheckBoxRight("- tag or layer is undefined", EM_QSetting.ErrorShowWhenTagOrLayerIsUndefined);
                indentLevel -= 16;
                drawTextField("Ignore packages/classes", EM_QSetting.ErrorIgnoreString);
                drawSpace(1);
            }
        }

        private void drawRendererComponentSettings()
        {
            if (drawComponentCheckBox("Renderer", EM_QSetting.RendererShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.RendererShowDuringPlayMode);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.RendererShowDuringPlayMode);
                drawSpace(1);
            }
        }

        private void drawPrefabComponentSettings()
        {
            if (drawComponentCheckBox("Prefab", EM_QSetting.PrefabShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.PrefabShowBrakedPrefabsOnly);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show icon for broken prefabs only", EM_QSetting.PrefabShowBrakedPrefabsOnly);
                drawSpace(1);
            }
        }

        private void drawTagLayerComponentSettings()
        {
            if (drawComponentCheckBox("Tag And Layer", EM_QSetting.TagAndLayerShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerSizeShowType);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerType);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerSizeValueType);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerSizeValuePixel);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerSizeValuePercent);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerAlignment);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerLabelSize);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerLabelAlpha);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerTagLabelColor);
                    QSettings.getInstance().restore(EM_QSetting.TagAndLayerLayerLabelColor);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 10 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.TagAndLayerShowDuringPlayMode);
                drawEnum("Show", EM_QSetting.TagAndLayerSizeShowType, typeof(QHierarchyTagAndLayerShowType));
                drawEnum("Show tag and layer", EM_QSetting.TagAndLayerType, typeof(QHierarchyTagAndLayerType));

                QHierarchyTagAndLayerSizeType newTagAndLayerSizeValueType = (QHierarchyTagAndLayerSizeType) drawEnum("Unit of width", EM_QSetting.TagAndLayerSizeValueType, typeof(QHierarchyTagAndLayerSizeType));

                if (newTagAndLayerSizeValueType == QHierarchyTagAndLayerSizeType.Pixel)
                    drawIntSlider("Width in pixels", EM_QSetting.TagAndLayerSizeValuePixel, 5, 250);
                else
                    drawFloatSlider("Percentage width", EM_QSetting.TagAndLayerSizeValuePercent, 0, 0.5f);

                drawEnum("Alignment", EM_QSetting.TagAndLayerAlignment, typeof(QHierarchyTagAndLayerAligment));
                drawEnum("Label size", EM_QSetting.TagAndLayerLabelSize, typeof(QHierarchyTagAndLayerLabelSize));
                drawFloatSlider("Label alpha if default", EM_QSetting.TagAndLayerLabelAlpha, 0, 1.0f);
                drawColorPicker("Tag label color", EM_QSetting.TagAndLayerTagLabelColor);
                drawColorPicker("Layer label color", EM_QSetting.TagAndLayerLayerLabelColor);
                drawSpace(1);
            }
        }

        private void drawColorComponentSettings()
        {
            if (drawComponentCheckBox("Color", EM_QSetting.ColorShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.ColorShowDuringPlayMode);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.ColorShowDuringPlayMode);
                drawSpace(1);
            }
        }

        private void drawGameObjectIconComponentSettings()
        {
            if (drawComponentCheckBox("GameObject Icon", EM_QSetting.GameObjectIconShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.GameObjectIconShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.GameObjectIconSize);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 2 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.GameObjectIconShowDuringPlayMode);
                drawEnum("Icon size", EM_QSetting.GameObjectIconSize, typeof(QHierarchySizeAll));
                drawSpace(1);
            }
        }

        private void drawTagIconComponentSettings()
        {
            if (drawComponentCheckBox("Tag Icon", EM_QSetting.TagIconShow))
            {
                string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

                bool showTagIconList = QSettings.getInstance().get<bool>(EM_QSetting.TagIconListFoldout);

                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.TagIconShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.TagIconSize);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 3 + (showTagIconList ? 18 * tags.Length : 0) + 4 + 5);

                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.TagIconShowDuringPlayMode);
                drawEnum("Icon size", EM_QSetting.TagIconSize, typeof(QHierarchySizeAll));
                if (drawFoldout("Tag icon list", EM_QSetting.TagIconListFoldout))
                {
                    List<QTagTexture> tagTextureList = QTagTexture.loadTagTextureList();

                    bool changed = false;
                    for (int i = 0; i < tags.Length; i++)
                    {
                        string tag = tags[i];
                        QTagTexture tagTexture = tagTextureList.Find(t => t.tag == tag);
                        Texture2D newTexture = (Texture2D) EditorGUI.ObjectField(GetNewRect(0, 16, 34 + 16, 6), tag, tagTexture == null ? null : tagTexture.texture, typeof(Texture2D), false);
                        if (newTexture != null && tagTexture == null)
                        {
                            QTagTexture newTagTexture = new QTagTexture(tag, newTexture);
                            tagTextureList.Add(newTagTexture);

                            changed = true;
                        }
                        else if (newTexture == null && tagTexture != null)
                        {
                            tagTextureList.Remove(tagTexture);
                            changed = true;
                        }
                        else if (tagTexture != null && tagTexture.texture != newTexture)
                        {
                            tagTexture.texture = newTexture;
                            changed = true;
                        }

                        drawSpace(i == tags.Length - 1 ? 2 : 2);
                    }

                    if (changed)
                    {
                        QTagTexture.saveTagTextureList(EM_QSetting.TagIconList, tagTextureList);
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }

                drawSpace(1);
            }
        }

        private void drawLayerIconComponentSettings()
        {
            if (drawComponentCheckBox("Layer Icon", EM_QSetting.LayerIconShow))
            {
                string[] layers = UnityEditorInternal.InternalEditorUtility.layers;

                bool showLayerIconList = QSettings.getInstance().get<bool>(EM_QSetting.LayerIconListFoldout);

                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.LayerIconShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.LayerIconSize);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 3 + (showLayerIconList ? 18 * layers.Length : 0) + 4 + 5);

                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.LayerIconShowDuringPlayMode);
                drawEnum("Icon size", EM_QSetting.LayerIconSize, typeof(QHierarchySizeAll));
                if (drawFoldout("Layer icon list", EM_QSetting.LayerIconListFoldout))
                {
                    List<QLayerTexture> layerTextureList = QLayerTexture.loadLayerTextureList();

                    bool changed = false;
                    for (int i = 0; i < layers.Length; i++)
                    {
                        string layer = layers[i];
                        QLayerTexture layerTexture = layerTextureList.Find(t => t.layer == layer);
                        Texture2D newTexture = (Texture2D) EditorGUI.ObjectField(GetNewRect(0, 16, 34 + 16, 6), layer, layerTexture == null ? null : layerTexture.texture, typeof(Texture2D), false);
                        if (newTexture != null && layerTexture == null)
                        {
                            QLayerTexture newLayerTexture = new QLayerTexture(layer, newTexture);
                            layerTextureList.Add(newLayerTexture);

                            changed = true;
                        }
                        else if (newTexture == null && layerTexture != null)
                        {
                            layerTextureList.Remove(layerTexture);
                            changed = true;
                        }
                        else if (layerTexture != null && layerTexture.texture != newTexture)
                        {
                            layerTexture.texture = newTexture;
                            changed = true;
                        }

                        drawSpace(i == layers.Length - 1 ? 2 : 2);
                    }

                    if (changed)
                    {
                        QLayerTexture.saveLayerTextureList(EM_QSetting.LayerIconList, layerTextureList);
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }

                drawSpace(1);
            }
        }

        private void drawChildrenCountComponentSettings()
        {
            if (drawComponentCheckBox("Children Count", EM_QSetting.ChildrenCountShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.ChildrenCountShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.ChildrenCountLabelSize);
                    QSettings.getInstance().restore(EM_QSetting.ChildrenCountLabelColor);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 3 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.ChildrenCountShowDuringPlayMode);
                drawEnum("Label size", EM_QSetting.ChildrenCountLabelSize, typeof(QHierarchySize));
                drawColorPicker("Label color", EM_QSetting.ChildrenCountLabelColor);
                drawSpace(1);
            }
        }

        private void drawVerticesAndTrianglesCountComponentSettings()
        {
            if (drawComponentCheckBox("Vertices And Triangles Count", EM_QSetting.VerticesAndTrianglesShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesShowVertices);
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesShowTriangles);
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesCalculateTotalCount);
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesLabelSize);
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesVerticesLabelColor);
                    QSettings.getInstance().restore(EM_QSetting.VerticesAndTrianglesTrianglesLabelColor);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 7 + 5);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.VerticesAndTrianglesShowDuringPlayMode);
                if (drawCheckBoxRight("Show vertices count", EM_QSetting.VerticesAndTrianglesShowVertices))
                {
                    if (QSettings.getInstance().get<bool>(EM_QSetting.VerticesAndTrianglesShowVertices) == false && QSettings.getInstance().get<bool>(EM_QSetting.VerticesAndTrianglesShowTriangles) == false)
                        QSettings.getInstance().set(EM_QSetting.VerticesAndTrianglesShowTriangles, true);
                }

                if (drawCheckBoxRight("Show triangles count (very slow)", EM_QSetting.VerticesAndTrianglesShowTriangles))
                {
                    if (QSettings.getInstance().get<bool>(EM_QSetting.VerticesAndTrianglesShowVertices) == false && QSettings.getInstance().get<bool>(EM_QSetting.VerticesAndTrianglesShowTriangles) == false)
                        QSettings.getInstance().set(EM_QSetting.VerticesAndTrianglesShowVertices, true);
                }

                drawCheckBoxRight("Calculate the count including children (very slow)", EM_QSetting.VerticesAndTrianglesCalculateTotalCount);
                drawEnum("Label size", EM_QSetting.VerticesAndTrianglesLabelSize, typeof(QHierarchySize));
                drawColorPicker("Vertices label color", EM_QSetting.VerticesAndTrianglesVerticesLabelColor);
                drawColorPicker("Triangles label color", EM_QSetting.VerticesAndTrianglesTrianglesLabelColor);
                drawSpace(1);
            }
        }

        private void drawComponentsComponentSettings()
        {
            if (drawComponentCheckBox("Components", EM_QSetting.ComponentsShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (drawRestore())
                {
                    QSettings.getInstance().restore(EM_QSetting.ComponentsShowDuringPlayMode);
                    QSettings.getInstance().restore(EM_QSetting.ComponentsIconSize);
                }

                drawBackground(rect.x, rect.y, rect.width, 18 * 3 + 6);
                drawSpace(4);
                drawCheckBoxRight("Show component during play mode", EM_QSetting.ComponentsShowDuringPlayMode);
                drawEnum("Icon size", EM_QSetting.ComponentsIconSize, typeof(QHierarchySizeAll));
                drawTextField("Ignore packages/classes", EM_QSetting.ComponentsIgnore);
                drawSpace(2);
            }
        }

        // COMPONENTS ORDER
        private void drawOrderSettings()
        {
            if (drawRestore())
            {
                QSettings.getInstance().restore(EM_QSetting.ComponentsOrder);
            }

            indentLevel += 4;

            string componentOrder = QSettings.getInstance().get<string>(EM_QSetting.ComponentsOrder);
            string[] componentIds = componentOrder.Split(';');

            Rect rect = GetNewRect(position.width, 17 * componentIds.Length + 10, 0, 0);
            if (componentsOrderList == null)
                componentsOrderList = new QComponentsOrderList(this);
            componentsOrderList.draw(rect, componentIds);

            indentLevel -= 4;
        }

        // ADDITIONAL SETTINGS
        private void drawAdditionalSettings()
        {
            if (drawRestore())
            {
                QSettings.getInstance().restore(EM_QSetting.AdditionalShowHiddenQHierarchyObjectList);
                QSettings.getInstance().restore(EM_QSetting.AdditionalHideIconsIfNotFit);
                QSettings.getInstance().restore(EM_QSetting.AdditionalIndentation);
                QSettings.getInstance().restore(EM_QSetting.AdditionalShowModifierWarning);
                QSettings.getInstance().restore(EM_QSetting.AdditionalBackgroundColor);
                QSettings.getInstance().restore(EM_QSetting.AdditionalActiveColor);
                QSettings.getInstance().restore(EM_QSetting.AdditionalInactiveColor);
                QSettings.getInstance().restore(EM_QSetting.AdditionalSpecialColor);
            }

            drawSpace(4);
            drawCheckBoxRight("Show QHierarchyObjectList GameObject", EM_QSetting.AdditionalShowHiddenQHierarchyObjectList);
            drawCheckBoxRight("Hide icons if not fit", EM_QSetting.AdditionalHideIconsIfNotFit);
            drawIntSlider("Right indent", EM_QSetting.AdditionalIndentation, 0, 500);
            drawCheckBoxRight("Show warning when using modifiers + click", EM_QSetting.AdditionalShowModifierWarning);
            drawColorPicker("Background color", EM_QSetting.AdditionalBackgroundColor);
            drawColorPicker("Active color", EM_QSetting.AdditionalActiveColor);
            drawColorPicker("Inactive color", EM_QSetting.AdditionalInactiveColor);
            drawColorPicker("Special color", EM_QSetting.AdditionalSpecialColor);
            drawSpace(1);
        }

        // PRIVATE


        /// <summary>
        /// 绘制分隔符
        /// </summary>
        private void DrawSeparator(int spaceBefore = 0, int spaceAfter = 0, int height = 1)
        {
            if (spaceBefore > 0)
            {
                drawSpace(spaceBefore);
            }
            
            var rect = GetNewRect(0, height);
            rect.width += 8;
            EditorGUI.DrawRect(rect, separatorColor);
            
            if (spaceAfter > 0)
            {
                drawSpace(spaceAfter);
            }
        }

        private bool drawComponentCheckBox(string label, EM_QSetting setting)
        {
            indentLevel += 8;

            Rect rect = GetNewRect(0, 28, 0, 0);

            float rectWidth = rect.width;
            bool isChecked = QSettings.getInstance().get<bool>(setting);

            rect.x -= 1;
            rect.y += 7;
            rect.width = 14;
            rect.height = 14;

            if (UnityEngine.GUI.Button(rect, isChecked ? checkBoxChecked : checkBoxUnchecked, GUIStyle.none))
            {
                QSettings.getInstance().set(setting, !isChecked);
            }

            rect.x += 14 + 10;
            rect.width = rectWidth - 14 - 8;
            rect.y -= (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
            rect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(rect, label);

            indentLevel -= 8;

            return isChecked;
        }

        private bool drawCheckBoxRight(string label, EM_QSetting setting)
        {
            Rect rect = GetNewRect(0, 18, 34, 6);
            bool result = false;
            bool isChecked = QSettings.getInstance().get<bool>(setting);

            float tempX = rect.x;
            rect.x += rect.width - 14;
            rect.y += 1;
            rect.width = 14;
            rect.height = 14;

            if (UnityEngine.GUI.Button(rect, isChecked ? checkBoxChecked : checkBoxUnchecked, GUIStyle.none))
            {
                QSettings.getInstance().set(setting, !isChecked);
                result = true;
            }

            rect.width = rect.x - tempX - 4;
            rect.x = tempX;
            rect.y -= (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
            rect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(rect, label);

            return result;
        }

        private void drawSpace(int value)
        {
            GetNewRect(0, value, 0, 0);
        }

        private void drawBackground(float x, float y, float width, float height)
        {
            EditorGUI.DrawRect(new Rect(x, y, width, height), separatorColor);
        }


        private bool drawRestore()
        {
            if (UnityEngine.GUI.Button(new Rect(lastRect.x + lastRect.width - 16 - 5, lastRect.y - 20, 16, 16), restoreButtonTexture, GUIStyle.none))
            {
                if (EditorUtility.DisplayDialog("Restore", "Restore default settings?", "Ok", "Cancel"))
                {
                    return true;
                }
            }

            return false;
        }

        // GUI COMPONENTS
        private void drawLabel(string label)
        {
            Rect rect = GetNewRect(0, 16, 34, 6);
            rect.y -= (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
            EditorGUI.LabelField(rect, label);
            drawSpace(2);
        }

        private void drawTextField(string label, EM_QSetting setting)
        {
            string currentValue = QSettings.getInstance().get<string>(setting);
            string newValue = EditorGUI.TextField(GetNewRect(0, 16, 34, 6), label, currentValue);
            if (!currentValue.Equals(newValue))
                QSettings.getInstance().set(setting, newValue);
            drawSpace(2);
        }

        private bool drawFoldout(string label, EM_QSetting setting)
        {
#if UNITY_2019_1_OR_NEWER
            Rect foldoutRect = GetNewRect(0, 16, 19, 6);
#else
                Rect foldoutRect = getControlRect(0, 16, 22, 6);
#endif
            bool foldoutValue = QSettings.getInstance().get<bool>(setting);
            bool newFoldoutValue = EditorGUI.Foldout(foldoutRect, foldoutValue, label);
            if (foldoutValue != newFoldoutValue)
                QSettings.getInstance().set(setting, newFoldoutValue);
            drawSpace(2);
            return newFoldoutValue;
        }

        private void drawColorPicker(string label, EM_QSetting setting)
        {
            Color currentColor = QSettings.getInstance().getColor(setting);
            Color newColor = EditorGUI.ColorField(GetNewRect(0, 16, 34, 6), label, currentColor);
            if (!currentColor.Equals(newColor))
                QSettings.getInstance().setColor(setting, newColor);
            drawSpace(2);
        }

        private Enum drawEnum(string label, EM_QSetting setting, Type enumType)
        {
            Enum currentEnum = (Enum) Enum.ToObject(enumType, QSettings.getInstance().get<int>(setting));
            Enum newEnumValue;
            if (!(newEnumValue = EditorGUI.EnumPopup(GetNewRect(0, 16, 34, 6), label, currentEnum)).Equals(currentEnum))
                QSettings.getInstance().set(setting, (int) (object) newEnumValue);
            drawSpace(2);
            return newEnumValue;
        }

        private void drawIntSlider(string label, EM_QSetting setting, int minValue, int maxValue)
        {
            Rect rect = GetNewRect(0, 16, 34, 4);
            int currentValue = QSettings.getInstance().get<int>(setting);
            int newValue = EditorGUI.IntSlider(rect, label, currentValue, minValue, maxValue);
            if (currentValue != newValue)
                QSettings.getInstance().set(setting, newValue);
            drawSpace(2);
        }

        private void drawFloatSlider(string label, EM_QSetting setting, float minValue, float maxValue)
        {
            Rect rect = GetNewRect(0, 16, 34, 4);
            float currentValue = QSettings.getInstance().get<float>(setting);
            float newValue = EditorGUI.Slider(rect, label, currentValue, minValue, maxValue);
            if (currentValue != newValue)
                QSettings.getInstance().set(setting, newValue);
            drawSpace(2);
        }
    }
}
