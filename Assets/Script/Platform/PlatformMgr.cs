//#define Test
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Scene;
using System.Text;
using System.IO;
using System;
using Game.Event;
using LitJson;
using Game.Resource;
using Game.UI;

namespace Game.Platform
{
    public class PlatformMgr : MonoBehaviour
    {
        private static PlatformMgr m_instance;
        public static PlatformMgr Instance { get { return m_instance; } }

        public delegate void CallUnityDelegate(object arg);
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
        public string Robot_Servo_Version = string.Empty;
        /// <summary>
        /// 系统舵机程序路径
        /// </summary>
        public string Robot_Servo_FilePath = string.Empty;

        public ReadPowerMsgAck PowerData = new ReadPowerMsgAck();

        public bool NeedUpdateFlag = false;
        /*
        #if UNITY_ANDROID
                AndroidJavaClass m_javaClass = null;
                AndroidJavaObject m_javaObj = null;
        #endif*/
        PlatformInterface mPlatInterface = null;

        BluetoothMgr m_blueMgr = new BluetoothMgr();

        /// <summary>
        /// 边充边玩,true表示可以边充边玩，false表示不可以
        /// </summary>
        bool isChargePlaying = true;

        public bool IsChargeProtected
        {
            get { return GetBluetoothState() && !isChargePlaying && PowerData.isAdapter; }
        }
        /// <summary>
        /// 等待升级完成标志
        /// </summary>
        public bool IsWaitUpdateFlag = false;

        Dictionary<string, CallUnityDelegate> mUnityDelegateDict;
        int count = 0;

        string error = "";

        public string Pic_Path;


        string mConnectedMac;
        string mConnectedName;
        public float lastDicConnectedTime = 0;

        bool isConnecting = false;
        string mConnectingMac = string.Empty;


        string mConnectedSpeakerMac = string.Empty;
        string mConnectingSpeakerMac = string.Empty;

        //要取消连接的设备
        List<string> mCannelConDeviceList = null;
        public bool GetBluetoothState()
        {
/*
#if UNITY_EDITOR
            return true;
#endif*/
            return m_blueMgr.ConnenctState;
        }
        
        public void CleanUpBlue()
        {
            m_blueMgr.ClearDevice();
        }
        /// <summary>
        /// 保存最后连接的蓝牙
        /// </summary>
        /// <param name="mac"></param>
        public void SaveLastConnectedData(string mac)
        {
            PlayerPrefs.SetString(PublicFunction.Last_Connected_Bluetooth, mac);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 保存模型连接的mac地址
        /// </summary>
        /// <param name="mac"></param>
        public void SaveRobotLastConnectedData(string robotId, string mac)
        {
            PlayerPrefs.SetString(robotId, mac);
            if (!PlayerPrefs.HasKey(mac))
            {
                PlayerPrefs.SetString(mac, mConnectedName);
            }
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 保存mac地址对应的名字
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="name"></param>
        public void SaveMacAnotherName(string mac, string name)
        {
            if (!string.IsNullOrEmpty(mac) && !string.IsNullOrEmpty(name))
            {
                PlayerPrefs.SetString(mac, name);
                PlayerPrefs.Save();
            }
        }
        /// <summary>
        /// 通过mac地址获取主板名字
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public string GetNameForMac(string mac)
        {
            if (PlayerPrefs.HasKey(mac))
            {
                return PlayerPrefs.GetString(mac);
            }
            if (GetBluetoothState())
            {
                return mConnectedName;
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取模型的mac地址
        /// </summary>
        /// <param name="robotId"></param>
        /// <returns></returns>
        public string GetRobotConnectedMac(string robotId)
        {
            if (GetBluetoothState())
            {
                return mConnectedMac;
            }
            else if (PlayerPrefs.HasKey(robotId))
            {
                return PlayerPrefs.GetString(robotId);
            }
            return string.Empty;
        }

        public string GetLastConnectedMac()
        {
            if (PlayerPrefs.HasKey(PublicFunction.Last_Connected_Bluetooth))
            {
                return PlayerPrefs.GetString(PublicFunction.Last_Connected_Bluetooth);
            }
            return string.Empty;
        }
        /// <summary>
        /// 修改内存中缓存的名字
        /// </summary>
        /// <param name="name"></param>
        public void MotherboardRename(string name)
        {
            m_blueMgr.BlueRename(name, mConnectedMac);
        }
        

        public void RegesiterCallUnityDelegate(CallUnityFuncID id, CallUnityDelegate dlgt)
        {
            if (null == mUnityDelegateDict)
            {
                mUnityDelegateDict = new Dictionary<string, CallUnityDelegate>();
            }
            mUnityDelegateDict[id.ToString()] = dlgt;
        }
        void Awake()
        {
            m_instance = this;
#if UNITY_EDITOR
            mPlatInterface = new PlatformInterface();
#elif UNITY_ANDROID
            mPlatInterface = new AndroidInterface();
#elif UNITY_IPHONE
            mPlatInterface = new IosInterface();
#endif
            //DontDestroyOnLoad(this.gameObject);
            RegesiterCallUnityDelegate(CallUnityFuncID.RegisterRobotResult, RegisterRobotResult);
            RegesiterCallUnityDelegate(CallUnityFuncID.UnitySetupSteeringEngineID, ModifyServoId);
            RegesiterCallUnityDelegate(CallUnityFuncID.MainboardProgramVersion, MainboardProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.SteeringEngineProgramVersion, SteeringEngineProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.ChargeProtection, ChargeProtectionCallBack);
            RegesiterCallUnityDelegate(CallUnityFuncID.LogicCMD, LogicCMDCallUnity);
            RegesiterCallUnityDelegate(CallUnityFuncID.ExitLogicView, ExitLogicView);
            RegesiterCallUnityDelegate(CallUnityFuncID.ConnectBLE, OpenBlueSearch);
            RegesiterCallUnityDelegate(CallUnityFuncID.JsShowExceptionCallback, JsShowExceptionCallback);
            RegesiterCallUnityDelegate(CallUnityFuncID.Destroy, QuitUnity);
            
            EventMgr.Inst.Regist(EventID.Connected_Jimu_Result, ReadDeviceResult);
            if (PlayerPrefs.HasKey("isChargePlaying"))
            {
                int result = PlayerPrefs.GetInt("isChargePlaying");
                if (result >= 1)
                {
                    isChargePlaying = true;
                }
                else
                {
                    isChargePlaying = false;
                }
            }
            /*Robot_System_Version = "Jimu_p1.30";
            Robot_System_FilePath = "Jimu2primary_P1.30";*/
            Robot_Servo_Version = "41161301";
            Robot_Servo_FilePath = "jimu2_app_41161301";
            /*Robot_System_Version = "Jimu_p1.29";
            Robot_System_FilePath = "Jimu2primary_P1.29";
            Robot_Servo_Version = "41155201";
            Robot_Servo_FilePath = "jimu2_app_41155201";*/
        }
        void OnDestroy()
        {
            EventMgr.Inst.UnRegist(EventID.Connected_Jimu_Result, ReadDeviceResult);
        }
        void FixedUpdate()
        {
            SingletonObject<MyTime>.GetInst().Update();
            Timer.Update();
        }

#region 蓝牙模块

#region Unity调用Platform

        public void PlatformInit()
        {
            mPlatInterface.PlatformInit();
        }

        public void PlatformDestroy()
        {
            mPlatInterface.PlatformDestroy();
        }
        //打开蓝牙
        public void OpenBluetooth()
        {
            try
            {
                mPlatInterface.OpenBluetooth();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }

        }


        public void CloseBluetooth()
        {
            try
            {
                mPlatInterface.CloseBluetooth();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }

        }
        /// <summary>
        /// 蓝牙是否开启
        /// </summary>
        /// <returns></returns>
        public bool IsOpenBluetooth()
        {
            try
            {
                return mPlatInterface.IsOpenBluetooth();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            return false;
        }
        
        public void StartScan()
        {
            try
            {
                /*m_blueMgr.OldMatchedFound();
                m_blueMgr.OldNewFound();*/
                m_blueMgr.ClearDevice();
                mPlatInterface.StartScan();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

        public void StopScan()
        {
            try
            {
                mPlatInterface.StopScan();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

        //连接蓝牙设备
        public void ConnenctBluetooth(string mac, string name)
        {
            try
            {
                Debuger.Log(string.Format("ConnenctBluetooth mac = {0} name = {1}", mac, name));
                isConnecting = true;
                mConnectingMac = mac;
                mConnectedMac = mac;
                if (!string.IsNullOrEmpty(name))
                {
                    mConnectedName = name;
                }
                mPlatInterface.DisConnenctBuletooth();
                //RobotManager.GetInst().DisAllConnencted();
                mPlatInterface.ConnenctBluetooth(mac);
            }
            catch (System.Exception ex)
            {
                isConnecting = false;
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }

        }
        /// <summary>
        ///  断开蓝牙连接
        /// </summary>
        public void DisConnenctBuletooth()
        {
            if (GetBluetoothState())
            {
                lastDicConnectedTime = Time.time;
            }
            RobotManager.GetInst().DisAllConnencted();
            m_blueMgr.DisConnenctBuletooth();
            m_blueMgr.MatchResult(false);
            NeedUpdateFlag = false;
            NetWork.GetInst().ClearAllMsg();
            try
            {
                mPlatInterface.DisConnenctBuletooth();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            
        }
        /// <summary>
        /// 取消蓝牙连接
        /// </summary>
        public void CannelConnectBluetooth()
        {
            try
            {
                Debuger.Log(string.Format("CannelConnectBluetooth mac = {0}", mConnectingMac));
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
                if (null != robot)
                {
                    robot.CannelConnect();
                }
                if (!isConnecting && string.IsNullOrEmpty(mConnectingMac))
                {
                    DisConnenctBuletooth();
                    return;
                }
                
                if (null == mCannelConDeviceList)
                {
                    mCannelConDeviceList = new List<string>();
                }
                if (!mCannelConDeviceList.Contains(mConnectingMac))
                {
                    mCannelConDeviceList.Add(mConnectingMac);
                }
                isConnecting = false;
                //mPlatInterface.CancelConnectBluetooth();
            }
            catch (System.Exception ex)
            {
                isConnecting = false;
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }
        public void SendMsg(byte cmd, byte[] param, int len)
        {
            try
            {
                mPlatInterface.SendMsg(cmd, param, len);
            }
            catch (System.Exception ex)
            {
                //MyLog.Log("platform SendMsg Exception" + ex.ToString());
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }

        }

        public void SaveModelOrActions(string contents)
        {
            try
            {
                mPlatInterface.SaveModelOrActions(contents);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

        public void DelModel(string contents)
        {
            try
            {
                mPlatInterface.DelModel(contents);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

        public void BackThirdApp()
        {
            try
            {
                RobotMgr.Instance.rbtnametempt = string.Empty;
                Timer.Add(0.5f, 1, 1, mPlatInterface.BackThirdApp);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }
        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="picPath"></param>
        public void Photograph(string modelName, string picPath)
        {
            mPlatInterface.Photograph(modelName, picPath);
        }
        /// <summary>
        /// 保存模型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void SaveModel(string name, ResFileType type = ResFileType.Type_playerdata)
        {
            mPlatInterface.SaveModel(name, type);
        }
        /// <summary>
        /// 发布模型
        /// </summary>
        /// <param name="name"></param>
        public void PublishModel(string name)
        {
            SceneMgr.EnterScene(SceneType.EmptyScene);
            mPlatInterface.PublishModel(name);
        }
        /// <summary>
        /// 激活机器人
        /// </summary>
        /// <param name="mcuId"></param>
        /// <param name="sn"></param>
        public void ActivationRobot(string mcuId, string sn)
        {
            mPlatInterface.ActivationRobot(mcuId, sn);
        }
        /// <summary>
        /// 调用各平台函数
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="arg"></param>
        public void CallPlatformFunc(CallPlatformFuncID funcId, string arg)
        {
            mPlatInterface.CallPlatformFunc(funcId.ToString(), arg);
        }
        /// <summary>
        /// 设置心跳开关
        /// </summary>
        /// <param name="state"></param>
        public void SetSendXTState(bool state, bool needActive = true)
        {
            mPlatInterface.SetSendXTState(state);
            IsWaitUpdateFlag = !state;
            if (needActive)
            {
                if (!state)
                {//防止等待升级的过程中卡死
                    Timer.Add(10, 1, 1, OpenSendXT);
                }
            }
        }

        void OpenSendXT()
        {
            SetSendXTState(true);
        }
        /// <summary>
        /// 获取用户数据
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string GetUserData(UserDataType dataType = UserDataType.userId)
        {
            return mPlatInterface.GetPlatformData(dataType.ToString());
        }
        /// <summary>
        /// 友盟统计事件
        /// </summary>
        /// <param name="id"></param>
        public void MobClickEvent(MobClickEventID id)
        {
            MobClickEvent(id, string.Empty);
        }
        /// <summary>
        /// 友盟统计事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="arg"></param>
        public void MobClickEvent(MobClickEventID id, object arg)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["eventName"] = id.ToString();
            dict["params"] = arg.ToString();
            string result = Json.Serialize(dict);
            Debuger.Log("MobClickEvent = " + result);
            CallPlatformFunc(CallPlatformFuncID.MobClickEvent, result);
        }


        public bool IsConnectedSpeaker(string speaker)
        {
            return mPlatInterface.IsConnectedSpeaker(speaker);
        }

        public void ConnectSpeaker(string speaker)
        {
            if (!string.IsNullOrEmpty(mConnectingSpeakerMac))
            {
                return;
            }
            mConnectingSpeakerMac = speaker;
            mPlatInterface.ConnectSpeaker(speaker);
        }

        public void DisConnectSpeaker(string speaker)
        {
            mConnectingSpeakerMac = string.Empty;
            mConnectedSpeakerMac = string.Empty;
            mPlatInterface.DisConnectSpeaker(speaker);
        }
        #endregion

        #region Platform回调Unity
        //蓝牙连接结果
        public void ConnenctCallBack(string str)
        {
            Debuger.Log(string.Format("ConnenctCallBack str = {0} isConnecting = {1} mConnectingMac = {2}", str, isConnecting, mConnectingMac));
            if (isConnecting)
            {
                isConnecting = false;
                mConnectingMac = string.Empty;
                bool result = true;
                NetWork.GetInst().ClearAllMsg();
                if (string.IsNullOrEmpty(str))
                {
                    PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                    result = false;
                    /*if (StepManager.GetIns().OpenOrCloseGuide)
                    {
                        EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().BTSelectStep, false));
                    }*/
                    m_blueMgr.MatchResult(result);
                }
                else
                {
                    //mConnectedMac = str;
                    //mConnectedName = m_blueMgr.GetNameForMac(str);
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
                    if (null != robot)
                    {
                        m_blueMgr.MatchResult(result);
                        robot.ConnectRobotResult(str, result);
                        robot.ReadDeviceType();
                    }
                    else
                    {
                        DisConnenctBuletooth();
                    }
                }
            }
            else
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                DisConnenctBuletooth();
                if (null != mCannelConDeviceList)
                {
                    mCannelConDeviceList.Clear();
                }
            }
        }

        void ReadDeviceResult(EventArg arg)
        {
            try
            {
                Robot robot = (Robot)arg[0];
                bool result = (bool)arg[1];
                Debuger.Log(string.Format("ReadDeviceResult robotMac = {0} result = {1}", robot.Mac, result));
                //PopWinManager.GetInst().ClosePopWin(typeof(ConnenctBluetoothMsg));
                if (result)
                {
                    robot.StartReadSystemPower();
                    robot.SelfCheck(false);
                    robot.HandShake();
                    //2秒以后读取初始角度
                    //ClientMain.GetInst().WaitTimeInvoke(2, robot.ReadMotherboardData);
                    Timer.Add(2, 1, 1, robot.ReadMotherboardData);
                    /*if (StepManager.GetIns().OpenOrCloseGuide)
                    {
                        StartCoroutine(GuideWaitSometime(2f, true));
                    }*/
                }
                else
                {
                    PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                    /*if (StepManager.GetIns().OpenOrCloseGuide)
                    {
                        EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().BTSelectStep, false));
                    }*/
                    DisConnenctBuletooth();
                }
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                /*if (StepManager.GetIns().OpenOrCloseGuide)
                {
                    EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().BTSelectStep, false));
                }*/
                DisConnenctBuletooth();
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            
        }

        //发现蓝牙已匹配过的设备
        public void OnMatchedDevice(string name)
        {
            m_blueMgr.MatchedFound(name);
        }

        //当发现蓝牙未匹配过的设备
        public void OnFoundDevice(string name)
        {
            m_blueMgr.NewFound(name);
        }
        /// <summary>
        /// SendMsg回包
        /// </summary>
        /// <param name="msg">len + cmd + param</param>
        public void OnMsgAck(string msg)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(msg); //Encoding.UTF8.GetBytes(msg);
                MemoryStream readStream = new MemoryStream(bytes);
                BinaryReader br = new BinaryReader(readStream, Encoding.ASCII);
                int macLen = br.ReadByte();
                byte[] mac = br.ReadBytes(macLen);
                string macStr = Encoding.UTF8.GetString(mac);
                int len = br.ReadByte();
                byte cmd = br.ReadByte();
                if (ClientMain.Exception_Log_Flag)
                {
                    byte[] paramAry = new byte[bytes.Length - 3 - macLen];
                    for (int i = 0, imax = paramAry.Length; i < imax; ++i)
                    {
                        paramAry[i] = bytes[3 + macLen + i];
                    }
                    Debuger.Log("mac=" + macStr + ";len=" + len + ";cmd=" + cmd.ToString("X2") + " param = " + PublicFunction.BytesToHexString(paramAry));
                }

#if !Test
                NetWork.GetInst().ReceiveMsg((CMDCode)cmd, len, macStr, br);
#endif
                readStream.Flush();
                readStream.Close();
                br.Close();

            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            
        }

        /// <summary>
        /// 进入unity场景
        /// </summary>
        /// <param name="msg"></param>
        public void GotoScene(string msg)
        {
            try
            {
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string modelId = null;
                    string modelName = null;
                    string picPath = null;
                    int modelType = 0;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelName"))
                    {
                        modelName = data["modelName"].ToString();
                    }
                    if (data.ContainsKey("picPath"))
                    {
                        picPath = data["picPath"].ToString();
                        Pic_Path = picPath;
                    }
                    if (data.ContainsKey("modelType"))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    
                    ResFileType fileType = (ResFileType)modelType;
                    string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(fileType));
                    //RobotMgr.Instance.rbtnametempt = namewithtype;
                    Robot robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                    if (null == robot)
                    {
                        GetRobotData.GetInst().ReadOneRobot(namewithtype);
                        robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                    }
                    if (null != robot)
                    {
                        RobotManager.GetInst().IsCreateRobotFlag = false;
                        robot.ShowName = modelName;
                        if (fileType == ResFileType.Type_default)
                        {
                            SingletonBehaviour<GetRobotData>.GetInst().SelectRobotDefault(modelId);
#if UNITY_ANDROID
                            ActionsManager.GetInst().ReadOfficialActionsXml();
#endif
                        }
                        else if (fileType == ResFileType.Type_playerdata)
                        {
                            SingletonBehaviour<GetRobotData>.GetInst().SelectRobotPlayer(modelId);
                        }
                        else
                        {
                            SingletonBehaviour<GetRobotData>.GetInst().SelectRobotDownload(modelId);
                        }
                        RobotManager.GetInst().ChoiceRobotForID(robot.ID);
                    }
					else if (fileType == ResFileType.Type_playerdata)
                    {//不存在，新建
                        if (!string.IsNullOrEmpty(modelId))
                        {
                            string modelPath = Path.Combine(ResourcesEx.GetRootPath(ResFileType.Type_playerdata), modelId);
                            if (!Directory.Exists(modelPath))
                            {
                                Directory.CreateDirectory(modelPath);
                                PlatformMgr.Instance.SaveModel(modelId);
                            }
                        }
                        RobotManager.GetInst().SetCurrentRobot(null);
                        RecordContactInfo.Instance.openType = "playerdata";
                        RobotMgr.Instance.rbtnametempt = RobotMgr.NameWithType(modelId, "playerdata");
                        RobotManager.GetInst().IsCreateRobotFlag = true;
                        Robot newRobot = RobotManager.GetInst().GetCreateRobot();
                        newRobot.SetRobotMacAndName(newRobot.Mac, namewithtype);
                        newRobot.ShowName = modelName;
                        /*#if UNITY_EDITOR
                        //用于模拟加入社区版本
                        if (PublicFunction.IsInteger(modelId))
                        {
                            RobotManager.GetInst().IsCreateRobotFlag = false;
                            List<byte> list = new List<byte>();
                            int count = int.Parse(modelId);
                            for (byte i = 1; i <= count; ++i)
                            {
                                list.Add(i);
                            }
                            SingletonBehaviour<GetRobotData>.Inst.CreateGO(list, namewithtype);
                        }
#else*/
                        if (PlatformMgr.Instance.GetBluetoothState())
                        {
                            PlatformMgr.Instance.DisConnenctBuletooth();
                        }
                        if (!PlatformMgr.Instance.IsOpenBluetooth())
                        {
                            Timer.Add(0.2f, 1, 1, SearchBluetoothMsg.ShowMsg);
                        }
                        else
                        {
                            SearchBluetoothMsg.ShowMsg();
                        }
                        
                        //PopWinManager.Inst.ShowPopWin(typeof(ConnenctBluetoothMsg));
                        //#endif
                    }
                }
                //ClientMain.GetInst().LoadTexture(Pic_Path, 5);
                ClientMain.GetInst().LoadGameBgTexture();
                if (SceneMgr.GetCurrentSceneType() != SceneType.MainWindow)
                {
                    SceneMgr.EnterScene(SceneType.MainWindow);
                }
                else
                {
                    EventMgr.Inst.Fire(EventID.Set_Choice_Robot);
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
            
        }
        /// <summary>
        /// 拍照返回
        /// </summary>
        /// <param name="msg">图片路径</param>
        public void PhotographBack(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Pic_Path = msg;
                EventMgr.Inst.Fire(EventID.Photograph_Back);
            }
        }
        /// <summary>
        /// 下载了新的模型
        /// </summary>
        /// <param name="msg"></param>
        public void DownloadModel(string msg)
        {
            try
            {
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string modelId = null;
                    int modelType = 0;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelType"))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    
                    ResFileType fileType = (ResFileType)modelType;
                    if (fileType == ResFileType.Type_default)
                    {
                        RecordContactInfo.Instance.openType = "default";
                        string robotName = SingletonBehaviour<GetRobotData>.GetInst().AddMoreFile(fileType, modelId);
                        if (!string.IsNullOrEmpty(robotName))
                        {//增加官方动作
                            Robot tmpRobot = RobotManager.GetInst().GetRobotForName(robotName);
                            if (null != tmpRobot)
                            {
                                List<string> actionList = ActionsManager.GetInst().GetActionsIDList(tmpRobot.ID);
                                for (int i = 0, imax = actionList.Count; i < imax; ++i)
                                {
                                    ActionSequence actions = ActionsManager.GetInst().GetActionForID(tmpRobot.ID, actionList[i]);
                                    if (null != actions && actions.IsOfficial())
                                    {
                                        ActionsManager.GetInst().AddOfficial(actionList[i]);
                                    }
                                }
                                ActionsManager.GetInst().SaveOfficialActions();
                            }
                        }
                    }
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
        }

        public void ChangeRobotShowName(string msg)
        {
            try
            {
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot)
                {
                    robot.ShowName = msg;
                }
                EventMgr.Inst.Fire(EventID.Change_Robot_Name_Back, new EventArg(msg));
                SceneMgr.EnterScene(SceneType.MainWindow);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }
        /// <summary>
        /// 删除了模型
        /// </summary>
        /// <param name="msg"></param>
        public void DeleteModel(string msg)
        {
            try
            {
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string modelId = null;
                    int modelType = 0;
                    if (data.ContainsKey("modelID"))
                    {
                        modelId = data["modelID"].ToString();
                    }
                    if (data.ContainsKey("modelType"))
                    {
                        modelType = int.Parse(data["modelType"].ToString());
                    }
                    ResFileType fileType = (ResFileType)modelType;
                    string modelName = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(fileType));
                    Robot robot = RobotManager.GetInst().GetRobotForName(modelName);
                    if (null != robot)
                    {
                        //删除遥控器数据
                        ControllerManager.DeletaController(robot.ID);
                    }
                    SingletonBehaviour<GetRobotData>.GetInst().DeleteRobotData(modelName);

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
        }

        /// <summary>
        /// 调用unity里面的方法
        /// </summary>
        /// <param name="msg"></param>
        public void CallUnityFunc(string msg)
        {
            try
            {
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(msg);
                if (null != data)
                {
                    string funcName = null;
                    object arg = null;
                    if (data.ContainsKey("funcName"))
                    {
                        funcName = data["funcName"].ToString();
                    }
                    if (data.ContainsKey("arg"))
                    {
                        arg = data["arg"];
                    }
                    if (null != mUnityDelegateDict && mUnityDelegateDict.ContainsKey(funcName) && null != mUnityDelegateDict[funcName])
                    {
                        mUnityDelegateDict[funcName](arg);
                    }
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
        }
        /// <summary>
        /// 断开连接回调
        /// </summary>
        /// <param name="mac"></param>
        public void OnDisConnenct(string mac)
        {
            if (!isConnecting)
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("蓝牙断开"));
                    LogicCtrl.GetInst().NotifyLogicDicBlue();
                }
                RobotManager.GetInst().DisAllConnencted();
                m_blueMgr.DisConnenctBuletooth();
                m_blueMgr.MatchResult(false);
                NeedUpdateFlag = false;
                if (null != mCannelConDeviceList)
                {
                    mCannelConDeviceList.Clear();
                }
            }
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
            if (null != robot)
            {
                robot.CannelConnect();
            }
            NetWork.GetInst().ClearAllMsg();
        }
#endregion

        public void RegisterRobotResult(object arg)
        {
            if (null != arg)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                int result = 0;
                string sn = string.Empty;
                do
                {
                    if (dict.ContainsKey("sn"))
                    {
                        sn = dict["sn"].ToString();
                    }
                    if (dict.ContainsKey("isSuccess"))
                    {
                        result = int.Parse(dict["isSuccess"].ToString());
                        if (1 == result)
                        {
                            break;
                        }
                    }
                    if (dict.ContainsKey("reason"))
                    {
                        result = int.Parse(dict["reason"].ToString()) == 2002 ? 1 : 0;
                    }
                } while (false);
                if (1 == result)
                {
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        robot.ActivationRobotSuccess();
                        if (!string.IsNullOrEmpty(sn))
                        {
                            robot.WriteSn(sn);
                        }
                    }
                }
            }
        }

        private void ModifyServoId(object arg)
        {
            ClientMain.GetInst().LoadGameBgTexture();
            SetScene.GotoSetScene(SetSceneType.SetSceneTypeDevice);
        }


        private void MainboardProgramVersion(object arg)
        {
            if (null != arg)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                string version = string.Empty;
                string filePath = string.Empty;
                do
                {
                    if (dict.ContainsKey("Version"))
                    {
                        version = dict["Version"].ToString();
                    }
                    if (dict.ContainsKey("FilePath"))
                    {
                        filePath = dict["FilePath"].ToString();
                    }
                } while (false);
                if (!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(filePath))
                {
                    Robot_System_Version = version;
                    Robot_System_FilePath = filePath;
                }
            }
        }

        private void SteeringEngineProgramVersion(object arg)
        {
            if (null != arg)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                string version = string.Empty;
                string filePath = string.Empty;
                do
                {
                    if (dict.ContainsKey("Version"))
                    {
                        version = dict["Version"].ToString();
                    }
                    if (dict.ContainsKey("FilePath"))
                    {
                        filePath = dict["FilePath"].ToString();
                    }
                } while (false);
                if (!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(filePath))
                {
                    Robot_Servo_Version = version;
                    Robot_Servo_FilePath = filePath;
                }
            }
        }

        private void LogicCMDCallUnity(object arg)
        {
            try
            {
                LogicCtrl.GetInst().CallUnityCmd((string)arg);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            
        }

        private void ExitLogicView(object arg)
        {
            LogicCtrl.GetInst().ExitLogic();
        }

        private void OpenBlueSearch(object arg)
        {
            LogicCtrl.GetInst().OpenBlueSearch();
        }


        private void ChargeProtectionCallBack(object arg)
        {
            try
            {
                int result = int.Parse(arg.ToString());
                if (result >= 1)
                {
                    isChargePlaying = false;
                }
                else
                {
                    isChargePlaying = true;
                }
                PlayerPrefs.SetInt("isChargePlaying", isChargePlaying ? 1 : 0);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }
        //js异常提示点击按钮返回
        private void JsShowExceptionCallback(object arg)
        {
            try
            {
                int result = int.Parse(arg.ToString());
                LogicCtrl.GetInst().JsExceptionOnClick(1 == result ? true : false);
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

        private void QuitUnity(object arg)
        {
            try
            {
                SceneMgr.EnterScene(SceneType.EmptyScene);
                SingletonObject<SceneManager>.GetInst().CloseCurrentScene();
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

#endregion
    }

    /// <summary>
    /// android或ios平台调用unity函数
    /// </summary>
    public enum CallUnityFuncID
    {
        RegisterRobotResult = 0,//注册机器人结果
        UnitySetupSteeringEngineID = 1,//修改舵机id
        MainboardProgramVersion = 2,//主板升级
        SteeringEngineProgramVersion = 3,//舵机升级
        ChargeProtection = 14,//充电保护
        LogicCMD = 20,//逻辑编程总接口
        ExitLogicView = 21,//退出逻辑编程
        ConnectBLE = 22,//连接蓝牙
        JsShowExceptionCallback = 23,//JS异常提示
        Destroy,//销毁unity
        GiveupVideoState,//放弃视频
    }

    /// <summary>
    /// unity调用各平台函数
    /// </summary>
    public enum CallPlatformFuncID
    {
        ExitSetupSteeringEngineID = 0,//设置舵机界面返回
        OpenLogicProgramming = 1,//打开逻辑编程(arg:模型数据，动作列表)
        LogicCMDResult = 20,//逻辑编程返回总接口
        ConnectBLECallBack = 21,//连接蓝牙返回
        BLEDisconnectNotity,
        CommonTips,
        JsShowException,//js异常信息
        ChargeProtected,//充电保护
        MobClickEvent,//友盟统计
        GetCameraCurrentState,//进入拍摄界面
        GetBluetoothCurrentState,//进入蓝牙连接
        ExitBluetoothCurrentState,//退出蓝牙连接
    }
    /// <summary>
    /// 用户数据
    /// </summary>
    public enum UserDataType : byte
    {
        userId = 0,
    }
    /// <summary>
    /// 友盟统计事件
    /// </summary>
    public enum MobClickEventID : byte
    {
        ModelPage_TappedConnectBluetoothButton = 0,
        ModelPage_TappedResetModelPositionButton,
        ModelPage_TappedBuildModelButton,
        ModelPage_TappedActionListButton,
        ModelPage_TappedCodingButton,
        ModelPage_TappedControllerButton,
        BluetoothConnectionPage_ConnectionSucceeded,
        BluetoothConnectionPage_ConnectionFailed,
    }
}

