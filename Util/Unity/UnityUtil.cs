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
    }
}