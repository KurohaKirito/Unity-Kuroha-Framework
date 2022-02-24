using System;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QHelper
{
    public static class QHierarchyColorUtils
    {
        /// <summary>
        /// 默认颜色
        /// </summary>
        public static Color DefaultColor { get; set; } = Color.white;

        public static void SetColor(Color newColor, float multiColor, float multiAlpha)
        {
            newColor.r *= multiColor;
            newColor.g *= multiColor;
            newColor.b *= multiColor;
            newColor.a *= multiAlpha;
            
            UnityEngine.GUI.color = newColor;
        }

        /// <summary>
        /// (8 位十六进制) 字符串转为颜色
        /// </summary>
        public static Color StringToColor(string colorString)
        {
            return IntToColor(Convert.ToUInt32(colorString, 16));
        }

        /// <summary>
        /// 颜色转换为字符串 (8 位十六进制)
        /// </summary>
        public static string ColorToString(Color color)
        {
            var intColor = ColorToInt(color);
            return intColor.ToString("X8");
        }

        /// <summary>
        /// int 数字转换为颜色
        /// </summary>
        private static Color IntToColor(uint color)
        {
            var r = ((color >> 16) & 0xFF) / 255.0f;
            var g = ((color >> 8)  & 0xFF) / 255.0f;
            var b = ((color >> 0)  & 0xFF) / 255.0f;
            var a = ((color >> 24) & 0xFF) / 255.0f;
            
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// 颜色转换为 int 数字
        /// </summary>
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
