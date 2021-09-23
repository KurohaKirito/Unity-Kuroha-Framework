using UnityEditor;
using UnityEngine;

/// <summary>
/// 将特效的性能数据显示到 Scene 窗口
/// </summary>
[CustomEditor(typeof(ParticleEffectManager))] 
public class MyParticleEffectUI : Editor
{
    private readonly string[] labelArray = new string[20];
    private ParticleEffectManager Target => target as ParticleEffectManager;

    private void OnSceneGUI()
    {
        var index = 0;
        labelArray[index] = GetParticleEffectData.GetTextRuntimeMemorySize(Target.gameObject);
        labelArray[++index] = GetParticleEffectData.GetParticleSystemCount(Target.gameObject);

        if (EditorApplication.isPlaying)
        {
            labelArray[++index] = GetParticleEffectData.GetOnlyParticleEffectDrawCallStr();
            labelArray[++index] = GetParticleEffectData.GetParticleCountStr(Target);
            labelArray[++index] = GetParticleEffectData.GetPixDrawAverageStr(Target);
            labelArray[++index] = GetParticleEffectData.GetPixActualDrawAverageStr(Target);
            labelArray[++index] = GetParticleEffectData.GetPixRateStr(Target);
        }

        DrawSceneGUI(); 
    }

    private void DrawSceneGUI()
    {
        Handles.BeginGUI();
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            {
                var style = new GUIStyle
                {
                    richText = true,
                    fontStyle = FontStyle.Bold
                };

                foreach (var label in labelArray)
                {
                    if (string.IsNullOrEmpty(label) == false)
                    {
                        GUILayout.Label(label, style);
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var autoCullingTips = GetParticleEffectData.GetCullingSupportedString(Target.gameObject);
        
        if (string.IsNullOrEmpty(autoCullingTips) == false)
        {
            GUILayout.Label("ParticleSystem 以下选项会导致无法自动剔除: ", EditorStyles.whiteLargeLabel);
            GUILayout.Label(autoCullingTips);
        }
    }
}
