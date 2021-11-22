using Kuroha.Framework.Message;
using UnityEngine;

namespace Kuroha.Framework.Updater
{
    public class UpdateableMonoBehaviour : MonoBehaviour, IUpdateable
    {
        public virtual bool OnUpdate(BaseMessage message)
        {
            return false;
        }
    }
}
