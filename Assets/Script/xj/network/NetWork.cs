//#define Test
using Game.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;

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
    Queue<SendMsgData> mSendMsgList = null;

    Dictionary<CMDCode, ExtendCMDCode> mLastSendExCmdDict;
    //Thread mSendThread;
#if UNITY_EDITOR
    const int Wait_Msg_Out_Time = 0;
#else
    const int Wait_Msg_Out_Time = 10;
#endif
    float mLastSendTime;
    /// <summary>
    /// 禁止边充边玩时可以发送的指令
    /// </summary>
    CMDCode[] mChargeCanSendCmd = new CMDCode[] { CMDCode.Hand_Shake, CMDCode.Read_System_Power, CMDCode.Read_Device_Type, CMDCode.Self_Check, CMDCode.Read_Motherboard_Data, CMDCode.Read_System_Version, CMDCode.Read_IC_Flash, CMDCode.Write_IC_Flash, CMDCode.Read_MCU_ID, CMDCode.Change_Name, CMDCode.Robot_Update_Start, CMDCode.Robot_Update_Finish, CMDCode.Robot_Update_Write, CMDCode.Robot_Restart_Update_Start_Ack, CMDCode.Robot_Restart_Update_Finish_Ack, CMDCode.Servo_Update_Start, CMDCode.Servo_Update_Write, CMDCode.Servo_Update_Finish, CMDCode.Change_ID, CMDCode.Change_Sensor_ID };
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
            mSendMsgList.Enqueue(data);
            if (mLastSendTime < 0.1f)
            {
                SendMsg();
            }
            //MyLog.Log("SendMsg start end");
        }
        
#endif
    }

    public void ReceiveMsg(CMDCode cmd, int len, string mac, BinaryReader br)
    {
        if (mSendMsgList.Count > 0)
        {
            SendMsgData data = mSendMsgList.Dequeue();
            ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, data.extendCmd);
        }
        else
        {
            if (mLastSendExCmdDict.ContainsKey(cmd))
            {
                ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, mLastSendExCmdDict[cmd]);
            }
            else
            {
                ProtocolClient.GetInst().OnMsgDelegate(cmd, len, mac, br, ExtendCMDCode.Extend_Code_None);
            }
        }
        mLastSendTime = 0;
        if (mSendMsgList.Count > 0)
        {
            SendMsg();
        }
    }


    public void Update()
    {
#if !Test
        if (mLastSendTime > 0.1f && Time.time - mLastSendTime >= Wait_Msg_Out_Time)
        {
            if (mSendMsgList.Count > 0)
            {
                SendMsgData sendData = mSendMsgList.Peek();
                ReceiveMsg(sendData.cmd, 0, sendData.mac, null);
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
    #endregion

    #region 私有函数
    NetWork()
    {
        mSendMsgList = new Queue<SendMsgData>();
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

    void SendMsg()
    {
        try
        {
            //MyLog.Log("private SendMsg start 1");
            SendMsgData sendData = mSendMsgList.Peek();
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
                if (ClientMain.Exception_Log_Flag)
                {
                    Debuger.Log("send time =" + Time.time + ",msg cmd=" + cmd + " length=" + pMsg.Length + "msg=" + PublicFunction.BytesToHexString(pMsg));
                }
                mLastSendExCmdDict[cmd] = sendData.extendCmd;
                PlatformMgr.Instance.SendMsg((byte)cmd, pMsg, pMsg.Length);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
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