using Game;
using Game.Event;
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ConnectBluetoothMsg.cs
/// Description:连接蓝牙页面
/// Time:2016/12/2 11:21:36
/// </summary>
public class ConnectBluetoothMsg : BasePopWin
{
    #region 公有属性

    enum ConnectMsgType : byte
    {
        Connect_Msg_None,
        Connect_Msg_Search_Ready,
        Connect_Msg_Search,
        Connect_Msg_Select,
        Connect_Msg_Into_Select_Ing,
        Connect_Msg_Connecting,
    }

    public delegate void CloseCallBack();
    public CloseCallBack OnCloseCallBack;
    #endregion

    #region 其他属性
    static ConnectBluetoothMsg sInst;
    ConnectMsgType mMsgType = ConnectMsgType.Connect_Msg_None;
    long mControllerFlashIndex = -1;
    UISprite mLightSprite = null;
    TweenAlpha mLightTweenAlpha = null;
    Vector3 mControllerPosition = Vector3.zero;
    Vector3 mPhonePosition = Vector3.zero;
    UITexture[] mSearchTexture = null;
    bool mSearchUpdateFlag = false;
    int mSearchTextureIndex = 0;
    float mSearchTime = 0;

    long mSearchBlueIndex = -1;
    long mCheckSearchIndex = -1;

    List<DeviceInfo> mCacheDeviceList = new List<DeviceInfo>();
    Dictionary<GameObject, DeviceInfo> mDeviceDict = new Dictionary<GameObject, DeviceInfo>();
    Dictionary<string, GameObject> mDeviceForMacDict = new Dictionary<string, GameObject>();

    Vector3 mRefreshBtnPosition = Vector3.zero;
    Vector3 mSelectBluePosition = Vector3.zero;

    GameObject mBlueItem = null;
    Transform mBlueGridTrans = null;
    Vector2 mBlueItemSize = Vector2.one;
    GameObject mConnectingObj;

    Vector2 mConnectingLineSize = Vector2.one;
    GameObject mLineItem;
    Transform[] mConnectLineAry = null;
    bool isMoveConnectLine = false;
    Vector3 mLineMoveSpeed = new Vector3(0, 1);
    int mLineHeight;
    float mLineUpdateTime;
    #endregion

    #region 公有函数

    public ConnectBluetoothMsg()
    {
        mUIResPath = "Prefab/UI/ConnectBluetoothMsg";
        sInst = this;
    }

    public static void ShowMsg()
    {
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "未断开连接进入蓝牙连接页面");
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
        if (null == sInst)
        {
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(ConnectBluetoothMsg));
        }
        else
        {
            sInst.OnShow();
        }
    }

    public static void CloseMsg()
    {
        if (null != sInst)
        {
            sInst.OnClose();
        }
    }

    public static void ConnectFail(string text)
    {
        if (null != sInst)
        {
            sInst.mConnectingObj = null;
            if (sInst.mMsgType == ConnectMsgType.Connect_Msg_Connecting)
            {
                PromptMsg msg = PromptMsg.ShowSinglePrompt(text, sInst.ConnectOnClick);
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("重试"));
            }
        }
    }

    public override void Release()
    {
        base.Release();
        if (-1 != mControllerFlashIndex)
        {
            Timer.Cancel(mControllerFlashIndex);
            mControllerFlashIndex = -1;
        }
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
            mCheckSearchIndex = -1;
        }
        if (-1 != mSearchBlueIndex)
        {
            Timer.Cancel(mSearchBlueIndex);
            mSearchBlueIndex = -1;
        }
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_ON_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnectResult);
        sInst = null;
    }

    public override void Update()
    {
        base.Update();
        if (isShow)
        {
            if (mSearchUpdateFlag && null != mSearchTexture)
            {
                mSearchTime += Time.deltaTime;
                if (mSearchTime >= mSearchTextureIndex * 0.25f)
                {
                    ++mSearchTextureIndex;
                    if (mSearchTextureIndex >= mSearchTexture.Length)
                    {
                        mSearchTime = -0.5f;
                        mSearchTextureIndex = -1;
                    }
                    for (int i = 0, imax = mSearchTexture.Length; i < imax; ++i)
                    {
                        if (i <= mSearchTextureIndex)
                        {
                            mSearchTexture[i].alpha = 1;
                        }
                        else
                        {
                            mSearchTexture[i].alpha = 0;
                        }
                    }
                }
            }
            if (isMoveConnectLine && null != mConnectLineAry)
            {
                mLineUpdateTime += Time.deltaTime;
                if (mLineUpdateTime >= 0.03f)
                {
                    mLineUpdateTime -= 0.03f;
                    for (int i = 0, imax = mConnectLineAry.Length; i < imax; ++i)
                    {
                        if (null != mConnectLineAry[i])
                        {
                            mConnectLineAry[i].localPosition += mLineMoveSpeed;
                            if (mConnectLineAry[i].localPosition.y >= mLineHeight)
                            {
                                mConnectLineAry[i].localPosition -= new Vector3(0, mLineHeight * 2);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 1;
    }
    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        base.AddEvent();
        //永不待机
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        EventMgr.Inst.Regist(EventID.BLUETOOTH_ON_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnectResult);
        SingletonObject<ConnectManager>.GetInst().CleanUp();
        try
        {
            if (null != mTrans)
            {
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    Transform title = top.Find("title");
                    if (null != title)
                    {
                        Vector3 pos = UIManager.GetWinPos(title, UIWidget.Pivot.Top, 0, 43);
                        TweenPosition tp = title.GetComponent<TweenPosition>();
                        if (null != tp)
                        {
                            title.localPosition = pos + new Vector3(0, 300);
                            GameHelper.PlayTweenPosition(tp, pos, 0.6f);
                        }
                        else
                        {
                            title.localPosition = pos;
                        }
                    }

                    Transform btnBack = top.Find("btnBack");
                    if (null != btnBack)
                    {
                        Vector3 pos = UIManager.GetWinPos(btnBack, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        TweenPosition tp = btnBack.GetComponent<TweenPosition>();
                        if (null != tp)
                        {
                            btnBack.localPosition = pos - new Vector3(300, 0);
                            GameHelper.PlayTweenPosition(tp, pos, 0.6f);
                        }
                        else
                        {
                            btnBack.localPosition = pos;
                        }
                    }

                    Transform btnRefresh = top.Find("btnRefresh");
                    if (null != btnRefresh)
                    {
                        mRefreshBtnPosition = UIManager.GetWinPos(btnRefresh, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        btnRefresh.localPosition = mRefreshBtnPosition + new Vector3(300, 0);
                        btnRefresh.gameObject.SetActive(false);
                    }
                }

                Transform center = mTrans.Find("center");
                if (null != center)
                {
                    Transform searchBtn = center.Find("searchBtn");
                    if (null != searchBtn)
                    {
                        searchBtn.localScale = Vector3.zero;
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(searchBtn, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("点击搜索");
                        }
                    }

                    Transform controller = center.Find("controller");
                    if (null != controller)
                    {
                        mControllerPosition = UIManager.GetWinPos(controller, UIWidget.Pivot.Top, 0, 196);
                        controller.localPosition = mControllerPosition;
                        controller.gameObject.SetActive(false);
                        mLightSprite = GameHelper.FindChildComponent<UISprite>(controller, "light");
                        mLightTweenAlpha = GameHelper.FindChildComponent<TweenAlpha>(controller, "light");
                    }

                    Transform controllerSelect = center.Find("controller_select");
                    if (null != controllerSelect)
                    {
                        controllerSelect.localPosition = mControllerPosition;
                    }

                    Transform line = center.Find("line");
                    if (null != line)
                    {
                        Transform connectingLine = line.Find("connectingLine");
                        if (null != connectingLine)
                        {
                            mLineItem = connectingLine.gameObject;
                            mConnectingLineSize = NGUIMath.CalculateRelativeWidgetBounds(connectingLine).size;
                            mLineItem.SetActive(false);
                        }
                        line.gameObject.SetActive(false);
                    }
                    
                }

                Transform bottom = mTrans.Find("bottom");
                if (null != bottom)
                {
                    Transform phone = bottom.Find("phone");
                    if (null != phone)
                    {
                        mPhonePosition = UIManager.GetWinPos(phone, UIWidget.Pivot.Bottom);
                        phone.gameObject.SetActive(false);
                        Transform search = phone.Find("search");
                        if (null != search)
                        {
                            mSearchTexture = new UITexture[3];
                            for (int i = 0; i < 3; ++i)
                            {
                                mSearchTexture[i] = GameHelper.FindChildComponent<UITexture>(search, string.Format("search_{0}", i));
                            }
                        }
                    }

                    Transform blueList = bottom.Find("blueList");
                    if (null != blueList)
                    {
                        Transform Label = blueList.Find("Label");
                        GameHelper.SetLabelText(Label, LauguageTool.GetIns().GetText("查看控制器背后的编号并点击下方"));
                        mSelectBluePosition = UIManager.GetWinPos(blueList, UIWidget.Pivot.Bottom);
                        blueList.localPosition = mSelectBluePosition - new Vector3(0, 500);
                        Transform bg = blueList.Find("bg");
                        if (null != bg)
                        {
                            UISprite bgSprite = bg.GetComponent<UISprite>();
                            if (null != bgSprite)
                            {
                                bgSprite.width = PublicFunction.GetWidth();
                            }
                            UISprite shadow = GameHelper.FindChildComponent<UISprite>(bg, "shadow");
                            if (null != shadow)
                            {
                                shadow.width = PublicFunction.GetWidth();
                            }
                        }

                        Transform blueItem = blueList.Find("item");
                        if (null != blueItem)
                        {
                            mBlueItem = blueItem.gameObject;
                            mBlueItemSize = NGUIMath.CalculateRelativeWidgetBounds(blueItem).size;
                            mBlueItem.gameObject.SetActive(false);
                        }

                        Transform panel = blueList.Find("panel");
                        if (null != panel)
                        {
                            UIPanel uiPanel = panel.GetComponent<UIPanel>();
                            if (null != uiPanel)
                            {
                                uiPanel.depth = mDepth + 1;
                                Vector4 rect = uiPanel.finalClipRegion;
                                rect.z = PublicFunction.GetWidth();
                                uiPanel.baseClipRegion = rect;
                            }
                            mBlueGridTrans = panel.Find("grid");
                        }
                        blueList.gameObject.SetActive(false);

                    }
                }
                IntoSearchMsg();
            }
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "连接页面加载完毕");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            string name = obj.name;
            if (name.Equals("btnBack"))
            {
                if (mMsgType == ConnectMsgType.Connect_Msg_Search_Ready || mMsgType == ConnectMsgType.Connect_Msg_Search)
                {
                    OnClose();
                    SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                }
                else if (mMsgType == ConnectMsgType.Connect_Msg_Select || mMsgType == ConnectMsgType.Connect_Msg_Into_Select_Ing)
                {
                    PlatformMgr.Instance.StopScan();
                    OnClose();
                    SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                }
                else if (mMsgType == ConnectMsgType.Connect_Msg_Connecting)
                {
                    if (null != mConnectingObj)
                    {
                        mConnectingObj = null;
                        PlatformMgr.Instance.CannelConnectBluetooth();
                    }
                    ClearAllDevice();
                    IntoSearchMsg();
                }
            }
            else if (name.Equals("searchBtn"))
            {
                IntoSearchMsg();
            }
            else if (name.Equals("btnRefresh"))
            {
                ClearAllDevice();
                if (-1 != mSearchBlueIndex)
                {
                    Timer.Cancel(mSearchBlueIndex);
                    mSearchBlueIndex = -1;
                }
                if (-1 != mCheckSearchIndex)
                {
                    Timer.Cancel(mCheckSearchIndex);
                    mCheckSearchIndex = -1;
                }
                if (PlatformMgr.Instance.IsOpenBluetooth())
                {
                    OpenSearchBlue();
                }
                else
                {
                    IntoSearchMsg();
                }
            }
            else if (name.StartsWith("blue_"))
            {//选中了蓝牙
                if (mDeviceDict.ContainsKey(obj))
                {
                    mConnectingObj = obj;
                    PlatformMgr.Instance.StopScan();
                    IntoMsgState(ConnectMsgType.Connect_Msg_Connecting);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void IntoSearchMsg()
    {
        if (PlatformMgr.Instance.IsOpenBluetooth())
        {
            IntoMsgState(ConnectMsgType.Connect_Msg_Search);
        }
        else
        {
#if UNITY_ANDROID
            PromptMsg msg = PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("此应用需要使用蓝牙功能"), OpenBlueOnClick);
            msg.SetLeftBtnText(LauguageTool.GetIns().GetText("拒绝"));
            msg.SetRightBtnText(LauguageTool.GetIns().GetText("允许"));
#else
            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("蓝牙未打开").Replace('-', '>'), OpenIosBlueOnClick);
#endif
            IntoMsgState(ConnectMsgType.Connect_Msg_Search_Ready);
        }
    }

    void IntoMsgState(ConnectMsgType msgType)
    {
        if (mMsgType == msgType)
        {
            return;
        }
        try
        {
            SetTitle(msgType);
            switch (mMsgType)
            {
                case ConnectMsgType.Connect_Msg_Search_Ready:
                    SetSearchBtnActive(false);
                    break;
                case ConnectMsgType.Connect_Msg_Search:
                    if (-1 != mControllerFlashIndex)
                    {
                        Timer.Cancel(mControllerFlashIndex);
                        mControllerFlashIndex = -1;
                    }
                    SetSearchPhoneActive(false);
                    SetControllerAlphaActive(false);
                    break;
                case ConnectMsgType.Connect_Msg_Select:
                case ConnectMsgType.Connect_Msg_Into_Select_Ing:
                    SetRefreshActive(false);
                    SetControllerSelectActive(false);
                    SetSelectBlueListActive(false);
                    break;
                case ConnectMsgType.Connect_Msg_Connecting:
                    if (mMsgType != ConnectMsgType.Connect_Msg_Search)
                    {
                        SetControllerAlphaActive(false);
                        SetConnectingPhoneActive(false);
                    }
                    SetConnectingLineActive(false);
                    break;
            }
            if (msgType == ConnectMsgType.Connect_Msg_Select)
            {
                mMsgType = ConnectMsgType.Connect_Msg_Into_Select_Ing;
            }
            else
            {
                mMsgType = msgType;
            }
            switch (msgType)
            {
                case ConnectMsgType.Connect_Msg_Search_Ready:
                    SetSearchBtnActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Search:
                    SetControllerAlphaActive(true);
                    SetSearchPhoneActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Select:
                    SetRefreshActive(true);
                    SetControllerSelectActive(true);
                    SetSelectBlueListActive(true);
                    break;
                case ConnectMsgType.Connect_Msg_Connecting:
                    SetControllerAlphaActive(true);
                    SetConnectingPhoneActive(true);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void SetTitle(ConnectMsgType msgType)
    {
        if (null == mTrans)
        {
            return;
        }
        Transform title = mTrans.Find("top/title");
        if (null != title)
        {
            Transform maintitle = title.Find("maintitle");
            
            if (null != maintitle)
            {
                TweenAlpha tweenAlpha = maintitle.GetComponent<TweenAlpha>();
                if (null != tweenAlpha)
                {
                    if (ConnectMsgType.Connect_Msg_None != mMsgType)
                    {
                        GameHelper.PlayTweenAlpha(tweenAlpha, 0.01f);
                        tweenAlpha.SetOnFinished(delegate () {
                            SetMainTitleText(maintitle, msgType);
                            tweenAlpha.onFinished.Clear();
                            GameHelper.PlayTweenAlpha(tweenAlpha, 1);
                        });
                    }
                    else
                    {
                        SetMainTitleText(maintitle, msgType);
                    }
                }
                else
                {
                    SetMainTitleText(maintitle, msgType);
                }
            }
            Transform subtitle = title.Find("subtitle");
            if (null != subtitle)
            {
                if (ConnectMsgType.Connect_Msg_None == mMsgType)
                {
                    GameHelper.SetLabelText(subtitle, LauguageTool.GetIns().GetText("请勿让Jimu远离手机"));
                }
            }
        }
    }
    void SetMainTitleText(Transform maintitle, ConnectMsgType msgType)
    {
        switch (msgType)
        {
            case ConnectMsgType.Connect_Msg_Search_Ready:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("搜索Jimu"));
                break;
            case ConnectMsgType.Connect_Msg_Search:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("搜索Jimu中..."));
                break;
            case ConnectMsgType.Connect_Msg_Connecting:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("连接Jimu中..."));
                break;
            case ConnectMsgType.Connect_Msg_Select:
                GameHelper.SetLabelText(maintitle, LauguageTool.GetIns().GetText("选择你要连接的Jimu"));
                break;
        }
    }

    void SetSearchBtnActive(bool activeFlag)
    {
        Transform searchBtn = mTrans.Find("center/searchBtn");
        if (null != searchBtn)
        {
            TweenScale tweenScale = searchBtn.GetComponent<TweenScale>();
            if (null != tweenScale)
            {
                if (activeFlag)
                {
                    searchBtn.gameObject.SetActive(activeFlag);
                    searchBtn.localScale = Vector3.zero;
                    GameHelper.PlayTweenScale(tweenScale, Vector3.one);
                    tweenScale.onFinished.Clear();
                }
                else
                {
                    GameHelper.PlayTweenScale(tweenScale, Vector3.zero);
                    tweenScale.SetOnFinished(delegate () {
                        searchBtn.gameObject.SetActive(false);
                    });
                }
            }
        }
    }

    void ControllerLight()
    {
        if (null != mLightSprite)
        {
            mLightSprite.alpha = 1;
        }
        GameHelper.PlayTweenAlpha(mLightTweenAlpha, 0, 0.1f);
    }

    void SetControllerActive(bool activeFlag)
    {
        Transform controller = mTrans.Find("center/controller");
        SetTransformPositionActive(controller, activeFlag, mControllerPosition, mControllerPosition + new Vector3(0, 300), delegate () {
            if (activeFlag)
            {
                if (-1 != mControllerFlashIndex)
                {
                    Timer.Cancel(mControllerFlashIndex);
                }
                mControllerFlashIndex = Timer.Add(0f, 1f, ControllerLight);
            }
            else
            {
                if (-1 != mControllerFlashIndex)
                {
                    Timer.Cancel(mControllerFlashIndex);
                    mControllerFlashIndex = -1;
                }
            }
        });
    }

    void SetControllerAlphaActive(bool activeFlag)
    {
        Transform controller = mTrans.Find("center/controller");
        SetTransformAlphaActive(controller, activeFlag, delegate () {
            if (activeFlag)
            {
                if (-1 != mControllerFlashIndex)
                {
                    Timer.Cancel(mControllerFlashIndex);
                }
                mControllerFlashIndex = Timer.Add(0f, 1f, ControllerLight);
            }
            else
            {
                if (-1 != mControllerFlashIndex)
                {
                    Timer.Cancel(mControllerFlashIndex);
                    mControllerFlashIndex = -1;
                }
            }
        });
    }

    void SetSearchPhoneActive(bool activeFlag)
    {
        Transform phone = mTrans.Find("bottom/phone");
        SetTransformPositionActive(phone, activeFlag, mPhonePosition, mPhonePosition - new Vector3(0, 300), delegate () {
            if (activeFlag)
            {
                Transform search = phone.Find("search");
                if (null != search)
                {
                    mSearchUpdateFlag = true;
                    mSearchTextureIndex = 0;
                    mSearchTime = 0;
                    search.gameObject.SetActive(true);
                    if (-1 != mSearchBlueIndex)
                    {
                        Timer.Cancel(mSearchBlueIndex);
                        mSearchBlueIndex = -1;
                    }
                    OpenSearchBlue();
                }
            }
        });
        if (!activeFlag)
        {
            Transform search = phone.Find("search");
            if (null != search)
            {
                mSearchUpdateFlag = false;
                search.gameObject.SetActive(false);
            }
        }
    }

    void SetConnectingPhoneActive(bool activeFlag)
    {
        Transform phone = mTrans.Find("bottom/phone");
        SetTransformPositionActive(phone, activeFlag, mPhonePosition, mPhonePosition - new Vector3(0, 300), delegate () {
            if (activeFlag)
            {
                SetConnectingLineActive(activeFlag);
                if (null != mConnectingObj && mDeviceDict.ContainsKey(mConnectingObj))
                {
                    PlatformMgr.Instance.ConnenctBluetooth(mDeviceDict[mConnectingObj].Mac, mDeviceDict[mConnectingObj].Name);
                }
                else
                {
                    IntoSearchMsg();
                }
            }
        });
    }


    void SetControllerSelectActive(bool activeFlag)
    {
        Transform controllerSelect = mTrans.Find("center/controller_select");
        SetTransformAlphaActive(controllerSelect, activeFlag);
    }

    void SetSelectBlueListActive(bool activeFlag)
    {
        Transform blueList = mTrans.Find("bottom/blueList");
        SetTransformPositionActive(blueList, activeFlag, mSelectBluePosition, mSelectBluePosition - new Vector3(0, 500), delegate () {
            if (activeFlag)
            {
                CreateCacheDevice();
                mMsgType = ConnectMsgType.Connect_Msg_Select;
            }
            else
            {
                if (null != blueList)
                {
                    Transform panel = blueList.Find("panel");
                    if (null != panel)
                    {
                        panel.localPosition = Vector3.zero;
                        UIPanel uiPanel = panel.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            Vector2 offset = uiPanel.clipOffset;
                            offset.y = 0;
                            uiPanel.clipOffset = offset;
                        }
                    }
                }
            }
        });
    }

    void SetRefreshActive(bool activeFlag)
    {
        Transform btnRefresh = mTrans.Find("top/btnRefresh");
        SetTransformPositionActive(btnRefresh, activeFlag, mRefreshBtnPosition, mRefreshBtnPosition + new Vector3(300, 0));
    }

    void SetConnectingLineActive(bool activeFlag)
    {
        Transform center = mTrans.Find("center");
        Transform bottom = mTrans.Find("bottom");
        if (null != center && null != bottom)
        {
            Transform controllerDot = center.Find("controller/connectingDot");
            Transform phoneDot = bottom.Find("phone/connectingDot");
            Vector3 controllerDotPos = Vector3.zero;
            Vector3 phoneDotPos = Vector3.zero;
            if (null != controllerDot)
            {
                controllerDotPos = controllerDot.localPosition + controllerDot.parent.localPosition;
                if (!activeFlag)
                {
                    SetDotScaleEnabled(controllerDot, activeFlag);
                }
                SetTransformScaleActive(controllerDot, activeFlag, delegate() {
                    if (activeFlag)
                    {
                        SetDotScaleEnabled(controllerDot, activeFlag);
                    }
                });
            }
            if (null != phoneDot)
            {
                phoneDotPos = phoneDot.localPosition + phoneDot.parent.localPosition + bottom.localPosition;
                if (!activeFlag)
                {
                    SetDotScaleEnabled(phoneDot, activeFlag);
                }
                SetTransformScaleActive(phoneDot, activeFlag, delegate() {
                    SetDotScaleEnabled(phoneDot, activeFlag);
                });
            }
            Transform line = center.Find("line");
            if (null != line)
            {
                if (null == mConnectLineAry)
                {
                    mLineHeight = (int)Mathf.Abs(controllerDotPos.y - phoneDotPos.y);
                    int y = (int)(phoneDotPos.y + mLineHeight / 2);
                    line.localPosition = new Vector3(0, y);
                    UIPanel uiPanel = line.GetComponent<UIPanel>();
                    if (null != uiPanel)
                    {
                        uiPanel.depth = mDepth + 1;
                        Vector4 rect = uiPanel.finalClipRegion;
                        rect.w = mLineHeight;
                        uiPanel.baseClipRegion = rect;
                    }
                    mConnectLineAry = new Transform[2];
                    for (int i = 0, imax = mConnectLineAry.Length; i < imax; ++i)
                    {
                        mConnectLineAry[i] = CreateConnectingLine(mLineHeight, line);
                        if (null != mConnectLineAry[i])
                        {
                            mConnectLineAry[i].localPosition = new Vector3(0, - i * mLineHeight);
                        }
                    }
                }
                isMoveConnectLine = activeFlag;
                mLineUpdateTime = 0;
                SetTransformAlphaActive(line, activeFlag);
            }
        }
    }

    void SetDotScaleEnabled(Transform trans, bool enabled)
    {
        TweenScale tmp = GameHelper.FindChildComponent<TweenScale>(trans, "dot");
        if (null != tmp)
        {
            tmp.enabled = enabled;
            tmp.transform.localScale = Vector3.one;
        }
    }


    void SetTransformAlphaActive(Transform trans, bool activeFlag, EventDelegate.Callback onFinished = null)
    {
        if (null != trans)
        {
            TweenAlpha tweenAlpha = trans.GetComponent<TweenAlpha>();
            if (null != tweenAlpha)
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(true);
                    GameHelper.SetTransformAlpha(trans, 0);
                    GameHelper.PlayTweenAlpha(tweenAlpha, 1, 0.6f);
                    tweenAlpha.SetOnFinished(delegate () {
                        tweenAlpha.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
                else
                {
                    GameHelper.PlayTweenAlpha(tweenAlpha, 0, 0.6f);
                    tweenAlpha.SetOnFinished(delegate ()
                    {
                        trans.gameObject.SetActive(false);
                        tweenAlpha.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
            }
            else
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(activeFlag);
                    GameHelper.SetTransformAlpha(trans, 1);
                }
                else
                {
                    GameHelper.SetTransformAlpha(trans, 0);
                    trans.gameObject.SetActive(activeFlag);
                }
                if (null != onFinished)
                {
                    onFinished();
                }
            }
        }
    }

    void SetTransformPositionActive(Transform trans, bool activeFlag, Vector3 pos, Vector3 hidePos, EventDelegate.Callback onFinished = null)
    {
        if (null != trans)
        {
            TweenPosition tweenPosition = trans.GetComponent<TweenPosition>();
            if (null != tweenPosition)
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(true);
                    trans.localPosition = hidePos;
                    GameHelper.PlayTweenPosition(tweenPosition, pos, 0.6f);
                    tweenPosition.SetOnFinished(delegate ()
                    {
                        tweenPosition.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
                else
                {
                    GameHelper.PlayTweenPosition(tweenPosition, hidePos, 0.6f);
                    tweenPosition.SetOnFinished(delegate ()
                    {
                        trans.gameObject.SetActive(false);
                        tweenPosition.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
            }
            else
            {
                trans.gameObject.SetActive(activeFlag);
                if (activeFlag)
                {
                    trans.localPosition = pos;
                }
                else
                {
                    trans.localPosition = hidePos;
                }
                if (null != onFinished)
                {
                    onFinished();
                }
            }
        }
    }

    void SetTransformScaleActive(Transform trans, bool activeFlag, EventDelegate.Callback onFinished = null)
    {
        if (null != trans)
        {
            TweenScale tweenScale = trans.GetComponent<TweenScale>();
            if (null != tweenScale)
            {
                if (activeFlag)
                {
                    trans.gameObject.SetActive(true);
                    trans.localScale = Vector3.zero;
                    GameHelper.PlayTweenScale(tweenScale, Vector3.one, 0.6f);
                    tweenScale.SetOnFinished(delegate ()
                    {
                        tweenScale.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
                else
                {
                    trans.localScale = Vector3.one;
                    GameHelper.PlayTweenScale(tweenScale, Vector3.zero, 0.6f);
                    tweenScale.SetOnFinished(delegate ()
                    {
                        trans.gameObject.SetActive(false);
                        tweenScale.onFinished.Clear();
                        if (null != onFinished)
                        {
                            onFinished();
                        }
                    });
                }
            }
            else
            {
                trans.gameObject.SetActive(activeFlag);
                if (activeFlag)
                {
                    trans.localScale = Vector3.one;
                }
                else
                {
                    trans.localScale = Vector3.zero;
                }
                if (null != onFinished)
                {
                    onFinished();
                }
            }
        }
    }

    Transform CreateConnectingLine(int height, Transform parent)
    {
        if (null != mLineItem)
        {
            GameObject tmp = GameObject.Instantiate(mLineItem) as GameObject;
            Transform trans = tmp.transform;
            trans.parent = parent;
            trans.localScale = Vector3.one;
            tmp.SetActive(true);
            UISprite sprite = tmp.GetComponent<UISprite>();
            if (null != sprite)
            {
                sprite.height = height;
            }
            return trans;
        }
        return null;
    }

    void OpenSearchBlue()
    {
        float waitTime = Time.time - SingletonObject<ConnectManager>.GetInst().LastDicConnectedTime;
        if (waitTime >= 3)
        {
            mSearchBlueIndex = Timer.Add(1, 1, 1, SearchBlue);
        }
        else
        {
            float wt = 4f - waitTime;
            mSearchBlueIndex = Timer.Add(wt, 1, 1, SearchBlue);
        }
    }
    void SearchBlue()
    {
#if UNITY_EDITOR
        Timer.Add(1, 1, 10, Test);
#endif
        mSearchBlueIndex = -1;
        PlatformMgr.Instance.StartScan();
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
        }
        mCheckSearchIndex = Timer.Add(20, 1, 1, CheckSearchDevice);
    }

    void CheckSearchDevice()
    {
        mCheckSearchIndex = -1;
        if (mDeviceDict.Count < 1)
        {
            IntoMsgState(ConnectMsgType.Connect_Msg_Search_Ready);
            if (SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag)
            {
                PlatformMgr.Instance.PopWebErrorType(ConnectionErrorType.ConnectionSearchJimuType);
            }
            else
            {
                PromptMsg msg = PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("搜索不到蓝牙设备"), RefreshBlueOnClick);
                if (null != msg)
                {
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("刷新"));
                }
            }
        }
    }

    void RefreshBlueOnClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    GameObject AddDevice(DeviceInfo device)
    {
        if (null != mBlueItem && null != mBlueGridTrans)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "发现设备:mac = " + device.Mac + " name=" + device.Name + " rssi =" + device.RSSI);
            GameObject tmp = GameObject.Instantiate(mBlueItem) as GameObject;
            tmp.name = string.Format("blue_{0}", device.RSSI);
            tmp.SetActive(true);
            mDeviceDict[tmp] = device;
            mDeviceForMacDict[device.Mac] = tmp;
            Transform trans = tmp.transform;
            trans.parent = mBlueGridTrans;
            trans.localScale = new Vector3(1, 0, 1);
            trans.localEulerAngles = Vector3.zero;
            SetDeviceSignal(trans, device.RSSI);
            BlueListReposition(trans);
            UILabel label = GameHelper.FindChildComponent<UILabel>(trans, "Label");
            if (null != label)
            {
                string mac = string.Empty;
                if (RobotManager.GetInst().IsCreateRobotFlag || RobotManager.GetInst().IsSetDeviceIDFlag)
                {
                    mac = PlatformMgr.Instance.GetLastConnectedMac();
                }
                else
                {
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        mac = PlatformMgr.Instance.GetRobotConnectedMac(robot.ID);
                    }
                }
                string rename = PlatformMgr.Instance.GetNameForMac(device.Mac);
                if (string.IsNullOrEmpty(rename))
                {
                    if (device.Name.StartsWith("Jimu_") || device.Name.StartsWith("JIMU_"))
                    {
                        if (device.Mac.Equals(mac))
                        {
                            label.text = device.Name;
                            label.color = Color.green;
                        }
                        else
                        {
                            label.text = string.Format("[777b7c]{0}[-][-]", device.Name.Replace("_", "_[eb3b75]"));
                        }
                    }
                    else
                    {
                        label.text = device.Name;
                        if (device.Mac.Equals(mac))
                        {
                            label.color = Color.green;
                        }
                        else
                        {
                            label.color = PublicFunction.GreyColor;
                        }
                        
                    }
                }
                else
                {
                    label.text = rename;
                    if (device.Mac.Equals(mac))
                    {
                        label.color = Color.green;
                    }
                    else
                    {
                        label.color = PublicFunction.GreyColor;
                    }
                }
                //label.text = label.text + string.Format(" [777b7c]信号强度{0}[-]", device.RSSI.ToString());
            }
            UIManager.SetButtonEventDelegate(trans, mBtnDelegate);
            return tmp;
        }
        return null;
    }

    void SetDeviceSignal(Transform tmp, int rssi)
    {
        UISprite sprite = GameHelper.FindChildComponent<UISprite>(tmp, "signal");
        if (null != sprite)
        {
            sprite.spriteName = GetSignalIconName(rssi);
            sprite.MakePixelPerfect();
        }
    }

    string GetSignalIconName(int rssi)
    {
        if (rssi < -90)
        {
            return "signal1";
        }
        else if (rssi < -80)
        {
            return "signal2";
        }
        else if (rssi < -70)
        {
            return "signal3";
        }
        else
        {
            return "signal4";
        }
    }

    void BlueListReposition(Transform item)
    {
        List<Transform> list = new List<Transform>();
        for (int i = 0, imax = mBlueGridTrans.childCount; i < imax; ++i)
        {
            list.Add(mBlueGridTrans.GetChild(i));
        }
        list.Sort(delegate (Transform a, Transform b) {
            int num1 = int.Parse(a.name.Substring("blue_".Length));
            int num2 = int.Parse(b.name.Substring("blue_".Length));
            return num2 - num1;
        });
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            Transform trans = list[i];
            Vector2 pos = new Vector2(0, -mBlueItemSize.y / 2 - mBlueItemSize.y * i - 14 * (i + 1));
            if (item == trans)
            {
                trans.localPosition = pos;
                TweenScale tweenScale = GameHelper.PlayTweenScale(trans, Vector3.one);
                if (null != tweenScale)
                {
                    tweenScale.SetOnFinished(delegate () {
                        UIButtonScale btnScale = trans.GetComponent<UIButtonScale>();
                        if (null != btnScale)
                        {
                            btnScale.enabled = true;
                        }
                    });
                }
                else
                {
                    UIButtonScale btnScale = trans.GetComponent<UIButtonScale>();
                    if (null != btnScale)
                    {
                        btnScale.enabled = true;
                    }
                }
            }
            else
            {
                if (!pos.Equals(trans.localPosition))
                {
                    GameHelper.PlayTweenPosition(trans, pos);
                }
            }
        }
    }

    void CreateCacheDevice()
    {
        for (int i = 0, imax = mCacheDeviceList.Count; i < imax; ++i)
        {
            AddDevice(mCacheDeviceList[i]);
        }
        mCacheDeviceList.Clear();
    }

    void ClearAllDevice()
    {
        mCacheDeviceList.Clear();
        foreach (var kvp in mDeviceForMacDict)
        {
            kvp.Value.transform.parent = null;
            GameObject.Destroy(kvp.Value);
        }
        mDeviceForMacDict.Clear();
        mDeviceDict.Clear();
        if (null != mBlueGridTrans)
        {
            mBlueGridTrans.DetachChildren();
        }
    }


    void OnFoundDevice(EventArg arg)
    {
        try
        {
            DeviceInfo info = arg[0] as DeviceInfo;
            if (info == null) return;
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, string.Format("发现的设备 name = {0} mac = {1} rssi = {2}", info.Name, info.Mac, info.RSSI));
            if (info.Name.StartsWith("Jimuspk_"))
            {
                return;
            }
            
            if (info.Name.StartsWith("JIMU") || info.Name.StartsWith("Jimu") || info.Name.StartsWith("jimu"))
            {
                if (info.RSSI == 127)
                {
                    info.RSSI = -127;
                }
                if (-1 != mCheckSearchIndex)
                {
                    Timer.Cancel(mCheckSearchIndex);
                    mCheckSearchIndex = -1;
                }
                Robot robot = SingletonObject<ConnectManager>.GetInst().GetConnectRobot();
                if (null != robot)
                {
                    if (SingletonObject<ConnectManager>.GetInst().IsAutoConnect(robot.ID, info.Mac))
                    {
                        mConnectingObj = AddDevice(info);
                        mCacheDeviceList.Clear();
                        PlatformMgr.Instance.StopScan();
                        IntoMsgState(ConnectMsgType.Connect_Msg_Connecting);
                        return;
                    }
                }
                if (!mDeviceForMacDict.ContainsKey(info.Mac))
                {
                    if (mMsgType == ConnectMsgType.Connect_Msg_Select)
                    {
                        AddDevice(info);
                    }
                    else if (mMsgType == ConnectMsgType.Connect_Msg_Search)
                    {
                        mCacheDeviceList.Add(info);
                        IntoMsgState(ConnectMsgType.Connect_Msg_Select);
                    }
                    else if (mMsgType == ConnectMsgType.Connect_Msg_Into_Select_Ing)
                    {
                        mCacheDeviceList.Add(info);
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


    void OnConnectResult(EventArg arg)
    {
        try
        {
            bool result = (bool)arg[0];
            if (result)
            {
            }
            else
            {
                Robot robot = SingletonObject<ConnectManager>.GetInst().GetConnectRobot();
                if (null != robot)
                {
                    if (SingletonObject<ConnectManager>.GetInst().GetDeviceConnectState(robot) != ConnectState.Connect_Cannel && PlatformMgr.Instance.PowerData.power <= PublicFunction.Robot_Power_Empty)
                    {
                        PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed, BlueConnectFailReason.LowPower.ToString());
                    }
                }
                
                if (null != mConnectingObj && mMsgType == ConnectMsgType.Connect_Msg_Connecting)
                {
                    PromptMsg msg = PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("连接失败"), ConnectOnClick);
                    msg.SetRightBtnText(LauguageTool.GetIns().GetText("重试"));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void ConnectOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            ClearAllDevice();
            IntoSearchMsg();
        }
    }

    void OpenBlueOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.LeftBtnName))
        {
        }
        else if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            PlatformMgr.Instance.OpenBluetooth();
            NetWaitMsg.ShowWait(1);
            Timer.Add(1f, 1, 1, delegate () { 
                if (PlatformMgr.Instance.IsOpenBluetooth())
                {
                    IntoMsgState(ConnectMsgType.Connect_Msg_Search);
                }
            });
        }
    }

    void OpenIosBlueOnClick(GameObject obj)
    {
        if (PlatformMgr.Instance.IsOpenBluetooth())
        {
            IntoMsgState(ConnectMsgType.Connect_Msg_Search);
        }
    }

    void Test()
    {
        int rssi = UnityEngine.Random.Range(0, 128);
        DeviceInfo info = null;
        if (rssi % 2 == 0)
        {
            info = new DeviceInfo("JIMU\n" + Guid.NewGuid() + "\n" + (-rssi));
        }
        else
        {
            string name = "Jimu_" + UnityEngine.Random.Range(1000, 9999);
            info = new DeviceInfo(name + "\n" + Guid.NewGuid() + "\n" + (-rssi));
        }
        OnFoundDevice(new EventArg(info));
    }
#endregion
}