namespace Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool {
    public class SceneAnalysisData {
        public int id;
        public string assetType;
        public int tris;
        public int verts;
        public string readwrite;
        public int uv;
        public int uv2;
        public int uv3;
        public int uv4;
        public int colors;
        public int tangents;
        public int normals;
        public string meshCompression;
        public string meshOptimizationFlags;
        public string importNormals;
        public string importLights;
        public string importCameras;
        public string weldVertices;
        public string assetName;
        public string assetPath;

        public bool IsEqual(SceneAnalysisData other) {
            if (other != null) {
                if (assetPath == other.assetPath &&
                    tris == other.tris &&
                    verts == other.verts &&
                    uv == other.uv &&
                    uv2 == other.uv2 &&
                    uv3 == other.uv3 &&
                    colors == other.colors) {
                    return true;
                }
            }

            return false;
        }
    }
}
