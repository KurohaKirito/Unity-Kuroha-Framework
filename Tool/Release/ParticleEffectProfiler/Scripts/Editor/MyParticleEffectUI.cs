using UnityEditor;
using UnityEngine;

/// <summary>
/// 将特效的性能数据显示到 Scene 窗口
/// </summary>
[CustomEditor(typeof(ParticleEffectScript))] 
public class MyParticleEffectUI : Editor
{
    private readonly string[] labelArray = new string[20];

    private void OnSceneGUI()
    {
        var particleEffectScript = (ParticleEffectScript) target;

        var index = 0;
        labelArray[index] = GetParticleEffectData.GetTextRuntimeMemorySize(particleEffectScript.gameObject);
        labelArray[++index] = GetParticleEffectData.GetParticleSystemCount(particleEffectScript.gameObject);

        if (EditorApplication.isPlaying)
        {
            labelArray[++index] = GetParticleEffectData.GetOnlyParticleEffectDrawCallStr();
            labelArray[++index] = GetParticleEffectData.GetParticleCountStr(particleEffectScript);
            labelArray[++index] = GetParticleEffectData.GetPixDrawAverageStr(particleEffectScript);
            labelArray[++index] = GetParticleEffectData.GetPixActualDrawAverageStr(particleEffectScript);
            labelArray[++index] = GetParticleEffectData.GetPixRateStr(particleEffectScript);
        }

        ShowUI(); 
    }

    void ShowUI()
    {
        //开始绘制GUI
        Handles.BeginGUI();

        //规定GUI显示区域
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontStyle = FontStyle.Bold;

        for (int i = 0; i < labelArray.Length; i++)
		{
            if (!string.IsNullOrEmpty(labelArray[i]))
	        {
		        GUILayout.Label(labelArray[i], style);
	        }
		}

        GUILayout.EndArea();

        Handles.EndGUI();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ParticleEffectScript particleEffectScript = (ParticleEffectScript)target;

        string autoCullingTips = GetParticleEffectData.GetCullingSupportedString(particleEffectScript.gameObject);
        if (!string.IsNullOrEmpty(autoCullingTips))
        {
            GUILayout.Label("ParticleSystem以下选项会导致无法自动剔除：", EditorStyles.whiteLargeLabel);
            GUILayout.Label(autoCullingTips);
        }
    }
}
