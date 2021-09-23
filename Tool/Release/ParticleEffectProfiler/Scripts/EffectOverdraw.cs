#if UNITY_EDITOR
using System;
using UnityEngine;

/// <summary>
/// 主要用于计算 overdraw 像素
/// </summary>
public class EffectOverdraw
{
    private Camera camera;
    private readonly SingleEffectOverdraw singleEffectOverdraw;

    // 采集特效数据的区域大小
    private const int RT_WIDTH = 512;
    private const int RT_HEIGHT = 512;
    private readonly RenderTexture renderTexture;

    public EffectOverdraw(Camera camera)
    {
        SetCamera(camera);
        renderTexture = new RenderTexture(RT_WIDTH, RT_HEIGHT, 0, RenderTextureFormat.ARGB32);
        singleEffectOverdraw = new SingleEffectOverdraw(1);
    }

    private void SetCamera(Camera rendererCamera)
    {
        camera = rendererCamera;
        rendererCamera.SetReplacementShader(Shader.Find("ParticleEffectProfiler/OverDraw"), "");
    }

    public void Update()
    {
        RecordOverDrawData(singleEffectOverdraw);
    }

    public EffectOverdrawData[] GetEffectEvlaData()
    {
        return singleEffectOverdraw.GetEffectEvlaDatas();
    }

    #region overdraw

    private void RecordOverDrawData(SingleEffectOverdraw singleEffectOverdraw)
    {
        var pixTotal = 0;
        var pixActualDraw = 0;

        GetCameraOverDrawData(out pixTotal, out pixActualDraw);

        // 往数据+1
        singleEffectOverdraw.UpdateOneData(pixTotal, pixActualDraw);
    }

    private void GetCameraOverDrawData(out int pixTotal, out int pixActualDraw)
    {
        //记录当前激活的渲染纹理
        var activeTexture = RenderTexture.active;

        //渲染指定范围的rt，并记录范围内所有rgb像素值
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        GetOverDrawData(texture, out pixTotal, out pixActualDraw);

        //恢复之前激活的渲染纹理
        RenderTexture.active = activeTexture;
        Texture2D.DestroyImmediate(texture);
        renderTexture.Release();
        camera.targetTexture = null;
    }

    private static void GetOverDrawData(Texture2D texture, out int pixTotal, out int pixActualDraw)
    {
        var texw = texture.width;
        var texh = texture.height;

        var pixels = texture.GetPixels();

        var index = 0;

        pixTotal = 0;
        pixActualDraw = 0;

        for (var y = 0; y < texh; y++)
        {
            for (var x = 0; x < texw; x++)
            {
                var r = pixels[index].r;
                var g = pixels[index].g;
                var b = pixels[index].b;

                var isEmptyPix = IsEmptyPix(r, g, b);
                if (!isEmptyPix)
                {

                    pixTotal++;
                }

                var drawThisPixTimes = DrawPixTimes(r, g, b);
                pixActualDraw += drawThisPixTimes;

                index++;
            }
        }
    }


    //计算单像素的绘制次数
    private static int DrawPixTimes(float r, float g, float b)
    {
        //在OverDraw.Shader中像素每渲染一次，g值就会叠加0.04
        return Convert.ToInt32(g / 0.04);
    }

    private static bool IsEmptyPix(float r, float g, float b)
    {
        return r == 0 && g == 0 && b == 0;
    }
    #endregion
}
#endif
