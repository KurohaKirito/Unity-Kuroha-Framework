using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentMonoBehavior : QHierarchyBaseComponent
    {
        private const float TREE_STEP_WIDTH = 14.0f;
        private const float TREE_STEP_HEIGHT = 16.0f;

        private readonly Texture2D monoBehaviourIconTexture;
        private readonly Texture2D monoBehaviourIconObjectTexture;
        private bool ignoreUnityMonoBehaviour;
        private Color iconColor;
        private bool showTreeMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentMonoBehavior()
        {
            rect.width = TREE_STEP_WIDTH;
            rect.height = TREE_STEP_HEIGHT;

            monoBehaviourIconTexture = QResources.Instance().GetTexture(QTexture.QMonoBehaviourIcon);
            monoBehaviourIconObjectTexture = QResources.Instance().GetTexture(QTexture.QTreeMapObject);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.MonoBehaviourIconIgnoreUnityMonoBehaviour, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.MonoBehaviourIconShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.MonoBehaviourIconShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.MonoBehaviourIconColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.TreeMapShow, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 更改设置
        /// </summary>
        private void SettingsChanged()
        {
            ignoreUnityMonoBehaviour = QSettings.Instance().Get<bool>(EM_QHierarchySettings.MonoBehaviourIconIgnoreUnityMonoBehaviour);
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.MonoBehaviourIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.MonoBehaviourIconShowDuringPlayMode);
            iconColor = QSettings.Instance().GetColor(EM_QHierarchySettings.MonoBehaviourIconColor);
            showTreeMap = QSettings.Instance().Get<bool>(EM_QHierarchySettings.TreeMapShow);

            EditorApplication.RepaintHierarchyWindow();
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var isCustomComponent = false;

            if (ignoreUnityMonoBehaviour)
            {
                var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
                foreach (var monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour != null)
                    {
                        var fullName = monoBehaviour.GetType().FullName;
                        if (fullName != null && fullName.Contains("UnityEngine") == false)
                        {
                            isCustomComponent = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                isCustomComponent = gameObject.GetComponent<MonoBehaviour>() != null;
            }

            if (isCustomComponent)
            {
                var ident = Mathf.FloorToInt(selectionRect.x / TREE_STEP_WIDTH) - 1;

                rect.x = ident * TREE_STEP_WIDTH;
                rect.y = selectionRect.y;
                rect.width = 16;

                rect.x += TREE_STEP_WIDTH + 1;
                rect.width += 1;

                QHierarchyColorUtils.SetColor(iconColor);
                UnityEngine.GUI.DrawTexture(rect, monoBehaviourIconTexture);
                QHierarchyColorUtils.ClearColor();

                if (showTreeMap == false && gameObject.transform.childCount == 0)
                {
                    rect.width = 14;
                    UnityEngine.GUI.DrawTexture(rect, monoBehaviourIconObjectTexture);
                }
            }
        }
    }
}
