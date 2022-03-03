using UnityEditor;
using UnityEngine;

namespace Kuroha.GUI.Editor
{
    public class Searcher
    {
        private GUIStyle textFieldRoundEdge;
        private GUIStyle textFieldRoundEdgeCancelButton;
        private GUIStyle textFieldRoundEdgeCancelButtonEmpty;
        private GUIStyle transparentTextField;

        /// <summary>
        /// 绘制搜索框
        /// </summary>
        /// <param name="text">搜索文本 [实时]</param>
        /// <param name="tipText">提示文本</param>
        /// <returns></returns>
        public bool OnGUI(ref string text, string tipText)
        {
            if (textFieldRoundEdge == null)
            {
                textFieldRoundEdge = new GUIStyle("SearchTextField");
                textFieldRoundEdgeCancelButton = new GUIStyle("SearchCancelButton");
                textFieldRoundEdgeCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
                transparentTextField = new GUIStyle(EditorStyles.whiteLabel)
                {
                    normal =
                    {
                        textColor = EditorStyles.textField.normal.textColor
                    }
                };
            }

            // 获取当前输入框的 Rect (位置大小)
            var position = EditorGUILayout.GetControlRect();
            // 设置圆角 style 的 GUIStyle
            var localRoundEdgeStyle = textFieldRoundEdge;
            // 设置输入框的 GUIStyle 为透明, 所以看到的输入框是 textFieldRoundEdge 的风格
            var localTextFieldStyle = transparentTextField;
            // 选择取消按钮(x)的 GUIStyle
            var guiStyle = string.IsNullOrEmpty(text)? textFieldRoundEdgeCancelButtonEmpty : textFieldRoundEdgeCancelButton;

            // 输入框的水平位置向左移动取消按钮宽度的距离
            position.width -= guiStyle.fixedWidth;

            // 如果面板重绘
            if (Event.current.type == EventType.Repaint)
            {
                UnityEngine.GUI.contentColor = EditorGUIUtility.isProSkin? Color.black : new Color(0f, 0f, 0f, 0.5f);
                localRoundEdgeStyle.Draw(position, string.IsNullOrEmpty(text) ? new GUIContent(tipText) : new GUIContent(string.Empty), 0);
                UnityEngine.GUI.contentColor = Color.white;
            }

            var rect = position;
            
            // 空出左边那个放大镜的位置
            var num = localRoundEdgeStyle.CalcSize(new GUIContent("")).x - 2f;
            rect.width -= num;
            rect.x += num;
            
            // 和后面的 style 对齐
            rect.y += 1f;

            text = EditorGUI.TextField(rect, text, localTextFieldStyle);

            // 绘制取消按钮，位置要在输入框右边
            position.x += position.width;
            position.width = guiStyle.fixedWidth;
            position.height = guiStyle.fixedHeight;
            if (UnityEngine.GUI.Button(position, GUIContent.none, guiStyle) && !string.IsNullOrEmpty(text))
            {
                text = string.Empty;
                UnityEngine.GUI.changed = true;
                GUIUtility.keyboardControl = 0;
            }

            return UnityEngine.GUI.changed;
        }
    }
}
