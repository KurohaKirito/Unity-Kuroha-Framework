using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Framework.AsyncLoad.RunTime
{
    [CreateAssetMenu(fileName = "AssetConfig_XXX.asset", menuName = "Kuroha/AssetConfig")]
    [Serializable]
    public class ScriptableObjectAsyncLoadAsset : ScriptableObject
    {
        [Tooltip("资源路径")]
        public List<string> assetPaths = new List<string>();
    }
}
