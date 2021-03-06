﻿using Game.Platform;
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
    BlueConnectFailReason mFailReason = BlueConnectFailReason.unknow;
    ErrorCode mSystemUpdateResult;//主板更新结果
    ErrorCode mServoUpdateResult;//舵机更新结果
    ErrorCode mCompareResult;//比较模型的结果
    bool mCheckUpdateFlag;//检查升级的时候检测到升级是否升级

    Dictionary<TopologyPartType, ErrorCode> mSensorUpdateResult;//传感器更新结果
    Dictionary<TopologyPartType, List<byte>> mSensorNeedUpdateList;//需要升级的传感器id

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
    //Transform mBtnDeviceTrans;
    Transform mBtnSettingTrans;
    Transform mBtnHelpTrans;
    Transform mBtnSetServoTrans;

    TopologyUI mTopologyUI;

    TopologyMsgType mMsgType = TopologyMsgType.Topology_Confirm;

    float mUpdateTime = 0;

    bool isSpeakerPromptFlag = false;

    byte UpdateOutTime = 30;


#if UNITY_EDITOR

    GameObject mAddSensorGameObject = null;

    public static TopologyUI GetTopologyUI()
    {
        if (null == mInst)
        {
            return null;
        }
        return mInst.mTopologyUI;
    }
#endif

    public TopologyBaseMsg(TopologyMsgType msgType)
    {
        mUIResPath = "Prefab/UI/TopologyBase";
        mTopologyUI = new TopologyUI(eUICameraType.OrthographicTwo);
        isSingle = true;
        isCoverAddPanel = true;
        mNeedUpdateServoList = new List<byte>();
        mSensorNeedUpdateList = new Dictionary<TopologyPartType, List<byte>>();
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
        //永不待机
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
                mInst.mTopologyUI.RefreshIndependent();
                mInst.mNeedUpdateServoList.Clear();
                mInst.mSensorNeedUpdateList.Clear();
                mInst.CheckSystemUpdate();
                if (ErrorCode.Result_OK != mInst.mServoUpdateResult)
                {
                    mInst.GetNeedUpdateServo();
                }
                mInst.GetNeedUpdateSensor();
                mInst.mTopologyUI.SetPartState(mInst.mSystemUpdateResult, mInst.mNeedUpdateServoList, mInst.mSensorNeedUpdateList);
                if (SingletonObject<UpdateManager>.GetInst().IsSystemUpdateSucces())
                {
                    mInst.isUpdating = false;
                    mInst.UpdateFinishedAnim(true);
                    Timer.Add(0.6f, 1, 1, delegate () {
                        mInst.mCheckUpdateFlag = true;
                        mInst.CheckModelData();
                    });
                }
                else if (mInst.isUpdateSuccess && mInst.isSuccess)
                {
                    if (!SingletonObject<UpdateManager>.GetInst().IsSystemUpdateSucces())
                    {//防止重复调用
                        mInst.isUpdating = false;
                        mInst.SetCheckUpdateInfo();
                        mInst.UpdateFinishedAnim(true);
                    }
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("设备升级成功"), HUDTextTips.Color_Green);
                    Timer.Add(0.6f, 1, 1, delegate ()
                    {
                        if (null != mInst)
                        {
                            mInst.ConfirmFinished();
                        }
                    });

                }
                else
                {
                    mInst.mCheckUpdateFlag = false;
                    mInst.CheckModelData();
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
            EventMgr.Inst.Regist(EventID.Update_Cannel, UpdateCannelCallBack);
            EventMgr.Inst.Regist(EventID.Save_Topology_Data, SaveTopologyCallBack);
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

                    /*Transform btnDevice = top.Find("btnDevice");
                    if (null != btnDevice)
                    {
                        mBtnDeviceTrans = btnDevice;
                        mBtnDeviceTrans.localPosition = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y) + new Vector3(300, 0);
                        mBtnDeviceTrans.gameObject.SetActive(false);
                    }*///20161209注释

                    mBtnSetServoTrans = top.Find("btnSetServo");
                    if (null != mBtnSetServoTrans)
                    {
                        mBtnSetServoTrans.localPosition = UIManager.GetWinPos(mBtnSetServoTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y) + new Vector3(300, 0);
                        mBtnSetServoTrans.gameObject.SetActive(false);
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
                            /*mNameInput = GameHelper.FindChildComponent<UIInput>(mConfirmTitleTrans, "Input");
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
                            }*/
                            if (null != mRobot)
                            {
                                GameHelper.SetLabelText(mConfirmTitleTrans.Find("maintitle"), mRobot.ShowName);
                            }
                            else
                            {
                                GameHelper.SetLabelText(mConfirmTitleTrans.Find("maintitle"), string.Empty);
                            }
                            GameHelper.SetLabelText(mConfirmTitleTrans.Find("subtitle"), LauguageTool.GetIns().GetText("请勿让Jimu远离手机"));
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
                        mBtnRefreshTrans.gameObject.SetActive(false);
                    }

                }

            }
            
            ShowBottomBtn();
            if (mMsgType == TopologyMsgType.Topology_Confirm)
            {
                mCheckUpdateFlag = false;
                Timer.Add(0.1f, 1, 1, CheckModelData);
                mTopologyUI.SetOnClickDelegate(ConfirmTopologyUIOnClick);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }

    }

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 1;
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
        EventMgr.Inst.UnRegist(EventID.Update_Cannel, UpdateCannelCallBack);
        EventMgr.Inst.UnRegist(EventID.Save_Topology_Data, SaveTopologyCallBack);
        mInst = null;
#if UNITY_EDITOR
        if (null != mAddSensorGameObject)
        {
            GameObject.Destroy(mAddSensorGameObject);
            mAddSensorGameObject = null;
        }
#endif
        if (isConnecting)
        {
            if (isSuccess)
            {
                
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    if (null != mMainBoardData && mMainBoardData.power <= PublicFunction.Robot_Power_Empty)
                    {//没电了,断开蓝牙
                        PlatformMgr.Instance.DisConnenctBuletooth();
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("电量低，请充电"), delegate (GameObject obj)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                        });
                        PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, BlueConnectFailReason.LowPower.ToString());
                        return;
                    }
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        SingletonObject<ConnectManager>.GetInst().ConnectFinished(robot);
                        mTopologyUI.SaveTopologySensorData();
                        PlatformMgr.Instance.SaveLastConnectedData(mRobot.Mac);
                    }
                    else
                    {
                        PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionSucceeded);
                        SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                    }
                }
                else
                {
                    SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                }
                /*if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
                {
                    PowerMsg.OpenPower();
                }*/
            }
            else
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, mFailReason.ToString());
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
            if (Time.time - mUpdateTime > UpdateOutTime)
            {
                isUpdating = false;
                NetWaitMsg.ShowWait(1);
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(SingletonObject<UpdateManager>.GetInst().GetUpdateDeviceType()));
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
            if (isUpdating)
            {
                return;
            }
            string name = obj.name;
            if (name.Equals("btnRefresh"))
            {
                //mTopologyUI.HideChoicePartPanel(false);
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
                        //mTopologyUI.RecoverTopology();
                        //SetMsgType(TopologyMsgType.Topology_ShowInfo);
                        //mTopologyUI.SaveTopologyData();
                        if (!isConnecting)
                        {
                            EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
                        }
                        OnClose();
                    }
                    else if (mMsgType == TopologyMsgType.Topology_Confirm)
                    {
                        if (BlueConnectFailReason.unknow != mFailReason)
                        {
                            mFailReason = BlueConnectFailReason.Cancel;
                        }
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
                if (isSuccess)
                {
                    ConfirmFinished();
                }
                else
                {
                    CheckUpdateData(true);
                    SetCheckUpdateInfo();
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
                    //mTopologyUI.HideChoicePartPanel(false);
                    mTopologyUI.RemoveDisContinuousPart();
                }
            }
            else if (name.Equals("btnSetServo"))
            {//设置舵机模式
                OnHide();
                mTopologyUI.OnHide();
                SetServoTypeMsg.ShowMsg(SetServoModelOnClose);
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
                    //SearchBluetoothMsg.ShowMsg();
                    ConnectBluetoothMsg.ShowMsg();
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
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetServoModelOnClose()
    {
        EventMgr.Inst.Fire(EventID.Exit_Blue_Connect);
        OnClose();
    }


    void ConfirmTopologyUIOnClick(GameObject obj)
    {
        if (isUpdating)
        {
            return;
        }
        string btnName = obj.name;
        if (btnName.Equals("MotherBox"))
        {//点击了主板
            if (null != mMainBoardData && !string.IsNullOrEmpty(SingletonObject<UpdateManager>.GetInst().Robot_System_Version) && !SingletonObject<UpdateManager>.GetInst().Robot_System_Version.Equals(mMainBoardData.mbVersion))
            {
                if (isUpdating)
                {
                    //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
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
                PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机ID重复，请修改舵机ID"), id), PopRetrieveMotherboardDataOnClick, PopServoRepeatHelpOnClick);
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
            else if (mNeedUpdateServoList.Contains(id))
            {//需要升级的舵机
                if (isUpdating)
                {
                    //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
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
                    //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                }
            }
            else if (null != mMainBoardData && !mMainBoardData.ids.Contains(id))
            {//舵机不存在，检查连接
                PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机连接异常"), id), PopRetrieveMotherboardDataOnClick, PopServoConnectionExceptionHelpOnClick);
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
            else if (null != mMainBoardData && !string.IsNullOrEmpty(SingletonObject<UpdateManager>.GetInst().Robot_Servo_Version) && !SingletonObject<UpdateManager>.GetInst().Robot_Servo_Version.Equals(mMainBoardData.djVersion))
            {
                if (isUpdating)
                {
                    //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
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
                            else if (mSensorNeedUpdateList.ContainsKey(partType) && mSensorNeedUpdateList[partType].Contains(id))
                            {
                                if (!isUpdating)
                                {
                                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                                }
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
                            else if (!string.IsNullOrEmpty(SingletonObject<UpdateManager>.GetInst().GetSensorVersion(partType)) && !SingletonObject<UpdateManager>.GetInst().GetSensorVersion(partType).Equals(data.version))
                            {
                                if (!isUpdating)
                                {
                                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("需要升级"));
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
                
            }
        }
    }

    void ShowInfoOnClick(GameObject obj)
    {
        try
        {
            if (!PlatformMgr.Instance.GetBluetoothState() || null == mRobot || null == mMainBoardData)
            {
                return;
            }
            string btnName = obj.name;
            if (btnName.Equals("MotherBox"))
            {//点击了主板
                PromptDevice.ShowMainboardInfo(mMainBoardData);
            }
            else if (btnName.StartsWith("servo_"))
            {//点击了某个舵机
                byte id = byte.Parse(btnName.Substring("servo_".Length));
                DuoJiData data = mRobot.GetAnDjData(id);
                if (null != data)
                {
                    if (data.modelType == ServoModel.Servo_Model_Angle)
                    {
                        PromptDevice.ShowAngleServoInfo(id, mMainBoardData.djVersion);
                    }
                    else
                    {
                        PromptDevice.ShowTurnServoInfo(id, mMainBoardData.djVersion);
                    }
                }
            }
            else if (btnName.StartsWith("sensor_"))
            {//点中了传感器
                string[] arys = btnName.Split('_');
                if (arys.Length == 3)
                {
                    TopologyPartType partType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), arys[1]);
                    byte id = byte.Parse(arys[2]);
                    SensorData data = mMainBoardData.GetSensorData(partType);
                    if (null != data)
                    {
                        PromptDevice.ShowSensorInfo(partType, id, data.version);
                    }
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
        mCompareResult = ErrorCode.Result_OK;
        if (null != mMainBoardData)
        {
            CheckUpdateResult();
        }
        else
        {
            mSystemUpdateResult = ErrorCode.Result_OK;
            mServoUpdateResult = ErrorCode.Result_OK;
        }
        

        /*bool defaultFlag = false;
        if (RecordContactInfo.Instance.openType == "default")
        {
            defaultFlag = true;
        }*/
        if (null != mMainBoardData && mMainBoardData.errorIds.Count > 0)
        {//舵机id重复
            mCompareResult = ErrorCode.Result_DJ_ID_Repeat;
            mFailReason = BlueConnectFailReason.ModelInfoIncorrect_repeatedID;
            PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机ID重复，请修改舵机ID"), PublicFunction.ListToString(mMainBoardData.errorIds)), PopRetrieveMotherboardDataOnClick, PopServoRepeatHelpOnClick);
            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
        }
        else
        {
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                if (null != mMainBoardData && mMainBoardData.ids.Count < 1)
                {
                    mCompareResult = ErrorCode.Result_Servo_Num_Inconsistent;
                    mFailReason = BlueConnectFailReason.ModelInfoIncorrect_differentID;
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("WuDuoJi"), PopRetrieveMotherboardDataOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
                else
                {
                    CheckUpdateData(mCheckUpdateFlag);
                }
            }
            else
            {
                if (null != mRobot)
                {
                    mCompareResult = SingletonObject<ConnectManager>.GetInst().CompareServoData(mRobot, mMainBoardData);
                    if (ErrorCode.Result_Servo_Num_Inconsistent == mCompareResult)
                    {//舵机数量不一致
                        mFailReason = BlueConnectFailReason.ModelInfoIncorrect_servoAmount;
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("舵机数量跟拓扑图不一致，请检查后重试！"), PopRetrieveMotherboardDataOnClick, PopServoNumDoNotMatchHelpOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                    else if (ErrorCode.Result_Servo_ID_Inconsistent == mCompareResult)
                    {//舵机id不匹配
                        mFailReason = BlueConnectFailReason.ModelInfoIncorrect_differentID;
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("舵机ID跟拓扑图不一致，请检查后重试！"), PopRetrieveMotherboardDataOnClick, PopServoIDDoNotMatchHelpOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                    else
                    {//模型已匹配
                        CheckUpdateData(mCheckUpdateFlag);
                    }
                }
            }
        }
        if (!isUpdating)
        {
            mTopologyUI.SetPartState(mSystemUpdateResult, mNeedUpdateServoList, mSensorNeedUpdateList);
        }
        SetConfirmBtnState();
        SetCheckUpdateInfo();
    }

    void SetCheckUpdateInfo()
    {
        if (isSuccess && !isUpdateSuccess)
        {
            SetNormalTitle(LauguageTool.GetIns().GetText("请确认你的Jimu信息"), false);
            UpdateBtnOnAwake();
        }
        else if (isUpdating)
        {
            SetNormalTitle(LauguageTool.GetIns().GetText("固件升级中..."), true);
            UpdateBtnOnSleep();
        }
        else
        {
            string name = string.Empty;
            if (null != mRobot)
            {
                name = mRobot.ShowName;
            }
            SetNormalTitle(name, false);
            UpdateBtnOnAwake();
        }
    }
    /// <summary>
    /// 检查升级
    /// </summary>
    void CheckUpdateData(bool updateFlag)
    {
        if (!PlatformMgr.Instance.GetBluetoothState())
        {
            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
            return;
        }
        mCheckUpdateFlag = updateFlag;
        isSuccess = false;
        isUpdating = false;
        //PlatformMgr.Instance.NeedUpdateFlag = false;
        mNeedUpdateServoList.Clear();
        mSensorNeedUpdateList.Clear();
        if (null == mMainBoardData)
        {//用于模拟器上
            isSuccess = true;
            return;
        }
        CheckSystemUpdate();
        if (ErrorCode.Result_OK != mServoUpdateResult)
        {
            GetNeedUpdateServo();
        }
        GetNeedUpdateSensor();
        if (ErrorCode.Result_OK == mSystemUpdateResult)
        {
            CheckServoUpdate();
        }
        if (ErrorCode.Result_OK == mSystemUpdateResult && ErrorCode.Result_OK == mServoUpdateResult)
        {
            if (!mCheckUpdateFlag)
            {
                CheckSensorData();
            }
            else
            {
                CheckSensorUpdate();
            }
            if (CheckDeviceUpdateFinished())
            {
                isSuccess = true;
            }
        }
        else if (ErrorCode.Robot_Adapter_Open_Protect == mSystemUpdateResult || ErrorCode.Robot_Adapter_Open_Protect == mServoUpdateResult)
        {
            if (mCheckUpdateFlag)
            {
                isSuccess = true;
            }
        }
        

        if (mCheckUpdateFlag && isUpdating)
        {
            isUpdateSuccess = false;
            if (ErrorCode.Can_Upgraded == mSystemUpdateResult)
            {
                UpdateOutTime = 30;
                mTopologyUI.OpenMainBoardUpdateAnim();
            }
            else if (ErrorCode.Can_Upgraded == mServoUpdateResult)
            {
                UpdateOutTime = 30;
                mTopologyUI.OpenServoUpdateAnim(mNeedUpdateServoList);
            }
            else
            {
                if (null != mSensorUpdateResult)
                {
                    foreach (var kvp in mSensorUpdateResult)
                    {
                        if (ErrorCode.Can_Upgraded == kvp.Value && mSensorNeedUpdateList.ContainsKey(kvp.Key))
                        {
                            UpdateOutTime = (byte)(GetSensorUpdateTime(kvp.Key) * mSensorNeedUpdateList[kvp.Key].Count * 2 + 10);
                            mTopologyUI.OpenSensorUpdateAnim(kvp.Key, mSensorNeedUpdateList[kvp.Key]);
                            break;
                        }
                    }
                }
            }
        }
    }

    byte GetSensorUpdateTime(TopologyPartType sensorType)
    {
        byte sec = 5;
        switch (sensorType)
        {
            case TopologyPartType.Infrared:
                sec = 5;
                break;
            case TopologyPartType.Touch:
                sec = 4;
                break;
            case TopologyPartType.Gyro:
                sec = 10;
                break;
            case TopologyPartType.Light:
                sec = 9;
                break;
            case TopologyPartType.Gravity:
                break;
            case TopologyPartType.Speaker:
                sec = 4;
                break;
            case TopologyPartType.Ultrasonic:
                break;
            case TopologyPartType.DigitalTube:
                sec = 5;
                break;
        }
        return sec;
    }

    void GetNeedUpdateServo()
    {
        byte needUpdateId = 0;
        if (null != mMainBoardData && mMainBoardData.errorVerIds.Count == 1 && mMainBoardData.djVersion.Equals(SingletonObject<UpdateManager>.GetInst().Robot_Servo_Version))
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
    }

    void GetNeedUpdateSensor()
    {
        TopologyPartType[] partAry = PublicFunction.Open_Topology_Part_Type;
        for (int i = 0, imax = partAry.Length; i < imax; ++i)
        {
            if (!mSensorUpdateResult.ContainsKey(partAry[i]) || mSensorUpdateResult[partAry[i]] != ErrorCode.Result_OK)
            {
                if (null != mMainBoardData)
                {
                    SensorData data = mMainBoardData.GetSensorData(partAry[i]);
                    if (null != data)
                    {
                        byte needUpdateId = 0;
                        if (data.errorVerIds.Count == 1 && data.version.Equals(SingletonObject<UpdateManager>.GetInst().GetSensorVersion(partAry[i])))
                        {
                            needUpdateId = data.errorVerIds[0];
                        }
                        List<byte> needUpdateList = new List<byte>();
                        if (0 == needUpdateId)
                        {
                            for (int sensorIndex = 0, indexMax = data.ids.Count; sensorIndex < indexMax; ++sensorIndex)
                            {
                                needUpdateList.Add(data.ids[sensorIndex]);
                            }
                            for (int sensorIndex = 0, indexMax = data.errorIds.Count; sensorIndex < indexMax; ++sensorIndex)
                            {
                                needUpdateList.Add(data.errorIds[sensorIndex]);
                            }
                        }
                        else
                        {
                            needUpdateList.Add(needUpdateId);
                        }
                        mSensorNeedUpdateList[partAry[i]] = needUpdateList;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查升级结果
    /// </summary>
    void CheckUpdateResult()
    {
        mServoUpdateResult = SingletonObject<UpdateManager>.GetInst().CheckUpdate(TopologyPartType.Servo, mMainBoardData);
        mSystemUpdateResult = SingletonObject<UpdateManager>.GetInst().CheckUpdate(TopologyPartType.MainBoard, mMainBoardData);
        TopologyPartType[] partAry = PublicFunction.Open_Topology_Part_Type;
        if (null == mSensorUpdateResult)
        {
            mSensorUpdateResult = new Dictionary<TopologyPartType, ErrorCode>();
        }
        for (int i = 0, imax = partAry.Length; i < imax; ++i)
        {
            mSensorUpdateResult[partAry[i]] = SingletonObject<UpdateManager>.GetInst().CheckUpdate(partAry[i], mMainBoardData);
        }
    }

    void CheckServoUpdate()
    {
        /*if (ErrorCode.Robot_Adapter_Open_Protect == mServoUpdateResult)
        {//有升级，且开启了充电保护，不断开
            if (mCheckUpdateFlag)
            {
                //PlatformMgr.Instance.NeedUpdateFlag = true;
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerAdapterOnClick);
            }
        }
        else if (ErrorCode.Robot_Adapter_Close_Protect == mServoUpdateResult)
        {//有升级，未开充电保护，断开
            if (mCheckUpdateFlag)
            {
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerAdapterOnClick);
            }
        }*/
        if (ErrorCode.Result_OK == mServoUpdateResult)
        {//一切正常

        }
        else if (ErrorCode.Robot_Power_Low == mServoUpdateResult)
        {//电量过低，不升级
            if (mCheckUpdateFlag)
            {
                mFailReason = BlueConnectFailReason.UnableUpgrade;
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("固件需要升级，但是电量过低"), PopPowerLowOnClick);
            }
        }
        else
        {//需要升级
            byte needUpdateId = (mNeedUpdateServoList.Count == 1) ? mNeedUpdateServoList[0] : (byte)0;
            if (null != mRobot && (!mCheckUpdateFlag || SingletonObject<UpdateManager>.GetInst().UpdateStart(TopologyPartType.Servo, mRobot, needUpdateId)/* mRobot.ServoUpdate(needUpdateId)*/))
            {
                if (mCheckUpdateFlag)
                {
                    isUpdating = true;
                    mFailReason = BlueConnectFailReason.UnableUpgrade;
                    mUpdateTime = Time.time;
                }
            }
            else
            {
                if (mMainBoardData.errorVerIds.Count > 0)
                {
                    if (mCheckUpdateFlag)
                    {
                        mFailReason = BlueConnectFailReason.UnableUpgrade;
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), PublicFunction.ListToString(mMainBoardData.errorVerIds)), PopRetrieveMotherboardDataOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                }
                else
                {
                    mServoUpdateResult = ErrorCode.Result_OK;
                    mNeedUpdateServoList.Clear();
                }
            }
        }
    }

    void CheckSystemUpdate()
    {

        if (ErrorCode.Result_OK == mSystemUpdateResult)
        {//一切正常

        }
        /*else if (ErrorCode.Robot_Adapter_Open_Protect == mSystemUpdateResult)
        {//有升级，且开启了充电保护，不断开
            if (mCheckUpdateFlag)
            {
                //PlatformMgr.Instance.NeedUpdateFlag = true;
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerAdapterOnClick);
            }
        }
        else if (ErrorCode.Robot_Adapter_Close_Protect == mSystemUpdateResult)
        {//有升级，未开充电保护，断开
            if (mCheckUpdateFlag)
            {
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerAdapterOnClick);
                //msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
        }*/
        else if (ErrorCode.Robot_Power_Low == mSystemUpdateResult)
        {//电量过低，不升级
            if (mCheckUpdateFlag)
            {
                mFailReason = BlueConnectFailReason.UnableUpgrade;
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("固件需要升级，但是电量过低"), PopPowerLowOnClick);
            }
        }
        else
        {//需要升级
            if (!mCheckUpdateFlag || SingletonObject<UpdateManager>.GetInst().UpdateStart(TopologyPartType.MainBoard, mRobot))
            {
                if (mCheckUpdateFlag)
                {
                    mFailReason = BlueConnectFailReason.UnableUpgrade;
                    isUpdating = true;
                    mUpdateTime = Time.time;
                }
            }
            else
            {
                mSystemUpdateResult = ErrorCode.Result_OK;
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
                            PromptMsg msg = PromptMsg.ShowSinglePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                            //msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                            return;
                        }
                    }
                    else if (data.errorIds.Count > 0)
                    {
                        PromptMsg msg = PromptMsg.ShowSinglePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                        return;
                    }
                }
            }
            CheckSensorUpdate();
        }
        
    }

    /// <summary>
    /// 检查传感器升级
    /// </summary>
    void CheckSensorUpdate()
    {
        if (null != mMainBoardData && null != mSensorUpdateResult)
        {
            TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = partType.Length; i < imax; ++i)
            {
                SensorData data = mMainBoardData.GetSensorData(partType[i]);
                if (null != data && mSensorUpdateResult.ContainsKey(partType[i]) && mSensorNeedUpdateList.ContainsKey(partType[i]))
                {
                    if (ErrorCode.Result_OK == mSensorUpdateResult[partType[i]])
                    {
                    }
                    else if (ErrorCode.Robot_Power_Low == mSensorUpdateResult[partType[i]])
                    {//电量过低，不升级
                        if (mCheckUpdateFlag)
                        {
                            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("固件需要升级，但是电量过低"), PopPowerLowOnClick);
                            return;
                        }
                    }
                    else
                    {//需要升级
                        byte needUpdateId = (mSensorNeedUpdateList[partType[i]].Count == 1) ? mSensorNeedUpdateList[partType[i]][0] : (byte)0;
                        if (null != mRobot && (!mCheckUpdateFlag || SingletonObject<UpdateManager>.GetInst().UpdateStart(partType[i], mRobot, needUpdateId)/* mRobot.ServoUpdate(needUpdateId)*/))
                        {
                            if (mCheckUpdateFlag)
                            {
                                isUpdating = true;
                                mUpdateTime = Time.time;
                                return;
                            }
                        }
                        else
                        {
                            mSensorUpdateResult[partType[i]] = ErrorCode.Result_OK;
                            mSensorNeedUpdateList.Remove(partType[i]);
                        }
                    }
                }
            }
        }
    }

    bool CheckDeviceUpdateFinished()
    {
        if (ErrorCode.Result_OK != mSystemUpdateResult)
        {
            return false;
        }
        if (ErrorCode.Result_OK != mServoUpdateResult)
        {
            return false;
        }
        if (null != mSensorUpdateResult)
        {
            foreach (var kvp in mSensorUpdateResult)
            {
                if (ErrorCode.Result_OK != kvp.Value)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void PopSensorErrorOnClick(GameObject obj)
    {
        if (null != mMainBoardData)
        {
            TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
            /*if (partType[mCheckSensorTypeIndex] == TopologyPartType.Speaker && PromptMsg.RightBtnName.Equals(obj.name))
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
            else*/
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
                                PromptMsg msg = PromptMsg.ShowSinglePrompt(GetSensorRepeatTips(partType[i], PublicFunction.ListToString<byte>(data.errorIds)), PopSensorErrorOnClick);
                                //msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                                return;
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
            CheckSensorUpdate();
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

    
    void UpdateFinishedAnim(bool state, bool instant = false)
    {
        mTopologyUI.UpdateFinishedAnim(state, instant);
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
                mTopologyUI.SaveTopologyData();
                OnClose();
                /*PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("设置你的拓扑图"), PopSetTopologyOnClick);
                msg.SetLeftBtnText(LauguageTool.GetIns().GetText("模型无轮子"));
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("去设置"));*/
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
                        SetTransPosition(mBtnRefreshTrans, pos, true);
                    }
                    mTopologyUI.CloseEditTopology();
#if UNITY_EDITOR
                    if (null != mAddSensorGameObject)
                    {
                        GameObject.Destroy(mAddSensorGameObject);
                        mAddSensorGameObject = null;
                    }
#endif
                }
                break;
            case TopologyMsgType.Topology_ShowInfo:
                {
                    SetTransPosition(mConfirmTitleTrans, new Vector3(0, 300), true);
                    SetTransPosition(mShowInfoBottomTrans, new Vector3(0, -300), true);
                    /*if (null != mBtnDeviceTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnDeviceTrans, pos + new Vector3(300, 0), true);
                    }*///20161209注释
                    if (null != mBtnSetServoTrans)
                    {
                        Vector3 pos = UIManager.GetWinPos(mBtnSettingTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnSetServoTrans, pos + new Vector3(300, 0), true);
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
                        mBtnRefreshTrans.gameObject.SetActive(true);
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
                    mTopologyUI.SetOnClickDelegate(null);
                    mTopologyUI.SetChoicePartActiveCallBack(ChangeRefreshPosition);
                    mTopologyUI.OpenEditTopology();
                    if (null != mRobot)
                    {
                        mRobot.RobotPowerDown();
                    }
#if UNITY_EDITOR
                    mAddSensorGameObject = new GameObject("addSensor");
                    mAddSensorGameObject.AddComponent<TopologyAddSensor>();
#endif
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
                    if (null != mRobot)
                    {
                        SetNormalTitle(mRobot.ShowName, false);
                    }
                    else
                    {
                        SetNormalTitle(string.Empty, false);
                    }
                    mTopologyUI.SetOnClickDelegate(ShowInfoOnClick);
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
                    /*if (null != mBtnDeviceTrans)
                    {
                        mBtnDeviceTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x + 124, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnDeviceTrans, pos, false);
                    }*///20161209注释
                    if (null != mBtnSettingTrans)
                    {
                        mBtnSettingTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnSettingTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnSettingTrans, pos, false);
                    }
                    if (null != mBtnSetServoTrans)
                    {
                        mBtnSetServoTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnSetServoTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x + 124, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnSetServoTrans, pos, false);
                    }
                }
                else
                {
                    
                    /*if (null != mBtnDeviceTrans)
                    {
                        mBtnDeviceTrans.gameObject.SetActive(true);
                        Vector3 pos = UIManager.GetWinPos(mBtnDeviceTrans, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        SetTransPosition(mBtnDeviceTrans, pos, false);
                    }*///20161209注释
                }
                break;
        }
    }

    void SetNormalTitle(string text, bool showSubTitle)
    {
        Transform title = mTrans.Find("top/title/normal");
        if (null != title)
        {
            Transform maintitle = title.Find("maintitle");
            if (null != maintitle)
            {
                GameHelper.SetLabelText(maintitle, text);
                TweenPosition tweenPosition = maintitle.GetComponent<TweenPosition>();
                if (null != tweenPosition)
                {
                    if (showSubTitle)
                    {
                        GameHelper.PlayTweenPosition(tweenPosition, new Vector3(0, 17));
                    }
                    else
                    {
                        GameHelper.PlayTweenPosition(tweenPosition, Vector3.zero);
                    }
                }
                else
                {
                    if (showSubTitle)
                    {
                        maintitle.localPosition = new Vector3(0, 17);
                    }
                    else
                    {
                        maintitle.localPosition = Vector3.zero;
                    }
                }
            }
            Transform subtitle = title.Find("subtitle");
            if (null != subtitle)
            {
                TweenAlpha tweenAlpha = subtitle.GetComponent<TweenAlpha>();
                if (null != tweenAlpha)
                {
                    tweenAlpha.onFinished.Clear();
                    if (showSubTitle)
                    {
                        subtitle.gameObject.SetActive(true);
                        GameHelper.SetTransformAlpha(subtitle, 0);
                        GameHelper.PlayTweenAlpha(tweenAlpha, 1);
                    }
                    else
                    {
                        if (subtitle.gameObject.activeSelf)
                        {
                            GameHelper.PlayTweenAlpha(tweenAlpha, 0);
                            tweenAlpha.SetOnFinished(delegate () { subtitle.gameObject.SetActive(false); });
                        }
                    }
                }
                else
                {
                    if (showSubTitle)
                    {
                        subtitle.gameObject.SetActive(true);
                    }
                    else
                    {
                        subtitle.gameObject.SetActive(false);
                    }
                }
            }
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

    void UpdateBtnOnSleep()
    {
        SetOnSleep(mConfirmBtnTrans);
        SetOnSleep(mReDetectBtnTrans);
        if (null != mTrans)
        {
            SetOnSleep(mTrans.Find("top/btnBack"));
        }
    }

    void UpdateBtnOnAwake()
    {
        SetOnAwake(mConfirmBtnTrans);
        SetOnAwake(mReDetectBtnTrans);
        if (null != mTrans)
        {
            SetOnAwake(mTrans.Find("top/btnBack"));
        }
    }

    void SetOnAwake(Transform trans)
    {
        if (null != trans)
        {
            UIButton btn = trans.GetComponent<UIButton>();
            if (null != btn)
            {
                btn.OnAwake();
            }
        }
    }

    void SetOnSleep(Transform trans)
    {
        if (null != trans)
        {
            UIButton btn = trans.GetComponent<UIButton>();
            if (null != btn)
            {
                btn.OnSleep();
            }
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
                    CheckUpdateData(true);
                    SetCheckUpdateInfo();
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
                isUpdateSuccess = true;
                if (PlatformMgr.Instance.GetBluetoothState())
                {
                    mRobot.RetrieveMotherboardData();
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                }
            }
            
            //SetBtnState();
        }
    }

    void PopPowerAdapterOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            CheckUpdateResult();
            CheckUpdateData(mCheckUpdateFlag);
            SetCheckUpdateInfo();
        }
    }

    void PopPowerLowOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            OnClose();
        }
    }

    void UpdateFinishedCallBack(EventArg arg)
    {
        try
        {
            TopologyPartType partType = (TopologyPartType)arg[0];
            isSuccess = true;
            
            if (TopologyPartType.MainBoard == partType)
            {
                mSystemUpdateResult = ErrorCode.Result_OK;
                Timer.Add(3, 1, 1, delegate () {
                    //必须等待一段时间才发送命令，否则握手命令不会回包
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        mRobot.RetrieveMotherboardData(false);
                    }
                    else
                    {
                        isUpdating = false;
                        SetCheckUpdateInfo();
                        UpdateFinishedAnim(true);
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                    }
                });
            }
            else
            {
                if (partType == TopologyPartType.Servo)
                {
                    mServoUpdateResult = ErrorCode.Result_OK;
                }
                else
                {
                    mSensorUpdateResult[partType] = ErrorCode.Result_OK;
                }
                if (CheckDeviceUpdateFinished())
                {
                    isUpdateSuccess = true;
                    if (null != mRobot)
                    {
                        Timer.Add(2, 1, 1, delegate ()
                        {
                            if (PlatformMgr.Instance.GetBluetoothState())
                            {
                                mRobot.RetrieveMotherboardData(false);
                            }
                            else
                            {
                                isUpdating = false;
                                SetCheckUpdateInfo();
                                UpdateFinishedAnim(true);
                                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                            }
                        });
                    }
                }
                else
                {
                    UpdateFinishedAnim(true);
                    Timer.Add(1f, 1, 1, delegate ()
                    {
                        CheckUpdateData(mCheckUpdateFlag);
                    });
                }
                /*isUpdating = false;
                SetCheckUpdateInfo();
                UpdateFinishedAnim(true);
                CheckUpdateResult();
                CheckUpdateData(mCheckUpdateFlag);*/
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void UpdateErrorCallBack(EventArg arg)
    {
        try
        {
            PlatformMgr.Instance.SetSendXTState(true);
            TopologyPartType partType = (TopologyPartType)arg[0];
            UpdateFinishedAnim(false);
            if (null != mRobot)
            {
                switch (partType)
                {
                    case TopologyPartType.MainBoard:
                        mRobot.RobotBlueUpdateStop();
                        break;
                    case TopologyPartType.Servo:
                        mRobot.StopServoUpdate();
                        break;
                    default:
                        mRobot.StopSensorUpdate(partType);
                        break;
                }
            }
            Timer.Add(1, 1, 1, delegate ()
            {
                isUpdating = false;
                SetCheckUpdateInfo();
                PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("升级异常！您是否需要重新升级？"), PopUpdateErrorOnClick, PopUpdateFailHelpOnClick);
            });

        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void UpdateFailCallBack(EventArg arg)
    {
        try
        {
            TopologyPartType partType = (TopologyPartType)arg[0];
            switch (partType)
            {
                case TopologyPartType.MainBoard:
                    UpdateFinishedAnim(false);
                    isUpdating = false;
                    SetCheckUpdateInfo();
                    break;
                case TopologyPartType.Servo:
                    List<byte> servoList = (List<byte>)arg[1];
                    UpdateFinishedAnim(false);
                    Timer.Add(1, 1, 1, delegate ()
                    {
                        isUpdating = false;
                        SetCheckUpdateInfo();
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机升级失败,您是否需要重新升级？"), PublicFunction.ListToString(servoList)), PopUpdateErrorOnClick, PopUpdateFailHelpOnClick);
                    });
                    break;
                default:
                    List<byte> sensorList = (List<byte>)arg[1];
                    UpdateFinishedAnim(false);
                    Timer.Add(1, 1, 1, delegate ()
                    {
                        isUpdating = false;
                        SetCheckUpdateInfo();
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("固件升级失败,您是否需要重新升级？"), PublicFunction.ListToString(sensorList)), PopUpdateErrorOnClick, PopUpdateFailHelpOnClick);
                    });
                    break;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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
                if (mFailReason != BlueConnectFailReason.unknow)
                {
                    mFailReason = BlueConnectFailReason.Disconnect;
                }
                else if (PlatformMgr.Instance.PowerData.power <= PublicFunction.Robot_Power_Empty)
                {
                    mFailReason = BlueConnectFailReason.LowPower;
                }
                if (isUpdating)
                {
                    isUpdating = false;
                    if (UpdateState.State_Success == SingletonObject<UpdateManager>.GetInst().GetUpdateState())
                    {
                        UpdateFinishedAnim(true);
                    }
                    else
                    {
                        UpdateFinishedAnim(false);
                    }
                    SetCheckUpdateInfo();
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void UpdateCannelCallBack(EventArg arg)
    {
        try
        {
            UpdateFinishedAnim(false, true);
            Timer.Add(0.2f, 1, 1, delegate ()
            {
                isUpdating = false;
                SetCheckUpdateInfo();
                PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("升级异常！您是否需要重新升级？"), PopUpdateErrorOnClick);
            });
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SaveTopologyCallBack(EventArg arg)
    {
        if (null != mTopologyUI)
        {
            mTopologyUI.SaveTopologyData();
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
            if (isSpeakerPromptFlag || isUpdating)
            {
                return;
            }
            SpeakerData speakerData = (SpeakerData)args[0];
            if (null != speakerData)
            {
                SpeakerInfoData infoData = speakerData.GetSpeakerData();
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
                    isSpeakerPromptFlag = true;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
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

    /// <summary>
    /// 舵机重复
    /// </summary>
    /// <param name="obj"></param>
    void PopServoRepeatHelpOnClick(GameObject obj)
    {
        PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionServoIdRepeatType);
    }
    /// <summary>
    /// 舵机ID不匹配
    /// </summary>
    /// <param name="obj"></param>
    void PopServoIDDoNotMatchHelpOnClick(GameObject obj)
    {
        PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionServoVSLineType);
    }
    /// <summary>
    /// 舵机数量不匹配
    /// </summary>
    /// <param name="obj"></param>
    void PopServoNumDoNotMatchHelpOnClick(GameObject obj)
    {
        PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionServoNumVsLineType);
    }
    /// <summary>
    /// 舵机连接异常
    /// </summary>
    /// <param name="obj"></param>
    void PopServoConnectionExceptionHelpOnClick(GameObject obj)
    {
        PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionServoLineErrorType);
    }
    /// <summary>
    /// 固件升级失败
    /// </summary>
    /// <param name="obj"></param>
    void PopUpdateFailHelpOnClick(GameObject obj)
    {
        PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionFirmwareUpdateErrorType);
    }

}


