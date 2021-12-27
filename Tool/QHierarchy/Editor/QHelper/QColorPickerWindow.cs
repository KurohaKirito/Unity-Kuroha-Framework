using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Kuroha.Tool.QHierarchy.Editor.QData;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public delegate void QColorSelectedHandler(GameObject[] gameObjects, Color color);
    public delegate void QColorRemovedHandler(GameObject[] gameObjects);

    public class QColorPickerWindow: PopupWindowContent 
    {
        private GameObject[] gameObjects;
        private QColorSelectedHandler colorSelectedHandler;
        private QColorRemovedHandler colorRemovedHandler;
        private readonly Texture2D colorPaletteTexture;
        private Rect paletteRect;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QColorPickerWindow(GameObject[] gameObjects, QColorSelectedHandler colorSelectedHandler, QColorRemovedHandler colorRemovedHandler)
        {
            this.gameObjects = gameObjects;
            this.colorSelectedHandler = colorSelectedHandler;
            this.colorRemovedHandler = colorRemovedHandler;

            colorPaletteTexture = QResources.getInstance().getTexture(QTexture.QColorPalette);
            paletteRect = new Rect(0, 0, colorPaletteTexture.width, colorPaletteTexture.height);
        }

        // DESTRUCTOR
        public override void OnClose()
        {
            gameObjects = null;
            colorSelectedHandler = null;
            colorRemovedHandler = null; 
        }

        // GUI
        public override Vector2 GetWindowSize()
        {
            return new Vector2(paletteRect.width, paletteRect.height);
        }

        public override void OnGUI(Rect rect)
        {
            UnityEngine.GUI.DrawTexture(paletteRect, colorPaletteTexture);

            var mousePosition = Event.current.mousePosition;
            if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.MouseUp && paletteRect.Contains(mousePosition))
            {
                Event.current.Use();
                if (mousePosition.x < 15 && mousePosition.y < 15)
                {
                    colorRemovedHandler(gameObjects);
                }
                else
                {
                    colorSelectedHandler(gameObjects, colorPaletteTexture.GetPixel((int)mousePosition.x, colorPaletteTexture.height - (int)mousePosition.y));
                }
                this.editorWindow.Close();
            }
        }
    }
}

