using Kuroha.Util.RunTime;
using UnityEditor;
using UnityEngine;

namespace Kuroha.Tool.Release.ParticleEffectProfiler.Scripts.Editor
{
    /// <summary>
    /// 给选中的特效添加脚本的
    /// </summary>
    [InitializeOnLoad]
    public static class TestParticleEffect
    {
        private const string REQUEST_TEST_KEY = "TestParticleEffectRquestTest";
        private static bool hasPlayed;
        private static bool isRestart;

        [MenuItem("GameObject/Effects/ParticleTest", false, 11)]
        private static void Test()
        {
            var hierarchyGameObject = Selection.activeGameObject;
            var renderers = hierarchyGameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);

            if (renderers.Length <= 0)
            {
                DebugUtil.LogError("不是特效无法测试!", hierarchyGameObject, "red");
            }
            else
            {
                EditorPrefs.SetBool(REQUEST_TEST_KEY, true);

                // 已经在播放状态, 使其重新开始
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                    isRestart = true;
                }
                else
                {
                    EditorApplication.isPlaying = true;
                }

                var particleEffectScript = hierarchyGameObject.GetComponentsInChildren<ParticleEffectManager>(true);
                if (particleEffectScript.Length <= 0)
                {
                    hierarchyGameObject.AddComponent<ParticleEffectManager>();
                }
            }
        }

        static TestParticleEffect()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private static void Update()
        {
            if (EditorPrefs.HasKey(REQUEST_TEST_KEY) &&
                EditorApplication.isPlayingOrWillChangePlaymode &&
                EditorApplication.isPlaying && hasPlayed == false)
            {
                EditorPrefs.DeleteKey(REQUEST_TEST_KEY);
                hasPlayed = true;
            }
        }

        private static void PlayModeStateChanged(PlayModeStateChange change)
        {
            if (EditorApplication.isPlaying == false)
            {
                hasPlayed = false;
            }

            if (isRestart)
            {
                EditorApplication.isPlaying = true;
                isRestart = false;
            }
        }
    }
}
