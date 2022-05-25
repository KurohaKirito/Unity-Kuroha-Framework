using System.Collections.Generic;
using System.IO;
using Script.Effect.Editor.AssetTool.Tool.Editor.AssetCheckTool;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UPRTools.Editor;

public static class ParticleSystemProfiler
{
    // 表格左侧的留白
    private const float UI_TABLE_LEFT_MARGIN = 1;

    // 表格上方的高度
    private const float UI_MODULE_HEIGHT = 65;

    // 表格的高度
    private const float UI_TABLE_AREA_HEIGHT = 350;

    // 导出区域的高度
    private const float UI_EXPORT_AREA_HEIGHT = 70;

    private static bool profilingDone;
    private static bool psProfilerRunning;
    private static bool particleSystemFoldout = true;

    private static int profilingIndex = -1;

    private static string profileResultPath;
    //private static string particlePlayScene;
    //private static GameObject profilingObject;

    private static List<int> enabledIds;

    //private static List<string> particleSystemPaths;
    private static List<GameObject> particleSystems;
    private static List<ParticleSystemElement> particleSystemElements;

    private static TreeViewState particleSystemTreeViewState;
    private static MultiColumnHeaderState multiColumnHeaderState;
    private static MultiColumnTreeView particleSystemTreeView;
    private static MultiColumnItemTreeView itemSystemTreeView;

    private static AssetCheckToolWindow window;

    public static void Init(AssetCheckToolWindow baseWindow)
    {
        window = baseWindow;
        
        profileResultPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "ProfileResults.json";

        if (particleSystemTreeViewState == null)
        {
            particleSystemTreeViewState = new TreeViewState();
        }

        if (particleSystemElements == null)
        {
            particleSystemElements = new List<ParticleSystemElement>();
        }

        if (particleSystems == null)
        {
            particleSystems = new List<GameObject>();
        }

        var firstInit = multiColumnHeaderState == null;

        var headerState =
            MultiColumnTreeView.CreateDefaultMultiColumnHeaderState(GetMultiColumnTreeViewRect().width);
        if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, headerState))
        {
            MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, headerState);
        }

        multiColumnHeaderState = headerState;
        var multiColumnHeader = new MultiColumnHeader(headerState) {canSort = false};

        if (firstInit)
        {
            multiColumnHeader.ResizeToFit();
        }

        var treeModel = new TreeModel<ParticleSystemElement>(particleSystemElements);
        particleSystemTreeView =
            new MultiColumnTreeView(particleSystemTreeViewState, multiColumnHeader, treeModel);
    }

    public static void OnGUI_ParticleSystemProfiler()
    {
        if (psProfilerRunning)
        {
            if (EditorUtility.DisplayCancelableProgressBar(
                "Profiling Particle Systems",
                $"Playing {profilingIndex + 1} / {enabledIds.Count}",
                (float) (profilingIndex + 1) / enabledIds.Count))
            {
                Debug.Log("Profiling canceled manually");
                psProfilerRunning = false;
                EditorUtility.ClearProgressBar();
            }
        }

        GUILayout.Space(2 * AssetCheckToolWindow.UI_DEFAULT_MARGIN);

        particleSystemFoldout = EditorGUILayout.Foldout(particleSystemFoldout, "Particle System Profiler", true);
        if (!particleSystemFoldout)
        {
            return;
        }

        GUI.enabled = !psProfilerRunning;

        #region 顶部的两个按钮

        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("Detect", GUILayout.Height(20), GUILayout.Width(120)))
        {
            DetectParticleSystems();
        }

        if (GUILayout.Button("Start", GUILayout.Height(20), GUILayout.Width(120)))
        {
            StartProfileParticleSystems();
        }

        GUILayout.EndHorizontal();

        #endregion

        // 中间的表格 (表格使用固定布局模式绘制)
        particleSystemTreeView.OnGUI(GetMultiColumnTreeViewRect());
        GUI.enabled = true;

        #region 底部的导出按钮

        // 固定模式下想用自动布局模式的话
        // 必须用 BeginArea EndArea 包起来
        GUILayout.BeginArea(GetUploadResultViewRect());
        GUILayout.Space(2 * AssetCheckToolWindow.UI_DEFAULT_MARGIN);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Result Path");
        profileResultPath = GUILayout.TextArea(profileResultPath, GUILayout.Width(500));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.enabled = profilingDone;
        GUILayout.Space(AssetCheckToolWindow.UI_DEFAULT_MARGIN);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical("Box");
        var exportBtn = GUILayout.Button("Export Result", GUILayout.Height(20), GUILayout.Width(120));
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(AssetCheckToolWindow.UI_DEFAULT_MARGIN);
        GUILayout.EndArea();

        if (exportBtn)
        {
            // TODO 导出数据
        }

        #endregion

        GUI.enabled = true;
    }

    private static void DetectParticleSystems()
    {
        // 询问是否保存场景
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        particleSystems = new List<GameObject>();
        //particleSystemPaths = new List<string>();
        particleSystemElements = new List<ParticleSystemElement> {new ParticleSystemElement("", "", -1, 0)};

        var idCounter = 0;
        var assetsIndex = 0;
        var allGameObjects = AssetDatabase.FindAssets("t:GameObject");

        foreach (var guid in allGameObjects)
        {
            ++assetsIndex;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (EditorUtility.DisplayCancelableProgressBar(
                "Particle System Profiler", $"detecting... ({assetsIndex} / {allGameObjects.Length})",
                (float) assetsIndex / allGameObjects.Length))
            {
                break;
            }

            if (gameObj.GetComponentsInChildren<ParticleSystem>().Length == 0) continue;

            idCounter++;
            particleSystems.Add(gameObj);
            //particleSystemPaths.Add (path);
            particleSystemElements.Add(new ParticleSystemElement(path, "", 0, idCounter));
        }

        EditorUtility.ClearProgressBar();

        Debug.Log("Detected Particle Systems Count: " + particleSystems.Count);
        Init(window);
    }

    private static void StartProfileParticleSystems()
    {
        enabledIds = new List<int>();

        for (var i = 1; i < particleSystemElements.Count; i++)
        {
            if (particleSystemElements[i].enabled)
            {
                enabledIds.Add(i - 1);
            }
        }

        if (particleSystems.Count == 0 || enabledIds.Count == 0)
        {
            Debug.Log("No particle system selected for profiling.");
            return;
        }

        Debug.Log(enabledIds.Count + " particle system assets selected for profiling.");

        if (File.Exists(profileResultPath))
        {
            File.Delete(profileResultPath);
        }

        psProfilerRunning = true;
        profilingDone = false;
        profilingIndex = 0;
        PlayNextParticleSystem();
    }

    private static void PlayNextParticleSystem()
    {
        //var realIndex = enabledIds[profilingIndex];
        //profilingObject = Instantiate(particleSystems[realIndex], new Vector3(0, 0, 0), particleSystems[realIndex].GetComponent<Transform>().rotation);
        //var manager = profilingObject.AddComponent<ProfileManager>();
        //manager.m_ResultsPersistencePath = profileResultPath;
        //manager.m_PrefabPath = particleSystemPaths[realIndex];
        EditorApplication.isPlaying = true;
    }

    private static Rect GetMultiColumnTreeViewRect()
    {
        return new Rect(
            UI_TABLE_LEFT_MARGIN,
            UI_MODULE_HEIGHT + AssetCheckToolWindow.UI_DEFAULT_MARGIN,
            window.position.width - 11f,
            UI_TABLE_AREA_HEIGHT);
    }

    private static Rect GetUploadResultViewRect()
    {
        return new Rect(UI_TABLE_LEFT_MARGIN,
            UI_MODULE_HEIGHT + UI_TABLE_AREA_HEIGHT + AssetCheckToolWindow.UI_DEFAULT_MARGIN,
            window.position.width - 2 * UI_TABLE_LEFT_MARGIN,
            UI_EXPORT_AREA_HEIGHT);
    }
}