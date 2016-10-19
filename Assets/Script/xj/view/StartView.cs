using Game;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:StartView.cs
/// Description:
/// Time:2016/3/4 11:10:30
/// </summary>
public class StartView : MonoBehaviour
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数 
    #endregion

    #region 其他函数
    void Awake()
    {
        UITexture iosTex = GameHelper.FindChildComponent<UITexture>(transform, "tex");
        if (null != iosTex)
        {
            int screenWidth = PublicFunction.GetWidth();
            int screenHeight = PublicFunction.GetHeight();

            float oldWidth = 1334.0f;
            float oldHeight = 750.0f;
            int height = (int)(screenHeight * oldWidth / screenWidth);
            if (height >= oldHeight)
            {
                iosTex.height = screenHeight;
                iosTex.width = Mathf.CeilToInt(screenHeight * oldWidth / oldHeight);
            }
            else
            {
                iosTex.height = Mathf.CeilToInt(screenWidth * oldHeight / oldWidth);
                iosTex.width = screenWidth;
            }
        }
    }
#endregion
}