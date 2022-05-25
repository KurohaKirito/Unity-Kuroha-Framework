using System;
using System.Collections.Generic;
using UnityEngine;

namespace UPRTools.Editor
{
    [Serializable]
    public class TreeElement
    {
        [SerializeField]
        public int id;

        [SerializeField]
        public string name;

        [SerializeField]
        public int depth;

        [NonSerialized]
        public TreeElement parent;

        [NonSerialized]
        public List<TreeElement> children;

        public bool HasChildren => children != null && children.Count > 0;
        
        public TreeElement()
        {
        }

        protected TreeElement(string name, int depth, int id)
        {
            this.name = name;
            this.id = id;
            this.depth = depth;
        }

        public static TreeElement Init()
        {
            return new TreeElement("", -1, 0);
        }
    }
}