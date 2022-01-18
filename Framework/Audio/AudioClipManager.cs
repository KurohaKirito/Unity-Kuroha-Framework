using System;
using System.Collections.Generic;
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
            singleClipList ??= new List<Kuroha.Framework.Audio.SingleClip>();
            
            if (singleClipList.Count <= 0)
            {
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
        /// 初始化, 读取所有的 SingleClip
        /// </summary>
        public void OnInit()
        {
            var assets = Resources.LoadAll<SingleClip>("DataBase/Audio");
            foreach (var asset in assets)
            {
                singleClipDic[asset.id] = asset;
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
