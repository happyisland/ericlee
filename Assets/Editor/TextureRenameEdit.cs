using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

/// <summary>
/// Author:xj
/// FileName:TextureRenameEdit.cs
/// Description:
/// Time:2016/4/15 10:42:47
/// </summary>
public class TextureRenameEdit : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    [MenuItem("MyTool/序列帧改名", false, 2)]
    public static void OpenTextureRenameEdit()
    {
        TextureRenameEdit windows = EditorWindow.GetWindow<TextureRenameEdit>(true, "TextureRenameEdit");
        windows.position = new Rect(400, 300, 500, 550);


    }

    #endregion

    #region 其他函数
    string mFolderPath = string.Empty;
    void OnGUI()
    {
        string folderPath = string.Empty;
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100), GUILayout.Height(20)))
        {
            folderPath = EditorUtility.OpenFolderPanel("选择文件夹", mFolderPath, string.Empty);
        }
        if (!string.IsNullOrEmpty(folderPath))
        {
            mFolderPath = folderPath;
            string[] files = Directory.GetFiles(folderPath);
            if (null != files)
            {
                for (int i = 0, imax = files.Length; i < imax; ++i)
                {
                    string path = Path.GetDirectoryName(files[i]);
                    string ex = Path.GetExtension(files[i]);
                    string fileName = Path.GetFileNameWithoutExtension(files[i]);
                    string[] tmp = fileName.Split('_');
                    if (null != tmp && tmp.Length == 2 && PublicFunction.IsInteger(tmp[1]))
                    {
                        int num = int.Parse(tmp[1]);
                        string newName = tmp[0] + "_" + num;
                        File.Move(files[i], path + "/" + newName + ex);
                    }
                }
            }
            
        }


    }
    #endregion
}