using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 根据图片生成模型
/// </summary>
public class CreateMesh : MonoBehaviour 
{
    [SerializeField]
    private RawImage textRwa;
    [SerializeField]
    private RawImage showRwa;

    [SerializeField]
    private MeshFilter meshRoot;
    [SerializeField]
    private GameObject cube;

    // 原始texture2d数据
    private Texture2D scTexture = null;
    // 处理后的texture2d数据
    private Texture2D newTexture = null;

    /// <summary>
    /// 像素数据
    /// </summary>
    private class texData
    {
        public int x;
        public int y;
        public Color color;

        /// <summary>
        /// 构造
        /// </summary>
        public texData(int x_, int y_, Color color_)
        {
            x = x_;
            y = y_;
            color = color_;
        }
    }

    private List<texData> texDataList = new List<texData>();
    private List<texData> marginDataList = new List<texData>();
    private List<texData> orderMarginDataList = new List<texData>();

    /// <summary>
    /// 起始
    /// </summary>
	void Start () 
    {
        scTexture = TextureTool.OnGetTextureGray(textRwa.texture);
        OnFindMarginPoint();
	}

    /// <summary>
    /// 找到边缘点
    /// </summary>
    private void OnFindMarginPoint()
    {
        texDataList.Clear();
        marginDataList.Clear();

        for(int y = 0; y < scTexture.height; ++y)
        {
            for (int x = 0; x < scTexture.height; ++x)
            {
                Color cl = scTexture.GetPixel(x, y);
                if(cl.a == 0)
                {
                    Color leftCl = scTexture.GetPixel(x - 1, y);
                    Color rightCl = scTexture.GetPixel(x + 1, y);
                    Color upCl = scTexture.GetPixel(x, y - 1);
                    Color downCl = scTexture.GetPixel(x, y + 1);

                    if (leftCl.a != 0 || rightCl.a != 0 || upCl.a != 0 || downCl.a != 0)
                    {
                        texData redCl = new texData(x, y, Color.green);
                        texDataList.Add(redCl);
                        marginDataList.Add(redCl);
                    }
                    else
                    {
                        texDataList.Add(new texData(x, y, cl));
                    }
                }
                else
                {
                    texDataList.Add(new texData(x, y, cl));
                }
            }
        }

        //Debug.Log(texDataList.Count);
        for (int i = 0; i < texDataList.Count; ++i)
        {
            texData data = texDataList[i];
            scTexture.SetPixel(data.x, data.y, data.color);
        }
        scTexture.Apply();
        showRwa.texture = scTexture;

        // -----------------------------
        orderMarginDataList.Clear();
        OnFindMarginPointOrder();
        Debug.Log(orderMarginDataList.Count);
        //------------------------------
        // 验证获取顺序
        //StartCoroutine(OnVerifyOrderTest());

        // 创建mesh
        OnCreateMesh();
    }

    /// <summary>
    /// 重新获取边缘点顺序
    /// </summary>
    private void OnFindMarginPointOrder()
    {
        int index = 0;
        texData initData = marginDataList[0];
        orderMarginDataList.Add(initData);
        
        while(true)
        {
            texData findData = null;
            List<texData> curTest = bbbbbb(initData);
            if (curTest.Count > 1)
            {
                for(int i = 0; i < curTest.Count; ++i)
                {
                    List<texData> dataList = bbbbbb(curTest[i]);
                    if(dataList.Count > 0)
                    {
                        findData = curTest[i];
                        break;
                    }
                }
            }
            else
            {
                if(curTest.Count > 0)
                    findData = curTest[0];
            }
 
            if (findData != null && index < marginDataList.Count && (initData.x != findData.x || initData.y != findData.y))
            {
                index++;
                orderMarginDataList.Add(findData);
                initData = findData;
            }
            else
            {
                break;
            }
        }
    }

    private List<texData> bbbbbb(texData initData_)
    {
        List<texData> curTest = new List<texData>();

        texData findDataDown = null;
        findDataDown = OnCheckPointMat(marginDataList.Find((texData data) => data.x == initData_.x && data.y == (initData_.y - 1)));
        if (findDataDown != null)
            curTest.Add(findDataDown);

        texData findDataRight = null;
        findDataRight = OnCheckPointMat(marginDataList.Find((texData data) => data.x == (initData_.x - 1) && data.y == initData_.y));
        if (findDataRight != null)
            curTest.Add(findDataRight);

        texData findDataUp = null;
        findDataUp = OnCheckPointMat(marginDataList.Find((texData data) => data.x == initData_.x && data.y == (initData_.y + 1)));
        if (findDataUp != null)
            curTest.Add(findDataUp);

        texData findDataLeft = null;
        findDataLeft = OnCheckPointMat(marginDataList.Find((texData data) => data.x == (initData_.x + 1) && data.y == initData_.y));
        if (findDataLeft != null)
            curTest.Add(findDataLeft);

        texData findDataRightUp = null;
        findDataRightUp = OnCheckPointMat(marginDataList.Find((texData data) => data.x == (initData_.x + 1) && data.y == (initData_.y + 1)));
        if (findDataRightUp != null)
            curTest.Add(findDataRightUp);

        texData findDataRightDown = null;
        findDataRightDown = OnCheckPointMat(marginDataList.Find((texData data) => data.x == (initData_.x + 1) && data.y == (initData_.y - 1)));
        if (findDataRightDown != null)
            curTest.Add(findDataRightDown);

        texData findDataLeftDown = null;
        findDataLeftDown = OnCheckPointMat(marginDataList.Find((texData data) => data.x == (initData_.x - 1) && data.y == (initData_.y - 1)));
        if (findDataLeftDown != null)
            curTest.Add(findDataLeftDown);

        texData findDataLeftUp = null;
        findDataLeftUp = OnCheckPointMat(marginDataList.Find((texData data) => data.x == (initData_.x - 1) && data.y == (initData_.y + 1)));
        if (findDataLeftUp != null)
            curTest.Add(findDataLeftUp);

        return curTest;
    }

    /// <summary>
    /// 检查点是否已经记录
    /// </summary>
    private texData OnCheckPointMat(texData data_)
    {
        texData retData = data_;
        if (data_ != null)
        {
            texData data = orderMarginDataList.Find((texData datas) => datas.x == data_.x && datas.y == data_.y);
            if (data != null)
                retData = null;
        }

        return retData;       
    }

    /// <summary>
    /// 创建mesh面片
    /// </summary>
    private void OnCreateMesh()
    {
        Vector3[] vertices = new Vector3[orderMarginDataList.Count * 2];
        int idx = 0;
        for (int i = 0; i < orderMarginDataList.Count; ++i)
        {
            texData data = orderMarginDataList[i];
            vertices[idx] = new Vector3(data.x * 0.01f, 0, data.y * 0.01f);
            OnTestCreateCube(vertices[idx]);
            idx++;

            vertices[idx] = new Vector3(data.x * 0.01f , 0.1f, data.y * 0.01f);
            OnTestCreateCube(vertices[idx]);
            idx++;
        }

        Debug.Log(vertices.Length);
        
        int[] index = new int[vertices.Length * 3];
        int count = vertices.Length / 2;
        for (int i = 0; i < count; ++i)
        {
            try
            {
                int offset6 = i * 6;
                int offset2 = i * 2;

                index[offset6] = offset2;
                if((offset2 + 1) < vertices.Length)
                    index[offset6 + 1] = offset2 + 1;
                if ((offset2 + 2) < vertices.Length)
                    index[offset6 + 2] = offset2 + 2;
                if ((offset2 + 1) < vertices.Length)
                    index[offset6 + 3] = offset2 + 1;
                if ((offset2 + 3) < vertices.Length)
                    index[offset6 + 4] = offset2 + 3;
                if ((offset2 + 2) < vertices.Length)
                    index[offset6 + 5] = offset2 + 2;

                //index[offset6 + 6] = offset2;
                //if ((offset2 + 3) <= vertices.Length)
                //    index[offset6 + 7] = offset2 + 3;
                //if ((offset2 + 2) <= vertices.Length)
                //    index[offset6 + 8] = offset2 + 2;
                //if ((offset2 + 2) <= vertices.Length)
                //    index[offset6 + 9] = offset2 + 2;
                //if ((offset2) <= vertices.Length)
                //    index[offset6 + 10] = offset2;
                //if ((offset2 + 3) <= vertices.Length)
                //    index[offset6 + 11] = offset2 + 3;
            }
            catch
            {

            }
        }

        Debug.Log(index.Length);
        
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = index;
        mesh.UploadMeshData(false);
        meshRoot.mesh = mesh;
    }

    /// <summary>
    /// 测试函数
    /// </summary>
    private void OnTestCreateCube(Vector3 pos_)
    {
        GameObject obj_1 = Instantiate(cube);
        obj_1.transform.SetParent(meshRoot.transform);
        obj_1.transform.position = pos_;
        obj_1.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        obj_1.SetActive(true);
    }

    /// <summary>
    /// 验证点顺序是否正确(测试)
    /// </summary>
    private IEnumerator OnVerifyOrderTest()
    {
        for (int i = 0; i < orderMarginDataList.Count; ++i)
        {
            yield return new WaitForEndOfFrame();
            texData data = orderMarginDataList[i];

            //Debug.Log(data.x + "   " + data.y);

            scTexture.SetPixel(data.x, data.y, Color.red);
            scTexture.Apply();
            showRwa.texture = scTexture;
        }
    }


}
