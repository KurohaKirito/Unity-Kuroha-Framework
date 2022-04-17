using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Tool.AssetTool.AssetRenameTool.Editor
{
    [Serializable]
    public class RenameStepScriptableObject : ScriptableObject
    {
        [SerializeField]
        public List<RenameStep> steps = new List<RenameStep>();
    }
}
