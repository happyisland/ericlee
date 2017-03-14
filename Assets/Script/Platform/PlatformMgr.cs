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

        public ReadPowerMsgAck PowerData = new ReadPowerMsgAck();

        //public bool NeedUpdateFlag = false;
        
        PlatformInterface mPlatInterface = null;

        BluetoothMgr m_blueMgr = new BluetoothMgr();

        /// <summary>
        /// 边充边玩,true表示可以边充边玩，false表示不可以
        /// </summary>
        bool isChargePlaying = true;

        string mLastUser = string.Empty;

        public bool IsChargeProtected
        {
            get { return GetBluetoothState() && !isChargePlaying && PowerData.isAdapter; }
        }
        /// <summary>
        /// 等待升级完成标志
        /// </summary>
        public bool IsWaitUpdateFlag = false;

        Dictionary<string, CallUnityDelegate> mUnityDelegateDict;

        public string Pic_Path;

        string mConnectedSpeakerMac = string.Empty;
        string mConnectingSpeakerMac = string.Empty;

        public bool GetBluetoothState()
        {

/*
#if UNITY_EDITOR
            return true;
#endif*/
            return SingletonObject<ConnectManager>.GetInst().GetBluetoothState();
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
            /*if (!PlayerPrefs.HasKey(mac))
            {
                PlayerPrefs.SetString(mac, SingletonObject<ConnectManager>.GetInst().GetConnectedName());
            }*/
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
                return SingletonObject<ConnectManager>.GetInst().GetConnectedName();
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
                return SingletonObject<ConnectManager>.GetInst().GetConnectedMac();
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
            RegesiterCallUnityDelegate(CallUnityFuncID.autoConnect, BlueAutoConnect);
            RegesiterCallUnityDelegate(CallUnityFuncID.Screenshots, Screenshots);
            RegesiterCallUnityDelegate(CallUnityFuncID.SensorProgramVersion, SensorProgramVersion);
            RegesiterCallUnityDelegate(CallUnityFuncID.setServoMode, SetServoModel);
            
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
        }
        void OnDestroy()
        {
        }
        void FixedUpdate()
        {
            SingletonObject<MyTime>.GetInst().Update();
            Timer.Update();
        }

        public void TestCallUnityFunc(CallUnityFuncID id, string arg)
        {
            if (null != mUnityDelegateDict && mUnityDelegateDict.ContainsKey(id.ToString()) && null != mUnityDelegateDict[id.ToString()])
            {
                mUnityDelegateDict[id.ToString()](arg);
            }
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            return false;
        }
        
        public void StartScan()
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "开启蓝牙搜索");
                SingletonObject<ConnectManager>.GetInst().CleanConnectData();
                m_blueMgr.ClearDevice();
                mPlatInterface.StartScan();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void StopScan()
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "停止蓝牙搜索");
                mPlatInterface.StopScan();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        //连接蓝牙设备
        public void ConnenctBluetooth(string mac, string name)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, string.Format("ConnenctBluetooth mac = {0} name = {1}", mac, name));
                SingletonObject<ConnectManager>.GetInst().OnConnect(mac, name);
                mPlatInterface.DisConnenctBuletooth();
                mPlatInterface.ConnenctBluetooth(mac);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

        }
        /// <summary>
        ///  断开蓝牙连接
        /// </summary>
        public void DisConnenctBuletooth()
        {
            SingletonObject<ConnectManager>.GetInst().OnDisconnect();
            //NeedUpdateFlag = false;
            OnlyDisConnectBluetooth();
        }

        public void OnlyDisConnectBluetooth()
        {
            try
            {
                m_blueMgr.ClearDevice();
                DisConnectSpeaker();
                mPlatInterface.DisConnenctBuletooth();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// 取消蓝牙连接
        /// </summary>
        public void CannelConnectBluetooth()
        {
            try
            {
                SingletonObject<ConnectManager>.GetInst().CancelConnecting();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void BackThirdApp()
        {
            try
            {
                if (null != RobotMgr.Instance)
                {
                    RobotMgr.Instance.rbtnametempt = string.Empty;
                }
                Log(MyLogType.LogTypeEvent, "BackThirdApp");
                Timer.Add(0.5f, 1, 1, mPlatInterface.BackThirdApp);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
            Log(MyLogType.LogTypeEvent, "发布模型 name =" + name);
            //SceneMgr.EnterScene(SceneType.EmptyScene);
            mPlatInterface.PublishModel(name);
        }
        /// <summary>
        /// 激活机器人
        /// </summary>
        /// <param name="mcuId"></param>
        /// <param name="sn"></param>
        public void ActivationRobot(string mcuId, string sn)
        {
            Log(MyLogType.LogTypeEvent, string.Format("激活设备 mcu = {0}  sn = {1}", mcuId, sn));
            mPlatInterface.ActivationRobot(mcuId, sn);
        }
        /// <summary>
        /// 调用各平台函数
        /// </summary>
        /// <param name="funcId"></param>
        /// <param name="arg"></param>
        public void CallPlatformFunc(CallPlatformFuncID funcId, string arg)
        {
            Log(MyLogType.LogTypeEvent, arg);
            mPlatInterface.CallPlatformFunc(funcId.ToString(), arg);
        }
        /// <summary>
        /// 设置心跳开关
        /// </summary>
        /// <param name="state"></param>
        public void SetSendXTState(bool state)
        {
            Log(MyLogType.LogTypeEvent, string.Format("设置心跳开关 state = {0}", state.ToString()));
            mPlatInterface.SetSendXTState(state);
            IsWaitUpdateFlag = !state;
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

        public void DisConnectSpeaker()
        {
            mPlatInterface.DisConnectSpeaker(mConnectedSpeakerMac);
            mConnectedSpeakerMac = string.Empty;
            mConnectingSpeakerMac = string.Empty;
        }
        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="modelId"></param>
        /// <param name="modelType"></param>
        /// <param name="filePath"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public bool OperateSyncFile(string modelId, ResFileType modelType, string filePath, OperateFileType operateType)
        {
            Log(MyLogType.LogTypeEvent, string.Format("同步文件 modelId = {0} modelType = {1} operateType = {2} filePath = {3}", modelId, modelType.ToString(), operateType.ToString(), filePath));
            return mPlatInterface.OperateSyncFile(modelId, modelType, filePath, operateType);
        }

        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="filePath"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public bool OperateSyncFile(string robotName, string filePath, OperateFileType operateType)
        {
            string[] ary = robotName.Split('_');
            if (null != ary && ary.Length == 2)
            {
                return mPlatInterface.OperateSyncFile(ary[0], ResourcesEx.GetResFileType(ary[1]), filePath, operateType);
            }
            return false;
        }

        public void Log(MyLogType logType, string text)
        {
            switch (logType)
            {
                case MyLogType.LogTypeInfo:
                    mPlatInterface.LogInfo(text);
                    break;
                case MyLogType.LogTypeDebug:
                    mPlatInterface.LogDebug(text);
                    break;
                case MyLogType.LogTypeEvent:
                    mPlatInterface.LogEvent(text);
                    break;
            }
        }

        public void PopWebErrorType(ConnectionErrorType errorType)
        {
            CallPlatformFunc(CallPlatformFuncID.NotificationNameConnectionError, ((byte)errorType).ToString());
        }
        #endregion

        #region Platform回调Unity
        //蓝牙连接结果
        public void ConnenctCallBack(string str)
        {
            Log(MyLogType.LogTypeEvent, string.Format("ConnenctCallBack = {0}", str));
            if (string.IsNullOrEmpty(str))
            {
                SingletonObject<ConnectManager>.GetInst().ConnectFail();
            }
            else
            {
                SingletonObject<ConnectManager>.GetInst().ConnectSuccess(str);
            }
        }

        /// <summary>
        /// 蓝牙音响连接结果
        /// </summary>
        /// <param name="str"></param>
        public void ConnenctSpeakerCallBack(string str)
        {
            mConnectingSpeakerMac = string.Empty;
            if (string.IsNullOrEmpty(str))
            {//连接失败
                Robot robot = null;
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    robot = RobotManager.GetInst().GetCreateRobot();
                }
                else
                {
                    robot = RobotManager.GetInst().GetCurrentRobot();
                }
                if (null != robot && null != robot.MotherboardData)
                {
                    SpeakerData speakerData = (SpeakerData)robot.GetReadSensorData(TopologyPartType.Speaker);
                    if (null != speakerData)
                    {
                        SpeakerInfoData infoData = speakerData.GetSpeakerData();
                        if (null != infoData)
                        {
                            PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("检测到蓝牙音响需要连接"), infoData.speakerName), PopSpeakerOnClick);
                        }
                    }
                }
                
            }
            else
            {
                mConnectedSpeakerMac = str;
            }
        }

        void PopSpeakerOnClick(GameObject obj)
        {
            if (PromptMsg.RightBtnName.Equals(obj.name))
            {
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.OpenAndroidBLESetting, string.Empty);
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
                byte[] paramAry = new byte[bytes.Length - 3 - macLen];
                for (int i = 0, imax = paramAry.Length; i < imax; ++i)
                {
                    paramAry[i] = bytes[3 + macLen + i];
                }
                Log(MyLogType.LogTypeEvent, "mac=" + macStr + ";len=" + len + ";cmd=" + ((CMDCode)cmd).ToString() + " param = " + PublicFunction.BytesToHexString(paramAry));

#if !Test
                NetWork.GetInst().ReceiveMsg((CMDCode)cmd, len, macStr, br);
#endif
                readStream.Flush();
                readStream.Close();
                br.Close();

            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                Log(MyLogType.LogTypeEvent, "GotoScene msg = " + msg);
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
                    if (!string.IsNullOrEmpty(mLastUser) && !GetUserData().Equals(mLastUser))
                    {//切换了账号，清除模型数据
                        Log(MyLogType.LogTypeDebug, "切换账号，清除数据");
                        try
                        {
                            if (null != RobotMgr.Instance.rbt)
                            {
                                RobotMgr.Instance.rbt.Clear();
                            }
                            ActionsManager.GetInst().CleanUp();
                            RobotManager.GetInst().CleanUp();
                        }
                        catch (System.Exception ex)
                        {
                            Log(MyLogType.LogTypeEvent, "切换账号时清除数据 error = " + ex.ToString());
                        }
                        
                    }
                    mLastUser = GetUserData();
                    ResFileType fileType = (ResFileType)modelType;
                    string namewithtype = RobotMgr.NameWithType(modelId, ResourcesEx.GetFileTypeString(fileType));
                    //RobotMgr.Instance.rbtnametempt = namewithtype;
                    Robot robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                    if (null == robot)
                    {
                        string robotPath = string.Empty;
                        if (fileType == ResFileType.Type_default)
                        {
                            robotPath = ResourcesEx.GetCommonPathForNoTypeName(modelId);
                            GetRobotData.GetInst().ReadOneRobot(namewithtype);
                            robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                        }
                        else
                        {
                            robotPath = ResourcesEx.GetRobotPathForNoTypeName(modelId);
                            if (!Directory.Exists(robotPath))
                            {
                                Directory.CreateDirectory(robotPath);
                                PlatformMgr.Instance.SaveModel(modelId);
                            }
                            else
                            {
                                GetRobotData.GetInst().ReadOneRobot(namewithtype);
                                robot = RobotManager.GetInst().GetRobotForName(namewithtype);
                            }
                        }
                    }
                    if (null != robot)
                    {
                        Log(MyLogType.LogTypeEvent, string.Format("进入已有模型 modelId = {0}, madelName = {1}, modelType = {2}", modelId, modelName, fileType.ToString()));
                        RobotManager.GetInst().IsCreateRobotFlag = false;
                        robot.ShowName = modelName;
                        if (fileType == ResFileType.Type_default)
                        {
                            SingletonBehaviour<GetRobotData>.GetInst().SelectRobotDefault(modelId);
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
                        Log(MyLogType.LogTypeEvent, "不存在模型，走新建流程");
                        RobotManager.GetInst().SetCurrentRobot(null);
                        RecordContactInfo.Instance.openType = "playerdata";
                        RobotMgr.Instance.rbtnametempt = RobotMgr.NameWithType(modelId, "playerdata");
                        RobotManager.GetInst().IsCreateRobotFlag = true;
                        Robot newRobot = RobotManager.GetInst().GetCreateRobot();
                        newRobot.SetRobotMacAndName(newRobot.Mac, namewithtype);
                        newRobot.ShowName = modelName;
                        
                        if (PlatformMgr.Instance.GetBluetoothState())
                        {
                            PlatformMgr.Instance.DisConnenctBuletooth();
                        }
                        if (!PlatformMgr.Instance.IsOpenBluetooth())
                        {
                            Timer.Add(0.2f, 1, 1, ConnectBluetoothMsg.ShowMsg);
                        }
                        else
                        {
                            ConnectBluetoothMsg.ShowMsg();
                        }
                        
                    }
                }
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }
        /// <summary>
        /// 拍照返回
        /// </summary>
        /// <param name="msg">图片路径</param>
        public void PhotographBack(string msg)
        {
            Log(MyLogType.LogTypeEvent, "拍照返回 msg = " + msg);
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
                        SingletonBehaviour<GetRobotData>.GetInst().AddMoreFile(fileType, modelId);
                        /*if (!string.IsNullOrEmpty(robotName))
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
                        }*/
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        public void ChangeRobotShowName(string msg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "修改模型名字返回" + msg);
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot && ResourcesEx.GetRobotType(robot) == ResFileType.Type_playerdata)
                {
                    robot.ShowName = msg;
                }
                else
                {
                    return;
                }
                EventMgr.Inst.Fire(EventID.Change_Robot_Name_Back, new EventArg(msg));
                if (SceneMgr.GetCurrentSceneType() != SceneType.MainWindow)
                {
                    SceneMgr.EnterScene(SceneType.MainWindow);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                Log(MyLogType.LogTypeDebug, msg);
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        /// <summary>
        /// 断开连接回调
        /// </summary>
        /// <param name="mac"></param>
        public void OnDisConnenct(string mac)
        {
            SingletonObject<ConnectManager>.GetInst().DisconnectNotify();
            m_blueMgr.ClearDevice();
            //NeedUpdateFlag = false;
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
            if (SceneManager.GetInst().GetCurrentScene() == null || SceneManager.GetInst().GetCurrentScene().GetType() != typeof(SetScene))
            {
                SetScene.GotoSetScene(SetSceneType.SetSceneTypeDevice);
            }
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
                Log(MyLogType.LogTypeEvent, string.Format("主板本地程序:version={0}  filePath={1}", version, filePath));
                SingletonObject<UpdateManager>.GetInst().Robot_System_Version = version;
                SingletonObject<UpdateManager>.GetInst().Robot_System_FilePath = filePath;
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
                Log(MyLogType.LogTypeEvent, string.Format("舵机本地程序:version={0}  filePath={1}", version, filePath));
                SingletonObject<UpdateManager>.GetInst().Robot_Servo_Version = version;
                SingletonObject<UpdateManager>.GetInst().Robot_Servo_FilePath = filePath;
            }
        }


        private void SensorProgramVersion(object arg)
        {
            if (null != arg)
            {
                try
                {
                    Dictionary<string, object> dict = (Dictionary<string, object>)arg;
                    int sensorType = 0;
                    string version = string.Empty;
                    string filePath = string.Empty;
                    do
                    {
                        if (dict.ContainsKey("sensorType"))
                        {
                            sensorType = int.Parse(dict["sensorType"].ToString());
                            if (sensorType > 100 && sensorType < 200)
                            {
                                sensorType -= 100;
                            }
                        }
                        else
                        {
                            break;
                        }
                        if (dict.ContainsKey("Version"))
                        {
                            version = dict["Version"].ToString();
                        }
                        else
                        {
                            break;
                        }
                        if (dict.ContainsKey("FilePath"))
                        {
                            filePath = dict["FilePath"].ToString();
                        }
                        else
                        {
                            break;
                        }
                        TopologyPartType partType = (TopologyPartType)sensorType;
                        SingletonObject<UpdateManager>.GetInst().SetSensorUpdateData(partType, version, filePath);
                        string sensorInfo = string.Format("{0}|{1}", version, filePath);
                        PlayerPrefs.SetString(partType.ToString(), sensorInfo);
                        Log(MyLogType.LogTypeDebug, "保存传感器数据 sensorTyoe = " + partType.ToString() + " sensorInfo = " + sensorInfo);
                        PlayerPrefs.Save();
                    } while (false);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }

        private void LogicCMDCallUnity(object arg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "逻辑编程结果返回 = " + (string)arg);
                LogicCtrl.GetInst().CallUnityCmd((string)arg);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

        private void ExitLogicView(object arg)
        {
            Log(MyLogType.LogTypeEvent, "退出逻辑编程");
            LogicCtrl.GetInst().ExitLogic();
        }

        private void OpenBlueSearch(object arg)
        {
            Log(MyLogType.LogTypeEvent, "打开蓝牙搜索页面");
            LogicCtrl.GetInst().OpenBlueSearch();
        }

        private void SetServoModel(object arg)
        {
            Log(MyLogType.LogTypeEvent, "设置舵机模式");
            SingletonObject<LogicCtrl>.GetInst().OpenSetServoModel();
        }


        private void ChargeProtectionCallBack(object arg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "设置充电保护 result = " + arg.ToString());
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
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        //js异常提示点击按钮返回
        private void JsShowExceptionCallback(object arg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "js异常提示点击按钮返回" + arg.ToString());
                int result = int.Parse(arg.ToString());
                LogicCtrl.GetInst().JsExceptionOnClick(1 == result ? true : false);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        private void QuitUnity(object arg)
        {
            try
            {
                Log(MyLogType.LogTypeEvent, "退出unity");
                SceneMgr.EnterScene(SceneType.EmptyScene);
                SingletonObject<SceneManager>.GetInst().CloseCurrentScene();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString()); 
            }
        }

        void BlueAutoConnect(object arg)
        {
            Log(MyLogType.LogTypeEvent, "设置自动连接" + arg.ToString());
            int result = int.Parse(arg.ToString());
            SingletonObject<ConnectManager>.GetInst().SetAutoConnectFlag(result == 1 ? true : false);
        }

        void Screenshots(object arg)
        {
            try
            {
                SingletonBehaviour<Screenshots>.GetInst().SaveScreenshots();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
        CurrentStartPlaying,//当前状态是开始录制
        //CurrentStopPlaying,//当前状态是停止录制
        CurrentSaveVideo,//当前状态可以保存视频
        ReplayVideoState,//重拍当前预览的视频
        autoConnect,//自动连接开关
        SensorProgramVersion,//传感器版本
        Screenshots,//截图
        setServoMode,//设置舵机模式
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
        GiveupCurrentVideo,//放弃录制的视频
        StartPlayingVideo,//开始录制视频
        StopPlayingVideo,//停止录制视频
        SavedCurrentVideo,//保存当前视频
        GetBluetoothCurrentState,//进入蓝牙连接
        ExitBluetoothCurrentState,//退出蓝牙连接
        JsExceptionWaitResult,//异常等待返回
        ExitPlayVideoMode,//充电保护下注销拍摄视频模式
        OpenAndroidBLESetting,//打开android蓝牙设置
        NotificationNameConnectionError,
        refreshAllServo,//设置舵机返回
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
        ModelPage_CreateActionCount,    //新建模型动作数量
    }

    public enum MyLogType : byte
    {
        LogTypeEvent = 0,//重要日志
        LogTypeInfo,//一般日志
        LogTypeDebug,//调试日志
    }
    /// <summary>
    /// 连接异常
    /// </summary>
    public enum ConnectionErrorType : byte
    {
        ConnectionUnknowErrorType = 0,
        ConnectionSearchJimuType = 1,
        ConnectionServoIdRepeatType = 2,
        ConnectionServoVSLineType = 3,
        ConnectionServoNumVsLineType = 4,
        ConnectionServoLineErrorType = 5,
        ConnectionFirmwareUpdateErrorType = 6,
    }
}

