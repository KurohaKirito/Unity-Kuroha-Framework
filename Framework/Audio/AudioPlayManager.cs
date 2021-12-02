using Kuroha.Framework.Singleton;
using Kuroha.Util.RunTime;

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

        /// <summary>
        /// 音频播放器
        /// </summary>
        private AudioSourceManager audioSourceManager;
        
        /// <summary>
        /// 音频资源
        /// </summary>
        private AudioClipManager audioClipManager;

        /// <summary>
        /// 播放
        /// </summary>
        public void Play(string clipID, bool isLoop = false)
        {
            audioSourceManager ??= new AudioSourceManager(gameObject);
            audioClipManager ??= new AudioClipManager();
            
            var clip = audioClipManager.Get(clipID);
            var source = audioSourceManager.Get();

            if (ReferenceEquals(clip, null))
            {
                DebugUtil.LogError("找不到指定 ID 的音频资源, 请检查音频数据库!", null, "red");
            }
            else
            {
                clip.Play(source, isLoop);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop(string clipID)
        {
            var clip = audioClipManager.Get(clipID);
            
            if (ReferenceEquals(clip, null))
            {
                DebugUtil.LogError("找不到指定 ID 的音频资源, 请检查音频数据库!", null, "red");
            }
            else
            {
                audioSourceManager.Stop(clip.GetClipName());
            }
        }

        /// <summary>
        /// 内存卸载
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            audioSourceManager = null;
            audioClipManager = null;
        }
    }
}
