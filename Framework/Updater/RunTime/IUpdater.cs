using Kuroha.Framework.Message;
using Kuroha.Framework.Message.RunTime;

namespace Kuroha.Framework.Updater
{
    public interface IUpdater
    {
        public bool UpdateEvent(BaseMessage message);
    }
}
