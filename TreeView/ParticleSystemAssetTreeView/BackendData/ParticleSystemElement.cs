using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UPRTools.Editor
{
    [Serializable]
    internal class ParticleSystemElement : TreeElement
    {
        public int    index;
        public string path;
        public Texture2D icon;
        public string displayName;
        public bool   enabled;
        public float  duration;
        public string message;
        
        public ParticleSystemElement() { }
        
        public ParticleSystemElement (string path, string name, int depth, int id) : base (name, depth, id)
        {
            index = id;
            this.path = path;
            var o = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (o != null)
            {
                icon = AssetDatabase.GetCachedIcon(path) as Texture2D;
                displayName = o.name;
                var longestDuration = 0f;
                foreach (var ps in o.GetComponentsInChildren<ParticleSystem>())
                {
                    if (ps.main.duration > longestDuration)
                    {
                        longestDuration = ps.main.duration;
                    }
                }
                duration = longestDuration;

                if (!path.Contains("ToBundle")) {
                    message = "特效在非法目录下，必须要求放在 ToBundle";
                }
            }
            
            enabled = true;
        }
    }
    
    public class ParticleSystemElementsAsset : ScriptableObject
    {
        [SerializeField]
        internal List<ParticleSystemElement> treeElements = new List<ParticleSystemElement> ();
    }
}