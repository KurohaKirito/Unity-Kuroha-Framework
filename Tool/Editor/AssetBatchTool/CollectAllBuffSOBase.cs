using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Editor.AssetBatchTool
{
    // 收集指定路径中的 BuffSOBase 类型的数据
    public class CollectAllBuffSoBase
    {
        private static void Collect()
        {
            var result = new List<string> { "Asset Path,Buff Name,Buff Info,Sync State,Sync Range Distance" };

            // 获取指定路径下的 SO 文件
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ToBundle/ScriptableObject/Buff" });
            Debug.Log($"一共查询到了 {guids.Length} 个 SO 文件");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<BuffSOBase>(assetPath);
                var buffName = asset.BuffName;
                var buffInfo = asset.BuffInfo;
                var buffSyncType = asset.SyncState;
                var buffSyncRangeDistance = asset.SyncRangeDistance;
                result.Add($"{assetPath},{buffName},{buffInfo},{buffSyncType},{buffSyncRangeDistance}");
            }

            // 导出文件
            File.WriteAllLines("c:/result.csv", result, Encoding.UTF8);
        }
    }
}