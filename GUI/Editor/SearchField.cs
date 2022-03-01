using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.GUI.Editor {
    public class SearchField {
        private GUIStyle textFieldRoundEdge;
        private GUIStyle textFieldRoundEdgeCancelButton;
        private GUIStyle textFieldRoundEdgeCancelButtonEmpty;
        private GUIStyle transparentTextField;

        public string DrawSearchField(string inputSearchText) {
            if (textFieldRoundEdge == null) {
                textFieldRoundEdge = new GUIStyle("SearchTextField");
                textFieldRoundEdgeCancelButton = new GUIStyle("SearchCancelButton");
                textFieldRoundEdgeCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
                transparentTextField = new GUIStyle(EditorStyles.whiteLabel) {
                    normal = {
                        textColor = EditorStyles.textField.normal.textColor
                    }
                };
            }

            // 获取当前输入框的 Rect(位置大小)
            var position = EditorGUILayout.GetControlRect();
            // 设置圆角 style 的 GUIStyle
            var localRoundEdge = this.textFieldRoundEdge;
            // 设置输入框的 GUIStyle 为透明, 所以看到的输入框是 textFieldRoundEdge 的风格
            var localTextField = this.transparentTextField;
            // 选择取消按钮(x)的 GUIStyle
            var gUIStyle = inputSearchText != ""? textFieldRoundEdgeCancelButton : textFieldRoundEdgeCancelButtonEmpty;

            // 输入框的水平位置向左移动取消按钮宽度的距离
            position.width -= gUIStyle.fixedWidth;
            // 如果面板重绘
            if (Event.current.type == EventType.Repaint) {
                // 根据是否是专业版来选取颜色
                UnityEngine.GUI.contentColor = (EditorGUIUtility.isProSkin? Color.black : new Color(0f, 0f, 0f, 0.5f));
                // 当没有输入的时候提示请输入
                localRoundEdge.Draw(position, string.IsNullOrEmpty(inputSearchText)? new GUIContent("请输入") : new GUIContent(""), 0);
                // 因为是全局变量, 用完要重置回来
                UnityEngine.GUI.contentColor = Color.white;
            }

            var rect = position;
            // 为了空出左边那个放大镜的位置
            var num = localRoundEdge.CalcSize(new GUIContent("")).x - 2f;
            rect.width -= num;
            rect.x += num;
            rect.y += 1f; // 为了和后面的 style 对其

            inputSearchText = EditorGUI.TextField(rect, inputSearchText, localTextField);
            // 绘制取消按钮，位置要在输入框右边
            position.x += position.width;
            position.width = gUIStyle.fixedWidth;
            position.height = gUIStyle.fixedHeight;
            if (UnityEngine.GUI.Button(position, GUIContent.none, gUIStyle) && inputSearchText != "") {
                inputSearchText = "";
                // 用户是否做了输入
                UnityEngine.GUI.changed = true;
                // 把焦点移开输入框
                GUIUtility.keyboardControl = 0;
            }

            return inputSearchText;
        }
    }
}