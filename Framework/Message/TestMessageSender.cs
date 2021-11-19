using UnityEngine;

namespace Kuroha.Framework.Message
{
    public class TestMessageSender : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 下一帧触发
                MessageSystem.Instance.EnqueueMessage(new TestMessage(1, 50f));
                
                // 立即触发
                MessageSystem.Instance.TriggerMessage(new TestMessage(2, 100f));
            }
        }
    }
}
