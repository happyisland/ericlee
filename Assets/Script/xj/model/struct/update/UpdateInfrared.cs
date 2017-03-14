using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateInfrared.cs
/// Description:升级红外传感器
/// Time:2016/12/16 14:37:30
/// </summary>
public class UpdateInfrared : UpdateSensor
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
        mFilePath = SingletonObject<UpdateManager>.GetInst().Robot_Infrared_FilePath;
        mVersion = SingletonObject<UpdateManager>.GetInst().Robot_Infrared_Version;
        mSensorType = TopologyPartType.Infrared;
        base.Init();
    }
    #endregion
}