using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QHierarchyComponentVerticesAndTrianglesCount : QHierarchyBaseComponent
    {
        private readonly GUIStyle labelStyle;
        private Color verticesLabelColor;
        private Color trianglesLabelColor;
        private bool calculateTotalCount;
        private bool showTrianglesCount;
        private bool showVerticesCount;
        private EM_QHierarchySize labelSize;

        public QHierarchyComponentVerticesAndTrianglesCount()
        {
            labelStyle = new GUIStyle
            {
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleRight
            };

            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShow, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShowDuringPlayMode, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesCalculateTotalCount, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShowTriangles, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesShowVertices, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesLabelSize, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesVerticesLabelColor, SettingsChanged);
            QSettings.Instance().AddEventListener(EM_QHierarchySettings.VerticesAndTrianglesTrianglesLabelColor, SettingsChanged);

            SettingsChanged();
        }

        private void SettingsChanged()
        {
            enabled = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShowDuringPlayMode);
            calculateTotalCount = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesCalculateTotalCount);
            showTrianglesCount = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShowTriangles);
            showVerticesCount = QSettings.Instance().Get<bool>(EM_QHierarchySettings.VerticesAndTrianglesShowVertices);
            verticesLabelColor = QSettings.Instance().GetColor(EM_QHierarchySettings.VerticesAndTrianglesVerticesLabelColor);
            trianglesLabelColor = QSettings.Instance().GetColor(EM_QHierarchySettings.VerticesAndTrianglesTrianglesLabelColor);
            labelSize = (EM_QHierarchySize) QSettings.Instance().Get<int>(EM_QHierarchySettings.VerticesAndTrianglesLabelSize);

            labelStyle.fontSize = labelSize == EM_QHierarchySize.Big ? 10 : 8;
            rect.width = labelSize == EM_QHierarchySize.Big ? 36 : 30;
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public override EM_QLayoutStatus Layout(GameObject gameObject, QHierarchyObjectList hierarchyObjectList, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < rect.width + COMPONENT_SPACE)
            {
                return EM_QLayoutStatus.Failed;
            }
            
            curRect.x -= rect.width + COMPONENT_SPACE;
            rect.x = curRect.x;
            rect.y = curRect.y;
            
            return EM_QLayoutStatus.Success;
        }

        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public override void Draw(GameObject gameObjectToDraw, QHierarchyObjectList hierarchyObjectList, Rect selectionRect)
        {
            var vertexCount = 0;
            var triangleCount = 0;

            var meshFilterArray = gameObjectToDraw.GetComponentsInChildren<MeshFilter>(calculateTotalCount);
            foreach (var meshFilter in meshFilterArray)
            {
                var sharedMesh = meshFilter.sharedMesh;
                if (sharedMesh == null)
                {
                    continue;
                }

                if (showVerticesCount)
                {
                    vertexCount += sharedMesh.vertexCount;
                }

                if (showTrianglesCount)
                {
                    triangleCount += sharedMesh.triangles.Length;
                }
            }

            var skinnedMeshRendererArray = gameObjectToDraw.GetComponentsInChildren<SkinnedMeshRenderer>(calculateTotalCount);
            foreach (var skinnedMeshRenderer in skinnedMeshRendererArray)
            {
                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                if (sharedMesh == null)
                {
                    continue;
                }

                if (showVerticesCount)
                {
                    vertexCount += sharedMesh.vertexCount;
                }

                if (showTrianglesCount)
                {
                    triangleCount += sharedMesh.triangles.Length;
                }
            }

            triangleCount /= 3;

            if (vertexCount <= 0 && triangleCount <= 0)
            {
                return;
            }
            
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

        /// <summary>
        /// 处理数字为字符串
        /// </summary>
        private static string GetCountString(int count)
        {
            string result;
            
            // 小于 1 K
            if (count < 1000)
            {
                result = count.ToString();
            }

            // 小于 100 K
            else if (count < 100000)
            {
                result = $"{count / 1000.0f:0.0}K";
            }
            
            // 小于 1 M
            else if (count < 1000000)
            {
                result = $"{count / 1000.0f:0}K";
            }
            
            // 小于 100 M
            else if (count < 100000000)
            {
                result = $"{count / 1000000.0f:0.0}M";
            }
            else
            {
                result = $"{count / 1000000.0f:0}M";
            }
            
            return result;
        }
    }
}
