using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentLock : QHierarchyBaseComponent
    {
        private readonly Texture2D lockButtonTexture;

        private Color activeColor;
        private Color inactiveColor;

        private bool showModifierWarning;

        private const int RECT_WIDTH = 13;
        private const string WHITE_LIST = "Canvas (Environment)";
        private const string SHIFT_TIP_LOCK = "要递归锁定此物体吗? (可以在设置中关闭此提示)";
        private const string SHIFT_TIP_UNLOCK = "要递归解锁此物体吗? (可以在设置中关闭此提示)";
        private const string ALT_TIP_LOCK = "要同时锁定此物体以及全部同级物体吗? (可以在设置中关闭此提示)";
        private const string ALT_TIP_UNLOCK = "要同时解锁此物体以及全部同级物体吗? (可以在设置中关闭此提示)";

        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyComponentLock()
        {
            rect.width = RECT_WIDTH;

            lockButtonTexture = QResources.Instance().GetTexture(QTexture.QLockButton);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalShowModifierWarning, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.LockShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 修改设置
        /// </summary>
        private void SettingsChanged()
        {
            showModifierWarning = QSettings.Instance().Get<bool>(EM_QHierarchySettings.AdditionalShowModifierWarning);
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.LockShowDuringPlayMode);
            activeColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width + COMPONENT_SPACE)
            {
                return EM_QLayoutStatus.Failed;
            }

            curRect.x -= rect.width + COMPONENT_SPACE;
            rect.x = curRect.x;
            rect.y = curRect.y;
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 判断是否锁定
        /// </summary>
        private static bool IsLocked(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            return hierarchyObjectList != null && hierarchyObjectList.lockedObjects.Contains(gameObject);
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            // 特殊情况: 没有 Canvas 组件的 UGUI 预制体会自动创建一个 Locked 标记的 Canvas
            if (CheckWhiteList(gameObject))
            {
                return;
            }

            // 在 QHierarchy 工具数据库中当前游戏物体是否被记录为 Locked
            var isLockedInQHierarchy = IsLocked(gameObject, hierarchyObjectList);

            // 在 Hierarchy 面板中当前游戏物体是否被记录为 Locked
            var isLockedInHierarchy = (gameObject.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable;

            // 如果两个记录有冲突
            if (isLockedInQHierarchy)
            {
                if (isLockedInHierarchy == false)
                {
                    // 或运算
                    gameObject.hideFlags |= HideFlags.NotEditable;
                    EditorUtility.SetDirty(gameObject);
                }
            }
            else
            {
                if (isLockedInHierarchy)
                {
                    // 异或运算 (不等返回真)
                    gameObject.hideFlags ^= HideFlags.NotEditable;
                    EditorUtility.SetDirty(gameObject);
                }
            }

            // 图标颜色
            QHierarchyColorUtils.SetColor(isLockedInQHierarchy ? activeColor : inactiveColor);

            // 绘制图标
            UnityEngine.GUI.DrawTexture(rect, lockButtonTexture);

            QHierarchyColorUtils.ClearColor();
        }

        /// <summary>
        /// 白名单检测
        /// </summary>
        private static bool CheckWhiteList(UnityEngine.Object obj)
        {
            // 特殊情况: 没有 Canvas 组件的 UGUI 预制体会自动创建一个 Locked 标记的 Canvas
            return WHITE_LIST.Contains(obj.name);
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            if (CheckWhiteList(gameObject))
            {
                return;
            }

            // 左键点击图标
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                var isLock = IsLocked(gameObject, hierarchyObjectList);
                var targetGameObjects = new List<GameObject>();

                if (currentEvent.shift)
                {
                    var tip = isLock ? SHIFT_TIP_UNLOCK : SHIFT_TIP_LOCK;
                    if (showModifierWarning == false || EditorUtility.DisplayDialog("改变锁定状态", tip, "Yes", "Cancel"))
                    {
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects);
                    }
                }
                else if (currentEvent.alt)
                {
                    var parent = gameObject.transform.parent;

                    if (parent != null)
                    {
                        var tip = isLock ? ALT_TIP_UNLOCK : ALT_TIP_LOCK;
                        if (showModifierWarning == false || EditorUtility.DisplayDialog("改变锁定状态", tip, "Yes", "Cancel"))
                        {
                            GetGameObjectListRecursive(parent.gameObject, ref targetGameObjects, 1);
                            targetGameObjects.Remove(parent.gameObject);
                        }
                    }
                }
                else
                {
                    if (Selection.Contains(gameObject))
                    {
                        targetGameObjects.AddRange(Selection.gameObjects);
                    }
                    else
                    {
                        GetGameObjectListRecursive(gameObject, ref targetGameObjects, 0);
                    }
                }

                SetLock(targetGameObjects, hierarchyObjectList, !isLock);
                currentEvent.Use();
            }
        }

        /// <summary>
        /// Lock 功能关闭事件
        /// </summary>
        public override void DisabledHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList)
        {
            if (hierarchyObjectList != null && hierarchyObjectList.lockedObjects.Contains(gameObject))
            {
                // 清空锁定物体缓存
                hierarchyObjectList.lockedObjects.Remove(gameObject);

                // 与运算关闭 NotEditable
                gameObject.hideFlags &= ~HideFlags.NotEditable;

                EditorUtility.SetDirty(gameObject);
            }
        }

        /// <summary>
        /// 设置锁定状态
        /// </summary>
        private static void SetLock(in List<GameObject> gameObjects, QHierarchyObjectList hierarchyObjectList, bool targetLock)
        {
            if (gameObjects.Count == 0)
            {
                return;
            }

            if (hierarchyObjectList == null)
            {
                hierarchyObjectList = QHierarchyObjectListManager.Instance().GetObjectList(gameObjects[0]);
            }

            Undo.RecordObject(hierarchyObjectList, targetLock ? "Lock" : "Unlock");

            for (var i = gameObjects.Count - 1; i >= 0; i--)
            {
                var curGameObject = gameObjects[i];

                Undo.RecordObject(curGameObject, targetLock ? "Lock" : "Unlock");

                if (targetLock)
                {
                    curGameObject.hideFlags |= HideFlags.NotEditable;

                    if (hierarchyObjectList.lockedObjects.Contains(curGameObject) == false)
                    {
                        hierarchyObjectList.lockedObjects.Add(curGameObject);
                    }
                }
                else
                {
                    curGameObject.hideFlags &= ~HideFlags.NotEditable;
                    hierarchyObjectList.lockedObjects.Remove(curGameObject);
                }

                EditorUtility.SetDirty(curGameObject);
            }
        }
    }
}
