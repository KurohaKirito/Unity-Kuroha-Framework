using System;
using UnityEditor;

namespace Kuroha.Util.Editor
{
    public class TimerUtil
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public int Millis { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public bool AutoReStart { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public Action TimedAction { get; set; }

        private bool isStart;
        private long lastTicks;
        private long currentTicks;
        private long currentMillis;

        /// <summary>
        /// 计时器 (毫秒)
        /// </summary>
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
        // ReSharper disable once MemberCanBePrivate.Global
        public void Start()
        {
            lastTicks = DateTime.Now.Ticks;
            isStart = true;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
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
