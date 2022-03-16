using System.Threading.Tasks;
using Kuroha.Framework.AsyncLoad.RunTime;
using Kuroha.Framework.Audio.RunTime;
using Kuroha.Framework.Singleton;
using Kuroha.Framework.Singleton.RunTime;

namespace Kuroha.Framework.Launcher.RunTime
{
    public class Launcher : Singleton<Launcher>
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static Launcher Instance => InstanceBase as Launcher;
        
        /// <summary>
        /// 场景异步加载器
        /// </summary>
        private AsyncLoadScene asyncLoadScene;

        /// <summary>
        /// [Async] 初始化
        /// </summary>
        public sealed override async Task InitAsync()
        {
            asyncLoadScene ??= new AsyncLoadScene();
            
            asyncLoadScene.OnLaunch();
            
            await AudioPlayManager.Instance.InitAsync();
            await BugReport.RunTime.BugReport.Instance.InitAsync();
        }
    }
}
