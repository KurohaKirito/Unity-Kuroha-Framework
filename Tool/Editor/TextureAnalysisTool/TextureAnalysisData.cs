﻿namespace Script.Effect.Editor.AssetTool.Tool.Editor.TextureAnalysisTool {
    public class TextureAnalysisData {
        /// <summary>
        /// 检测类型
        /// </summary>
        public enum DetectType {
            /// <summary>
            /// 检测当前场景
            /// </summary>
            Scene,

            /// <summary>
            /// 检测指定路径
            /// </summary>
            Path,

            /// <summary>
            /// 检测特定游戏物体及其所有子物体
            /// </summary>
            GameObject
        }
        
        public enum DetectTypeAtPath {
            /// <summary>
            /// 检测指定路径下全部的纹理资源
            /// </summary>
            Textures,
            
            /// <summary>
            /// 检测指定路径全部预制体所引用的纹理
            /// </summary>
            Prefabs,
        }

        public int id;
        public int width;
        public int height;
        public float memory;
        public bool isSolid;
        public string repeatInfo;
        public string textureName;
        public string texturePath;
        
        public bool Equal(TextureAnalysisData other) {
            if (other != null) {
                if (width == other.width &&
                    height == other.height &&
                    textureName == other.textureName) {
                    return true;
                }
            }

            return false;
        }
    }
}