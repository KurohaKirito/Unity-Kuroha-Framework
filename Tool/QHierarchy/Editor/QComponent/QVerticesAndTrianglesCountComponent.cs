using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QVerticesAndTrianglesCountComponent: QBaseComponent
    {
        // PRIVATE
        private GUIStyle labelStyle;
        private Color verticesLabelColor;
        private Color trianglesLabelColor;
        private bool calculateTotalCount;
        private bool showTrianglesCount;
        private bool showVerticesCount;
        private QHierarchySize labelSize;

        // CONSTRUCTOR
        public QVerticesAndTrianglesCountComponent ()
        {
            labelStyle = new GUIStyle();            
            labelStyle.clipping = TextClipping.Clip;  
            labelStyle.alignment = TextAnchor.MiddleRight;

            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesShow                  , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesShowDuringPlayMode    , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesCalculateTotalCount   , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesShowTriangles         , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesShowVertices          , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesLabelSize             , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesVerticesLabelColor    , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.VerticesAndTrianglesTrianglesLabelColor   , settingsChanged);

            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            enabled                     = QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowDuringPlayMode);
            calculateTotalCount         = QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesCalculateTotalCount);
            showTrianglesCount          = QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowTriangles);
            showVerticesCount           = QSettings.Instance().Get<bool>(EM_QSetting.VerticesAndTrianglesShowVertices);
            verticesLabelColor          = QSettings.Instance().GetColor(EM_QSetting.VerticesAndTrianglesVerticesLabelColor);
            trianglesLabelColor         = QSettings.Instance().GetColor(EM_QSetting.VerticesAndTrianglesTrianglesLabelColor);
            labelSize                   = (QHierarchySize)QSettings.Instance().Get<int>(EM_QSetting.VerticesAndTrianglesLabelSize);

            #if UNITY_2019_1_OR_NEWER
                labelStyle.fontSize = labelSize == QHierarchySize.Big ? 7 : 6;
                rect.width = labelSize == QHierarchySize.Big ? 24 : 22;
            #else
                labelStyle.fontSize = labelSize == QHierarchySize.Big ? 9 : 8;
                rect.width = labelSize == QHierarchySize.Big ? 33 : 25;
            #endif
        }   

        // DRAW
        public override EM_QLayoutStatus Layout(GameObject gameObject, QObjectList objectList, Rect selectionRect, ref Rect curRect, float maxWidth)
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
                    rect.y += labelSize == QHierarchySize.Big ? 2 : 1;
                #endif
                return EM_QLayoutStatus.Success;
            }
        }
        
        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {  
            int vertexCount = 0;
            int triangleCount = 0;

            MeshFilter[] meshFilterArray = calculateTotalCount ? gameObject.GetComponentsInChildren<MeshFilter>(true) : gameObject.GetComponents<MeshFilter>();
            for (int i = 0; i < meshFilterArray.Length; i++)
            {
                Mesh sharedMesh = meshFilterArray[i].sharedMesh;
                if (sharedMesh != null)
                {
                    if (showVerticesCount) vertexCount += sharedMesh.vertexCount;
                    if (showTrianglesCount) triangleCount += sharedMesh.triangles.Length;
                }
            }

            SkinnedMeshRenderer[] skinnedMeshRendererArray = calculateTotalCount ? gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true) : gameObject.GetComponents<SkinnedMeshRenderer>();
            for (int i = 0; i < skinnedMeshRendererArray.Length; i++)
            {
                Mesh sharedMesh = skinnedMeshRendererArray[i].sharedMesh;
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
                    EditorGUI.LabelField(rect, getCountString(vertexCount), labelStyle);

                    rect.y += 8;
                    labelStyle.normal.textColor = trianglesLabelColor;
                    EditorGUI.LabelField(rect, getCountString(triangleCount), labelStyle);
                }
                else if (showVerticesCount)
                {
                    labelStyle.normal.textColor = verticesLabelColor;
                    EditorGUI.LabelField(rect, getCountString(vertexCount), labelStyle);
                }
                else
                {
                    labelStyle.normal.textColor = trianglesLabelColor;
                    EditorGUI.LabelField(rect, getCountString(triangleCount), labelStyle);
                }
            }
        }

        // PRIVATE
        private string getCountString(int count)
        {
            if (count < 1000) return count.ToString();
            else if (count < 1000000) 
            {
                if (count > 100000) return (count / 1000.0f).ToString("0") + "k";
                else return (count / 1000.0f).ToString("0.0") + "k";
            }
            else 
            {
                if (count > 10000000) return (count / 1000.0f).ToString("0") + "M";
                else return (count / 1000000.0f).ToString("0.0") + "M";
            }
        }
    }
}

