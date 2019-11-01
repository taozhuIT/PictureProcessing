using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 学习mesh生成基础
/// </summary>
public class CreateMesh2 : MonoBehaviour
{
    [SerializeField]
    private MeshFilter meshObj = null;
    
	private void Start ()
    {
        OnCreateMesh();
    }
	
	private void Update ()
    {
	}

    /// <summary>
    /// 创建mesh
    /// </summary>
    private void OnCreateMesh()
    {
        Vector3[] vertices = new Vector3[3];
        Color[] colors = new Color[3];
        Vector2[] uvs = new Vector2[3];

        // 顶点索引信息
        int[] index = new int[vertices.Length];
        // 顶点法线信息
        //Vector3[] norms = new Vector3[vertices.Length];

        vertices[0] = new Vector3(-1f, 0f, 0f);
        vertices[1] = new Vector3(0f, 1f, 0f);
        vertices[2] = new Vector3(1f, 0f, 0f);

        colors[0] = Color.red;
        colors[1] = Color.red;
        colors[2] = Color.red;

        uvs[0] = new Vector2(-1f, 0f);
        uvs[1] = new Vector2(0f, 1f);
        uvs[2] = new Vector2(1f, 0f);

        index[0] = 0;
        index[1] = 1;
        index[2] = 2;

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        //mesh.normals = norms;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = index;
        mesh.UploadMeshData(false);
        meshObj.mesh = mesh;
    }
}
