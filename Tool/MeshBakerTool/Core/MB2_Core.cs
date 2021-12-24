using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace Kuroha.Tool.MeshBakerTool.Core
{
	public delegate void ProgressUpdateDelegate(string msg, float progress);
    public delegate bool ProgressUpdateCancelableDelegate(string msg, float progress);

    public enum MeshBakerObjectsToCombineTypes
    {
		PrefabOnly,
		SceneObjOnly,
		DontCare
	}
	
	public enum MeshBakerOutputOptions
	{
		BakeIntoPrefab,
		BakeMeshInPlace,
		BakeTextureAtlasesOnly,
		BakeIntoSceneObject
	}
	
	public enum MeshBakerRenderType
	{
		MeshRenderer,
		SkinnedMeshRenderer
	}
	
	public enum MeshBaker2OutputOptions
	{
		BakeIntoSceneObject,
		BakeMeshAssetsInPlace,
		BakeIntoPrefab
	}
	
	public enum MeshBaker2LightmapOptions
	{
		PreserveCurrentLightmapping,
		IgnoreUV2,
		CopyUV2Unchanged,
		GenerateNewUV2Layout,
        CopyUV2UnchangedToSeparateRects,
    }

	public enum MeshBaker2PackingAlgorithmEnum
	{
		UnityPackTextures,
		MeshBakerTexturePacker,
		MeshBakerTexturePackerFast,
        MeshBakerTexturePackerHorizontal, //special packing packs all horizontal. makes it possible to use an atlas with tiling textures
        MeshBakerTexturePackerVertical, //special packing packs all Vertical. makes it possible to use an atlas with tiling textures
    }

    public enum MeshBaker2TextureTilingTreatment{
        None,
        ConsiderUVs,
        EdgeToEdgeX,
        EdgeToEdgeY,
        EdgeToEdgeXY, // One image in atlas.
        Unknown,
    }
	
	public enum MeshBaker2ValidationLevel{
		None,
		Quick,
		Robust
	}

	public interface IMeshBaker2EditorMethods
	{
		public void Clear();

		public void RestoreReadFlagsAndFormats(ProgressUpdateDelegate progressInfo);

		public void SetReadWriteFlag(Texture2D tx, bool isReadable, bool addToList);

		public void AddTextureFormat(Texture2D tx, bool isNormalMap);

		public void SaveAtlasToAssetDatabase(Texture2D atlas, ShaderTextureProperty texPropertyName, int atlasNum, Material resMat);
		
		//void SetMaterialTextureProperty(Material target, ShaderTextureProperty texPropName, string texturePath);
		//void SetNormalMap(Texture2D tx);
		public bool IsNormalMap(Texture2D tx);

		public string GetPlatformString();

		public void SetTextureSize(Texture2D tx, int size);

		public bool IsCompressed(Texture2D tx);

		public void CheckBuildSettings(long estimatedAtlasSize);

		public bool CheckPrefabTypes(MeshBakerObjectsToCombineTypes prefabType, List<GameObject> gos);

		public bool ValidateSkinnedMeshes(List<GameObject> mom);

		public void CommitChangesToAssets();

		public void OnPreTextureBake();

		public void OnPostTextureBake();
		
		//Needed because io.writeAllBytes does not exist in webplayer.
		public void Destroy(UnityEngine.Object o);
	}
}
