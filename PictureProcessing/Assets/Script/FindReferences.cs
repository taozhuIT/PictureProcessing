using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

public class FindReferences
{
    static int index = 0;
    static List<FileSystemInfo> imageList = new List<FileSystemInfo>();

    [MenuItem("Tools/查找文件下图片是否未被引用", false, 10)]
    static private void Find()
    {
        index = 0;
        imageList.Clear();

        // 得到编辑器下鼠标选中的文件夹
        string[] strs = Selection.assetGUIDs;
        string path = AssetDatabase.GUIDToAssetPath(strs[0]);

        if (!string.IsNullOrEmpty(path))
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileSystemInfo[] systemInfo = info.GetFileSystemInfos();
            for(int i = 0; i < systemInfo.Length; ++i)
            {
                if (systemInfo[i].Extension != ".meta")
                {
                    imageList.Add(systemInfo[i]);
                }
            }

            // 总的当前匹配数量
            string FullName = imageList[index].FullName;
            string filePath = FullName.Substring(FullName.IndexOf("Assets"));
            bbbb(filePath);
        }
    }

    static private void bbbb(string filePath)
    {
        string guid = AssetDatabase.AssetPathToGUID(filePath);
        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        
        // 单个图片匹配数量
        int startIndex = 0;
        bool isUse = false;
        EditorApplication.update = delegate ()
        {
            if (startIndex < files.Length)
            {
                string file = files[startIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    isUse = true;
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    if (!isUse)
                        Debug.Log(filePath + "   没有被使用");

                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;

                    index++;
                    if (index < imageList.Count)
                    {
                        string FullName = imageList[index].FullName;
                        string filePath_x = FullName.Substring(FullName.IndexOf("Assets"));
                        bbbb(filePath_x);
                    }
                }
            }
        };
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}