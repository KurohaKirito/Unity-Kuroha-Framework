#if Kuroha
using Kuroha.GUI.Editor.Splitter;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Editor
{
    public class HorizontalSplitterSampleWindow : EditorWindow
    {
        private HorizontalSplitter horizontalSplitter;
        
        [MenuItem("Tools/Sample/HorizontalSplitterWindow")]
        private static void Open()
        {
            GetWindow<HorizontalSplitterSampleWindow>("横向分割").minSize = new Vector2(150, 150);
        }
        
        private void OnGUI()
        {
            horizontalSplitter ??= new HorizontalSplitter(this, 50, 50, false);
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
