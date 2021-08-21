using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.TextureAnalysisTool
{
    public class TextureAnalysisWindow : EditorWindow
    {
        public static void Open()
        {
            var window = GetWindow<TextureAnalysisWindow>("纹理分析工具");
            window.minSize = new Vector2(414, 108);
            window.maxSize = window.minSize;
        }

        protected void OnGUI()
        {
            TextureAnalysisGUI.OnGUI();
        }
    }
}