using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:UpdateSpeaker.cs
/// Description:升级蓝牙喇叭
/// Time:2016/12/16 15:11:32
/// </summary>
public class UpdateSpeaker : UpdateSensor
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
        mFilePath = SingletonObject<UpdateManager>.GetInst().Robot_Speaker_FilePath;
        mVersion = SingletonObject<UpdateManager>.GetInst().Robot_Speaker_Version;
        mSensorType = TopologyPartType.Speaker;
        base.Init();
    }
    #endregion
}