using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using Kuroha.Tool.QHierarchy.Editor.QHelper;
using Kuroha.Tool.QHierarchy.Editor.QData;
using System.Reflection;
using System.Collections;
using UnityEditorInternal;
using System.Text;
using Kuroha.GUI.Editor;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentError : QHierarchyBaseComponent
    {
        /// <summary>
        /// 有 Error 时图标的颜色
        /// </summary>
        private Color activeColor;

        /// <summary>
        /// 无 Error 时图标的颜色
        /// </summary>
        private Color inactiveColor;

        /// <summary>
        /// Error 提示的图标
        /// </summary>
        private readonly Texture2D errorIconTexture;

        private bool settingsShowErrorOfChildren;
        private bool settingsShowErrorTypeReferenceIsNull;
        private bool settingsShowErrorTypeReferenceIsMissing;
        private bool settingsShowErrorTypeStringIsEmpty;
        private bool settingsShowErrorIconScriptIsMissing;
        private bool settingsShowErrorIconWhenTagIsUndefined;
        private bool settingsShowErrorForDisabledComponents;
        private bool settingsShowErrorIconMissingEventMethod;
        private bool settingsShowErrorForDisabledGameObjects;

        /// <summary>
        /// 忽略错误的关键字列表
        /// </summary>
        private List<string> ignoreErrorOfMonoBehaviours;

        private int errorCount;
        private const int RECT_WIDTH = 7;
        private StringBuilder errorStringBuilder;
        private readonly List<string> targetPropertiesNames = new List<string>(10);

        /// <summary>
        /// 构造函数初始化
        /// </summary>
        public QHierarchyComponentError()
        {
            rect.width = RECT_WIDTH;

            errorIconTexture = QResources.Instance().GetTexture(QTexture.QErrorIcon);

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowIconOnParent, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowReferenceIsNull, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowReferenceIsMissing, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowStringIsEmpty, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowScriptIsMissing, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowForDisabledComponents, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowForDisabledGameObjects, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowMissingEventMethod, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowWhenTagOrLayerIsUndefined, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.ErrorIgnoreString, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalActiveColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.AdditionalInactiveColor, SettingsChanged);

            SettingsChanged();
        }

        /// <summary>
        /// 设置更改
        /// </summary>
        private void SettingsChanged()
        {
            activeColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalActiveColor);
            inactiveColor = QSettings.Instance().GetColor(EM_QHierarchySettings.AdditionalInactiveColor);

            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShow);
            settingsShowErrorOfChildren = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowIconOnParent);
            settingsShowErrorTypeReferenceIsNull = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowReferenceIsNull);
            settingsShowErrorTypeReferenceIsMissing = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowReferenceIsMissing);
            settingsShowErrorTypeStringIsEmpty = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowStringIsEmpty);
            settingsShowErrorIconScriptIsMissing = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowScriptIsMissing);
            settingsShowErrorForDisabledComponents = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowForDisabledComponents);
            settingsShowErrorForDisabledGameObjects = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowForDisabledGameObjects);
            settingsShowErrorIconMissingEventMethod = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowMissingEventMethod);
            settingsShowErrorIconWhenTagIsUndefined = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowWhenTagOrLayerIsUndefined);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.ErrorShowDuringPlayMode);
            var ignoreErrorOfMonoBehavioursString = QSettings.Instance().Get<string>(EM_QHierarchySettings.ErrorIgnoreString);

            if (string.IsNullOrEmpty(ignoreErrorOfMonoBehavioursString) == false)
            {
                ignoreErrorOfMonoBehaviours = new List<string>(ignoreErrorOfMonoBehavioursString.Split(new char[] {',', ';', '.', ' '}));
                ignoreErrorOfMonoBehaviours.RemoveAll(item => item == "");
            }
            else
            {
                ignoreErrorOfMonoBehaviours = null;
            }
        }

        /// <summary>
        /// 进行布局
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="selectionRect"></param>
        /// <param name="curRect"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
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
        /// 进行绘制
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="selectionRect"></param>
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var errorFound = FindError(gameObject, gameObject.GetComponents<MonoBehaviour>());

            if (errorFound)
            {
                QHierarchyColorUtils.SetColor(activeColor);
                UnityEngine.GUI.DrawTexture(rect, errorIconTexture);
                QHierarchyColorUtils.ClearColor();
            }
            else if (settingsShowErrorOfChildren)
            {
                var children = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
                errorFound = FindError(gameObject, children);
                if (errorFound)
                {
                    QHierarchyColorUtils.SetColor(inactiveColor);
                    UnityEngine.GUI.DrawTexture(rect, errorIconTexture);
                    QHierarchyColorUtils.ClearColor();
                }
            }
        }

        /// <summary>
        /// 单击事件处理
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hierarchyObjectList"></param>
        /// <param name="currentEvent"></param>
        public override void EventHandler(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Event currentEvent)
        {
            // 鼠标左键单击
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();

                errorCount = 0;
                errorStringBuilder = new StringBuilder();

                FindError(gameObject, gameObject.GetComponents<MonoBehaviour>(), true);

                if (errorCount > 0)
                {
                    var title = errorCount + (errorCount == 1 ? " error was found" : " errors were found");
                    Dialog.Display(title, errorStringBuilder.ToString(), Dialog.DialogType.Error, "OK");
                }
            }
        }

        /// <summary>
        /// 查找错误
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="components"></param>
        /// <param name="printError"></param>
        /// <returns></returns>
        private bool FindError(GameObject gameObject, in MonoBehaviour[] components, bool printError = false)
        {
            if (settingsShowErrorIconWhenTagIsUndefined)
            {
                #region Tag 未定义

                if (string.IsNullOrEmpty(gameObject.tag))
                {
                    if (printError)
                    {
                        AppendErrorLine("Tag is undefined");
                    }
                    else
                    {
                        return true;
                    }
                }

                #endregion

                #region Layer 未定义

                if (string.IsNullOrEmpty(LayerMask.LayerToName(gameObject.layer)))
                {
                    if (printError)
                    {
                        AppendErrorLine("Layer is undefined");
                    }
                    else
                    {
                        return true;
                    }
                }

                #endregion
            }

            for (var i = 0; i < components.Length; i++)
            {
                var monoBehaviour = components[i];

                #region Component Missing

                if (monoBehaviour == null)
                {
                    if (settingsShowErrorIconScriptIsMissing)
                    {
                        if (printError)
                        {
                            AppendErrorLine("Component #" + i + "# is missing");
                        }
                        else
                        {
                            return true;
                        }
                    }
                }

                #endregion

                else
                {
                    #region 白名单过滤

                    if (ignoreErrorOfMonoBehaviours != null)
                    {
                        for (var index = ignoreErrorOfMonoBehaviours.Count - 1; index >= 0; index--)
                        {
                            if (monoBehaviour.GetType().FullName.IndexOf(ignoreErrorOfMonoBehaviours[index], StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                return false;
                            }
                        }
                    }

                    #endregion

                    if (settingsShowErrorIconMissingEventMethod)
                    {
                        if (monoBehaviour.gameObject.activeSelf || settingsShowErrorForDisabledComponents)
                        {
                            if (IsUnityEventsNullOrMissing(monoBehaviour, printError))
                            {
                                if (printError == false)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    if (settingsShowErrorTypeReferenceIsNull || settingsShowErrorTypeStringIsEmpty || settingsShowErrorTypeReferenceIsMissing)
                    {
                        if (!monoBehaviour.enabled && !settingsShowErrorForDisabledComponents)
                        {
                            continue;
                        }

                        if (!monoBehaviour.gameObject.activeSelf && !settingsShowErrorForDisabledGameObjects)
                        {
                            continue;
                        }

                        var type = monoBehaviour.GetType();

                        while (type != null)
                        {
                            var bf = BindingFlags.Instance | BindingFlags.Public;
                            if (!type.FullName.Contains("UnityEngine"))
                            {
                                bf |= BindingFlags.NonPublic;
                            }

                            var fieldArray = type.GetFields(bf);

                            foreach (var field in fieldArray)
                            {
                                if (System.Attribute.IsDefined(field, typeof(HideInInspector)) ||
                                    System.Attribute.IsDefined(field, typeof(QHierarchyNullableAttribute)) ||
                                    System.Attribute.IsDefined(field, typeof(NonSerializedAttribute)) ||
                                    field.IsStatic) continue;

                                if (field.IsPrivate || !field.IsPublic)
                                {
                                    if (!System.Attribute.IsDefined(field, typeof(SerializeField)))
                                    {
                                        continue;
                                    }
                                }

                                var value = field.GetValue(monoBehaviour);

                                try
                                {
                                    if (settingsShowErrorTypeStringIsEmpty && field.FieldType == typeof(string) && value != null && ((string) value).Equals(""))
                                    {
                                        if (printError)
                                        {
                                            AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": String value is empty");
                                            continue;
                                        }

                                        return true;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }

                                try
                                {
                                    if (settingsShowErrorTypeReferenceIsMissing && value is Component component && component == null)
                                    {
                                        if (printError)
                                        {
                                            AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": Reference is missing");
                                            continue;
                                        }

                                        return true;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }

                                try
                                {
                                    if (settingsShowErrorTypeReferenceIsNull && (value == null || value.Equals(null)))
                                    {
                                        if (printError)
                                        {
                                            AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": Reference is null");
                                            continue;
                                        }

                                        return true;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }

                                try
                                {
                                    if (settingsShowErrorTypeReferenceIsNull && value is IEnumerable enumerable)
                                    {
                                        foreach (var item in enumerable)
                                        {
                                            if (item == null || item.Equals(null))
                                            {
                                                if (printError)
                                                {
                                                    AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": IEnumerable has value with null reference");
                                                    continue;
                                                }

                                                return true;
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            type = type.BaseType;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 检测是否有空的 Unity 事件
        /// </summary>
        /// <param name="monoBehaviour"></param>
        /// <param name="printError"></param>
        /// <returns></returns>
        private bool IsUnityEventsNullOrMissing(UnityEngine.Object monoBehaviour, bool printError)
        {
            targetPropertiesNames.Clear();

            // 反射得到全部的字段
            var fieldArray = monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // 筛选出全部的 UnityEventBase 类
            for (var index = fieldArray.Length - 1; index >= 0; index--)
            {
                var field = fieldArray[index];
                if (field.FieldType == typeof(UnityEventBase) || field.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                {
                    targetPropertiesNames.Add(field.Name);
                }
            }

            if (targetPropertiesNames.Count > 0)
            {
                var serializedMonoBehaviour = new SerializedObject(monoBehaviour);
                for (var i = targetPropertiesNames.Count - 1; i >= 0; i--)
                {
                    var targetProperty = targetPropertiesNames[i];
                    var property = serializedMonoBehaviour.FindProperty(targetProperty);
                    var propertyRelativeArray = property.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    
                    for (var j = propertyRelativeArray.arraySize - 1; j >= 0; j--)
                    {
                        var arrayElementAtIndex = propertyRelativeArray.GetArrayElementAtIndex(j);
                        var propertyTarget = arrayElementAtIndex.FindPropertyRelative("m_Target");
                        if (propertyTarget.objectReferenceValue == null)
                        {
                            if (printError)
                            {
                                AppendErrorLine(monoBehaviour.GetType().Name + ": Event object reference is null");
                            }
                            else
                            {
                                return true;
                            }
                        }

                        var propertyMethodName = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
                        if (string.IsNullOrEmpty(propertyMethodName.stringValue))
                        {
                            if (printError)
                            {
                                AppendErrorLine(monoBehaviour.GetType().Name + ": Event handler function is not selected");
                                continue;
                            }
                            else
                            {
                                return true;
                            }
                        }


                        var argumentAssemblyTypeName = arrayElementAtIndex.FindPropertyRelative("m_Arguments").FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue;

                        System.Type argumentAssemblyType;

                        if (!string.IsNullOrEmpty(argumentAssemblyTypeName))
                        {
                            argumentAssemblyType = System.Type.GetType(argumentAssemblyTypeName, false) ?? typeof(UnityEngine.Object);
                        }
                        else
                        {
                            argumentAssemblyType = typeof(UnityEngine.Object);
                        }

                        UnityEventBase dummyEvent = null;

                        var typeName = property.FindPropertyRelative("m_TypeName");
                        if (typeName != null)
                        {
                            var propertyTypeName = System.Type.GetType(typeName.stringValue, false);
                            if (propertyTypeName == null)
                            {
                                dummyEvent = new UnityEvent();
                            }
                            else
                            {
                                dummyEvent = Activator.CreateInstance(propertyTypeName) as UnityEventBase;
                            }
                        }

                        if (dummyEvent != null)
                        {
                            if (!UnityEventDrawer.IsPersistantListenerValid(dummyEvent, propertyMethodName.stringValue, propertyTarget.objectReferenceValue, (PersistentListenerMode) arrayElementAtIndex.FindPropertyRelative("m_Mode").enumValueIndex, argumentAssemblyType))
                            {
                                if (printError)
                                {
                                    AppendErrorLine(monoBehaviour.GetType().Name + ": Event handler function is missing");
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 增加 1 条错误
        /// </summary>
        /// <param name="error"></param>
        private void AppendErrorLine(string error)
        {
            errorCount++;
            errorStringBuilder.Append(errorCount.ToString());
            errorStringBuilder.Append(": ");
            errorStringBuilder.AppendLine(error);
        }
    }
}
