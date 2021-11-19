namespace Kuroha.Framework.Message
{
    public class TestMessage : BaseMessage
    {
        public readonly int id;
        public readonly float hp;

        public TestMessage(int id, float hp)
        {
            this.id = id;
            this.hp = hp;
        }
    }
}