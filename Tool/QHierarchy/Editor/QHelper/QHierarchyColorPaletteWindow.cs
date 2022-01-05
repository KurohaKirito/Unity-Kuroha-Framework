using UnityEngine;
using UnityEditor;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    /// <summary>
    /// 选择颜色事件
    /// </summary>
    public delegate void QColorSelectedHandler(GameObject[] gameObjects, Color color);

    /// <summary>
    /// 清除颜色事件
    /// </summary>
    public delegate void QColorRemovedHandler(GameObject[] gameObjects);

    /// <summary>
    /// 颜色选择窗口
    /// </summary>
    public class QHierarchyColorPaletteWindow : PopupWindowContent
    {
        private readonly Texture2D colorPaletteTexture;
        private Rect paletteRect;
        private GameObject[] gameObjects;
        private QColorSelectedHandler colorSelectedHandler;
        private QColorRemovedHandler colorRemovedHandler;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QHierarchyColorPaletteWindow(GameObject[] gameObjects, QColorSelectedHandler colorSelectedHandler, QColorRemovedHandler colorRemovedHandler)
        {
            this.gameObjects = gameObjects;
            this.colorSelectedHandler = colorSelectedHandler;
            this.colorRemovedHandler = colorRemovedHandler;

            colorPaletteTexture = QResources.Instance().GetTexture(QTexture.QColorPalette);
            paletteRect = new Rect(0, 0, colorPaletteTexture.width, colorPaletteTexture.height);
        }

        /// <summary>
        /// 关闭事件
        /// </summary>
        public override void OnClose()
        {
            gameObjects = null;
            colorSelectedHandler = null;
            colorRemovedHandler = null;
        }

        /// <summary>
        /// 定义窗口大小
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetWindowSize()
        {
            return new Vector2(paletteRect.width, paletteRect.height);
        }

        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="rect"></param>
        public override void OnGUI(Rect rect)
        {
            UnityEngine.GUI.DrawTexture(paletteRect, colorPaletteTexture);

            var mousePosition = Event.current.mousePosition;
            
            if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.MouseUp && paletteRect.Contains(mousePosition))
            {
                Event.current.Use();
                
                // 左上角 (15, 15) 为移除颜色按钮
                if (mousePosition.x < 15 && mousePosition.y < 15)
                {
                    colorRemovedHandler(gameObjects);
                }
                else
                {
                    var positionX = (int)mousePosition.x;
                    var positionY = colorPaletteTexture.height - (int)mousePosition.y;
                    var color = colorPaletteTexture.GetPixel(positionX, positionY);
                    colorSelectedHandler(gameObjects, color);
                }

                editorWindow.Close();
            }
        }
    }
}
