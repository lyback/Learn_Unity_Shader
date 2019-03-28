using UnityEngine;

/// <summary>
/// 高斯模糊
/// </summary>
public class GaussianBlur : PostEffectsBase
{
    [Range(0, 4)]
    public int iterations = 3;                  // 模糊迭代次数

    [Range(0.2f, 3.0f)]
    public float blurSpread = 0.6f;             // 模糊范围，过大会造成虚影

    [Range(1, 8)]
    public int downSample = 2;                  // 减少采样倍数的平方。越大，处理像素越少，过大可能会像素化

    /// 版本1（不调用）： 普通模糊
    void OnRenderImage1(RenderTexture src, RenderTexture dest)
    {
        if (TargetMaterial != null)
        {
            int rtW = src.width;
            int rtH = src.height;
            RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0); // 屏幕大小的缓冲区，用于存第一个Pass执行后的模糊结果

            // 渲染第一个竖直Pass，存到buffer
            Graphics.Blit(src, buffer, TargetMaterial, 0);
            // 渲染第二个水平Pass，送到屏幕
            Graphics.Blit(buffer, dest, TargetMaterial, 1);

            RenderTexture.ReleaseTemporary(buffer);         // 释放掉缓冲
        }
        else
            Graphics.Blit(src, dest);
    }

    /// 版本2（不调用）： 加上缩放系数
    void OnRenderImage2(RenderTexture src, RenderTexture dest)
    {
        if (TargetMaterial != null)
        {
            int rtW = src.width / downSample;           // 使用小于原屏幕尺寸，减少处理像素个数，提高性能，可能更好模糊效果
            int rtH = src.height / downSample;
            RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

            buffer.filterMode = FilterMode.Bilinear;    // 滤波设置双线性。

            Graphics.Blit(src, buffer, TargetMaterial, 0);
            Graphics.Blit(buffer, dest, TargetMaterial, 1);

            RenderTexture.ReleaseTemporary(buffer);
        }
        else
            Graphics.Blit(src, dest);
    }

    /// 版本3 ： 使用迭代
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (TargetMaterial != null)
            Post(src, dest, TargetMaterial, iterations, blurSpread, downSample);
        else
            Graphics.Blit(src, dest);
    }

    /// <summary>
    /// 使用高斯模糊
    /// </summary>
    public static void Post(RenderTexture src, RenderTexture dest, Material material, int iterations, float blurSpread, int downSample)
    {
        int rtW = src.width / downSample;
        int rtH = src.height / downSample;

        RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
        RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
        buffer0.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, buffer0);                            // 用到所有Pass块

        // buffer0 存将要被处理的缓存，buffer1存搞好的
        for (int i = 0; i < iterations; i++)
        {
            material.SetFloat("_BlurSize", 1.0f + (i + 1) * blurSpread);

            Graphics.Blit(buffer0, buffer1, material, 0);
            Graphics.Blit(buffer1, buffer0, material, 1);
        }

        Graphics.Blit(buffer0, dest);
        RenderTexture.ReleaseTemporary(buffer0);
        RenderTexture.ReleaseTemporary(buffer1);
    }

}