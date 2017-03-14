using Game;
using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ConnectDevice.cs
/// Description:连接设备的数据
/// Time:2016/12/8 10:05:11
/// </summary>
public class ConnectDevice
{
    #region 公有属性
    #endregion

    #region 其他属性
    ConnectState mState = ConnectState.Connect_None;
    byte mReadDeviceOutTimeCount = 0;
    byte mConnectTimes = 0;
    public byte ConnectTimes
    {
        get { return mConnectTimes; }
    }
    public ConnectState ConState
    {
        get { return mState; }
    }
    string mMac = string.Empty;
    public string Mac
    {
        get { return mMac; }
    }
    string mName = string.Empty;
    public string Name
    {
        get { return mName; }
    }

    long mReadDeviceTypeIndex = -1;
    long mReadMotherboardDataIndex = -1;
    #endregion

    #region 公有函数
    public ConnectDevice(string mac, string name)
    {
        mMac = mac;
        mName = name;
    }

    /// <summary>
    /// 连接
    /// </summary>
    public void OnConnect(string name)
    {
        CleanUpTimer();
        mName = name;
        mState = ConnectState.Connect_Ing;
        ++mConnectTimes;
        mReadDeviceOutTimeCount = 0;
    }

    public void ConnectFail(BlueConnectFailReason reason)
    {
        CleanUpTimer();
        mState = ConnectState.Connect_Fail;
        EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(false));
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, reason.ToString());
    }

    public void ConnectSuccess()
    {
        Robot robot = SingletonObject<ConnectManager>.GetInst().GetConnectRobot();
        mState = ConnectState.Connect_Verify;
        robot.ConnectRobotResult(mMac, true);
        robot.ReadDeviceType();
        //mReadDeviceTypeIndex = Timer.Add(1, 1, 1, robot.ReadDeviceType);
    }

    public void ConnectCannel()
    {
        CleanUpTimer();
        mState = ConnectState.Connect_Cannel;
    }

    public void Disconnect()
    {
        CleanUpTimer();
        if (mState == ConnectState.Connect_Finished)
        {
            mConnectTimes = 0;
        }
        mState = ConnectState.Connect_Disconnect;
        
    }

    public void ReadDeviceResult(Robot robot, bool result)
    {
        mReadDeviceOutTimeCount = 0;
        if (result)
        {
            robot.HandShake();
            //2秒以后读取初始角度
            mReadMotherboardDataIndex = Timer.Add(2, 1, 1, robot.ReadMotherboardData);
            mState = ConnectState.Connect_ReadData;
        }
        else
        {
            mState = ConnectState.Connect_Fail;
            robot.HandShake();//加入握手命令是防止一直连不上必须重启主控盒的情况出现
            PlatformMgr.Instance.DisConnenctBuletooth();
            PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, BlueConnectFailReason.ReadDeviceFail.ToString());
        }
    }

    public void ReadDeviceOutTime(Robot robot)
    {
        ++mReadDeviceOutTimeCount;
        if (mReadDeviceOutTimeCount > 4)
        {
            ReadDeviceResult(robot, false);
        }
        else
        {
            mReadDeviceTypeIndex = Timer.Add(1, 1, 1, delegate() {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("第{0}次读取设备类型", (mReadDeviceOutTimeCount + 1)));
                robot.ReadDeviceType();
            });
        }
    }

    public void ReadDataOutTime()
    {
        mState = ConnectState.Connect_ReadData_OutTime;
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, BlueConnectFailReason.ReadControlboxInfoFail.ToString());
    }

    public void ConnectFinished(Robot robot)
    {
        EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(true));
        mState = ConnectState.Connect_Finished;
        robot.StartReadSystemPower();
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            PlatformMgr.Instance.SaveLastConnectedData(robot.Mac);
        }
        else
        {
            robot.ReadMCUInfo();
            robot.SelfCheck(true);
            robot.canShowPowerFlag = true;
            TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = partType.Length; i < imax; ++i)
            {
                if (partType[i] == TopologyPartType.Speaker)
                {
                    continue;
                }
                if (null != robot.MotherboardData)
                {
                    SensorData sensorData = robot.MotherboardData.GetSensorData(partType[i]);
                    if (null != sensorData && sensorData.ids.Count > 0)
                    {
                        robot.SensorInit(sensorData.ids, partType[i]);
                    }
                }
            }
            if (PlatformMgr.Instance.IsChargeProtected)
            {
                SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
            }
            else
            {
                if (robot.GetAllDjData().GetAngleList().Count > 0)
                {
                    robot.ReadConnectedAngle();
                }
                else
                {
                    SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                    EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
                }
            }
        }
        PlatformMgr.Instance.SaveRobotLastConnectedData(robot.ID, robot.Mac);
    }

    #endregion

    #region 其他函数
    void CleanUpTimer()
    {
        if (-1 != mReadDeviceTypeIndex)
        {
            Timer.Cancel(mReadDeviceTypeIndex);
            mReadDeviceTypeIndex = -1;
        }
        if (-1 != mReadMotherboardDataIndex)
        {
            Timer.Cancel(mReadMotherboardDataIndex);
            mReadMotherboardDataIndex = -1;
        }
    }
    #endregion
}

/// <summary>
/// 连接的状态
/// </summary>
public enum ConnectState : byte
{
    Connect_None,
    Connect_Ing,
    Connect_Cannel,
    Connect_Fail,
    Connect_Verify,
    Connect_ReadData,
    Connect_ReadData_OutTime,
    Connect_Disconnect,
    Connect_Finished,
}