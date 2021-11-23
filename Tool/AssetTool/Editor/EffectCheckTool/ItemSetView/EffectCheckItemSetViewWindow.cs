using System;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.Check;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.GUI;
using Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemListView;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.ItemSetView
{
    /// <summary>
    /// 单个检查项设置页面
    /// </summary>
    public class EffectCheckItemSetViewWindow : EditorWindow
    {
        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static string ParameterString1 { get; set; }
        
        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static int ParameterInt1 { get; set; }

        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static int ParameterInt2 { get; set; }

        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static bool ParameterBool1 { get; set; }

        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static bool ParameterBool2 { get; set; }

        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static bool ParameterBool3 { get; set; }

        /// <summary>
        /// [GUI] 子检查项参数记录
        /// </summary>
        public static bool ParameterBool4 { get; set; }

        /// <summary>
        /// [GUI] 检查项
        /// </summary>
        public static CheckItemInfo itemInfo;

        /// <summary>
        /// [GUI] 标题风格
        /// </summary>
        private static GUIStyle titleStyle;

        /// <summary>
        /// 用于设置参数之间的分隔符
        /// </summary>
        public const char DELIMITER = ';';

        /// <summary>
        /// 文本对齐方式
        /// </summary>
        private TextAnchor oldAnchor;

        /// <summary>
        /// 是否是编辑模式
        /// </summary>
        private static bool isEditMode;

        /// <summary>
        /// 全局默认 margin
        /// </summary>
        private const float UI_DEFAULT_MARGIN = 5;

        /// <summary>
        /// 全局按钮的宽度
        /// </summary>
        private const float UI_BUTTON_WIDTH = 120;

        /// <summary>
        /// 全局按钮的高度
        /// </summary>
        private const float UI_BUTTON_HEIGHT = 25;

        /// <summary>
        /// 打开窗口, 展示特定的检查项
        /// </summary>
        /// <param name="info">检查项</param>
        public static void Open(CheckItemInfo info)
        {
            itemInfo = info;
            isEditMode = info != null;
            EffectCheckItemViewWindow.isRefresh = true;
            var window = GetWindow<EffectCheckItemSetViewWindow>("特效检查项设置");
            window.minSize = new Vector2(500, 470);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {
            titleStyle = new GUIStyle
            {
                fontSize = 24,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };
            
            if (isEditMode == false)
            {
                ParameterString1 = default;
                ParameterInt1 = default;
                ParameterInt2 = default;
                ParameterBool1 = default;
                ParameterBool2 = default;
                ParameterBool3 = default;
                ParameterBool4 = default;
            }
            
            EffectCheckItemSetViewInit.Init(itemInfo, isEditMode);
        }

        /// <summary>
        /// 绘制页面
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Space(UI_DEFAULT_MARGIN);

            #region 标题

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("检查项设置", titleStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(UI_DEFAULT_MARGIN);

            #region 共有 UI

            itemInfo.title = EditorGUILayout.TextField("标题", itemInfo.title);
            itemInfo.assetsType = (EffectToolData.AssetsType)EditorGUILayout.EnumPopup("资源类型", itemInfo.assetsType);
            itemInfo.path = EditorGUILayout.TextField("相对路径", itemInfo.path);
            itemInfo.assetWhiteRegex = EditorGUILayout.TextField("资源白名单【正则】", itemInfo.assetWhiteRegex);
            itemInfo.objectWhiteRegex = EditorGUILayout.TextField("物体白名单【正则】", itemInfo.objectWhiteRegex);
            itemInfo.isCheckSubFile = EditorGUILayout.Toggle("是否检测子目录", itemInfo.isCheckSubFile);
            itemInfo.effectEnable = EditorGUILayout.Toggle("是否参与特效检测", itemInfo.effectEnable);
            itemInfo.cicdEnable = EditorGUILayout.Toggle("是否参与 CICD 检测", itemInfo.cicdEnable);

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region 特有 UI

            switch (itemInfo.assetsType)
            {
                case EffectToolData.AssetsType.ParticleSystem:
                    itemInfo.checkType = EditorGUILayout.Popup("检测内容", itemInfo.checkType, CheckParticleSystem.checkOptions);
                    OnGUI_CheckParticleSystem();
                    break;
                
                case EffectToolData.AssetsType.Mesh:
                    itemInfo.checkType = EditorGUILayout.Popup("检测内容", itemInfo.checkType, CheckMesh.checkOptions);
                    OnGUI_CheckMesh();
                    break;

                case EffectToolData.AssetsType.Texture:
                    itemInfo.checkType = EditorGUILayout.Popup("检测内容", itemInfo.checkType, CheckTexture.checkOptions);
                    OnGUI_CheckTexture();
                    break;

                case EffectToolData.AssetsType.Prefab:
                    itemInfo.checkType = EditorGUILayout.Popup("检测内容", itemInfo.checkType, CheckPrefab.checkOptions);
                    OnGUI_CheckPrefab();
                    break;

                case EffectToolData.AssetsType.Model:
                    itemInfo.checkType = EditorGUILayout.Popup("检测内容", itemInfo.checkType, CheckModel.checkOptions);
                    OnGUI_CheckModel();
                    break;

                case EffectToolData.AssetsType.Asset:
                    itemInfo.checkType = EditorGUILayout.Popup("检测内容", itemInfo.checkType, CheckAsset.checkOptions);
                    OnGUI_CheckAsset();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            #region 共有 UI

            itemInfo.dangerLevel =
                EditorGUILayout.Popup("危险级别", itemInfo.dangerLevel, EffectCheckItemSetView.dangerLevelOptions);
            itemInfo.remark = EditorGUILayout.TextField("备注", itemInfo.remark, GUILayout.Height(120));

            #endregion

            GUILayout.FlexibleSpace();

            #region 保存 与 取消

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("保存", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2)))
            {
                EffectCheckItemView.SaveCheckItem(new CheckItemInfo(itemInfo), isEditMode);
                Close();
            }

            GUILayout.EndVertical();

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("取消", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH / 2)))
            {
                Close();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(2 * UI_DEFAULT_MARGIN);
        }

        /// <summary>
        /// 绘制 ParticleSystem 检查页面
        /// </summary>
        private static void OnGUI_CheckParticleSystem()
        {
            var modeType = (CheckParticleSystem.CheckOptions)itemInfo.checkType;
            var oldAlignment = UnityEngine.GUI.skin.label.alignment;

            switch (modeType)
            {
                case CheckParticleSystem.CheckOptions.SubEmittersError:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 检查 Sub-Emitters 绑定子节点错误特效");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckParticleSystem.CheckOptions.RenderMode:
                    ParameterInt1 = EditorGUILayout.Popup("渲染方式: ", ParameterInt1, CheckParticleSystem.renderModeOptions);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;

                case CheckParticleSystem.CheckOptions.Prewarm:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 特效需要关闭预热");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckParticleSystem.CheckOptions.CastShadows:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 关闭特效阴影投射");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    itemInfo.parameter = "false";
                    break;

                case CheckParticleSystem.CheckOptions.ReceiveShadows:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 关闭特效阴影接收");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    itemInfo.parameter = "false";
                    break;

                case CheckParticleSystem.CheckOptions.MeshTrisLimit:
                    ParameterInt1 = EditorGUILayout.IntField("面数限制: ", ParameterInt1);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;

                case CheckParticleSystem.CheckOptions.MeshUV:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("禁用属性列表: uv3, uv4, Tangent, Normal");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckParticleSystem.CheckOptions.CollisionAndTrigger:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 粒子特效强制需要关闭 Collision 以及 Trigger");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckParticleSystem.CheckOptions.ZeroSurface:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 粒子系统的发射器类型为 Mesh 时, 必须指定 Mesh!");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 绘制 Texture 检查页面
        /// </summary>
        private static void OnGUI_CheckTexture()
        {
            var modeType = (CheckTexture.CheckOptions)itemInfo.checkType;

            switch (modeType)
            {
                case CheckTexture.CheckOptions.Size:
                    ParameterInt1 = EditorGUILayout.Popup("最大长", ParameterInt1, CheckTexture.sizeOptions);
                    ParameterInt2 = EditorGUILayout.Popup("最大宽", ParameterInt2, CheckTexture.sizeOptions);
                    itemInfo.parameter = $"{ParameterInt1}{DELIMITER}{ParameterInt2}";
                    break;

                case CheckTexture.CheckOptions.ReadWriteEnable:
                    ParameterBool1 = EditorGUILayout.Toggle("开启 Read Write", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;

                case CheckTexture.CheckOptions.MipMaps:
                    ParameterBool1 = EditorGUILayout.Toggle("开启 Mip Maps", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 绘制 Mesh 检查页面
        /// </summary>
        private static void OnGUI_CheckMesh()
        {
            var modeType = (CheckMesh.CheckOptions)itemInfo.checkType;

            switch (modeType)
            {
                case CheckMesh.CheckOptions.MeshUV:
                    ParameterBool1 = EditorGUILayout.Toggle("禁用 uv2", ParameterBool1);
                    ParameterBool2 = EditorGUILayout.Toggle("禁用 uv3", ParameterBool2);
                    ParameterBool3 = EditorGUILayout.Toggle("禁用 uv4", ParameterBool3);
                    ParameterBool4 = EditorGUILayout.Toggle("禁用 colors", ParameterBool4);
                    itemInfo.parameter = $"{ParameterBool1}{DELIMITER}{ParameterBool2}{DELIMITER}{ParameterBool3}{DELIMITER}{ParameterBool4}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 绘制 Prefab 检查页面
        /// </summary>
        private static void OnGUI_CheckPrefab()
        {
            var modeType = (CheckPrefab.CheckOptions)itemInfo.checkType;
            var oldAlignment = UnityEngine.GUI.skin.label.alignment;

            switch (modeType)
            {
                case CheckPrefab.CheckOptions.ObjectName:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    ParameterBool1 = EditorGUILayout.Toggle("中文", ParameterBool1);
                    ParameterBool2 = EditorGUILayout.Toggle("空格", ParameterBool2);
                    itemInfo.parameter = $"{ParameterBool1}{DELIMITER}{ParameterBool2}";
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 命名不能有空格 & 中文");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckPrefab.CheckOptions.DisableObject:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 检查预制体中是否有 Disable 的物体");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckPrefab.CheckOptions.ForbidCollision:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 预制体不能有碰撞");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckPrefab.CheckOptions.TextureSize:
                    ParameterInt1 = EditorGUILayout.Popup("最大长", ParameterInt1, CheckPrefab.textureSizeOptions);
                    ParameterInt2 = EditorGUILayout.Popup("最大宽", ParameterInt2, CheckPrefab.textureSizeOptions);
                    itemInfo.parameter = $"{ParameterInt1}{DELIMITER}{ParameterInt2}";
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 检查预制体引用纹理的尺寸大小");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckPrefab.CheckOptions.MotionVectors:
                    ParameterBool1 = EditorGUILayout.Toggle("开启 Motion Vectors", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;
                
                case CheckPrefab.CheckOptions.DynamicOcclusion:
                    ParameterBool1 = EditorGUILayout.Toggle("开启 Dynamic Occlusion", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;
                
                case CheckPrefab.CheckOptions.ForbidParticleSystem:
                    ParameterBool1 = EditorGUILayout.Toggle("禁用 Particle System", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;
                
                case CheckPrefab.CheckOptions.CastShadows:
                    ParameterInt1 = EditorGUILayout.Popup("Cast Shadows: ", ParameterInt1, CheckPrefab.castShadowsOptions);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;
                
                case CheckPrefab.CheckOptions.LightProbes:
                    ParameterInt1 = EditorGUILayout.Popup("Light Probes: ", ParameterInt1, CheckPrefab.lightProbesOptions);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;
                
                case CheckPrefab.CheckOptions.ReflectionProbes:
                    ParameterInt1 = EditorGUILayout.Popup("Reflection Probes: ", ParameterInt1, CheckPrefab.reflectionProbesOptions);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;

                case CheckPrefab.CheckOptions.AnimatorCullMode:
                    ParameterInt1 = EditorGUILayout.Popup("Animator Cull Mode: ", ParameterInt1, CheckPrefab.animatorCullModeOptions);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 绘制 Model 检查页面
        /// </summary>
        private static void OnGUI_CheckModel()
        {
            var modeType = (CheckModel.CheckOptions)itemInfo.checkType;
            var oldAlignment = UnityEngine.GUI.skin.label.alignment;

            switch (modeType)
            {
                case CheckModel.CheckOptions.ReadWriteEnable:
                    GUILayout.BeginHorizontal();
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 关闭 Read Write Enable");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    GUILayout.EndHorizontal();
                    break;

                case CheckModel.CheckOptions.Normals:
                    GUILayout.BeginHorizontal();
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 仅作为碰撞用的模型不需要开启 Normals 平滑处理");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    GUILayout.EndHorizontal();
                    break;
                
                case CheckModel.CheckOptions.OptimizeMesh:
                    ParameterBool1 = EditorGUILayout.Toggle("开启 Optimize Mesh", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;
                
                case CheckModel.CheckOptions.MeshCompression:
                    ParameterInt1 = EditorGUILayout.Popup("网格压缩等级", ParameterInt1, CheckModel.meshCompressionOptions);
                    itemInfo.parameter = ParameterInt1.ToString();
                    break;
                
                case CheckModel.CheckOptions.WeldVertices:
                    ParameterBool1 = EditorGUILayout.Toggle("开启 Weld Vertices", ParameterBool1);
                    itemInfo.parameter = $"{ParameterBool1}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 绘制 Asset 检查页面
        /// </summary>
        private static void OnGUI_CheckAsset()
        {
            var modeType = (CheckAsset.CheckOptions)itemInfo.checkType;
            var oldAlignment = UnityEngine.GUI.skin.label.alignment;

            switch (modeType)
            {
                case CheckAsset.CheckOptions.AssetName:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    ParameterString1 = EditorGUILayout.TextField("命名规则(正则)", ParameterString1);
                    itemInfo.parameter = ParameterString1;
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 资源命名规则检测");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;

                case CheckAsset.CheckOptions.FolderName:
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    ParameterString1 = EditorGUILayout.TextField("命名规则(正则)", ParameterString1);
                    itemInfo.parameter = ParameterString1;
                    UnityEngine.GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label("描述: 文件夹命名规则检测");
                    UnityEngine.GUI.skin.label.alignment = oldAlignment;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
