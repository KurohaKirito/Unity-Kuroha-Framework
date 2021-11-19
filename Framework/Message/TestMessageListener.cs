using Kuroha.Util.RunTime;
using UnityEngine;

namespace Kuroha.Framework.Message
{
    /// <summary>
    /// 监听器
    /// </summary>
    public class TestMessageListener : MonoBehaviour
    {
        private void Start()
        {
            MessageSystem.Instance.AddListener(typeof(TestMessage), Handler);
        }

        private static bool Handler(BaseMessage baseMessage)
        {
            if (baseMessage is TestMessage message)
            {
                DebugUtil.Log($"得到了消息: id {message.id} hp {message.hp}!");
            }
            
            return false;
        }
    }
}