//#define Update_Test
//#define Update_Test1
using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:UpdateManager.cs
/// Description:升级管理
/// Time:2016/11/24 10:45:01
/// </summary>
public class UpdateManager : SingletonObject<UpdateManager>
{
    #region 公有属性

#if Update_Test
    public string Robot_System_Version = "Jimu_p1.30";
    public string Robot_System_FilePath = "Jimu2primary_P1.30";
    public string Robot_Servo_Version = "41161301";
    public string Robot_Servo_FilePath = "jimu2_app_41161301";
    public string Robot_Infrared_Version = "14161215";
    public string Robot_Infrared_FilePath = "Jimu2_infrared_sensor_14161215";
    public string Robot_Touch_Version = "14161215";
    public string Robot_Touch_FilePath = "Jimu2_touch_sensor_14161215";
    public string Robot_Gyro_Version = "13161215";
    public string Robot_Gyro_FilePath = "Jimu2_gyros_sensor_13161215";
    public string Robot_Speaker_Version = "01161215";
    public string Robot_Speaker_FilePath = "Jimu2_speaker_sensor_01161215";
    public string Robot_DigitalTube_Version = "02161215";
    public string Robot_DigitalTube_FilePath = "Jimu2_digital_sensor_02161215";
#elif Update_Test1
    public string Robot_System_Version = "Jimu_p1.37";
    public string Robot_System_FilePath = "Jimu2primary_p1.37";
    public string Robot_Servo_Version = "41165101";
    public string Robot_Servo_FilePath = "jimu2_app_41165101";
    public string Robot_Infrared_Version = "14161215";
    public string Robot_Infrared_FilePath = "Jimu2_infrared_sensor_14161215";
    public string Robot_Touch_Version = "14161215";
    public string Robot_Touch_FilePath = "Jimu2_touch_sensor_14161215";
    public string Robot_Gyro_Version = "13161215";
    public string Robot_Gyro_FilePath = "Jimu2_gyros_sensor_13161215";
    public string Robot_Speaker_Version = "01161215";
    public string Robot_Speaker_FilePath = "Jimu2_speaker_sensor_01161215";
    public string Robot_DigitalTube_Version = "02161215";
    public string Robot_DigitalTube_FilePath = "Jimu2_digital_sensor_02161215";
#else
    /// <summary>
    /// 系统主板版本
    /// </summary>
    public string Robot_System_Version = string.Empty;
    /// <summary>
    /// 系统主板程序路径
    /// </summary>
    public string Robot_System_FilePath = string.Empty;
    /// <summary>
    /// 系统舵机版本
    /// </summary>
    public string Robot_Servo_Version = "41165101";
    /// <summary>
    /// 系统舵机程序路径
    /// </summary>
    public string Robot_Servo_FilePath = "jimu2_app_41165101";
    /// <summary>
    /// 红外版本
    /// </summary>
    /*public string Robot_Infrared_Version = "14161215";
    /// <summary>
    /// 红外路径
    /// </summary>
    public string Robot_Infrared_FilePath = "Jimu2_infrared_sensor_14161215";
    /// <summary>
    /// 触碰版本
    /// </summary>
    public string Robot_Touch_Version = "14161215";
    /// <summary>
    /// 触碰路径
    /// </summary>
    public string Robot_Touch_FilePath = "Jimu2_touch_sensor_14161215";
    /// <summary>
    /// 陀螺仪版本
    /// </summary>
    public string Robot_Gyro_Version = "13161215";
    /// <summary>
    /// 陀螺仪路径
    /// </summary>
    public string Robot_Gyro_FilePath = "Jimu2_gyros_sensor_13161215";
    /// <summary>
    /// 蓝牙喇叭版本
    /// </summary>
    public string Robot_Speaker_Version = "01161215";
    /// <summary>
    /// 蓝牙喇叭路径
    /// </summary>
    public string Robot_Speaker_FilePath = "Jimu2_speaker_sensor_01161215";
    /// <summary>
    /// 数码管版本
    /// </summary>
    public string Robot_DigitalTube_Version = "02161215";
    /// <summary>
    /// 数码管路径
    /// </summary>
    public string Robot_DigitalTube_FilePath = "Jimu2_digital_sensor_02161215";*/
    public string Robot_Infrared_Version = string.Empty;
    public string Robot_Infrared_FilePath = string.Empty;
    public string Robot_Touch_Version = string.Empty;
    public string Robot_Touch_FilePath = string.Empty;
    public string Robot_Gyro_Version = string.Empty;
    public string Robot_Gyro_FilePath = string.Empty;
    public string Robot_Speaker_Version = string.Empty;
    public string Robot_Speaker_FilePath = string.Empty;
    public string Robot_DigitalTube_Version = string.Empty;
    public string Robot_DigitalTube_FilePath = string.Empty;
    public string Robot_Light_Version = string.Empty;
    public string Robot_Light_FilePath = string.Empty;
    public string Robot_Gravity_Version = string.Empty;
    public string Robot_Gravity_FilePath = string.Empty;
    public string Robot_Ultrasonic_Version = string.Empty;
    public string Robot_Ultrasonic_FilePath = string.Empty;
#endif
    #endregion

    #region 其他属性
    TopologyPartType mUpdateDeviceType = TopologyPartType.None;
    TopologyPartType mLastSuccesUpdateDeviceType = TopologyPartType.None;
    UpdateState mUpdateState = UpdateState.State_Start;
    Dictionary<TopologyPartType, UpdateBase> mUpdateDataDict = null;
#endregion

#region 公有函数
    public UpdateManager()
    {
        mUpdateDataDict = new Dictionary<TopologyPartType, UpdateBase>();
        TopologyPartType[] sensorType = PublicFunction.Open_Topology_Part_Type;
        for (int i = 0, imax = sensorType.Length; i < imax; ++i)
        {
            if (PlayerPrefs.HasKey(sensorType[i].ToString()))
            {
                string[] args = PlayerPrefs.GetString(sensorType[i].ToString()).Split('|');
                if (null != args && args.Length == 2)
                {
                    if (File.Exists(args[1]))
                    {
                        SetSensorUpdateData(sensorType[i], args[0], args[1]);
                    }
                    else
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "传感器升级文件不存在 path = " + args[1]);
                    }
                }
                else
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "保存的传感器升级数据错误" + PlayerPrefs.GetString(sensorType[i].ToString()));
                }
            }
            else
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "未保存传感器升级信息 " + sensorType[i].ToString());
            }
        }
    }

    /// <summary>
    /// 检查升级
    /// </summary>
    /// <param name="deviceType">升级的硬件类型</param>
    /// <param name="msg">主板信息</param>
    /// <returns></returns>
    public ErrorCode CheckUpdate(TopologyPartType deviceType, ReadMotherboardDataMsgAck msg)
    {
        CreateUpdate(deviceType);
        mLastSuccesUpdateDeviceType = TopologyPartType.None;
        if (mUpdateDataDict.ContainsKey(deviceType))
        {
            return mUpdateDataDict[deviceType].CheckUpdate(msg);
        }
        return ErrorCode.Result_OK;
    }

    /// <summary>
    /// 开始升级
    /// </summary>
    /// <param name="deviceType">升级的硬件类型</param>
    /// <param name="robot"></param>
    /// <param name="arg">升级参数</param>
    /// <returns>返回true表示能升级</returns>
    public bool UpdateStart(TopologyPartType deviceType, Robot robot, byte arg = 0)
    {
        CreateUpdate(deviceType);
        if (mUpdateDataDict.ContainsKey(deviceType))
        {
            mUpdateDeviceType = deviceType;
            mUpdateState = UpdateState.State_Start;
            return mUpdateDataDict[deviceType].UpdateStart(robot, arg);
        }
        return false;
    }
    /// <summary>
    /// 发送升级数据
    /// </summary>
    public void WriteFrame()
    {
        if (mUpdateDataDict.ContainsKey(mUpdateDeviceType))
        {
            mUpdateDataDict[mUpdateDeviceType].SendFrame();
            mUpdateDataDict[mUpdateDeviceType].UpdateProgress();
            if (mUpdateDataDict[mUpdateDeviceType].IsWriteFinished())
            {
                mUpdateState = UpdateState.State_Wait;
            }
            else
            {
                mUpdateState = UpdateState.State_Write;
            }
        }
    }
    /// <summary>
    /// 升级异常
    /// </summary>
    public void UpdateError()
    {
        mUpdateState = UpdateState.State_Fail;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(mUpdateDeviceType));
            CleanUpdate();
        }
    }
    /// <summary>
    /// 升级失败
    /// </summary>
    /// <param name="obj"></param>
    public void UpdateFail(List<byte> list)
    {
        mUpdateState = UpdateState.State_Fail;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Fail, new EventArg(mUpdateDeviceType, list));
            CleanUpdate();
        }
    }
    /// <summary>
    /// 升级成功
    /// </summary>
    public void UpdateSucces()
    {
        mUpdateState = UpdateState.State_Success;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            mLastSuccesUpdateDeviceType = mUpdateDeviceType;
            EventMgr.Inst.Fire(EventID.Update_Finished, new EventArg(mUpdateDeviceType));
            CleanUpdate();
        }
    }
    /// <summary>
    /// 升级超时
    /// </summary>
    public void UpdateOutTime()
    {
        mUpdateState = UpdateState.State_Fail;
        if (mUpdateDeviceType != TopologyPartType.None)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(mUpdateDeviceType));
            CleanUpdate();
        }
    }

    public void CannelUpdate()
    {
        if (TopologyPartType.None != mUpdateDeviceType)
        {
            EventMgr.Inst.Fire(EventID.Update_Cannel, new EventArg(mUpdateDeviceType));
            CleanUpdate();
        }
    }

    public bool IsSystemUpdateSucces()
    {
        if (TopologyPartType.MainBoard == mLastSuccesUpdateDeviceType && UpdateState.State_Success == mUpdateState)
        {
            return true;
        }
        return false;
    }

    public UpdateState GetUpdateState()
    {
        return mUpdateState;
    }

    public TopologyPartType GetUpdateDeviceType()
    {
        return mUpdateDeviceType;
    }

    public string GetSensorVersion(TopologyPartType sensorType)
    {
        string version = string.Empty;
        switch (sensorType)
        {
            case TopologyPartType.Infrared:
                version = Robot_Infrared_Version;
                break;
            case TopologyPartType.Gyro:
                version = Robot_Gyro_Version;
                break;
            case TopologyPartType.Touch:
                version = Robot_Touch_Version;
                break;
            case TopologyPartType.Light:
                version = Robot_Light_Version;
                break;
            case TopologyPartType.Gravity:
                version = Robot_Gravity_Version;
                break;
            case TopologyPartType.Ultrasonic:
                version = Robot_Ultrasonic_Version;
                break;
            case TopologyPartType.DigitalTube:
                version = Robot_DigitalTube_Version;
                break;
            case TopologyPartType.Speaker:
                version = Robot_Speaker_Version;
                break;
        }
        return version;
    }

    public void SetSensorUpdateData(TopologyPartType sensorType, string version, string filePath)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, string.Format("设置传感器升级信息 sensorType = {0} version = {1} filePath = {2}", sensorType, version, filePath));
        switch (sensorType)
        {
            case TopologyPartType.Infrared:
                Robot_Infrared_Version = version;
                Robot_Infrared_FilePath = filePath;
                break;
            case TopologyPartType.Gyro:
                Robot_Gyro_Version = version;
                Robot_Gyro_FilePath = filePath;
                break;
            case TopologyPartType.Touch:
                Robot_Touch_Version = version;
                Robot_Touch_FilePath = filePath;
                break;
            case TopologyPartType.Light:
                Robot_Light_Version = version;
                Robot_Light_FilePath = filePath;
                break;
            case TopologyPartType.Gravity:
                Robot_Gravity_Version = version;
                Robot_Gravity_FilePath = filePath;
                break;
            case TopologyPartType.Ultrasonic:
                Robot_Ultrasonic_Version = version;
                Robot_Ultrasonic_FilePath = filePath;
                break;
            case TopologyPartType.DigitalTube:
                Robot_DigitalTube_Version = version;
                Robot_DigitalTube_FilePath = filePath;
                break;
            case TopologyPartType.Speaker:
                Robot_Speaker_Version = version;
                Robot_Speaker_FilePath = filePath;
                break;
        }
    }

    public override void CleanUp()
    {
        base.CleanUp();
        mUpdateDeviceType = TopologyPartType.None;
        mLastSuccesUpdateDeviceType = TopologyPartType.None;
        mUpdateState = UpdateState.State_Start;
        mUpdateDataDict.Clear();
    }

    #endregion

    #region 其他函数

    void CreateUpdate(TopologyPartType deviceType)
    {
        if (!mUpdateDataDict.ContainsKey(deviceType))
        {
            switch (deviceType)
            {
                case TopologyPartType.MainBoard:
                    UpdateSystem system = new UpdateSystem();
                    mUpdateDataDict[deviceType] = system;
                    break;
                case TopologyPartType.Servo:
                    UpdateServo servo = new UpdateServo();
                    mUpdateDataDict[deviceType] = servo;
                    break;
                case TopologyPartType.Infrared:
                    UpdateInfrared infrared = new UpdateInfrared();
                    mUpdateDataDict[deviceType] = infrared;
                    break;
                case TopologyPartType.Touch:
                    UpdateTouch touch = new UpdateTouch();
                    mUpdateDataDict[deviceType] = touch;
                    break;
                case TopologyPartType.Gyro:
                    UpdateGyro gyro = new UpdateGyro();
                    mUpdateDataDict[deviceType] = gyro;
                    break;
                case TopologyPartType.DigitalTube:
                    UpdateDigitalTube digital = new UpdateDigitalTube();
                    mUpdateDataDict[deviceType] = digital;
                    break;
                case TopologyPartType.Speaker:
                    UpdateSpeaker speaker = new UpdateSpeaker();
                    mUpdateDataDict[deviceType] = speaker;
                    break;
                case TopologyPartType.Light:
                    UpdateLight light = new UpdateLight();
                    mUpdateDataDict[deviceType] = light;
                    break;
            }
        }
    }

    void CleanUpdate()
    {
        if (mUpdateDataDict.ContainsKey(mUpdateDeviceType))
        {
            mUpdateDataDict[mUpdateDeviceType].CleanUp();
            mUpdateDataDict.Remove(mUpdateDeviceType);
            mUpdateDeviceType = TopologyPartType.None;
        }
    }
#endregion
}


public enum UpdateDeviceType : byte
{
    None,//无升级
    System_Update,//主板升级
    Servo_Update,//舵机升级
}

public enum UpdateState : byte
{
    State_Start,
    State_Write,
    State_Wait,
    State_Success,
    State_Fail,
}