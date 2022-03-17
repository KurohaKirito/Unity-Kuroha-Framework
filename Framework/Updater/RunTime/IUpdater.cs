using Kuroha.Framework.Message.RunTime;

namespace Kuroha.Framework.Updater.RunTime
{
    public interface IUpdater
    {
        public bool UpdateEvent(BaseMessage message);
    }
}
