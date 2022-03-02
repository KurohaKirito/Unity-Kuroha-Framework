using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QData
{
    public class QLayerTexture
    {
        public readonly string layer;
        public Texture2D texture;

        public QLayerTexture(string layer, Texture2D texture)
        {
            this.layer = layer;
            this.texture = texture;
        }

        public static List<QLayerTexture> LoadLayerTextureList()
        {
            var layerTextureList = new List<QLayerTexture>();
            
            var customTagIcon = QSettings.Instance().Get<string>(EM_QHierarchySettings.LayerIconList);
            
            var customLayerIconArray = customTagIcon.Split(';');
            
            var layers = new List<string>(UnityEditorInternal.InternalEditorUtility.layers);
            
            for (var i = 0; i < customLayerIconArray.Length - 1; i += 2)
            {
                var layer = customLayerIconArray[i];
                
                if (layers.Contains(layer) == false)
                {
                    continue;
                }
                
                var texturePath = customLayerIconArray[i + 1];

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                
                if (texture != null)
                {
                    var tagTexture = new QLayerTexture(layer, texture);
                    layerTextureList.Add(tagTexture);
                }
            }

            return layerTextureList;
        }

        public static void SaveLayerTextureList(EM_QHierarchySettings hierarchySettings, List<QLayerTexture> layerTextureList)
        {
            var result = new StringBuilder();

            foreach (var layerTexture in layerTextureList)
            {
                var id = AssetDatabase.GetAssetPath(layerTexture.texture.GetInstanceID());
                result.Append($"{layerTexture.layer};{id};");
            }

            QSettings.Instance().Set(hierarchySettings, result.ToString());
        }
    }
}
