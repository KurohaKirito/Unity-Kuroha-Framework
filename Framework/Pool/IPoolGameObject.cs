namespace Kuroha.Framework.Pool
{
    public interface IPoolGameObject
    {
        /// <summary>
        /// 激活
        /// </summary>
        public void Enable();
        
        /// <summary>
        /// 失活
        /// </summary>
        public void Disable();
        
        /// <summary>
        /// 释放
        /// </summary>
        public void Release();
    }
}
