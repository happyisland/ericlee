using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateTouch.cs
/// Description:升级触碰
/// Time:2016/12/16 15:08:26
/// </summary>
public class UpdateTouch : UpdateSensor
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    protected override void Init()
    {
        mFilePath = SingletonObject<UpdateManager>.GetInst().Robot_Touch_FilePath;
        mVersion = SingletonObject<UpdateManager>.GetInst().Robot_Touch_Version;
        mSensorType = TopologyPartType.Touch;
        base.Init();
    }
    #endregion
}