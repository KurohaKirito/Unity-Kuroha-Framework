using System;
using System.Collections.Generic;
using Kuroha.Tool.QHierarchy.Editor.QBase;
using Kuroha.Tool.QHierarchy.RunTime;
using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;
using Kuroha.Tool.QHierarchy.Editor.QHelper;

namespace Kuroha.Tool.QHierarchy.Editor.QComponent
{
    public class QMonoBehaviorIconComponent: QBaseComponent
    {
        // CONST
        private const float TREE_STEP_WIDTH  = 14.0f;
        private const float TREE_STEP_HEIGHT = 16.0f;

        // PRIVATE
        private Texture2D monoBehaviourIconTexture;   
        private Texture2D monoBehaviourIconObjectTexture; 
        private bool ignoreUnityMonobehaviour;
        private Color iconColor;
        private bool showTreeMap;

        // CONSTRUCTOR 
        public QMonoBehaviorIconComponent()
        {
            rect.width  = 14;
            rect.height = 16;
            
            monoBehaviourIconTexture = QResources.Instance().GetTexture(QTexture.QMonoBehaviourIcon);
            monoBehaviourIconObjectTexture  = QResources.Instance().GetTexture(QTexture.QTreeMapObject);

            QSettings.Instance().addEventListener(EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.MonoBehaviourIconShow                     , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.MonoBehaviourIconShowDuringPlayMode       , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.MonoBehaviourIconColor                    , settingsChanged);
            QSettings.Instance().addEventListener(EM_QSetting.TreeMapShow                               , settingsChanged);
            settingsChanged();
        }

        // PRIVATE
        private void settingsChanged()
        {
            ignoreUnityMonobehaviour    = QSettings.Instance().Get<bool>(EM_QSetting.MonoBehaviourIconIgnoreUnityMonoBehaviour);
            enabled                     = QSettings.Instance().Get<bool>(EM_QSetting.MonoBehaviourIconShow);
            showComponentDuringPlayMode = QSettings.Instance().Get<bool>(EM_QSetting.MonoBehaviourIconShowDuringPlayMode);
            iconColor                   = QSettings.Instance().getColor(EM_QSetting.MonoBehaviourIconColor);
            showTreeMap                 = QSettings.Instance().Get<bool>(EM_QSetting.TreeMapShow);
            EditorApplication.RepaintHierarchyWindow();  
        }

        public override void Draw(GameObject gameObject, QObjectList objectList, Rect selectionRect)
        {
            bool foundCustomComponent = false;   
            if (ignoreUnityMonobehaviour)
            {
                Component[] components = gameObject.GetComponents<MonoBehaviour>();                
                for (int i = components.Length - 1; i >= 0; i--)
                {
                    if (components[i] != null && !components[i].GetType().FullName.Contains("UnityEngine")) 
                    {
                        foundCustomComponent = true;
                        break;
                    }
                }                
            }
            else
            {
                foundCustomComponent = gameObject.GetComponent<MonoBehaviour>() != null;
            }

            if (foundCustomComponent)
            {
                int ident = Mathf.FloorToInt(selectionRect.x / TREE_STEP_WIDTH) - 1;

                rect.x = ident * TREE_STEP_WIDTH;
                rect.y = selectionRect.y;
                rect.width = 16;

                #if UNITY_2018_3_OR_NEWER
                    rect.x += TREE_STEP_WIDTH + 1;
                    rect.width += 1;
                #elif UNITY_5_6_OR_NEWER
                    
                #elif UNITY_5_3_OR_NEWER
                    rect.x += TREE_STEP_WIDTH;
                #endif                

                QColorUtils.setColor(iconColor);
                UnityEngine.GUI.DrawTexture(rect, monoBehaviourIconTexture);
                QColorUtils.clearColor();

                if (!showTreeMap && gameObject.transform.childCount == 0)
                {
                    rect.width = 14;
                    UnityEngine.GUI.DrawTexture(rect, monoBehaviourIconObjectTexture);
                }
            }
        }
    }
}

