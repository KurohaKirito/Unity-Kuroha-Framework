using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Framework.Audio.RunTime
{
    /// <summary>
    /// 音频播放器管理器
    /// </summary>
    [Serializable]
    public class AudioSourceManager
    {
        [Header("音频播放器对象池")]
        [SerializeField]
        private List<AudioSource> audioSourcePool;

        [Header("音频播放器挂载者")]
        [SerializeField]
        private GameObject audioSourceOwner;
        
        /// <summary>
        /// 构造方法
        /// </summary>
        public AudioSourceManager(GameObject owner)
        {
            audioSourceOwner = owner;
            Init();
        }

        /// <summary>
        /// 预加载 2 个 AudioSource
        /// </summary>
        private void Init()
        {
            audioSourcePool ??= new List<AudioSource>(5);
            audioSourcePool.Add(audioSourceOwner.AddComponent<AudioSource>());
            audioSourcePool.Add(audioSourceOwner.AddComponent<AudioSource>());
        }

        /// <summary>
        /// 获得闲置的 AudioSource
        /// </summary>
        /// <returns></returns>
        public AudioSource Get()
        {
            // 判断当前有没有闲置的 Audio Source
            foreach (var audioSource in audioSourcePool)
            {
                if (audioSource.isPlaying == false)
                {
                    return audioSource;
                }
            }

            // 新挂载一个 Audio Source
            var newAudioSource = audioSourceOwner.AddComponent<AudioSource>();
            audioSourcePool.Add(newAudioSource);

            return newAudioSource;
        }

        /// <summary>
        /// 停止特定音乐或者音效的播放
        /// </summary>
        public void Stop(string clipName)
        {
            if (string.IsNullOrEmpty(clipName) == false)
            {
                foreach (var audioSource in audioSourcePool)
                {
                    if (audioSource.isPlaying && audioSource.clip.name.Equals(clipName))
                    {
                        audioSource.Stop();
                    }
                }
            }
        }
    }
}
