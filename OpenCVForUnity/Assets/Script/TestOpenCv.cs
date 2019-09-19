using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;

public class TestOpenCv : MonoBehaviour
{
    // 摄像头设备名称
    public string deviceName;
    // 接收返回的图片数据
    private WebCamTexture webCamtex;
    // 将摄像头纹理渲染到RawImage
    public RawImage rawImage;
    // 完成人脸识别获取的图
    public RawImage detectFaceImage;

    // -----------人脸识别代码--------------
    private CascadeClassifier haarCascade = null;
    private Mat haarResult;
    private byte[] bs;
    private Texture2D rt;

    void Start ()
    {
        // 加载官方训练集(人脸识别代码)
        haarCascade = new CascadeClassifier(Application.streamingAssetsPath + "/haarcascades/haarcascade_frontalface_alt2.xml");
        // 调用摄像头
        StartCoroutine(startWebCam());
    }

    private void Update ()
    {
        
    }

    private void FixedUpdate()
    {
        
    }

    private void LateUpdate()
    {
        OnPlayFace();
    }

    /// <summary>
    /// 打开摄像头
    /// </summary>
    public IEnumerator startWebCam()
    {
        // 调用摄像头信息
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            deviceName = devices[0].name;
            webCamtex = new WebCamTexture(deviceName, 700, 700, 12);
            webCamtex.Play();

            rawImage.texture = webCamtex;
        }
        else
        {
            Debug.Log("没有找到摄像头设备");
        }
    }

    // -----------人脸识别代码--------------
    /// <summary>
    /// 执行人脸检测
    /// </summary>
    public void OnPlayFace()
    {
        if(webCamtex != null)
        {
            haarResult = DetectFace(haarCascade, GetTexture2D(webCamtex));
            bs = haarResult.ToBytes(".png");

            rt = new Texture2D(webCamtex.width, webCamtex.height);
            rt.LoadImage(bs);
            rt.Apply();

            detectFaceImage.texture = rt;
        }
    }

    Mat result;
    OpenCvSharp.Rect[] faces;
    Mat src;
    Mat gray = new Mat();
    Size axes = new Size();

    Point center = new Point();
    Point upPoint = new Point();
    Point downPoint = new Point();

    /// <summary>
    /// 调用OpenCvSharp处理人脸识别
    /// </summary>
    private Mat DetectFace(CascadeClassifier cascade, Texture2D tex2D_)
    {
        src = Mat.FromImageData(tex2D_.EncodeToPNG(), ImreadModes.Color);
        result = src.Clone();
        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        src = null;
        // Detect faces (openCV库人脸识别函数)
        faces = cascade.DetectMultiScale(gray, 1.08, 4, HaarDetectionType.ScaleImage, new Size(10, 10));

        //高斯滤波
        Mat resultsss = new Mat();
        Cv2.GaussianBlur(src, resultsss, new Size(3, 3), 0, 0);

        /*
        // 给识别区域画出范围
        Debug.Log(faces.Length);
        for (int i = 0; i < faces.Length; i++)
        {
            // 画椭圆
            //center.X = (int)(faces[i].X + faces[i].Width * 0.5);
            //center.Y = (int)(faces[i].Y + faces[i].Height * 0.5);
            //axes.Width = (int)(faces[i].Width * 0.5);
            //axes.Height = (int)(faces[i].Height * 0.5);
            //Cv2.Ellipse(result, center, axes, 0, 0, 360, new Scalar(0, 0, 255), 4);

            // 画矩形
            upPoint.X = faces[i].X;
            upPoint.Y = faces[i].Y;
            downPoint.X = (int)(faces[i].X + faces[i].Width);
            downPoint.Y = (int)(faces[i].Y + faces[i].Height);
            Cv2.Rectangle(result, upPoint, downPoint, new Scalar(0, 0, 255), 3, LineTypes.Link8, 0);
        }
        */
        return result;
    }

    /// <summary>
    /// 获取摄像头视频流一帧保存成Texture2D
    /// </summary>
    Texture2D temp;
    Texture2D GetTexture2D(WebCamTexture wct)
    {
        temp = new Texture2D(wct.width, wct.height);
        temp.SetPixels(wct.GetPixels());
        temp.Apply();
        return temp;
    }
}
