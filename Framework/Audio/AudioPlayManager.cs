using Kuroha.Framework.Singleton;
using UnityEngine;

namespace Kuroha.Framework.Audio {
    public class AudioPlayManager : Singleton<AudioPlayManager> {
        public static AudioPlayManager Instance => InstanceBase as AudioPlayManager;

        private AudioSourceManager audioSourceManager;
        private AudioClipManager audioClipManager;

        protected void Awake() {
            audioSourceManager = new AudioSourceManager(gameObject);
            audioClipManager = new AudioClipManager();
        }
        
        public void Play(string audioName, bool isLoop = false)
        {
            Debug.Log(GetType() + "/Play()/ audioName : " + audioName);
            var clip = audioClipManager.GetAudioByName(audioName);
            var source = audioSourceManager.GetIdleAudioSource();
            clip?.Play(source, isLoop);
        }

        public void Stop(string audioName) {
            Debug.Log(GetType() + "/Stop()/ audioName : " + audioName);
            audioSourceManager.Stop(audioName);
        }
    }
}