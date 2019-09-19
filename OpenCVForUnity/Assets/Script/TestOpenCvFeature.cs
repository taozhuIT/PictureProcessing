using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

public class TestOpenCvFeature : MonoBehaviour {
    public Image m_srcImage;

    void Start ()
    {
    }

    void Update ()
    {
		
	}

    /// <summary>
    /// 随机一个路径
    /// </summary>
    public void OnRandomPath()
    {
        List<string> pathList = new List<string>();
        pathList.Add("/Textures/p2.jpg");
        pathList.Add("/Textures/p3.jpg");
        pathList.Add("/Textures/p5.jpg");
        pathList.Add("/Textures/p6.jpg");

        int index = Random.Range(0, pathList.Count);

        OnOrb(pathList[index]);
    }

    //void Detect()
    //{
    //    var gray = new Mat(Application.streamingAssetsPath + "/Textures/p1.jpg", ImreadModes.Grayscale);
    //    KeyPoint[] keyPoints = null;
    //    using (var orb = ORB.Create(500))
    //    {
    //        keyPoints = orb.Detect(gray);
    //        Debug.Log($"KeyPoint has {keyPoints.Length} items.");
    //    }
    //}

    //void DetectAndCompute()
    //{
    //    var gray = new Mat(Application.streamingAssetsPath + "/Textures/p1.jpg", ImreadModes.Grayscale);
    //    KeyPoint[] keyPoints = null;
    //    using (var orb = ORB.Create(500))
    //    using (Mat descriptor = new Mat())
    //    {
    //        orb.DetectAndCompute(gray, new Mat(), out keyPoints, descriptor);
    //        Debug.Log($"keyPoints has {keyPoints.Length} items.");
    //        Debug.Log($"descriptor has {descriptor.Rows} items.");
    //    }
    //}

    Mat dstMat;
    Texture2D t2d;
    /// <summary>
    /// Orb特征提取
    /// </summary>
    void OnOrb(string path2_)
    {
        Debug.Log(path2_);

        Mat image01 = Cv2.ImRead(Application.streamingAssetsPath + "/Textures/p1.jpg");
        Mat image02 = Cv2.ImRead(Application.streamingAssetsPath + path2_);

        //灰度图转换
        Mat image1 = new Mat(), image2 = new Mat();
        Cv2.CvtColor(image01, image1, ColorConversionCodes.RGB2GRAY);
        Cv2.CvtColor(image02, image2, ColorConversionCodes.RGB2GRAY);

        KeyPoint[] keyPoint1 = null;
        KeyPoint[] keyPoint2 = null;
        using (ORB orb = ORB.Create(500))
        using (Mat descriptor1 = new Mat())
        using (Mat descriptor2 = new Mat())
        using (var matcher = new BFMatcher())
        {
            //特征点提取并计算
            orb.DetectAndCompute(image1, new Mat(), out keyPoint1, descriptor1);
            orb.DetectAndCompute(image2, new Mat(), out keyPoint2, descriptor2);
            Debug.Log("image1 keyPoints:  " + keyPoint1.Length + "   descriptor:  " + descriptor1.Rows);
            Debug.Log("image2 keyPoints:  " + keyPoint2.Length + "   descriptor:  " + descriptor2.Rows);

            //特征点匹配
            //DMatch[] matchePoint = null;
            //matchePoint = matcher.Match(descriptor2, descriptor1);
            //Debug.Log(matchePoint.Length);

            // Lowe's algorithm,获取优秀匹配点
            DMatch[][] matchePoints = null;
            DMatch[][] matchePointssss = null;
            // 使用knnMatch最邻近匹配
            matchePoints = matcher.KnnMatch(descriptor1, descriptor2, 2);

            List<DMatch> GoodMatchePoints = new List<DMatch>();
            for (int i = 0; i < matchePoints.Length; i++)
            {
                float minRatio = 0.9f;
                float disRation = matchePoints[i][0].Distance / matchePoints[i][1].Distance;
                if(disRation < minRatio)
                    GoodMatchePoints.Add(matchePoints[i][0]);
            }
            
            DMatch[] matchePointss = GoodMatchePoints.ToArray();

            Debug.Log("手写文字图总特征点： " + descriptor2.Rows);
            Debug.Log("匹配符合的文字特征点： " + matchePointss.Length);
            
            //float zongVa = (matchePoints.Length / 2);
            Debug.Log("相识度比例：  " + ((float)matchePointss.Length / descriptor2.Rows));

            dstMat = new Mat();
            Cv2.DrawMatches(image01, keyPoint1, image02, keyPoint2, matchePointss, dstMat);
            t2d = MatToTexture2D(dstMat);
        }

        Sprite dst_sp = Sprite.Create(t2d, new UnityEngine.Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        m_srcImage.sprite = dst_sp;
        m_srcImage.preserveAspect = true;
    }

    /// <summary>
    /// Mat转Texture2D
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static Texture2D MatToTexture2D(Mat mat)
    {
        Texture2D t2d = new Texture2D(mat.Width, mat.Height);
        t2d.LoadImage(mat.ToBytes());
        t2d.Apply();
        //赋值完后为什么要Apply
        //因为在贴图更改像素时并不是直接对显存进行更改，而是在另外一个内存空间中更改，这时候GPU还会实时读取旧的贴图位置。
        //当Apply后，CPU会告诉GPU你要换个地方读贴图了。
        return t2d;
    }
}
