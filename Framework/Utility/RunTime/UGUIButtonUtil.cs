using UnityEngine.Events;
using UnityEngine.UI;

namespace Kuroha.Framework.Utility.RunTime
{
    public static class UGUIButtonUtil
    {
        public static void AddListener(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }
    }
}