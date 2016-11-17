#define USE_RECTANGLE
using Game.Platform;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
using Game.Resource;
using Game.Scene;

/// <summary>
/// Author:xj
/// FileName:SearchBluetoothMsg.cs
/// Description:搜索蓝牙页面
/// Time:2016/3/28 14:33:14
/// </summary>
public class SearchBluetoothMsg : BasePopWin
{
    #region 公有属性
    public delegate void CloseCallBack();
    public CloseCallBack OnCloseCallBack;
    #endregion

    #region 其他属性
    GameObject mDevicePrefabs;
    Transform mRadarChart;
    Dictionary<GameObject, DeviceInfo> mDeviceDict = new Dictionary<GameObject, DeviceInfo>();
    Dictionary<string, GameObject> mDeviceForMacDict = new Dictionary<string, GameObject>();
    Vector2 mGridSize = new Vector2(300, 120);
    float GridDistanceMin = 200;
    UITexture mBgUITexture;
    Dictionary<string, Texture> mTextureDict;
    int mTextureIndex = 0;
    int mTextureIndexMax = 29;
    string mBgTexFont = "seach_";
    float mTime;

    static SearchBluetoothMsg mInst;
#if USE_RECTANGLE
    bool[,] mUseArea = null;
    List<Int2> mRandomList = null;
    
    Vector2 mStartPos = Vector2.zero;
    float mRandomModulus = 0.3f;
#else
    float mRadius = 300;
    

#endif
    Vector3 mErrorPos = new Vector2(-100000, -100000);

    Vector3 mBackBtnPos;
    Vector3 mRefreshBtnPos;
    //Vector2 mBackSize;
    long mCheckSearchIndex = -1;
    long mSearchIndex = -1;

    float mMaxY = 0;
    float mMinY = 0;

    bool isConnecting;
    TweenPosition mBackBtnTweenPosition;
    TweenPosition mRefreshBtnTweenPosition;
    TweenAlpha mBgTexTweenAlpha;
    GameObject mConnectingObj;

    bool mUpdateTexFlag = true;
#endregion

#region 公有函数
    public SearchBluetoothMsg()
    {
        mUIResPath = "Prefab/UI/SearchBluetoothMsg";
        mTextureDict = new Dictionary<string, Texture>();
        isSingle = true;
        mInst = this;
        isConnecting = false;
        mUpdateTexFlag = true;
    }

    public static void ShowMsg()
    {
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            PlatformMgr.Instance.DisConnenctBuletooth();
        }
        if (null == mInst)
        {
            PopWinManager.GetInst().ShowPopWin(typeof(SearchBluetoothMsg));
        }
        else
        {
            mInst.OnShow();
        }
    }

    public static void CloseMsg()
    {
        if (null != mInst)
        {
            mInst.OnClose();
        }
    }

    public static void SetConnectingState(bool connectingFlag)
    {
        if (null != mInst)
        {
            mInst.isConnecting = connectingFlag;
        }
    }
    public override void OnShow()
    {
        base.OnShow();
        mUpdateTexFlag = true;
        if (null != mBgUITexture)
        {
            mBgUITexture.alpha = 1;
        }
        if (PlatformMgr.Instance.IsOpenBluetooth())
        {
            if (-1 != mSearchIndex)
            {
                Timer.Cancel(mSearchIndex);
                mSearchIndex = -1;
            }
            SearchBlue();
        }
        else
        {
            /*if (StepManager.GetIns().OpenOrCloseGuide)
            {
                EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().ErrorStep, false));
            }*/
            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("请打开蓝牙！"), OpenBluetoothOnClick);
        }
        
    }

    public override void OnHide()
    {
        base.OnHide();
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
        }
        if (-1 != mSearchIndex)
        {
            Timer.Cancel(mSearchIndex);
        }
        PlatformMgr.Instance.StopScan();
        /*foreach (KeyValuePair<string, GameObject> kvp in mDeviceForMacDict)
        {
            kvp.Value.SetActive(false);
        }*/
    }

    public override void Release()
    {
        base.Release();
        mInst = null;
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_ON_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, OnFoundDevice);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
        }
        if (-1 != mSearchIndex)
        {
            Timer.Cancel(mSearchIndex);
        }
        mTextureDict.Clear();
        ConnectingMsg.CloseMsg();
    }

    public override void Update()
    {
        base.Update();
        if (isShow)
        {
            if (null != mBgUITexture && mUpdateTexFlag)
            {
                mTime += Time.deltaTime;
                if (mTime >= mTextureIndex * 0.125f)
                {//每秒8帧
                    mBgUITexture.mainTexture = GetBgTextrue(mTextureIndex);
                    ++mTextureIndex;
                    if (mTextureIndex > mTextureIndexMax)
                    {
                        mTime = -0.125f;
                        mTextureIndex = 0;
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
        try
        {
            EventMgr.Inst.Regist(EventID.BLUETOOTH_ON_DEVICE_FOUND, OnFoundDevice);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_ON_MATCHED_DEVICE_FOUND, OnFoundDevice);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
            
            InitUi();
            if (PlatformMgr.Instance.IsOpenBluetooth())
            {
                float waitTime = Time.time - PlatformMgr.Instance.lastDicConnectedTime;
                if (waitTime >= 3)
                {
                    SearchBlue();
                }
                else
                {
                    float wt = 3.5f - waitTime;
                    //NetWaitMsg.ShowWait(wt);
                    mSearchIndex = Timer.Add(wt, 1, 1, SearchBlue);
                }
            }
            else
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("请打开蓝牙！"), OpenBluetoothOnClick);
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
            string name = obj.name;
            if (name.StartsWith("blue_"))
            {//选中了蓝牙
                if (mDeviceDict.ContainsKey(obj))
                {
                    mConnectingObj = obj;
                    mUpdateTexFlag = false;
                    isConnecting = true;
                    OpenConnectedAnim();
                }
            }
            else if (name.Equals("backbtn"))
            {
                OnClose();
                SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
            }
            else if (name.Equals("skipBtn"))
            {
                OnClose();
                EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
            }
            else if (name.Equals("refreshbtn"))
            {
                HideAllDevice();
                if (-1 != mSearchIndex)
                {
                    Timer.Cancel(mSearchIndex);
                    mSearchIndex = -1;
                }
                SearchBlue();
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

    void SearchBlue()
    {
        mSearchIndex = -1;
        PlatformMgr.Instance.StartScan();
        if (-1 != mCheckSearchIndex)
        {
            Timer.Cancel(mCheckSearchIndex);
        }
        mCheckSearchIndex = Timer.Add(30, 1, 1, CheckSearchDevice);
    }

    void CheckSearchDevice()
    {
        mCheckSearchIndex = -1;
        if (mDeviceDict.Count < 1)
        {
            PromptMsg msg = PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("搜索不到蓝牙设备"), RefreshBlueOnClick);
            if (null != msg)
            {
                msg.SetRightBtnText(LauguageTool.GetIns().GetText("刷新"));
            }
        }
    }

    void RefreshBlueOnClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
            if (name.Equals(PromptMsg.RightBtnName))
            {
                if (-1 != mSearchIndex)
                {
                    Timer.Cancel(mSearchIndex);
                    mSearchIndex = -1;
                }
                SearchBlue();
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError("PublicPrompt" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    void InitUi()
    {
        mTime = 0;
        mBgUITexture = GameHelper.FindChildComponent<UITexture>(mTrans, "bg");
        if (null != mBgUITexture)
        {
            mTextureIndex = 0;
            mBgUITexture.mainTexture = GetBgTextrue(mTextureIndex);
            //float screenWidth = PublicFunction.GetWidth();
            /*float screenHeight = PublicFunction.GetHeight();

            mBgUITexture.width = (int)(screenHeight * mBgUITexture.width / (mBgUITexture.height + 0.0f));
            mBgUITexture.height = (int)screenHeight;*/
            
            /*int height = (int)(screenHeight * mBgUITexture.width / screenWidth);
            if (height >= mBgUITexture.height)
            {
                mBgUITexture.height = (int)screenHeight;
                mBgUITexture.width = (int)(screenHeight * mBgUITexture.width / mBgUITexture.height);
            }
            else
            {
                mBgUITexture.height = (int)(screenWidth * mBgUITexture.height / mBgUITexture.width);
                mBgUITexture.width = (int)screenWidth;
            }*/
            ++mTextureIndex;
            mBgTexTweenAlpha = GameHelper.FindChildComponent<TweenAlpha>(mTrans, "bg");
            if (null != mBgTexTweenAlpha)
            {
                mBgTexTweenAlpha.value = 0;
                GameHelper.PlayTweenAlpha(mBgTexTweenAlpha, 1);
            }
            else
            {
                mBgUITexture.alpha = 1;
            }
        }
        
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
#if USE_RECTANGLE
        if (null == mUseArea)
        {
            int width = PublicFunction.GetWidth();
            int height = PublicFunction.GetHeight();
            float wnum = 4.0f * PublicFunction.GetWidth() / PublicFunction.Default_Screen_Width;
            float hnum = 6.0f * PublicFunction.GetHeight() / PublicFunction.Default_Screen_Height;
            mUseArea = new bool[PublicFunction.Rounding(wnum, 0.8f), PublicFunction.Rounding(hnum, 0.6f)];
        }
        if (null == mRandomList)
        {
            mRandomList = UseAreaToList();
        }
        mStartPos = new Vector2(-PublicFunction.GetWidth() / 2 + mGridSize.x / 2, PublicFunction.GetHeight() / 2 - mGridSize.y / 2);
#else
            mRadius = PublicFunction.GetHeight() / PublicFunction.Default_Screen_Height * 300;
#endif
        if (null != mTrans)
        {
            Transform bluetoothLabel = mTrans.Find("bluetoothLabel");
            if (null != bluetoothLabel)
            {
                bluetoothLabel.localPosition = UIManager.GetWinPos(bluetoothLabel, UIWidget.Pivot.Top, 0, PublicFunction.Back_Btn_Pos.y);
                UILabel lb = bluetoothLabel.GetComponent<UILabel>();
                if (null != lb)
                {
                    lb.text = LauguageTool.GetIns().GetText("选择连接的设备");
                }
                mMaxY = bluetoothLabel.localPosition.y - 30;
            }
            Transform backbtn = mTrans.Find("backbtn");
            if (null != backbtn)
            {
                mBackBtnTweenPosition = backbtn.GetComponent<TweenPosition>();
                if (null != mBackBtnTweenPosition)
                {
                    Vector3 pos = UIManager.GetWinPos(backbtn, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    mBackBtnPos = pos;
                    backbtn.localPosition = pos - new Vector3(300, 0);
                    GameHelper.PlayTweenPosition(mBackBtnTweenPosition, pos, 0.6f);
                }
                else
                {
                    backbtn.localPosition = UIManager.GetWinPos(backbtn, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    mBackBtnPos = backbtn.localPosition;
                }
                //mBackSize = NGUIMath.CalculateRelativeWidgetBounds(backbtn).size;
            }
            
            Transform refreshbtn = mTrans.Find("refreshbtn");
            if (null != refreshbtn)
            {
                mRefreshBtnTweenPosition = refreshbtn.GetComponent<TweenPosition>();
                if (null != mRefreshBtnTweenPosition)
                {
                    Vector3 pos = UIManager.GetWinPos(refreshbtn, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    mRefreshBtnPos = pos;
                    refreshbtn.localPosition = pos + new Vector3(300, 0);
                    GameHelper.PlayTweenPosition(mRefreshBtnTweenPosition, pos, 0.6f);
                }
                else
                {
                    refreshbtn.localPosition = UIManager.GetWinPos(refreshbtn, UIWidget.Pivot.TopRight, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    mRefreshBtnPos = refreshbtn.localPosition;
                }
                
            }

            Transform skipBtn = mTrans.Find("skipBtn");
            if (null != skipBtn)
            {
                if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble)
                {
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(skipBtn, "Label");
                    if (null != lb)
                    {
                        lb.text = LauguageTool.GetIns().GetText("暂不连接");
                    }
                    Vector3 pos = UIManager.GetWinPos(skipBtn, UIWidget.Pivot.Bottom, 0, -2);
                    TweenPosition tweenPos = skipBtn.GetComponent<TweenPosition>();
                    if (null != tweenPos)
                    {
                        skipBtn.localPosition = pos + new Vector3(0, -300);
                        GameHelper.PlayTweenPosition(tweenPos, pos, 0.6f);
                    }
                    else
                    {
                        skipBtn.localPosition = pos;
                    }
                    mMinY = pos.y - 40;
                }
                else
                {
                    skipBtn.gameObject.SetActive(false);
                }
            }
            Transform device = mTrans.Find("device");
            if (null != device)
            {
                mDevicePrefabs = device.gameObject;
            }
            mRadarChart = mTrans.Find("RadarChart");
            if (null != mRadarChart)
            {
                /*for (int row = 0, rowMax = mUseArea.GetLength(0); row < rowMax; ++row)
                {
                    for (int col = 0, colMax = mUseArea.GetLength(1); col < colMax; ++col)
                    {
                        DeviceInfo info = new DeviceInfo("JIMU\nhehehehehehe\n" + (-UnityEngine.Random.Range(0, 200)));
                        AddDevice(info);
                    }
                }*/
            }
            //Timer.Add(1, 1, 6, Test);
        }
    }

    
    void OpenConnectedAnim()
    {
        if (null != mBgTexTweenAlpha)
        {
            mBgTexTweenAlpha.value = 1;
            mBgTexTweenAlpha.delay = 0;
            GameHelper.PlayTweenAlpha(mBgTexTweenAlpha, 0);
            mBgTexTweenAlpha.SetOnFinished(OpenConnectedAnimFinished);
        }
    }

    void OpenConnectedAnimFinished()
    {
        string rename = PlayerPrefs.GetString(mDeviceDict[mConnectingObj].Mac);
        if (string.IsNullOrEmpty(rename))
        {
            rename = mDeviceDict[mConnectingObj].Name;
        }
        int index = mTextureIndex - 1;
        Vector2 size = Vector2.zero;
        if (index == 9)
        {
            size = new Vector2(88, 160);
        }
        else if (index == 10)
        {
            size = new Vector2(85, 152);
        }
        else
        {
            size = new Vector2(64, 116);
        }
        ConnectingMsg.ShowMsg(rename, size);
        PlatformMgr.Instance.ConnenctBluetooth(mDeviceDict[mConnectingObj].Mac, mDeviceDict[mConnectingObj].Name);
        OnHide();
    }

    void Test()
    {
        DeviceInfo info = new DeviceInfo("JIMU\n" + Guid.NewGuid() + "\n" + (-UnityEngine.Random.Range(0, 200)));
        AddDevice(info);
    }

    void HideAllDevice()
    {
        foreach (KeyValuePair<string, GameObject> kvp in mDeviceForMacDict)
        {
            kvp.Value.SetActive(false);
        }
    }

    void AddDevice(DeviceInfo data)
    {
        if (null != mDevicePrefabs && null != mRadarChart)
        {
            string name = string.Empty;
            Vector3 pos = GetDevicePos(data, ref name);
            if (!pos.Equals(mErrorPos))
            {
                GameObject tmp = GameObject.Instantiate(mDevicePrefabs) as GameObject;
                tmp.name = name;// "blue" + (mDeviceDict.Count + 1).ToString();
                tmp.transform.parent = mRadarChart;
                tmp.transform.localScale = Vector3.one;
                tmp.transform.localEulerAngles = Vector3.zero;
                tmp.transform.localPosition = pos;
                UILabel label = GameHelper.FindChildComponent<UILabel>(tmp.transform, "name");
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
                    if (!string.IsNullOrEmpty(mac) && data.Mac.Equals(mac))
                    {
                        label.color = Color.green;
                    }
                    string rename = PlatformMgr.Instance.GetNameForMac(data.Mac);
                    if (string.IsNullOrEmpty(rename))
                    {
                        label.text = data.Name;
                    }
                    else
                    {
                        label.text = rename;
                    }
                }
                
                tmp.SetActive(true);
                mDeviceDict[tmp] = data;
                mDeviceForMacDict[data.Mac] = tmp;
                MyTweenAlpha tween = tmp.GetComponent<MyTweenAlpha>();
                if (null != tween)
                {
                    tween.value = 0.1f;
                    GameHelper.PlayMyTweenAlpha(tween, 1, 1);
                }
                
                UIManager.SetButtonEventDelegate(tmp.transform, mBtnDelegate);
            }
        }
    }


    void OnFoundDevice(EventArg arg)
    {
        try
        {
            DeviceInfo info = arg[0] as DeviceInfo;
            if (info == null) return;
            if (info.Name.StartsWith("Jimuspk_"))
            {
                return;
            }
            if (info.Name.StartsWith("JIMU") || info.Name.StartsWith("Jimu") || info.Name.StartsWith("jimu"))
            {
                if (mDeviceForMacDict.ContainsKey(info.Mac))
                {
                    MyTweenAlpha tween = mDeviceForMacDict[info.Mac].GetComponent<MyTweenAlpha>();
                    if (null != tween)
                    {
                        tween.value = 0.1f;
                        GameHelper.PlayMyTweenAlpha(tween, 1, 1);
                    }
                    mDeviceForMacDict[info.Mac].SetActive(true);
                }
                else
                {
                    AddDevice(info);
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

    void OnConnenctResult(EventArg arg)
    {
        try
        {
            bool result = (bool)arg[0];
            if (result)
            {
                //OnClose();
                OnHide();
            }
            else
            {
                if (isConnecting)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("LianJieShiBai"));
                    ConnectingMsg.HideMsg();
                    HideAllDevice();
                    if (-1 != mSearchIndex)
                    {
                        Timer.Cancel(mSearchIndex);
                        mSearchIndex = -1;
                    }
                    SearchBlue();
                }
                //NetWaitMsg.CloseWait();
                //Show();
            }
            isConnecting = false;
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
#if USE_RECTANGLE
    Vector3 GetDevicePos(DeviceInfo data, ref string name)
    {
        if (null == mRandomList)
        {
            mRandomList = UseAreaToList();
        }
        float mod = Mathf.Abs(data.RSSI + 50 / 200.0f);
        int startIndex = (int)(mRandomList.Count * mod);
        if (startIndex >= mRandomList.Count)
        {
            startIndex = 0;
        }
        int endCount = mRandomList.Count;
        do 
        {
            for (int i = startIndex, imax = endCount; i < imax; ++i)
            {
                if (!mUseArea[mRandomList[i].num1, mRandomList[i].num2])
                {
                    mUseArea[mRandomList[i].num1, mRandomList[i].num2] = true;
                    Vector3 pos = GetPos(mRandomList[i].num1, mRandomList[i].num2);

                    if (pos.y > mMaxY)
                    {
                        continue;
                    }
                    if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble)
                    {
                        if (pos.y < mMinY)
                        {
                            continue;
                        }
                    }
                    if (Vector3.Distance(pos, mBackBtnPos) < 100)
                    {
                        continue;
                    }
                    if (Vector3.Distance(pos, mRefreshBtnPos) < 100)
                    {
                        continue;
                    }
                    if (Mathf.Abs(pos.x) < mGridSize.x / 2 && Mathf.Abs(pos.y) < mGridSize.y / 2 + 50)
                    {
                        continue;
                    }
                    bool successFlag = true;
                    /*int rs = mRandomList[i].num1 - 1 < 0 ? 0 : mRandomList[i].num1 - 1;
                    int re = mRandomList[i].num1 + 1 >= mUseArea.GetLength(0) ? mUseArea.GetLength(0) - 1 : mRandomList[i].num1 + 1;
                    int cs = mRandomList[i].num2 - 1 < 0 ? 0 : mRandomList[i].num2 - 1;
                    int ce = mRandomList[i].num2 + 1 >= mUseArea.GetLength(1) ? mUseArea.GetLength(1) - 1 : mRandomList[i].num2 + 1;
                    
                    for (int row = rs; row <= re; ++row)
                    {
                        for (int col = cs; col <= ce; ++col)
                        {
                            if (mUseArea[row, col])
                            {//已使用
                                string tmpName = "blue_" + row + "_" + col;
                                Transform tmpTrans = mRadarChart.Find(tmpName);
                                if (null != tmpTrans)
                                {
                                    if (Mathf.Abs(pos.x - tmpTrans.localPosition.x) < mGridSize.x * 0.67f || Mathf.Abs(pos.y - tmpTrans.localPosition.y) < mGridSize.y * 0.67f)
                                    {
                                        successFlag = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!successFlag)
                        {
                            break;
                        }
                    }*/
                    if (successFlag)
                    {
                        name = "blue_" + mRandomList[i].num1 + "_" + mRandomList[i].num2;
                        return pos;
                    }
                }
            }
            if (0 == startIndex)
            {
                break;
            }
            endCount = startIndex;
            startIndex = 0;
        } while (true);
        
        return mErrorPos;
    }

    Vector3 GetPos(int row, int col)
    {
        Vector3 pos = mStartPos + new Vector2(row * mGridSize.x, -col * mGridSize.y);
        float offsetX = UnityEngine.Random.Range(-30, 30);
        float offsetY = UnityEngine.Random.Range(-15, 15);
        pos += new Vector3(offsetX, offsetY);
        return pos;
    }

    List<Int2> UseAreaToList()
    {
        int rowMax = mUseArea.GetLength(0);
        int colMax = mUseArea.GetLength(1);
        int cid = colMax / 2;
        int rid = rowMax / 2;
        List<Int2> list = new List<Int2>();
        Int2 num = new Int2();
        num.num1 = rid;
        num.num2 = cid;
        list.Add(num);
        int rowSt = rid - 1;
        int rowEd = rid + 1;
        int colSt = cid - 1;
        int colEd = cid + 1;
        while (rowSt >= 0 || rowEd < rowMax || colSt >= 0 || colEd < colMax)
        {
            List<Int2> tmpList = GetList(rowSt, rowEd, colSt, colEd);
            while (tmpList.Count > 0)
            {
                int count = UnityEngine.Random.Range(0, tmpList.Count);
                int index = UnityEngine.Random.Range((int)(list.Count * mRandomModulus), (int)(list.Count * 1.2f));
                if (index < list.Count)
                {
                    list.Insert(index, tmpList[count]);
                }
                else
                {
                    list.Add(tmpList[count]);
                }
                tmpList.RemoveAt(count);
            }
            --rowSt;
            ++rowEd;
            --colSt;
            ++colEd;
        }
        return list;
    }

    List<Int2> GetList(int rowSt, int rowEd, int colSt, int colEd)
    {
        List<Int2> list = new List<Int2>();
        int rowMax = mUseArea.GetLength(0);
        int colMax = mUseArea.GetLength(1);
        for (int col = colSt; col <= colEd; ++col)
        {
            for (int row = rowSt; row <= rowEd; ++row)
            {
                if (row < 0 || row >= rowMax || col < 0 || col >= colMax)
                {
                    continue;
                }
                if (row == rowSt || row == rowEd || col == colSt || col == colEd)
                {
                    Int2 num = new Int2();
                    num.num1 = row;
                    num.num2 = col;
                    list.Add(num);
                }
            }
        }
        return list;
    }
#else
    Vector3 GetDevicePos(DeviceInfo data)
    {        
        return GetPos(data.RSSI, 60, 1);
    }

    Vector3 GetPos(int rssi, float offset, int count)
    {
        float radiusMin = mRadius * Math.Abs((rssi + offset) / 200);
        float radiusMax = mRadius * Math.Abs((rssi - offset) / 200);
        
        /*if (radiusMin > mRadius)
        {
            return mErrorPos;
        }*/
        if (radiusMin < 0 || radiusMin >= mRadius)
        {
            radiusMin = 0;
        }
        if (radiusMax > mRadius)
        {
            radiusMax = mRadius;
        }
        //求出圆环该分成多少个区域
        /*int xNum = (int)((radiusMax + radiusMin) / 2 / mGridSize.x);
        int yNum = (int)((radiusMax + radiusMin) / 2 / mGridSize.y);
        int angleNum = Mathf.Min(xNum, yNum);
        if (angleNum < 3)
        {
            angleNum = 3;
        }*/
        
        int radNum = (int)((radiusMax - radiusMin) / Mathf.Min(mGridSize.x, mGridSize.y) * 3);
        if (radNum <= 0)
        {
            List<int> angleList = new List<int>();
            for (int i = 0; i < 10; ++i)
            {
                angleList.Add(i);
            }
            float radius = UnityEngine.Random.Range(radiusMin, radiusMax);
            Vector3 pos = GetPos(radius, UnityEngine.Random.Range(0, (float)(2 * Math.PI)), angleList);
            if (!pos.Equals(mErrorPos))
            {
                return pos;
            }
        }
        else
        {
            List<int> radList = new List<int>();
            for (int i = 0; i < radNum; ++i)
            {
                radList.Add(i);
            }
            int totalRadNum = radList.Count;
            while (radList.Count > 0)
            {
                int radIndex = UnityEngine.Random.Range(0, radList.Count);

                float radius = radiusMin + (radiusMax - radiusMin) * radList[radIndex] / totalRadNum + UnityEngine.Random.Range(-(radiusMax - radiusMin) / totalRadNum / 3, (radiusMax - radiusMin) / totalRadNum / 3);
                List<int> angleList = new List<int>();
                for (int i = 0; i < 10; ++i)
                {
                    angleList.Add(i);
                }
                Vector3 pos = GetPos(radius, UnityEngine.Random.Range(0, (float)(2 * Math.PI)), angleList);
                if (!pos.Equals(mErrorPos))
                {
                    return pos;
                }
                radList.RemoveAt(radIndex);
            }
        }
        if (count > 10)
        {
            return mErrorPos;
        }
        return GetPos(rssi, 2 * offset, ++count);
    }

    Vector3 GetPos(float radius, float startAngle, List<int> angleList)
    {
        int totalCount = angleList.Count;
        while (angleList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, angleList.Count);
            float angleSpace = (float)(2 * Math.PI / totalCount);
            float angle = startAngle + (float)(angleSpace * angleList[index]);
            //angle += UnityEngine.Random.Range(angleSpace / 3, angleSpace / 3);
            Vector3 pos = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0);
            if (null == mDeviceDict || mDeviceDict.Count <= 0)
            {
                return pos;
            }
            bool breakFlag = false;
            foreach (KeyValuePair<GameObject, string> kvp in mDeviceDict)
            {
                if (Vector3.Distance(pos, kvp.Key.transform.localPosition) < GridDistanceMin)
                {
                    breakFlag = true;
                    break;
                }
            }
            if (breakFlag)
            {
                angleList.RemoveAt(index);
            }
            else
            {
                return pos;
            }

        }
        return mErrorPos;
    }
#endif

    Texture GetBgTextrue(int index)
    {
        string name = mBgTexFont + index;
        if (mTextureDict.ContainsKey(name))
        {
            return mTextureDict[name];
        }
        Texture tex = ResourcesEx.Load<Texture>("Texture/BlueSearch/" + name);
        if (null != tex)
        {
            mTextureDict[name] = tex;
            return tex;
        }
        return null;
    }

    void OpenBluetoothOnClick(GameObject obj)
    {
        try
        {
            string name = obj.name;
            if (name.Equals(PromptMsg.RightBtnName))
            {
                if (-1 != mSearchIndex)
                {
                    Timer.Cancel(mSearchIndex);
                    mSearchIndex = -1;
                }
                SearchBlue();
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError("PublicPrompt" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    #endregion
}