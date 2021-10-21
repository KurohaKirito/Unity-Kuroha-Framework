using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuroha.Util.Editor;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.ModelAnalysisTool
{
    public class ModelAnalysisGUI : UnityEditor.Editor
    {
        /// <summary>
        /// 统计纹理用的 打印数据结构
        /// </summary>
        private readonly struct PrintNode
        {
            public readonly string message;
            public readonly Object obj;

            public PrintNode(string message, Object obj)
            {
                this.message = message;
                this.obj = obj;
            }
        }
        
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
        /// 一键检测备注
        /// </summary>
        private static string oneKeyCheckRemark;

        /// <summary>
        /// 是否是高级模式, 高级模式下可以单独对检查项进行检查
        /// </summary>
        private static bool isAdvanceMode;

        /// <summary>
        /// [GUI] 折叠框
        /// </summary>
        private static bool modelAnalysisDetectFoldout = true;

        /// <summary>
        /// 一键检测按钮的 UI 内容
        /// </summary>
        private static GUIContent oneKeyContent;

        /// <summary>
        /// 初始化标题
        /// </summary>
        private static void InitTitle()
        {
            if (ReferenceEquals(oneKeyContent, null))
            {
                var remarkStringBuilder = new StringBuilder();
                remarkStringBuilder.Append("目前支持一键快速检测的检查项有:");
                remarkStringBuilder.Append("\n　(1) 场景中摄像机的数量.");
                remarkStringBuilder.Append("\n　(2) 场景中引用纹理的尺寸.");
                remarkStringBuilder.Append("\n　(3) 场景中动画状态机的剔除模式设置.");
                remarkStringBuilder.Append("\n　(4) 场景中引用的模型资源的法线导入设置.");
                remarkStringBuilder.Append("\n　(5) 场景中所有隐藏的游戏物体.");

                oneKeyContent = new GUIContent(EditorGUIUtility.IconContent("console.infoicon.sml"))
                {
                    tooltip = remarkStringBuilder.ToString(),
                    text = "  开始检测       "
                };
            }
        }

        /// <summary>
        /// 绘制界面
        /// </summary>
        public static void OnGUI()
        {
            InitTitle();
            
            GUILayout.Space(2 * UI_DEFAULT_MARGIN);

            // 折叠框
            modelAnalysisDetectFoldout = EditorGUILayout.Foldout(modelAnalysisDetectFoldout, "模型资源分析统计工具", true);
            if (modelAnalysisDetectFoldout)
            {
                GUILayout.Space(UI_DEFAULT_MARGIN);
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.Label("1. 一键快速检测.");
                    GUILayout.BeginHorizontal("Box");
                    if (GUILayout.Button(oneKeyContent, GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        CheckCameraCount();
                        CheckTextureSize();
                        CheckAnimatorCullMode();
                        CheckColliderModelNormals();
                        CheckDisableGameObject();
                    }

                    GUILayout.EndHorizontal();


                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    GUILayout.Label("2. 统计当前场景中所有碰撞体组件引用的 Mesh 网格资源的详细面数和顶点数.");
                    GUILayout.BeginVertical("Box");
                    if (GUILayout.Button("统计碰撞 Mesh", GUILayout.Height(UI_BUTTON_HEIGHT),
                        GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        CheckAllCollider();
                    }

                    GUILayout.EndVertical();


                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    GUILayout.Label("3. 统计当前场景中所有渲染类组件引用的 Mesh 网格资源的详细面数和顶点数");
                    GUILayout.BeginVertical("Box");
                    if (GUILayout.Button("统计渲染 Mesh", GUILayout.Height(UI_BUTTON_HEIGHT),
                        GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        CheckAllRenderer();
                    }

                    GUILayout.EndVertical();


                    GUILayout.Space(2 * UI_DEFAULT_MARGIN);
                    GUILayout.Label("4. 高级模式. (高级模式下可以单独执行特定的检查项)");
                    GUILayout.BeginVertical("Box");
                    if (GUILayout.Button(isAdvanceMode ? "关闭高级模式" : "开启高级模式", GUILayout.Height(UI_BUTTON_HEIGHT),
                        GUILayout.Width(UI_BUTTON_WIDTH)))
                    {
                        isAdvanceMode = !isAdvanceMode;
                    }

                    GUILayout.EndVertical();


                    if (isAdvanceMode)
                    {
                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        GUILayout.Label("    (1) 检查当前场景中所有的相机, 并在控制台输出.");
                        GUILayout.BeginVertical("Box");
                        if (GUILayout.Button("相机检查", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CheckCameraCount();
                        }

                        GUILayout.EndVertical();


                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        GUILayout.Label("    (2) 检查当前场景中引用的所有纹理的尺寸, 控制台输出超出最大尺寸的纹理名称.");
                        GUILayout.BeginVertical("Box");
                        if (GUILayout.Button("纹理检查", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CheckTextureSize();
                        }

                        GUILayout.EndVertical();


                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        GUILayout.Label("    (3) 检查当前场景中动画状态机的剔除模式设置, 控制台输出所有未设置为 Cull Completely 的状态机.");
                        GUILayout.BeginVertical("Box");
                        if (GUILayout.Button("动画状态机检查", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CheckAnimatorCullMode();
                        }

                        GUILayout.EndVertical();


                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        GUILayout.Label("    (4) 检查当前场景中引用的所有模型资源的法线导入设置.");
                        GUILayout.BeginVertical("Box");
                        if (GUILayout.Button("法线导入检查", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CheckColliderModelNormals();
                        }

                        GUILayout.EndVertical();


                        GUILayout.Space(UI_DEFAULT_MARGIN);
                        GUILayout.Label("    (5) 检查当前场景中所有隐藏的游戏物体并在控制台输出.");
                        GUILayout.BeginVertical("Box");
                        if (GUILayout.Button("隐藏物体检查", GUILayout.Height(UI_BUTTON_HEIGHT), GUILayout.Width(UI_BUTTON_WIDTH)))
                        {
                            CheckDisableGameObject();
                        }

                        GUILayout.EndVertical();
                    }
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// 统计碰撞用的 Mesh 信息
        /// </summary>
        private static void CheckAllCollider()
        {
            ModelAnalysisTableWindow.Open(true, null, true);
        }

        /// <summary>
        /// 统计渲染用的 Mesh 信息
        /// </summary>
        private static void CheckAllRenderer()
        {
            ModelAnalysisTableWindow.Open(false, null, true);
        }

        /// <summary>
        /// 统计场景中的摄像机数量
        /// </summary>
        private static void CheckCameraCount()
        {
            var cameras = AssetUtil.GetAllComponentsInScene<Camera>(AssetUtil.FindType.All);
            if (cameras.Count >= 2)
            {
                DebugUtil.LogError($"<color='red'>错误: 当前场景中一共有 {cameras.Count} 个摄像机</color>");
                for (var i = 0; i < cameras.Count; i++)
                {
                    DebugUtil.LogError($"第 {i + 1} 个摄像机名称为: {cameras[i].name}", cameras[i]);
                }
            }
            else if (cameras.Count == 1)
            {
                DebugUtil.Log($"当前场景中仅有 1 个摄像机: {cameras[0].name}");
            }
            else if (cameras.Count < 1)
            {
                DebugUtil.Log("当前场景中没有相机");
            }
        }

        /// <summary>
        /// 统计场景中引用的所有超出限制大小的纹理
        /// </summary>
        private static void CheckTextureSize()
        {
            const int WIDTH = 256;
            const int HEIGHT = 256;
            var infos = new List<PrintNode>();

            // 获取所有的 MeshRenderer
            var meshRenderers = AssetUtil.GetAllComponentsInScene<MeshRenderer>(AssetUtil.FindType.All);

            // 找出所有引用纹理尺寸过大的物体
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer != null && meshRenderer.sharedMaterials != null)
                {
                    foreach (var material in meshRenderer.sharedMaterials)
                    {
                        TextureUtil.GetAllTexturesInMaterial(material, out var textureDataList);
                        
                        if (textureDataList.Count > 0)
                        {
                            infos.AddRange(from textureData in textureDataList
                                select textureData.asset into asset where asset != null && (asset.width > WIDTH || asset.height > HEIGHT)
                                select new PrintNode($"纹理尺寸过大: {asset.width} X {asset.height}, {asset.name}", asset));
                        }
                    }
                }
            }
            
            // 打印
            foreach (var info in infos)
            {
                DebugUtil.LogError(info.message, info.obj);
            }
        }

        /// <summary>
        /// 统计场景中所有动画剔除模式设置错误的状态机
        /// </summary>
        private static void CheckAnimatorCullMode()
        {
            var animators = AssetUtil.GetAllComponentsInScene<Animator>(AssetUtil.FindType.All);
            DebugUtil.Log($"目前场景中有 {animators.Count} 个动画组件");

            foreach (var animator in animators)
            {
                if (animator.cullingMode != AnimatorCullingMode.CullCompletely)
                {
                    DebugUtil.LogError("动画的 Cull Mode 设置错误", animator.gameObject);
                }
            }
        }

        /// <summary>
        /// 统计碰撞用模型的法线导入设置
        /// </summary>
        private static void CheckColliderModelNormals()
        {
            var meshColliders = AssetUtil.GetAllComponentsInScene<MeshCollider>(AssetUtil.FindType.All);

            var models = new List<ModelImporter>();
            foreach (var meshCollider in meshColliders)
            {
                var path = AssetDatabase.GetAssetPath(meshCollider.sharedMesh);
                if (string.IsNullOrEmpty(path) == false)
                {
                    var assetImporter = AssetImporter.GetAtPath(path);
                    var model = assetImporter as ModelImporter;
                    models.Add(model);
                }
            }

            foreach (var model in models)
            {
                if (model != null && model.importNormals != ModelImporterNormals.None)
                {
                    DebugUtil.LogError($"模型未关闭 Normals {model.assetPath.Split('/').Last()}", model);
                }
            }
        }

        /// <summary>
        /// 统计场景中的隐藏游戏物体
        /// </summary>
        private static void CheckDisableGameObject()
        {
            var objects = AssetUtil.GetAllTransformInScene(AssetUtil.FindType.DisableOnly);
            if (objects.Count > 0)
            {
                DebugUtil.LogError($"场景中一共有 {objects.Count} 个隐藏物体!");
                foreach (var obj in objects)
                {
                    DebugUtil.LogError($"游戏物体 {obj.name} 为隐藏状态", obj);
                }
            }
        }
    }
}