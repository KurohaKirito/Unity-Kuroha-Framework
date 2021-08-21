using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuroha.GUI.Editor;
using Kuroha.Tool.Editor.AssetBatchTool;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_2019_2_OR_NEWER == false
using System.Text.RegularExpressions;
#endif

public static class AutoCheckTool
{
    /// <summary>
    /// 检查项
    /// </summary>
    private static event Action<List<Dictionary<string, string>>> detectItems;

    /// <summary>
    /// 检测结果
    /// </summary>
    private static readonly List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

    /// <summary>
    /// CICD 检测项初始化
    /// </summary>
    private static void Init()
    {
        detectItems += EffectDetect.Detect;
        detectItems += SolidTextureDetect.Detect;
        detectItems += RepeatTextureDetect.Detect;
        
        //detectItems += GetAllFolder;
        //detectItems += DetectFbxUVColors;
        //detectItems += Test1;
        //detectItems += Test2;
    }
    
    // [MenuItem("Kuroha/test")]
    public static void AutoCheck()
    {
        try
        {
            if (detectItems == null)
            {
                Init();
            }
    
            // 检测
            detectItems?.Invoke(results);

            #if UNITY_2019_2_OR_NEWER == false
            
            // 将检测结果序列化为 json 文本
            var resultList = new List<string>();
            foreach (var result in results)
            {
                var jsonUnicode = XUPorterJSON.MiniJSON.jsonEncode(result);
                var jsonStr = Regex.Unescape(jsonUnicode);
                resultList.Add(jsonStr);
            }
    
            // 将 json 文本写入文本文件
            var resultFilePath = StringUtil.Concat(Application.dataPath, "/CICDDetectResult.txt");
            File.WriteAllLines(resultFilePath, resultList);
            
            #endif
            
            Debug.Log("CICD Detect Completed!");
        }
        catch (Exception e)
        {
            Debug.Log($"CICD Detect Error: {e}");
            throw;
        }
    }

    // 统计各个文件夹下 Prefab 的数量, 判断哪些 AssetBundle 包过大
    // ReSharper disable once UnusedMember.Local
    private static void GetAllFolder()
    {
        const string PATH = @"C:\Workspace\Sausage\Assets\Art\Effects\Textures\";
        var detectResult = new Dictionary<DirectoryInfo, int>();
        var allDirectory = new List<DirectoryInfo>
        {
            new DirectoryInfo(PATH)
        };

        for (var index = 0; index < allDirectory.Count; index++)
        {
            // 获取路径中第一层中的子文件夹
            var subDir = allDirectory[index].GetDirectories();

            // 获取路径中第一层中的文件
            var files = allDirectory[index].GetFiles("*.*", SearchOption.TopDirectoryOnly).ToList();
            for (var i = files.Count - 1; i >= 0; i--)
            {
                if (files[i].FullName.IndexOf(".meta", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    files.RemoveAt(i);
                }
            }

            if (subDir.Length > 0 && files.Count > 0)
            {
                // 资源与文件夹同级
                DebugUtil.LogError($"资源与文件夹同级, {allDirectory[index].FullName}");
                allDirectory.AddRange(subDir);
            }
            else if (subDir.Length == 0 && files.Count == 0)
            {
                // 空文件夹
                DebugUtil.LogError($"空文件夹, {allDirectory[index].FullName}");
            }
            else if (subDir.Length > 0 && files.Count == 0)
            {
                // 中间层文件夹
                allDirectory.AddRange(subDir);
            }
            else if (subDir.Length == 0 && files.Count > 0)
            {
                // 底层文件夹
                detectResult.Add(allDirectory[index], files.Count);
            }
        }

        //输出结果
        var result = new List<string>();
        foreach (var key in detectResult.Keys.Where(key => !key.Name.Equals(".git")))
        {
            var dir = key.FullName.Substring(key.FullName.IndexOf("Assets", StringComparison.Ordinal));
            var log = $"路径 {dir} 下有 {detectResult[key]} 个预制体资源";
            result.Add(log);

            var go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dir);
            DebugUtil.Log(log, go);
        }

        // 导出结果
        File.WriteAllLines("C:\\AssetCounter.txt", result);
    }

    // 收集场景中的粒子特效, 判断是否有 sub-emitter 错误
    // ReSharper disable once UnusedMember.Local
    private static void Test1()
    {
        // 获得所有场景的资源路径
        var scenes = AssetDatabase.FindAssets("t:Scene");
        var scenePaths = new string[scenes.Length];
        for (var index = 0; index < scenes.Length; index++)
        {
            scenePaths[index] = AssetDatabase.GUIDToAssetPath(scenes[index]);
        }

        // 遍历场景
        for (var index = 0; index < scenePaths.Length; index++)
        {
            ProgressBar.DisplayProgressBar("进度",
                $"{index + 1}/{scenePaths.Length}", index + 1, scenePaths.Length);
            var path = scenePaths[index];
            if (path.IndexOf("scenes/main", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (path.IndexOf("levelEditor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (path.IndexOf("maps/editor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            Debug.Log($"当前检测的场景是: {path}");
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            var rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var particles = root.GetComponentsInChildren<ParticleSystem>();
                foreach (var particle in particles)
                {
                    if (particle.subEmitters.enabled)
                    {
                        var subEmittersCount = particle.subEmitters.subEmittersCount;
                        Debug.LogError(
                            $"场景: {scene.name}, 根物体: {root.name}, 粒子系统: {particle.name} 启用了 {subEmittersCount} 个 Sub-Emitter");

                        if (subEmittersCount <= 0)
                        {
                            continue;
                        }

                        for (var i = 0; i < subEmittersCount; i++)
                        {
                            // 获取所有的子粒子系统
                            var allSubParticleSystems = particle.GetComponentsInChildren<ParticleSystem>(true);
                            // 获取 SubEmitterSystem 设置
                            var setting = particle.subEmitters.GetSubEmitterSystem(i);

                            var isError = true;
                            if (ReferenceEquals(setting, null) == false)
                            {
                                foreach (var subParticleSystem in allSubParticleSystems)
                                {
                                    if (setting == subParticleSystem)
                                    {
                                        isError = false;
                                    }
                                }
                            }

                            if (isError == false)
                            {
                                continue;
                            }

                            Debug.LogError(
                                $"Sub-EmittersError: 场景: {scene.name}, 根物体: {root.name}, 子物体: {particle.gameObject.name}");
                        }
                    }
                }
            }

            EditorSceneManager.CloseScene(scene, false);
        }
    }

    // 收集指定路径中的 BuffSOBase 类型的数据
    // ReSharper disable once UnusedMember.Local
    private static void Test2()
    {
        var result = new List<string> { "Asset Path,Buff Name,Buff Info,Sync State,Sync Range Distance" };

        // 获取指定路径下的 SO 文件
        var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ToBundle/ScriptableObject/Buff" });
        Debug.Log($"一共查询到了 {guids.Length} 个 SO 文件");

        // foreach (var guid in guids)
        // {
        //     var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        //     var asset = AssetDatabase.LoadAssetAtPath<BuffSOBase>(assetPath);
        //     var buffName = asset.BuffName;
        //     var buffInfo = asset.BuffInfo;
        //     var buffSyncType = asset.SyncState;
        //     var buffSyncRangeDistance = asset.SyncRangeDistance;
        //     result.Add($"{assetPath},{buffName},{buffInfo},{buffSyncType},{buffSyncRangeDistance}");
        // }

        // 导出文件
        File.WriteAllLines("c:/result.csv", result, Encoding.UTF8);
    }
    
    /// <summary>
    /// 检测无引用的纹理 (废弃纹理)
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private static void UnusedTexture()
    {
        var paths = UnusedAssetCleaner.Detect(UnusedAssetCleaner.UnusedAssetType.Texture,
            "Assets/Art/Effects/Textures", true);
    
        foreach (var path in paths) {
            results.Add(new Dictionary<string, string> {
                {"错误名称", "未使用的贴图资源"},
                {"资源路径", path},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "可用工具批量删除(删除前请仔细确认)"}
            });
        }
    }

    /// <summary>
    /// 检测无引用的模型 (废弃模型)
    /// </summary>

    // ReSharper disable once UnusedMember.Local
    private static void UnusedModel() {
        var paths = UnusedAssetCleaner.Detect(UnusedAssetCleaner.UnusedAssetType.Model,
            "Assets/Art/Effects/Models", true);
    
        foreach (var path in paths) {
            var result = new Dictionary<string, string> {
                {"错误名称", "未使用的模型资源"},
                {"资源路径", path},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "可用工具批量删除(删除前请仔细确认)"}
            };
            results.Add(result);
        }
    }

    /// <summary>
    /// 检测无引用的材质球 (废弃材质球)
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private static void UnusedMaterial() {
        var paths = UnusedAssetCleaner.Detect(UnusedAssetCleaner.UnusedAssetType.Material,
            "Assets/Art/Effects/Materials", true);
    
        foreach (var path in paths) {
            var result = new Dictionary<string, string> {
                {"错误名称", "未使用的材质资源"},
                {"资源路径", path},
                {"错误等级", "Error"},
                {"负责人", "傅佳亿"},
                {"备注", "可用工具批量删除(删除前请仔细确认)"}
            };
            results.Add(result);
        }
    }
}