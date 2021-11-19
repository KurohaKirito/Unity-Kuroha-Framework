using UnityEngine;

namespace Kuroha.Framework.Message
{
    public class TestMessageSender : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MessageSystem.Instance.EnqueueMessage(new TestMessage(1, 50f));
            }
        }
    }
}