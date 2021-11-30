using UnityEngine;

namespace Kuroha.Framework.Audio {
    public class SingleClip {
        private readonly AudioClip clip;

        public SingleClip(AudioClip clip) {
            this.clip = clip;
        }

        public void Play(AudioSource audioSource, bool isLoop = false) {
            audioSource.clip = clip;
            audioSource.loop = isLoop;
            audioSource.Play();
        }
    }
}