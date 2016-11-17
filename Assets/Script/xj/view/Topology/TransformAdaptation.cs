using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TransformAdaptation.cs
/// Description:
/// Time:2016/9/22 15:19:30
/// </summary>
public class TransformAdaptation : MonoBehaviour
{
    #region 公有属性
    public float ScalingFactor = 0.9f;
    public bool isRecalculate = false;
    #endregion

    #region 其他属性
    Transform mTrans;
    Vector4 mRect;
    #endregion

    #region 公有函数
    public void Recalculate(Vector4 rect)
    {
        isRecalculate = true;
        mRect = rect;
    }
    #endregion

    #region 其他函数
    void Start()
    {
        mTrans = transform;
        mRect = new Vector4(0, 0, PublicFunction.GetWidth() * ScalingFactor, PublicFunction.GetHeight() * ScalingFactor);
    }

    void Update()
    {
        if (isRecalculate)
        {
            isRecalculate = false;
            Camera camera = NGUITools.FindInParents<Camera>(mTrans);
            if (null != camera)
            {
                PublicFunction.RemoveToCenter(mTrans, camera.transform, ScalingFactor, mRect, false);
            }
            
        }
    }
    #endregion
}