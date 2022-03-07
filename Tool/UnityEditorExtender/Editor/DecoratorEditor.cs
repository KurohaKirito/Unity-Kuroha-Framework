using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.UnityEditorExtender.Editor
{
    public abstract class DecoratorEditor : UnityEditor.Editor
    {
        private static readonly object[] emptyArray = Array.Empty<object>();
        
		/// <summary>
		/// Type object for the internally used (decorated) editor.
		/// </summary>
		private readonly System.Type targetCustomEditorType;
		
		/// <summary>
		/// Type object for the object that is edited by this editor.
		/// </summary>
		private System.Type selfCustomEditorType;
		
		private static readonly Dictionary<string, MethodInfo> decoratedMethods = new Dictionary<string, MethodInfo>();
		
		
		
		
		
		private UnityEditor.Editor editorInstance;

		/// <summary>
		/// Inspector 面板的 Editor 脚本实例
		/// </summary>
		private UnityEditor.Editor EditorInstance
		{
			get
			{
				if (editorInstance == null && targets != null && targets.Length > 0)
				{
					editorInstance = CreateEditor(targets, targetCustomEditorType);
				}
				
				if (editorInstance == null)
				{
					Debug.LogError("Could not create editor !");
				}
				
				return editorInstance;
			}
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		protected DecoratorEditor (string editorTypeName)
		{
			targetCustomEditorType = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetTypes().FirstOrDefault(t => t.Name == editorTypeName);
			
			GetSelfInspectorType ();
			
			// Check CustomEditor types.
			var originalEditedType = GetTargetInspectorType(targetCustomEditorType);
			if (originalEditedType != selfCustomEditorType)
			{
				throw new System.ArgumentException($"Type {selfCustomEditorType} does not match the editor {editorTypeName} type {originalEditedType}");
			}
		}
		
		/// <summary>
		/// 得到目标类的 CustomEditor 特性信息
		/// </summary>
		private static System.Type GetTargetInspectorType(System.Type type)
		{
			var attributes = type.GetCustomAttributes(typeof(CustomEditor), true) as CustomEditor[];
			var field = attributes?.Select(editor => editor.GetType().GetField("m_InspectedType", BindingFlags.NonPublic	| BindingFlags.Instance)).First();
			return field?.GetValue(attributes[0]) as System.Type;
		}
		
		/// <summary>
		/// 得到当前类的 CustomEditor 特性信息
		/// </summary>
		private void GetSelfInspectorType()
		{
			if (this.GetType().GetCustomAttributes(typeof(CustomEditor), true) is CustomEditor[] attributes)
			{
				var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", BindingFlags.NonPublic | BindingFlags.Instance)).First();
				selfCustomEditorType = field?.GetValue(attributes[0]) as System.Type;
			}
		}

		#region Unity Editor 方法

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            if (editorInstance != null)
            {
                DestroyImmediate(editorInstance);
            }
        }

        /*
        
        /// <summary>
        /// OnSceneGUI
        /// </summary>
        public void OnSceneGUI()
        {
            CallInspectorMethod("OnSceneGUI");
        }
        
        */

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorInstance.OnInspectorGUI();
        }

        /// <summary>
        /// DrawPreview
        /// </summary>
        public override void DrawPreview(Rect previewArea)
        {
            EditorInstance.DrawPreview(previewArea);
        }

        /// <summary>
        /// GetInfoString
        /// </summary>
        public override string GetInfoString()
        {
            return EditorInstance.GetInfoString();
        }

        /// <summary>
        /// GetPreviewTitle
        /// </summary>
        public override GUIContent GetPreviewTitle()
        {
            return EditorInstance.GetPreviewTitle();
        }

        /// <summary>
        /// HasPreviewGUI
        /// </summary>
        public override bool HasPreviewGUI()
        {
            return EditorInstance.HasPreviewGUI();
        }

        /// <summary>
        /// OnInteractivePreviewGUI
        /// </summary>
        public override void OnInteractivePreviewGUI(Rect rect, GUIStyle background)
        {
            EditorInstance.OnInteractivePreviewGUI(rect, background);
        }

        /// <summary>
        /// OnPreviewGUI
        /// </summary>
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            EditorInstance.OnPreviewGUI(rect, background);
        }

        /// <summary>
        /// OnPreviewSettings
        /// </summary>
        public override void OnPreviewSettings()
        {
            EditorInstance.OnPreviewSettings();
        }

        /// <summary>
        /// ReloadPreviewInstances
        /// </summary>
        public override void ReloadPreviewInstances()
        {
            EditorInstance.ReloadPreviewInstances();
        }

        /// <summary>
        /// RenderStaticPreview
        /// </summary>
        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            return EditorInstance.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        /// <summary>
        /// RequiresConstantRepaint
        /// </summary>
        public override bool RequiresConstantRepaint()
        {
            return EditorInstance.RequiresConstantRepaint();
        }

        /// <summary>
        /// UseDefaultMargins
        /// </summary>
        public override bool UseDefaultMargins()
        {
            return EditorInstance.UseDefaultMargins();
        }
        
        /// <summary>
        /// OnHeaderGUI
        /// </summary>
        protected override void OnHeaderGUI()
        {
            CallInspectorMethod("OnHeaderGUI");
        }

        #endregion

        /// <summary>
        /// 调用原生的 Inspector 方法
        /// </summary>
        /// <param name="methodName"></param>
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

            method?.Invoke(EditorInstance, emptyArray);
        }
    }
}