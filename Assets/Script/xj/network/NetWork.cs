//#define Test
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Text;

/// <summary>
/// Author:xj
/// FileName:NetWork.cs
/// Description:
/// Time:2015/7/1 18:04:10
/// </summary>
public class NetWork
{
    #region 公有属性
    #endregion

    #region 私有属性
    static NetWork mInst = null;
    //static UnityEngine.Object mLock = new UnityEngine.Object();
    //Queue<SendMsgData> mSendMsgList = null;
    List<SendMsgData> mSendMsgList = null;

    Dictionary<CMDCode, ExtendCMDCode> mLastSendExCmdDict;
    //Thread mSendThread;
#if UNITY_EDITOR
    const int Wait_Msg_Out_Time_Long = 0;
    const int Wait_Msg_Out_Time_Normal = 0;
#else
    const int Wait_Msg_Out_Time_Long = 10;
    const int Wait_Msg_Out_Time_Normal = 1;
#endif
    float mLastSendTime;
    /// <summary>
    /// 禁止边充边玩时可以发送的指令
    /// </summary>
    CMDCode[] mChargeCanSendCmd = new CMDCode[] { CMDCode.Hand_Shake, CMDCode.Read_System_Power, CMDCode.Read_Device_Type, CMDCode.Self_Check, CMDCode.Read_Motherboard_Data, CMDCode.Read_System_Version, CMDCode.Read_IC_Flash, CMDCode.Write_IC_Flash, CMDCode.Read_MCU_ID, CMDCode.Change_Name, CMDCode.Robot_Update_Start, CMDCode.Robot_Update_Finish, CMDCode.Robot_Update_Write, CMDCode.Robot_Update_Stop, CMDCode.Robot_Restart_Update_Start_Ack, CMDCode.Robot_Restart_Update_Finish_Ack, CMDCode.Servo_Update_Start, CMDCode.Servo_Update_Write, CMDCode.Servo_Update_Finish, CMDCode.Servo_Update_Stop, CMDCode.Change_ID, CMDCode.Change_Sensor_ID, CMDCode.Read_Sensor_Data, CMDCode.Read_Sensor_Data_Other, CMDCode.Sensor_Update_Start, CMDCode.Sensor_Update_Write, CMDCode.Sensor_Update_Stop, CMDCode.Sensor_Update_Finish };
    bool mChargePromptFlag = false;

    SendMsgData mSendedMsgData;
    #endregion

    #region 公有函数
    public static NetWork GetInst()
    {
        if (null == mInst)
        {
            mInst = new NetWork();
        }
        return mInst;
    }
    /// <summary>
    /// 发包
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="msg"></param>
    public void SendMsg(CMDCode cmd, CBaseMsg msg, string mac, ExtendCMDCode exCmd = ExtendCMDCode.Extend_Code_None)
    {
        //MyLog.Log("SendMsg start");
        if (!PlatformMgr.Instance.GetBluetoothState())
        {
            return;
        }
        if (PlatformMgr.Instance.IsChargeProtected)
        {//禁止边充边玩
            bool returnFlag = true;
            for (int i = 0, imax = mChargeCanSendCmd.Length; i < imax; ++i)
            {
                if (cmd == mChargeCanSendCmd[i])
                {
                    returnFlag = false;
                    break;
                }
            }
            if (returnFlag)
            {
                NetWaitMsg.CloseWait();
                /*if (!mChargePromptFlag)
                {
                    mChargePromptFlag = true;
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("禁止边充边玩"));
                    //GuideViewBase.Ins.OnCloseGuide(null);
                }*/

                return;
            }
        }
#if Test
        MemoryStream DataStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(DataStream);

        if (null != msg)
        {
            msg.Write(writer);
        }
        byte[] pMsg = DataStream.ToArray();

        writer.Close();
        DataStream.Close();
        PlatformMgr.Instance.SendMsg((byte)cmd, pMsg, pMsg.Length);
#else
        //lock (mLock)
        {
            //MyLog.Log("SendMsg start 1");
            SendMsgData data = new SendMsgData(cmd, msg, mac, exCmd);
            AddSendMsg(data);
            if (mSendMsgList.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0, imax = mSendMsgList.Count; i < imax; ++i)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(PublicFunction.Separator_Comma);
                    }
                    sb.Append(string.Format("cmd= {0} exCmd = {1}", mSendMsgList[i].cmd, mSendMsgList[i].extendCmd));
                }
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeDebug, string.Format("等待的命令数量={0},  {1}", mSendMsgList.Count, sb.ToString()));
            }
            if (mLastSendTime < 0.1f)
            {
                SendMsg();
            }
        }
        
#endif
    }

    public void ReceiveMsg(CMDCode cmd, int len, string mac, BinaryReader br)
    {
        bool finished = true;
        if (null != mSendedMsgData)
        {
            finished = ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, mSendedMsgData.extendCmd);
        }
        else
        {
            if (mLastSendExCmdDict.ContainsKey(cmd))
            {
                finished = ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, mLastSendExCmdDict[cmd]);
            }
            else
            {
                finished = ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, ExtendCMDCode.Extend_Code_None);
            }
        }
        if (cmd != CMDCode.Read_Sensor_Data_Other)
        {
            finished = true;
        }
        if (finished)
        {
            if (null != mSendedMsgData && mSendedMsgData.cmd == CMDCode.Read_Back)
            {//回读命令需特殊处理，等所有的舵机都回了以后在发送下一条指令
                ReadBackMsg msg = (ReadBackMsg)mSendedMsgData.msg;
                msg.needReadBackCount--;
                if (msg.needReadBackCount <= 0)
                {
                    mSendedMsgData = null;
                    mLastSendTime = 0;
                    if (mSendMsgList.Count > 0)
                    {
                        SendMsg();
                    }
                }
            }
            else
            {
                mLastSendTime = 0;
                mSendedMsgData = null;
                if (mSendMsgList.Count > 0)
                {
                    SendMsg();
                }
            }
        }        
    }


    public void Update()
    {
#if !Test
        if (mLastSendTime > 0.1f)
        {
            float time = Time.time;
            if (null != mSendedMsgData)
            {
                //读取传感器的超时时间设为1秒，防止逻辑编程发送速度太快出现数据丢失等待时间过长
                bool outTime = (mSendedMsgData.cmd == CMDCode.Hand_Shake || mSendedMsgData.cmd == CMDCode.Read_Motherboard_Data) ? time - mLastSendTime >= Wait_Msg_Out_Time_Long : time - mLastSendTime >= Wait_Msg_Out_Time_Normal;
                if (outTime)
                {
                    NetWaitMsg.CloseWait();
                    ReceiveMsg(mSendedMsgData.cmd, 0, mSendedMsgData.mac, null);
                }
            }
        }
#endif

    }


    public void ClearAllMsg()
    {
        //lock (mLock)
        {
            mSendMsgList.Clear();
        }
        mLastSendExCmdDict.Clear();
        mLastSendTime = 0;
        mChargePromptFlag = false;
        mSendedMsgData = null;
    }

    public void ClearCacheMsg()
    {
        mSendMsgList.Clear();
    }
    #endregion

    #region 私有函数
    NetWork()
    {
        mSendMsgList = new List<SendMsgData>();
        mLastSendTime = 0;
        mLastSendExCmdDict = new Dictionary<CMDCode, ExtendCMDCode>();
        /*mSendThread = new Thread(new ThreadStart(Test));
        mSendThread.Start();*/
    }
    /*List<long> list = new List<long>();
    void Test()
    {
        while (true)
        {
            list.Add(DateTime.Now.Millisecond / 10000);
            if (list.Count > 1000)
            {
                break;
            }
        }
    }*/

    void AddSendMsg(SendMsgData msg)
    {
        if (msg.cmd == CMDCode.Ctrl_Action && msg.extendCmd != ExtendCMDCode.CtrlAction || msg.cmd == CMDCode.Read_Sensor_Data || msg.cmd == CMDCode.Read_Sensor_Data_Other)
        {
            for (int i = 0, imax = mSendMsgList.Count; i < imax; ++i)
            {
                if (msg.cmd == mSendMsgList[i].cmd && msg.extendCmd == mSendMsgList[i].extendCmd)
                {
                    //mSendMsgList[i] = msg;
                    mSendMsgList.RemoveAt(i);
                    break;
                }
            }
        }
        mSendMsgList.Add(msg);
    }

    SendMsgData GetSendMsg()
    {
        if (mSendMsgList.Count > 0)
        {
            SendMsgData msg = mSendMsgList[0];
            mSendMsgList.RemoveAt(0);
            return msg;
        }
        return null;
    }

    void SendMsg()
    {
        try
        {
            SendMsgData sendData = GetSendMsg();
            if (null == sendData)
            {
                return;
            }
            if (mSendedMsgData == sendData)
            {//防止回包里面发了包引起同一个包重复发送
                return;
            }
            mSendedMsgData = sendData;
            //SendMsgData sendData = mSendMsgList.Dequeue();
            if (null != sendData)
            {
                MemoryStream DataStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(DataStream);
                CBaseMsg msg = sendData.msg;
                CMDCode cmd = sendData.cmd;
                if (null != msg)
                {
                    msg.Write(writer);
                }
                byte[] pMsg = DataStream.ToArray();

                writer.Close();
                DataStream.Close();
                PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeEvent, "send time =" + Time.time + ",msg cmd=" + cmd.ToString() + " length=" + pMsg.Length + "msg=" + PublicFunction.BytesToHexString(pMsg));

                mLastSendExCmdDict[cmd] = sendData.extendCmd;
                PlatformMgr.Instance.SendMsg((byte)cmd, pMsg, pMsg.Length);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        mLastSendTime = Time.time;
    }

    
    #endregion
}


public class SendMsgData
{
    public string mac;
    public CMDCode cmd;
    public CBaseMsg msg;
    public ExtendCMDCode extendCmd;

    public SendMsgData(CMDCode cmd, CBaseMsg msg, string mac, ExtendCMDCode exCmd)
    {
        this.cmd = cmd;
        this.msg = msg;
        this.mac = mac;
        this.extendCmd = exCmd;
    }
}