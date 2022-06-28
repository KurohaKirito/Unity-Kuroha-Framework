using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Script.Effect.Editor.AssetTool.Util.Unity
{
    public static class UnityUtil
    {
        private static readonly Regex regexReadWrite = new Regex(@"m_IsReadable: [\d]");
        
        public static bool TryGetComponent<T>(this Component source, out T component) where T : Component
        {
            return (component = source.GetComponent<T>()) != null;
        }

        public static bool TryGetComponent<T>(this GameObject source, out T component) where T : Component
        {
            return (component = source.GetComponent<T>()) != null;
        }
        
        public static async Task SetReadable(this Mesh mesh, bool readable)
        {
            string meshData;
            var meshPath = AssetDatabase.GetAssetPath(mesh);
            var meshFullPath = Path.GetFullPath(meshPath);

            using (var reader = new StreamReader(meshFullPath))
            {
                meshData = await reader.ReadToEndAsync();
                var readWriteString = "m_IsReadable: " + (readable ? 1 : 0);
                meshData = regexReadWrite.Replace(meshData, readWriteString);
            }

            using (var writer = new StreamWriter(meshFullPath))
            {
                await writer.WriteAsync(meshData);
            }
        }
        
        public static void RemoveMaterial(this ModelImporter modelImporter)
        {
            var path = AssetDatabase.GetAssetPath(modelImporter);
            var remove = path.Substring(0, path.LastIndexOf('/')) + "/Materials";
        
            #region 删除模型的内嵌材质

            // 开启材质导入, 提取出模型的内嵌材质到 Materials 文件夹
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.SaveAndReimport();

            // 删除提取出来的材质球
            AssetDatabase.DeleteAsset(remove);

            // 修改模型材质引用类型为内嵌材质
            modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
            modelImporter.SaveAndReimport();

            #endregion
        }
    }
}
