using System;
using Kuroha.Framework.Singleton;
using UnityEngine;

namespace Kuroha.Util.RunTime
{
    /// <summary>
    /// 屏幕截图类
    /// </summary>
    public class ScreenshotUtil : Singleton<ScreenshotUtil>
    {
        [SerializeField]
        private RenderTexture renderTexture;
        [SerializeField]
        private int cameraShotWidth;
        [SerializeField]
        private int cameraShotHeight;
        [SerializeField]
        private Texture2D cameraShot;

        /// <summary>
        /// 单例
        /// </summary>
        public static ScreenshotUtil Instance => InstanceBase as ScreenshotUtil;

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void AutoInit()
        {
            cameraShotWidth = Screen.width;
            cameraShotHeight = Screen.height;

            renderTexture ??= new RenderTexture(cameraShotWidth, cameraShotHeight, 0);
            cameraShot ??= new Texture2D(cameraShotWidth, cameraShotHeight, TextureFormat.RGBA32, false, true);
        }

        /// <summary>
        /// 对相机截图
        /// </summary>
        /// <param name="targetCameras">目标相机</param>
        /// <param name="rect">范围</param>
        /// <returns>截图</returns>
        public Texture2D CaptureCameraShot(Rect rect, params Camera[] targetCameras)
        {
            // 释放资源 (第 1 次以后的截图需要用到)
            Release();
            
            // 刷新尺寸
            cameraShotWidth = Convert.ToInt16(rect.width);
            cameraShotHeight = Convert.ToInt16(rect.height);
            
            // 设置尺寸
            renderTexture.width = cameraShotWidth;
            renderTexture.height = cameraShotHeight;
            cameraShot.Resize(cameraShotWidth, cameraShotHeight);

            // 临时设置相关相机的 targetTexture, 并手动渲染相关相机
            foreach (var targetCamera in targetCameras)
            {
                targetCamera.targetTexture = renderTexture;
                targetCamera.Render();
            }
            
            // 激活 renderTexture
            RenderTexture.active = renderTexture;

            // 从 RenderTexture.active 中读取像素
            cameraShot.ReadPixels(rect, 0, 0);
            cameraShot.Apply();
            
            // 重置相机渲染
            foreach (var targetCamera in targetCameras)
            {
                targetCamera.targetTexture = null;
            }
            
            // 失活 renderTexture
            RenderTexture.active = null;
            
            return cameraShot;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        private void Release()
        {
            renderTexture.Release();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Release();
            cameraShot = null;
            renderTexture = null;
        }
    }
}
