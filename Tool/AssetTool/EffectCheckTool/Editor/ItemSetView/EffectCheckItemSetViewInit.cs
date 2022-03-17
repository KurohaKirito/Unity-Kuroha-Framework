using System;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Check;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.Check.Other;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.GUI;
using Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemListView;

namespace Kuroha.Tool.AssetTool.EffectCheckTool.Editor.ItemSetView
{
    /// <summary>
    /// 初始化类
    /// </summary>
    public static class EffectCheckItemSetViewInit
    {
        /// <summary>
        /// 初始化检查项设置页面
        /// </summary>
        /// <param name="itemInfo">检查项的详细信息</param>
        /// <param name="isEditMode">是否是编辑模式</param>
        public static void Init(CheckItemInfo itemInfo, bool isEditMode)
        {
            if (isEditMode)
            {
                switch (itemInfo.checkAssetType)
                {
                    case EffectToolData.AssetsType.TextureImporter:
                        InitSettingTextureImporter(itemInfo);
                        break;
                    
                    case EffectToolData.AssetsType.Prefab:
                        InitPrefab(itemInfo);
                        break;

                    case EffectToolData.AssetsType.ParticleSystem:
                        InitParticleSystem(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Mesh:
                        InitMesh(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Texture:
                        InitTexture(itemInfo);
                        break;

                    case EffectToolData.AssetsType.ModelImporter:
                        InitModel(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Asset:
                        InitAsset(itemInfo);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                EffectCheckItemSetViewWindow.itemInfo = new CheckItemInfo(string.Empty, string.Empty, EffectToolData.AssetsType.Mesh,
                    0, 0, string.Empty, string.Empty, string.Empty, string.Empty,
                    0, true, false, true, string.Empty);
            }
        }
        
        /// <summary>
        /// 初始化 Texture 检查项设置页面
        /// </summary>
        private static void InitSettingTextureImporter(CheckItemInfo info)
        {
            switch ((CheckTextureImporter.EM_CheckOption)info.checkOption)
            {
                case CheckTextureImporter.EM_CheckOption.ImporterSize:
                    var parameterImporterSize = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(parameterImporterSize[0]);
                    EffectCheckItemSetViewWindow.ParameterInt2 = Convert.ToInt32(parameterImporterSize[1]);
                    break;

                case CheckTextureImporter.EM_CheckOption.ReadWriteEnable:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckTextureImporter.EM_CheckOption.MipMaps:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;
                
                case CheckTextureImporter.EM_CheckOption.CompressFormat:
                    var parameterCompressFormat = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(parameterCompressFormat[0]);
                    EffectCheckItemSetViewWindow.ParameterInt2 = Convert.ToInt32(parameterCompressFormat[1]);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 初始化 Mesh 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitMesh(CheckItemInfo info)
        {
            switch ((CheckMesh.CheckOptions)info.checkOption)
            {
                case CheckMesh.CheckOptions.MeshUV:
                    var parameter = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);

                    if (parameter.Length >= 1 && bool.TryParse(parameter[0], out var flag1))
                    {
                        EffectCheckItemSetViewWindow.ParameterBool1 = flag1;
                    }

                    if (parameter.Length >= 2 && bool.TryParse(parameter[1], out var flag2))
                    {
                        EffectCheckItemSetViewWindow.ParameterBool2 = flag2;
                    }

                    if (parameter.Length >= 3 && bool.TryParse(parameter[2], out var flag3))
                    {
                        EffectCheckItemSetViewWindow.ParameterBool3 = flag3;
                    }

                    if (parameter.Length >= 4 && bool.TryParse(parameter[3], out var flag4))
                    {
                        EffectCheckItemSetViewWindow.ParameterBool4 = flag4;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 初始化 Texture 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitTexture(CheckItemInfo info)
        {
            switch ((CheckTextureImporter.EM_CheckOption)info.checkOption)
            {
                case CheckTextureImporter.EM_CheckOption.ImporterSize:
                    var parameter = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(parameter[0]);
                    EffectCheckItemSetViewWindow.ParameterInt2 = Convert.ToInt32(parameter[1]);
                    break;

                case CheckTextureImporter.EM_CheckOption.ReadWriteEnable:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckTextureImporter.EM_CheckOption.MipMaps:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;
                
                case CheckTextureImporter.EM_CheckOption.CompressFormat:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 初始化 ParticleSystem 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitParticleSystem(CheckItemInfo info)
        {
            switch ((CheckParticleSystem.CheckOptions)info.checkOption)
            {
                case CheckParticleSystem.CheckOptions.RenderMode:
                    var parameterRenderMode = Convert.ToInt32(info.parameter);
                    EffectCheckItemSetViewWindow.ParameterInt1 = parameterRenderMode;
                    break;

                case CheckParticleSystem.CheckOptions.MeshTrisLimit:
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(info.parameter);
                    break;

                case CheckParticleSystem.CheckOptions.CastShadows:
                    break;

                case CheckParticleSystem.CheckOptions.ReceiveShadows:
                    break;

                case CheckParticleSystem.CheckOptions.MeshUV:
                    break;

                case CheckParticleSystem.CheckOptions.CollisionAndTrigger:
                    break;

                case CheckParticleSystem.CheckOptions.Prewarm:
                    break;

                case CheckParticleSystem.CheckOptions.SubEmittersError:
                    break;

                case CheckParticleSystem.CheckOptions.ZeroSurface:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 初始化 Prefab 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitPrefab(CheckItemInfo info)
        {
            switch ((CheckPrefab.CheckOptions)info.checkOption)
            {
                case CheckPrefab.CheckOptions.ObjectName:
                    var parameterObjectName = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
                    if (parameterObjectName.Length >= 1 && bool.TryParse(parameterObjectName[0], out var flag1))
                    {
                        EffectCheckItemSetViewWindow.ParameterBool1 = flag1;
                    }

                    if (parameterObjectName.Length >= 2 && bool.TryParse(parameterObjectName[1], out var flag2))
                    {
                        EffectCheckItemSetViewWindow.ParameterBool2 = flag2;
                    }

                    break;

                case CheckPrefab.CheckOptions.DisableObject:
                    break;

                case CheckPrefab.CheckOptions.ForbidCollision:
                    break;

                case CheckPrefab.CheckOptions.TextureSize:
                    var parameterTextureSize = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(parameterTextureSize[0]);
                    EffectCheckItemSetViewWindow.ParameterInt2 = Convert.ToInt32(parameterTextureSize[1]);
                    break;

                case CheckPrefab.CheckOptions.MotionVectors:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckPrefab.CheckOptions.DynamicOcclusion:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckPrefab.CheckOptions.ForbidParticleSystem:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckPrefab.CheckOptions.CastShadows:
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(info.parameter);
                    break;

                case CheckPrefab.CheckOptions.LightProbes:
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(info.parameter);
                    break;

                case CheckPrefab.CheckOptions.ReflectionProbes:
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(info.parameter);
                    break;

                case CheckPrefab.CheckOptions.AnimatorCullMode:
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(info.parameter);
                    break;
                
                case CheckPrefab.CheckOptions.LODGroupRenderers:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 初始化 Model 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitModel(CheckItemInfo info)
        {
            switch ((CheckModelImporter.EM_CheckOption)info.checkOption)
            {
                case CheckModelImporter.EM_CheckOption.ReadWriteEnable:
                    break;

                case CheckModelImporter.EM_CheckOption.Normals:
                    break;

                case CheckModelImporter.EM_CheckOption.MeshOptimize:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckModelImporter.EM_CheckOption.MeshCompression:
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(info.parameter);
                    break;

                case CheckModelImporter.EM_CheckOption.WeldVertices:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 初始化 Asset 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitAsset(CheckItemInfo info)
        {
            switch ((CheckAsset.CheckOptions)info.checkOption)
            {
                case CheckAsset.CheckOptions.AssetName:
                    var parameterAssetName = info.parameter;
                    EffectCheckItemSetViewWindow.ParameterString1 = parameterAssetName;
                    break;

                case CheckAsset.CheckOptions.FolderName:
                    var parameterFolderName = info.parameter;
                    EffectCheckItemSetViewWindow.ParameterString1 = parameterFolderName;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
