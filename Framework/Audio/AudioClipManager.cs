using System.Collections.Generic;
using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    /// <summary>
    /// 音频片段管理器, 管理所有的音频资源
    /// </summary>
    public class AudioClipManager
    {
        /// <summary>
        /// 音频数据库路径
        /// </summary>
        private const string AUDIO_DATABASE_PATH = "DataBase/Audio";

        /// <summary>
        /// 音频数据库字典
        /// </summary>
        private readonly Dictionary<string, SingleClip> singleClipDic;
        
        /// <summary>
        /// 构造方法
        /// </summary>
        public AudioClipManager()
        {
            singleClipDic = new Dictionary<string, SingleClip>();

            ReadAudioDataBase();
        }
        
        /// <summary>
        /// 读取所有的 SingleClip
        /// </summary>
        private void ReadAudioDataBase()
        {
            // TODO: 优化点, 用到哪个音频资源就加载哪个音频资源, 而不是一下子全部加载
            var singleClipArray = Resources.LoadAll<SingleClip>(AUDIO_DATABASE_PATH);
            DebugUtil.Log($"一共加载到了 {singleClipArray.Length} 个音频资源");

            foreach (var singleClip in singleClipArray)
            {
                singleClipDic[singleClip.id] = singleClip;
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
