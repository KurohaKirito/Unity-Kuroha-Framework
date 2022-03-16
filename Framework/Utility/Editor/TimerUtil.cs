using System;
using UnityEditor;

namespace Kuroha.Framework.Utility.Editor
{
    /// <summary>
    /// 计时器 (毫秒)
    /// </summary>
    public class TimerUtil
    {
        private int Millis { get; set; }

        public bool AutoReStart { get; set; }

        private Action TimedAction { get; set; }

        private bool isStart;
        private long lastTicks;
        private long currentTicks;
        private long currentMillis;
        
        public TimerUtil(int millis, Action timedAction)
        {
            Millis = millis;
            TimedAction = timedAction;
            
            isStart = false;
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        public void Start()
        {
            lastTicks = DateTime.Now.Ticks;
            isStart = true;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public void Stop()
        {
            isStart = false;
        }

        /// <summary>
        /// 编辑器更新时计算计时
        /// </summary>
        private void EditorUpdate()
        {
            if (isStart == false)
            {
                return;
            }
            
            currentTicks = DateTime.Now.Ticks;
            currentMillis = (currentTicks - lastTicks) / 10000;
            
            if (currentMillis >= Millis)
            {
                TimedAction?.Invoke();
                
                if (AutoReStart)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
    }
}
