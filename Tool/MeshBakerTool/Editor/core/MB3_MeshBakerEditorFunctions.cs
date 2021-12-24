using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using Kuroha.Tool.MeshBakerTool;
using Kuroha.Tool.MeshBakerTool.Core;
using UnityEditor;

public class MB3_MeshBakerEditorFunctions {

    public static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    public static bool BakeIntoCombined(MB3_MeshBakerCommon mom, out bool createdDummyTextureBakeResults)
    {
		MeshBaker2OutputOptions prefabOrSceneObject = mom.MeshCombiner.outputOption;
        createdDummyTextureBakeResults = false;
        if (MB3_MeshCombiner.EVAL_VERSION && prefabOrSceneObject == MeshBaker2OutputOptions.BakeIntoPrefab){
			Debug.LogError("Cannot BakeIntoPrefab with evaluation version.");
			return false;
		}
		if (prefabOrSceneObject != MeshBaker2OutputOptions.BakeIntoPrefab && prefabOrSceneObject != MeshBaker2OutputOptions.BakeIntoSceneObject){
			Debug.LogError("Paramater prefabOrSceneObject must be bakeIntoPrefab or bakeIntoSceneObject");
			return false;
		}

		MB3_TextureBaker tb = mom.GetComponentInParent<MB3_TextureBaker>();
		if (mom.textureBakeResults == null && tb != null){
			mom.textureBakeResults = tb.textureBakeResults;	
		}
        if (mom.textureBakeResults == null)
        {
            if (_OkToCreateDummyTextureBakeResult(mom))
            {
                createdDummyTextureBakeResults = true;
                List<GameObject> gos = mom.GetObjectsToCombine();
                if (mom.GetNumObjectsInCombined() > 0)
                {
                    if (mom.clearBuffersAfterBake) { mom.ClearMesh(); }
                    else
                    {
                        Debug.LogError("'Texture Bake Result' must be set to add more objects to a combined mesh that already contains objects. Try enabling 'clear buffers after bake'");
                        return false;
                    }
                }
                mom.textureBakeResults = MB2_TextureBakeResults.CreateForMaterialsOnRenderer(gos.ToArray(), mom.MeshCombiner.GetMaterialsOnTargetRenderer());
                if (mom.MeshCombiner.LOG_LEVEL >= MB2_LogLevel.debug) { Debug.Log("'Texture Bake Result' was not set. Creating a temporary one. Each material will be mapped to a separate submesh."); }
            }
        }
        MeshBaker2ValidationLevel vl = Application.isPlaying ? MeshBaker2ValidationLevel.Quick : MeshBaker2ValidationLevel.Robust; 
		if (!MB3_MeshBakerRoot.DoCombinedValidate(mom, MeshBakerObjectsToCombineTypes.SceneObjOnly, new MB3_EditorMethods(),vl)) return false;	
		if (prefabOrSceneObject == MeshBaker2OutputOptions.BakeIntoPrefab && 
			mom.resultPrefab == null){
			Debug.LogError("Need to set the Combined Mesh Prefab field. Create a prefab asset, drag an empty game object into it, and drag it to the 'Combined Mesh Prefab' field.");
			return false;
		}
		if (mom.MeshCombiner.resultSceneObject != null &&
						(PrefabUtility.GetPrefabType (mom.MeshCombiner.resultSceneObject) == PrefabType.ModelPrefab ||
						PrefabUtility.GetPrefabType (mom.MeshCombiner.resultSceneObject) == PrefabType.Prefab)) {
			Debug.LogWarning("Result Game Object was a project asset not a scene object instance. Clearing this field.");
			mom.MeshCombiner.resultSceneObject = null;
		}

		mom.ClearMesh();
		if (mom.AddDeleteGameObjects(mom.GetObjectsToCombine().ToArray(),null,false)){
			mom.Apply( UnwrapUV2 );
            if (createdDummyTextureBakeResults)
            {
                Debug.Log(String.Format("Successfully baked {0} meshes each material is mapped to its own submesh.", mom.GetObjectsToCombine().Count));
            } else
            {
                Debug.Log(String.Format("Successfully baked {0} meshes", mom.GetObjectsToCombine().Count));
            }

            
			if (prefabOrSceneObject == MeshBaker2OutputOptions.BakeIntoSceneObject){
				PrefabType pt = PrefabUtility.GetPrefabType(mom.MeshCombiner.resultSceneObject);
				if (pt == PrefabType.Prefab || pt == PrefabType.ModelPrefab){
					Debug.LogError("Combined Mesh Object is a prefab asset. If output option bakeIntoSceneObject then this must be an instance in the scene.");
					return false;
				}
			} else if (prefabOrSceneObject == MeshBaker2OutputOptions.BakeIntoPrefab){
				string prefabPth = AssetDatabase.GetAssetPath(mom.resultPrefab);
				if (prefabPth == null || prefabPth.Length == 0){
					Debug.LogError("Could not save result to prefab. Result Prefab value is not an Asset.");
					return false;
				}
				string baseName = Path.GetFileNameWithoutExtension(prefabPth);
				string folderPath = prefabPth.Substring(0,prefabPth.Length - baseName.Length - 7);		
				string newFilename = folderPath + baseName + "-mesh";
				SaveMeshsToAssetDatabase(mom, folderPath,newFilename);
				

				if (mom.MeshCombiner.renderType == MeshBakerRenderType.SkinnedMeshRenderer){
					Debug.LogWarning("Render type is skinned mesh renderer. " +
							"Can't create prefab until all bones have been added to the combined mesh object " + mom.resultPrefab + 
							" Add the bones then drag the combined mesh object to the prefab.");	
					
				}
				RebuildPrefab(mom);

                MB_Utility.Destroy(mom.MeshCombiner.resultSceneObject);
			} else {
				Debug.LogError("Unknown parameter");
				return false;
			}
		} else
        {
            if (mom.clearBuffersAfterBake) { mom.MeshCombiner.ClearBuffers(); }
            if (createdDummyTextureBakeResults) MB_Utility.Destroy(mom.textureBakeResults);
            return false;
		}
        if (mom.clearBuffersAfterBake) { mom.MeshCombiner.ClearBuffers(); }
        if (createdDummyTextureBakeResults) MB_Utility.Destroy(mom.textureBakeResults);
		return true;
	}

	public static void SaveMeshsToAssetDatabase(MB3_MeshBakerCommon mom, string folderPath, string newFileNameBase){
		if (MB3_MeshCombiner.EVAL_VERSION) return;
		if (mom is MB3_MeshBaker){
			MB3_MeshBaker mb = (MB3_MeshBaker) mom;
			string newFilename = newFileNameBase + ".asset";
			string ap = AssetDatabase.GetAssetPath(((MB3_MeshCombinerSingle) mb.MeshCombiner).GetMesh());
			if (ap == null || ap.Equals("")){
				Debug.Log("Saving mesh asset to " + newFilename);
				AssetDatabase.CreateAsset(((MB3_MeshCombinerSingle) mb.MeshCombiner).GetMesh(), newFilename);
			} else {
				Debug.Log("Mesh is an asset at " + ap);	
			}
		} else if (mom is MB3_MultiMeshBaker){
			MB3_MultiMeshBaker mmb = (MB3_MultiMeshBaker) mom;
			List<MB3_MultiMeshCombiner.CombinedMesh> combiners = ((MB3_MultiMeshCombiner) mmb.MeshCombiner).meshCombiners;
			for (int i = 0; i < combiners.Count; i++){
				string newFilename = newFileNameBase + i + ".asset";
				Mesh mesh = combiners[i].combinedMesh.GetMesh();
				string ap = AssetDatabase.GetAssetPath(mesh);
				if (ap == null || ap.Equals("")){
					Debug.Log("Saving mesh asset to " + newFilename);
					AssetDatabase.CreateAsset(mesh, newFilename);
				} else {
					Debug.Log("Mesh is an asset at " + ap);	
				}			
			}				
		} else {
			Debug.LogError("Argument was not a MB3_MeshBaker or an MB3_MultiMeshBaker.");	
		}
	}
	
	public static void RebuildPrefab(MB3_MeshBakerCommon mom){
		if (MB3_MeshCombiner.EVAL_VERSION) return;
        GameObject prefabRoot = mom.resultPrefab;
        GameObject rootGO = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
        //remove all renderer childeren of rootGO
        Renderer[] rs = rootGO.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rs.Length; i++)
        {
            if (rs[i] != null && rs[i].transform.parent == rootGO.transform)
            {
                MB_Utility.Destroy(rs[i].gameObject);
            }
        }
        if (mom is MB3_MeshBaker){
			MB3_MeshBaker mb = (MB3_MeshBaker) mom;
			MB3_MeshCombinerSingle mbs = (MB3_MeshCombinerSingle) mb.MeshCombiner;
            MB3_MeshCombinerSingle.BuildPrefabHierarchy(mbs, rootGO, mbs.GetMesh());		
		} else if (mom is MB3_MultiMeshBaker){
			MB3_MultiMeshBaker mmb = (MB3_MultiMeshBaker) mom;
			MB3_MultiMeshCombiner mbs = (MB3_MultiMeshCombiner) mmb.MeshCombiner;
			for (int i = 0; i < mbs.meshCombiners.Count; i++){
				MB3_MeshCombinerSingle.BuildPrefabHierarchy(mbs.meshCombiners[i].combinedMesh, rootGO, mbs.meshCombiners[i].combinedMesh.GetMesh(),true);
			}	
		} else {
			Debug.LogError("Argument was not a MB3_MeshBaker or an MB3_MultiMeshBaker.");	
		}
        string prefabPth = AssetDatabase.GetAssetPath(prefabRoot);
        PrefabUtility.ReplacePrefab(rootGO, AssetDatabase.LoadAssetAtPath(prefabPth, typeof(GameObject)), ReplacePrefabOptions.ConnectToPrefab);
        if (mom.MeshCombiner.renderType != MeshBakerRenderType.SkinnedMeshRenderer) { Editor.DestroyImmediate(rootGO); }
    }

	public static void UnwrapUV2(Mesh mesh, float hardAngle, float packingMargin){
			UnwrapParam up = new UnwrapParam();
			UnwrapParam.SetDefaults(out up);
			up.hardAngle = hardAngle;
			up.packMargin = packingMargin;
			Unwrapping.GenerateSecondaryUVSet(mesh,up);
	}

    public static bool _OkToCreateDummyTextureBakeResult(MB3_MeshBakerCommon mom)
    {
        List<GameObject> objsToMesh = mom.GetObjectsToCombine();
        if (objsToMesh.Count == 0)
            return false;
        return true;
    }
}
