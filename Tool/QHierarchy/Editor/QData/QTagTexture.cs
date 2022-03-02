using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.QHierarchy.Editor.QData
{
    public class QTagTexture
    {
        public readonly string tag;
        public Texture2D texture;

        public QTagTexture(string tag, Texture2D texture)
        {
            this.tag = tag;
            this.texture = texture;
        }

        public static List<QTagTexture> LoadTagTextureList()
        {
            var tagTextureList = new List<QTagTexture>();
            
            var customTagIcon = QSettings.Instance().Get<string>(EM_QHierarchySettings.TagIconList);
            
            var customTagIconArray = customTagIcon.Split(';');
            
            var tags = new List<string>(UnityEditorInternal.InternalEditorUtility.tags);
            
            for (var i = 0; i < customTagIconArray.Length - 1; i += 2)
            {
                var tag = customTagIconArray[i];
                
                if (tags.Contains(tag) == false)
                {
                    continue;
                }
                
                var texturePath = customTagIconArray[i + 1];

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                
                if (texture != null)
                {
                    var tagTexture = new QTagTexture(tag, texture);
                    
                    tagTextureList.Add(tagTexture);
                }
            }

            return tagTextureList;
        }

        public static void SaveTagTextureList(EM_QHierarchySettings hierarchySettings, List<QTagTexture> tagTextureList)
        {
            var result = new StringBuilder();

            foreach (var tagTexture in tagTextureList)
            {
                var id = AssetDatabase.GetAssetPath(tagTexture.texture.GetInstanceID());
                result.Append($"{tagTexture.tag};{id};");
            }

            QSettings.Instance().Set(hierarchySettings, result.ToString());
        }
    }
}
