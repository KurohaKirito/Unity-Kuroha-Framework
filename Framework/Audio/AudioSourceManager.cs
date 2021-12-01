using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    public class AudioSourceManager
    {
        private List<AudioSource> allSourcesLst;
        private readonly GameObject owner;

        public AudioSourceManager(GameObject owner)
        {
            this.owner = owner;
            Init();
        }

        /// <summary>
        /// 初始化列表，并预加载两个 AudioSource
        /// </summary>
        private void Init()
        {
            allSourcesLst = new List<AudioSource>();

            for (var i = 0; i < 2; i++)
            {
                var item = owner.AddComponent<AudioSource>();
                allSourcesLst.Add(item);
            }
        }

        /// <summary>
        /// 获得闲置的 AudioSource
        /// </summary>
        /// <returns></returns>
        public AudioSource GetIdleAudioSource()
        {
            //Debug.Log(GetType() + "/GetIdleAudioSource()/");

            foreach (var audioSource in allSourcesLst.Where(audioSource => audioSource.isPlaying == false))
            {
                return audioSource;
            }

            var item = owner.AddComponent<AudioSource>();
            allSourcesLst.Add(item);

            return item;
        }

        /// <summary>
        /// 释放多余的 AudioSource
        /// 包正列表中不会有太多 AudioSource
        /// </summary>
        public void ReleaseUnnecessaryAudioSource()
        {
            var tmpCount = 0;
            var tmpSourceLst = new List<AudioSource>();
            foreach (var t in allSourcesLst.Where(t => t.isPlaying == false))
            {
                tmpCount++;

                if (tmpCount > 3)
                {
                    tmpSourceLst.Add(t);
                }
            }

            foreach (var item in tmpSourceLst)
            {
                allSourcesLst.Remove(item);

                Object.Destroy(item);
            }

            tmpSourceLst.Clear();
        }

        public void Stop(string audioName)
        {
            if (string.IsNullOrEmpty(audioName) == false)
            {
                foreach (var t in allSourcesLst.Where(t => t.isPlaying == true && t.clip.name.Equals(audioName)))
                {
                    t.Stop();
                }
            }
        }
    }
}