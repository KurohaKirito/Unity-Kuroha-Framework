using System;

namespace Script.Effect.Editor.AssetTool.Tool.Editor.SceneAnalysisTool {
    public class SceneAnalysisData {
        public int id;
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
        public string assetName;
        public string assetPath;

        public bool Equals(SceneAnalysisData other) {
            if (other != null) {
                if (tris == other.tris &&
                    verts == other.verts &&
                    uv == other.uv &&
                    uv2 == other.uv2 &&
                    uv3 == other.uv3 &&
                    uv4 == other.uv4 &&
                    colors == other.colors &&
                    tangents == other.tangents &&
                    normals == other.normals &&
                    assetName == other.assetName) {
                    return true;
                }
            }

            return false;
        }
    }
}