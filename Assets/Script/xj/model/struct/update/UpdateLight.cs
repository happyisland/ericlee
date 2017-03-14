using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateLight.cs
/// Description:升级灯光
/// Time:2016/12/28 16:48:24
/// </summary>
public class UpdateLight : UpdateSensor
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
        mFilePath = SingletonObject<UpdateManager>.GetInst().Robot_Light_FilePath;
        mVersion = SingletonObject<UpdateManager>.GetInst().Robot_Light_Version;
        mSensorType = TopologyPartType.Light;
        base.Init();
    }
    #endregion
}