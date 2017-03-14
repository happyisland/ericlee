using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateDigitalTube.cs
/// Description:升级数码管
/// Time:2016/12/16 15:14:10
/// </summary>
public class UpdateDigitalTube : UpdateSensor
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
        mFilePath = SingletonObject<UpdateManager>.GetInst().Robot_DigitalTube_FilePath;
        mVersion = SingletonObject<UpdateManager>.GetInst().Robot_DigitalTube_Version;
        mSensorType = TopologyPartType.DigitalTube;
        base.Init();
    }
    #endregion
}