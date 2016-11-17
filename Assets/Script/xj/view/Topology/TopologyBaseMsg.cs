using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Event;
using Game;
/// <summary>
/// Author:xj
/// FileName:TopologyBaseMsg.cs
/// Description:拓扑图基类
/// Time:2016/7/11 15:09:09
/// </summary>
public class TopologyBaseMsg : BasePopWin
{
    public enum TopologyMsgType
    {
        Topology_Confirm,//确认界面
        Topology_Setting,//拓扑图设置界面
        Topology_ShowInfo,//显示信息
    }

    static TopologyBaseMsg mInst;

    protected ReadMotherboardDataMsgAck mMainBoardData;
    protected Robot mRobot;
    List<byte> mNeedUpdateServoList;
    bool isUpdating;//正在升级
    bool isSuccess;//模型匹配成功了
    bool isUpdateSuccess;//升级成功了
    bool isConnecting;//是否处于连接流程中
    ErrorCode mSystemUpdateResult;//主板更新结果
    ErrorCode mServoUpdateResult;//舵机更新结果
    ErrorCode mCompareResult;//比较模型的结果
    bool mCheckUpdateFlag;//检查升级的时候检测到升级是否升级

    protected UIInput mNameInput;
    Vector3 mBottomBtnTargetPos;
    TweenPosition mBottomTweenPosition;

    Transform mReDetectBtnTrans;
    Transform mConfirmBtnTrans;

    Transform mConfirmTitleTrans;
    Transform mSettingTitleTrans;

    Transform mConfirmBottomTrans;
    Transform mSettingBottomTrans;
    Transform mShowInfoBottomTrans;

    Transform mBtnRefreshTrans;
    Transform mBtnFinishedTrans;
    Transform mBtnDeviceTrans;
    Transform mBtnSettingTrans;
    Transform mBtnHelpTrans;

    TopologyUI mTopologyUI;

    TopologyMsgType mMsgType = TopologyMsgType.Topology_Confirm;

    float mUpdateTime = 0;




    public TopologyBaseMsg(TopologyMsgType msgType)
    {
        mUIResPath = "Prefab/UI/TopologyBase";
        mTopologyUI = new TopologyUI(eUICameraType.OrthographicTwo);
        isSingle = true;
        isCoverAddPanel = true;
        mNeedUpdateServoList = new List<byte>();
        isUpdating = false;
        isSuccess = false;
        mCheckUpdateFlag = false;
        mInst = this;
        isUpdateSuccess = false;
        mMsgType = msgType;
        if (msgType == TopologyMsgType.Topology_Confirm)
        {
            isConnecting = true;
        }
        else
        {
            isConnecting = false;
        }
    }

    public static void ShowMsg(ReadMotherboardDataMsgAck data)
    {
        if (null == mInst)
        {
            object[] args = new object[1];
            args[0] = TopologyMsgType.Topology_Confirm;
            PopWinManager.GetInst().ShowPopWin(typeof(TopologyBaseMsg), args, 0.95f);
        }
        else
        {
            mInst.mMainBoardData = data;
            if (TopologyMsgType.Topology_Confirm == mInst.mMsgType)
            {
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    mInst.mTopologyUI.RefreshIndependent();
                }
                mInst.CheckModelData();
                if (mInst.isUpdateSuccess && mInst.isSuccess)
                {
                    mInst.ConfirmFinished();
                }
            }
        }
    }
    /// <summary>
    /// 显示拓扑图详情页面
    /// </summary>
    public static void ShowInfoMsg()
    {
        object[] args = new object[1];
        args[0] = TopologyMsgType.Topology_ShowInfo;
        TopologyBaseMsg msg = (TopologyBaseMsg)PopWinManager.GetInst().ShowPopWin(typeof(TopologyBaseMsg), args, 0.95f);
        msg.HideOldMsg(TopologyMsgType.Topology_Confirm, TopologyMsgType.Topology_ShowInfo);
        msg.ShowNewMsg(TopologyMsgType.Topology_Confirm, TopologyMsgType.Topology_ShowInfo);
        msg.mMsgType = TopologyMsgType.Topology_ShowInfo;
    }


    protected override void AddEvent()
    {
        try
        {
            base.AddEvent();
            EventMgr.Inst.Regist(EventID.Update_Finished, UpdateFinishedCallBack);
            EventMgr.Inst.Regist(EventID.Update_Error, UpdateErrorCallBack);
            EventMgr.Inst.Regist(EventID.Update_Fail, UpdateFailCallBack);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
            EventMgr.Inst.Regist(EventID.Update_Progress, UpdateProgressResult);
            EventMgr.Inst.Regist(EventID.Read_Speaker_Data_Ack, ReadSpeakerCallBack);
            mTopologyUI.SetDepth(mDepth);
            mTopologyUI.Open();
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                mRobot = RobotManager.GetInst().GetCreateRobot();
            }
            else
            {
                mRobot = RobotManager.GetInst().GetCurrentRobot();
            }
            if (null != mRobot)
            {
                mMainBoardData = mRobot.MotherboardData;
                /*if (null == mMainBoardData)
                {
                    mMainBoardData = new ReadMotherboardDataMsgAck();
                    mMainBoardData.ids.AddRange(mRobot.GetAllDjData().GetIDList());
                }*/
            }
            if (null != mTrans)
            {
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    Transform btnBack = top.Find("btnBack");
                    if (null != btnBack)
                    {
                        TweenPosition backBtnTweenPosition = btnBack.GetComponent<TweenPosition>();
                        if (null != backBtnTweenPosition)
                        {
                            Vector3 pos = UIManager.GetWinPos(btnBack, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                            btnBack.localPosition = pos - new Vector3(300, 0);
                            GameHelper.PlayTweenPosition(backBtnTweenPosition, pos, 0.6f);
                        }
                        else
                        {
                            btnBack.localPosition = UIManager.GetWinPos(btnBack, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        }
                    }

                    Transform btnDevice = top.Find("btnDevice");
                    if (null != btnDevice)
                    {
                        mBtnDeviceTrans = btnDevice;
                        mBtnDeviceTrans.localPosition = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y) + new Vector3(300, 0);
                        mBtnDeviceTrans.gameObject.SetActive(false);
                    }

                    Transform btnFinished = top.Find("btnFinished");
                    if (null != btnFinished)
                    {
                        mBtnFinishedTrans = btnFinished;
                        btnFinished.localPosition = UIManager.GetWinPos(btnFinished, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y) + new Vector3(300, 0);
                        btnFinished.gameObject.SetActive(false);
                    }

                    Transform btnSetting = top.Find("btnSetting");
                    if (null != btnSetting)
                    {
                        mBtnSettingTrans = btnSetting;
                        mBtnSettingTrans.localPosition = UIManager.GetWinPos(mBtnSettingTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y) + new Vector3(300, 0);
                        mBtnSettingTrans.gameObject.SetActive(false);
                    }
                    Transform btnHelp = top.Find("btnHelp");
                    if (null != btnHelp)
                    {
                        mBtnHelpTrans = btnHelp;
                        mBtnHelpTrans.localPosition = UIManager.GetWinPos(mBtnHelpTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y) + new Vector3(300, 0);
                        mBtnHelpTrans.gameObject.SetActive(false);
                    }
                    Transform title = top.Find("title");
                    if (null != title)
                    {
                        title.localPosition = UIManager.GetWinPos(title, UIWidget.Pivot.Top, 0, 60);
                        mConfirmTitleTrans = title.Find("normal");
                        if (null != mConfirmTitleTrans)
                        {
                            mNameInput = GameHelper.FindChildComponent<UIInput>(mConfirmTitleTrans, "Input");
                            if (null != mNameInput)
                            {
                                mNameInput.defaultText = LauguageTool.GetIns().GetText("请输入名字");
                                string mainName = PlatformMgr.Instance.GetNameForMac(PlatformMgr.Instance.GetRobotConnectedMac(mRobot.ID));// PlatformMgr.Instance.GetMotherboardName();
                                if (LauguageTool.IsArab(mainName))
                                {
                                    mNameInput.value = LauguageTool.ConvertArab(mainName);
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(mainName))
                                    {
                                        mNameInput.value = " ";
                                        BoxCollider box = mNameInput.GetComponent<BoxCollider>();
                                        if (null != box)
                                        {
                                            box.enabled = false;
                                        }
                                    }
                                    else
                                    {
                                        mNameInput.value = mainName;
                                    }
                                }
                                //mNameInput.value = mNameInput.value.PadRight(10, ' ');
                                mNameInput.onSelect = OnInputSelect;
                                mNameInput.onValidate = OnValidate;
                            }
                        }
                        mSettingTitleTrans = title.Find("setting");
                        if (null != mSettingTitleTrans)
                        {
                            mSettingTitleTrans.localPosition = new Vector3(0, 300);
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(mSettingTitleTrans, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("设置拓扑图");
                            }
                        }
                    }
                }
                
                Transform bottombtn = mTrans.Find("bottom");
                if (null != bottombtn)
                {
                    mBottomBtnTargetPos = UIManager.GetWinPos(bottombtn, UIWidget.Pivot.Bottom, 0, -2);
                    mBottomTweenPosition = bottombtn.GetComponent<TweenPosition>();
                    bottombtn.localPosition = mBottomBtnTargetPos - new Vector3(0, 300);

                    mConfirmBottomTrans = bottombtn.Find("confirm");
                    if (null != mConfirmBottomTrans)
                    {
                        mReDetectBtnTrans = mConfirmBottomTrans.Find("btnReDetect");
                        mConfirmBtnTrans = mConfirmBottomTrans.Find("btnConfirm");
                        if (null != mReDetectBtnTrans)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(mReDetectBtnTrans, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("重新检测");
                            }

                        }
                        if (null != mConfirmBtnTrans)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(mConfirmBtnTrans, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("确定");
                            }
                        }
                    }
                    mSettingBottomTrans = bottombtn.Find("setting");
                    if (null != mSettingBottomTrans)
                    {
                        mSettingBottomTrans.localPosition = new Vector3(0, -300);
                        /*Transform btnFinished = mSettingBottomTrans.Find("btnFinished");
                        if (null != btnFinished)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(mSettingBottomTrans, "Label");
                            if (null != lb)
                            {
                            }
                            SetButtonTransformData(new Transform[] { btnFinished });
                        }*/
                    }
                    mShowInfoBottomTrans = bottombtn.Find("showInfo");
                    if (null != mShowInfoBottomTrans)
                    {
                        mShowInfoBottomTrans.localPosition = new Vector3(0, -300);
                        //Transform btnDevice = mShowInfoBottomTrans.Find("btnDevice");
                        Transform btnDisconnect = mShowInfoBottomTrans.Find("btnDisconnect");
                        //Transform btnSetting = mShowInfoBottomTrans.Find("btnSetting");

                        if (null != btnDisconnect)
                        {
                            SetButtonTransformData(new Transform[] { btnDisconnect });
                        }
                        /*UILabel devicelb = GameHelper.FindChildComponent<UILabel>(btnDevice, "Label");
                        if (null != devicelb)
                        {
                            devicelb.text = LauguageTool.GetIns().GetText("硬件信息");
                        }*/
                        SetConnectLabel(PlatformMgr.Instance.GetBluetoothState());
                        /*UILabel settinglb = GameHelper.FindChildComponent<UILabel>(btnSetting, "Label");
                        if (null != settinglb)
                        {
                            settinglb.text = LauguageTool.GetIns().GetText("设置拓扑图");
                        }*/
                    }

                    Transform btnRefresh = mTrans.Find("btnRefresh");
                    if (null != btnRefresh)
                    {
                        mBtnRefreshTrans = btnRefresh;
                        TweenPosition btnRefreshTweenPosition = btnRefresh.GetComponent<TweenPosition>();
                        //Vector2 bottomSize = NGUIMath.CalculateRelativeWidgetBounds(bottombtn).size;
                        Vector3 pos = UIManager.GetWinPos(btnRefresh, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, 100);
                        if (null != btnRefreshTweenPosition)
                        {
                            btnRefresh.localPosition = pos - new Vector3(0, 300);
                            GameHelper.PlayTweenPosition(btnRefreshTweenPosition, pos, 0.6f);
                        }
                        else
                        {
                            btnRefresh.localPosition = pos;
                        }
                    }

                }

            }
            
            ShowBottomBtn();
            if (mMsgType == TopologyMsgType.Topology_Confirm)
            {
                CheckModelData();
                mTopologyUI.SetOnClickDelegate(ConfirmTopologyUIOnClick);
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

    public override void Release()
    {
        base.Release();
        mTopologyUI.OnClose();
        EventMgr.Inst.UnRegist(EventID.Update_Finished, UpdateFinishedCallBack);
        EventMgr.Inst.UnRegist(EventID.Update_Error, UpdateErrorCallBack);
        EventMgr.Inst.UnRegist(EventID.Update_Fail, UpdateFailCallBack);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
        EventMgr.Inst.UnRegist(EventID.Update_Progress, UpdateProgressResult);
        EventMgr.Inst.UnRegist(EventID.Read_Speaker_Data_Ack, ReadSpeakerCallBack);
        mInst = null;
        if (isConnecting)
        {
            if (isSuccess)
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(true));
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
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
                        robot.ReadConnectedAngle();
                        if (PlatformMgr.Instance.IsChargeProtected)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                            EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
                        }
                    }
                    else
                    {
                        SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                    }
                }
                else
                {
                    SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                }
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionSucceeded);
                /*if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
                {
                    PowerMsg.OpenPower();
                }*/
            }
            else
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                PlatformMgr.Instance.DisConnenctBuletooth();
                SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (isUpdating)
        {
            if (Time.time - mUpdateTime > 30.0f)
            {
                isUpdating = false;
                NetWaitMsg.ShowWait(1);
                if (mServoUpdateResult == ErrorCode.Result_OK)
                {
                    EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(false));
                }
                else
                {
                    EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                }
            }
        }
    }

    protected void ShowBottomBtn()
    {
        GameHelper.PlayTweenPosition(mBottomTweenPosition, mBottomBtnTargetPos, 0.6f);
    }


    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            string name = obj.name;
            if (name.Equals("btnRefresh"))
            {
                mTopologyUI.HideChoicePartPanel(false);
                mTopologyUI.ResetTopology();
            }
            else if (name.Equals("btnBack"))
            {//返回
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                }
                else
                {
                    if (mMsgType == TopologyMsgType.Topology_Setting)
                    {
                        //mTopologyUI.HideChoicePartPanel(true);
                        mTopologyUI.RecoverTopology();
                        //SetMsgType(TopologyMsgType.Topology_ShowInfo);
                        mTopologyUI.SaveTopologyData();
                        if (!isConnecting)
                        {
                            EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
                        }
                        OnClose();
                    }
                    else if (mMsgType == TopologyMsgType.Topology_Confirm)
                    {
                        PlatformMgr.Instance.DisConnenctBuletooth();
                        OnClose();
                    }
                    else
                    {
                        OnClose();
                        SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                    }
                }
            }
            else if (name.Equals("btnReDetect"))
            {//重新获取主板信息
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                    return;
                }
                if (null != mRobot)
                {
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        mRobot.RetrieveMotherboardData();
                    }
                    else
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                    }
                }
            }
            else if (name.Equals("btnConfirm"))
            {
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                    return;
                }
                PlatformMgr.Instance.SaveLastConnectedData(mRobot.Mac);
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    if (ClientMain.Use_Third_App_Flag || ClientMain.Simulation_Use_Third_App_Flag)
                    {
                        SingletonBehaviour<GetRobotData>.Inst.CreateGO(mRobot.MotherboardData.ids, mRobot.Name);
                    }
                    else
                    {
                        AssembleMenu._instance.CreateGO(mRobot.MotherboardData.ids);
                    }

                    EventMgr.Inst.Fire(EventID.Set_Choice_Robot);
                    RobotManager.GetInst().IsCreateRobotFlag = false;
                    mRobot = RobotManager.GetInst().GetCurrentRobot();
                    mTopologyUI.UpdateRobot(mRobot);
                }
                PlatformMgr.Instance.SaveRobotLastConnectedData(mRobot.ID, mRobot.Mac);
                if (isSuccess)
                {
                    ConfirmFinished();
                }
                else
                {
                    CheckUpdateData(true);
                }
            }
            else if (name.Equals("btnFinished"))
            {//拓扑图设置完成
                if (mTopologyUI.IsSettingFinished())
                {//设置完成
                    //mTopologyUI.HideChoicePartPanel(true);
                    mTopologyUI.SaveTopologyData();
                    if (!isConnecting)
                    {
                        EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
                    }
                    OnClose();
                    //SetMsgType(TopologyMsgType.Topology_ShowInfo);
                }
                else
                {
                    mTopologyUI.HideChoicePartPanel(false);
                    mTopologyUI.RemoveDisContinuousPart();
                }
            }
            else if (name.Equals("btnDevice"))
            {//显示设备硬件信息
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(DeviceMsg));
                }
                else
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("QingLianJieSheBei"));
                }
            }
            else if (name.Equals("btnDisconnect"))
            {//断开蓝牙
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedConnectBluetoothButton);
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    PublicPrompt.ShowDisconnect();
                }
                else
                {
                    OnClose();
                    SearchBluetoothMsg.ShowMsg();
                }
            }
            else if (name.Equals("btnSetting"))
            {//设置拓扑图
                SetMsgType(TopologyMsgType.Topology_Setting);
            }
            else if (name.Equals("btnHelp"))
            {
                PopWinManager.GetInst().ShowPopWin(typeof(TopologyGuideMsg));
            }
            Debuger.Log(name);
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


    void ConfirmTopologyUIOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals("MotherBox"))
        {//点击了主板
            if (null != mMainBoardData && !PlatformMgr.Instance.Robot_System_Version.Equals(mMainBoardData.mbVersion))
            {
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                }
            }
        }
        else if (btnName.StartsWith("servo_"))
        {//点击了某个舵机
            byte id = byte.Parse(btnName.Substring("servo_".Length));
            if (null != mMainBoardData && mMainBoardData.errorIds.Contains(id))
            {//重复舵机id
                PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机ID重复，请修改舵机ID"), id), PopRetrieveMotherboardDataOnClick);
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
            else if (mNeedUpdateServoList.Contains(id))
            {//需要升级的舵机
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                }
            }
            else if (null != mMainBoardData && mMainBoardData.errorVerIds.Contains(id))
            {//舵机版本不一致
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                }
            }
            else if (null != mMainBoardData && !mMainBoardData.ids.Contains(id))
            {//舵机不存在，检查连接
                PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机连接异常"), id), PopRetrieveMotherboardDataOnClick);
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
            else if (null != mMainBoardData && !string.IsNullOrEmpty(PlatformMgr.Instance.Robot_Servo_Version) && !PlatformMgr.Instance.Robot_Servo_Version.Equals(mMainBoardData.djVersion))
            {
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                }
            }
        }
        else if (btnName.StartsWith("sensor_"))
        {//点中了传感器
            string[] arys = btnName.Split('_');
            if (arys.Length == 3)
            {
                try
                {
                    TopologyPartType partType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), arys[1]);
                    byte id = byte.Parse(arys[2]);
                    if (null != mMainBoardData)
                    {
                        SensorData data = mMainBoardData.GetSensorData(partType);
                        if (null == data)
                        {//传感器不存在
                            PromptMsg msg = PromptMsg.ShowDoublePrompt(GetSensorLinkTips(partType, id), PopRetrieveMotherboardDataOnClick);
                            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                        }
                        else
                        {
                            if (data.errorIds.Contains(id) || TopologyPartType.Speaker == partType && data.ids.Count > 1)
                            {//重复id,或者有多个蓝牙喇叭
                                PromptMsg msg = PromptMsg.ShowDoublePrompt(GetSensorRepeatTips(partType, id.ToString()), PopRetrieveMotherboardDataOnClick);
                                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                            }
                            else if (data.errorVerIds.Contains(id))
                            {//版本不一致
                               PromptMsg.ShowSinglePrompt(GetSensorVersionTips(partType, id));
                            }
                            else if (TopologyPartType.Speaker != partType && !data.ids.Contains(id) || TopologyPartType.Speaker == partType && data.ids.Count < 1)
                            {//传感器不存在，检查连接
                                PromptMsg msg = PromptMsg.ShowDoublePrompt(GetSensorLinkTips(partType, id), PopRetrieveMotherboardDataOnClick);
                                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                            }
                            else if (TopologyPartType.Speaker == partType)
                            {//喇叭
                                SpeakerData speakerData = (SpeakerData)mRobot.GetReadSensorData(TopologyPartType.Speaker);
                                if (null != speakerData)
                                {
                                    SpeakerInfoData infoData = speakerData.GetSpeakerData(id);
                                    if (null != infoData)
                                    {

#if UNITY_ANDROID
                                        if (!PlatformMgr.Instance.IsConnectedSpeaker(infoData.speakerMac))
                                        {
                                            PlatformMgr.Instance.ConnectSpeaker(infoData.speakerMac);
                                        }
#else
                                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("外设蓝牙需要通过手机系统设置进行连接"), infoData.speakerName), PopSpeakerOnClick);

#endif
                                    }
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
        }
    }

    /// <summary>
    /// 获取传感器id重复提示
    /// </summary>
    /// <param name="partType"></param>
    /// <returns></returns>
    string GetSensorRepeatTips(TopologyPartType partType, string id)
    {
        string tips = string.Empty;
        switch (partType)
        {
            case TopologyPartType.Infrared:
                tips = string.Format(LauguageTool.GetIns().GetText("红外传感器ID重复，请修改ID"), id);
                break;
            case TopologyPartType.Gyro:
                tips = string.Format(LauguageTool.GetIns().GetText("陀螺仪传感器ID重复，请修改ID"), id);
                break;
            case TopologyPartType.Touch:
                tips = string.Format(LauguageTool.GetIns().GetText("触碰传感器ID重复，请修改ID"), id);
                break;
            case TopologyPartType.Light:
                tips = string.Format(LauguageTool.GetIns().GetText("灯光传感器ID重复，请修改ID"), id);
                break;
            case TopologyPartType.DigitalTube:
                tips = string.Format(LauguageTool.GetIns().GetText("数码管传感器ID重复，请修改ID"), id);
                break;
            case TopologyPartType.Speaker:
                tips = LauguageTool.GetIns().GetText("只能接一个蓝牙喇叭");
                break;
        }
        return tips;
    }
    /// <summary>
    /// 获取传感器连接问题提示
    /// </summary>
    /// <param name="partType"></param>
    /// <returns></returns>
    string GetSensorLinkTips(TopologyPartType partType, byte id)
    {
        string tips = string.Empty;
        switch (partType)
        {
            case TopologyPartType.Infrared:
                tips = string.Format(LauguageTool.GetIns().GetText("红外传感器连接异常"), id);
                break;
            case TopologyPartType.Touch:
                tips = string.Format(LauguageTool.GetIns().GetText("触碰传感器连接异常"), id);
                break;
            case TopologyPartType.Gyro:
                tips = string.Format(LauguageTool.GetIns().GetText("陀螺仪传感器连接异常"), id);
                break;
            case TopologyPartType.Light:
                tips = string.Format(LauguageTool.GetIns().GetText("灯光传感器连接异常"), id);
                break;
            case TopologyPartType.DigitalTube:
                tips = string.Format(LauguageTool.GetIns().GetText("数码管传感器连接异常"), id);
                break;
            case TopologyPartType.Speaker:
                tips = LauguageTool.GetIns().GetText("蓝牙喇叭连接异常");
                break;
        }
        return tips;
    }
    /// <summary>
    /// 获取传感器版本不一致提示
    /// </summary>
    /// <param name="partType"></param>
    /// <returns></returns>
    string GetSensorVersionTips(TopologyPartType partType, byte id)
    {
        string tips = string.Empty;
        switch (partType)
        {
            case TopologyPartType.Infrared:
                tips = string.Format(LauguageTool.GetIns().GetText("红外传感器版本不一致"), id);
                break;
            case TopologyPartType.Gyro:
                tips = string.Format(LauguageTool.GetIns().GetText("陀螺仪传感器版本不一致"), id);
                break;
            case TopologyPartType.Touch:
                tips = string.Format(LauguageTool.GetIns().GetText("触碰传感器版本不一致"), id);
                break;
            case TopologyPartType.Light:
                tips = string.Format(LauguageTool.GetIns().GetText("灯光传感器版本不一致"), id);
                break;
            case TopologyPartType.DigitalTube:
                tips = string.Format(LauguageTool.GetIns().GetText("数码管传感器版本不一致"), id);
                break;
        }
        return tips;
    }

    /*protected virtual void onFirstTouchBegan(TouchEventArgs args)
    {

    }

    protected virtual void onFirstTouchMoved(TouchEventArgs args)
    {

    }

    protected virtual void onFirstTouchEnded(TouchEventArgs args)
    {

    }*/


    void OnInputSelect(bool isSelect, GameObject obj)
    {
        try
        {
            if (isSelect)
            {
                //mNameInput.value = mNameInput.value.TrimEnd(' ');
            }
            else
            {
                if (null != mNameInput)
                {
                    string name = mNameInput.value;
                    //name = name.TrimEnd(' ');
                    string mac = PlatformMgr.Instance.GetRobotConnectedMac(mRobot.ID);
                    if (string.IsNullOrEmpty(mac))
                    {
                        mNameInput.value = string.Empty;
                        return;
                    }
                    string oldName = PlatformMgr.Instance.GetNameForMac(mac);
                    if (!string.IsNullOrEmpty(name) && !name.Equals(oldName) && !name.StartsWith("Jimuspk_"))
                    {
                        PlatformMgr.Instance.SaveMacAnotherName(mac, name);
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改成功"), HUDTextTips.Color_Green);
                    }
                    else
                    {
                        mNameInput.value = oldName;
                    }
                    //mNameInput.value = mNameInput.value.PadRight(10, ' ');
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
    byte tmpLenght = 0;
    char OnValidate(string text, int charIndex, char addedChar)
    {
        if (charIndex == 0)
        {
            tmpLenght = 0;
        }
        if (tmpLenght >= 11)
        {//限制长度
            return (char)0;
        }
        if (Convert.ToInt32(addedChar) >= Convert.ToInt32(Convert.ToChar(128)))
        {//中文字符
            if (tmpLenght + 2 > 11)
            {
                return (char)0;
            }
            tmpLenght += 2;
        }
        else
        {
            ++tmpLenght;
        }
        return addedChar;
    }

    //////////////////////////////////////////////////////////////////////////
    //升级相关

    /// <summary>
    /// 检查模型数据是否匹配
    /// </summary>
    void CheckModelData()
    {
        isUpdating = false;
        isSuccess = false;
        mCheckUpdateFlag = false;
        mCompareResult = ErrorCode.Result_OK;
        if (null != mMainBoardData)
        {
            mServoUpdateResult = Robot.CheckServoUpdate(mMainBoardData);
            mSystemUpdateResult = Robot.CheckSystemUpdate(mMainBoardData.mbVersion);
        }
        else
        {
            mSystemUpdateResult = ErrorCode.Do_Not_Upgrade;
            mServoUpdateResult = ErrorCode.Do_Not_Upgrade;
        }
        

        bool defaultFlag = false;
        if (RecordContactInfo.Instance.openType == "default")
        {
            defaultFlag = true;
        }
        if (null != mMainBoardData && mMainBoardData.errorIds.Count > 0)
        {//舵机id重复
            mCompareResult = ErrorCode.Result_DJ_ID_Repeat;
            PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机ID重复，请修改舵机ID"), PublicFunction.ListToString(mMainBoardData.errorIds)), PopRetrieveMotherboardDataOnClick);
            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
        }
        else
        {
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                if (null != mMainBoardData && mMainBoardData.ids.Count < 1)
                {
                    mCompareResult = ErrorCode.Result_Servo_Num_Inconsistent;
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("WuDuoJi"), PopRetrieveMotherboardDataOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
                else
                {
                    CheckUpdateData(false);
                }
            }
            else
            {
                if (null != mRobot)
                {
                    mCompareResult = CompareServoData();
                    if (ErrorCode.Result_Servo_Num_Inconsistent == mCompareResult)
                    {//舵机数量不一致
                        string str = string.Empty;
                        if (defaultFlag)
                        {
                            str = LauguageTool.GetIns().GetText("舵机数量跟拓扑图不一致，请检查后重试！");
                        }
                        else
                        {
                            str = LauguageTool.GetIns().GetText("舵机数量跟个人模型不一致，请检查后重试！");
                        }
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(str, PopRetrieveMotherboardDataOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                    else if (ErrorCode.Result_Servo_ID_Inconsistent == mCompareResult)
                    {//舵机id不匹配
                        string str = string.Empty;
                        if (defaultFlag)
                        {
                            str = LauguageTool.GetIns().GetText("舵机ID跟拓扑图不一致，请检查后重试！");
                        }
                        else
                        {
                            str = LauguageTool.GetIns().GetText("舵机ID跟个人模型不匹配，请检查后重试！");
                        }
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(str, PopRetrieveMotherboardDataOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                    else
                    {//模型已匹配
                        CheckUpdateData(false);
                    }
                }
            }
        }
        mTopologyUI.SetPartState(mSystemUpdateResult, mNeedUpdateServoList);
        SetConfirmBtnState();
    }
    /// <summary>
    /// 检查升级
    /// </summary>
    void CheckUpdateData(bool updateFlag)
    {
        mCheckUpdateFlag = updateFlag;
        isSuccess = false;
        isUpdating = false;
        PlatformMgr.Instance.NeedUpdateFlag = false;
        mNeedUpdateServoList.Clear();
        if (null == mMainBoardData)
        {//用于模拟器上
            isSuccess = true;
            return;
        }
        if (ErrorCode.Do_Not_Upgrade == mServoUpdateResult)
        {//一切正常
        }
        else
        {
            byte needUpdateId = 0;
            if (null != mMainBoardData && mMainBoardData.errorVerIds.Count == 1 && mMainBoardData.djVersion.Equals(PlatformMgr.Instance.Robot_Servo_Version))
            {
                needUpdateId = mMainBoardData.errorVerIds[0];
            }
            if (0 == needUpdateId)
            {
                if (null == mMainBoardData)
                {//PC端模拟才会为空
                    mNeedUpdateServoList.AddRange(mRobot.GetAllDjData().GetIDList());
                }
                else
                {
                    for (int i = 0, imax = mMainBoardData.ids.Count; i < imax; ++i)
                    {
                        mNeedUpdateServoList.Add(mMainBoardData.ids[i]);
                    }
                }
            }
            else
            {
                mNeedUpdateServoList.Add(needUpdateId);
            }
            if (ErrorCode.Robot_Adapter_Open_Protect == mServoUpdateResult)
            {//有升级，且开启了充电保护，不断开
                if (mCheckUpdateFlag)
                {
                    isSuccess = true;
                    PlatformMgr.Instance.NeedUpdateFlag = true;
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
                }
            }
            else if (ErrorCode.Robot_Adapter_Close_Protect == mServoUpdateResult)
            {//有升级，未开充电保护，断开
                if (mCheckUpdateFlag)
                {
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
                }
            }
            else if (ErrorCode.Robot_Power_Low == mServoUpdateResult)
            {//电量过低，不升级
                if (mCheckUpdateFlag)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机版本不一致且设备电量过低"));
                }
            }
            else
            {//需要升级
                if (null != mRobot && (!mCheckUpdateFlag || mRobot.ServoUpdate(needUpdateId)))
                {
                    if (mCheckUpdateFlag)
                    {
                        isUpdating = true;
                        mUpdateTime = Time.time;
                    }
                }
                else
                {
                    mServoUpdateResult = ErrorCode.Do_Not_Upgrade;
                    mNeedUpdateServoList.Clear();
                }
            }
        }

        if (ErrorCode.Do_Not_Upgrade != mServoUpdateResult)
        {//有更新
        }
        else if (mMainBoardData.errorVerIds.Count > 0)
        {
            if (mCheckUpdateFlag)
            {
                PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), PublicFunction.ListToString(mMainBoardData.errorVerIds)), PopRetrieveMotherboardDataOnClick);
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
        }
        else
        {//
            if (ErrorCode.Do_Not_Upgrade == mSystemUpdateResult)
            {//一切正常
                isSuccess = true;
                if (!mCheckUpdateFlag)
                {
                    CheckSensorData();
                }
            }
            else if (ErrorCode.Robot_Adapter_Open_Protect == mSystemUpdateResult)
            {//有升级，且开启了充电保护，不断开
                if (mCheckUpdateFlag)
                {
                    isSuccess = true;
                    PlatformMgr.Instance.NeedUpdateFlag = true;
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
                }
            }
            else if (ErrorCode.Robot_Adapter_Close_Protect == mSystemUpdateResult)
            {//有升级，未开充电保护，断开
                if (mCheckUpdateFlag)
                {
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
                    //msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
            }
            else if (ErrorCode.Robot_Power_Low == mSystemUpdateResult)
            {//电量过低，不升级
                if (mCheckUpdateFlag)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("检测到主板程序有更新且设备电量过低"));
                }
            }
            else
            {//需要升级
                if (!mCheckUpdateFlag || mRobot.RobotBlueUpdate())
                {
                    if (mCheckUpdateFlag)
                    {
                        isUpdating = true;
                        mUpdateTime = Time.time;
                    }
                }
                else
                {
                    if (!mCheckUpdateFlag)
                    {
                        CheckSensorData();
                    }
                    mSystemUpdateResult = ErrorCode.Do_Not_Upgrade;
                    isSuccess = true;
                }
            }
        }
        if (mCheckUpdateFlag)
        {
            if (isUpdating)
            {
                isUpdateSuccess = false;
                if (ErrorCode.Result_OK == mServoUpdateResult)
                {
                    mTopologyUI.OpenServoUpdateAnim(mNeedUpdateServoList);
                }
                else if (ErrorCode.Result_OK == mSystemUpdateResult)
                {
                    mTopologyUI.OpenMainBoardUpdateAnim();
                }
            }
        }
    }

    int mCheckSensorTypeIndex;
    void CheckSensorData()
    {
        if (null != mMainBoardData)
        {
            TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = partType.Length; i < imax; ++i)
            {
                mCheckSensorTypeIndex = i;
                SensorData data = mMainBoardData.GetSensorData(partType[i]);
                if (null != data)
                {
                    if (TopologyPartType.Speaker == partType[i])
                    {
                        if (data.ids.Count > 1 || data.errorIds.Count > 0)
                        {
                            PromptMsg msg = PromptMsg.ShowDoublePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                        }
                    }
                    else if (data.errorIds.Count > 0)
                    {
                        PromptMsg.ShowSinglePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                        return;
                    }
                }
            }
        }
        
    }

    void PopSensorErrorOnClick(GameObject obj)
    {
        if (null != mMainBoardData)
        {
            TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
            if (partType[mCheckSensorTypeIndex] == TopologyPartType.Speaker && PromptMsg.RightBtnName.Equals(obj.name))
            {
                if (null != mRobot)
                {
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        mRobot.RetrieveMotherboardData();
                    }
                    else
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                    }
                }
            }
            else
            {
                for (int i = mCheckSensorTypeIndex + 1, imax = partType.Length; i < imax; ++i)
                {
                    mCheckSensorTypeIndex = i;
                    SensorData data = mMainBoardData.GetSensorData(partType[i]);
                    if (null != data)
                    {
                        if (TopologyPartType.Speaker == partType[i])
                        {
                            if (data.ids.Count > 1 || data.errorIds.Count > 0)
                            {
                                PromptMsg msg = PromptMsg.ShowDoublePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                            }
                        }
                        else if (data.errorIds.Count > 0)
                        {
                            PromptMsg.ShowSinglePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                            return;
                        }
                    }
                }
            }
            
        }
    }

    void PopSpeakerOnClick(GameObject obj)
    {
        if (PromptMsg.RightBtnName.Equals(obj.name))
        {
            PlatformMgr.Instance.ConnectSpeaker(string.Empty);
        }
    }

    /// <summary>
    /// 比较实物舵机与软件记录的舵机数据是否匹配
    /// </summary>
    /// <returns></returns>
    ErrorCode CompareServoData()
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do
        {
            if (null == mMainBoardData)
            {
                break;
            }
            List<byte> list = mRobot.GetAllDjData().GetIDList();
            if (list.Count != mMainBoardData.ids.Count)
            {
                ret = ErrorCode.Result_Servo_Num_Inconsistent;
                break;
            }
            for (int i = 0, icount = list.Count; i < icount; ++i)
            {
                if (list[i] != mMainBoardData.ids[i])
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
    /// <summary>
    /// 设置确认界面按钮状态
    /// </summary>
    void SetConfirmBtnState()
    {
        if (null != mReDetectBtnTrans && null != mConfirmBtnTrans)
        {
            if (mCompareResult == ErrorCode.Result_OK)
            {
                mConfirmBtnTrans.gameObject.SetActive(true);
                SetButtonTransformData(new Transform[] { mReDetectBtnTrans, mConfirmBtnTrans });
                /*Vector3 redetectPos = mReDetectBtnTrans.localPosition;
                redetectPos.x = -mConfirmBtnTrans.localPosition.x;
                mReDetectBtnTrans.localPosition = redetectPos;*/
            }
            else
            {
                mConfirmBtnTrans.gameObject.SetActive(false);
                //mReDetectBtnTrans.localPosition = Vector3.zero;
                SetButtonTransformData(new Transform[] { mReDetectBtnTrans });
            }
        }
    }

    
    void UpdateFinishedAnim(bool state)
    {
        mTopologyUI.UpdateFinishedAnim(state);
    }
    /// <summary>
    /// 确认完毕，跳入下一个页面
    /// </summary>
    void ConfirmFinished()
    {
        if (null != mRobot)
        {
            ServosConnection servosConnection = SingletonObject<ServosConManager>.GetInst().GetServosConnection(mRobot.ID);
            if (null == servosConnection)
            {//提示去设置拓扑图
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("设置你的拓扑图"), PopSetTopologyOnClick);
                msg.SetLeftBtnText(LauguageTool.GetIns().GetText("模型无轮子"));
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("去设置"));
            }
            else
            {
                //SetMsgType(TopologyMsgType.Topology_ShowInfo);
                OnClose();
            }
        }
        else
        {
            //SetMsgType(TopologyMsgType.Topology_ShowInfo);
            OnClose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msgType"></param>
    void SetMsgType(TopologyMsgType msgType)
    {
        if (mMsgType == msgType)
        {
            return;
        }
        HideOldMsg(mMsgType, msgType);
        ShowNewMsg(mMsgType, msgType);
        mMsgType = msgType;

    }

    void HideOldMsg(TopologyMsgType oldMsgType, TopologyMsgType newMsgType)
    {
        switch (oldMsgType)
        {
            case TopologyMsgType.Topology_Confirm:
                {
                    if (newMsgType != TopologyMsgType.Topology_ShowInfo)
                    {
                        SetTransPosition(mConfirmTitleTrans, new Vector3(0, 300), true);
                        /*if (msgType == TopologyMsgType.Topology_Setting && RecordContactInfo.Instance.openType != "default")
                        {
                            Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                            SetTransPosition(mBtnRefreshTrans, pos + new Vector3(300, 0), true);
                        }*/
                    }
                    SetTransPosition(mConfirmBottomTrans, new Vector3(0, -300), true);
                    mTopologyUI.HidePartState();
                }
                break;
            case TopologyMsgType.Topology_Setting:
                {
                    SetTransPosition(mSettingTitleTrans, new Vector3(0, 300), true);

                    if (null != mBtnFinishedTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnFinishedTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnFinishedTrans, pos + new Vector3(300, 0), true);
                    }
                    if (RecordContactInfo.Instance.openType != "default")
                    {
                        if (null != mBtnHelpTrans)
                        {
                            Vector3 pos = UIManager.GetWinPos(mBtnHelpTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x + 124, PublicFunction.Back_Btn_Pos.y);
                            SetTransPosition(mBtnHelpTrans, pos + new Vector3(300, 0), true);
                        }
                    }
                    if (null != mBtnRefreshTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, 100);
                        SetTransPosition(mBtnRefreshTrans, pos, false);
                    }
                    mTopologyUI.CloseEditTopology();
                }
                break;
            case TopologyMsgType.Topology_ShowInfo:
                {
                    SetTransPosition(mConfirmTitleTrans, new Vector3(0, 300), true);
                    SetTransPosition(mShowInfoBottomTrans, new Vector3(0, -300), true);
                    if (null != mBtnDeviceTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnDeviceTrans, pos + new Vector3(300, 0), true);
                    }
                    if (null != mBtnSettingTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnSettingTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnSettingTrans, pos + new Vector3(300, 0), true);
                    }
                    /*if (msgType == TopologyMsgType.Topology_Setting && RecordContactInfo.Instance.openType != "default")
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnRefreshTrans, pos + new Vector3(300, 0), true);
                    }*/
                }
                break;
        }
    }

    void ShowNewMsg(TopologyMsgType oldMsgType, TopologyMsgType newMsgType)
    {
        switch (newMsgType)
        {
            case TopologyMsgType.Topology_Confirm:
                {
                    if (null != mConfirmTitleTrans)
                    {
                        mConfirmTitleTrans.gameObject.SetActive(true);
                    }
                    if (null != mConfirmBottomTrans)
                    {
                        mConfirmBottomTrans.gameObject.SetActive(true);
                    }
                    SetTransPosition(mConfirmTitleTrans, Vector3.zero, false);
                    SetTransPosition(mConfirmBottomTrans, Vector3.zero, false);
                    mTopologyUI.SetOnClickDelegate(ConfirmTopologyUIOnClick);
                    mTopologyUI.SetChoicePartActiveCallBack(null);
                }
                break;
            case TopologyMsgType.Topology_Setting:
                {
                    if (null != mBtnRefreshTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnRefreshTrans, pos, false);
                    }
                    if (null != mSettingTitleTrans)
                    {
                        mSettingTitleTrans.gameObject.SetActive(true);
                    }
                    SetTransPosition(mSettingTitleTrans, Vector3.zero, false);
                    if (null != mBtnFinishedTrans)
                    {
                        mBtnFinishedTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnFinishedTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnFinishedTrans, pos, false);
                    }
                    if (RecordContactInfo.Instance.openType != "default")
                    {
                        if (null != mBtnHelpTrans)
                        {
                            mBtnHelpTrans.gameObject.SetActive(true);
                            Vector3 pos = UIManager.GetWinPos(mBtnHelpTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x + 124, PublicFunction.Back_Btn_Pos.y);
                            SetTransPosition(mBtnHelpTrans, pos, false);
                        }
                    }
                    mTopologyUI.OpenEditTopology();
                    mTopologyUI.SetOnClickDelegate(null);
                    mTopologyUI.SetChoicePartActiveCallBack(ChangeRefreshPosition);
                    if (null != mRobot)
                    {
                        mRobot.RobotPowerDown();
                    }
                }
                break;
            case TopologyMsgType.Topology_ShowInfo:
                {
                    if (null != mShowInfoBottomTrans)
                    {
                        mShowInfoBottomTrans.gameObject.SetActive(true);
                    }
                    SetTransPosition(mShowInfoBottomTrans, Vector3.zero, false);
                    if (oldMsgType != TopologyMsgType.Topology_Confirm)
                    {
                        if (null != mConfirmTitleTrans)
                        {
                            mConfirmTitleTrans.gameObject.SetActive(true);
                        }
                        SetTransPosition(mConfirmTitleTrans, Vector3.zero, false);
                        /*if (mMsgType == TopologyMsgType.Topology_Setting && RecordContactInfo.Instance.openType != "default")
                        {
                            if (null != mBtnRefreshTrans)
                            {
                                mBtnRefreshTrans.gameObject.SetActive(true);
                            }
                            Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                            SetTransPosition(mBtnRefreshTrans, pos, false);
                        }*/

                    }
                    mTopologyUI.SetOnClickDelegate(null);
                    mTopologyUI.SetChoicePartActiveCallBack(null);
                }
                bool showSetFlag = false;
#if UNITY_EDITOR
                showSetFlag = true;
#else
                if (RecordContactInfo.Instance.openType != "default")
                {
                    showSetFlag = true;
                }
#endif
                if (showSetFlag)
                {
                    if (null != mBtnDeviceTrans)
                    {
                        mBtnDeviceTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x + 124, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnDeviceTrans, pos, false);
                    }
                    if (null != mBtnSettingTrans)
                    {
                        mBtnSettingTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnSettingTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnSettingTrans, pos, false);
                    }
                }
                else
                {
                    if (null != mBtnDeviceTrans)
                    {
                        mBtnDeviceTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnDeviceTrans, pos, false);
                    }
                }
                break;
        }
    }

    void ChangeRefreshPosition(bool activeFlag)
    {
        if (null != mBtnRefreshTrans)
        {
            if (activeFlag)
            {
                Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, 174);
                SetTransPosition(mBtnRefreshTrans, pos, false);
            }
            else
            {
                Vector3 pos = UIManager.GetWinPos(mBtnRefreshTrans, UIWidget.Pivot.BottomLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                SetTransPosition(mBtnRefreshTrans, pos, false);
            }

        }
    }

    void SetTransPosition(Transform trans, Vector3 pos, bool hideFlag)
    {
        if (null != trans)
        {
            TweenPosition tweenPosition = trans.GetComponent<TweenPosition>();
            if (null != tweenPosition)
            {
                GameHelper.PlayTweenPosition(tweenPosition, pos, 0.6f);
                if (hideFlag)
                {
                    tweenPosition.SetOnFinished(delegate ()
                    {
                        trans.gameObject.SetActive(false);
                    });
                }
                else
                {
                    if (null != tweenPosition.onFinished)
                    {
                        tweenPosition.onFinished.Clear();
                    }
                }
            }
            else
            {
                trans.localPosition = pos;
                if (hideFlag)
                {
                    trans.gameObject.SetActive(false);
                }
                else
                {
                    if (null != tweenPosition && null != tweenPosition.onFinished)
                    {
                        tweenPosition.onFinished.Clear();
                    }
                }
            }
        }
    }

    void SetConnectLabel(bool result)
    {
        if (null != mShowInfoBottomTrans)
        {
            UILabel lb = GameHelper.FindChildComponent<UILabel>(mShowInfoBottomTrans, "btnDisconnect/Label");
            UISprite bg = GameHelper.FindChildComponent<UISprite>(mShowInfoBottomTrans, "btnDisconnect/Background");
            if (null != lb && null != bg)
            {
                if (result)
                {
                    lb.text = LauguageTool.GetIns().GetText("断开连接");
                    bg.color = new Color32(237, 53, 114, 255);
                }
                else
                {
                    lb.text = LauguageTool.GetIns().GetText("连接");
                    bg.color = new Color32(57, 198, 234, 255);
                }

            }
        }
    }
    /// <summary>
    /// 设置按钮位置和大小
    /// </summary>
    /// <param name="btns"></param>
    void SetButtonTransformData(Transform[] btns)
    {
        int size = btns.Length;
        int space = 2;
        int screenWidth = PublicFunction.GetExtendWidth();
        int width = (screenWidth - (size - 1) * space) / size;
        for (int i = 0; i < size; ++i)
        {
            BoxCollider box = btns[i].GetComponent<BoxCollider>();
            if (null != box)
            {
                Vector3 boxSize = box.size;
                boxSize.x = width;
                box.size = boxSize;
            }
            UISprite bg = GameHelper.FindChildComponent<UISprite>(btns[i], "Background");
            if (null != bg)
            {
                bg.width = width;
            }
            UILabel lb = GameHelper.FindChildComponent<UILabel>(btns[i], "Label");
            if (null != lb)
            {
                lb.width = width;
            }
            btns[i].localPosition = new Vector3(-screenWidth/2 + space * i + width / 2 + width * i, 0);
        }
    }

    void PopUpdateErrorOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.LeftBtnName))
        {

        }
        else if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            if (null != mRobot)
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mRobot.RetrieveMotherboardData();
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                }
            }
        }
    }

    void PopRetrieveMotherboardDataOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.LeftBtnName))
        {

        }
        else if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            if (null != mRobot)
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mRobot.RetrieveMotherboardData();
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                }
            }
        }
    }

    void PopUpdateSuccessOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            if (null != mRobot)
            {
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mRobot.RetrieveMotherboardData();
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                }
            }
            isUpdateSuccess = true;
            //SetBtnState();
        }
    }

    void PopPowerLowOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            mServoUpdateResult = Robot.CheckServoUpdate(mMainBoardData);
            mSystemUpdateResult = Robot.CheckSystemUpdate(mMainBoardData.mbVersion);
            CheckUpdateData(mCheckUpdateFlag); ;
        }
    }

    void UpdateFinishedCallBack(EventArg arg)
    {
        try
        {
            bool updateSystem = (bool)arg[0];
            UpdateFinishedAnim(true);
            Timer.Add(1, 1, 1, delegate ()
            {
                isUpdating = false;
                isSuccess = true;
                if (updateSystem || mSystemUpdateResult == ErrorCode.Do_Not_Upgrade)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("设备升级成功"), PopUpdateSuccessOnClick);
                }
                else
                {
                    mMainBoardData.djVersion = PlatformMgr.Instance.Robot_Servo_Version;
                    mMainBoardData.errorVerIds.Clear();
                    mServoUpdateResult = Robot.CheckServoUpdate(mMainBoardData);
                    mSystemUpdateResult = Robot.CheckSystemUpdate(mMainBoardData.mbVersion);
                    CheckUpdateData(mCheckUpdateFlag);
                }
            });
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

    void UpdateErrorCallBack(EventArg arg)
    {
        try
        {
            PlatformMgr.Instance.SetSendXTState(true);
            bool updateSystem = (bool)arg[0];
            UpdateFinishedAnim(false);
            if (null != mRobot)
            {
                if (updateSystem)
                {
                    mRobot.RobotBlueUpdateStop();
                }
                else
                {
                    mRobot.StopServoUpdate();
                }
                
            }
            Timer.Add(1, 1, 1, delegate ()
            {
                isUpdating = false;
                PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("升级异常！您是否需要重新升级？"), PopUpdateErrorOnClick);
            });

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

    void UpdateFailCallBack(EventArg arg)
    {
        try
        {
            bool updateSystem = (bool)arg[0];
            if (updateSystem)
            {
                isUpdating = false;
            }
            else
            {
                ServoUpdateFailAck msg = (ServoUpdateFailAck)arg[1];
                UpdateFinishedAnim(false);
                Timer.Add(1, 1, 1, delegate ()
                {
                    isUpdating = false;
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机升级失败,您是否需要重新升级？"), PublicFunction.ListToString(msg.servoList)), PopUpdateErrorOnClick);
                });

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

    void OnConnenctResult(EventArg arg)
    {
        try
        {
            bool result = (bool)arg[0];
            SetConnectLabel(result);
            if (result)
            {

            }
            else
            {
                if (isUpdating)
                {
                    isUpdating = false;
                    UpdateFinishedAnim(false);
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
    /// 更新升级进度
    /// </summary>
    /// <param name="arg"></param>
    void UpdateProgressResult(EventArg arg)
    {
        try
        {
            int progress = (int)arg[0];
            mTopologyUI.UpdateProgress(progress);
            mUpdateTime = Time.time;
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
    /// 读取speaker数据返回
    /// </summary>
    /// <param name="arg"></param>
    void ReadSpeakerCallBack(EventArg args)
    {
        try
        {
            byte id = (byte)args[0];
            SpeakerData speakerData = (SpeakerData)mRobot.GetReadSensorData(TopologyPartType.Speaker);
            if (null != speakerData)
            {
                SpeakerInfoData infoData = speakerData.GetSpeakerData(id);
                if (null != infoData)
                {
#if UNITY_ANDROID
                    if (!PlatformMgr.Instance.IsConnectedSpeaker(infoData.speakerMac))
                    {
                        PlatformMgr.Instance.ConnectSpeaker(infoData.speakerMac);
                    }
#else
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("检测到蓝牙音响需要连接"), infoData.speakerName), PopSpeakerOnClick);
#endif
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


    void PopSetTopologyOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.LeftBtnName))
        {
            mTopologyUI.SaveTopologyData();
            //SetMsgType(TopologyMsgType.Topology_ShowInfo);
            OnClose();
        }
        else if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            //mTopologyUI.SaveTopologyData();
            SetMsgType(TopologyMsgType.Topology_Setting);
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(TopologyGuideMsg));
        }
    }

}


