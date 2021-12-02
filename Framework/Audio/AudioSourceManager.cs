using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    public class AudioSourceManager
    {
        /// <summary>
        /// Audio Source Pool
        /// 管理所有的 Audio Source 组件, 这些组件都挂载在 audioSourceOwner 上面
        /// </summary>
        private List<AudioSource> audioSourcePool;
        
        /// <summary>
        /// audio Source 挂载者
        /// </summary>
        private readonly GameObject audioSourceOwner;
        
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
