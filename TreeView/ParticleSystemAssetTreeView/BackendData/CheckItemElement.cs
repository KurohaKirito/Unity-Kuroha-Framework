using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UPRTools.Editor;

namespace UPRTools.Editor {
    [Serializable]
    internal class CheckItemElement : TreeElement {
        public int mainType;
        public Texture2D icon;
        public string path;
        public int fileType;
        public string parameter;
        public int dangerLevel;
        public bool enabled;

        public CheckItemElement() {
        }

        public CheckItemElement(string path, string name, int depth, int id) : base(name, depth, id) {
            mainType = 111111;
            path = "";
            fileType = 3333;
            parameter = "";
            dangerLevel = 333333333;
            enabled = true;
        }
    }
}