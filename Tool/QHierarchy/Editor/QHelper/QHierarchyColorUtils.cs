using System;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public static class QHierarchyColorUtils
    {
        private static Color defaultColor = Color.white;

        public static void SetDefaultColor(Color color)
        {
            defaultColor = color;
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

        public static Color StringToColor(string color)
        {
            return IntToColor(Convert.ToUInt32(color, 16));
        }

        public static string ColorToString(Color color)
        {
            var intColor = ColorToInt(color);
            return intColor.ToString("X8");
        }

        private static Color IntToColor(uint color)
        {
            var r = ((color >> 16) & 0xFF) / 255.0f;
            var g = ((color >> 8)  & 0xFF) / 255.0f;
            var b = ((color >> 0)  & 0xFF) / 255.0f;
            var a = ((color >> 24) & 0xFF) / 255.0f;
            
            return new Color(r, g, b, a);
        }

        private static uint ColorToInt(Color color)
        {
            var r = (uint) ((byte) (color.r * 255) << 16);
            var g = (uint) ((byte) (color.g * 255) << 8);
            var b = (uint) ((byte) (color.b * 255) << 0);
            var a = (uint) ((byte) (color.a * 255) << 24);

            return r + g + b + a;
        }
    }
}
