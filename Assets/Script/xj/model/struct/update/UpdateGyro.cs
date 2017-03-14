using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateGyro.cs
/// Description:升级陀螺仪
/// Time:2016/12/16 15:10:12
/// </summary>
public class UpdateGyro : UpdateSensor
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
        mFilePath = SingletonObject<UpdateManager>.GetInst().Robot_Gyro_FilePath;
        mVersion = SingletonObject<UpdateManager>.GetInst().Robot_Gyro_Version;
        mSensorType = TopologyPartType.Gyro;
        base.Init();
    }
    #endregion
}