using DigitalOpus.MB.Core;
using Kuroha.Tool.MeshBakerTool.Core;
using UnityEngine;

namespace Kuroha.Tool.MeshBakerTool
{
    public sealed class MB3_MeshBaker : MB3_MeshBakerCommon
    {
        [SerializeField] private MB3_MeshCombinerSingle meshCombiner = new MB3_MeshCombinerSingle();

        public override MB3_MeshCombiner MeshCombiner => meshCombiner;

        private void BuildSceneMeshObject() {
            meshCombiner.BuildSceneMeshObject();
        }

        public bool ShowHide(GameObject[] gos, GameObject[] deleteGOs) {
            return meshCombiner.ShowHideGameObjects(gos, deleteGOs);
        }

        public void ApplyShowHide() {
            meshCombiner.ApplyShowHide();
        }

        public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true) {
            // if (meshCombiner.outputOption == MeshBaker2OutputOptions.BakeIntoSceneObject ||
            //     meshCombiner.outputOption == MeshBaker2OutputOptions.BakeIntoPrefab &&
            //     meshCombiner.renderType == MeshBakerRenderType.SkinnedMeshRenderer) {
            // 	BuildSceneMeshObject();
            // }

            meshCombiner.name = name + "-mesh";
            return meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
        }

        public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGoInstanceIDs, bool disableRendererInSource = true) {
            // if (meshCombiner.outputOption == MeshBaker2OutputOptions.BakeIntoSceneObject ||
            //     meshCombiner.outputOption == MeshBaker2OutputOptions.BakeIntoPrefab &&
            //     meshCombiner.renderType == MeshBakerRenderType.SkinnedMeshRenderer) {
            //     BuildSceneMeshObject();
            // }
            
            meshCombiner.name = name + "-mesh";
            return meshCombiner.AddDeleteGameObjectsByID(gos, deleteGoInstanceIDs, disableRendererInSource);
        }
    }
}