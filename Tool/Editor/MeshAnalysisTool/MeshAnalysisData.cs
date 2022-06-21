namespace Script.Effect.Editor.AssetTool.Tool.Editor.MeshAnalysisTool {
    public static class MeshAnalysisData {
        /// <summary>
        /// 检测类型
        /// </summary>
        public enum DetectType {
            Scene,
            Path,
            GameObject
        }
        
        public enum DetectTypeAtPath {
            Meshes,
            Prefabs,
        }
        
        /// <summary>
        /// 检测类型
        /// </summary>
        public enum DetectMeshType {
            RendererMesh,
            ColliderMesh
        }
    }
}
