using System;
using UnityEngine;
using UnityEditor;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public class QColorUtils
    {
        private static Color defaultColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        public static void SetDefaultColor(Color defaultColor)
        {
            QColorUtils.defaultColor = defaultColor;
        }

        public static void SetColor(Color newColor)
        {
            UnityEngine.GUI.color = newColor;
        }

        public static void SetColor(Color newColor, float multiColor, float multiAlpha)
        {
            newColor.r *= multiColor;
            newColor.g *= multiColor;
            newColor.b *= multiColor;
            newColor.a *= multiAlpha;
            UnityEngine.GUI.color = newColor;
        }

        public static void ClearColor()
        {
            UnityEngine.GUI.color = defaultColor;
        }

        public static Color fromString(string color)
        {
            return fromInt(Convert.ToUInt32(color, 16));
        }

        public static string toString(Color color)
        {
            uint intColor = toInt(color);
            return intColor.ToString("X8");
        }

        public static Color fromInt(uint color)
        {
            return new Color(((color >> 16) & 0xFF) / 255.0f,
                ((color >> 8) & 0xFF) / 255.0f,
                ((color >> 0) & 0xFF) / 255.0f,
                ((color >> 24) & 0xFF) / 255.0f);
        }

        public static uint toInt(Color color)
        {
            return (uint) ((byte) (color.r * 255) << 16) +
                   (uint) ((byte) (color.g * 255) << 8) +
                   (uint) ((byte) (color.b * 255) << 0) +
                   (uint) ((byte) (color.a * 255) << 24);
        }
    }
}
