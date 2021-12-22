using Script.Effect.Editor.AssetTool.Tool.Editor.AssetBatchTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.AssetCheckTool;
using Script.Effect.Editor.AssetTool.Tool.Editor.EffectCheckTool.GUI;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Menu {
    public class ToolMenu : MonoBehaviour {
        [MenuItem("GameObject/Scene Batch Tool/LODTool", false, 12)]
        public static void Tool1() {
            LodBatchWindow.Open();
        }
        
        [MenuItem("Funny/资源检测工具/Asset Check Tool")]
        public static void Tool2() {
            AssetCheckToolWindow.Open();
        }

        [MenuItem("Funny/资源检测工具/飞高高产出检测工具")]
        public static void Tool3() {
            EffectCheckToolGUI.Detect(false, "飞高高产出检测工具");
        }
        
        [MenuItem("Assets/Scene Batch Tool/Pick Mesh Collider", false, 12)]
        public static void Tool4() {
            BatchToolInScene.PickUpAllCollider();
        }
    }
}