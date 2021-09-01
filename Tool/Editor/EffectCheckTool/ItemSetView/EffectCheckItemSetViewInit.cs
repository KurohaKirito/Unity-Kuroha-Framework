using System;
using Kuroha.Tool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.Editor.EffectCheckTool.GUI;
using Kuroha.Tool.Editor.EffectCheckTool.ItemListView;

namespace Kuroha.Tool.Editor.EffectCheckTool.ItemSetView
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
                switch (itemInfo.assetsType)
                {
                    case EffectToolData.AssetsType.Prefab:
                        InitPrefab(itemInfo);
                        break;

                    case EffectToolData.AssetsType.ParticleSystem:
                        InitParticleSystem(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Animator:
                        InitAnimator(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Mesh:
                        InitMesh(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Texture:
                        InitTexture(itemInfo);
                        break;

                    case EffectToolData.AssetsType.Model:
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
                EffectCheckItemSetViewWindow.itemInfo = new CheckItemInfo(
                    string.Empty, string.Empty, EffectToolData.AssetsType.Mesh, 0, string.Empty, string.Empty,
                    string.Empty, 0, true, false, true, string.Empty);
            }
        }
        
        /// <summary>
        /// 初始化 Mesh 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitMesh(CheckItemInfo info)
        {
            switch ((CheckMesh.CheckOptions)info.checkType)
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
            switch ((CheckTexture.CheckOptions)info.checkType)
            {
                case CheckTexture.CheckOptions.Size:
                    var parameter = info.parameter.Split(EffectCheckItemSetViewWindow.DELIMITER);
                    EffectCheckItemSetViewWindow.ParameterInt1 = Convert.ToInt32(parameter[0]);
                    EffectCheckItemSetViewWindow.ParameterInt2 = Convert.ToInt32(parameter[1]);
                    break;

                case CheckTexture.CheckOptions.ReadWriteEnable:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                case CheckTexture.CheckOptions.MipMaps:
                    EffectCheckItemSetViewWindow.ParameterBool1 = Convert.ToBoolean(info.parameter);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 初始化 Animator 检查项设置页面
        /// </summary>
        /// <param name="info">检查项信息</param>
        private static void InitAnimator(CheckItemInfo info)
        {
            switch ((CheckAnimator.CheckOptions)info.checkType)
            {
                case CheckAnimator.CheckOptions.CullMode:
                    var parameter = Convert.ToInt32(info.parameter);
                    EffectCheckItemSetViewWindow.ParameterInt1 = parameter;
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
            switch ((CheckParticleSystem.CheckOptions)info.checkType)
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
            switch ((CheckPrefab.CheckOptions)info.checkType)
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
            switch ((CheckModel.CheckOptions)info.checkType)
            {
                case CheckModel.CheckOptions.ReadWriteEnable:
                    break;

                case CheckModel.CheckOptions.RendererCastShadow:
                    break;

                case CheckModel.CheckOptions.Normals:
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
            switch ((CheckAsset.CheckOptions)info.checkType)
            {
                case CheckAsset.CheckOptions.AssetName:
                    var parameterAssetName = info.parameter;
                    EffectCheckItemSetViewWindow.ParameterString1 = parameterAssetName;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}