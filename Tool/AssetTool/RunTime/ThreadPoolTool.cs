using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kuroha.Tool.Release
{
    /// <summary>
    /// 线程池工具
    /// </summary>
    public class ThreadPoolTool
    {
        /// <summary>
        /// 任务接口
        /// </summary>
        public interface ITask
        {
            /// <summary>
            /// 任务是否结束
            /// </summary>
            /// <returns></returns>
            bool IsDone();

            /// <summary>
            /// 执行任务
            /// </summary>
            void Execute();
        }

        /// <summary>
        /// 任务
        /// </summary>
        private readonly List<ITask> tasks;

        /// <summary>
        /// 总任务数
        /// </summary>
        public int taskCount;

        /// <summary>
        /// 已完成任务数
        /// </summary>
        public int completedTaskCount;

        /// <summary>
        /// 是否已完成多线程任务
        /// </summary>
        public bool IsDone
        {
            get
            {
                taskCount = tasks.Count;
                completedTaskCount = tasks.Count(task => task.IsDone());
                return completedTaskCount >= taskCount;
            }
        }

        /// <summary>
        /// 创建多线程
        /// </summary>
        /// <param name="tasksEnumerable">任务</param>
        public ThreadPoolTool(IEnumerable<ITask> tasksEnumerable)
        {
            tasks = tasksEnumerable.ToList();
            foreach (var task in tasks)
            {
                ThreadPool.QueueUserWorkItem(state => { task.Execute(); });
            }
        }
    }
}