using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 图像处理工具类
/// </summary>
public class TextureTool
{
    /// <summary>
    /// 图片灰度化和二值化
    /// </summary>
    public static Texture2D OnGetTextureGray(Texture texture_)
    {
        Dictionary<float, int> pixelCounts = new Dictionary<float, int>();
        Texture2D scTexture = TextureToTexture2D(texture_);
        Texture2D grayTexture = new Texture2D(scTexture.width, scTexture.height);

        for (int y = 0; y < scTexture.height; ++y)
        {
            for (int x = 0; x < scTexture.width; ++x)
            {
                Color curColor = scTexture.GetPixel(x, y);
                float g = curColor.r * 0.299f + curColor.g * 0.587f + curColor.b * 0.114f;

                // 记录相同灰度亮度值的像素有多少个
                int lumina = (int)(g * 256);
                if (pixelCounts.ContainsKey(lumina))
                    pixelCounts[lumina] = pixelCounts[lumina] + 1;
                else
                    pixelCounts[lumina] = 1;
            }
        }

        // 图像二值化与otsu算法,获取平均阈值
        float[] s_max = new float[2] { 0, -10 };
        for (int i = 0; i < 256; ++i)
        {
            int w_0 = 0, w_1 = 0, u_0 = 0, u_1 = 0, u = 0, g = 0;
            foreach (KeyValuePair<float, int> val in pixelCounts)
            {
                if (val.Key < i)
                    w_0 += val.Value;    // 得到阈值以下像素个数的和
                else
                    w_1 += val.Value;    // 得到阈值以上像素个数的和
            }

            // 得到阈值下所有像素的平均灰度
            for (int b = 0; b < i; b++)
            {
                if (pixelCounts.ContainsKey(b))
                {
                    u_0 += w_0 > 0 ? (b * pixelCounts[b]) / w_0 : 0;
                }
            }
            for (int c = 200; c < 256; c++)
            {
                if (pixelCounts.ContainsKey(c))
                {
                    u_1 += w_1 > 0 ? (c * pixelCounts[c]) / w_1 : 0;
                }
            }


            // 总平均灰度
            u = w_0 * u_0 + w_1 * u_1;

            // 类间方差
            g = w_0 * (u_0 - u) * (u_0 - u) + w_1 * (u_1 - u) * (u_1 - u);

            // 类间方差等价公式
            //g = w_0 * w_1 * (u_0 * u_1) * (u_0 * u_1);

            // 取最大的
            if (g > s_max[1])
                s_max = new float[2] { i, g };
        }

        // 平均值
        //Debug.Log(s_max[0]);
        // 二值化
        for (int y = 0; y < scTexture.height; ++y)
        {
            for (int x = 0; x < scTexture.width; ++x)
            {
                Color curColor = scTexture.GetPixel(x, y);
                float g = curColor.r * 0.299f + curColor.g * 0.587f + curColor.b * 0.114f;

                Color newColor = curColor;
                int lumina = (int)(g * 256);
                if (lumina > s_max[0])
                    newColor = new Color(1, 1, 1, 0);
                else
                    newColor = new Color(0, 0, 0, curColor.a);

                grayTexture.SetPixel(x, y, newColor);
            }
        }

        return grayTexture;
    }

    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="path_"></param>
    /// <param name="name_"></param>
    /// <param name="texture_"></param>
    public static void OnSaveTexture2d(string path_, string name_, Texture2D texture_)
    {
        byte[] bytes = texture_.EncodeToPNG();
        if (!Directory.Exists(path_))
            Directory.CreateDirectory(path_);

        FileStream file = File.Open(path_ + "/" + name_ + ".png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 运行模式下Texture转换成Texture2D
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);
        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }
}
