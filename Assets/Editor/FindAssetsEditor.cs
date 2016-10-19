using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:FindAssetsEditor.cs
/// Description:
/// Time:2016/9/20 10:26:54
/// </summary>
public class FindAssetsEditor : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    [MenuItem("MyTool/查找资源")]
    public static void OpenFindAssetsEditor()
    {
        FindAssetsEditor windows = EditorWindow.GetWindow<FindAssetsEditor>(true, "FindAssetsEditor");
        windows.position = new Rect(400, 300, 500, 550);
    }
    #endregion

    Type[] types = new Type[] { typeof(UISprite), typeof(UITexture) };
    string[] inputText = new string[] { string.Empty, string.Empty };
    #region 其他函数
    void OnGUI()
    {
        GUILayout.BeginVertical();

        for (int i = 0, imax = types.Length; i < imax; ++i)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("查找" + types[i].Name);
            inputText[i] = EditorGUILayout.TextField(inputText[i], GUILayout.Width(200));
            if (GUILayout.Button("查找", GUILayout.Width(60)))
            {
                if (string.IsNullOrEmpty(inputText[i]))
                {
                    EditorUtility.DisplayDialog("错误", "请输入查找的文件", "确定");
                    continue;
                }
                UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
                if (null == objs || objs.Length < 1)
                {
                    EditorUtility.DisplayDialog("错误", "请选择需要查找的文件或文件夹", "确定");
                }
                else
                {
                    for (int objIndex = 0, objMax = objs.Length; objIndex < objMax; ++objIndex)
                    {
                        if (objs[objIndex] is GameObject)
                        {
                            GameObject tmpObj = objs[objIndex] as GameObject;
                            Component[] coms = tmpObj.GetComponentsInChildren(types[i], true);
                            for (int comIndex = 0, comMax = coms.Length; comIndex < comMax; ++comIndex)
                            {
                                if (types[i] == typeof(UISprite))
                                {
                                    if ((coms[comIndex] as UISprite).spriteName == inputText[i])
                                    {
                                        Debug.Log(AssetDatabase.GetAssetPath(tmpObj));
                                    }
                                }
                                else if (types[i] == typeof(UITexture))
                                {
                                    Texture tex = (coms[comIndex] as UITexture).mainTexture;
                                    if (null != tex && tex.name == inputText[i])
                                    {
                                        Debug.Log(AssetDatabase.GetAssetPath(tmpObj));
                                    }
                                }
                            }
                        }
                    }
                    EditorUtility.DisplayDialog("提示", "查找完毕，请查看控制台输出", "确定");
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
    #endregion
}