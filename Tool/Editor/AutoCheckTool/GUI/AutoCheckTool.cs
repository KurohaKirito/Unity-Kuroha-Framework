using UnityEditor;

public static class AutoCheckTool
{
    /// <summary>
    /// 自动检测使用
    /// </summary>
    #if Kuroha == false
    [MenuItem("Funny/资源检测工具/Auto Check Tool")]
    #endif
    public static void AutoCheck()
    {
        AutoCheckToolGUI.Check();
    }
}