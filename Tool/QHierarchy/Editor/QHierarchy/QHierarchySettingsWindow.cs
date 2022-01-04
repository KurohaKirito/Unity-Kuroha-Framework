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
        /// <summary>
        /// 初始化标志
        /// </summary>
        private bool initFlag;
        
        /// <summary>
        /// 是否为黑色风格
        /// </summary>
        private bool isProSkin;

        /// <summary>
        /// 折叠框
        /// </summary>
        private bool isOpenComponentsSettings;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private bool isOpenComponentsOrders;
        
        /// <summary>
        /// 折叠框
        /// </summary>
        private bool isOpenAdditionalSettings;
        
        /// <summary>
        /// 缩进像素数
        /// </summary>
        private int indentLevel;

        /// <summary>
        /// 窗口可绘制界面区域的总宽度
        /// 使用 window.position.width 获取的宽度无法自动适配右侧可能会出现的滑动条, 因此手动控制总宽度
        /// </summary>
        private float paintWidth;
        
        /// <summary>
        /// 记录上次绘制使用的 Rect
        /// </summary>
        private Rect lastRect;

        /// <summary>
        /// 当前可绘制区域的 Y 值
        /// </summary>
        private float currentRectY;
        
        /// <summary>
        /// 用于滑动条计算
        /// </summary>
        private Vector2 scrollPosition;
        
        /// <summary>
        /// 窗口
        /// </summary>
        private static QHierarchySettingsWindow window;

        /// <summary>
        /// 菜单按钮颜色
        /// </summary>
        private Color menuButtonColor;
        
        /// <summary>
        /// 黄色区域的颜色
        /// </summary>
        private Color yellowColor;
        
        /// <summary>
        /// 分隔器的颜色
        /// </summary>
        private Color separatorColor;

        private Texture2D checkBoxChecked;
        private Texture2D checkBoxUnchecked;
        private Texture2D restoreButtonTexture;

        private QComponentsOrderList componentsOrderList;

        /// <summary>
        /// 打开窗口
        /// </summary>
        [MenuItem("Tools/QHierarchyMain/Settings")]
        public static void Open()
        {
            window = GetWindow<QHierarchySettingsWindow>("QHierarchyMain");
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
                yellowColor = isProSkin ? new Color(1.00f, 0.90f, 0.40f) : new Color(0.31f, 0.31f, 0.31f);
                separatorColor = isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.59f, 0.59f, 0.59f);
                menuButtonColor = isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.9f, 0.9f, 0.9f);

                checkBoxChecked = QResources.Instance().GetTexture(QTexture.QCheckBoxChecked);
                checkBoxUnchecked = QResources.Instance().GetTexture(QTexture.QCheckBoxUnchecked);
                restoreButtonTexture = QResources.Instance().GetTexture(QTexture.QRestoreButton);
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
                // 计算前景区域的宽度
                // EditorGUILayout.GetControlRect 获取到的高度默认是 EditorGUIUtility.singleLineHeight, 即 18
                // 获取的宽度在 Layout 状态下为 1, 在 Repaint 状态下为当前坐标尽可能延展的宽度 - 6 像素的留白
                // 这里不需要告诉 Layout 高度, 所以传 0, 如果使用默认值会导致使用了 Layout 系统的滑动条的范围高度多出部分像素
                paintWidth = EditorGUILayout.GetControlRect(GUILayout.Height(0)).width;
                paintWidth += 6;

                // 绘制菜单框 COMPONENTS SETTINGS
                DrawMenuBox("COMPONENTS SETTINGS", ref isOpenComponentsSettings);
                currentRectY = lastRect.y + lastRect.height;
                if (isOpenComponentsSettings)
                {
                    DrawSettingsHierarchyTree();
                    DrawSeparator();
                    DrawSettingsMonoBehaviourIcon();
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
                    DrawLine(currentRectY, lastRect.y + lastRect.height, separatorColor);
                }

                // 开始绘制
                DrawMenuBox("ORDER OF COMPONENTS", ref isOpenComponentsOrders);
                currentRectY = lastRect.y + lastRect.height;
                if (isOpenComponentsOrders)
                {
                    DrawSpace(6);
                    drawOrderSettings();
                    DrawSpace(6);
                    DrawLine(currentRectY, lastRect.y + lastRect.height, separatorColor);
                }

                // 开始绘制
                DrawMenuBox("ADDITIONAL SETTINGS", ref isOpenAdditionalSettings);
                currentRectY = lastRect.y + lastRect.height;
                if (isOpenAdditionalSettings)
                {
                    DrawSpace(3);
                    drawAdditionalSettings();
                    DrawLine(currentRectY, lastRect.y + lastRect.height + 4, separatorColor);
                }

                indentLevel -= 1;
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 获得一个新的 UI 绘制区域
        /// 1. 自动通知 Layout 系统的组件
        /// 2. 自动更新 lastRect
        /// </summary>
        /// <param name="width">区域的宽度</param>
        /// <param name="rectHeight">区域的高度</param>
        /// <param name="leftIndent">左侧缩进</param>
        /// <param name="rightIndent">右侧缩进</param>
        /// <returns></returns>
        private Rect GetNewRect(float width, float rectHeight, float leftIndent = 0, float rightIndent = 0)
        {
            // 为了使 GUILayout 的滑动条检测正确生效, 必须有这一句
            EditorGUILayout.GetControlRect(false, rectHeight, GUIStyle.none, GUILayout.ExpandWidth(true));
            
            // Rect Width
            var rectWidth = width == 0 ? paintWidth - indentLevel - leftIndent - rightIndent : width;
            
            // Position X
            var positionX = indentLevel + leftIndent;
            
            // Position Y
            var positionY = lastRect.y + lastRect.height;
            
            var rect = new Rect(positionX, positionY, rectWidth, rectHeight);
            
            // Update lastRect
            lastRect = rect;
            
            return rect;
        }
        
        /// <summary>
        /// 绘制菜单
        /// </summary>
        private void DrawMenuBox(string sectionTitle, ref bool buttonFlag)
        {
            // 定义数据
            const float ICON_WIDTH = 28;
            const float MENU_HEIGHT = 24;
            const float MENU_WIDTH = 240;
            
            // 告诉 Layout 要绘制一个默认宽度, 指定高度的区域
            // 默认宽度就是可绘制区域宽度 (比窗口宽度小 24 像素)
            var rect = GetNewRect(0, MENU_HEIGHT);
            
            // 覆盖右侧留白
            rect.width *= 2;
            
            // 绘制底色 Box (最左侧绘制)
            rect.x = 0;
            UnityEngine.GUI.Box(rect, string.Empty);

            // 绘制左侧竖线
            DrawLine(rect.y, rect.y + MENU_HEIGHT, yellowColor, indentLevel);
            rect.x += indentLevel;

            // 绘制折叠图标
            var oldColor = UnityEngine.GUI.backgroundColor;
            UnityEngine.GUI.backgroundColor = menuButtonColor;
            var oldAlignment = UnityEngine.GUI.skin.button.alignment;
            UnityEngine.GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            var autoCheckIcon = EditorGUIUtility.IconContent(buttonFlag ? "sv_icon_dot11_pix16_gizmo" : "sv_icon_dot8_pix16_gizmo");
            if (UnityEngine.GUI.Button(new Rect(rect.x, rect.y, MENU_WIDTH, rect.height), autoCheckIcon))
            {
                buttonFlag = !buttonFlag;
            }
            UnityEngine.GUI.backgroundColor = oldColor;
            UnityEngine.GUI.skin.button.alignment = oldAlignment;
            rect.x += ICON_WIDTH;

            // 绘制设置图标
            EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("GameManager Icon"));
            rect.x += ICON_WIDTH;

            // 绘制名称
            EditorGUI.LabelField(rect, sectionTitle);
        }
        
        /// <summary>
        /// 绘制 HierarchyTree
        /// </summary>
        private void DrawSettingsHierarchyTree()
        {
            const int UP_DOWN_SPACE = 4;
            const int ITEM_SETTING_HEIGHT = 18;
            
            if (DrawCheckBox("Hierarchy Tree", "在 Hierarchy 面板的左侧绘制树形结构\r\n体现游戏物体的父子级关系", EM_QSetting.TreeMapShow))
            {
                if (DrawRestore(24))
                {
                    QSettings.Instance().Restore(EM_QSetting.TreeMapColor);
                    QSettings.Instance().Restore(EM_QSetting.TreeMapEnhanced);
                    QSettings.Instance().Restore(EM_QSetting.TreeMapTransparentBackground);
                }
                
                var rect = GetNewRect(0, 0);
                
                // 绘制背景色
                DrawBackground(rect.x, rect.y, rect.width, ITEM_SETTING_HEIGHT * 3 + UP_DOWN_SPACE * 2);
                
                // 绘制空白
                DrawSpace(UP_DOWN_SPACE);
                
                DrawColorPicker("树形结构的颜色", EM_QSetting.TreeMapColor);
                DrawCheckBoxRight("透明背景", EM_QSetting.TreeMapTransparentBackground);
                DrawCheckBoxRight("简洁模式", EM_QSetting.TreeMapEnhanced);
                
                // 绘制空白
                DrawSpace(UP_DOWN_SPACE);
            }
        }

        /// <summary>
        /// 绘制 MonoBehaviourIcon
        /// </summary>
        private void DrawSettingsMonoBehaviourIcon()
        {
            if (DrawCheckBox("Highlight MonoBehaviour", "高亮 MonoBehaviour", EM_QSetting.MonoBehaviourIconShow))
            {
                var rect = GetNewRect(0, 0);
                
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.MonoBehaviourIconShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.MonoBehaviourIconColor);
                    QSettings.Instance().Restore(EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 3 + 8);
                
                DrawSpace(4);
                
                DrawColorPicker("Icon color", EM_QSetting.MonoBehaviourIconColor);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.MonoBehaviourIconShowDuringPlayMode);
                DrawCheckBoxRight("Ignore Unity MonoBehaviours", EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour);
                
                DrawSpace(4);
            }
        }

        private void drawSeparatorComponentSettings()
        {
            if (DrawCheckBox("Separator", "", EM_QSetting.SeparatorShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.SeparatorColor);
                    QSettings.Instance().Restore(EM_QSetting.SeparatorShowRowShading);
                    QSettings.Instance().Restore(EM_QSetting.SeparatorOddRowShadingColor);
                    QSettings.Instance().Restore(EM_QSetting.SeparatorEvenRowShadingColor);
                }

                bool rowShading = QSettings.Instance().Get<bool>(EM_QSetting.SeparatorShowRowShading);

                DrawBackground(rect.x, rect.y, rect.width, 18 * (rowShading ? 4 : 2) + 5);
                DrawSpace(4);
                DrawColorPicker("Separator Color", EM_QSetting.SeparatorColor);
                DrawCheckBoxRight("Row shading", EM_QSetting.SeparatorShowRowShading);
                if (rowShading)
                {
                    DrawColorPicker("Even row shading color", EM_QSetting.SeparatorEvenRowShadingColor);
                    DrawColorPicker("Odd row shading color", EM_QSetting.SeparatorOddRowShadingColor);
                }

                DrawSpace(1);
            }
        }

        private void drawVisibilityComponentSettings()
        {
            if (DrawCheckBox("Visibility", "", EM_QSetting.VisibilityShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.VisibilityShowDuringPlayMode);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.VisibilityShowDuringPlayMode);
                DrawSpace(1);
            }
        }

        private void drawLockComponentSettings()
        {
            if (DrawCheckBox("Lock", "", EM_QSetting.LockShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.LockShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.LockPreventSelectionOfLockedObjects);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 2 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.LockShowDuringPlayMode);
                DrawCheckBoxRight("Prevent selection of locked objects", EM_QSetting.LockPreventSelectionOfLockedObjects);
                DrawSpace(1);
            }
        }

        private void drawStaticComponentSettings()
        {
            if (DrawCheckBox("Static", "", EM_QSetting.StaticShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.StaticShowDuringPlayMode);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.StaticShowDuringPlayMode);
                DrawSpace(1);
            }
        }

        private void drawErrorComponentSettings()
        {
            if (DrawCheckBox("Error", "", EM_QSetting.ErrorShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowIconOnParent);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowForDisabledComponents);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowForDisabledGameObjects);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowScriptIsMissing);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowReferenceIsMissing);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowReferenceIsNull);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowStringIsEmpty);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowMissingEventMethod);
                    QSettings.Instance().Restore(EM_QSetting.ErrorShowWhenTagOrLayerIsUndefined);
                    QSettings.Instance().Restore(EM_QSetting.ErrorIgnoreString);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 12 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.ErrorShowDuringPlayMode);
                DrawCheckBoxRight("Show error icon up of hierarchy (very slow)", EM_QSetting.ErrorShowIconOnParent);
                DrawCheckBoxRight("Show error icon for disabled components", EM_QSetting.ErrorShowForDisabledComponents);
                DrawCheckBoxRight("Show error icon for disabled GameObjects", EM_QSetting.ErrorShowForDisabledGameObjects);
                drawLabel("Show error icon for the following:");
                indentLevel += 16;
                DrawCheckBoxRight("- script is missing", EM_QSetting.ErrorShowScriptIsMissing);
                DrawCheckBoxRight("- reference is missing", EM_QSetting.ErrorShowReferenceIsMissing);
                DrawCheckBoxRight("- reference is null", EM_QSetting.ErrorShowReferenceIsNull);
                DrawCheckBoxRight("- string is empty", EM_QSetting.ErrorShowStringIsEmpty);
                DrawCheckBoxRight("- callback of event is missing (very slow)", EM_QSetting.ErrorShowMissingEventMethod);
                DrawCheckBoxRight("- tag or layer is undefined", EM_QSetting.ErrorShowWhenTagOrLayerIsUndefined);
                indentLevel -= 16;
                drawTextField("Ignore packages/classes", EM_QSetting.ErrorIgnoreString);
                DrawSpace(1);
            }
        }

        private void drawRendererComponentSettings()
        {
            if (DrawCheckBox("Renderer", "", EM_QSetting.RendererShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.RendererShowDuringPlayMode);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.RendererShowDuringPlayMode);
                DrawSpace(1);
            }
        }

        private void drawPrefabComponentSettings()
        {
            if (DrawCheckBox("Prefab", "", EM_QSetting.PrefabShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.PrefabShowBrakedPrefabsOnly);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show icon for broken prefabs only", EM_QSetting.PrefabShowBrakedPrefabsOnly);
                DrawSpace(1);
            }
        }

        private void drawTagLayerComponentSettings()
        {
            if (DrawCheckBox("Tag And Layer", "", EM_QSetting.TagAndLayerShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerSizeShowType);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerType);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerSizeValueType);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerSizeValuePixel);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerSizeValuePercent);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerAlignment);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerLabelSize);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerLabelAlpha);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerTagLabelColor);
                    QSettings.Instance().Restore(EM_QSetting.TagAndLayerLayerLabelColor);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 10 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.TagAndLayerShowDuringPlayMode);
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
                DrawColorPicker("Tag label color", EM_QSetting.TagAndLayerTagLabelColor);
                DrawColorPicker("Layer label color", EM_QSetting.TagAndLayerLayerLabelColor);
                DrawSpace(1);
            }
        }

        private void drawColorComponentSettings()
        {
            if (DrawCheckBox("Color", "", EM_QSetting.ColorShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.ColorShowDuringPlayMode);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.ColorShowDuringPlayMode);
                DrawSpace(1);
            }
        }

        private void drawGameObjectIconComponentSettings()
        {
            if (DrawCheckBox("GameObject Icon", "", EM_QSetting.GameObjectIconShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.GameObjectIconShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.GameObjectIconSize);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 2 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.GameObjectIconShowDuringPlayMode);
                drawEnum("Icon size", EM_QSetting.GameObjectIconSize, typeof(QHierarchySizeAll));
                DrawSpace(1);
            }
        }

        private void drawTagIconComponentSettings()
        {
            if (DrawCheckBox("Tag Icon", "", EM_QSetting.TagIconShow))
            {
                string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

                bool showTagIconList = QSettings.Instance().Get<bool>(EM_QSetting.TagIconListFoldout);

                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.TagIconShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.TagIconSize);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 3 + (showTagIconList ? 18 * tags.Length : 0) + 4 + 5);

                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.TagIconShowDuringPlayMode);
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

                        DrawSpace(i == tags.Length - 1 ? 2 : 2);
                    }

                    if (changed)
                    {
                        QTagTexture.saveTagTextureList(EM_QSetting.TagIconList, tagTextureList);
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }

                DrawSpace(1);
            }
        }

        private void drawLayerIconComponentSettings()
        {
            if (DrawCheckBox("Layer Icon", "", EM_QSetting.LayerIconShow))
            {
                string[] layers = UnityEditorInternal.InternalEditorUtility.layers;

                bool showLayerIconList = QSettings.Instance().Get<bool>(EM_QSetting.LayerIconListFoldout);

                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.LayerIconShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.LayerIconSize);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 3 + (showLayerIconList ? 18 * layers.Length : 0) + 4 + 5);

                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.LayerIconShowDuringPlayMode);
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

                        DrawSpace(i == layers.Length - 1 ? 2 : 2);
                    }

                    if (changed)
                    {
                        QLayerTexture.saveLayerTextureList(EM_QSetting.LayerIconList, layerTextureList);
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }

                DrawSpace(1);
            }
        }

        private void drawChildrenCountComponentSettings()
        {
            if (DrawCheckBox("Children Count", "", EM_QSetting.ChildrenCountShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.ChildrenCountShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.ChildrenCountLabelSize);
                    QSettings.Instance().Restore(EM_QSetting.ChildrenCountLabelColor);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 3 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.ChildrenCountShowDuringPlayMode);
                drawEnum("Label size", EM_QSetting.ChildrenCountLabelSize, typeof(QHierarchySize));
                DrawColorPicker("Label color", EM_QSetting.ChildrenCountLabelColor);
                DrawSpace(1);
            }
        }

        private void drawVerticesAndTrianglesCountComponentSettings()
        {
            if (DrawCheckBox("Vertices And Triangles Count", "", EM_QSetting.VerticesAndTrianglesShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesShowVertices);
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesShowTriangles);
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesCalculateTotalCount);
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesLabelSize);
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesVerticesLabelColor);
                    QSettings.Instance().Restore(EM_QSetting.VerticesAndTrianglesTrianglesLabelColor);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 7 + 5);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.VerticesAndTrianglesShowDuringPlayMode);
                if (DrawCheckBoxRight("Show vertices count", EM_QSetting.VerticesAndTrianglesShowVertices))
                {
                    if (QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowVertices) == false && QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowTriangles) == false)
                        QSettings.Instance().Set(EM_QSetting.VerticesAndTrianglesShowTriangles, true);
                }

                if (DrawCheckBoxRight("Show triangles count (very slow)", EM_QSetting.VerticesAndTrianglesShowTriangles))
                {
                    if (QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowVertices) == false && QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowTriangles) == false)
                        QSettings.Instance().Set(EM_QSetting.VerticesAndTrianglesShowVertices, true);
                }

                DrawCheckBoxRight("Calculate the count including children (very slow)", EM_QSetting.VerticesAndTrianglesCalculateTotalCount);
                drawEnum("Label size", EM_QSetting.VerticesAndTrianglesLabelSize, typeof(QHierarchySize));
                DrawColorPicker("Vertices label color", EM_QSetting.VerticesAndTrianglesVerticesLabelColor);
                DrawColorPicker("Triangles label color", EM_QSetting.VerticesAndTrianglesTrianglesLabelColor);
                DrawSpace(1);
            }
        }

        private void drawComponentsComponentSettings()
        {
            if (DrawCheckBox("Components", "", EM_QSetting.ComponentsShow))
            {
                Rect rect = GetNewRect(0, 0);
                if (DrawRestore(28))
                {
                    QSettings.Instance().Restore(EM_QSetting.ComponentsShowDuringPlayMode);
                    QSettings.Instance().Restore(EM_QSetting.ComponentsIconSize);
                }

                DrawBackground(rect.x, rect.y, rect.width, 18 * 3 + 6);
                DrawSpace(4);
                DrawCheckBoxRight("Show component during play mode", EM_QSetting.ComponentsShowDuringPlayMode);
                drawEnum("Icon size", EM_QSetting.ComponentsIconSize, typeof(QHierarchySizeAll));
                drawTextField("Ignore packages/classes", EM_QSetting.ComponentsIgnore);
                DrawSpace(2);
            }
        }

        // COMPONENTS ORDER
        private void drawOrderSettings()
        {
            if (DrawRestore(24))
            {
                QSettings.Instance().Restore(EM_QSetting.ComponentsOrder);
            }

            indentLevel += 4;

            string componentOrder = QSettings.Instance().Get<string>(EM_QSetting.ComponentsOrder);
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
            if (DrawRestore(24))
            {
                QSettings.Instance().Restore(EM_QSetting.AdditionalShowHiddenQHierarchyObjectList);
                QSettings.Instance().Restore(EM_QSetting.AdditionalHideIconsIfNotFit);
                QSettings.Instance().Restore(EM_QSetting.AdditionalIndentation);
                QSettings.Instance().Restore(EM_QSetting.AdditionalShowModifierWarning);
                QSettings.Instance().Restore(EM_QSetting.AdditionalBackgroundColor);
                QSettings.Instance().Restore(EM_QSetting.AdditionalActiveColor);
                QSettings.Instance().Restore(EM_QSetting.AdditionalInactiveColor);
                QSettings.Instance().Restore(EM_QSetting.AdditionalSpecialColor);
            }

            DrawSpace(4);
            DrawCheckBoxRight("Show QHierarchyObjectList GameObject", EM_QSetting.AdditionalShowHiddenQHierarchyObjectList);
            DrawCheckBoxRight("Hide icons if not fit", EM_QSetting.AdditionalHideIconsIfNotFit);
            drawIntSlider("Right indent", EM_QSetting.AdditionalIndentation, 0, 500);
            DrawCheckBoxRight("Show warning when using modifiers + click", EM_QSetting.AdditionalShowModifierWarning);
            DrawColorPicker("Background color", EM_QSetting.AdditionalBackgroundColor);
            DrawColorPicker("Active color", EM_QSetting.AdditionalActiveColor);
            DrawColorPicker("Inactive color", EM_QSetting.AdditionalInactiveColor);
            DrawColorPicker("Special color", EM_QSetting.AdditionalSpecialColor);
            DrawSpace(1);
        }

        // PRIVATE

        // GUI COMPONENTS
        private void drawLabel(string label)
        {
            Rect rect = GetNewRect(0, 16, 34, 6);
            rect.y -= (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
            EditorGUI.LabelField(rect, label);
            DrawSpace(2);
        }

        private void drawTextField(string label, EM_QSetting setting)
        {
            string currentValue = QSettings.Instance().Get<string>(setting);
            string newValue = EditorGUI.TextField(GetNewRect(0, 16, 34, 6), label, currentValue);
            if (!currentValue.Equals(newValue))
                QSettings.Instance().Set(setting, newValue);
            DrawSpace(2);
        }

        private bool drawFoldout(string label, EM_QSetting setting)
        {
#if UNITY_2019_1_OR_NEWER
            Rect foldoutRect = GetNewRect(0, 16, 19, 6);
#else
                Rect foldoutRect = getControlRect(0, 16, 22, 6);
#endif
            bool foldoutValue = QSettings.Instance().Get<bool>(setting);
            bool newFoldoutValue = EditorGUI.Foldout(foldoutRect, foldoutValue, label);
            if (foldoutValue != newFoldoutValue)
                QSettings.Instance().Set(setting, newFoldoutValue);
            DrawSpace(2);
            return newFoldoutValue;
        }
        
        private Enum drawEnum(string label, EM_QSetting setting, Type enumType)
        {
            Enum currentEnum = (Enum) Enum.ToObject(enumType, QSettings.Instance().Get<int>(setting));
            Enum newEnumValue;
            if (!(newEnumValue = EditorGUI.EnumPopup(GetNewRect(0, 16, 34, 6), label, currentEnum)).Equals(currentEnum))
                QSettings.Instance().Set(setting, (int) (object) newEnumValue);
            DrawSpace(2);
            return newEnumValue;
        }

        private void drawIntSlider(string label, EM_QSetting setting, int minValue, int maxValue)
        {
            Rect rect = GetNewRect(0, 16, 34, 4);
            int currentValue = QSettings.Instance().Get<int>(setting);
            int newValue = EditorGUI.IntSlider(rect, label, currentValue, minValue, maxValue);
            if (currentValue != newValue)
                QSettings.Instance().Set(setting, newValue);
            DrawSpace(2);
        }

        private void drawFloatSlider(string label, EM_QSetting setting, float minValue, float maxValue)
        {
            Rect rect = GetNewRect(0, 16, 34, 4);
            float currentValue = QSettings.Instance().Get<float>(setting);
            float newValue = EditorGUI.Slider(rect, label, currentValue, minValue, maxValue);
            if (currentValue != newValue)
                QSettings.Instance().Set(setting, newValue);
            DrawSpace(2);
        }


        #region 界面组件

        /// <summary>
        /// 绘制线
        /// </summary>
        private void DrawLine(float fromY, float toY, Color color, float width = 0)
        {
            var lineWidth = width == 0 ? indentLevel : width;
            var lineHeight = toY - fromY;
            EditorGUI.DrawRect(new Rect(0, fromY, lineWidth, lineHeight), color);
        }
        
        /// <summary>
        /// 绘制单选框
        /// </summary>
        private bool DrawCheckBox(string labelText, string toolTip, EM_QSetting setting)
        {
            // 定义
            const int SECOND_INDENT = 10;
            const float CHECKED_ICON = 14;
            
            // 进行 2 级缩进
            indentLevel += SECOND_INDENT;

            // 通知 Layout 绘制指定高度的矩形
            var rect = GetNewRect(0, CHECKED_ICON * 2);
            
            // 缓存总宽度
            var rectTotalWidth = rect.width;
            
            // 计算单选框的绘制区域
            rect.y += CHECKED_ICON * 0.5f;
            rect.width = CHECKED_ICON;
            rect.height = CHECKED_ICON;
            var isChecked = QSettings.Instance().Get<bool>(setting);
            var icon = isChecked ? checkBoxChecked : checkBoxUnchecked;
            if (UnityEngine.GUI.Button(rect, icon, GUIStyle.none))
            {
                QSettings.Instance().Set(setting, !isChecked);
            }

            // 计算标题的绘制区域
            rect.x += CHECKED_ICON + 10;
            // Y 坐标上调 (LabelHeight / 2) - (CheckBoxHeight / 2) => (LabelHeight - CheckBoxHeight) / 2
            rect.y -= (EditorGUIUtility.singleLineHeight - rect.height) * 0.5f;
            // 确保覆盖右侧全部留白
            rect.width = rectTotalWidth * 2;
            // 默认高度: 18
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, new GUIContent(labelText, toolTip));

            // 取消 2 级缩进
            indentLevel -= SECOND_INDENT;

            return isChecked;
        }
        
        /// <summary>
        /// 绘制重置按钮
        /// </summary>
        /// <returns></returns>
        private bool DrawRestore(float restoreMenuHeight)
        {
            const float RIGHT_SPACE = 8;
            const float RESTORE_ICON_WIDTH_HEIGHT = 16;

            var positionX = lastRect.x + lastRect.width - RESTORE_ICON_WIDTH_HEIGHT - RIGHT_SPACE;
            var positionY = lastRect.y - (restoreMenuHeight + RESTORE_ICON_WIDTH_HEIGHT) * 0.5f;
            var rect = new Rect(positionX, positionY, RESTORE_ICON_WIDTH_HEIGHT, RESTORE_ICON_WIDTH_HEIGHT);
            
            if (UnityEngine.GUI.Button(rect, restoreButtonTexture, GUIStyle.none))
            {
                if (EditorUtility.DisplayDialog("Restore", "Restore default settings?", "Ok", "Cancel"))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// 绘制指定高度的空白区域
        /// </summary>
        private void DrawSpace(int height)
        {
            GetNewRect(0, height);
        }

        /// <summary>
        /// 绘制底色背景
        /// </summary>
        private void DrawBackground(float x, float y, float width, float height)
        {
            EditorGUI.DrawRect(new Rect(x, y, width, height), separatorColor);
        }
        
        /// <summary>
        /// 绘制取色器
        /// </summary>
        private void DrawColorPicker(string label, EM_QSetting setting)
        {
            const int COLOR_PICKER_HEIGHT = 16;
            const int LEFT_INDENT = 34;
            const int RIGHT_INDENT = 10;

            var newRect = GetNewRect(0, COLOR_PICKER_HEIGHT, LEFT_INDENT, RIGHT_INDENT);
            var currentColor = QSettings.Instance().GetColor(setting);
            var newColor = EditorGUI.ColorField(newRect, label, currentColor);
            if (currentColor != newColor)
            {
                QSettings.Instance().SetColor(setting, newColor);
            }
            
            DrawSpace(2);
        }
        
        /// <summary>
        /// 绘制附带 Label 的右侧单选框
        /// </summary>
        private bool DrawCheckBoxRight(string label, EM_QSetting setting)
        {
            const int CHECK_BOX_HEIGHT_WIDTH = 14;
            const int ITEM_SETTING_HEIGHT = 18;
            const int LEFT_INDENT =  10 * 2 + CHECK_BOX_HEIGHT_WIDTH;
            const int RIGHT_INDENT = 10;
            const int SPACE_LABEL_CHECKBOX = 4;
            
            var result = false;
            var isChecked = QSettings.Instance().Get<bool>(setting);
            var isCheckedIcon = isChecked? checkBoxChecked : checkBoxUnchecked;
            var rect = GetNewRect(0, ITEM_SETTING_HEIGHT, LEFT_INDENT, RIGHT_INDENT);
            
            // 绘制左侧 Label
            rect.width = rect.width - CHECK_BOX_HEIGHT_WIDTH - SPACE_LABEL_CHECKBOX;
            EditorGUI.LabelField(rect, label);
            
            // 绘制右侧单选框
            rect.x += rect.width + SPACE_LABEL_CHECKBOX;
            rect.y += 1;
            rect.width = CHECK_BOX_HEIGHT_WIDTH;
            rect.height = CHECK_BOX_HEIGHT_WIDTH;
            if (UnityEngine.GUI.Button(rect, isCheckedIcon, GUIStyle.none))
            {
                QSettings.Instance().Set(setting, !isChecked);
                result = true;
            }

            return result;
        }
        
        /// <summary>
        /// 绘制分隔符
        /// </summary>
        private void DrawSeparator(int spaceBefore = 0, int spaceAfter = 0, int height = 1)
        {
            if (spaceBefore > 0)
            {
                DrawSpace(spaceBefore);
            }
            
            var rect = GetNewRect(0, height);
            
            // 确保覆盖全部留白
            rect.width += 8;
            
            EditorGUI.DrawRect(rect, separatorColor);
            
            if (spaceAfter > 0)
            {
                DrawSpace(spaceAfter);
            }
        }

        #endregion
    }
}
