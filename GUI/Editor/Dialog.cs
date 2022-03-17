using System;
using Kuroha.Framework.Utility.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.GUI.Editor
{
    public class Dialog : EditorWindow
    {
        /// <summary>
        /// 弹窗类型
        /// </summary>
        public enum DialogType
        {
            Message,
            Warn,
            Error
        }

        /// <summary>
        /// 弹窗按钮事件类型
        /// </summary>
        private enum DialogButtonType
        {
            Null,
            Ok,
            Cancel,
            Alt
        }

        /// <summary>
        /// 消息的滑动框
        /// </summary>
        private static Vector2 messageVector2Scroll;

        /// <summary>
        /// 弹窗标题
        /// </summary>
        private static string windowTitle;

        /// <summary>
        /// 弹窗消息内容
        /// </summary>
        private static string message;

        /// <summary>
        /// OK 按钮文本
        /// </summary>
        private static string buttonOk;

        /// <summary>
        /// 取消按钮文本
        /// </summary>
        private static string buttonCancel;

        /// <summary>
        /// Alt 按钮文本
        /// </summary>
        private static string buttonAlt;

        /// <summary>
        /// 弹窗类型: 日志, 警告, 错误
        /// </summary>
        private static DialogType windowType;

        /// <summary>
        /// 记录默认颜色
        /// </summary>
        private static Color defaultColor;

        /// <summary>
        /// 窗口句柄
        /// </summary>
        private static Dialog window;

        /// <summary>
        /// 处理按钮事件前的窗口 ID
        /// </summary>
        private static int windowIDBeforeEvent;
        
        /// <summary>
        /// 处理按钮事件后的窗口 ID
        /// </summary>
        private static int windowIDAfterEvent;

        /// <summary>
        /// 弹窗按钮事件结果
        /// </summary>
        private static DialogButtonType pressedButton = DialogButtonType.Null;

        /// <summary>
        /// 确定按钮事件
        /// </summary>
        private static Action okEvent;

        /// <summary>
        /// 取消按钮事件
        /// </summary>
        private static Action cancelEvent;

        /// <summary>
        /// Alt 按钮事件
        /// </summary>
        private static Action altEvent;

        /// <summary>
        /// 显示弹窗
        /// </summary>
        /// <param name="titleText">弹窗标题</param>
        /// <param name="info">弹窗中要显示的信息</param>
        /// <param name="type">想要弹出什么类型的弹窗</param>
        /// <param name="buttonOkName">OK 按钮的显示文本</param>
        /// <param name="buttonCancelName">Cancel 按钮的显示文本</param>
        /// <param name="buttonAltName">Alt 按钮的显示文本</param>
        /// <param name="okAction">OK 按钮事件</param>
        /// <param name="cancelAction">Cancel 按钮事件</param>
        /// <param name="altAction">Alt 按钮事件</param>
        public static void Display(string titleText, string info, DialogType type, string buttonOkName, string buttonCancelName, string buttonAltName, Action okAction = null, Action cancelAction = null, Action altAction = null)
        {
            if (windowTitle == titleText && message == info && windowType == type && buttonOk == buttonOkName && buttonCancel == buttonCancelName && buttonAlt == buttonAltName)
            {
                return;
            }
            
            if (window != null)
            {
                CloseWindow();
            }
            
            windowTitle = titleText;
            message = info;
            windowType = type;
            
            buttonOk = string.IsNullOrEmpty(buttonOkName) ? "OK" : buttonOkName;
            buttonCancel = buttonCancelName;
            buttonAlt = buttonAltName;

            window = GetWindow<Dialog>();
            windowIDAfterEvent = window.GetInstanceID();
            window.minSize = new Vector2(400, 150);
            window.maxSize = window.minSize;
            window.titleContent = windowType switch
            {
                DialogType.Message => new GUIContent(windowTitle, EditorGUIUtility.IconContent("console.infoIcon.sml").image as Texture2D, "消息"),
                DialogType.Warn => new GUIContent(windowTitle, EditorGUIUtility.IconContent("console.warnIcon.sml").image as Texture2D, "警告"),
                DialogType.Error => new GUIContent(windowTitle, EditorGUIUtility.IconContent("console.errorIcon.sml").image as Texture2D, "错误"),
                _ => throw new Exception()
            };
            
            if (okAction != null) { okEvent = okAction; }
            if (cancelAction != null) { cancelEvent = cancelAction; }
            if (altAction != null) { altEvent = altAction; }
            
            defaultColor = UnityEngine.GUI.backgroundColor;
        }

        /// <summary>
        /// 强制焦点和强制刷新
        /// </summary>
        private void OnInspectorUpdate()
        {
            Focus();
            Repaint();
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUILayout.Space(25);

            #region 大图标

            switch (windowType)
            {
                case DialogType.Message:
                    GUILayout.Label(EditorGUIUtility.IconContent("console.infoIcon@2x"));
                    break;

                case DialogType.Warn:
                    GUILayout.Label(EditorGUIUtility.IconContent("console.warnIcon@2x"));
                    break;

                case DialogType.Error:
                    GUILayout.Label(EditorGUIUtility.IconContent("console.errorIcon@2x"));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            #endregion

            GUILayout.Space(15);

            GUILayout.BeginVertical();
            messageVector2Scroll = GUILayout.BeginScrollView(messageVector2Scroll);
            GUILayout.FlexibleSpace();
            GUILayout.Label(message, "wordWrappedLabel");
            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Space(110);
            GUILayout.FlexibleSpace();

            #region 取消按钮

            if (!string.IsNullOrEmpty(buttonCancel))
            {
                if (GUILayout.Button(buttonCancel, GUILayout.Height(20), GUILayout.Width(80)))
                {
                    pressedButton = DialogButtonType.Cancel;
                    OnDialogEvent();
                }

                GUILayout.FlexibleSpace();
            }

            #endregion

            #region Alt 按钮

            if (!string.IsNullOrEmpty(buttonAlt))
            {
                if (GUILayout.Button(buttonAlt, GUILayout.Height(20), GUILayout.Width(80)))
                {
                    pressedButton = DialogButtonType.Alt;
                    OnDialogEvent();
                }

                GUILayout.FlexibleSpace();
            }

            #endregion

            #region 确定按钮

            if (!string.IsNullOrEmpty(buttonOk))
            {
                UnityEngine.GUI.backgroundColor = new Color(57f / 255, 150f / 255, 249f / 255);
                if (GUILayout.Button(buttonOk, GUILayout.Height(20), GUILayout.Width(80)))
                {
                    pressedButton = DialogButtonType.Ok;
                    OnDialogEvent();
                }

                UnityEngine.GUI.backgroundColor = defaultColor;
                GUILayout.FlexibleSpace();
            }

            #endregion

            GUILayout.Space(50);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private static void CloseWindow()
        {
            // 清空 ID
            windowIDAfterEvent = 0;
            
            // 重置枚举
            windowType = DialogType.Message;
            pressedButton = DialogButtonType.Null;
            
            // 清空消息
            message = string.Empty;
            
            // 清空按钮
            buttonOk = string.Empty;
            buttonCancel = string.Empty;
            buttonAlt = string.Empty;
            
            // 清空事件
            okEvent = null;
            cancelEvent = null;
            altEvent = null;
            
            // 关闭窗口
            window.Close();
            DestroyImmediate(window);
        }

        /// <summary>
        /// 按钮事件
        /// </summary>
        private static void OnDialogEvent()
        {
            if (pressedButton != DialogButtonType.Null)
            {
                // 记录 ID
                windowIDBeforeEvent = windowIDAfterEvent;
                
                switch (pressedButton)
                {
                    case DialogButtonType.Ok:
                        okEvent?.Invoke();
                        break;

                    case DialogButtonType.Cancel:
                        cancelEvent?.Invoke();
                        break;

                    case DialogButtonType.Alt:
                        altEvent?.Invoke();
                        break;
                    
                    case DialogButtonType.Null:
                        break;
                    
                    default:
                        DebugUtil.LogError("Kuroha.GUI.Editor.Dialog 枚举错误!");
                        break;
                }
                
                if (windowIDBeforeEvent == 0 || windowIDBeforeEvent == windowIDAfterEvent)
                {
                    CloseWindow();
                }
            }
        }
    }
}
