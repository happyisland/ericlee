using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Platform;
using Game.Event;

/// <summary>
/// Author:xj
/// FileName:ServosConStateMsg.cs
/// Description:舵机连接情况界面
/// Time:2016/4/5 17:47:27
/// </summary>
public class ServosConStateMsg : BasePopWin
{
    #region 公有属性
    public enum ConStateMsgType
    {
        ConState_Default = 0,
        ConState_Playerdata,
    }
    #endregion

    #region 其他属性
    GameObject mServoPrefab;
    Dictionary<int, PortData> mPortDict;
    Vector2 mMotherBoxSize;
    Vector2 mServoSize;
    int mLineWidth = 120;
    ServosConnection mServosConnection;
    Robot mRobot;
    ConStateMsgType mConStateMsgType;
    Transform mSelfTrans;
    ReadMotherboardDataMsgAck mMotherboardData;
    Dictionary<GameObject, byte> mServoDict;

    Dictionary<Transform, UILabel> mUpdateDict;
    List<byte> mNeedUpdateServoList;

    PopInputMsg mRenameMsg;

    bool isUpdating;//正在升级
    bool isSuccess;//模型匹配成功了

    ErrorCode mSystemUpdateResult;//主板更新结果
    ErrorCode mServoUpdateResult;//舵机更新结果

    //UIButton mBackBtn;
    //UIButton mConfirmBtn;

    Transform mBackBtnTrans;
    Transform mLeftBtnTrans;
    Transform mRightBtnTrans;

    UISprite mMothStateSprite;

    static ServosConStateMsg mInst;

    TweenPosition mBackBtnTweenPosition;
    TweenPosition mRenameBtnTweenPosition;
    #endregion

    #region 公有函数
    public ServosConStateMsg(ReadMotherboardDataMsgAck data)
    {
        mInst = this;
        mUIResPath = "Prefab/UI/ServosConStateMsg";
        isSingle = true;
        mMotherboardData = data;
        mPortDict = new Dictionary<int, PortData>();
        mServoDict = new Dictionary<GameObject, byte>();
        mUpdateDict = new Dictionary<Transform, UILabel>();
        mNeedUpdateServoList = new List<byte>();
        isUpdating = false;
        isSuccess = false;
    }

    public static void ShowMsg(ReadMotherboardDataMsgAck data)
    {
        Debuger.Log(string.Format("mbvesion = {0} servo version = {1}", data.mbVersion, data.djVersion));
        if (null == mInst)
        {
            object[] args = new object[1];
            args[0] = data;
            PopWinManager.GetInst().ShowPopWin(typeof(ServosConStateMsg), args);
        }
        else
        {
            mInst.mMotherboardData = data;
            if (ConStateMsgType.ConState_Default == mInst.mConStateMsgType)
            {
                mInst.CheckDefaultData();
                mInst.UpdateServoState();
                mInst.SetBtnState();
            }
            else
            {
                mInst.SelfRefresh();
            }

        }

    }

    public override void Release()
    {
        base.Release();
        EventMgr.Inst.UnRegist(EventID.Update_Finished, UpdateFinishedCallBack);
        EventMgr.Inst.UnRegist(EventID.Update_Error, UpdateErrorCallBack);
        EventMgr.Inst.UnRegist(EventID.Update_Fail, UpdateFailCallBack);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
        EventMgr.Inst.UnRegist(EventID.Update_Progress, UpdateProgressResult);
        mInst = null;
        if (isSuccess)
        {
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                if (ClientMain.Use_Third_App_Flag || ClientMain.Simulation_Use_Third_App_Flag)
                {
                    SingletonBehaviour<GetRobotData>.Inst.CreateGO(mMotherboardData.ids, RobotManager.GetInst().GetCreateRobot().Name);
                }
                else
                {
                    AssembleMenu._instance.CreateGO(mMotherboardData.ids);
                }

                EventMgr.Inst.Fire(EventID.Set_Choice_Robot);
            }
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(true));
                Robot robot = RobotManager.GetInst().GetCurrentRobot();
                if (null != robot)
                {
                    robot.ReadMCUInfo();
                    robot.SelfCheck(true);
                    robot.canShowPowerFlag = true;
                    SensorData sensorData = mMotherboardData.GetSensorData(TopologyPartType.Infrared);
                    if (null != sensorData && sensorData.ids.Count > 0)
                    {
                        //robot.SensorInit(sensorData.ids);
                    }
                    robot.ReadConnectedAngle();
                }
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
        }
        SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
        RobotManager.GetInst().IsCreateRobotFlag = false;

        //ClientMain.GetInst().UseTestModelFlag = true;
        /*if (StepManager.GetIns().OpenOrCloseGuide)
        {
#if UNITY_EDITOR
            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().DJmsgConfirm, true));
#else
            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().DJmsgConfirm, PlatformMgr.Instance.GetBluetoothState()));
#endif
        }*/

    }
    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            EventMgr.Inst.Regist(EventID.Update_Finished, UpdateFinishedCallBack);
            EventMgr.Inst.Regist(EventID.Update_Error, UpdateErrorCallBack);
            EventMgr.Inst.Regist(EventID.Update_Fail, UpdateFailCallBack);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
            EventMgr.Inst.Regist(EventID.Update_Progress, UpdateProgressResult);
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                mConStateMsgType = ConStateMsgType.ConState_Playerdata;
            }
            else
            {
                mRobot = RobotManager.GetInst().GetCurrentRobot();
                if (null != mRobot)
                {
                    mServosConnection = ServosConManager.GetInst().GetServosConnection(mRobot.ID);
                    if (null != mServosConnection)
                    {
                        mConStateMsgType = ConStateMsgType.ConState_Default;
                    }
                    else
                    {
                        mConStateMsgType = ConStateMsgType.ConState_Playerdata;
                    }
                }
            }

            if (null != mTrans)
            {
                Transform btnBack = mTrans.Find("btnBack");
                if (null != btnBack)
                {
                    mBackBtnTweenPosition = btnBack.GetComponent<TweenPosition>();
                    if (null != mBackBtnTweenPosition)
                    {
                        Vector3 pos = UIManager.GetWinPos(btnBack, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        btnBack.localPosition = pos - new Vector3(300, 0);
                        GameHelper.PlayTweenPosition(mBackBtnTweenPosition, pos, 0.6f);
                    }
                    else
                    {
                        btnBack.localPosition = UIManager.GetWinPos(btnBack, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    }
                    //mBackBtn = btnBack.GetComponent<UIButton>();
                    mBackBtnTrans = btnBack;
                }

                Transform title = mTrans.Find("title");
                if (null != title)
                {
                    title.localPosition = UIManager.GetWinPos(title, UIWidget.Pivot.Top, 0, 60);
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(title, "Label");
                }

                Transform btnRename = mTrans.Find("btnRename");
                if (null != btnRename)
                {
                    mRenameBtnTweenPosition = btnRename.GetComponent<TweenPosition>();
                    if (null != mRenameBtnTweenPosition)
                    {
                        Vector3 pos = UIManager.GetWinPos(btnRename, UIWidget.Pivot.TopRight, 34, 70);
                        btnRename.localPosition = pos + new Vector3(300, 0);
                        GameHelper.PlayTweenPosition(mRenameBtnTweenPosition, pos, 0.6f);
                    }
                    else
                    {
                        btnRename.localPosition = UIManager.GetWinPos(btnRename, UIWidget.Pivot.TopRight, 34, 70);
                    }
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(btnRename, "Label");
                    if (null != lb)
                    {
                        lb.text = LauguageTool.GetIns().GetText("改名");
                    }
                }

                Transform bottombtn = mTrans.Find("bottombtn");
                if (null != bottombtn)
                {
                    bottombtn.localPosition = UIManager.GetWinPos(bottombtn, UIWidget.Pivot.Bottom, 0, 20);
                    Transform btnLeft = bottombtn.Find("btnLeft");
                    if (null != btnLeft)
                    {
                        mLeftBtnTrans = btnLeft;
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(btnLeft, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("重新检测");
                        }
                        /*if (ConStateMsgType.ConState_Default == mConStateMsgType)
                        {
                            btnLeft.gameObject.SetActive(false);
                        }
                        else
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(btnLeft, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("重新检测");
                            }
                        }*/
                    }
                    Transform btnRight = bottombtn.Find("btnRight");
                    if (null != btnRight)
                    {
                        mRightBtnTrans = btnRight;
                        //mConfirmBtn = btnRight.GetComponent<UIButton>();
                        /*if (ConStateMsgType.ConState_Default == mConStateMsgType)
                        {
                            btnRight.localPosition = Vector2.zero;
                        }*/
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(btnRight, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("确定");
                        }
                    }
                }
                Transform servo = mTrans.Find("servo");
                if (null != servo)
                {
                    mServoPrefab = servo.gameObject;
                    if (null != mServoPrefab)
                    {
                        mServoSize = NGUIMath.CalculateRelativeWidgetBounds(mServoPrefab.transform).size;
                        mServoPrefab.SetActive(false);
                    }

                }
                Transform bgTrans = mTrans.Find("bg");
                if (null != bgTrans)
                {
                    UISprite sp = bgTrans.GetComponent<UISprite>();
                    if (null != sp)
                    {
                        sp.width = PublicFunction.GetWidth() + 4;
                        sp.height = PublicFunction.GetHeight() + 4;
                    }
                    BoxCollider box = bgTrans.GetComponent<BoxCollider>();
                    if (null != box)
                    {
                        box.size = new Vector2(PublicFunction.GetWidth() + 4, PublicFunction.GetHeight() + 4);
                    }
                }
                Transform panel = mTrans.Find("panel");
                if (null != panel)
                {//官方
                    if (ConStateMsgType.ConState_Default == mConStateMsgType)
                    {
                        UIPanel uiPanel = panel.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            uiPanel.depth = mDepth + 1;
                            Vector4 rect = uiPanel.finalClipRegion;
                            rect.z = PublicFunction.GetWidth() - 20;
                            uiPanel.baseClipRegion = rect;
                        }
                        Transform grid = panel.Find("grid");
                        if (null != grid)
                        {
                            mMothStateSprite = GameHelper.FindChildComponent<UISprite>(grid, "state");
                            UISprite bg = GameHelper.FindChildComponent<UISprite>(grid, "MotherBox/bg");
                            if (null != bg)
                            {
                                mMotherBoxSize = new Vector2(bg.width, bg.height);
                            }
                            List<int> list = null;
                            if (null != mServosConnection)
                            {
                                list = mServosConnection.GetPortList();
                            }

                            Transform motherBox = grid.Find("MotherBox");
                            if (null != motherBox && null != list)
                            {
                                UILabel lb = GameHelper.FindChildComponent<UILabel>(motherBox, "Label");
                                if (null != lb)
                                {
                                    lb.text = LauguageTool.GetIns().GetText("主控盒");
                                }
                                for (int i = 0, imax = list.Count; i < imax; ++i)
                                {
                                    Transform port = motherBox.Find("port" + list[i]);
                                    if (null != port)
                                    {
                                        port.gameObject.SetActive(true);
                                    }
                                }
                            }

                            Transform portList = grid.Find("portList");
                            if (null != portList && null != list)
                            {
                                for (int i = 0, imax = list.Count; i < imax; ++i)
                                {
                                    Transform port = portList.Find("port" + list[i]);
                                    if (null != port)
                                    {
                                        port.gameObject.SetActive(true);
                                        PortData data = new PortData();
                                        data.trans = port;
                                        if (list[i] <= 3)
                                        {
                                            data.offsetX = -(mServoSize.x + mLineWidth);
                                        }
                                        else
                                        {
                                            data.offsetX = mServoSize.x + mLineWidth;
                                        }
                                        data.offsetY = 0;
                                        /*if (list[i] == 2)
                                        {
                                            data.offsetX = -(mServoSize.x + mLineWidth);
                                        }
                                        else if (list[i] == 5)
                                        {
                                            data.offsetX = mServoSize.x + mLineWidth;
                                        }
                                        else
                                        {
                                            data.offsetX = 0;
                                        }
                                        if (list[i] == 1)
                                        {
                                            data.offsetY = mServoSize.y + mLineWidth;
                                        }
                                        else if (list[i] == 3 || list[i] == 4)
                                        {
                                            data.offsetY = -(mServoSize.y + mLineWidth);
                                        }
                                        else
                                        {
                                            data.offsetY = 0;
                                        }*/
                                        data.count = 0;
                                        mPortDict[list[i]] = data;
                                    }
                                }
                            }
                        }
                        InitDefaultUI();
                    }
                    else
                    {
                        panel.gameObject.SetActive(false);
                    }

                }

                Transform panelself = mTrans.Find("panelself");
                if (null != panelself)
                {
                    if (ConStateMsgType.ConState_Default == mConStateMsgType)
                    {
                        panelself.gameObject.SetActive(false);
                    }
                    else
                    {
                        panelself.localPosition = UIManager.GetWinPos(panelself, UIWidget.Pivot.Bottom);
                        UISprite bg = GameHelper.FindChildComponent<UISprite>(panelself, "bg");
                        if (null != bg)
                        {
                            bg.width = PublicFunction.GetExtendWidth();
                            UISprite shandow = GameHelper.FindChildComponent<UISprite>(bg.transform, "shandow");
                            if (null != shandow)
                            {
                                shandow.width = bg.width;
                            }
                        }
                        Transform motherBox = panelself.Find("MotherBox");
                        if (null != motherBox)
                        {
                            motherBox.localPosition = UIManager.GetWinPos(motherBox, UIWidget.Pivot.Center, 0, 100) - panelself.localPosition;
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(motherBox, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("主控盒");
                            }
                        }
                        Transform grid = panelself.Find("grid");
                        if (null != grid)
                        {
                            mSelfTrans = grid;

                            Vector3 pos = mSelfTrans.localPosition;
                            pos.y = 130;
                            pos.x = -PublicFunction.GetWidth() / 2 + PublicFunction.Back_Btn_Pos.x;
                            mSelfTrans.localPosition = pos;
                            //适配裁剪区域
                            UIPanel uiPanel = grid.GetComponent<UIPanel>();
                            if (null != uiPanel)
                            {
                                uiPanel.depth = mDepth + 1;
                                Vector4 rect = uiPanel.finalClipRegion;
                                rect.z = PublicFunction.GetWidth() - PublicFunction.Back_Btn_Pos.x * 2;
                                rect.x = rect.z / 2;
                                uiPanel.baseClipRegion = rect;
                            }
                        }
                        InitPlayerDataUI();
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


    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            string btnName = obj.name;
            if (btnName.Equals("btnRight"))
            {//关闭拓扑图
                OnClose();
            }
            else if (btnName.Equals("btnBack"))
            {//返回
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                }
                else
                {
                    OnClose();
                }
            }
            else if (btnName.Equals("btnLeft"))
            {//重新获取主板信息
                if (isUpdating)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                    return;
                }
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    Robot robot = RobotManager.GetInst().GetCreateRobot();
                    if (PlatformMgr.Instance.GetBluetoothState())
                    {
                        robot.RetrieveMotherboardData();
                    }
                    else
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙断开"));
                    }
                }
                else
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
            else if (btnName.Equals("btnRename"))
            {//重命名
                string titleText = LauguageTool.GetIns().GetText("请输入名字");
                mRenameMsg = PopInputMsg.ShowPopInputMsg(titleText, titleText, string.Empty, PopRenameMsgOnClick);

                if (null != mRenameMsg)
                {
                    mRenameMsg.SetOnSelect(OnInputSelect);
                    mRenameMsg.SetOnValidate(OnValidate);
                    if (null != mRenameMsg.mRightBtn)
                    {
                        mRenameMsg.mRightBtn.OnSleep();
                    }
                }
            }
            else if (btnName.Equals("MotherBox"))
            {//点击了主板
                if (!PlatformMgr.Instance.Robot_System_Version.Equals(mMotherboardData.mbVersion))
                {
                    if (isUpdating)
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("升级中，请稍后！"));
                    }
                }
            }
            else if (btnName.StartsWith("servo-"))
            {//点击了某个舵机
                byte id = byte.Parse(btnName.Substring("servo-".Length));
                if (mMotherboardData.errorIds.Contains(id))
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
                        PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), id));
                    }
                }
                else if (mMotherboardData.errorVerIds.Contains(id))
                {//舵机版本不一致
                    if (isUpdating)
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
                    }
                    else
                    {
                        PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), id));
                    }
                }
                else if (!mMotherboardData.ids.Contains(id))
                {//舵机不存在，检查连接
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机连接异常"), id), PopRetrieveMotherboardDataOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
                else if (!string.IsNullOrEmpty(PlatformMgr.Instance.Robot_Servo_Version) && !PlatformMgr.Instance.Robot_Servo_Version.Equals(mMotherboardData.djVersion))
                {
                    if (isUpdating)
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机升级中，请稍后！"));
                    }
                    else
                    {
                        PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), id));
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

    //////////////////////////////////////////////////////////////////////////
    void InitDefaultUI()
    {
        if (null != mServosConnection && null != mServoPrefab)
        {
            mServoDict.Clear();
            List<int> portList = mServosConnection.GetPortList();
            if (null != portList)
            {
                for (int portIndex = 0, portMax = portList.Count; portIndex < portMax; ++portIndex)
                {
                    if (mPortDict.ContainsKey(portList[portIndex]))
                    {
                        PortData portData = mPortDict[portList[portIndex]];
                        List<int> servoList = mServosConnection.GetServosForPort(portList[portIndex]);
                        if (null != servoList)
                        {
                            for (int servoIndex = 0, servoMax = servoList.Count; servoIndex < servoMax; ++servoIndex)
                            {
                                GameObject obj = GameObject.Instantiate(mServoPrefab) as GameObject;
                                mServoDict[obj] = (byte)servoList[servoIndex];
                                obj.name = "servo-" + servoList[servoIndex];
                                obj.SetActive(true);
                                obj.transform.parent = portData.trans;
                                obj.transform.localScale = Vector3.one;
                                obj.transform.localEulerAngles = Vector3.zero;
                                obj.transform.localPosition = new Vector3(portData.count * portData.offsetX, portData.count * portData.offsetY);

                                UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
                                if (null != lb)
                                {
                                    lb.text = "ID " + servoList[servoIndex];
                                }

                                Transform line = obj.transform.Find("line");
                                if (null != line)
                                {
                                    if (servoIndex == 0)
                                    {
                                        line.gameObject.SetActive(false);
                                    }
                                    else
                                    {
                                        UISprite sp = line.GetComponent<UISprite>();
                                        if (null != sp)
                                        {
                                            sp.height = mLineWidth + 28;
                                        }
                                        if (portData.offsetX < -0.1f)
                                        {
                                            line.localPosition = new Vector3(mServoSize.x / 2 + mLineWidth / 2, 0);
                                        }
                                        else if (portData.offsetX > 0.1f)
                                        {
                                            line.localPosition = new Vector3(-mServoSize.x / 2 - mLineWidth / 2, 0);
                                        }
                                        if (portData.offsetY < -0.1f)
                                        {
                                            line.localPosition = new Vector3(0, -mServoSize.y / 2 - mLineWidth / 2);
                                            line.localEulerAngles = new Vector3(0, 0, 90);
                                        }
                                        else if (portData.offsetY > 0.1f)
                                        {
                                            line.localPosition = new Vector3(0, mServoSize.y / 2 + mLineWidth / 2);
                                            line.localEulerAngles = new Vector3(0, 0, 90);
                                        }
                                    }
                                }
                                portData.count++;
                            }
                        }
                    }
                }
                
                UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
            }
        }
        CheckDefaultData();
        UpdateServoState();
        SetBtnState();
    }

    void UpdateServoState()
    {
        foreach (KeyValuePair<GameObject, byte> kvp in mServoDict)
        {
            SetServoState(kvp.Key.transform, kvp.Value);
        }
    }



    void InitPlayerDataUI()
    {
        if (null != mServoPrefab && null != mSelfTrans)
        {
            mServoDict.Clear();
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                List<byte> sameServoList = mMotherboardData.errorIds;
                int index = 0;
                for (int i = 0, imax = sameServoList.Count; i < imax; ++i)
                {
                    AddSelfServo(sameServoList[i], index);
                    ++index;
                }
                List<byte> servoList = mMotherboardData.ids;
                for (int i = 0, imax = servoList.Count; i < imax; ++i)
                {
                    AddSelfServo(servoList[i], index);
                    ++index;
                }
            }
            else if (null != mRobot)
            {
                List<byte> servoList = mRobot.GetAllDjData().GetIDList();
                for (int i = 0, imax = servoList.Count; i < imax; ++i)
                {
                    AddSelfServo(servoList[i], i);
                }
            }

            UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
        }
        CheckSelfData();
        UpdateServoState();
        SetBtnState();
    }

    void AddSelfServo(byte id, int index)
    {
        GameObject obj = GameObject.Instantiate(mServoPrefab) as GameObject;
        obj.transform.parent = mSelfTrans;
        Transform line = obj.transform.Find("line");
        if (null != line)
        {
            line.gameObject.SetActive(false);
        }
        SetSelfServoData(id, index, obj.transform);
    }

    void SetSelfServoData(byte id, int index, Transform trans)
    {
        trans.gameObject.SetActive(true);
        trans.name = "servo-" + id;
        trans.transform.localScale = Vector3.one;
        trans.transform.localEulerAngles = Vector3.zero;
        trans.transform.localPosition = new Vector3(mServoSize.x / 2 + index * (mServoSize.x + 46), 0);
        UILabel lb = GameHelper.FindChildComponent<UILabel>(trans, "Label");
        if (null != lb)
        {
            lb.text = "ID " + id;
        }
        mServoDict[trans.gameObject] = id;
        //SetServoState(trans, id);
    }
    void SetServoState(Transform trans, byte id)
    {
        bool state = true;
        if (mMotherboardData.errorIds.Contains(id))
        {
            state = false;
        }
        else if (mMotherboardData.errorVerIds.Contains(id))
        {
            if (mNeedUpdateServoList.Contains(id))
            {
                if (ErrorCode.Result_OK == mServoUpdateResult)
                {
                    return;
                }
                else
                {
                    state = false;
                }
            }
            else
            {
                state = false;
            }
        }
        else if (!mMotherboardData.ids.Contains(id))
        {
            state = false;
        }
        else if (mNeedUpdateServoList.Contains(id))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(PlatformMgr.Instance.Robot_Servo_Version) && !PlatformMgr.Instance.Robot_Servo_Version.Equals(mMotherboardData.djVersion))
        {
            state = false;
        }
        Transform stateTrans = trans.Find("state");
        if (null != stateTrans)
        {
            UISprite bgSp = GameHelper.FindChildComponent<UISprite>(stateTrans, "bg");
            UISprite iconSp = GameHelper.FindChildComponent<UISprite>(stateTrans, "icon");
            if (null != bgSp)
            {
                if (state)
                {
                    bgSp.spriteName = "success";
                }
                else
                {
                    bgSp.spriteName = "Unsuccessful";
                }
                bgSp.MyMakePixelPerfect();
            }
            if (null != iconSp)
            {
                if (state)
                {
                    iconSp.spriteName = "yes";
                }
                else
                {
                    iconSp.spriteName = "no";
                }
                iconSp.MyMakePixelPerfect();
            }
        }
    }

    void SelfRefresh()
    {
        if (null != mServoPrefab && null != mSelfTrans)
        {
            mServoDict.Clear();
            int index = 0;
            int oldCount = mSelfTrans.childCount;
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                List<byte> sameServoList = mMotherboardData.errorIds;
                for (int i = 0, imax = sameServoList.Count; i < imax; ++i)
                {
                    if (index < oldCount)
                    {
                        SetSelfServoData(sameServoList[i], index, mSelfTrans.GetChild(index));
                    }
                    else
                    {
                        AddSelfServo(sameServoList[i], index);
                    }
                    ++index;
                }
                List<byte> servoList = mMotherboardData.ids;
                for (int i = 0, imax = servoList.Count; i < imax; ++i)
                {
                    if (index < oldCount)
                    {
                        SetSelfServoData(servoList[i], index, mSelfTrans.GetChild(index));
                    }
                    else
                    {
                        AddSelfServo(servoList[i], index);
                    }
                    ++index;
                }
            }
            else if (null != mRobot)
            {
                List<byte> servoList = mRobot.GetAllDjData().GetIDList();
                for (int i = 0, imax = servoList.Count; i < imax; ++i)
                {
                    if (index < oldCount)
                    {
                        SetSelfServoData(servoList[i], index, mSelfTrans.GetChild(index));
                    }
                    else
                    {
                        AddSelfServo(servoList[i], index);
                    }
                    ++index;
                }
            }

            if (index < oldCount)
            {
                for (int i = index; i < oldCount; ++i)
                {
                    Transform child = mSelfTrans.GetChild(i);
                    child.gameObject.SetActive(false);
                }
            }
            UIManager.SetButtonEventDelegate(mTrans, mBtnDelegate);
        }
        CheckSelfData();
        UpdateServoState();
        SetBtnState();
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
            List<byte> list = mRobot.GetAllDjData().GetIDList();
            if (list.Count != mMotherboardData.ids.Count)
            {
                ret = ErrorCode.Result_Servo_Num_Inconsistent;
                break;
            }
            for (int i = 0, icount = list.Count; i < icount; ++i)
            {
                if (list[i] != mMotherboardData.ids[i])
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

    void CheckUpdateData()
    {
        isSuccess = false;
        isUpdating = false;
        PlatformMgr.Instance.NeedUpdateFlag = false;
        mUpdateDict.Clear();
        mNeedUpdateServoList.Clear();
        Robot robot = mRobot;
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            robot = RobotManager.GetInst().GetCreateRobot();
        }
        if (ErrorCode.Do_Not_Upgrade == mServoUpdateResult)
        {//一切正常
        }
        else if (ErrorCode.Robot_Adapter_Open_Protect == mServoUpdateResult)
        {//有升级，且开启了充电保护，不断开
            isSuccess = true;
            PlatformMgr.Instance.NeedUpdateFlag = true;
            PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
        }
        else if (ErrorCode.Robot_Adapter_Close_Protect == mServoUpdateResult)
        {//有升级，未开充电保护，断开
            PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
            //msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
        }
        else if (ErrorCode.Robot_Power_Low == mServoUpdateResult)
        {//电量过低，不升级
            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机版本不一致且设备电量过低"));
        }
        else
        {//需要升级
            byte needUpdateId = 0;
            if (mMotherboardData.errorVerIds.Count == 1 && mMotherboardData.djVersion.Equals(PlatformMgr.Instance.Robot_Servo_Version))
            {
                needUpdateId = mMotherboardData.errorVerIds[0];
            }
            if (robot.ServoUpdate(needUpdateId))
            {
                isUpdating = true;
                foreach (KeyValuePair<GameObject, byte> kvp in mServoDict)
                {
                    if (0 == needUpdateId || kvp.Value == needUpdateId)
                    {
                        Transform stateTrans = kvp.Key.transform.Find("state");
                        if (null != stateTrans)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(stateTrans, "Label");
                            if (null != lb)
                            {
                                mUpdateDict[stateTrans] = lb;
                            }
                        }
                        mNeedUpdateServoList.Add(kvp.Value);
                    }
                }
                OpenUpdateAnim();
            }
            else
            {
                mServoUpdateResult = ErrorCode.Do_Not_Upgrade;
            }
        }

        if (ErrorCode.Do_Not_Upgrade != mServoUpdateResult)
        {//有更新
        }
        else if (mMotherboardData.errorVerIds.Count > 0)
        {
            PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), PublicFunction.ListToString(mMotherboardData.errorVerIds)), PopRetrieveMotherboardDataOnClick);
            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
        }
        else
        {//
            if (ErrorCode.Do_Not_Upgrade == mSystemUpdateResult)
            {//一切正常
                isSuccess = true;
            }
            else if (ErrorCode.Robot_Adapter_Open_Protect == mSystemUpdateResult)
            {//有升级，且开启了充电保护，不断开
                isSuccess = true;
                PlatformMgr.Instance.NeedUpdateFlag = true;
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
            }
            else if (ErrorCode.Robot_Adapter_Close_Protect == mSystemUpdateResult)
            {//有升级，未开充电保护，断开
                PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("充电状态下不能升级"), PopPowerLowOnClick);
                //msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            }
            else if (ErrorCode.Robot_Power_Low == mSystemUpdateResult)
            {//电量过低，不升级
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("检测到主板程序有更新且设备电量过低"));
            }
            else
            {//需要升级
                if (robot.RobotBlueUpdate())
                {
                    isUpdating = true;
                    Transform stateTrans = null;
                    if (ConStateMsgType.ConState_Default == mConStateMsgType)
                    {
                        stateTrans = mTrans.Find("panel/grid/MotherBox/state");
                    }
                    else
                    {
                        stateTrans = mTrans.Find("panelself/MotherBox/state");
                    }
                    if (null != stateTrans)
                    {
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(stateTrans, "Label");
                        if (null != lb)
                        {
                            mUpdateDict[stateTrans] = lb;
                        }
                    }
                    OpenUpdateAnim();
                }
                else
                {
                    mSystemUpdateResult = ErrorCode.Do_Not_Upgrade;
                    isSuccess = true;
                }
            } 
            /*if (robot.CheckSystemVersion(mMotherboardData.mbVersion))
            {//检测主板更新
                isUpdating = true;
            }
            else
            {//一切正常
                isSuccess = true;
            }*/
        }
        if (isSuccess)
        {
            PlatformMgr.Instance.SaveLastConnectedData(robot.Mac);
        }
    }

    void CheckDefaultData()
    {
        mServoUpdateResult = Robot.CheckServoUpdate(mMotherboardData);
        mSystemUpdateResult = Robot.CheckSystemUpdate(mMotherboardData.mbVersion);
        if (mMotherboardData.errorIds.Count > 0)
        {//舵机id重复
            PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机ID重复，请修改舵机ID"), PublicFunction.ListToString(mMotherboardData.errorIds)), PopRetrieveMotherboardDataOnClick);
            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
            //PlatformMgr.Instance.DisConnenctBuletooth();
        }
        else
        {
            if (null != mRobot)
            {
                ErrorCode ret = CompareServoData();
                if (ErrorCode.Result_Servo_Num_Inconsistent == ret)
                {//舵机数量不一致
                    //PlatformMgr.Instance.DisConnenctBuletooth();
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("舵机数量跟拓扑图不一致，请检查后重试！"), PopRetrieveMotherboardDataOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
                else if (ErrorCode.Result_Servo_ID_Inconsistent == ret)
                {//舵机id不匹配
                    //PlatformMgr.Instance.DisConnenctBuletooth();
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("舵机ID跟拓扑图不一致，请检查后重试！"), PopRetrieveMotherboardDataOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
                else
                {//模型已匹配
                    CheckUpdateData();
                }
            }
        }
        if (mSystemUpdateResult == ErrorCode.Do_Not_Upgrade)
        {
            SetMainboardState(true);
        }
        else if (mSystemUpdateResult != ErrorCode.Result_OK)
        {
            SetMainboardState(false);
        }
    }


    void CheckSelfData()
    {
        mServoUpdateResult = Robot.CheckServoUpdate(mMotherboardData);
        mSystemUpdateResult = Robot.CheckSystemUpdate(mMotherboardData.mbVersion);
        if (mMotherboardData.errorIds.Count > 0)
        {//舵机id重复
            PromptMsg msg = PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机ID重复，请修改舵机ID"), PublicFunction.ListToString(mMotherboardData.errorIds)), PopRetrieveMotherboardDataOnClick);
            msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
        }
        else
        {
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {//创建模型
                if (mMotherboardData.ids.Count < 1)
                {
                    PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("WuDuoJi"), PopRetrieveMotherboardDataOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                }
                else
                {
                    CheckUpdateData();
                }
            }
            else
            {
                if (null != mRobot)
                {
                    ErrorCode ret = CompareServoData();
                    if (ErrorCode.Result_Servo_Num_Inconsistent == ret)
                    {//舵机数量不一致
                        //PlatformMgr.Instance.DisConnenctBuletooth();
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("舵机数量跟个人模型不一致，请检查后重试！"), PopRetrieveMotherboardDataOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                    else if (ErrorCode.Result_Servo_ID_Inconsistent == ret)
                    {//舵机id不匹配
                        //PlatformMgr.Instance.DisConnenctBuletooth();
                        PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("舵机ID跟个人模型不匹配，请检查后重试！"), PopRetrieveMotherboardDataOnClick);
                        msg.SetRightBtnText(LauguageTool.GetIns().GetText("已修复"));
                    }
                    else
                    {//模型已匹配
                        CheckUpdateData();
                    }
                }
            }
        }
        if (mSystemUpdateResult == ErrorCode.Do_Not_Upgrade)
        {
            SetMainboardState(true);
        }
        else if (mSystemUpdateResult != ErrorCode.Result_OK)
        {
            SetMainboardState(false);
        }
    }

    void SetBtnState()
    {
        if (null != mLeftBtnTrans && null != mRightBtnTrans)
        {
            if (isSuccess)
            {
                Vector3 leftPos = mLeftBtnTrans.localPosition;
                leftPos.x = -mRightBtnTrans.localPosition.x;
                mLeftBtnTrans.localPosition = leftPos;
                mRightBtnTrans.gameObject.SetActive(true);
            }
            else
            {
                mLeftBtnTrans.localPosition = Vector3.zero;
                mRightBtnTrans.gameObject.SetActive(false);
            }
        }
    }

    void SetMainboardState(bool state)
    {
        Transform stateTrans = null;
        UISprite sp;
        if (ConStateMsgType.ConState_Default == mConStateMsgType)
        {
            stateTrans = mTrans.Find("panel/grid/MotherBox/state");
        }
        else
        {
            stateTrans = mTrans.Find("panelself/MotherBox/state");
        }
        UISprite bgSp = GameHelper.FindChildComponent<UISprite>(stateTrans, "bg");
        UISprite iconSp = GameHelper.FindChildComponent<UISprite>(stateTrans, "icon");
        if (null != bgSp)
        {
            if (state)
            {
                bgSp.spriteName = "success";
            }
            else
            {
                bgSp.spriteName = "Unsuccessful";
            }
            bgSp.MyMakePixelPerfect();
        }
        if (null != iconSp)
        {
            if (state)
            {
                iconSp.spriteName = "yes";
            }
            else
            {
                iconSp.spriteName = "no";
            }
            iconSp.MyMakePixelPerfect();
        }
    }


    void OnInputSelect(bool isSelect, GameObject obj)
    {
        try
        {
            if (!isSelect)
            {
                /*if (null != mRenameMsg && null != mRenameMsg.mInput)
                {
                    if (!string.IsNullOrEmpty(mRenameMsg.mInput.value) && !mRenameMsg.mInput.value.Equals(PlatformMgr.Instance.GetMotherboardName()))
                    {
                        if (null != mRenameMsg.mRightBtn)
                        {
                            mRenameMsg.mRightBtn.OnAwake();
                        }
                    }
                    else
                    {
                        if (null != mRenameMsg.mRightBtn)
                        {
                            mRenameMsg.mRightBtn.OnSleep();
                        }
                    }
                }*/
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

    void OpenUpdateAnim()
    {
        foreach (KeyValuePair<Transform, UILabel> kvp in mUpdateDict)
        {
            StateUpdateStart(kvp.Key);
        }
    }

    void UpdateFinishedAnim(bool state)
    {
        foreach (KeyValuePair<Transform, UILabel> kvp in mUpdateDict)
        {
            StateUpdateFinished(kvp.Key, state);
        }
    }

    void StateUpdateStart(Transform stateTrans)
    {
        UISprite bg = GameHelper.FindChildComponent<UISprite>(stateTrans, "bg");
        if (null != bg)
        {
            bg.spriteName = "update";
            bg.MyMakePixelPerfect();
            TweenRotation tweenRota = bg.gameObject.GetComponent<TweenRotation>();
            bg.transform.localEulerAngles = new Vector3(0, 0, 360);
            tweenRota.enabled = true;
            tweenRota.ResetToBeginning();
            tweenRota.duration = 1.5f;
            tweenRota.from = new Vector3(0, 0, 360);
            tweenRota.to = Vector3.zero;
            tweenRota.Play(true);
        }
        Transform icon = stateTrans.Find("icon");
        if (null != icon)
        {
            icon.gameObject.SetActive(false);
        }
        Transform label = stateTrans.Find("Label");
        if (null != label)
        {
            label.gameObject.SetActive(true);
        }
    }

    void StateUpdateFinished(Transform stateTrans, bool state)
    {
        UISprite bg = GameHelper.FindChildComponent<UISprite>(stateTrans, "bg");
        if (null != bg)
        {
            if (state)
            {
                bg.spriteName = "success";
            }
            else
            {
                bg.spriteName = "Unsuccessful";
            }
            bg.MyMakePixelPerfect();
            TweenRotation tweenRota = bg.gameObject.GetComponent<TweenRotation>();
            if (null != tweenRota)
            {
                tweenRota.enabled = false;
            }
        }
        Transform label = stateTrans.Find("Label");
        if (null != label)
        {
            label.gameObject.SetActive(false);
        }
        Transform icon = stateTrans.Find("icon");
        if (null != icon)
        {
            icon.gameObject.SetActive(true);
            UISprite sp = icon.GetComponent<UISprite>();
            if (null != sp)
            {
                if (state)
                {
                    sp.spriteName = "yes";
                }
                else
                {
                    sp.spriteName = "no";
                }
                sp.MyMakePixelPerfect();
            }
            icon.localScale = Vector3.zero;
            TweenScale tweenScale = icon.GetComponent<TweenScale>();
            GameHelper.PlayTweenScale(tweenScale, Vector3.one, 1);
        }
    }

    void PopRenameMsgOnClick(GameObject obj)
    {
        if (obj.name.Equals(PopInputMsg.LeftBtnName))
        {
            if (null != mRenameMsg)
            {
                mRenameMsg = null;
            }
        }
        else if (obj.name.Equals(PopInputMsg.RightBtnName))
        {
            if (null != mRenameMsg)
            {
                if (null != mRenameMsg.mInput)
                {
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(mTrans, "title/Label");
                    if (null != lb)
                    {
                        lb.text = mRenameMsg.mInput.value;
                    }
                    /*PlayerPrefs.SetString(PlatformMgr.Instance.GetConnectedMac(), mRenameMsg.mInput.value);
                    PlayerPrefs.Save();*/
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改成功"), HUDTextTips.Color_Green);
                }
                mRenameMsg = null;
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
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    Robot robot = RobotManager.GetInst().GetCreateRobot();
                    robot.RetrieveMotherboardData();
                }
                else
                {
                    if (null != mRobot)
                    {
                        mRobot.RetrieveMotherboardData();
                    }
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
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    Robot robot = RobotManager.GetInst().GetCreateRobot();
                    robot.RetrieveMotherboardData();
                }
                else
                {
                    if (null != mRobot)
                    {
                        mRobot.RetrieveMotherboardData();
                    }
                }
            }
        }
    }

    void PopUpdateSuccessOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                if (RobotManager.GetInst().IsCreateRobotFlag)
                {
                    Robot robot = RobotManager.GetInst().GetCreateRobot();
                    robot.RetrieveMotherboardData();
                }
                else
                {
                    if (null != mRobot)
                    {
                        mRobot.RetrieveMotherboardData();
                    }
                }
            }
            //SetBtnState();
        }
    }

    void PopPowerLowOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            mServoUpdateResult = Robot.CheckServoUpdate(mMotherboardData);
            mSystemUpdateResult = Robot.CheckSystemUpdate(mMotherboardData.mbVersion);
            CheckUpdateData();
        }
    }

    void UpdateFinishedCallBack(EventArg arg)
    {
        try
        {
            bool updateSystem = (bool)arg[0];
            UpdateFinishedAnim(true);
            Timer.Add(1, 1, 1, delegate () {
                isUpdating = false;
                isSuccess = true;
                if (updateSystem || mSystemUpdateResult == ErrorCode.Do_Not_Upgrade)
                {
                    PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("设备升级成功"), PopUpdateSuccessOnClick);
                }
                else
                {
                    mMotherboardData.djVersion = PlatformMgr.Instance.Robot_Servo_Version;
                    mMotherboardData.errorVerIds.Clear();
                    mServoUpdateResult = Robot.CheckServoUpdate(mMotherboardData);
                    mSystemUpdateResult = Robot.CheckSystemUpdate(mMotherboardData.mbVersion);
                    CheckUpdateData();
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

    void UpdateProgressResult(EventArg arg)
    {
        try
        {
            int progress = (int)arg[0];
            foreach (KeyValuePair<Transform, UILabel> kvp in mUpdateDict)
            {
                kvp.Value.text = string.Format("{0}%", progress);
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
    #endregion



    class PortData
    {
        public float offsetX;
        public float offsetY;
        public int count;
        public Transform trans;

        public PortData()
        {
            offsetX = 0;
            offsetY = 0;
            count = 0;
            trans = null;
        }
    }
}