#if KUROHA_DEBUG_MODE

using System.Collections.Generic;
using UnityEngine;

namespace Kuroha.Framework.Message.RunTime
{
    [System.Serializable]
    public struct MessageListener
    {
        public string messageTypeName;

        [SerializeField]
        public List<string> listenerList;
    }
}

#endif