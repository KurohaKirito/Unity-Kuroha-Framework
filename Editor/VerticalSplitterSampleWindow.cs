#if Kuroha
using Kuroha.GUI.Editor.Splitter;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Editor
{
    public class VerticalSplitterSampleWindow : EditorWindow
    {
        private VerticalSplitter horizontalSplitter;
        
        [MenuItem("Tools/Sample/VerticalSplitterWindow")]
        private static void Open()
        {
            GetWindow<VerticalSplitterSampleWindow>("纵向分割").minSize = new Vector2(150, 150);
        }

        private void OnGUI()
        {
            horizontalSplitter ??= new VerticalSplitter(this, 100, 50, false);
            horizontalSplitter.OnGUI(position, MainRect, SubRect);
        }
        
        private static void MainRect(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                GUILayout.Label("MainRect");
            }
        }
        
        private static void SubRect(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                GUILayout.Label("SubRect");
            }
        }
    }
}
#endif
