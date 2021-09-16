#if Kuroha
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kuroha.Tool.Release
{
    public static class PlaySceneTool
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            if (SceneManager.GetActiveScene().name != "Main")
            {
                //SceneManager.LoadScene("Main");
            }
        }
    }
}
#endif