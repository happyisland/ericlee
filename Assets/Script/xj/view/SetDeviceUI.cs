using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
using Game.Platform;
using Game.UI;

/// <summary>
/// Author:xj
/// FileName:SetDeviceUI.cs
/// Description:
/// Time:2015/8/29 10:50:33
/// </summary>
public class SetDeviceUI : BaseUI
{
    #region 公有属性
    #endregion

    #region 其他属性
    Transform mScrollViewTrans;
    Transform mUIGridTrans;
    GameObject mItemPrefab;
    UIScrollView mScrollView;
    UIGrid mGrid;
    List<byte> mDeviceDjIDList;
    List<byte> mOldIds;
    List<UIInput> mInputList;
    Dictionary<int, int> mSameDict;
    UISprite mConnectIcon;
    UIButton mConnectBtn;
    bool mConnected;

    UIButton mSaveBtn;
    UIButton mConfirmBtn;
    UILabel deviceCurIdss;

    Color32 Normal_Color = new Color32(125, 56, 0, 255);

    TweenAlpha mTipsTweenAlpha;
    GameObject mSaveBtnObj;

    bool isOldConnected = false;

    public enum tipIndex
    {
        tip_first,
        tip_second,
        tip_third,
    }

    tipIndex tipCurIndex;
    UIInput servoIdInput;
    Transform successDialog;
    Transform nextBtn, connectBtn;
    Transform img1, img2, img3;

    TopologyPartType mDevicePartType = TopologyPartType.Servo;
    #endregion

    #region 公有函数
    public SetDeviceUI()
    {
        mUIResPath = "Prefab/UI/SetDeviceID";
        mDeviceDjIDList = new List<byte>();
        mSameDict = new Dictionary<int, int>();
        mInputList = new List<UIInput>();
        mOldIds = new List<byte>();
    }
    #endregion

    #region 其他函数

    protected override void AddEvent()
    {
        try
        {
            base.AddEvent();
            EventMgr.Inst.Regist(EventID.Set_Device_ID_ReadData_Result, ReadDeviceResult);
            EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
            EventMgr.Inst.Regist(EventID.Set_Device_ID_Msg_Ack, ChangeCallBack);
            EventMgr.Inst.Regist(EventID.Change_Sensor_ID_Msg_Ack, ChangeSensorCallBack);
            RobotManager.GetInst().IsSetDeviceIDFlag = true;

            if (null != mTrans)
            {
                Transform backBtn = mTrans.Find("list/backBtn");
                if (null != backBtn)
                {
                    backBtn.localPosition = UIManager.GetWinPos(backBtn, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    /*TweenPosition tween = backBtn.GetComponent<TweenPosition>();
                    if (null != tween)
                    {
                        Vector3 pos = UIManager.GetWinPos(backBtn, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                        backBtn.localPosition = pos - new Vector3(300, 0);
                        GameHelper.PlayTweenPosition(tween, pos, 0.6f);
                    }
                    else
                    {
                        backBtn.localPosition = UIManager.GetWinPos(backBtn, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                    }*/
                }

                Transform title = mTrans.Find("list/title");
                if (null != title)
                {
                    title.localPosition = UIManager.GetWinPos(title, UIWidget.Pivot.Top, 0, 60);
                    UILabel lb = GameHelper.FindChildComponent<UILabel>(title, "Label");
                    if (null != lb)
                    {
                        lb.text = LauguageTool.GetIns().GetText("修改ID");
                    }
                }

                mScrollViewTrans = mTrans.Find("list/ScrollView");
                if (null != mScrollViewTrans)
                {
                    mScrollView = mScrollViewTrans.GetComponent<UIScrollView>();
                    mUIGridTrans = mScrollViewTrans.Find("UIGrid");
                    if (null != mUIGridTrans)
                    {
                        mGrid = mUIGridTrans.GetComponent<UIGrid>();
                    }
                    mItemPrefab = mScrollViewTrans.Find("Item").gameObject;
                }

                successDialog = mTrans.Find("successTips");
                if (null != successDialog)
                {
                    Transform diaTips = successDialog.Find("tips");
                    diaTips.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("修改ID成功提示");

                    mConfirmBtn = successDialog.Find("confirmBtn").GetComponent<UIButton>();

                    successDialog.gameObject.SetActive(false);
                }

                Transform saveBtn = mTrans.Find("list/saveBtn");
                if (null != saveBtn)
                {
                    mSaveBtn = saveBtn.GetComponent<UIButton>();
                    if (null != mSaveBtn)
                    {
                        saveBtn.localPosition = UIManager.GetWinPos(saveBtn, UIWidget.Pivot.Bottom, 0, -2);
                        mSaveBtn.OnSleep();
                    }

                    /*TweenPosition tween = saveBtn.GetComponent<TweenPosition>();
                    if (null != tween)
                    {
                        Vector3 pos = UIManager.GetWinPos(saveBtn, UIWidget.Pivot.Bottom, 0, -2);
                        saveBtn.localPosition = pos - new Vector3(0, 150);
                        GameHelper.PlayTweenPosition(tween, pos, 0.6f);
                    }
                    else
                    {
                        saveBtn.localPosition = UIManager.GetWinPos(saveBtn, UIWidget.Pivot.Bottom, 0, -2);
                    }*/
                    
                    Transform label = saveBtn.Find("Label");
                    if (null != label)
                    {
                        UILabel lb = label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("确定");
                        }
                    }
                    mSaveBtnObj = saveBtn.gameObject;
                }
                nextBtn = mTrans.Find("list/nextBtn");
                if (null != nextBtn)
                {
                    nextBtn.gameObject.SetActive(true);
                    if (null != nextBtn.GetComponent<UIButton>())
                    {
                        nextBtn.localPosition = UIManager.GetWinPos(nextBtn, UIWidget.Pivot.Bottom, 0, -2);
                        //mSaveBtn.OnSleep();
                    }

                    Transform label = nextBtn.Find("Label");
                    if (null != label)
                    {
                        UILabel lb = label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("下一步");
                        }
                    }
                }
                connectBtn = mTrans.Find("list/connectBtn");
                if (null != connectBtn)
                {
                    if (null != connectBtn.GetComponent<UIButton>())
                    {
                        connectBtn.localPosition = UIManager.GetWinPos(connectBtn, UIWidget.Pivot.Bottom, 0, -2);
                        //mSaveBtn.OnSleep();
                    }

                    Transform label = connectBtn.Find("Label");
                    if (null != label)
                    {
                        UILabel lb = label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("连接");
                        }
                    }
                    connectBtn.gameObject.SetActive(false);
                }

                Transform tips = mTrans.Find("list/tips");
                if (null != tips)
                {
                    mTipsTweenAlpha = tips.GetComponent<TweenAlpha>();
                    Transform Label = tips.Find("Label");
                    if (null != Label)
                    {
                        int width = 0;
                        UILabel lb = Label.GetComponent<UILabel>();
                        if (null != lb)
                        {
                            lb.width = PublicFunction.GetWidth() - 128;
                            width = lb.width;
                            lb.text = LauguageTool.GetIns().GetText("修改舵机ID提示语");
                        }
                        Label.localPosition = new Vector3(-width / 2, (0.5f - 0.25f) * PublicFunction.GetHeight());
                    }

                    Transform img = tips.Find("img");
                    if (null != img)
                    {
                        tipCurIndex = tipIndex.tip_first;
                        img.localPosition = new Vector3(0, (0.5f - 0.6f) * PublicFunction.GetHeight());
                        img1 = img.Find("img1");
                        if (null != img1)
                        {
                            //img1.localPosition = new Vector3(UIManager.GetWinPos(img1, UIWidget.Pivot.Left, 160).x, 0);
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(img1, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("1.请先找到重复ID的舵机");
                            }
                        }
                        img2 = img.Find("img2");
                        if (null != img2)
                        {
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(img2, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("2.保留一个舵机连接主板");
                            }
                        }
                        img3 = img.Find("img3");
                        if (null != img3)
                        {
                            //img3.localPosition = new Vector3(UIManager.GetWinPos(img3, UIWidget.Pivot.Right, 160).x, 0);
                            UILabel lb = GameHelper.FindChildComponent<UILabel>(img3, "Label");
                            if (null != lb)
                            {
                                lb.text = LauguageTool.GetIns().GetText("3.手机连接蓝牙修改ID");
                            }
                        }
                        img1.gameObject.SetActive(true);
                        img2.gameObject.SetActive(false);
                        img3.gameObject.SetActive(false);
                    }
                }               
            }

            if (PlatformMgr.Instance.GetBluetoothState())
                PlatformMgr.Instance.DisConnenctBuletooth();

            mSaveBtnObj.SetActive(false);

            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot && robot.GetDjNum() == 1 && PlatformMgr.Instance.GetBluetoothState())
            {
                NetWork.GetInst().ClearAllMsg();
                Robot setRobot = RobotManager.GetInst().GetSetDeviceRobot();
                if (null != setRobot)
                {
                    string mac = robot.Mac;
                    setRobot.ConnectRobotResult(mac, true);
                    setRobot.HandShake();
                    NetWaitMsg.ShowWait();
                    //2秒以后读取初始角度
                    Timer.Add(2, 0, 1, robot.ReadMotherboardData);
                    isOldConnected = true;
                }
                if (null != mTipsTweenAlpha)
                {
                    mTipsTweenAlpha.gameObject.SetActive(false);
                }
            }
            else
            {
                PlatformMgr.Instance.DisConnenctBuletooth();
                mSaveBtnObj.SetActive(false);
            }
            SetConnectState();
            /*ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck();
            for (byte i = 1; i <= 1; ++i)
            {
                msg.ids.Add(i);
            }
            EventMgr.Inst.Fire(EventID.Set_Device_ID_ReadData_Result, new EventArg(msg));*/
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }       
    }

    void InitItem()
    {
        if (null != mItemPrefab && null != mUIGridTrans && null != mGrid)
        {
            for (int i = 0, imax = mDeviceDjIDList.Count; i < imax; ++i)
            {
                AddItem(i, mDeviceDjIDList[i]);
            }
            mGrid.repositionNow = true;
        }
    }

    void AddItem(int index, int id)
    {
        GameObject obj = GameObject.Instantiate(mItemPrefab) as GameObject;
        if (null != obj)
        {
            obj.name = index.ToString();
            obj.SetActive(true);
            obj.transform.parent = mUIGridTrans;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            SetItem(index, id, obj);
        }
    }

    void SetItem(int index, int id, GameObject obj)
    {
        Transform input = obj.transform.Find("Input");
        UIInput tmpInput = null;
        if (null != input)
        {
            tmpInput = input.GetComponent<UIInput>();
            servoIdInput = input.GetComponent<UIInput>();
            if (null != tmpInput)
            {
                tmpInput.value = id.ToString();
                //SetInputColor(tmpInput, Normal_Color);
                if (null == tmpInput.onSelect)
                {
                    tmpInput.onSelect = OnInputSelect;
                    tmpInput.onValidate = OnValidate;
                }
            }
            input.name = index.ToString();
        }
        UILabel lb = GameHelper.FindChildComponent<UILabel>(obj.transform, "Label");
        if (null != lb)
        {
            lb.text = LauguageTool.GetIns().GetText("设备ID:");
        }
        mInputList.Add(tmpInput);
        if (mDevicePartType == TopologyPartType.Servo)
        {
            deviceCurIdss = GameHelper.FindChildComponent<UILabel>(obj.transform, "duoji/id");
            if (null != deviceCurIdss)
            {
                deviceCurIdss.text = id.ToString();
            }
        }
        else
        {
            Transform duoji = obj.transform.Find("duoji");
            if (null != duoji)
            {
                duoji.gameObject.SetActive(false);
            }
            Transform sensor = obj.transform.Find("sensor");
            if (null != sensor)
            {
                sensor.gameObject.SetActive(true);
                deviceCurIdss = GameHelper.FindChildComponent<UILabel>(sensor, "Label");
                if (null != deviceCurIdss)
                {
                    deviceCurIdss.text = id.ToString();
                }
                TopologyUI.SetSensorBg(sensor.gameObject, mDevicePartType);
                TopologyUI.SetSensorIcon(sensor.gameObject, mDevicePartType);
            }
        }
        
    }

    void SetInputColor(UIInput input, Color color)
    {
        input.activeTextColor = color;
        UILabel lb = GameHelper.FindChildComponent<UILabel>(input.transform, "Label");
        if (null != lb)
        {
            lb.color = color;
        }
    }
    protected override void Close()
    {
        base.Close();
        EventMgr.Inst.UnRegist(EventID.Set_Device_ID_ReadData_Result, ReadDeviceResult);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnConnenctResult);
        EventMgr.Inst.UnRegist(EventID.Set_Device_ID_Msg_Ack, ChangeCallBack);
        EventMgr.Inst.UnRegist(EventID.Change_Sensor_ID_Msg_Ack, ChangeSensorCallBack);
        RobotManager.GetInst().IsSetDeviceIDFlag = false;
        PlatformMgr.Instance.DisConnenctBuletooth();
        /*Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot && robot.Connected)
        {
            robot.ConnectRobotResult(robot.Mac, true);
        }*/
        /*if (!mOldConnected)
        {
            EventMgr.Inst.Fire(EventID.Change_Device_ID);
        }*/
    }

    IEnumerator DelayShowSuccessTips(float t)
    {
        yield return new WaitForSeconds(t);
        successDialog.gameObject.SetActive(true);
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Equals("saveBtn"))
        {
            //bool checkResults = CheckID();
            //Debug.Log("now check Result is " + checkResults);
            if (CheckID())
            {//可以发包了
                if (mConnected && mOldIds.Count > 0 && mDeviceDjIDList.Count > 0)
                {
                    //mDeviceDjIDList.Sort();
                    Robot robot = RobotManager.GetInst().GetSetDeviceRobot();
                    if (null != robot)
                    {
                        NetWaitMsg.ShowWait(2);
                        if (mDevicePartType == TopologyPartType.Servo)
                        {
                            robot.ChangeDeviceId(mOldIds[0], mDeviceDjIDList);
                            //curModifyId = servoIdInput.value;
                            //ClientMain.GetInst().StartCoroutine(DelayShowSuccessTips(2.0f));
                        }
                        else
                        {
                            robot.ChangeSensorID(mDevicePartType, mOldIds[0], mDeviceDjIDList[0]);
                            //curModifyId = servoIdInput.value;
                            //ClientMain.GetInst().StartCoroutine(DelayShowSuccessTips(2.0f));
                        }
                    }
                }
            }
        }
        if (obj.name.Equals("confirmBtn"))
        {
            if (null != successDialog)
                successDialog.gameObject.SetActive(false);
        }
        else if (obj.name.Equals("nextBtn"))
        {
            if (tipCurIndex == tipIndex.tip_first)
            {
                img1.gameObject.SetActive(false);
                img2.gameObject.SetActive(true);
                img3.gameObject.SetActive(false);
                tipCurIndex = tipIndex.tip_second;
            }
            else if (tipCurIndex == tipIndex.tip_second)
            {
                img1.gameObject.SetActive(false);
                img2.gameObject.SetActive(false);
                img3.gameObject.SetActive(true);
                tipCurIndex = tipIndex.tip_third;
                nextBtn.gameObject.SetActive(false);
                connectBtn.gameObject.SetActive(true);
            }
        }
        else if (obj.name.Equals("backBtn"))
        {
            if (tipCurIndex == tipIndex.tip_first)
            {
                OnClose();
                SceneManager.GetInst().CloseCurrentScene();
                Timer.Add(0.001f, 1, 1, BackApp);
            }
            else if (tipCurIndex == tipIndex.tip_second)
            {
                img1.gameObject.SetActive(true);
                img2.gameObject.SetActive(false);
                img3.gameObject.SetActive(false);
                tipCurIndex = tipIndex.tip_first;
            }
            else if (tipCurIndex == tipIndex.tip_third)
            {
                img1.gameObject.SetActive(false);
                img2.gameObject.SetActive(true);
                img3.gameObject.SetActive(false);
                tipCurIndex = tipIndex.tip_second;
                nextBtn.gameObject.SetActive(true);
                connectBtn.gameObject.SetActive(false);
            }
        }
        else if (obj.name.Equals("connectBtn"))
        {
            tipCurIndex = tipIndex.tip_first;
            /*ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck();
            msg.ids.Add(1);
            ReadDeviceResult(new EventArg(msg));
            connectBtn.gameObject.SetActive(false);
            return;*/
            //ReadDeviceResult(null);
            PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedConnectBluetoothButton);
            
            if (mConnected)
            {
                PlatformMgr.Instance.DisConnenctBuletooth();
            }
            else
            {
                //PopWinManager.GetInst().ShowPopWin(typeof(ConnenctBluetoothMsg));
                ConnectBluetoothMsg.ShowMsg();
            }
        }
    }

    void BackApp()
    {
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ExitSetupSteeringEngineID, string.Empty);
    }

    void OnInputSelect(bool isSelect, GameObject obj)
    {
        try
        {
            if (!isSelect)
            {
                if (mDeviceDjIDList.Count != mInputList.Count)
                {
                    return;
                }
                int index = int.Parse(obj.name);
                if (index >= 0 && index < mDeviceDjIDList.Count)
                {
                    if (!PublicFunction.IsInteger(mInputList[index].value))
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DuoJiIDZhengShu"));
                        mInputList[index].value = mDeviceDjIDList[index].ToString();
                        return;
                    }
                    mInputList[index].value = mInputList[index].value.TrimStart('0');
                    int newId = 0;
                    if (!string.IsNullOrEmpty(mInputList[index].value))
                    {
                        newId = int.Parse(mInputList[index].value);
                    }
                    if (mDevicePartType == TopologyPartType.Servo)
                    {
                        if (newId < PublicFunction.DuoJi_Id_Min || newId > PublicFunction.DuoJi_Id_Max)
                        {
                            HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiIDFanWei"), PublicFunction.DuoJi_Id_Min, PublicFunction.DuoJi_Id_Max));
                            mInputList[index].value = mDeviceDjIDList[index].ToString();
                            return;
                        }
                    }
                    else if (mDevicePartType == TopologyPartType.Gyro)
                    {
                        if (newId < 1 || newId > 2)
                        {
                            HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiIDFanWei"), 1, 2));
                            mInputList[index].value = mDeviceDjIDList[index].ToString();
                            return;
                        }
                    }
                    else if (mDevicePartType == TopologyPartType.Speaker)
                    {
                        // 蓝牙音箱不可修改ID
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("禁止修改ID提示"));
                        return;
                    }
                    else
                    { 
                        if (newId < PublicFunction.Sensor_ID_Min || newId > PublicFunction.Sensor_ID_Max)
                        {
                            HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiIDFanWei"), PublicFunction.Sensor_ID_Min, PublicFunction.Sensor_ID_Max));
                            mInputList[index].value = mDeviceDjIDList[index].ToString();
                            return;
                        }
                    }

                    int oldId = mDeviceDjIDList[index];
                    if (mSameDict.ContainsKey(oldId))
                    {
                        mSameDict[oldId]--;
                    }
                    mDeviceDjIDList[index] = (byte)newId;
                    if (mSameDict.ContainsKey(newId))
                    {
                        mSameDict[newId]++;
                    }
                    else
                    {
                        mSameDict[newId] = 1;
                    }
                }
                
                for (int i = 0, imax = mDeviceDjIDList.Count; i < imax; ++i)
                {
                    int id = mDeviceDjIDList[i];
                    if (mSameDict.ContainsKey(id) && mSameDict[id] > 1)
                    {
                        //SetInputColor(mInputList[i], Color.red);
                    }
                    else
                    {
                        //SetInputColor(mInputList[i], Normal_Color);
                    }
                }

                CheckBtnState();
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
        if (mDevicePartType == TopologyPartType.Servo)
        {
            if (tmpLenght >= 2)
            {//限制长度
                return (char)0;
            }
        }
        else
        {
            if (tmpLenght >= 1)
            {//限制长度
                return (char)0;
            }
        }
        
        if (addedChar >= '0' && addedChar <= '9')
        {
            ++tmpLenght;
        }
        else
        {
            return (char)0;
        }
        return addedChar;
    }

    void SetConnectState()
    {
        string iconName;
        if (PlatformMgr.Instance.GetBluetoothState())
        {
            iconName = "connect";
        }
        else
        {
            iconName = "disconnect";
        }
        /*if (null != mConnectBtn)
        {
            mConnectBtn.normalSprite = iconName;
        }*/
        if (null != mConnectIcon)
        {
            mConnectIcon.spriteName = iconName;
            mConnectIcon.MyMakePixelPerfect();
        }
    }

    bool CheckID()
    {
        do 
        {
            int imax = mDeviceDjIDList.Count;
            if (imax < 1)
            {
                return true;
            }
            int minId = int.MaxValue;
            int maxId = int.MinValue;
            int idSum = 0;
            for (int i = 0; i < imax; ++i)
            {
                if (mDevicePartType == TopologyPartType.Servo)
                {
                    if (mDeviceDjIDList[i] < PublicFunction.DuoJi_Id_Min || mDeviceDjIDList[i] > PublicFunction.DuoJi_Id_Max)
                    {
                        HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiIDFanWei"), PublicFunction.DuoJi_Id_Min, PublicFunction.DuoJi_Id_Max));
                        return false;
                    }
                }
                else if (mDevicePartType == TopologyPartType.Gyro)
                {
                    if ((mDeviceDjIDList[i] < 1) || (mDeviceDjIDList[i] > 2))
                    {
                        HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiIDFanWei"), 1, 2));
                        return false;
                    }
                }
                else if (mDevicePartType == TopologyPartType.Speaker)
                {
                    // 蓝牙音箱不可修改ID
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("禁止修改ID提示"));
                    return false;
                }
                else
                {
                    if ((mDeviceDjIDList[i] < PublicFunction.Sensor_ID_Min) || (mDeviceDjIDList[i] > PublicFunction.Sensor_ID_Max))
                    {
                        HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiIDFanWei"), PublicFunction.Sensor_ID_Min, PublicFunction.Sensor_ID_Max));
                        return false;
                    }
                }
                
                if (mDeviceDjIDList[i] < minId)
                {
                    minId = mDeviceDjIDList[i];
                }
                if (mDeviceDjIDList[i] > maxId)
                {
                    maxId = mDeviceDjIDList[i];
                }
                idSum += mDeviceDjIDList[i];
            }
            foreach (KeyValuePair<int, int> kvp in mSameDict)
            {
                if (kvp.Value > 1)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("ChongFuDuoJiID"));
                    return false;
                }
            }
            /*if (Mathf.Abs((minId + maxId) * imax / 2.0f - idSum) > 0.1f)
            {//id不连续
                HUDTextTips.ShowTextTip("舵机必须连续");
                return false;
            }*/
            
        } while (false);
        return true;
    }

    void CleanUp()
    {
        if (null != mUIGridTrans)
        {
            for (int i = 0, icount = mUIGridTrans.childCount; i < icount; ++i)
            {
                Transform tmp = mUIGridTrans.GetChild(i);
                if (null != tmp)
                {
                    GameObject.Destroy(tmp.gameObject);
                }
            }
        }
        mDeviceDjIDList.Clear();
        mSameDict.Clear();
        mInputList.Clear();
        mOldIds.Clear();
        isOldConnected = false;
    }

    void ReadDeviceResult(EventArg arg)
    {
        try
        {
            mConnected = true;
            SetConnectState();
            ReadMotherboardDataMsgAck msg = (ReadMotherboardDataMsgAck)arg[0];
            if (null != msg)
            {
                CleanUp();
                for (int i = 0, imax = msg.ids.Count; i < imax; ++i)
                {
                    mDeviceDjIDList.Add(msg.ids[i]);
                    mSameDict.Add(msg.ids[i], 1);
                    mOldIds.Add(msg.ids[i]);
                }
                if (msg.ids.Count == 1)
                {
                    mDevicePartType = TopologyPartType.Servo;
                }
                else
                {
                    TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
                    for (int i = 0, imax = partType.Length; i < imax; ++i)
                    {
                        SensorData data = msg.GetSensorData(partType[i]);
                        if (null != data && data.ids.Count == 1)
                        {
                            for (int sensorIndex = 0, sensorMax = data.ids.Count; sensorIndex < sensorMax; ++sensorIndex)
                            {
                                mDeviceDjIDList.Add(data.ids[sensorIndex]);
                                mSameDict.Add(data.ids[sensorIndex], 1);
                                mOldIds.Add(data.ids[sensorIndex]);
                            }
                            mDevicePartType = partType[i];
                            break;
                        }
                    }
                }
                if (PlatformMgr.Instance.GetBluetoothState())
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接成功修改舵机提示"), HUDTextTips.Color_Green);
                InitItem();
                if (null != mSaveBtnObj)
                {
                    mSaveBtnObj.SetActive(true);
                    CheckBtnState();
                }
                GameHelper.PlayTweenAlpha(mTipsTweenAlpha, 0);
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
            if (!result)
            {
                mConnected = false;
                CleanUp();
            }
            else
            {
                connectBtn.gameObject.SetActive(false);

                if (!isOldConnected)
                {
                    PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionSucceeded);
                }
            }
            SetConnectState();
            isOldConnected = false;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void DelayShowTips()
    {
        for (int i = 0, imax = mInputList.Count; i < imax; ++i)
        {
            if (mDevicePartType == TopologyPartType.Servo)
            {
                UILabel lb = GameHelper.FindChildComponent<UILabel>(mInputList[i].transform.parent, "duoji/id");
                if (null != lb)
                {
                    lb.text = mInputList[i].value;
                }
            }
            else
            {
                UILabel lb = GameHelper.FindChildComponent<UILabel>(mInputList[i].transform.parent, "sensor/Label");
                if (null != lb)
                {
                    lb.text = mInputList[i].value;
                }
            }
            
        }
        if (mDevicePartType == TopologyPartType.Servo)
        {
            //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机ID修改成功，请及时更换舵机的ID标签！"));
            //curModifyId = servoIdInput.value;
            if (successDialog != null)
                successDialog.gameObject.SetActive(true);
        }
        else
        {
            //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("ID修改成功，请及时更换设备的ID标签！"));
            //curModifyId = servoIdInput.value;
            if (successDialog != null)
                successDialog.gameObject.SetActive(true);
        }
    }
    void ChangeCallBack(EventArg arg)
    {
        /*OnClose();
        EventMgr.Inst.Fire(EventID.Close_Set_Device_ID_UI);*/
        try
        {
            bool result = (bool)arg[0];
            if (result)
            {
                if (null != mOldIds && null != mDeviceDjIDList && mOldIds.Count == mDeviceDjIDList.Count)
                {
                    for (int i = 0, imax = mOldIds.Count; i < imax; ++i)
                    {
                        mOldIds[i] = mDeviceDjIDList[i];
                    }
                }
                Timer.Add(2, 1, 1, DelayShowTips);
            }
            else
            {
                if (null != mOldIds && null != mDeviceDjIDList && null != mInputList && mOldIds.Count == mDeviceDjIDList.Count && mInputList.Count == mOldIds.Count)
                {
                    for (int i = 0, imax = mOldIds.Count; i < imax; ++i)
                    {
                        if (mSameDict.ContainsKey(mDeviceDjIDList[i]))
                        {
                            mSameDict[mDeviceDjIDList[i]]--;
                        }
                        if (mSameDict.ContainsKey(mOldIds[i]))
                        {
                            mSameDict[mOldIds[i]]++;
                        }
                        else
                        {
                            mSameDict[mOldIds[i]] = 1;
                        }
                        mDeviceDjIDList[i] = mOldIds[i];
                        //mOldIds[i] = mDeviceDjIDList[i];
                        mInputList[i].value = mOldIds[i].ToString();
                    }
                }
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改失败"));
            }
            CheckBtnState();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void ChangeSensorCallBack(EventArg arg)
    {
        try
        {
            ChangeSensorIDMsgAck msg = (ChangeSensorIDMsgAck)arg[0];
            if (msg.result == (byte)ErrorCode.Result_OK)
            {
                if (null != mOldIds && null != mDeviceDjIDList && mOldIds.Count == mDeviceDjIDList.Count)
                {
                    for (int i = 0, imax = mOldIds.Count; i < imax; ++i)
                    {
                        mOldIds[i] = mDeviceDjIDList[i];
                    }
                }
                Timer.Add(2, 1, 1, DelayShowTips);
            }
            else
            {
                if (null != mOldIds && null != mDeviceDjIDList && null != mInputList && mOldIds.Count == mDeviceDjIDList.Count && mInputList.Count == mOldIds.Count)
                {
                    for (int i = 0, imax = mOldIds.Count; i < imax; ++i)
                    {
                        if (mSameDict.ContainsKey(mDeviceDjIDList[i]))
                        {
                            mSameDict[mDeviceDjIDList[i]]--;
                        }
                        if (mSameDict.ContainsKey(mOldIds[i]))
                        {
                            mSameDict[mOldIds[i]]++;
                        }
                        else
                        {
                            mSameDict[mOldIds[i]] = 1;
                        }
                        mDeviceDjIDList[i] = mOldIds[i];
                        //mOldIds[i] = mDeviceDjIDList[i];
                        mInputList[i].value = mOldIds[i].ToString();
                    }
                }
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改失败"));
            }
            CheckBtnState();
        }
        catch (System.Exception ex)
        {
        	
        }
    }

    void CheckBtnState()
    {
        bool changeFlag = false;
        for (int i = 0, imax = mDeviceDjIDList.Count; i < imax; ++i)
        {
            if (mDeviceDjIDList[i] != mOldIds[i])
            {
                changeFlag = true;
                break;
            }
        }
        if (null != mSaveBtn)
        {
            if (changeFlag)
            {
                mSaveBtn.OnAwake();
            }
            else
            {
                mSaveBtn.OnSleep();
            }
        }
    }
    #endregion
}