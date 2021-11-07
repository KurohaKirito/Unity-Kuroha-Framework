namespace Kuroha.Tool.AssetTool.Editor.EffectCheckTool.GUI
{
    public static class EffectToolData
    {
        /// <summary>
        ///可检测的资源类型
        /// </summary>
        public enum AssetsType
        {
            /// <summary>
            /// 网格
            /// </summary>
            Mesh = 0,

            /// <summary>
            /// 贴图
            /// </summary>
            Texture = 1,

            /// <summary>
            /// 动画状态机
            /// </summary>
            Animator = 2,

            /// <summary>
            /// 粒子系统
            /// </summary>
            ParticleSystem = 3,

            /// <summary>
            /// 预制体
            /// </summary>
            Prefab = 4,

            /// <summary>
            /// 模型
            /// </summary>
            Model = 5,
            
            /// <summary>
            /// 通用检查
            /// </summary>
            Asset = 6
        }
    }
}