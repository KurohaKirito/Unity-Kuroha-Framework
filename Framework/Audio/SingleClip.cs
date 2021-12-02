using System;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    [Serializable]
    public class SingleClip : ScriptableObject
    {
        [Header("音频唯一标识")]
        public string id;
        
        [Header("音频资源用途描述")]
        [SerializeField]
        private string describe;
        
        [Header("音频资源")]
        [SerializeField]
        private AudioClip audioClip;

        /// <summary>
        /// 构造方法
        /// </summary>
        public SingleClip(string id, string describe, AudioClip audioClip)
        {
            this.id = id;
            this.describe = describe;
            this.audioClip = audioClip;
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play(AudioSource audioSource, bool isLoop = false)
        {
            audioSource.clip = audioClip;
            audioSource.loop = isLoop;
            audioSource.Play();
        }
        
        /// <summary>
        /// 获取音频资源名称
        /// </summary>
        public string GetClipName()
        {
            return audioClip.name;
        }
    }
}
