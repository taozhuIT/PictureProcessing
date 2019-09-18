using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图片二值化处理
/// </summary>
public class Image2 : MonoBehaviour
{
    [SerializeField]
    private RawImage img;
    [SerializeField]
    private Image img2;

    [SerializeField]
    private GameObject obj;

    // 二值化亮度阈值
    //private int threshold = 200;

    private void Start () {
        ImageGray();
    }
	
	private void Update () {
		
	}

    /// <summary>
    /// 图片灰度化
    /// </summary>
    private void ImageGray()
    {
        Dictionary<float, int> pixelCounts = new Dictionary<float, int>();

        Texture2D tt = TextureToTexture2D(img.texture);
        for(int y = 0; y < tt.height; ++y)
        {
            for (int x = 0; x < tt.width; ++x)
            {
                Color curColor = tt.GetPixel(x, y);
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
            for(int b = 0; b < i; b++)
            {
                if(pixelCounts.ContainsKey(b))
                {
                    u_0 += w_0 > 0 ? (b * pixelCounts[b]) / w_0 : 0;
                }
            }
            for (int c = 200; c < 256; c++)
            {
                if(pixelCounts.ContainsKey(c))
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

        Debug.Log(s_max[0]);
        // 二值化
        for (int y = 0; y < tt.height; ++y)
        {
            for (int x = 0; x < tt.width; ++x)
            {
                Color curColor = tt.GetPixel(x, y);
                float g = curColor.r * 0.299f + curColor.g * 0.587f + curColor.b * 0.114f;

                Color newColor = curColor;
                int lumina = (int)(g * 256);
                if (lumina > s_max[0])
                    newColor = new Color(1, 1, 1, 0);
                else
                    newColor = new Color(0, 0, 0, curColor.a);

                tt.SetPixel(x, y, newColor);
            }
        }

        tt.Apply();

        OnCreateMesh(tt.width, tt.height, tt.GetPixels());
    }

    // <summary>
    /// 编辑器模式下Texture转换成Texture2D
    /// </summary>
    /// <param name="texture"></param>
    /// <returns></returns>
    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2d = texture as Texture2D;
        UnityEditor.TextureImporter ti = (UnityEditor.TextureImporter)UnityEditor.TextureImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(texture2d));
        //图片Read/Write Enable的开关
        ti.isReadable = true;
        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(texture2d));
        return texture2d;
    }

    /// <summary>
    /// 生成模型(平面位x，y像素网格生成方式)
    /// </summary>
    private void OnCreateMesh(int sizeX_, int sizeY_, Color[] pixes_)
    {
        // 创建模型
        Vector3[] verts = new Vector3[sizeX_ * sizeY_];
        Vector2[] uvs = new Vector2[sizeX_ * sizeY_];
        Color[] colors = new Color[sizeX_ * sizeY_];

        for (int y = 0; y < sizeY_; ++y)
        {
            for (int x = 0; x < sizeX_; ++x)
            {
                int idx = (y * sizeX_) + x;
                float px = x * 1;
                float py = y * 1;

                Color pixel = pixes_[(int)px + (sizeY_ - 1 - (int)py) * sizeX_];

                // 顶点总数除以2，目的是让mesh初始点在mesh中心
                verts[idx] = new Vector3(-sizeX_ / 2 + px, sizeY_ / 2 - py, pixel.r * 5);
                colors[idx] = pixel; //Color.white;
                // UV的值需要归一化0~1之间，所以需要除以总数
                uvs[idx] = new Vector2((float)x / sizeX_, (float)y / sizeY_);
            }
        }

        // 模型面片
        int faceNumX = sizeX_ - 1;
        int faceNumY = sizeY_ - 1;
        // 顶点索引信息
        int[] indxs = new int[faceNumX * faceNumY * 6];
        // 顶点法线信息
        Vector3[] norms = new Vector3[sizeX_ * sizeY_];

        for (int y = 0; y < faceNumY; ++y)
        {
            int startIdx = y * sizeX_;

            for (int x = 0; x < faceNumX; ++x)
            {
                int iidx = y * faceNumX + x;
                int vidx = startIdx + x;

                int i0 = iidx * 6 + 0;
                int i1 = iidx * 6 + 1;
                int i2 = iidx * 6 + 2;
                int i3 = iidx * 6 + 3;
                int i4 = iidx * 6 + 4;
                int i5 = iidx * 6 + 5;

                indxs[i0] = vidx;
                indxs[i1] = vidx + 1;
                indxs[i2] = vidx + 1 + sizeX_;
                indxs[i3] = vidx + 1 + sizeX_;
                indxs[i4] = vidx + sizeX_;
                indxs[i5] = vidx;

                // 计算法线信息   Vector3.Cross即向量c垂直与向量a,b所在的平面 及法线
                Vector3 norm1 = Vector3.Cross(verts[indxs[i1]] - verts[indxs[i0]], verts[indxs[i2]] - verts[indxs[i0]]).normalized;
                Vector3 norm2 = Vector3.Cross(verts[indxs[i4]] - verts[indxs[i3]], verts[indxs[i5]] - verts[indxs[i3]]).normalized;

                norms[indxs[i0]] = norm1;
                norms[indxs[i1]] = norm1;
                norms[indxs[i2]] = norm1;
                norms[indxs[i3]] = norm2;
                norms[indxs[i4]] = norm2;
                norms[indxs[i5]] = norm2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = indxs;
        mesh.UploadMeshData(false);

        obj.GetComponent<MeshFilter>().mesh = mesh;
    }
}
