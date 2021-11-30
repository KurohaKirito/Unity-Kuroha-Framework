using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Framework.Audio
{
    /// <summary>
    /// 音频片段管理器
    /// 管理所有的音频资源
    /// </summary>
    public class AudioClipManager
    {
        private const string CFG_PATH = "AudioClip/AudioClipCfg";

        private Dictionary<string, string> clipNamePathDic;
        
        private readonly Dictionary<string, SingleClip> clipNameAudioDic;
        
        private string[] clipPathLst;

        public AudioClipManager()
        {
            clipNamePathDic = new Dictionary<string, string>();
            clipNameAudioDic = new Dictionary<string, SingleClip>();

            ReadConfig();
            LoadClip();
        }
        
        private void ReadConfig()
        {
            var strConfig = Resources.Load<TextAsset>(CFG_PATH).text;
            var linesStr = strConfig.Split('\n');

            if (int.TryParse(linesStr[0], out var clipCount))
            {
                clipPathLst = new string[clipCount];
                for (var i = 1; i <= clipCount; i++)
                {
                    var namesPaths = linesStr[i].Split(':');
                    clipNamePathDic.Add(namesPaths[0], namesPaths[1].Trim());
                    clipPathLst[i - 1] = namesPaths[1];
                }
            }
        }
        
        public SingleClip GetAudioByName(string clipName) {
            clipNameAudioDic.TryGetValue(clipName, out var item);
            return item;
        }
        
        private void LoadClip() {
            foreach (var item in clipNamePathDic) {
                Debug.Log(GetType() + "/LoadClip()/item.Value == " + item.Value);
                var clip = Resources.Load<AudioClip>(item.Value);
                var singleClip = new SingleClip(clip);
                clipNameAudioDic.Add(item.Key, singleClip);
            }

            // clipNamePathDic 这个可能用不上了，可以清空，避免一直占用内存
            clipNamePathDic.Clear();
            clipNamePathDic = null;
        }
    }
}