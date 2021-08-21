using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuroha.GUI.Editor;
using Kuroha.Util.Release;
using UnityEditor;
using UnityEngine;

// 检测特定路径下 fbx 模型文件的 mesh 网格中的 uv2 uv3 uv4 colors 信息
public class FbxUVColorsDetect
{
    private const string FOLDER_PARTICLE = "Assets/Scenes/Models/CombatIsland/Building";
    
    public void Detect()
    {
        // 获取相对目录下所有的预制体
        var guids = AssetDatabase.FindAssets("t:Model", new[] {FOLDER_PARTICLE});
        var assetPaths = new List<string>(guids.Select(AssetDatabase.GUIDToAssetPath));

        #region 剥离路径和文件名并打印出来

        var allFBXPath = new List<string>();

        var allFBXName = new List<string>();
            foreach (var file in assetPaths)
        {
            var fileName = file.Split('/').Last();
            fileName = fileName.Split('\\').Last();
            allFBXPath.Add(file);
            allFBXName.Add(fileName);
        }

        File.WriteAllLines(@"C:\printVirtual.md", allFBXName);

        #endregion

        DebugUtil.Log($"待检测文件一共有 {allFBXPath.Count} 个!");

        // 开始检测
        var fixedCountUV2 = 0;
        var fixedCountUV3 = 0;
        var fixedCountUV4 = 0;
        var fixedCountColors = 0;
        var result = new List<string>();

        const string PRE = @"C:\Workspace\Sausage\";
            for (var i = 0; i<allFBXPath.Count;
        i++)
        {
            if (ProgressBar.DisplayProgressBarCancel($"修复: {allFBXName[i]}", $"{i} / {allFBXPath.Count}",
                i, allFBXPath.Count)) {
                DebugUtil.Log($"一共检测出 {fixedCountUV2} 个 UV2");
                DebugUtil.Log($"一共检测出 {fixedCountUV3} 个 UV3");
                DebugUtil.Log($"一共检测出 {fixedCountUV4} 个 UV4");
                DebugUtil.Log($"一共检测出 {fixedCountColors} 个 Colors");
                return;
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(allFBXPath[i].Substring(PRE.Length));

            // 检测 MeshFilter
            var meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
            foreach (var meshFilter in meshFilters)
            {
                var mesh = meshFilter.sharedMesh;

                if (mesh.uv2.Length > 0) {
                    ++fixedCountUV2;
                    var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 uv2!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }

                if (mesh.uv3.Length > 0) {
                    ++fixedCountUV3;
                    var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 uv3!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }

                if (mesh.uv4.Length > 0) {
                    ++fixedCountUV4;
                    var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 uv4!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }

                if (mesh.colors.Length > 0) {
                    ++fixedCountColors;
                    var log = $"MeshFilter: 模型 {allFBXPath[i].Substring(PRE.Length)} 中的 {mesh.name} 具有 colors!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }
            }

            // 检测 SkinnedMeshRenderer
            var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                var mesh = skinnedMeshRenderer.sharedMesh;

                if (mesh.uv2.Length > 0) {
                    ++fixedCountUV2;
                    var log = $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有uv2!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }

                if (mesh.uv3.Length > 0) {
                    ++fixedCountUV3;
                    var log = $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有uv3!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }

                if (mesh.uv4.Length > 0) {
                    ++fixedCountUV4;
                    var log = $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有uv4!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }

                if (mesh.colors.Length > 0) {
                    ++fixedCountColors;
                    var log =
                        $"SkinnedMeshRenderer: 模型{allFBXPath[i].Substring(PRE.Length)}中的{mesh.name}具有colors!";
                    result.Add(log);
                    DebugUtil.Log(log, mesh);
                }
            }
        }

        // 统计结果
        DebugUtil.Log($"一共检测出 {fixedCountUV2} 个 UV2");
        DebugUtil.Log($"一共检测出 {fixedCountUV3} 个 UV3");
        DebugUtil.Log($"一共检测出 {fixedCountUV4} 个 UV4");
        DebugUtil.Log($"一共检测出 {fixedCountColors} 个 Colors");

        // 输出结果到文件中
        File.WriteAllLines(@"C:\printCollider.md", result);
    }

    public List<Dictionary<string, string>> GetResult() {
        throw new System.NotImplementedException();
    }
}
