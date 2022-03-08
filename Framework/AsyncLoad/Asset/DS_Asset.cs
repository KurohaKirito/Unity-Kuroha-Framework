using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Framework.AsyncLoad.Asset
{
    [CreateAssetMenu(fileName = "AssetConfig_XXX.asset", menuName = "Kuroha/AssetConfig")]
    [Serializable]
    public class DS_Asset : ScriptableObject
    {
        [Tooltip("资源路径")]
        public List<string> assetPaths = new List<string>();
    }
}
