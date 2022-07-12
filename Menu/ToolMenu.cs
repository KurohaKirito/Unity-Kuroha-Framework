using Script.Effect.Editor.AssetTool.Tool.Editor.AssetCheckTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.AssetSearchTool.Searcher;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.GUI;
using Script.Effect.Editor.AssetTool.Tool.Editor.LODBatchTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.ProjectExtender;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Menu {
    public class ToolMenu : MonoBehaviour {
        [MenuItem("GameObject/Scene Tool/LODTool", false, 12)]
        public static void Tool1() {
            LODBatchWindow.Open();
        }

        [MenuItem("Funny/资源检测工具/Asset Check Tool")]
        public static void Tool2() {
            AssetCheckToolWindow.Open();
        }

        [MenuItem("Funny/资源检测工具/飞高高产出检测工具")]
        public static void Tool3() {
            EffectCheckToolGUI.Detect(false, "飞高高产出检测工具");
        }

        [MenuItem("Funny/资源检测工具/快速批量设置工具")]
        public static void Tool4() {
            BatchEditorWindow.Open();
        }

        [MenuItem("Funny/资源检测工具/PickItem检测工具")]
        public static void Tool5() {
            EffectCheckToolGUI.Detect(false, "PickItem检测工具");
        }

        [MenuItem("Funny/资源检测工具/Car LOD 检测工具")]
        public static void Tool6() {
            CarSetLOD.Open();
        }

        [MenuItem("Assets/Scene Batch Tool/Pick Mesh Collider Supernova", false, 12)]
        public static void Tool7() {
            BatchToolInScene.DetachLbpRendererAndRenderer();
        }

        [MenuItem("Funny/资源检测工具/依赖分析工具")]
        public static void Tool8() {
            ReferenceSearcher.OpenWindow();
        }

        [MenuItem("Funny/资源检测工具/网格信息浏览开关")]
        public static void Tool9() {
            MeshReadableShow.switchFlag = !MeshReadableShow.switchFlag;
        }
    }
}
