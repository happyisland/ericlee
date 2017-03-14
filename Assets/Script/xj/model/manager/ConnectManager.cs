using Game;
using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ConnectManager.cs
/// Description:连接管理类
/// Time:2016/12/8 9:59:19
/// </summary>
public class ConnectManager : SingletonObject<ConnectManager>
{
    #region 公有属性
    #endregion

    #region 其他属性
    string AutoConnectSaveKey = "AutoConnect";
    bool isAutoConnectFlag = true;
    Dictionary<string, ConnectDevice> mConnectDeviceDict;
    string mConnectingMac;
    string mConnectedMac;
    float lastDicConnectedTime;

    BlueConnectFailReason mConnectFailReason;

    long mOutTimeIndex = -1;
    public float LastDicConnectedTime
    {
        get { return lastDicConnectedTime; }
    }
    #endregion

    #region 公有函数
    public ConnectManager()
    {
        mConnectDeviceDict = new Dictionary<string, ConnectDevice>();
        mConnectingMac = string.Empty;
        mConnectedMac = string.Empty;
        if (PlayerPrefs.HasKey(AutoConnectSaveKey))
        {
            isAutoConnectFlag = PlayerPrefs.GetInt(AutoConnectSaveKey) == 1 ? true : false;
        }
        else
        {
            isAutoConnectFlag = true;
        }
        mConnectFailReason = BlueConnectFailReason.unknow;
    }
    /// <summary>
    /// 获取连接状态
    /// </summary>
    /// <returns></returns>
    public bool GetBluetoothState()
    {
        if (string.IsNullOrEmpty(mConnectedMac))
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// 是否需要自动连接
    /// </summary>
    /// <param name="robotId"></param>
    /// <param name="mac"></param>
    /// <returns></returns>
    public bool IsAutoConnect(string robotId, string mac)
    {
        if (!isAutoConnectFlag)
        {
            return false;
        }
        if (PlayerPrefs.HasKey(robotId))
        {
            if (PlayerPrefs.GetString(robotId).Equals(mac))
            {
                if (!mConnectDeviceDict.ContainsKey(mac) || mConnectDeviceDict[mac].ConnectTimes < 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OnConnect(string mac, string name)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接设备 mac =" + mac);
        if (!mConnectDeviceDict.ContainsKey(mac))
        {
            ConnectDevice device = new ConnectDevice(mac, name);
            mConnectDeviceDict[mac] = device;
        }
        if (!string.IsNullOrEmpty(mConnectedMac) && mConnectDeviceDict.ContainsKey(mConnectedMac))
        {
            mConnectDeviceDict[mConnectedMac].Disconnect();
            mConnectedMac = string.Empty;
        }
        CleanConnectData();
        mConnectingMac = mac;
        mConnectDeviceDict[mac].OnConnect(name);
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
        }
        mOutTimeIndex = Timer.Add(15f, 1, 1, ConnectOutTime, mac);
    }

    public void CancelConnecting()
    {
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
            mOutTimeIndex = -1;
        }
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "用户主动取消了连接");
        Robot robot = GetConnectRobot();
        if (null != robot)
        {
            robot.CancelConnect();
        }
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, BlueConnectFailReason.Cancel.ToString());
        if (string.IsNullOrEmpty(mConnectingMac))
        {
            if (!string.IsNullOrEmpty(mConnectedMac))
            {
                if (mConnectDeviceDict.ContainsKey(mConnectedMac))
                {
                    mConnectDeviceDict[mConnectedMac].ConnectCannel();
                }
                PlatformMgr.Instance.DisConnenctBuletooth();
            }
        }
        else
        {
            if (mConnectDeviceDict.ContainsKey(mConnectingMac))
            {
                mConnectDeviceDict[mConnectingMac].ConnectCannel();
            }
            mConnectingMac = string.Empty;
        }
    }

    /// <summary>
    /// 连接成功
    /// </summary>
    /// <param name="mac"></param>
    public void ConnectSuccess(string mac)
    {
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
            mOutTimeIndex = -1;
        }
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接成功 mac =" + mac);
        if (mac.Equals(mConnectingMac) && mConnectDeviceDict.ContainsKey(mac))
        {
            mConnectedMac = mac;
            mConnectDeviceDict[mac].ConnectSuccess();
        }
        else
        {
            if (mConnectDeviceDict.ContainsKey(mac))
            {
                if (mConnectDeviceDict[mac].ConState != ConnectState.Connect_Cannel)
                {
                    mConnectDeviceDict[mac].ConnectFail(BlueConnectFailReason.unknow);
                }
            }
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
        mConnectingMac = string.Empty;
    }

    public void ConnectFail()
    {
        if (-1 != mOutTimeIndex)
        {
            Timer.Cancel(mOutTimeIndex);
            mOutTimeIndex = -1;
        }
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("ConnectFail mConnectingMac = {0}", mConnectingMac));
        if (!string.IsNullOrEmpty(mConnectingMac) && mConnectDeviceDict.ContainsKey(mConnectingMac))
        {
            mConnectDeviceDict[mConnectingMac].ConnectFail(BlueConnectFailReason.BluetoothFail);
        }
        mConnectingMac = string.Empty;
    }

    public void ConnectFailForBoard(BlueConnectFailReason reason)
    {

    }

    public void ReadDeviceResult(Robot robot, bool result)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "读取设备类型result = " + result.ToString());
        if (robot.Mac.Equals(mConnectedMac))
        {
            if (mConnectDeviceDict.ContainsKey(robot.Mac))
            {
                mConnectDeviceDict[robot.Mac].ReadDeviceResult(robot, result);
            }
        }
        else if (string.IsNullOrEmpty(mConnectingMac))
        {
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
    }

    public void ReadDeviceOutTime(Robot robot)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "读取设备类型超时未回复");
        if (robot.Mac.Equals(mConnectedMac))
        {
            if (mConnectDeviceDict.ContainsKey(robot.Mac))
            {
                mConnectDeviceDict[robot.Mac].ReadDeviceOutTime(robot);
            }
        }
        else if (string.IsNullOrEmpty(mConnectingMac))
        {
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
    }
    /// <summary>
    /// 读取主板信息超时
    /// </summary>
    /// <param name="robot"></param>
    public void ReadDataOutTime(Robot robot)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "读取主板信息超时");
        if (robot.Mac.Equals(mConnectedMac))
        {
            if (mConnectDeviceDict.ContainsKey(robot.Mac))
            {
                mConnectDeviceDict[robot.Mac].ReadDataOutTime();
            }
        }
    }

    public void ConnectFinished(Robot robot)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接完成，蓝牙连接成功");
        if (robot.Mac.Equals(mConnectedMac))
        {
            if (mConnectDeviceDict.ContainsKey(robot.Mac))
            {
                mConnectDeviceDict[robot.Mac].ConnectFinished(robot);
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionSucceeded);
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("蓝牙连接成功"), HUDTextTips.Color_Green);
            }
        }
        
    }
    /// <summary>
    /// 主动断开连接
    /// </summary>
    public void OnDisconnect()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "主动断开蓝牙连接 mConnectedMac =" + mConnectedMac);
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            lastDicConnectedTime = Time.time;
            if (mConnectDeviceDict.ContainsKey(mConnectedMac))
            {
                mConnectDeviceDict[mConnectedMac].Disconnect();
            }
            CleanConnectData();
            EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(false));
        }
        else
        {
            CleanConnectData();
        }
    }
    /// <summary>
    /// 收到断开的通知
    /// </summary>
    public void DisconnectNotify()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "收到断开蓝牙的通知 mConnectedMac =" + mConnectedMac);
        Robot robot = GetConnectRobot();
        if (!string.IsNullOrEmpty(mConnectedMac))
        {
            if (mConnectDeviceDict.ContainsKey(mConnectedMac))
            {
                ConnectState oldState = mConnectDeviceDict[mConnectedMac].ConState;
                mConnectDeviceDict[mConnectedMac].Disconnect();
                if (oldState == ConnectState.Connect_Finished)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("蓝牙断开"));
                    if (LogicCtrl.GetInst().IsLogicOpenSearchFlag)
                    {
                        if (!PopWinManager.GetInst().IsExist(typeof(TopologyBaseMsg)))
                        {
                            LogicCtrl.GetInst().CloseBlueSearch();
                        }
                    }
                    else
                    {
                        LogicCtrl.GetInst().NotifyLogicDicBlue();
                    }
                }
            }
            CleanConnectData();
            EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(false));
        }
        else
        {
            CleanConnectData();
        }
        if (null != robot)
        {
            robot.CancelConnect();
        }
    }

    public Robot GetConnectRobot()
    {
        Robot robot = null;
        if (RobotManager.GetInst().IsSetDeviceIDFlag)
        {
            robot = RobotManager.GetInst().GetSetDeviceRobot();
        }
        else if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            robot = RobotManager.GetInst().GetCreateRobot();
        }
        else
        {
            robot = RobotManager.GetInst().GetCurrentRobot();
        }
        return robot;
    }
    public string GetConnectedMac()
    {
        return mConnectedMac;
    }

    public string GetConnectedName()
    {
        if (mConnectDeviceDict.ContainsKey(mConnectedMac))
        {
            return mConnectDeviceDict[mConnectedMac].Name;
        }
        return string.Empty;
    }
    /// <summary>
    /// 是否可以跳过连接图
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsSkipTopology(Robot robot, ReadMotherboardDataMsgAck data)
    {
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            return false;
        }
        ErrorCode ret = CheckCompareModel(robot, data);
        if (ErrorCode.Result_OK == ret)
        {
            return true;
        }
        return false;
    }

    public void SetAutoConnectFlag(bool openFlag)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "设置自动连接开关 state = " + openFlag);
        PlayerPrefs.SetInt(AutoConnectSaveKey, openFlag ? 1 : 0);
        isAutoConnectFlag = openFlag;
    }


    public ErrorCode CheckCompareModel(Robot robot, ReadMotherboardDataMsgAck data)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            if (null != data && data.errorIds.Count > 0)
            {
                ret = ErrorCode.Result_DJ_ID_Repeat;
                break;
            }
            ret = CompareServoData(robot, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            ret = CheckSensorData(robot, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            ret = SingletonObject<UpdateManager>.GetInst().CheckUpdate(TopologyPartType.MainBoard, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            ret = SingletonObject<UpdateManager>.GetInst().CheckUpdate(TopologyPartType.Servo, data);
            if (ErrorCode.Result_OK != ret)
            {
                break;
            }
            TopologyPartType[] partAry = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = partAry.Length; i < imax; ++i)
            {
                ret = SingletonObject<UpdateManager>.GetInst().CheckUpdate(partAry[i], data);
                if (ErrorCode.Result_OK != ret)
                {
                    return ret;
                }
            }
        } while (false);
        return ret;
    }

    /// <summary>
    /// 比较实物与模型舵机信息是否匹配
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public ErrorCode CompareServoData(Robot robot, ReadMotherboardDataMsgAck data)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            if (null == data || null == robot)
            {
                break;
            }
            List<byte> list = robot.GetAllDjData().GetIDList();
            if (list.Count != data.ids.Count)
            {
                ret = ErrorCode.Result_Servo_Num_Inconsistent;
                break;
            }
            for (int i = 0, icount = list.Count; i < icount; ++i)
            {
                if (list[i] != data.ids[i])
                {
                    ret = ErrorCode.Result_Servo_ID_Inconsistent;
                    break;
                }
                if (ErrorCode.Result_Servo_ID_Inconsistent == ret)
                {
                    break;
                }
            }
        } while (false);
        return ret;
    }


    public ErrorCode CheckSensorData(Robot robot , ReadMotherboardDataMsgAck mainData)
    {
        TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
        for (int i = 0, imax = partType.Length; i < imax; ++i)
        {
            SensorData data = mainData.GetSensorData(partType[i]);
            if (null != data)
            {
                if (TopologyPartType.Speaker == partType[i])
                {
                    if (data.ids.Count > 0 || data.errorIds.Count > 0)
                    {
                        return ErrorCode.Result_Sensor_Exception;
                    }
                }
                else if (data.errorIds.Count > 0)
                {
                    return ErrorCode.Result_Sensor_Exception;
                }
            }
        }
        return ErrorCode.Result_OK;
    }

    public ConnectState GetDeviceConnectState(Robot robot)
    {
        if (null != mConnectDeviceDict && mConnectDeviceDict.ContainsKey(robot.Mac))
        {
            return mConnectDeviceDict[robot.Mac].ConState;
        }
        return ConnectState.Connect_None;
    }

    public override void CleanUp()
    {
        CleanConnectData();
        mConnectDeviceDict.Clear();
    }
    public void CleanConnectData()
    {
        mConnectedMac = string.Empty;
        mConnectingMac = string.Empty;
        mConnectFailReason = BlueConnectFailReason.unknow;
        RobotManager.GetInst().DisAllConnencted();
        NetWork.GetInst().ClearAllMsg();
        SingletonObject<UpdateManager>.GetInst().CleanUp();
        PlatformMgr.Instance.PowerData.isAdapter = false;
        PlatformMgr.Instance.PowerData.isChargingFinished = false;
    }
    #endregion

    #region 其他函数
    void ConnectOutTime(params object [] args)
    {
        string mac = (string)args[0];
        if (null != mConnectDeviceDict && mConnectDeviceDict.ContainsKey(mac) && mConnectDeviceDict[mac].ConState == ConnectState.Connect_Ing)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, mac);
            ConnectFail();
        }
    }
    #endregion
}