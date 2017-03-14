using Game.Platform;
using System;
using System.Collections.Generic;
using LitJson;
using System.Text;
using Game.Resource;
using Game.Scene;
using Game;
using UnityEngine;
using Game.Event;
/// <summary>
/// 逻辑编程
/// </summary>
public class LogicCtrl : SingletonObject<LogicCtrl>
{
    public delegate void ExceptionDelegate(bool confirmFlag);

    public delegate void LogicCmdCallBack(string result);

    delegate void CallUnityCmdDelegate(string[] args);

    class WaitCmdData
    {
        public string[] args;
        public int waitMs;
        long startTime;

        public WaitCmdData(int waitTime, string[] args)
        {
            this.args = args;
            this.waitMs = waitTime;
            startTime = PublicFunction.GetNowMillisecond();
        }

        public bool IsTimeOut()
        {
            if (PublicFunction.GetNowMillisecond() - startTime >= 0)
            {
                return true;
            }
            return false;
        }
    }

    public LogicCmdCallBack logicCmdCallBack = null;

    /// <summary>
    /// 是否是处于逻辑编程通讯
    /// </summary>
    bool isLogicProgramming = false;
    public bool IsLogicProgramming
    {
        get { return isLogicProgramming; }
        set { isLogicProgramming = value; }
    }
    /// <summary>
    /// 由逻辑编程打开了蓝牙连接
    /// </summary>
    bool isLogicOpenSearchFlag = false;
    public bool IsLogicOpenSearchFlag
    {
        get { return isLogicOpenSearchFlag; }
        set { isLogicOpenSearchFlag = value; }
    }
    Dictionary<string, CallUnityCmdDelegate> mCallCmdDict;
#if UNITY_ANDROID
    public readonly string Logic_Cmd_Start = "jimu://";
#else
    public readonly string Logic_Cmd_Start = "https://js.jimu.com/";
#endif

    /// <summary>
    /// 正在执行的命令
    /// </summary>
    Dictionary<string, string[]> mRuningCmdDict;
    /// <summary>
    /// 正在等待执行的命令
    /// </summary>
    Dictionary<string, WaitCmdData> mWaitCmdDict;

    Robot mRobot;

    float mShowTipsLastTime = 0;

    bool mServoSetCallBackFlag = false;
    bool mServoSetFinishedFlag = false;
    long mWaitServoSetIndex = -1;

    bool mChargeProtectedFlag = false;//充电保护提示

    ExceptionDelegate mExceptionCallBack = null;//异常处理
    bool mExceptionTipsFlag = false;
    bool mWaitExceptionRepairFlag = false;
    /// <summary>
    /// 返回次数
    /// </summary>
    int mNeedWaitSensorCallBack = 0;

    int mSetLEDCount = 0;
    int mSetEmojiCount = 0;
    int mSetDigitalTubeCount = 0;
    
    public LogicCtrl()
    {
        mCallCmdDict = new Dictionary<string, CallUnityCmdDelegate>();
        mCallCmdDict[LogicCmd.query.ToString()] = LogicQuery;
        mCallCmdDict[LogicCmd.action.ToString()] = LogicAction;
        mCallCmdDict[LogicCmd.servoPowerOn.ToString()] = LogicServoPowerOn;
        mCallCmdDict[LogicCmd.servoPowerOff.ToString()] = LogicServoPowerOff;
        mCallCmdDict[LogicCmd.servoSet.ToString()] = LogicServoSet;
        mCallCmdDict[LogicCmd.servoSetbySpeed.ToString()] = LogicServoSetbySpeed;
        mCallCmdDict[LogicCmd.stopServo.ToString()] = LogicStopServo;
        mCallCmdDict[LogicCmd.getPosture.ToString()] = LogicGetPosture;
        mCallCmdDict[LogicCmd.stopRobot.ToString()] = LogicStopRobot;
        mCallCmdDict[LogicCmd.adjustServo.ToString()] = LogicAdjustServo;
        mCallCmdDict[LogicCmd.DisconnectBLE.ToString()] = LogicDisConnenct;
        mCallCmdDict[LogicCmd.queryInfrared.ToString()] = LogicQueryInfrared;
        mCallCmdDict[LogicCmd.queryTouchStatus.ToString()] = LogicQueryTouchStatue;
        mCallCmdDict[LogicCmd.queryGyroscope.ToString()] = LogicQueryGyroscope;
        mCallCmdDict[LogicCmd.setLEDs.ToString()] = LogicSetLEDs;
        mCallCmdDict[LogicCmd.setEmoji.ToString()] = LogicSetEmoji;
        mCallCmdDict[LogicCmd.setDigitalTube.ToString()] = LogicSetDigitalTube;
        mCallCmdDict[LogicCmd.queryAllSensor.ToString()] = LogicQueryAllSensor;
        mCallCmdDict[LogicCmd.setSensorLED.ToString()] = LogicCtrlSensorLED;
    }
    public void CallUnityCmd(string argStr)
    {
        PlatformMgr.Instance.Log(Game.Platform.MyLogType.LogTypeDebug, string.Format("CallUnityCmd = {0}", argStr));
        do 
        {
            if (!mWaitExceptionRepairFlag && argStr.StartsWith(Logic_Cmd_Start))
            {
                argStr = argStr.Substring(Logic_Cmd_Start.Length);
                string[] args = argStr.Split('|');
                if (null != args)
                {
                    string cmd = args[0];
                    if (mCallCmdDict.ContainsKey(cmd) && null != mCallCmdDict[cmd])
                    {
                        if (null == mRuningCmdDict)
                        {
                            mRuningCmdDict = new Dictionary<string, string[]>();
                        }
                        if (mRuningCmdDict.ContainsKey(cmd) && (null == mWaitCmdDict || !mWaitCmdDict.ContainsKey(cmd) || !mWaitCmdDict[cmd].IsTimeOut()))
                        {//同个执行需等待
                            if (null == mWaitCmdDict)
                            {
                                mWaitCmdDict = new Dictionary<string, WaitCmdData>();
                            }
                            //若有同种命令等待，则已最新的状态更新
                            int waitTime = 0;
                            if (cmd.Equals(LogicCmd.stopRobot.ToString()))
                            {
                                waitTime = 1;
                            }
                            else if (cmd.Equals(LogicCmd.queryAllSensor.ToString()) || cmd.Equals(LogicCmd.queryTouchStatus.ToString()) || cmd.Equals(LogicCmd.queryInfrared.ToString()) || cmd.Equals(LogicCmd.queryGyroscope.ToString()))
                            {
                                waitTime = 1000;
                            }
                            WaitCmdData waitData = new WaitCmdData(waitTime, args);
                            mWaitCmdDict[cmd] = waitData;
                        }
                        else
                        {
                            if (null != mWaitCmdDict && mWaitCmdDict.ContainsKey(cmd))
                            {
                                mWaitCmdDict.Remove(cmd);
                            }
                            mRuningCmdDict[cmd] = args;
                            mCallCmdDict[cmd](args);
                        }
                        return;
                    }
                }
            }
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["result"] = CallUnityResult.failure.ToString();
            dict["cmd"] = argStr;
            string jsonbill = Json.Serialize(dict);
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.LogicCMDResult, jsonbill);
        } while (false);
    }


    /// <summary>
    /// 打开逻辑编程
    /// </summary>
    public void OpenLogicForRobot(Robot robot)
    {
        try
        {
            CleanUp();
            IsLogicProgramming = true;
            mRobot = robot;
            StringBuilder sb = new StringBuilder();
            List<string> actList = robot.GetActionsIdList();
            for (int i = 0, imax = actList.Count; i < imax; ++i)
            {
                ActionSequence actions = robot.GetActionsForID(actList[i]);
                if (null != actions)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(PublicFunction.Separator_Or);
                    }
                    sb.Append(actions.Id);
                    sb.Append(PublicFunction.Separator_Comma);
                    sb.Append(actions.Name);
                }
            }
            string servoList = PublicFunction.ListToString<byte>(robot.GetAllDjData().GetAngleList());
            string circleServos = PublicFunction.ListToString<byte>(robot.GetAllDjData().GetTurnList());

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["action"] = sb.ToString();
            ResFileType type = ResourcesEx.GetResFileType(RobotMgr.DataType(robot.Name));
            dict["modelID"] = RobotMgr.NameNoType(mRobot.Name);
            dict["modelType"] = ((int)type).ToString();
            dict["servo"] = servoList;
            dict["circleServos"] = circleServos;
            AddSensorData(ref dict);
            
            string jsonbill = Json.Serialize(dict);
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.OpenLogicProgramming, jsonbill);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void AddSensorData(ref Dictionary<string, string> dict)
    {
        string infraredId = string.Empty;
        string touchId = string.Empty;
        string gyroscopeId = string.Empty;
        string lights = string.Empty;
        if (null != mRobot && null != mRobot.MotherboardData)
        {
            if (null != mRobot.GetReadSensorData(TopologyPartType.Infrared) && null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Infrared))
            {
                infraredId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Infrared).ids);
            }
            if (null != mRobot.GetReadSensorData(TopologyPartType.Touch) && null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Touch))
            {
                touchId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Touch).ids);
            }
            if (null != mRobot.GetReadSensorData(TopologyPartType.Gyro) && null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Gyro))
            {
                gyroscopeId = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Gyro).ids);
            }
            if (null != mRobot.GetReadSensorData(TopologyPartType.Light) && null != mRobot.MotherboardData.GetSensorData(TopologyPartType.Light))
            {
                lights = PublicFunction.ListToString<byte>(mRobot.MotherboardData.GetSensorData(TopologyPartType.Light).ids);
            }
        }
        dict["infraredId"] = infraredId;
        dict["touchId"] = touchId;
        dict["gyroscopeId"] = gyroscopeId;
        dict["lights"] = lights;
    }

    public void ExitLogic()
    {
        CleanUp();
        SceneMgr.EnterScene(SceneType.MainWindow);
    }

    public void OpenBlueSearch()
    {
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedConnectBluetoothButton);
        if (null != mRuningCmdDict)
        {
            mRuningCmdDict.Clear();
        }
        if (null != mWaitCmdDict)
        {
            mWaitCmdDict.Clear();
        }
        isLogicOpenSearchFlag = true;
        ConnectBluetoothMsg.ShowMsg();
    }

    public void CloseBlueSearch()
    {
        if (!PopWinManager.GetInst().IsExist(typeof(TopologyBaseMsg)) && !PopWinManager.GetInst().IsExist(typeof(ConnectBluetoothMsg)))
        {
            EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
        }
        if (isLogicOpenSearchFlag)
        {
            isLogicOpenSearchFlag = false;
            
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["blueState"] = (PlatformMgr.Instance.GetBluetoothState() ? 1 : 0).ToString();
            AddSensorData(ref dict);
            string jsonbill = Json.Serialize(dict);
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ConnectBLECallBack, jsonbill);
            if (PlatformMgr.Instance.IsChargeProtected)
            {
                LogicCtrl.GetInst().ChargeProtectedCallBack();
            }
        }
    }


    public void OpenSetServoModel()
    {
        SetServoTypeMsg.ShowMsg(CloseSetServoModel);
    }

    public void CloseSetServoModel()
    {
        string servoList = string.Empty;
        if (null != mRobot)
        {
            servoList = PublicFunction.ListToString<byte>(mRobot.GetAllDjData().GetAngleList(), PublicFunction.Separator_Or);
        }
        string circleServos = string.Empty;
        if (null != mRobot)
        {
            circleServos = PublicFunction.ListToString<byte>(mRobot.GetAllDjData().GetTurnList(), PublicFunction.Separator_Or);
        }
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict["commonServo"] = servoList;
        dict["circleServo"] = circleServos;
        string jsonbill = Json.Serialize(dict);
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.refreshAllServo, jsonbill);
        /*if (PlatformMgr.Instance.IsChargeProtected)
        {
            LogicCtrl.GetInst().ChargeProtectedCallBack();
        }*/
    }

    public void NotifyLogicDicBlue()
    {
        mWaitExceptionRepairFlag = false;
        if (IsLogicProgramming)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.BLEDisconnectNotity, LogicLanguage.GetText("蓝牙断开"));
        }
    }
    /// <summary>
    /// 异常信息返回
    /// </summary>
    /// <param name="exceptionString"></param>
    public void ExceptionCallBack(string exceptionString, ExceptionDelegate exDlgt)
    {
        if (IsLogicProgramming && !mExceptionTipsFlag)
        {
            mExceptionTipsFlag = true;
            mWaitExceptionRepairFlag = true;
            mExceptionCallBack = exDlgt;
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.JsShowException, exceptionString);
            if (!SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag)
            {
                PromptMsg.ShowDoublePrompt(exceptionString, SelfCheckErrorOnClick);
            }
        }
    }

    void SelfCheckErrorOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals(PromptMsg.LeftBtnName))
        {
            JsExceptionOnClick(false);
        }
        else if (btnName.Equals(PromptMsg.RightBtnName))
        {
            JsExceptionOnClick(true);
        }
    }

    public void JsExceptionOnClick(bool confirmFlag)
    {
        mExceptionTipsFlag = false;
        if (null != mExceptionCallBack)
        {
            if (!confirmFlag)
            {
                mWaitExceptionRepairFlag = false;
            }
            mExceptionCallBack(confirmFlag);
            mExceptionCallBack = null;
        }
    }
    /// <summary>
    /// 普通提示，不做其他处理
    /// </summary>
    /// <param name="tips"></param>
    public void CommonTipsCallBack(string tips, float intervalTime, CommonTipsColor color)
    {
        if (IsLogicProgramming)
        {
            if (0 != mShowTipsLastTime && Time.time - mShowTipsLastTime < intervalTime)
            {
                return;
            }
            mShowTipsLastTime = Time.time;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["msg"] = tips;
            dict["level"] = (byte)color;
            string jsonbill = Json.Serialize(dict);
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.CommonTips, jsonbill);
        }
    }
    /// <summary>
    /// 充电保护
    /// </summary>
    public void ChargeProtectedCallBack()
    {
        if (IsLogicProgramming && !isLogicOpenSearchFlag)
        {
            if (!mChargeProtectedFlag)
            {
                mChargeProtectedFlag = true;
                PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ChargeProtected, LogicLanguage.GetText("禁止边充边玩"));
            }
        }
    }


    public override void CleanUp()
    {
        base.CleanUp();
        mShowTipsLastTime = 0;
        IsLogicProgramming = false;
        mExceptionTipsFlag = false;
        mWaitExceptionRepairFlag = false;
        mExceptionCallBack = null;
        if (null != mRuningCmdDict)
        {
            mRuningCmdDict.Clear();
        }
        if (null != mWaitCmdDict)
        {
            mWaitCmdDict.Clear();
        }
        mChargeProtectedFlag = false;
    }

    #region 逻辑编程调用unity命令
    /// <summary>
    /// 查询命令
    /// </summary>
    /// <param name="args">Model/Servo</param>
    void LogicQuery(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_OK);
            }
            else
            {
                CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
                if (!PlatformMgr.Instance.GetBluetoothState())
                {
                    cmdResult = CallUnityErrorCode.Blue_DisConnect;
                }
                Dictionary<string, object> data = (Dictionary<string, object>)Json.Deserialize(args[1]);
                CallUnityResult result = CallUnityResult.success;
                do
                {
                    if (null == data)
                    {
                        result = CallUnityResult.failure;
                        break;
                    }
                    if (data.ContainsKey("model"))
                    {
                        if (data["model"].ToString().Equals("true") && PlatformMgr.Instance.PowerData.percentage > 20)
                        {
                            result = CallUnityResult.failure;
                        }
                    }
                    if (result == CallUnityResult.success && data.ContainsKey("servos"))
                    {//查询舵机掉电，如果舵机为空则查询所有舵机上电
                        string servos = data["servos"].ToString();
                        if (string.IsNullOrEmpty(servos))
                        {//查询所有舵机上电
                            ModelDjData servoData = mRobot.GetAllDjData();
                            foreach (KeyValuePair<byte, DuoJiData> kvp in servoData.GetAllData())
                            {
                                if (kvp.Value.isPowerDown)
                                {
                                    result = CallUnityResult.failure;
                                    break;
                                }
                            }
                        }
                        else
                        {//查询舵机掉电
                            List<int> list = PublicFunction.StringToList(servos);
                            for (int i = 0, imax = list.Count; i < imax; ++i)
                            {
                                DuoJiData servoData = mRobot.GetAnDjData(list[i]);
                                if (!servoData.isPowerDown)
                                {
                                    result = CallUnityResult.failure;
                                    break;
                                }
                            }
                        }
                    }
                } while (false);
                CmdCallBack(args[0], result, cmdResult);
                /*if (args[1].Equals("Model"))
                {
                    GetPowerState();
                }
                else if (args[1].Equals("Servo"))
                {
                    GetServoPowerState();
                }*/
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_OK);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    /// <summary>
    /// 获取电量状态
    /// </summary>
    void GetPowerState()
    {
        CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
        if (!PlatformMgr.Instance.GetBluetoothState())
        {
            cmdResult = CallUnityErrorCode.Blue_DisConnect;
        }
        CallUnityResult result;
        if (PlatformMgr.Instance.PowerData.isAdapter)
        {//接了适配器
            if (PlatformMgr.Instance.PowerData.power > PublicFunction.Robot_Power_Max)
            {
                result = CallUnityResult.highPower;
            }
            else
            {
                result = CallUnityResult.highPower;
            }
        }
        else
        {
            if (PlatformMgr.Instance.PowerData.power < PublicFunction.Robot_Power_Min)
            {
                result = CallUnityResult.lowPower;
            }
            else
            {
                result = CallUnityResult.highPower;
            }
        }
        CmdCallBack(LogicCmd.query.ToString(), result, cmdResult);
    }

    /// <summary>
    /// 获取舵机掉电状态
    /// </summary>
    void GetServoPowerState()
    {
        CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
        if (!PlatformMgr.Instance.GetBluetoothState())
        {
            cmdResult = CallUnityErrorCode.Blue_DisConnect;
        }
        CallUnityResult result = CallUnityResult.powerOff;
        ModelDjData servoData = mRobot.GetAllDjData();
        foreach (KeyValuePair<byte, DuoJiData> kvp in servoData.GetAllData())
        {
            if (!kvp.Value.isPowerDown)
            {
                result = CallUnityResult.powerOn;
                break;
            }
        }
        CmdCallBack(LogicCmd.query.ToString(), result, cmdResult);
    }

    /// <summary>
    /// 执行动作命令
    /// </summary>
    /// <param name="args">动作id</param>
    void LogicAction(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                string actId = args[1];
                CallUnityErrorCode cmdResult = CallUnityErrorCode.Result_OK;
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    ErrorCode ret = mRobot.PlayActionsForID(actId);
                    if (ErrorCode.Result_Action_Not_Exist == ret)
                    {
                        cmdResult = CallUnityErrorCode.Actions_Not_Exist;
                    }
                }
                else
                {
                    cmdResult = CallUnityErrorCode.Blue_DisConnect;
                }
                if (cmdResult != CallUnityErrorCode.Result_OK)
                {
                    CmdCallBack(args[0], CallUnityResult.failure, CallUnityErrorCode.Result_None);
                }
            }
            
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 设置舵机上电
    /// </summary>
    /// <param name="arg"></param>
    void LogicServoPowerOn(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    if (string.IsNullOrEmpty(args[1]) || args[1].Equals("0"))
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                    else
                    {
                        List<byte> servos = PublicFunction.StringToByteList(args[1]);
                        mRobot.ServoPowerOn(servos);
                    }
                    
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
                
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置舵机掉电
    /// </summary>
    /// <param name="args"></param>
    void LogicServoPowerOff(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    if (string.IsNullOrEmpty(args[1]) || args[1].Equals("0"))
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                    else
                    {
                        List<byte> servos = PublicFunction.StringToByteList(args[1]);
                        mRobot.ServoPowerDown(servos);
                    }
                        
                    //CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }

            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 调节舵机角度，用于coding界面时看效果
    /// </summary>
    /// <param name="args"></param>
    void LogicAdjustServo(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {
                RunCmdFinished(args[0]);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<byte, byte> rotas = new Dictionary<byte, byte>();
                    int time = -1;
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            if (-1 == time && dict.ContainsKey("ms"))
                            {
                                time = int.Parse(dict["ms"].ToString());
                            }
                            byte servo = byte.Parse(dict["servo"].ToString());
                            int angle = int.Parse(dict["degree"].ToString()) + PublicFunction.DuoJi_Start_Rota;
                            if (angle < PublicFunction.DuoJi_Min_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Min_Show_Rota;
                            }
                            else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Max_Show_Rota;
                            }
                            rotas[servo] = (byte)angle;
                            ErrorCode ret = mRobot.CtrlActionForDjId(servo, angle);
                            if (ret != ErrorCode.Result_OK)
                            {
                                RunCmdFinished(args[0]);
                            }
                        }
                    }
                    
                }
                else
                {
                    RunCmdFinished(args[0]);
                }
            }
        }
        catch (System.Exception ex)
        {
            RunCmdFinished(args[0]);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 控制舵机转动
    /// </summary>
    /// <param name="arg"></param>
    void LogicServoSet(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    Dictionary<byte, byte> rotas = new Dictionary<byte, byte>();
                    int time = -1;
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            if (-1 == time && dict.ContainsKey("ms"))
                            {
                                time = int.Parse(dict["ms"].ToString());
                            }
                            byte servo = byte.Parse(dict["servo"].ToString());
                            int angle = int.Parse(dict["degree"].ToString()) + PublicFunction.DuoJi_Start_Rota;
                            if (angle < PublicFunction.DuoJi_Min_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Min_Show_Rota;
                            }
                            else if (angle > PublicFunction.DuoJi_Max_Show_Rota)
                            {
                                angle = PublicFunction.DuoJi_Max_Show_Rota;
                            }
                            rotas[servo] = (byte)angle;
                        }
                        
                        
                    }
                    ErrorCode ret = mRobot.CtrlServoMove(rotas, time);
                    if (ret == ErrorCode.Result_OK)
                    {
                        mServoSetCallBackFlag = false;
                        mServoSetFinishedFlag = false;
                        mWaitServoSetIndex = Timer.Add(time / 1000.0f, 1, 1, ServoSetFinished);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
            
            
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 控制舵机轮模式
    /// </summary>
    /// <param name="args"></param>
    void LogicServoSetbySpeed(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    Dictionary<byte, TurnData> rotas = new Dictionary<byte, TurnData>();
                    int[] speedArray = new int[] { 0x0080, 0x00EA, 0x0154, 0x020E, 0x0292 };
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            TurnData turnData = new TurnData();
                            byte servo = byte.Parse(dict["servo"].ToString());
                            string speedStr = string.Empty;
                            if (dict.ContainsKey("speed"))
                            {
                                speedStr = dict["speed"].ToString();
                            }
                            int speed = 0;
                            switch (speedStr)
                            {
                                case "VS":
                                    speed = 0;
                                    break;
                                case "S":
                                    speed = 1;
                                    break;
                                case "M":
                                    speed = 2;
                                    break;
                                case "F":
                                    speed = 3;
                                    break;
                                case "VF":
                                    speed = 4;
                                    break;
                            }
                            speed = speedArray[speed];
                            int tmp = int.Parse(dict["direction"].ToString());
                            if (tmp == 1)
                            {
                                turnData.turnDirection = TurnDirection.turnByClock;
                            }
                            else if (tmp == 2)
                            {
                                turnData.turnDirection = TurnDirection.turnByDisclock;
                            }
                            else
                            {
                                turnData.turnDirection = TurnDirection.turnStop;
                            }
                            turnData.turnSpeed = (ushort)speed;
                            rotas[servo] = turnData;
                        }
                    }
                    if (rotas.Count > 0)
                    {
                        mRobot.CtrlServoTurn(rotas);
                    }
                    CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }


        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 停止舵机轮模式
    /// </summary>
    /// <param name="args"></param>
    void LogicStopServo(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    Dictionary<byte, TurnData> rotas = new Dictionary<byte, TurnData>();
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            TurnData turnData = new TurnData();
                            byte servo = byte.Parse(dict["servo"].ToString());
                            turnData.turnDirection = TurnDirection.turnStop;
                            turnData.turnSpeed = 0;
                            rotas[servo] = turnData;
                        }
                    }
                    mRobot.CtrlServoTurn(rotas);
                    CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }


        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 回读
    /// </summary>
    /// <param name="args"></param>
    void LogicGetPosture(string[] args)
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                bool isPowerFlag = false;
                List<byte> angleList = mRobot.GetAllDjData().GetAngleList();
                for (int i = 0, imax = angleList.Count; i < imax; ++i)
                {
                    DuoJiData data = mRobot.GetAnDjData(angleList[i]);
                    if (null != data && data.modelType == ServoModel.Servo_Model_Angle && data.isPowerDown)
                    {
                        isPowerFlag = true;
                        break;
                    }
                }
                if (isPowerFlag)
                {
                    mRobot.ReadBack(ExtendCMDCode.LogicGetPosture);
                }
                else
                {
                    Action ac = new Action();
                    mRobot.GetNowAction(ac);
                    GetPostureCallBack(CallUnityResult.success, ac);
                }
                /*Action ac = new Action();
                ac.UpdateRota(1, 20);
                ac.UpdateRota(2, 30);
                GetPostureCallBack(CallUnityResult.success, ac);*/
            }
            else
            {
                CmdCallBack(args[0], CallUnityResult.failure);
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 停止机器人
    /// </summary>
    /// <param name="args"></param>
    void LogicStopRobot(string[] args)
    {
        try
        {
            if (null != mRuningCmdDict)
            {
                mRuningCmdDict.Clear();
            }
            if (null != mWaitCmdDict)
            {
                mWaitCmdDict.Clear();
            }
            NetWork.GetInst().ClearCacheMsg();
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                mRobot.StopNowPlayActions();
                //RunCmdFinished(args[0]);
                
            }
            /*else
            {
                RunCmdFinished(args[0]);
            }*/
        }
        catch (System.Exception ex)
        {
            //RunCmdFinished(args[0]);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicDisConnenct(string[] args)
    {
        try
        {
            PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedConnectBluetoothButton);
            PlatformMgr.Instance.DisConnenctBuletooth();
            mWaitExceptionRepairFlag = false;
            RunCmdFinished(args[0]);
        }
        catch (System.Exception ex)
        {
            RunCmdFinished(args[0]);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicQueryAllSensor(string[] args)
    {
        try
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {

                if (null != mRobot.GetReadSensorData(TopologyPartType.Gyro) && null != mRobot.GetReadSensorData(TopologyPartType.Gyro).ids && mRobot.GetReadSensorData(TopologyPartType.Gyro).ids.Count > 0)
                {
                    mNeedWaitSensorCallBack = 2;
                    mRobot.ReadAllSensorData();
                    mRobot.ReadSensorData(mRobot.GetReadSensorData(TopologyPartType.Gyro).ids, TopologyPartType.Gyro, true);
                }
                else
                {
                    mNeedWaitSensorCallBack = 1;
                    mRobot.ReadAllSensorData();
                }
            }
            else
            {
                CmdCallBack(args[0], CallUnityResult.failure);
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 查询红外数据
    /// </summary>
    /// <param name="args"></param>
    void LogicQueryInfrared(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    if (null != mRobot.GetReadSensorData(TopologyPartType.Infrared) && ids.Count > 0)
                    {
                        mRobot.ReadSensorData(ids, TopologyPartType.Infrared, false);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 查询触碰数据
    /// </summary>
    /// <param name="args"></param>
    void LogicQueryTouchStatue(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    if (null != mRobot.GetReadSensorData(TopologyPartType.Touch) && ids.Count > 0)
                    {
                        mRobot.ReadSensorData(ids, TopologyPartType.Touch, false);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 查询陀螺仪数据
    /// </summary>
    /// <param name="args"></param>
    void LogicQueryGyroscope(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    if (null != mRobot.GetReadSensorData(TopologyPartType.Gyro) && ids.Count > 0)
                    {
                        mRobot.ReadSensorData(ids, TopologyPartType.Gyro, false);
                    }
                    else
                    {
                        CmdCallBack(args[0], CallUnityResult.failure);
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置Led数据
    /// </summary>
    /// <param name="args"></param>
    void LogicSetLEDs(string[] args)
    {
        try
        {
            if (args.Length < 3)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetLEDCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    UInt16 duration = UInt16.Parse(args[2]);
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            byte id = 0;
                            if (dict.ContainsKey("id"))
                            {
                                id = byte.Parse(dict["id"].ToString());
                            }
                            if (dict.ContainsKey("lights"))
                            {
                                Dictionary<string, List<byte>> showColor = new Dictionary<string, List<byte>>();
                                List<object> lightsData = (List < object >) dict["lights"];
                                for (int lightIndex = 0, lightMax = lightsData.Count; lightIndex < lightMax; ++lightIndex)
                                {
                                    string color = lightsData[lightIndex].ToString();
                                    //if (!string.IsNullOrEmpty(color))
                                    {
                                        if (!showColor.ContainsKey(color))
                                        {
                                            List<byte> list = new List<byte>();
                                            showColor[color] = list;
                                        }
                                        showColor[color].Add((byte)(lightIndex + 1));
                                    }
                                }
                                if (0 != id && showColor.Count > 0)
                                {
                                    List<byte> ids = new List<byte>();
                                    ids.Add(id);
                                    List<LightShowData> showData = new List<LightShowData>();
                                    foreach (var kvp in showColor)
                                    {
                                        LightShowData light = new LightShowData();
                                        light.ids = kvp.Value;
                                        light.rgb = kvp.Key;
                                        showData.Add(light);
                                    }
                                    ++mSetLEDCount;
                                    mRobot.SendLight(ids, showData, duration);
                                }
                            }
                        }
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 设置表情数据
    /// </summary>
    /// <param name="args"></param>
    void LogicSetEmoji(string[] args)
    {
        try
        {
            if (args.Length < 3)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetEmojiCount = 0;
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    UInt16 times = UInt16.Parse(args[2]);
                    //UInt16 duration = UInt16.Parse(args[3]);
                    Dictionary<byte, Dictionary<string, List<byte>>> emojiDict = new Dictionary<byte, Dictionary<string, List<byte>>>();
                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            byte id = 0;
                            byte lightType = 0;
                            string color = string.Empty;
                            if (dict.ContainsKey("id"))
                            {
                                id = byte.Parse(dict["id"].ToString());
                            }
                            if (dict.ContainsKey("emotionIndex"))
                            {
                                lightType = (byte)byte.Parse(dict["emotionIndex"].ToString());
                            }
                            if (dict.ContainsKey("color"))
                            {
                                color = dict["color"].ToString();
                            }
                            if (!emojiDict.ContainsKey(lightType))
                            {
                                Dictionary<string, List<byte>> colorDict = new Dictionary<string, List<byte>>();
                                emojiDict[lightType] = colorDict;
                            }
                            if (!emojiDict[lightType].ContainsKey(color))
                            {
                                List<byte> ids = new List<byte>();
                                emojiDict[lightType][color] = ids;
                            }
                            emojiDict[lightType][color].Add(id);
                        }
                    }
                    foreach (var kvp in emojiDict)
                    {
                        foreach (var tmp in kvp.Value)
                        {
                            ++mSetEmojiCount;
                            mRobot.SendEmoji(tmp.Value, kvp.Key, tmp.Key, times);
                        }
                    }
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    /// <summary>
    /// 设置数码管
    /// </summary>
    /// <param name="args">setDigitalTube|id1,2|控制类型0|需显示的数字位数1,2,3,4|需要显示的点的位数1,2,3,4|是否显示冒号0|是否是负数0|闪烁的次数10|闪烁或数值变化的频率300|起始值0|结束值1</param>
    void LogicSetDigitalTube(string[] args)
    {
        try
        {
            if (args.Length < 10)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mSetDigitalTubeCount = 0;
                    List<byte> ids = PublicFunction.StringToByteList(args[1]);
                    byte controlType = byte.Parse(args[2]);
                    List<byte> showNum = PublicFunction.StringToByteList(args[3]);
                    List<byte> showSubPoint = PublicFunction.StringToByteList(args[4]);
                    bool showColon = args[5].Equals("1");
                    bool isNegativeNum = args[6].Equals("1");
                    byte flickerTimes = byte.Parse(args[7]);
                    UInt32 flickerTimeout = UInt32.Parse(args[8]);
                    UInt32 startValue = UInt32.Parse(args[9]);
                    UInt32 endValue = UInt32.Parse(args[10]);
                    ++mSetDigitalTubeCount;
                    mRobot.SendDigitalTube(ids, controlType, showNum, showSubPoint, showColon, isNegativeNum, flickerTimes, flickerTimeout, startValue, endValue);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void LogicCtrlSensorLED(string[] args)
    {
        try
        {
            if (args.Length < 2)
            {//参数错误
                ParameterError(args[0], CallUnityErrorCode.Result_None);
            }
            else
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    JsonData data = new JsonData(Json.Deserialize(args[1]));
                    TopologyPartType partType = TopologyPartType.Infrared;
                    byte id = 0;
                    CtrlSensorLEDMsg.ControlType controlType = CtrlSensorLEDMsg.ControlType.Single_Flash;
                    UInt16 duration = 300;
                    byte times = 0;
                    Dictionary<string, object> dict = (Dictionary<string, object>)data.Dictionary;
                    if (dict.ContainsKey("sensorType"))
                    {
                        partType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), dict["sensorType"].ToString());
                    }
                    if (dict.ContainsKey("id"))
                    {
                        id = byte.Parse(dict["id"].ToString());
                    }
                    if (dict.ContainsKey("controlType"))
                    {
                        controlType = (CtrlSensorLEDMsg.ControlType)byte.Parse(dict["controlType"].ToString());
                    }
                    if (dict.ContainsKey("duration"))
                    {
                        duration = UInt16.Parse(dict["duration"].ToString());
                    }
                    if (dict.ContainsKey("times"))
                    {
                        times = byte.Parse(dict["times"].ToString());
                    }

                    mRobot.CtrlSensorLED(id, partType, controlType, duration, times);
                    CmdCallBack(args[0], CallUnityResult.success);
                }
                else
                {
                    CmdCallBack(args[0], CallUnityResult.failure);
                }
            }
        }
        catch (System.Exception ex)
        {
            CatchException(args[0], CallUnityErrorCode.Result_None);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    #endregion

    #region 各种命令返回结果
    public void PlayActionCallBack(CallUnityResult result)
    {
        if (IsLogicProgramming)
        {
            CmdCallBack(LogicCmd.action.ToString(), result);
        }
    }

    public void AdjustServoCallBack()
    {
        if (IsLogicProgramming)
        {
            RunCmdFinished(LogicCmd.adjustServo.ToString());
        }
    }

    public void ServoSetCallBack(CallUnityResult result)
    {
        if (IsLogicProgramming)
        {
            mServoSetCallBackFlag = true;
            if (result == CallUnityResult.success)
            {
                if (mServoSetFinishedFlag)
                {
                    CmdCallBack(LogicCmd.servoSet.ToString(), result);
                }
            }
            else
            {
                if (-1 != mWaitServoSetIndex)
                {
                    Timer.Cancel(mWaitServoSetIndex);
                    mWaitServoSetIndex = -1;
                }
                CmdCallBack(LogicCmd.servoSet.ToString(), result);
            }
        }
        else
        {
            if (-1 != mWaitServoSetIndex)
            {
                Timer.Cancel(mWaitServoSetIndex);
                mWaitServoSetIndex = -1;
            }
        }
        
    }
    /// <summary>
    /// 回读返回
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    public void GetPostureCallBack(CallUnityResult result, Action action)
    {
        if (IsLogicProgramming)
        {
            if (result == CallUnityResult.success && null != action)
            {
                List<Dictionary<string, int>> list = new List<Dictionary<string, int>>();
                List<byte> angleList = mRobot.GetAllDjData().GetAngleList();
                for (int i = 0, imax = angleList.Count; i < imax; ++i)
                {
                    Dictionary<string, int> dict = new Dictionary<string, int>();
                    dict["servo"] = angleList[i];
                    dict["degree"] = action.GetRota(angleList[i]) - PublicFunction.DuoJi_Start_Rota;
                    list.Add(dict);
                }
                string jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + Json.Serialize(list);
                CmdMoreCallBack(LogicCmd.getPosture.ToString(), jsonbill);
            }
            else
            {
                CmdCallBack(LogicCmd.getPosture.ToString(), CallUnityResult.failure);
            }
            
        }
    }

    public void ServoPowerOnCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdCallBack(LogicCmd.servoPowerOn.ToString(), result);
        }
    }

    public void ServoPowerOffCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            CmdCallBack(LogicCmd.servoPowerOff.ToString(), result);
        }
    }

    public void QueryAllSensorCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mNeedWaitSensorCallBack;
            if (mNeedWaitSensorCallBack == 0)
            {
                string jsonbill = string.Empty;
                TopologyPartType[] sensorTypes = PublicFunction.Read_All_Sensor_Type;
                Dictionary<string, object> dict = new Dictionary<string, object>();
                for (int i = 0, imax = sensorTypes.Length; i < imax; ++i)
                {
                    if (null != mRobot.GetReadSensorData(sensorTypes[i]))
                    {
                        dict[sensorTypes[i].ToString()] = mRobot.GetReadSensorData(sensorTypes[i]).GetReadAllResult();
                    }
                }
                if (null != mRobot.GetReadSensorData(TopologyPartType.Gyro))
                {
                    dict[TopologyPartType.Gyro.ToString()] = mRobot.GetReadSensorData(TopologyPartType.Gyro).GetReadAllResult();
                }
                jsonbill = result.ToString() + PublicFunction.Separator_Or + Json.Serialize(dict);
                CmdMoreCallBack(LogicCmd.queryAllSensor.ToString(), jsonbill);
            }
        }
    }

    /// <summary>
    /// 查询红外返回
    /// </summary>
    /// <param name="result"></param>
    public void QueryInfraredCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            if (result == CallUnityResult.success && null != mRobot.GetReadSensorData(TopologyPartType.Infrared))
            {
                jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + mRobot.GetReadSensorData(TopologyPartType.Infrared).GetReadResult();
            }
            else
            {
                jsonbill = CallUnityResult.failure.ToString();
            }
            CmdMoreCallBack(LogicCmd.queryInfrared.ToString(), jsonbill);
        }
    }
    /// <summary>
    /// 查询触碰返回
    /// </summary>
    /// <param name="result"></param>
    public void QueryTouchStatusCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            if (result == CallUnityResult.success && null != mRobot.GetReadSensorData(TopologyPartType.Touch))
            {
                jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + mRobot.GetReadSensorData(TopologyPartType.Touch).GetReadResult();
            }
            else
            {
                jsonbill = CallUnityResult.failure.ToString();
            }
            CmdMoreCallBack(LogicCmd.queryTouchStatus.ToString(), jsonbill);
        }
    }
    /// <summary>
    /// 查询陀螺仪返回
    /// </summary>
    /// <param name="result"></param>
    public void QueryGyroscopeCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            string jsonbill = string.Empty;
            if (result == CallUnityResult.success && null != mRobot.GetReadSensorData(TopologyPartType.Gyro))
            {
                jsonbill = CallUnityResult.success.ToString() + PublicFunction.Separator_Or + mRobot.GetReadSensorData(TopologyPartType.Gyro).GetReadResult();
            }
            else
            {
                jsonbill = CallUnityResult.failure.ToString();
            }
            CmdMoreCallBack(LogicCmd.queryGyroscope.ToString(), jsonbill);
        }
    }

    public void SetLEDsCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetLEDCount;
            if (mSetLEDCount <= 0)
            {
                CmdMoreCallBack(LogicCmd.setLEDs.ToString(), result.ToString());
            }
        }
    }

    public void SetEmojiCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetEmojiCount;
            if (mSetEmojiCount <= 0)
            {
                CmdMoreCallBack(LogicCmd.setEmoji.ToString(), result.ToString());
            }
        }
    }

    public void SetDigitalTubeCallBack(CallUnityResult result)
    {
        if (isLogicProgramming)
        {
            --mSetDigitalTubeCount;
            if (mSetDigitalTubeCount <= 0)
            {
                CmdMoreCallBack(LogicCmd.setDigitalTube.ToString(), result.ToString());
            }
        }
    }

    public void ExceptionRepairResult()
    {
        if (mWaitExceptionRepairFlag)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.JsExceptionWaitResult, string.Empty);
            mWaitExceptionRepairFlag = false;
        }
    }
#endregion
    /// <summary>
    /// 执行完了某条命令
    /// </summary>
    /// <param name="cmd"></param>
    void RunCmdFinished(string cmd)
    {
        if (null == mRuningCmdDict)
        {
            return;
        }
        mRuningCmdDict.Remove(cmd);
        if (!mWaitExceptionRepairFlag && null != mWaitCmdDict && mWaitCmdDict.ContainsKey(cmd))
        {
            mRuningCmdDict[cmd] = mWaitCmdDict[cmd].args;
            mWaitCmdDict.Remove(cmd);
            mCallCmdDict[cmd](mRuningCmdDict[cmd]);
        }
    }
    /// <summary>
    /// 参数错误
    /// </summary>
    /// <param name="cmd"></param>
    void ParameterError(string cmd, CallUnityErrorCode errorCode)
    {
        CmdCallBack(cmd, CallUnityResult.failure, errorCode);
    }
    /// <summary>
    /// 出现异常
    /// </summary>
    /// <param name="cmd"></param>
    void CatchException(string cmd, CallUnityErrorCode errorCode)
    {
        CmdCallBack(cmd, CallUnityResult.failure, errorCode);
    }

    void CmdCallBack(string cmd, CallUnityResult result, CallUnityErrorCode errorCode = CallUnityErrorCode.Result_None)
    {
        CmdMoreCallBack(cmd, result.ToString(), errorCode);
    }

    void CmdMoreCallBack(string cmd, string result, CallUnityErrorCode errorCode = CallUnityErrorCode.Result_None)
    {
        if (null == mRuningCmdDict || !mRuningCmdDict.ContainsKey(cmd))
        {
            return;
        }
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (CallUnityErrorCode.Result_None != errorCode)
        {
            dict["result"] = ((byte)errorCode).ToString() + PublicFunction.Separator_Or + result;
        }
        else
        {
            dict["result"] = result;
        }
        dict["cmd"] = CmdArgsToString(mRuningCmdDict[cmd]).ToString();
        string jsonbill = Json.Serialize(dict);
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.LogicCMDResult, jsonbill);
        if (null != logicCmdCallBack)
        {
            logicCmdCallBack(jsonbill);
        }
        RunCmdFinished(cmd);
    }

    string CmdArgsToString(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0, imax = args.Length; i < imax; ++i)
        {
            if (sb.Length == 0)
            {
                sb.Append(Logic_Cmd_Start);

            }
            else
            {
                sb.Append(PublicFunction.Separator_Or);
            }
            sb.Append(args[i]);
        }
        return sb.ToString();
    }

    Dictionary<string, string> StringToDict(string str)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        try
        {
            str = str.TrimStart('{').TrimEnd('}');
            string[] args = str.Split(',');
            for (int i = 0, imax = args.Length; i < imax; ++i)
            {
                string[] tmpArg = args[i].Split(':');
                if (2 == tmpArg.Length)
                {
                    dict[tmpArg[0]] = tmpArg[1];
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return dict;
    }


    //////////////////////////////////////////////////////////////////////////

    void ServoSetFinished()
    {
        mServoSetFinishedFlag = true;
        mWaitServoSetIndex = -1;
        if (mServoSetCallBackFlag)
        {
            CmdCallBack(LogicCmd.servoSet.ToString(), CallUnityResult.success);
        }
    }
}

/// <summary>
/// 逻辑编程命令
/// </summary>
public enum LogicCmd : byte
{
    query,
    action,
    servoSet,
    servoSetbySpeed,
    servoPowerOn,
    servoPowerOff,
    getPosture,
    stopServo,
    adjustServo,
    stopRobot,
    DisconnectBLE,
    queryAllSensor,
    queryInfrared,
    queryTouchStatus,
    queryGyroscope,
    setLEDs,
    setEmoji,
    setDigitalTube,
    setSensorLED,
}

/// <summary>
/// 逻辑编程调用unity接口事件结果返回
/// </summary>
public enum CallUnityResult : byte
{
    lowPower = 0,
    highPower,
    powerOn,
    powerOff,
    success,
    failure,
}
/// <summary>
/// 逻辑编程调用Unity接口错误码
/// </summary>
public enum CallUnityErrorCode : byte
{
    Result_None = 0,
    Result_OK = 1,
    Blue_DisConnect = 2,
    Actions_Not_Exist = 3,
}
/// <summary>
/// 提示颜色
/// </summary>
public enum CommonTipsColor : byte
{
    yellow = 0,
    green = 1,
    red = 2,
}
