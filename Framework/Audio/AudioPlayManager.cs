using Kuroha.Framework.Singleton;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    /// <summary>
    /// 音频播放管理器
    /// 用于控制音频的播放, 暂停, 停止
    /// 依赖于 AudioSourceManager 和 AudioClipManager
    /// </summary>
    public class AudioPlayManager : Singleton<AudioPlayManager>
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static AudioPlayManager Instance => InstanceBase as AudioPlayManager;

        private AudioSourceManager audioSourceManager;
        private AudioClipManager audioClipManager;

        public void Play(string audioName, bool isLoop = false)
        {
            audioSourceManager ??= new AudioSourceManager(gameObject);
            audioClipManager ??= new AudioClipManager();
            
            Debug.Log(GetType() + "/Play()/ audioName : " + audioName);
            var clip = audioClipManager.GetAudioByName(audioName);
            var source = audioSourceManager.GetIdleAudioSource();
            clip?.Play(source, isLoop);
        }

        public void Stop(string audioName)
        {
            Debug.Log(GetType() + "/Stop()/ audioName : " + audioName);
            audioSourceManager.Stop(audioName);
        }
    }
}