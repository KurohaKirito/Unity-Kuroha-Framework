using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    /// <summary>
    /// 音频片段管理器, 管理所有的音频资源
    /// </summary>
    [Serializable]
    public class AudioClipManager
    {
        #region 编辑器 API

        #if KUROHA_DEBUG_MODE

        [Header("音频资源个数")] [SerializeField]
        private int singleClipCount;

        [Header("当前全部的音频资源")] [SerializeField]
        private List<Kuroha.Framework.Audio.SingleClip> singleClipList;

        public void InspectorUpdate()
        {
            if (singleClipList.IsNullOrEmpty())
            {
                singleClipList = new List<Kuroha.Framework.Audio.SingleClip>();

                foreach (var singleClip in singleClipDic.Values)
                {
                    singleClipList.Add(singleClip);
                }

                singleClipCount = singleClipList.Count;
            }

            if (singleClipCount != singleClipDic.Count)
            {
                singleClipList.Clear();
            
                foreach (var singleClip in singleClipDic.Values)
                {
                    singleClipList.Add(singleClip);
                }
                
                singleClipCount = singleClipList.Count;
            }
        }

        #endif

        #endregion
        
        /// <summary>
        /// 音频数据库字典
        /// </summary>
        private readonly Dictionary<string, SingleClip> singleClipDic = new Dictionary<string, SingleClip>();

        /// <summary>
        /// [Async] 初始化, 读取所有的 SingleClip
        /// </summary>
        public async Task OnInit()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(SingleClip)}", new[]
            {
                "Assets/Resources/DataBase/Audio/"
            });
            
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                assetPath = assetPath.Replace("Assets/Resources/", "").Replace(".asset", "");
                var singleClipRequest = Resources.LoadAsync<SingleClip>(assetPath);
                await singleClipRequest;

                if (singleClipRequest.asset is SingleClip singleClip)
                {
                    singleClipDic[singleClip.id] = singleClip;
                }
            }
        }

        /// <summary>
        /// 获取 SingleClip
        /// </summary>
        public SingleClip Get(string clipID)
        {
            singleClipDic.TryGetValue(clipID, out var singleClip);
            return singleClip;
        }
    }
}
