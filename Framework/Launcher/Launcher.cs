using System.Threading.Tasks;
using Kuroha.Framework.AsyncLoad;
using Kuroha.Framework.AsyncLoad.Scene;
using Kuroha.Framework.Audio;
using Kuroha.Framework.Singleton;

namespace Kuroha.Framework.Launcher
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
            await BugReport.BugReport.Instance.InitAsync();
        }
    }
}
