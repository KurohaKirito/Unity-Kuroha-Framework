using Kuroha.Framework.Message;

namespace Kuroha.Framework.Updater
{
    public interface IUpdateable
    {
        public bool OnUpdate(BaseMessage message);
    }
}

