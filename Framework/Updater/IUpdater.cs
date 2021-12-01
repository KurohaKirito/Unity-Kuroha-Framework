using Kuroha.Framework.Message;

namespace Kuroha.Framework.Updater
{
    public interface IUpdater
    {
        public bool OnUpdate(BaseMessage message);
    }
}

