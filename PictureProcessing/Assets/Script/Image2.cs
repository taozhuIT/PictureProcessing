using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private RawImage img2;

    [SerializeField]
    private GameObject root;

    [SerializeField]
    private GameObject cube;

    // 二值化亮度阈值
    //private int threshold = 200;

    // 边缘像素点数据
    public class marginPixel
    {
        public int x = 0;
        public int y = 0;
        public Color color = new Color(0, 0, 0, 0);

        public marginPixel(int x_, int y_, Color color_)
        {
            x = x_;
            y = y_;
            color = color_;
        }
    }

    private void Start () {
        ImageGray();
    }
	
	private void Update () {
		
	}

    /// <summary>
    /// 图片灰度化和二值化
    /// </summary>
    private void ImageGray()
    {
        Dictionary<float, int> pixelCounts = new Dictionary<float, int>();
        
        Texture2D tt = TextureTool.TextureToTexture2D(img.texture);
        Texture2D grayTexture = new Texture2D(tt.width, tt.height);

        for (int y = 0; y < tt.height; ++y)
        {
            for (int x = 0; x < tt.width; ++x)
            {
                Color curColor = tt.GetPixel(x, y);
                float g = curColor.r * 0.299f + curColor.g * 0.587f + curColor.b * 0.114f;

                //grayTexture.SetPixel(x, y, new Color(g, g, g, 1f));

                // 记录相同灰度亮度值的像素有多少个
                int lumina = (int)(g * 256);
                if (pixelCounts.ContainsKey(lumina))
                    pixelCounts[lumina] = pixelCounts[lumina] + 1;
                else
                    pixelCounts[lumina] = 1;
            }
        }


        //grayTexture.Apply();
        //img2.texture = grayTexture;
        
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

                grayTexture.SetPixel(x, y, newColor);
            }
        }
        grayTexture.Apply();
        img2.texture = grayTexture;

        OnCreateMesh_4(grayTexture.width, grayTexture.height, grayTexture.GetPixels());
        //OnCreateMesh_3(grayTexture);
    }

    /// <summary>
    /// 查找彩色图像像素边缘（测试）
    /// </summary>
    private void OnGetPixelMarginColorized(Texture2D tt_)
    {
        Texture2D copyText = new Texture2D(tt_.width, tt_.height);
        copyText.SetPixels(tt_.GetPixels());//提取text图片的颜色，赋给copyText
        copyText.Apply();

        List<marginPixel> marginList = new List<marginPixel>();

        for (int y = 0; y < copyText.height; ++y)
        {
            for (int x = 0; x < copyText.width; ++x)
            {
                Color defColor = new Color(0, 0, 0, 0);
                Color upColor = defColor;
                Color leftColor = defColor;
                Color rightColor = defColor;
                Color DownColor = defColor;
                Color leftDownColor = defColor;
                Color leftUpColor = defColor;
                Color rightDownColor = defColor;
                Color rightUpColor = defColor;

                Color curColor = copyText.GetPixel(x, y);

                if (y < copyText.height)
                    upColor = copyText.GetPixel(x, y + 1);

                if (x > 0)
                    leftColor = copyText.GetPixel(x - 1, y);

                if (x < copyText.width)
                    rightColor = copyText.GetPixel(x + 1, y);

                if (y > 0)
                    DownColor = copyText.GetPixel(x, y - 1);

                if (x > 0 && y > 0)
                {
                    leftDownColor = copyText.GetPixel(x - 1, y - 1);
                }

                if (x > 0 && y < copyText.height)
                {
                    leftUpColor = copyText.GetPixel(x - 1, y + 1);
                }

                if (x < copyText.width && y > 0)
                {
                    rightDownColor = copyText.GetPixel(x + 1, y - 1);
                }

                if (x < copyText.width && y < copyText.height)
                {
                    rightUpColor = copyText.GetPixel(x + 1, y + 1);
                }

                bool ximian = leftDownColor != curColor || leftUpColor != curColor || rightDownColor != curColor || rightUpColor != curColor;
                bool zheng = upColor != curColor || leftColor != curColor || rightColor != curColor || DownColor != curColor;

                if (zheng)
                {
                    marginList.Add(new marginPixel(x, y, curColor));
                }
                else
                {
                    marginList.Add(new marginPixel(x, y, new Color(0, 0, 0, 0)));
                }
            }
        }

        for (int i = 0; i < marginList.Count; ++i)
        {
            marginPixel b_i = marginList[i];
            
            if(b_i.color.a != 0)
            {
                marginPixel model = null;
                if (b_i.x <= tt_.width / 2)
                {
                    for (int k = b_i.x + 1; k > b_i.x; --k)
                    {
                        model = marginList.FirstOrDefault(t => (t.x == k && t.y == b_i.y && t.color.a != 0));
                    }
                }
                else
                {
                    for (int k = b_i.x - 1; k < b_i.x; ++k)
                    {
                        model = marginList.FirstOrDefault(t => (t.x == k && t.y == b_i.y && t.color.a != 0));
                    }
                }

                if (model == null)
                    copyText.SetPixel(b_i.x, b_i.y, b_i.color);
                else
                    copyText.SetPixel(b_i.x, b_i.y, new Color(0, 0, 0, 0));
            }
            else
            {
                copyText.SetPixel(b_i.x, b_i.y, b_i.color);
            }
        }

        copyText.Apply();
        img2.texture = copyText;
    }

    /// <summary>
    /// 查找二值化后图像像素边缘（测试）
    /// </summary>
    private void OnGetPixelMargin(Texture2D tt_)
    {
        Texture2D copyText = new Texture2D(tt_.width, tt_.height);
        copyText.SetPixels(tt_.GetPixels());//提取text图片的颜色，赋给copyText
        copyText.Apply();

        List<marginPixel> marginList = new List<marginPixel>();

        for (int y = 0; y < copyText.height; ++y)
        {
            for (int x = 0; x < copyText.width; ++x)
            {
                Color curColor = tt_.GetPixel(x, y);
                Color upColor = new Color(0, 0, 0, 0);
                Color leftColor = new Color(0, 0, 0, 0);
                Color rightColor = new Color(0, 0, 0, 0);
                Color DownColor = new Color(0, 0, 0, 0);
                Color leftDownColor = new Color(0, 0, 0, 0);
                Color leftUpColor = new Color(0, 0, 0, 0);
                Color rightDownColor = new Color(0, 0, 0, 0);
                Color rightUpColor = new Color(0, 0, 0, 0);

                if (y < copyText.height)
                    upColor = copyText.GetPixel(x, y + 1);

                if (x > 0)
                    leftColor = copyText.GetPixel(x - 1, y);

                if (x < copyText.width)
                    rightColor = copyText.GetPixel(x + 1, y);

                if (y > 0)
                    DownColor = copyText.GetPixel(x, y - 1);

                if(x > 0 && y > 0)
                {
                    leftDownColor = copyText.GetPixel(x - 1, y - 1);
                }

                if (x > 0 && y < copyText.height)
                {
                    leftUpColor = copyText.GetPixel(x - 1, y + 1);
                }

                if (x < copyText.width && y > 0)
                {
                    rightDownColor = copyText.GetPixel(x + 1, y - 1);
                }

                if (x < copyText.width && y < copyText.height)
                {
                    rightUpColor = copyText.GetPixel(x + 1, y + 1);
                }

                if (curColor.a == 0)
                {
                    bool ximian = leftDownColor.a != 0 || leftUpColor.a != 0 || rightDownColor.a != 0 || rightUpColor.a != 0;
                    bool zheng = upColor.a != 0 || leftColor.a != 0 || rightColor.a != 0 || DownColor.a != 0;

                    if (zheng)
                        marginList.Add(new marginPixel(x, y, Color.red));
                }
                else
                {
                    marginList.Add(new marginPixel(x, y, new Color(1, 1, 1, 0)));
                }
            }
        }


        for(int i = 0; i < marginList.Count; ++i)
        {
            marginPixel b = marginList[i];
            copyText.SetPixel(b.x, b.y, b.color);
        }

        copyText.Apply();
        img.texture = copyText;

        //OnCreateModel_1(copyText);
    }

    /// <summary>
    /// 生成模型
    /// </summary>
    private void OnCreateModel_1(Texture2D texture_)
    {
        List<marginPixel> pixelS = new List<marginPixel>();
        for (int y = 0; y < texture_.height; ++y)
        {
            for (int x = 0; x < texture_.width; ++x)
            {
                Color pixel = texture_.GetPixel(x, y);
                if (pixel.a != 0)
                {
                    pixelS.Add(new marginPixel(x, y, pixel));
                }
            }
        }

        // 创建模型
        Vector3[] verts = new Vector3[pixelS.Count * 2];
        Vector2[] uvs = new Vector2[pixelS.Count * 2];
        Color[] colors = new Color[pixelS.Count * 2];

        int idx = 0;
        // 找当前点最近的一个点
        for(int i = 0; i < verts.Length / 2; ++i)
        {
            //marginPixel data = pixelS[i];

        }
        //for (int x = 0; x < texture_.width; ++x)
        //{
        //    for(int y = 0; y < texture_.height; ++y)
        //    {
        //        Color pixel = texture_.GetPixel(x, y);
        //        if (pixel.a != 0)
        //        {
        //            // 顶点总数除以2，目的是让mesh初始点在mesh中心
        //            verts[idx] = new Vector3((-texture_.width / 2) + x, (texture_.height / 2) - y, 0);
        //            colors[idx] = pixel; 
        //            // UV的值需要归一化0~1之间，所以需要除以总数
        //            uvs[idx] = new Vector2((float)x / texture_.width, (float)y / texture_.height);
        //            idx++;

        //            // 顶点总数除以2，目的是让mesh初始点在mesh中心
        //            verts[idx] = new Vector3((-texture_.width / 2) + x, (texture_.height / 2) - y, 3);
        //            colors[idx] = pixel;
        //            // UV的值需要归一化0~1之间，所以需要除以总数
        //            uvs[idx] = new Vector2((float)x / texture_.width, (float)y / texture_.height);
        //            idx++;
        //        }
        //    }
        //}
        
        // 顶点索引信息
        int[] indxs = new int[pixelS.Count * 6];
        for (int i = 0; i < indxs.Length; ++i)
        {
            int i0 = (i * 6) + 0;
            int i1 = (i * 6) + 1;
            int i2 = (i * 6) + 2;
            int i3 = (i * 6) + 3;
            int i4 = (i * 6) + 4;
            int i5 = (i * 6) + 5;

            if(i5 < indxs.Length)
            {
                indxs[i0] = i;
                indxs[i1] = i + 1;
                indxs[i2] = i + 2;
                indxs[i3] = i + 2;
                indxs[i4] = i + 3;
                indxs[i5] = i;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = indxs;
        mesh.UploadMeshData(false);

        root.GetComponent<MeshFilter>().mesh = mesh;
    }

    /// <summary>
    /// 生成模型(平面位x，y像素网格生成方式)
    /// </summary>
    marginPixel data_1 = null;
    List<marginPixel> usePixe = new List<marginPixel>();
    List<marginPixel> orderPixe = new List<marginPixel>();
    private void OnCreateMesh_3(Texture2D texture_)
    {
        usePixe.Clear();
        for (int y = 0; y < texture_.height; ++y)
        {
            for (int x = 0; x < texture_.width; ++x)
            {
                Color curColor = texture_.GetPixel(x, y);
                Color upColor = new Color(0, 0, 0, 0);
                Color leftColor = new Color(0, 0, 0, 0);
                Color rightColor = new Color(0, 0, 0, 0);
                Color DownColor = new Color(0, 0, 0, 0);
                Color leftDownColor = new Color(0, 0, 0, 0);
                Color leftUpColor = new Color(0, 0, 0, 0);
                Color rightDownColor = new Color(0, 0, 0, 0);
                Color rightUpColor = new Color(0, 0, 0, 0);

                if (y < texture_.height)
                    upColor = texture_.GetPixel(x, y + 1);

                if (x > 0)
                    leftColor = texture_.GetPixel(x - 1, y);

                if (x < texture_.width)
                    rightColor = texture_.GetPixel(x + 1, y);

                if (y > 0)
                    DownColor = texture_.GetPixel(x, y - 1);

                if (x > 0 && y > 0)
                {
                    leftDownColor = texture_.GetPixel(x - 1, y - 1);
                }

                if (x > 0 && y < texture_.height)
                {
                    leftUpColor = texture_.GetPixel(x - 1, y + 1);
                }

                if (x < texture_.width && y > 0)
                {
                    rightDownColor = texture_.GetPixel(x + 1, y - 1);
                }

                if (x < texture_.width && y < texture_.height)
                {
                    rightUpColor = texture_.GetPixel(x + 1, y + 1);
                }

                if (curColor.a == 0)
                {
                    //bool ximian = leftDownColor.a != 0 || leftUpColor.a != 0 || rightDownColor.a != 0 || rightUpColor.a != 0;
                    bool zheng = upColor.a != 0 || leftColor.a != 0 || rightColor.a != 0 || DownColor.a != 0;

                    if (zheng)
                        usePixe.Add(new marginPixel(x, y, Color.red));
                }
            }
        }

        for (int i = 0; i < usePixe.Count; ++i)
        {
            marginPixel b = usePixe[i];
            texture_.SetPixel(b.x, b.y, b.color);
        }

        texture_.Apply();

        // ----------------------

        data_1 = usePixe[0];

        orderPixe.Clear();
        orderPixe.Add(data_1);
        OnFindPixel(data_1);

        Debug.Log(orderPixe.Count);
        
        Vector3[] verts = new Vector3[orderPixe.Count * 2];
        Vector2[] uvs = new Vector2[orderPixe.Count * 2];
        Color[] colors = new Color[orderPixe.Count * 2];

        int idx = 0;
        for (int i = 0; i < orderPixe.Count; ++i)
        {
            if (i < orderPixe.Count)
            {
                marginPixel data = orderPixe[i];
                // 顶点总数除以2，目的是让mesh初始点在mesh中心
                verts[idx] = new Vector3(data.x, 0, data.y);
                colors[idx] = data.color;
                // UV的值需要归一化0~1之间，所以需要除以总数
                uvs[idx] = new Vector2((float)data.x / texture_.width, (float)data.y / texture_.height);
                idx++;

                // 顶点总数除以2，目的是让mesh初始点在mesh中心
                verts[idx] = new Vector3(data.x, 1, data.y);
                colors[idx] = data.color;
                // UV的值需要归一化0~1之间，所以需要除以总数
                uvs[idx] = new Vector2((float)data.x / texture_.width, (float)data.y / texture_.height);
                idx++;
            }
        }
        
        // 顶点索引信息
        int[] indxs = new int[orderPixe.Count * 6];
        for(int i = 0; i < indxs.Length; ++i)
        {
            int i0 = (i * 6) + 0;
            int i1 = (i * 6) + 1;
            int i2 = (i * 6) + 2;
            int i3 = (i * 6) + 3;
            int i4 = (i * 6) + 4;
            int i5 = (i * 6) + 5;

            if (i3 < indxs.Length)
            {
                indxs[i0] = i;
                indxs[i1] = i + 1;
                indxs[i2] = i + 2;
                indxs[i3] = i + 2;
                indxs[i4] = i + 3;
                indxs[i5] = i;
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = indxs;
        mesh.UploadMeshData(false);

        root.GetComponent<MeshFilter>().mesh = mesh;
    }

    /// <summary>
    /// 查找像素点
    /// </summary>
    
    private void OnFindPixel(marginPixel datas_)
    {
        int index = 0;
        marginPixel data_ = null;
        while (true)
        {
            if(data_ == null)
                data_ = datas_;

            //marginPixel findData = findData = usePixe.Find((marginPixel user) => (user.x == data_.x && user.y == (data_.y + 1) || (user.x == (data_.x + 1) && user.y == (data_.y + 1) || user.x == (data_.x + 1) && user.y == data_.y || user.x == (data_.x + 1) && user.y == (data_.y - 1) || user.x == data_.x && user.y == (data_.y - 1) || user.x == (data_.x - 1) && user.y == (data_.y - 1) || user.x == (data_.x - 1) && user.y == data_.y || user.x == (data_.x - 1) && user.y == (data_.y + 1))));
            marginPixel findData = null;
            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == data_.x && user.y == (data_.y + 1));
            }

            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == (data_.x + 1) && user.y == (data_.y + 1));
            }

            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == (data_.x + 1) && user.y == data_.y);
            }

            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == (data_.x + 1) && user.y == (data_.y - 1));
            }
            //-----------------------
            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == data_.x && user.y == (data_.y - 1));
            }

            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == (data_.x - 1) && user.y == (data_.y - 1));
            }

            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == (data_.x - 1) && user.y == data_.y);
            }

            if (findData == null)
            {
                findData = usePixe.Find((marginPixel user) => user.x == (data_.x - 1) && user.y == (data_.y + 1));
            }

            // 查找八个方向完成
            if (findData != null && index < usePixe.Count)
            {
                index++;
                data_ = findData;
                orderPixe.Add(findData);
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 二值化图像生成模型(镂空生成方式)(平面位x，y像素网格生成方式)
    /// </summary>
    private void OnCreateMesh_4(int sizeX_, int sizeY_, Color[] pixes_)
    {
        // 创建模型
        Vector3[] verts = new Vector3[sizeX_ * sizeY_];
        Vector2[] uvs = new Vector2[sizeX_ * sizeY_];
        Color[] colors = new Color[sizeX_ * sizeY_];

        List<Color> testBBB = new List<Color>();

        for (int y = 0; y < sizeY_; ++y)
        {
            for (int x = 0; x < sizeX_; ++x)
            {
                int idx = (y * sizeX_) + x;
                float px = x * 1;
                float py = y * 1;
                Color pixel = pixes_[(int)px + (sizeY_ - 1 - (int)py) * sizeX_];
                
                // 顶点总数除以2，目的是让mesh初始点在mesh中心
                verts[idx] = new Vector3(-sizeX_ / 2 + px, pixel.a * 5, sizeY_ / 2 - py);
                colors[idx] = pixel; //Color.white;
                // UV的值需要归一化0~1之间，所以需要除以总数
                uvs[idx] = new Vector2((float)x / sizeX_, (float)y / sizeY_);

                testBBB.Add(pixel);
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
                
                // 处理镂空(思路是找到后面右上和右下两个顶点判断颜色a值是否不为0，不为0则计算顶点索引)
                if(testBBB[vidx + 1].a != 0 || testBBB[vidx + 1 + sizeX_].a != 0)
                {
                    int offset6 = iidx * 6;
                    int i0 = offset6 + 0;
                    int i1 = offset6 + 1;
                    int i2 = offset6 + 2;
                    int i3 = offset6 + 3;
                    int i4 = offset6 + 4;
                    int i5 = offset6 + 5;

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
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = indxs;
        mesh.UploadMeshData(false);

        root.GetComponent<MeshFilter>().mesh = mesh;
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
                verts[idx] = new Vector3(-sizeX_ / 2 + px, pixel.a * 5, sizeY_ / 2 - py);
                colors[idx] = pixel; //Color.white;
                                        // UV的值需要归一化0~1之间，所以需要除以总数
                uvs[idx] = new Vector2((float)x / sizeX_, (float)y / sizeY_);

                idx++;
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

        root.GetComponent<MeshFilter>().mesh = mesh;
    }
}
