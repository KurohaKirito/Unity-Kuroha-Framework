namespace Kuroha.Framework.FiniteStateMachine
{
    public abstract class BaseState
    {
        public virtual void InputHandler(BaseStateInputPara inputPara) { }
        
        public virtual void Update(BaseStateUpdatePara updatePara) { }
    }
}
