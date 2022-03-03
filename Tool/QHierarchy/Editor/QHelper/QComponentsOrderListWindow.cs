using System;
using UnityEditor;
using UnityEngine;
using Kuroha.Tool.QHierarchy.Editor.QData;
using System.Text;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public class QComponentsOrderListWindow
    {
        private readonly EditorWindow window;
        
        /// <summary>
        /// 拖拽项目前面表示可拖拽的图标
        /// </summary>
        private readonly Texture2D dragButton;
        
        /// <summary>
        /// 拖拽标志
        /// </summary>
        private bool dragAndDrop;
        
        /// <summary>
        /// 被拖拽项目顶部 Y 坐标和当前鼠标 Y 坐标之间的偏移
        /// </summary>
        private float dragOffset;
        
        /// <summary>
        /// 项目高度
        /// </summary>
        public const int ITEM_HEIGHT = 18;
        
        private int originalDragIndex;
        
        private readonly Color backgroundColor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="window"></param>
        public QComponentsOrderListWindow(EditorWindow window)
        {
            this.window = window;
            dragButton = QResources.Instance().GetTexture(EM_QHierarchyTexture.QDragButton);
            backgroundColor = QResources.Instance().GetColor(EM_QHierarchyColor.BackgroundDark);
        }
        
        /// <summary>
        /// 绘制 GUI
        /// </summary>
        public void Draw(Rect rect, string[] componentIds)
        {
            
            var top = rect.y;
            var currentEvent = Event.current;

            // 计算当前鼠标所在的排序的索引
            // (鼠标位置 - 顶部位置) / ITEM_HEIGHT
            var currentMouseIndexRaw = Mathf.RoundToInt((currentEvent.mousePosition.y - dragOffset - top) / ITEM_HEIGHT);
            var currentMouseIndex = Mathf.Clamp(currentMouseIndexRaw, 0, componentIds.Length - 1);
            
            // 处理拖拽
            switch (dragAndDrop)
            {
                // 拖拽完成
                case true when currentEvent.type == EventType.MouseUp:
                {
                    dragAndDrop = false;
                    window.Repaint();

                    if (currentMouseIndex != originalDragIndex)
                    {
                        var newIconOrder = "";
                        for (var j = 0; j < componentIds.Length; j++)
                        {
                            if (j == currentMouseIndex)
                            {
                                if (j > originalDragIndex)
                                {
                                    newIconOrder += componentIds[j] + ";";
                                    newIconOrder += componentIds[originalDragIndex] + ";";
                                }
                                else
                                {
                                    newIconOrder += componentIds[originalDragIndex] + ";";
                                    newIconOrder += componentIds[j] + ";";
                                }
                            }
                            else if (j != originalDragIndex)
                            {
                                newIconOrder += componentIds[j] + ";";
                            }
                        }

                        newIconOrder = newIconOrder.TrimEnd(';');
                        QSettings.Instance().Set(EM_QHierarchySettings.ComponentsOrder, newIconOrder);
                        componentIds = newIconOrder.Split(';');
                    }

                    break;
                }
                // 拖拽中
                case true when currentEvent.type == EventType.MouseDrag:
                {
                    window.Repaint();
                    break;
                }
            }

            // 绘制
            for (var index = 0; index < componentIds.Length; index++)
            {
                var type = (EM_QHierarchyComponent) Enum.Parse(typeof(EM_QHierarchyComponent), componentIds[index]);
                var curRect = new Rect(rect.x, top + ITEM_HEIGHT * index, rect.width, ITEM_HEIGHT);
                
                if (dragAndDrop)
                {
                    if (originalDragIndex != index)
                    {
                        if (index < originalDragIndex && currentMouseIndex <= index)
                        {
                            curRect.y += ITEM_HEIGHT;
                        }
                        else if (index > originalDragIndex && currentMouseIndex >= index)
                        {
                            curRect.y -= ITEM_HEIGHT;
                        }

                        DrawComponentLabel(curRect, type);
                    }
                }
                else
                {
                    if (currentEvent.type == EventType.MouseDown && curRect.Contains(currentEvent.mousePosition))
                    {
                        dragAndDrop = true;
                        originalDragIndex = index;
                        dragOffset = currentEvent.mousePosition.y - curRect.y;
                        Event.current.Use();
                    }
                    
                    DrawComponentLabel(curRect, type);
                }
            }

            if (dragAndDrop)
            {
                var curY = currentEvent.mousePosition.y - dragOffset;
                curY = Mathf.Clamp(curY, rect.y, rect.y + rect.height - ITEM_HEIGHT);
                
                var type = (EM_QHierarchyComponent) Enum.Parse(typeof(EM_QHierarchyComponent), componentIds[originalDragIndex]);
                DrawComponentLabel(new Rect(rect.x, curY, rect.width, rect.height), type, true);
            }
        }

        private void DrawComponentLabel(Rect rect, EM_QHierarchyComponent type, bool withBackground = false)
        {
            if (withBackground)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y - 2, rect.width, 20), backgroundColor);
            }

            UnityEngine.GUI.DrawTexture(new Rect(rect.x, rect.y - 2, 20, 20), dragButton);
            
            var labelRect = new Rect(rect.x + 31, rect.y, rect.width - 20, 16);
            labelRect.y -= (EditorGUIUtility.singleLineHeight - labelRect.height) * 0.5f;
            EditorGUI.LabelField(labelRect, GetTextWithSpaces(type.ToString()));
        }

        private static string GetTextWithSpaces(string text)
        {
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                {
                    newText.Append(' ');
                }
                
                newText.Append(text[i]);
            }

            newText.Replace(" Component", "");
            return newText.ToString();
        }
    }
}
