using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentVerticesAndTrianglesCount: QHierarchyBaseComponent
    {
        // PRIVATE
        private readonly GUIStyle labelStyle;
        private Color verticesLabelColor;
        private Color trianglesLabelColor;
        private bool calculateTotalCount;
        private bool showTrianglesCount;
        private bool showVerticesCount;
        private EM_QHierarchySize labelSize;

        // CONSTRUCTOR
        public QHierarchyComponentVerticesAndTrianglesCount ()
        {
            labelStyle = new GUIStyle {
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleRight
            };

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShow                  , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShowDuringPlayMode    , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesCalculateTotalCount   , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShowTriangles         , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShowVertices          , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesLabelSize             , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesVerticesLabelColor    , SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesTrianglesLabelColor   , SettingsChanged);

            SettingsChanged();
        }

        // PRIVATE
        private void SettingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShowDuringPlayMode);
            calculateTotalCount         = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesCalculateTotalCount);
            showTrianglesCount          = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShowTriangles);
            showVerticesCount           = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShowVertices);
            verticesLabelColor          = QSettings.Instance().GetColor(EM_QHierarchySettings.VerticesAndTrianglesVerticesLabelColor);
            trianglesLabelColor         = QSettings.Instance().GetColor(EM_QHierarchySettings.VerticesAndTrianglesTrianglesLabelColor);
            labelSize                   = (EM_QHierarchySize)QSettings.Instance().Get<int>(EM_QHierarchySettings.VerticesAndTrianglesLabelSize);

            #if UNITY_2019_1_OR_NEWER
                labelStyle.fontSize = labelSize == EM_QHierarchySize.Big ? 7 : 6;
                rect.width = labelSize == EM_QHierarchySize.Big ? 24 : 22;
            #else
                labelStyle.fontSize = labelSize == EM_QHierarchySize.Big ? 9 : 8;
                rect.width = labelSize == EM_QHierarchySize.Big ? 33 : 25;
            #endif
        }   

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width)
            {
                return EM_QLayoutStatus.Failed;
            }
            else
            {
                curRect.x -= rect.width + 2;
                rect.x = curRect.x;
                rect.y = curRect.y;
                #if UNITY_2019_1_OR_NEWER                
                    rect.y += labelSize == EM_QHierarchySize.Big ? 2 : 1;
                #endif
                return EM_QLayoutStatus.Success;
            }
        }
        
        public override void Draw(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {  
            var vertexCount = 0;
            var triangleCount = 0;

            var meshFilterArray = calculateTotalCount ? gameObject.GetComponentsInChildren<MeshFilter>(true) : gameObject.GetComponents<MeshFilter>();
            foreach (var meshFilter in meshFilterArray) {
                var sharedMesh = meshFilter.sharedMesh;
                if (sharedMesh != null)
                {
                    if (showVerticesCount) vertexCount += sharedMesh.vertexCount;
                    if (showTrianglesCount) triangleCount += sharedMesh.triangles.Length;
                }
            }

            var skinnedMeshRendererArray = calculateTotalCount ? gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true) : gameObject.GetComponents<SkinnedMeshRenderer>();
            
            foreach (var skinnedMeshRenderer in skinnedMeshRendererArray) {
                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                if (sharedMesh != null)
                {   
                    if (showVerticesCount) vertexCount += sharedMesh.vertexCount;
                    if (showTrianglesCount) triangleCount += sharedMesh.triangles.Length;
                }
            }

            triangleCount /= 3;

            if (vertexCount > 0 || triangleCount > 0)
            {
                if (showTrianglesCount && showVerticesCount) 
                {
                    rect.y -= 4;
                    labelStyle.normal.textColor = verticesLabelColor;
                    EditorGUI.LabelField(rect, GetCountString(vertexCount), labelStyle);

                    rect.y += 8;
                    labelStyle.normal.textColor = trianglesLabelColor;
                    EditorGUI.LabelField(rect, GetCountString(triangleCount), labelStyle);
                }
                else if (showVerticesCount)
                {
                    labelStyle.normal.textColor = verticesLabelColor;
                    EditorGUI.LabelField(rect, GetCountString(vertexCount), labelStyle);
                }
                else
                {
                    labelStyle.normal.textColor = trianglesLabelColor;
                    EditorGUI.LabelField(rect, GetCountString(triangleCount), labelStyle);
                }
            }
        }

        // PRIVATE
        private static string GetCountString(int count) {
            if (count < 1000) {
                return count.ToString();
            }
            
            if (count < 1000000) {
                return (count > 100000) switch {
                    true => (count / 1000.0f).ToString("0") + "k",
                    _ => (count / 1000.0f).ToString("0.0") + "k"
                };
            }

            if (count > 10000000) {
                return (count / 1000.0f).ToString("0") + "M";
            }
            return (count / 1000000.0f).ToString("0.0") + "M";
        }
    }
}
