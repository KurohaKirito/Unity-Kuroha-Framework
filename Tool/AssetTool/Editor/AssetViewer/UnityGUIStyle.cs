namespace Kuroha.Tool.Editor.AssetViewer
{
    /*
    public class UnityGUIStyle : EditorWindow
    {
        private Vector2 scrollPosition;
        private string search;
        private GUIStyle textStyle;

        [MenuItem("Tool/GUIStyleViewer", false, 10)]
        private static void OpenStyleViewer()
        {
            GetWindow<UnityGUIStyle>(false, "内置GUIStyle");
        }

        private void OnEnable()
        {
            if (textStyle == null)
            {
                textStyle = new GUIStyle("HeaderLabel") {
                    fontSize = 25
                };
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal("HelpBox");
            {
                GUILayout.Label("结果如下：", textStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Search:");
                search = EditorGUILayout.TextField(search);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
            {
                GUILayout.Label("样式展示", textStyle, GUILayout.Width(300));
                GUILayout.Label("名字", textStyle, GUILayout.Width(300));
            }
            GUILayout.EndHorizontal();
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                foreach (var style in UnityEngine.GUI.skin.customStyles)
                {
                    if (style.name.ToLower().Contains(search.ToLower()))
                    {
                        GUILayout.Space(15);
                        GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                        {
                            if (GUILayout.Button(string.Empty, style, GUILayout.Width(160)))
                            {
                                EditorGUIUtility.systemCopyBuffer = style.name;
                            }
                            GUILayout.Space(40);
                            EditorGUILayout.SelectableLabel(style.name, GUILayout.Width(300));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                
            }
            GUILayout.EndScrollView();
        }
    }
    
    */
    
    using UnityEngine;
    using UnityEditor;

    public  class EditorStyleViewer : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private  string search =  string.Empty;

        #if Kuroha
        [MenuItem( "Tools/GUI样式查看器")]
        #endif
        public  static  void Init()
        {
            EditorWindow.GetWindow( typeof(EditorStyleViewer));
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal( "HelpBox");
            GUILayout.Label( "单击示例将复制其名到剪贴板",  "label");
            GUILayout.FlexibleSpace();
            GUILayout.Label( "查找:");
            search = EditorGUILayout.TextField(search);
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (GUIStyle style  in GUI.skin)
            {
                if (style.name.ToLower().Contains(search.ToLower()))
                {
                    GUILayout.BeginHorizontal( "PopupCurveSwatchBackground");
                    GUILayout.Space( 7);
                    if (GUILayout.Button(style.name, style))
                    {
                        EditorGUIUtility.systemCopyBuffer =  "\"" + style.name +  "\"";
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.SelectableLabel( "\"" + style.name +  "\"");
                    GUILayout.EndHorizontal();
                    GUILayout.Space( 11);
                }
            }

            GUILayout.EndScrollView();
        }
    }
}