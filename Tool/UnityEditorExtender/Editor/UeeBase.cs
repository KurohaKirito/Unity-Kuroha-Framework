using System;
using System.Collections.Generic;
using System.Reflection;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.UnityEditorExtender.Editor
{
    /// <summary>
    /// Uee: Unity Editor Extender
    /// Unity 编辑器原布局扩展器 - 基类
    /// </summary>
    public abstract class UeeBase : UnityEditor.Editor
    {
        /// <summary>
        /// 用于充当反射调用方法时会使用到的空参数
        /// </summary>
        private static readonly object[] emptyArgs = Array.Empty<object>();

        /// <summary>
        /// 目标组件的 Custom Editor 类型
        /// </summary>
        private readonly Type targetCustomEditorClassInfo;

        /// <summary>
        /// 目标组件的类型
        /// </summary>
        private readonly Type targetType;

        /// <summary>
        /// Type object for the object that is edited by this editor.
        /// </summary>
        private Type selfCustomEditorType;

        /// <summary>
        /// 缓存通过反射获取到的目标组件中的方法
        /// 避免重复使用反射获取
        /// </summary>
        private static readonly Dictionary<string, MethodInfo> decoratedMethods = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Inspector 面板的 Editor 脚本实例
        /// </summary>
        private UnityEditor.Editor editorInstance;

        /// <summary>
        /// 构造方法
        /// </summary>
        protected UeeBase(string targetCustomEditorTypeName)
        {
            // 获取目标的 CustomEditor 类型
            targetCustomEditorClassInfo = ReflectionUtil.GetClass(ReflectionUtil.GetAssembly(typeof(UnityEditor.Editor)), targetCustomEditorTypeName);

            // 获取目标 CustomEditor 的 Component 类型
            var targetComponentType = GetTargetComponentType(targetCustomEditorClassInfo);

            // 获取自身 CustomEditor 的 Component 类型
            var selfComponentType = GetTargetComponentType(GetType());

            // 对比两个组件类型
            if (targetComponentType != selfComponentType)
            {
                DebugUtil.LogError($"{GetType()} 所绘制的组件类型为 : {selfComponentType}\r\n{targetCustomEditorTypeName} 所绘制的组件类型为 : {targetComponentType}\r\n两者不匹配!", null, "red");
            }
        }

        /// <summary>
        /// 根绝 CustomEditor 类型得到 Component 的类型
        /// </summary>
        private static Type GetTargetComponentType(Type customEditorType)
        {
            // 得到 CustomEditor 特性
            var selfCustomEditor = ReflectionUtil.GetCustomAttribute<CustomEditor>(customEditorType, true);

            // 得到 m_InspectedType 字段
            const string FIELD_NAME = "m_InspectedType";
            const BindingFlags FIELD_BINDING = BindingFlags.NonPublic | BindingFlags.Instance;
            var inspectedType = selfCustomEditor.GetType().GetField(FIELD_NAME, FIELD_BINDING);

            // 得到组件的类型, 即 m_InspectedType 的值
            return inspectedType?.GetValue(selfCustomEditor) as Type;
        }

        /// <summary>
        /// 创建 Inspector 面板的 Editor 脚本实例
        /// </summary>
        private UnityEditor.Editor CreateEditorInstance()
        {
            UnityEditor.Editor newEditorInstance = null;

            if (targets != null && targets.Length > 0)
            {
                newEditorInstance = CreateEditor(targets, targetCustomEditorClassInfo);
            }

            if (newEditorInstance == null)
            {
                Debug.LogError($"不能创建此编辑器脚本 {targetCustomEditorClassInfo} !");
            }

            return newEditorInstance;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            editorInstance ??= CreateEditorInstance();
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            DestroyImmediate(editorInstance);
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            editorInstance.OnInspectorGUI();
        }

        /// <summary>
        /// DrawPreview
        /// </summary>
        public override void DrawPreview(Rect previewArea)
        {
            editorInstance.DrawPreview(previewArea);
        }

        /// <summary>
        /// GetInfoString
        /// </summary>
        public override string GetInfoString()
        {
            return editorInstance.GetInfoString();
        }

        /// <summary>
        /// GetPreviewTitle
        /// </summary>
        public override GUIContent GetPreviewTitle()
        {
            return editorInstance.GetPreviewTitle();
        }

        /// <summary>
        /// HasPreviewGUI
        /// </summary>
        public override bool HasPreviewGUI()
        {
            return editorInstance.HasPreviewGUI();
        }

        /// <summary>
        /// OnInteractivePreviewGUI
        /// </summary>
        public override void OnInteractivePreviewGUI(Rect rect, GUIStyle background)
        {
            editorInstance.OnInteractivePreviewGUI(rect, background);
        }

        /// <summary>
        /// OnPreviewGUI
        /// </summary>
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            editorInstance.OnPreviewGUI(rect, background);
        }

        /// <summary>
        /// OnPreviewSettings
        /// </summary>
        public override void OnPreviewSettings()
        {
            editorInstance.OnPreviewSettings();
        }

        /// <summary>
        /// ReloadPreviewInstances
        /// </summary>
        public override void ReloadPreviewInstances()
        {
            editorInstance.ReloadPreviewInstances();
        }

        /// <summary>
        /// RenderStaticPreview
        /// </summary>
        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            return editorInstance.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        /// <summary>
        /// RequiresConstantRepaint
        /// </summary>
        public override bool RequiresConstantRepaint()
        {
            return editorInstance.RequiresConstantRepaint();
        }

        /// <summary>
        /// UseDefaultMargins
        /// </summary>
        public override bool UseDefaultMargins()
        {
            return editorInstance.UseDefaultMargins();
        }

        /// <summary>
        /// OnHeaderGUI
        /// </summary>
        protected override void OnHeaderGUI()
        {
            CallInspectorMethod("OnHeaderGUI");
        }

        /// <summary>
        /// 调用 Inspector Editor 中的非公开方法
        /// </summary>
        private void CallInspectorMethod(string methodName)
        {
            MethodInfo method;

            if (decoratedMethods.ContainsKey(methodName) == false)
            {
                method = selfCustomEditorType.GetMethod(methodName);
                if (method != null)
                {
                    decoratedMethods[methodName] = method;
                }
                else
                {
                    Debug.LogError($"can't find method : {methodName}");
                }
            }
            else
            {
                method = decoratedMethods[methodName];
            }

            method?.Invoke(editorInstance, emptyArgs);
        }
    }
}
