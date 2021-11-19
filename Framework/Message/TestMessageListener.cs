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
            if (MessageSystem.Instance.AddListener(typeof(TestMessage), Handler))
            {
                DebugUtil.Log($"消息 {nameof(TestMessage)} 注册成功!", null, "green");
            }
            else
            {
                DebugUtil.Log($"消息 {nameof(TestMessage)} 注册失败!", null, "red");
            }
        }
        
        private void OnDestroy()
        {
            if (MessageSystem.IsActive)
            {
                if (MessageSystem.Instance.RemoveListener(typeof(TestMessage), Handler))
                {
                    DebugUtil.Log($"消息 {nameof(TestMessage)} 移除成功!", null, "green");
                }
                else
                {
                    DebugUtil.Log($"消息 {nameof(TestMessage)} 移除失败!", null, "red");
                }
            }
            else
            {
                DebugUtil.Log("单例 MessageSystem 已经被销毁了!", null, "red");
            }
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