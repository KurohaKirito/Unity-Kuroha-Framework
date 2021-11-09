using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetViewer.Editor
{
    public class SizeEdit : EditorWindow
    {
        public static void Open()
        {
            var window = GetWindow<SizeEdit>("Icons");
            window.minSize = new Vector2(125, 125);
            window.maxSize = window.minSize;
        }

        private void OnGUI()
        {
            if (GUILayout.Button($"缩小左右 {UnityIcon.windowWidth}", GUILayout.Width(120), GUILayout.Height(30)))
            {
                if (ReferenceEquals(UnityIcon.window, null))
                {
                    return;
                }

                UnityIcon.windowWidth--;
                Refresh();
            }
            else if (GUILayout.Button($"缩小上下 {UnityIcon.windowHeight}", GUILayout.Width(120), GUILayout.Height(30)))
            {
                if (ReferenceEquals(UnityIcon.window, null))
                {
                    return;
                }

                UnityIcon.windowHeight--;
                Refresh();
            }
            else if (GUILayout.Button($"增大左右 {UnityIcon.windowWidth}", GUILayout.Width(120), GUILayout.Height(30)))
            {
                if (ReferenceEquals(UnityIcon.window, null))
                {
                    return;
                }

                UnityIcon.windowWidth++;
                Refresh();
            }
            else if (GUILayout.Button($"增大上下 {UnityIcon.windowHeight}", GUILayout.Width(120), GUILayout.Height(30)))
            {
                if (ReferenceEquals(UnityIcon.window, null))
                {
                    return;
                }

                UnityIcon.windowHeight++;
                Refresh();
            }
        }

        /// <summary>
        /// 刷新窗口
        /// </summary>
        private static void Refresh()
        {
            UnityIcon.window.minSize = new Vector2(UnityIcon.windowWidth, UnityIcon.windowHeight);
            UnityIcon.window.maxSize = UnityIcon.window.minSize;
            UnityIcon.window.Repaint();
        }
    }
}