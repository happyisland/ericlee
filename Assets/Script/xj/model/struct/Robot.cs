using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game;
using Game.Event;
using Game.UI;
using Game.Platform;
using Game.Scene;
using Game.Resource;
using System.Text;


/// <summary>
/// Author:xj
/// FileName:Robot.cs
/// Description:机器人
/// Time:2015/6/26 13:34:00
/// </summary>
public class Robot
{
    public delegate void ConnectCallBack(bool result);
    public delegate void PlayActionsDelegate(int index, bool finished);
    #region 公有属性
    /// <summary>
    /// 机器人是否连接
    /// </summary>
    public bool Connected
    {
        get  { return connected;}
    }
    /// <summary>
    /// 获得机器人的mac地址
    /// </summary>
    public string Mac
    {
        get { return mac; }
    }
    /// <summary>
    /// 获得机器人id
    /// </summary>
    public string ID
    {
        get { return id; }
    }
    /// <summary>
    /// 获取机器人的名字
    /// </summary>
    public string Name
    {
        get { return name; }
    }
    /// <summary>
    /// 用于详细界面的显示
    /// </summary>
    public string ShowName
    {
        get
        {
            if (string.IsNullOrEmpty(showName))
            {
                return RobotMgr.NameNoType(name);
            }
            return showName;
        }
        set
        {
            showName = value;
        }
    }
    /// <summary>
    /// 机器人是否掉电
    /// </summary>
    public bool PowerDownFlag
    {
        get { return powerDownFlag; }
        set { powerDownFlag = value; EventMgr.Inst.Fire(EventID.PowerDown_ReadBack_Switch); }
    }
    /// <summary>
    /// 自检
    /// </summary>
    public bool SelfCheckFlag
    {
        get { return mSelfCheckFlag; }
    }

    public ReadMotherboardDataMsgAck MotherboardData = null;


    #endregion

    #region 私有属性
    string id;//模型的唯一id
    string mac;
    string name;
    string showName;
    bool powerDownFlag;
    ModelDjData mDjData;
    //传感器数据
    Dictionary<TopologyPartType, ReadSensorDataBase> mReadSensorDataDict;
    bool connected;//机器人是否连接
    //Dictionary<string, ActionSequence> mActions;
    List<Int2> mChangeDJId;
    int mReadBackNum;
    ExtendCMDCode mReadBackExCmd;
    long mReadBackOutTimeIndex;
    List<ActionSequence> mPlayActions;
    ActionSequence mNowPlayActions;
    int mNowPlayIndex;//当前动作的帧下标,用于暂停动作
    //bool bCanPlay;
    ConnectCallBack mConnectCallBack;
    PlayActionsDelegate mPlayActionDlgt = null;
    int mReSendReadMotherNum = 0;
    long mReSendReadMotherIndex = -1;
    float mNowPlayTime;//上一帧播放的时间
    float mNextPlayTime;//下一帧播放的时间
    float mPauseTime;//暂停的时间
    //long mPlayCallBackIndex;//计时器函数返回值
    long mReadPowerIndex = -1;//读取电量计时器函数返回值
    string mErrorRotaDjStr;
    bool mSelfCheckFlag;//自检
    bool mSelfCheckErrorFlag = false;//自检出错
    bool mRetrieveMotherboardFlag = false;//重新获取主板数据
    Dictionary<byte, ushort> mReadBackRotas = null;
    Dictionary<byte, ushort> mPowerDownRotas = null;

    string mMcuId = string.Empty;
    string mDeviceSn = string.Empty;

    #endregion

    #region 公有函数
    public Robot(string id)
    {
        this.id         = id;
        this.mac        = string.Empty;
        this.name       = string.Empty;
        this.mDjData    = new ModelDjData();
        mChangeDJId     = new List<Int2>();
        connected       = false;
        powerDownFlag   = false;
        mReadBackNum    = 0;
        mNowPlayIndex   = 0;
        mSelfCheckFlag  = false;
        mReadBackRotas  = new Dictionary<byte, ushort>();
        //bCanPlay        = true;
        //this.mActions   = new Dictionary<string,ActionSequence>();
        mPowerDownRotas = new Dictionary<byte, ushort>();
        mReadBackOutTimeIndex = -1;
    }
    public Robot(string mac, string name)
    {
        this.mac        = mac;
        this.name       = name;
        mDjData         = new ModelDjData();
        mChangeDJId     = new List<Int2>();
        //mActions        = new Dictionary<string,ActionSequence>();
        this.id         = CreateID.CreateRobotID();
        connected       = false;
        powerDownFlag   = false;
        mReadBackNum    = 0;
        mNowPlayIndex   = 0;
        mSelfCheckFlag  = false;
        mReadBackRotas  = new Dictionary<byte, ushort>();
        //bCanPlay        = true;
        mPowerDownRotas = new Dictionary<byte, ushort>();
    }

    /*public void InitActionSequence(Dictionary<string, ActionSequence> acts)
    {
        if (null != acts)
        {
            mActions = acts;
        }
    }*/
    /// <summary>
    /// 设置机器人的mac地址和名字
    /// </summary>
    /// <param name="mac"></param>
    /// <param name="name"></param>
    public void SetRobotMacAndName(string mac, string name)
    {
        this.mac    = mac;
        this.name   = name;
    }
    
    /// <summary>
    /// 连接成功
    /// </summary>
    /// <param name="mac"></param>
    /// <param name="name"></param>
    public void ConnectRobotResult(string mac,/* string name,*/ bool result)
    {
        this.connected = result;
        if (result)
        {
            this.mac = mac;
            //this.name = name;
            PowerDownFlag = false;
            RegisterNetCallBack();
        }
        if (null != mConnectCallBack)
        {
            mConnectCallBack(result);
        }
    }
    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        this.connected = false;
        canShowPowerFlag = false;
        PowerDownFlag = false;
        //mConnectCallBack = null;
        mNowPlayIndex = 0;
        mDjData.Reset();
        mSelfCheckFlag = false;
        mSelfCheckErrorFlag = false;
        mRetrieveMotherboardFlag = false;
        isPowerPromptFlag = false;
        if (-1 != mReadPowerIndex)
        {
            Timer.Cancel(mReadPowerIndex);
            mReadPowerIndex = -1;
        }
        if (-1 != mReadBackOutTimeIndex)
        {
            Timer.Cancel(mReadBackOutTimeIndex);
            mReadBackOutTimeIndex = -1;
        }
        if (null != mReadSensorDataDict)
        {
            mReadSensorDataDict.Clear();
            mReadSensorDataDict = null;
        }
        //bCanPlay = true;
    }

    public void CannelConnect()
    {
        Disconnect();
        if (-1 != mReSendReadMotherIndex)
        {
            Timer.Cancel(mReSendReadMotherIndex);
            mReSendReadMotherIndex = -1;
        }
    }

    public void ClearConnectCallBack()
    {
        mConnectCallBack = null;
    }
    /// <summary>
    /// 获取机器人所有舵机数据
    /// </summary>
    /// <returns></returns>
    public ModelDjData GetAllDjData()
    {
        return mDjData;
    }
    /// <summary>
    /// 获得当前各舵机的角度
    /// </summary>
    /// <param name="action"></param>
    public void GetNowAction(Action action)
    {
        Dictionary<byte, DuoJiData> dict = mDjData.GetAllData();
        if (null != dict)
        {
            foreach (KeyValuePair<byte, DuoJiData> kvp in dict)
            {
                if (kvp.Value.isTurn)
                {
                    action.UpdateTurn(kvp.Key, kvp.Value.turnData);
                }
                else if (kvp.Value.modelType == ServoModel.Servo_Model_Angle)
                {
                    action.UpdateRota(kvp.Key, kvp.Value.rota);
                }
                
            }
        }
    }

    public void SetNowAction(Action action)
    {
        CtrlAction(action, false);
    }

    public List<byte> GetReadBackShowList()
    {
        List<byte> list = new List<byte>();
        Dictionary<byte, DuoJiData> dict = mDjData.GetAllData();
        if (null != dict)
        {
            foreach (KeyValuePair<byte, DuoJiData> kvp in dict)
            {
                if (kvp.Value.modelType != ServoModel.Servo_Model_Angle)
                {
                    continue;
                }
                if (!mPowerDownRotas.ContainsKey(kvp.Key) || mPowerDownRotas[kvp.Key] != kvp.Value.rota)
                {
                    list.Add(kvp.Key);
                }
            }
        }
        return list;
    }

    public int GetDjNum()
    {
        return mDjData.Count;
    }
    /// <summary>
    /// 获取单个舵机的数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DuoJiData GetAnDjData(int id)
    {
        return mDjData.GetDjData((byte)id);
    }
    
    /// <summary>
    /// 检测是否可以设置id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    /// <returns></returns>
    public ErrorCode CheckSwitchDuoJiId(int id, int targetId)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            byte djId = (byte)id;
            byte targetDjId = (byte)targetId;
            if (!mDjData.IsExist(djId)/* || !mDjData.IsExist(targetDjId)*/)
            {
                ret = ErrorCode.Result_DJ_ID_Error;
                break;
            }
        } while (false);
        return ret;
    }
    /// <summary>
    /// 设置舵机id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    /// <returns></returns>
    public ErrorCode SwitchDuoJiId(int id, int targetId)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            byte djId = (byte)id;
            byte targetDjId = (byte)targetId;
            if (mDjData.IsExist(djId))
            {
                Int2 num;
                num.num1 = djId;
                num.num2 = targetDjId;
                mChangeDJId.Add(num);
                DuoJiData djData = mDjData.GetDjData(djId);
                DuoJiData targetData = mDjData.GetDjData(targetDjId);
                djData.id = targetDjId;
                mDjData.UpdateData(djData);
                if (null == targetData)
                {//目标id不存在
                    mDjData.RemoveData(djId);
                }
                else
                {//目标id已存在
                    targetData.id = djId;
                    mDjData.UpdateData(targetData);
                }
                //更改动作
                Dictionary<string, ActionSequence> dict = ActionsManager.GetInst().GetRobotActions(this.id);
                if (null != dict)
                {
                    foreach (KeyValuePair<string, ActionSequence> pair in dict)
                    {
                        pair.Value.SwitchDuoJiId(djId, targetDjId);
                    }
                }

                //修改模型舵机id
                RobotMgr.Instance.ReviseDJID(name, id, targetId);
            }

        } while (false);
        return ret;
    }

    public void SetModelDuoJiID(List<byte> ids)
    {
        if (ids.Count == mDjData.Count)
        {
            List<byte> mids = mDjData.GetIDList();
            byte idsMax = ids[ids.Count - 1];
            byte midsMax = mids[mids.Count - 1];
            int lenght = (idsMax > midsMax ? idsMax : midsMax) + 1;
            byte[] dvcIds = new byte[lenght];
            byte[] modIds = new byte[lenght];
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                dvcIds[ids[i]] = ids[i];
            }
            for (int i = 0, imax = mids.Count; i < imax; ++i)
            {
                modIds[mids[i]] = mids[i];
            }
            List<byte> dvcDifIds = new List<byte>();
            List<byte> modDifIds = new List<byte>();
            for (int i = 0; i < lenght; ++i)
            {
                if (dvcIds[i] != modIds[i] && (dvcIds[i] != 0 || modIds[i] != 0))
                {
                    if (dvcIds[i] != 0)
                    {
                        dvcDifIds.Add(dvcIds[i]);
                    }
                    if (modIds[i] != 0)
                    {
                        modDifIds.Add(modIds[i]);
                    }
                }
            }
            if (dvcDifIds.Count == modDifIds.Count && modDifIds.Count != 0)
            {
                for (int i = 0, imax = dvcDifIds.Count; i < imax; ++i)
                {
                    SwitchDuoJiId(modDifIds[i], dvcDifIds[i]);
                }
                RobotDataMgr.Instance.ReviseDJid(this.name);
            }
        }
    }
    /// <summary>
    /// 设置起始舵机id
    /// </summary>
    /// <param name="id"></param>
    public void SetStartDuoJiID(int id)
    {
        Dictionary<byte, DuoJiData> djData = mDjData.GetAllData();
        byte oldMinId = mDjData.MinId;
        byte oldMaxId = mDjData.MaxId;
        int idOffset = id - oldMinId;
        if (idOffset > 0)
        {
            for (byte i = oldMaxId; i >= oldMinId; --i)
            {
                if (djData.ContainsKey(i))
                {
                    djData[i].id = (byte)(djData[i].id + idOffset);
                    //mDjData.UpdateData(djData[i]);
                    EventMgr.Inst.Fire(EventID.Set_Change_DuoJi_Data, new EventArg((int)i, (int)djData[i].id));
                }
            }
            for (byte i = oldMinId, icount = (byte)(oldMinId + idOffset); i < icount; ++i)
            {
                mDjData.RemoveData(i);
            }
        }
        else if (idOffset < 0)
        {
            for (byte i = oldMinId; i <= oldMaxId; ++i)
            {
                if (djData.ContainsKey(i))
                {
                    djData[i].id = (byte)(djData[i].id + idOffset);
                    //mDjData.UpdateData(djData[i]);
                    EventMgr.Inst.Fire(EventID.Set_Change_DuoJi_Data, new EventArg((int)i, (int)djData[i].id));
                }
            }
            for (byte i = oldMaxId, icount = (byte)(oldMaxId + idOffset); i > icount; --i)
            {
                mDjData.RemoveData(i);
            }
        }
        
    }

    public bool SaveAllActions()
    {
        if (mChangeDJId.Count > 0)
        {
            Dictionary<string, ActionSequence> dict = ActionsManager.GetInst().GetRobotActions(this.id);
            if (null != dict)
            {
                foreach (KeyValuePair<string, ActionSequence> pair in dict)
                {
                    pair.Value.Save();
                }
            }
            mChangeDJId.Clear();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取该机器人所有动作
    /// </summary>
    /// <returns></returns>
    public List<string> GetActionsIdList()
    {
        return ActionsManager.GetInst().GetActionsIDList(this.id);
    }
    /// <summary>
    /// 获取机器人的动作的名字列表
    /// </summary>
    /// <returns></returns>
    public List<string> GetActionsNameList()
    {
        return ActionsManager.GetInst().GetActionsNameList(this.id);
        /*List<string> list = new List<string>();
        Dictionary<string, ActionSequence> dict = ActionsManager.GetInst().GetRobotActions(this.id);
        if (null != dict)
        {
            foreach (KeyValuePair<string, ActionSequence> pair in dict)
            {
                list.Add(pair.Value.Name);
            }
        }
        return list;*/
    }
    /// <summary>
    /// 通过id获取一套动作
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionSequence GetActionsForID(string id)
    {
        return ActionsManager.GetInst().GetActionForID(this.id, id);
    }
    /// <summary>
    /// 通过动作名字获取动作
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ActionSequence GetActionsForName(string name)
    {
        return ActionsManager.GetInst().GetActionForName(this.id, name);
    }
    /// <summary>
    /// 是否是官方动作
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsOfficialForId(string id)
    {
        ActionSequence act = GetActionsForID(id);
        if (null != act)
        {
            return ActionsManager.GetInst().IsOfficial(act.Id) || act.IsOfficial();
        }
        return false;
    }
    /// <summary>
    /// 是否是官方动作
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsOfficialForName(string name)
    {
        ActionSequence act = GetActionsForName(name);
        if (null != act)
        {
            return ActionsManager.GetInst().IsOfficial(act.Id) || act.IsOfficial();
        }
        return false;
    }
    /// <summary>
    /// 是否有默认动作
    /// </summary>
    /// <returns></returns>
    public bool HaveDefualtActions()
    {
        return ActionsManager.GetInst().HaveDefaultAction(this.id);
    }
    /// <summary>
    /// 创建默认动作
    /// </summary>
    public void CreateDefualtActions(Action action)
    {
        ActionsManager.GetInst().CreateDefaultAction(this.id, action);
    }

    /// <summary>
    /// 获取动作的图标通过id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetActionsIconForID(string id)
    {
        ActionSequence act = GetActionsForID(id);
        string icon = PublicFunction.Default_Actions_Icon_Name;
        if (null != act)
        {
            icon = ActionsManager.GetInst().GetActionIconName(act.IconID);
        }
        return icon;
    }

    /// 获取动作的名字通过图标
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
    public string GetActionNameForIcon(string iconName)
    {
        if (iconName != "")
        {
            foreach (var tem in GetActionsNameList())
            {
                if (iconName == GetActionsIconForName(tem))
                {
                    return tem;
                }
                //   return tem.
            }
        }
        return "";
    }
    /// <summary>
    /// 获取动作的图标通过名字
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetActionsIconForName(string name)
    {
        ActionSequence act = GetActionsForName(name);
        string icon = PublicFunction.Default_Actions_Icon_Name;
        if (null != act)
        {
            icon = ActionsManager.GetInst().GetActionIconName(act.IconID);
        }
        return icon;
    }
    /// 获取某个动作时长
    /// </summary>
    /// <param name="name"></param>
    /// <returns>单位毫秒</returns>
    public int GetActionsTimeForName(string name)
    {
        return ActionsManager.GetInst().GetActionsTimeForName(this.id, name);
    }
    /// <summary>
    /// 通过id播放动作
    /// </summary>
    /// <param name="id"></param>
    public ErrorCode PlayActionsForID(string id)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        ActionSequence acts = GetActionsForID(id);
        if (null != acts)
        {
            PlayActions(acts);
        }
        else
        {
            ret = ErrorCode.Result_Action_Not_Exist;
        }
        return ret;
    }
    /// <summary>
    /// 通过动作名字播放动作
    /// </summary>
    /// <param name="name"></param>
    public ErrorCode PlayActionsForName(string name)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        ActionSequence act = GetActionsForName(name);
        //bCanPlay = true;
        if (null != act)
        {
            PlayActions(act);
        }
        else
        {
            ret = ErrorCode.Result_Action_Not_Exist;
        }
        return ret;
    }
    /// <summary>
    /// 播放某个动作
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="startIndex">从第几帧开始播放，从0开始</param>
    public void PlayActions(ActionSequence actions, PlayActionsDelegate dlgt = null, int startIndex = 0)
    {
        //Timer.Cancel(mPlayCallBackIndex);
        SingletonObject<MyTime>.GetInst().StopTime();
        mPlayActionDlgt = dlgt;
        if (actions.Count > startIndex)
        {
            mNowPlayActions = actions;
            mNowPlayIndex = startIndex;
            if (0 == startIndex)
            {
                CtrlAction(actions[startIndex]);
            }
            else
            {
                CtrlAction(actions[startIndex], actions[startIndex - 1]);
            }
            /*if (null != mPlayActionDlgt)
            {
                mPlayActionDlgt(0, false);
            }*/
            int startTime = 0;
            for (int i = startIndex, imax = actions.Count; i < imax; ++i)
            {
                startTime += actions[i].AllTime;
                MyTime.GetInst().Add(startTime / 1000.0f, PlayActionsCallBack, i);
            }

            /*mNowPlayTime = PublicFunction.GetUnixMs();
            mNextPlayTime = mNowPlayTime + actions[0].AllTime;
            mPlayCallBackIndex = Timer.Add(actions[0].AllTime / 1000.0f, 0, 1, PlayActionsCallBack, 0);*/
        }
        else if (null != mPlayActions && mPlayActions.Count > 0)
        {
            mPlayActions.RemoveAt(0);
            if (mPlayActions.Count > 0)
            {
                PlayActions(mPlayActions[0]);
            }
        }
    }
    /// <summary>
    /// 暂停当前动作
    /// </summary>
    /// <param name="name"></param>
    public void PauseActionsForName(string name)
    {
        ActionSequence act = GetActionsForName(name);
        //Timer.Cancel(mPlayCallBackIndex);
        //mPauseTime = PublicFunction.GetUnixMs();
        MyTime.GetInst().PauseTime();
        if (null != act && act == mNowPlayActions && mNowPlayActions.Count > 0)
        {
            //bCanPlay = false;
            int index = mNowPlayIndex;
            if (index < 0)
            {
                index = 0;
            }
            else if (index >= mNowPlayActions.Count)
            {
                index = mNowPlayActions.Count - 1;
            }
            PauseTurnAction(mNowPlayActions[index]);
        }
    }
    /// <summary>
    /// 继续播放
    /// </summary>
    /// <param name="name"></param>
    public void ContinueActionsForName(string name)
    {
        ActionSequence act = GetActionsForName(name);
        if (null != act && act == mNowPlayActions)
        {
            //bCanPlay = true;
            int index = mNowPlayIndex + 1;
            if (index < mNowPlayActions.Count)
            {
                /*mNextPlayTime = mNextPlayTime - mPauseTime + PublicFunction.GetUnixMs();
                mPlayCallBackIndex = Timer.Add((int)(mNextPlayTime - PublicFunction.GetUnixMs()) / 1000.0f, 0, 1, PlayActionsCallBack, mNowPlayIndex);*/
                MyTime.GetInst().ContinueTime();
            }
            else if (act.IsTurnModel())
            {
                int lastIndex = mNowPlayActions.Count - 1;
                if (lastIndex >= 0)
                {
                    TurnAction(mNowPlayActions[lastIndex]);
                }
            }
            else
            {
                if (null != mPlayActions && mPlayActions.Count > 0)
                {
                    mPlayActions.RemoveAt(0);
                    if (mPlayActions.Count > 0)
                    {
                        mNowPlayIndex = 0;
                        PlayActions(mPlayActions[0]);
                    }
                    else
                    {
                        mNowPlayActions = null;
                    }
                }
                else
                {
                    mNowPlayActions = null;
                }
            }
        }
    }
    /// <summary>
    /// 删除动作根据动作名称
    /// </summary>
    /// <param name="name"></param>
    public void DeleteActionsForName(string name)
    {
        ActionSequence actions = GetActionsForName(name);
        if (null != actions)
        {
            DeleteActionsForID(actions.Id);
        }
    }
    /// <summary>
    /// 删除动作根据动作id
    /// </summary>
    /// <param name="actid"></param>
    public void DeleteActionsForID(string actid)
    {
        ActionsManager.GetInst().DeleteRobotActions(id, actid);
    }


    /// <summary>
    /// 判断动作是不是轮模式
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsTrunModelForName(string name)
    {
        ActionSequence act = GetActionsForName(name);
        if (null != act)
        {
            return act.IsTurnModel();
        }
        return false;
    }
    /// <summary>
    /// 通过id控制播放多个动作
    /// </summary>
    /// <param name="id"></param>
    public void PlayMoreActionsForID(List<string> id)
    {
        if (null != id && id.Count > 0)
        {
            if (null == mPlayActions)
            {
                mPlayActions = new List<ActionSequence>();
            }
            else
            {
                mPlayActions.Clear();
            }
            for (int i = 0, icount = id.Count; i < icount; ++i)
            {
                ActionSequence acts = GetActionsForID(id[i]);
                if (null != acts)
                {
                    mPlayActions.Add(acts);
                }
            }
            if (mPlayActions.Count > 0)
            {
                PlayActions(mPlayActions[0]);
            }
        }
    }
    /// <summary>
    /// 设置舵机初始角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    public void SetStartRota(int id, int rota)
    {
        DuoJiData data = mDjData.GetDjData((byte)id);
        if (null != data)
        {
            data.startRota = (short)rota;
        }
    }
    /// <summary>
    /// 机器人复位
    /// </summary>
    public void RestRobot(bool sendFlag = true)
    {
        ActionSequence acts = ActionsManager.GetInst().GetDefaultAction(this.id);
        if (null != acts)
        {
            PlayActions(acts);
        }
        /*Action acts = new Action();
        acts.sportTime = 1000;
        acts.waitTime = 0;
        mDjData.Reset();
        if (sendFlag)
        {
            List<byte> list = mDjData.GetIDList();
            for (int i = 0, icount = list.Count; i < icount; ++i)
            {
                DuoJiData tmp = mDjData.GetDjData(list[i]);
                if (null != tmp)
                {
                    acts.UpdateRota(tmp.id, tmp.startRota);
                }
            }
            if (null != acts)
            {
                CtrlAction(acts);
            }
        }*/
    }
    /// <summary>
    /// 通过名字控制播放多个动作
    /// </summary>
    /// <param name="name"></param>
    public void PlayMoreActionsForName(List<string> name)
    {
        if (null != id && name.Count > 0)
        {
            if (null == mPlayActions)
            {
                mPlayActions = new List<ActionSequence>();
            }
            else
            {
                mPlayActions.Clear();
            }
            for (int i = 0, icount = name.Count; i < icount; ++i)
            {
                ActionSequence acts = GetActionsForName(name[i]);
                if (null != acts)
                {
                    mPlayActions.Add(acts);
                }
            }
            if (mPlayActions.Count > 0)
            {
                PlayActions(mPlayActions[0]);
            }
        }
    }
    
    /// <summary>
    /// 回读整个机器人的状态
    /// </summary>
    public void ReadBack(ExtendCMDCode exCmd)
    {
        NetWaitMsg.ShowWait(5);
        mReadBackRotas.Clear();
        mReadBackNum = mDjData.Count;
        mErrorRotaDjStr = string.Empty;
        ReadBack(0, false, exCmd);
        if ((exCmd ==  ExtendCMDCode.LogicGetPosture || exCmd == ExtendCMDCode.ReadBack) && PlatformMgr.Instance.GetBluetoothState() && !PlatformMgr.Instance.IsChargeProtected)
        {
            if (-1 != mReadBackOutTimeIndex)
            {
                Timer.Cancel(mReadBackOutTimeIndex);
            }
            mReadBackOutTimeIndex = Timer.Add(5, 1, 1, ReadBackOutTime);
            //ClientMain.GetInst().WaitTimeInvoke(8, ReadBackOutTime);
        }
        //ServoPowerOn(mDjData.GetIDList());
        /*foreach (KeyValuePair<byte, DuoJiData> kvp in data)
        {
            if (!kvp.Value.isTurn)
            {
                mReadBackNum++;
                ReadBack(kvp.Key, true);
            }
            
        }*/
    }
    /// <summary>
    /// 回读连接时的角度
    /// </summary>
    public void ReadConnectedAngle()
    {
        ReadBack(ExtendCMDCode.ReadConnectedAngle);
    }
    /// <summary>
    /// 掉电
    /// </summary>
    public void RobotPowerDown()
    {
        mPowerDownRotas.Clear();
        ReadBack(0, true, ExtendCMDCode.RobotPowerDown);
        mReadBackNum = mDjData.Count;
        if (null != mDjData)
        {
            mDjData.ResetAllLastTurn();
        }
    }
    /// <summary>
    /// 单个舵机掉电
    /// </summary>
    /// <param name="id"></param>
    public void ServoPowerDown(byte id)
    {
        mPowerDownRotas.Clear();
        DuoJiData data = mDjData.GetDjData((byte)id);
        if (null != data)
        {
            data.isPowerDown = true;
            ReadBackMsg msg = new ReadBackMsg();
            msg.servoList.Add((byte)id);
            msg.powerDown = 0;
            mReadBackExCmd = ExtendCMDCode.ServoPowerDown;
            NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, ExtendCMDCode.ServoPowerDown);
            mReadBackNum = 1;
            if (data.isTurn)
            {
                data.CloseTurnModel();
            }
        }
    }
    /// <summary>
    /// 多个舵机掉电
    /// </summary>
    /// <param name="servoList"></param>
    public void ServoPowerDown(List<byte> servoList)
    {
        mPowerDownRotas.Clear();
        ReadBackMsg msg = new ReadBackMsg();
        mReadBackNum = servoList.Count;
        mReadBackExCmd = ExtendCMDCode.ServoPowerDown;
        msg.powerDown = 0;
        if (servoList.Count == mDjData.Count)
        {//所有的掉电
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                DuoJiData data = mDjData.GetDjData(servoList[i]);
                if (null != data)
                {
                    data.isPowerDown = true;
                    data.CloseTurnModel();
                }
            }
            msg.servoList.Add(0);
        }
        else
        {
            int lastId = -1;
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                DuoJiData data = mDjData.GetDjData(servoList[i]);
                if (null != data)
                {
                    if (-1 != lastId && servoList[i] - lastId > 1)
                    {
                        NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, ExtendCMDCode.ServoPowerDown);
                        msg = new ReadBackMsg();
                        msg.powerDown = 0;
                    }
                    lastId = servoList[i];
                    data.isPowerDown = true;
                    msg.servoList.Add(servoList[i]);
                    data.CloseTurnModel();
                }
            }
        }
        NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, ExtendCMDCode.ServoPowerDown);
    }
    
    /// <summary>
    /// 单个舵机上电
    /// </summary>
    /// <param name="id"></param>
    public void ServoPowerOn(byte id)
    {
        mReadBackRotas.Clear();
        mReadBackNum = 1;
        mErrorRotaDjStr = string.Empty;
        DuoJiData data = mDjData.GetDjData(id);
        if (null != data)
        {
            data.isPowerDown = false;
            ReadBackMsg msg = new ReadBackMsg();
            msg.servoList.Add(id);
            msg.powerDown = 1;
            mReadBackExCmd = ExtendCMDCode.ServoPowerOn;
            NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, ExtendCMDCode.ServoPowerOn);
        }
        
    }
    /// <summary>
    /// 多个舵机上电
    /// </summary>
    /// <param name="servoList"></param>
    public void ServoPowerOn(List<byte> servoList)
    {
        mReadBackRotas.Clear();
        mReadBackNum = servoList.Count;
        mErrorRotaDjStr = string.Empty;
        ReadBackMsg msg = new ReadBackMsg();
        mReadBackExCmd = ExtendCMDCode.ServoPowerOn;
        msg.powerDown = 1;
        if (servoList.Count == mDjData.Count)
        {//所有的上电
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                DuoJiData data = mDjData.GetDjData(servoList[i]);
                if (null != data)
                {
                    data.isPowerDown = false;
                }
            }
            msg.servoList.Add(0);
        }
        else
        {
            int lastId = -1;
            for (int i = 0, imax = servoList.Count; i < imax; ++i)
            {
                DuoJiData data = mDjData.GetDjData(servoList[i]);
                if (null != data)
                {
                    if (-1 != lastId && servoList[i] - lastId > 1)
                    {
                        NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, ExtendCMDCode.ServoPowerOn);
                        msg = new ReadBackMsg();
                        msg.powerDown = 1;
                    }
                    lastId = servoList[i];
                    data.isPowerDown = false;
                    msg.servoList.Add(servoList[i]);
                }
            }
        }
        NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, ExtendCMDCode.ServoPowerOn);
    }

    /// <summary>
    /// 回读某个舵机的状态
    /// </summary>
    /// <param name="id">舵机的id</param>
    /// <param name="powerDown">是否掉电回读true表示掉电</param>
    public ErrorCode ReadBack(int id, bool powerDown, ExtendCMDCode exCmd)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            if (0 != id && null == mDjData.GetDjData((byte)id))
            {//错误的舵机id
                ret = ErrorCode.Result_DJ_ID_Error;
                break;
            }
            ReadBackMsg msg = new ReadBackMsg();
            msg.servoList.Add((byte)id);
            //msg.servoList = mDjData.GetIDList();
            if (powerDown)
            {
                msg.powerDown = 0;
            }
            else
            {
                msg.powerDown = 1;
            }
            mReadBackExCmd = exCmd;
            NetWork.GetInst().SendMsg(CMDCode.Read_Back, msg, mac, exCmd);
        } while (false);
        return ret;
    }
    /// <summary>
    /// 控制指定舵机的角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    /// <returns></returns>
    public ErrorCode CtrlActionForDjId(int id, int rota)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            DuoJiData data = mDjData.GetDjData((byte)id);
            if (null == data)
            {//舵机不存在
                ret = ErrorCode.Result_DJ_ID_Error;
                break;
            }
            if (data.isTurn)
            {
                data.CloseTurnModel();
            }
            data.isPowerDown = false;
            CtrlActionMsg msg = new CtrlActionMsg();
            byte rota1 = (byte)rota;
            msg.ids.Add((byte)id);
            msg.AddRota(rota1);
            
            msg.sportTime = (ushort)200;
            msg.endTime = (ushort)200;

            data.lastRota = rota1;
            PowerDownFlag = false;
            NetWork.GetInst().SendMsg(CMDCode.Ctrl_Action, msg, mac, ExtendCMDCode.CtrlActionForDjId);

        } while (false);
        return ret;   
    }
    /// <summary>
    /// 控制指定舵机运动
    /// </summary>
    /// <param name="servoDict"></param>
    /// <param name="time"></param>
    public void CtrlServoMove(Dictionary<byte, byte> servoDict, int time)
    {
        CtrlActionMsg msg = new CtrlActionMsg();
        foreach (KeyValuePair<byte, byte> kvp in servoDict)
        {
            DuoJiData data = mDjData.GetDjData(kvp.Key);
            if (null != data)
            {
                if (data.isTurn)
                {
                    data.CloseTurnModel();
                }
                data.isPowerDown = false;
                msg.ids.Add(kvp.Key);
                msg.rotas.Add(kvp.Value);
                data.lastRota = kvp.Value;
            }
        }
        msg.sportTime = (ushort)time;
        msg.endTime = msg.sportTime;
        NetWork.GetInst().SendMsg(CMDCode.Ctrl_Action, msg, mac, ExtendCMDCode.CtrlServoMove);
    }
    /// <summary>
    /// 控制单个舵机运动
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    /// <param name="time"></param>
    public void CtrlServoMove(byte id, byte rota, int time)
    {
        CtrlActionMsg msg = new CtrlActionMsg();
        DuoJiData data = mDjData.GetDjData(id);
        if (null != data)
        {
            if (data.isTurn)
            {
                data.CloseTurnModel();
            }
            data.isPowerDown = false;
            msg.ids.Add(id);
            msg.rotas.Add(rota);
            data.lastRota = rota;
        }
        msg.sportTime = (ushort)time;
        msg.endTime = msg.sportTime;
        NetWork.GetInst().SendMsg(CMDCode.Ctrl_Action, msg, mac, ExtendCMDCode.CtrlServoMove);
    }

    /// <summary>
    /// 控制多个舵机开启轮模式
    /// </summary>
    /// <param name="servoDict"></param>
    public void CtrlServoTurn(Dictionary<byte, TurnData> servoDict)
    {
        List<DjTurnMsg> turns = null;
        DjTurnMsg turnMsg = null;
        foreach (KeyValuePair<byte, TurnData> kvp in servoDict)
        {
            DuoJiData data = mDjData.GetDjData(kvp.Key);
            if (null != data)
            {
                data.OpenTurnModel(kvp.Value.turnDirection, kvp.Value.turnSpeed);
                if (null == turns)
                {
                    turns = new List<DjTurnMsg>();
                }
                bool addFlag = false;
                for (int i = 0, icount = turns.Count; i < icount; ++i)
                {
                    if (turns[i].turnDirection == (byte)kvp.Value.turnDirection && turns[i].turnSpeed == kvp.Value.turnSpeed)
                    {
                        turns[i].ids.Add(kvp.Key);
                        addFlag = true;
                        break;
                    }
                }
                if (!addFlag)
                {
                    turnMsg = new DjTurnMsg();
                    turnMsg.turnDirection = (byte)kvp.Value.turnDirection;
                    turnMsg.turnSpeed = kvp.Value.turnSpeed;
                    turnMsg.ids.Add(kvp.Key);
                    turns.Add(turnMsg);
                    //记录上次数据
                    data.lastTurnData.turnDirection = kvp.Value.turnDirection;
                    data.lastTurnData.turnSpeed = kvp.Value.turnSpeed;

                }
            }
        }
        if (null != turns)
        {
            for (int i = 0, icount = turns.Count; i < icount; ++i)
            {
                NetWork.GetInst().SendMsg(CMDCode.DuoJi_Turn, turns[i], mac, ExtendCMDCode.CtrlServoTurn);
            }
        }
    }
    /// <summary>
    /// 控制单个舵机开启轮模式
    /// </summary>
    /// <param name="id"></param>
    /// <param name="turnData"></param>
    public void CtrlServoTurn(byte id, TurnData turnData)
    {
        DuoJiData servoData = mDjData.GetDjData(id);
        if (null != servoData)
        {
            servoData.OpenTurnModel(turnData.turnDirection, turnData.turnSpeed);
            DjTurnMsg turnMsg = new DjTurnMsg();
            turnMsg.turnDirection = (byte)turnData.turnDirection;
            turnMsg.turnSpeed = turnData.turnSpeed;
            turnMsg.ids.Add(id);
            //记录上次数据
            servoData.lastTurnData.turnDirection = turnData.turnDirection;
            servoData.lastTurnData.turnSpeed = turnData.turnSpeed;

            NetWork.GetInst().SendMsg(CMDCode.DuoJi_Turn, turnMsg, mac, ExtendCMDCode.CtrlServoTurn);
        }
    }

    /// <summary>
    /// 发送动作里面需要转动的舵机
    /// </summary>
    /// <param name="action"></param>
    public void TurnAction(Action action, bool sendFlag = true)
    {
        List<DjTurnMsg> turns = null;
        DjTurnMsg turnMsg = null;
        foreach (KeyValuePair<byte, TurnData> kvp in action.turnDict)
        {
            DuoJiData data = mDjData.GetDjData(kvp.Key);
            if (null != data)
            {
                data.OpenTurnModel(kvp.Value.turnDirection, kvp.Value.turnSpeed);
                data.isPowerDown = false;
                if (sendFlag && data.NeedSendTurnMsg())
                {
                    if (null == turns)
                    {
                        turns = new List<DjTurnMsg>();
                    }
                    bool addFlag = false;
                    for (int i = 0, icount = turns.Count; i < icount; ++i)
                    {
                        if (turns[i].turnDirection == (byte)kvp.Value.turnDirection && turns[i].turnSpeed == kvp.Value.turnSpeed)
                        {
                            turns[i].ids.Add(kvp.Key);
                            addFlag = true;
                            break;
                        }
                    }
                    if (!addFlag)
                    {
                        turnMsg = new DjTurnMsg();
                        turnMsg.turnDirection = (byte)kvp.Value.turnDirection;
                        turnMsg.turnSpeed = kvp.Value.turnSpeed;
                        turnMsg.ids.Add(kvp.Key);
                        turns.Add(turnMsg);
                        //记录上次数据
                        data.lastTurnData.turnDirection = kvp.Value.turnDirection;
                        data.lastTurnData.turnSpeed = kvp.Value.turnSpeed;

                    }
                }
            }
        }
        if (null != turns && sendFlag)
        {
            for (int i = 0, icount = turns.Count; i < icount; ++i)
            {
                NetWork.GetInst().SendMsg(CMDCode.DuoJi_Turn, turns[i], mac, ExtendCMDCode.TurnAction);
            }
        }
    }

    public void PauseTurnAction(Action action)
    {
        if (action.turnDict.Count <= 0)
        {
            return;
        }
        DjTurnMsg turnMsg = new DjTurnMsg();
        foreach (KeyValuePair<byte, TurnData> kvp in action.turnDict)
        {
            DuoJiData data = mDjData.GetDjData(kvp.Key);
            if (null != data)
            {
                data.CloseTurnModel();
                turnMsg.ids.Add(kvp.Key);
                turnMsg.turnDirection = (byte)TurnDirection.turnStop;
            }
        }
        if (null != turnMsg)
        {
            NetWork.GetInst().SendMsg(CMDCode.DuoJi_Turn, turnMsg, mac, ExtendCMDCode.TurnAction);
        }
    }

    /// <summary>
    /// 停止当前动作
    /// </summary>
    public void StopNowPlayActions()
    {
        mNowPlayIndex = 0;
        MyTime.GetInst().StopTime();
        if (null != mNowPlayActions)
        {
            EventMgr.Inst.Fire(EventID.Stop_Robot_Actions, new EventArg(mNowPlayActions));
            mNowPlayActions = null;
        }
        //Timer.Cancel(mPlayCallBackIndex);
        if (null != mPlayActions && mPlayActions.Count > 0)
        {
            mPlayActions.Clear();
        }
    }
    /// <summary>
    /// 停止某个id的轮转模式
    /// </summary>
    /// <param name="id"></param>
    public void StopTurnForID(int id)
    {
        DuoJiData data = mDjData.GetDjData((byte)id);
        if (null != data)
        {
            data.CloseTurnModel();
            DjTurnMsg turnMsg = new DjTurnMsg();
            turnMsg.ids.Add((byte)(id));
            turnMsg.turnDirection = (byte)TurnDirection.turnStop;
            NetWork.GetInst().SendMsg(CMDCode.DuoJi_Turn, turnMsg, mac, ExtendCMDCode.TurnAction);
        }
    }
    /// <summary>
    /// 停止设备当前有转轮模式的舵机
    /// </summary>
    public void StopAllTurn()
    {
        Dictionary<byte, DuoJiData> datas = mDjData.GetAllData();
        DjTurnMsg turnMsg = null;
        foreach (KeyValuePair<byte, DuoJiData> kvp in datas)
        {
            if (kvp.Value.modelType == ServoModel.Servo_Model_Turn && kvp.Value.isTurn)
            {
                if (null == turnMsg)
                {
                    turnMsg = new DjTurnMsg();
                }
                kvp.Value.CloseTurnModel();
                turnMsg.ids.Add(kvp.Key);
            }
        }
        if (null != turnMsg)
        {
            turnMsg.turnDirection = (byte)TurnDirection.turnStop;
            NetWork.GetInst().SendMsg(CMDCode.DuoJi_Turn, turnMsg, mac, ExtendCMDCode.TurnAction);
        }
    }
    /// <summary>
    /// 发送动作
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ErrorCode CtrlAction(Action action, bool sendFlag = true, bool trunFlag = true)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            TurnAction(action, sendFlag & trunFlag);
            List<byte> list = new List<byte>();
            foreach (KeyValuePair<byte, short> kvp in action.rotas)
            {
                if (action.turnDict.ContainsKey(kvp.Key))
                {
                    continue;
                }
                DuoJiData data = mDjData.GetDjData(kvp.Key);
                if (null != data && data.modelType == ServoModel.Servo_Model_Angle)
                {
                    list.Add(kvp.Key);
                    mDjData.UpdateData(kvp.Key, kvp.Value);
                    data.lastRota = (byte)kvp.Value;
                    data.isPowerDown = false;
                }
            }
            list.Sort();
            CtrlActionMsg msg = new CtrlActionMsg();
            msg.ids = list;
            msg.sportTime = (ushort)action.sportTime;
            msg.endTime = (ushort)(action.sportTime + action.waitTime);
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                short rota = action.rotas[list[i]];
                if (rota < 0)
                {
                    rota = (short)(-rota);
                }
                msg.rotas.Add((byte)rota);
            }
            if (sendFlag)
            {
                NetWork.GetInst().SendMsg(CMDCode.Ctrl_Action, msg, mac, ExtendCMDCode.CtrlAction);
                PowerDownFlag = false;
            }
        } while (false);
        EventMgr.Inst.Fire(EventID.Ctrl_Robot_Action, new EventArg(action));
        return ret;
    }

    public void CtrlAction(Action action, Action lastAction)
    {
        if (null == lastAction)
        {
            CtrlAction(action);
        }
        else
        {
            List<byte> ids = new List<byte>();
            foreach (KeyValuePair<byte, short> kvp in action.rotas)
            {
                if (lastAction.GetRota(kvp.Key) != kvp.Value)
                {
                    ids.Add(kvp.Key);
                }
            }
            
            ids.Sort();
            CtrlActionMsg msg = new CtrlActionMsg();
            msg.ids = ids;
            msg.sportTime = (ushort)action.sportTime;
            msg.endTime = (ushort)(action.sportTime + action.waitTime);
            for (int i = 0, imax = ids.Count; i < imax; ++i)
            {
                DuoJiData data = mDjData.GetDjData(ids[i]);
                if (null != data && data.modelType == ServoModel.Servo_Model_Angle)
                {
                    mDjData.UpdateData(ids[i], action.GetRota(ids[i]));
                    data.lastRota = (byte)action.GetRota(ids[i]);
                    data.isPowerDown = false;
                }
                msg.rotas.Add((byte)action.GetRota(ids[i]));
            }
            NetWork.GetInst().SendMsg(CMDCode.Ctrl_Action, msg, mac, ExtendCMDCode.CtrlAction);
            PowerDownFlag = false;
            EventMgr.Inst.Fire(EventID.Ctrl_Robot_Action, new EventArg(action));
        }

    }
    /// <summary>
    /// 是否开启自检
    /// </summary>
    /// <param name="openFlag"></param>
    public void SelfCheck(bool openFlag)
    {
        SelfCheckMsg msg = new SelfCheckMsg();
        msg.openFlag = openFlag;
        mSelfCheckFlag = openFlag;
        NetWork.GetInst().SendMsg(CMDCode.Self_Check, msg, mac);
    }

    /// <summary>
    /// 读取主板信息
    /// </summary>
    public void ReadMotherboardData()
    {
        //MyLog.Log("ReadMotherboardData start");
        ReadMotherboardDataMsg msg = new ReadMotherboardDataMsg();
        NetWork.GetInst().SendMsg(CMDCode.Read_Motherboard_Data, msg, mac);
    }

    public void RetrieveMotherboardData()
    {
        mRetrieveMotherboardFlag = true;
        NetWaitMsg.ShowWait();
        SelfCheck(false);
        HandShake();
        Timer.Add(3, 0, 1, ReadMotherboardData);
    }
    /// <summary>
    /// 修改设备舵机Id
    /// </summary>
    /// <param name="oldIds">必须在外面排序，由小到大</param>
    /// <param name="newIds">必须在外面排序，由小到大</param>
    public void ChangeDeviceId(byte startId, List<byte> newIds)
    {
        ChangeDeviceIdMsg msg = new ChangeDeviceIdMsg();
        msg.startId = startId;
        for (int i = 0, icount = newIds.Count; i < icount; ++i)
        {
            msg.ids.Add(newIds[i]);
        }
        NetWork.GetInst().SendMsg(CMDCode.Change_ID, msg, mac);
    }
    /// <summary>
    /// 修改传感器ID
    /// </summary>
    /// <param name="partType"></param>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    public void ChangeSensorID(TopologyPartType partType, byte id, byte targetId)
    {
        ChangeSensorIDMsg msg = new ChangeSensorIDMsg();
        msg.sensorType = (byte)partType;
        msg.id = id;
        msg.targetId = targetId;
        NetWork.GetInst().SendMsg(CMDCode.Change_Sensor_ID, msg, mac);
    }
    /// <summary>
    /// 读取设备是否是积木
    /// </summary>
    public void ReadDeviceType()
    {
        CommonMsg msg = new CommonMsg();
        NetWork.GetInst().SendMsg(CMDCode.Read_Device_Type, msg, mac);
    }
    /// <summary>
    /// 发送握手命令
    /// </summary>
    public void HandShake()
    {
        HandShakeMsg msg = new HandShakeMsg();
        NetWork.GetInst().SendMsg(CMDCode.Hand_Shake, msg, mac);
    }

    public byte[] mFlashWriteActions;
    int mWriteFrameNum;
    int mWriteTotalNum;
    /// <summary>
    /// 写入动作
    /// </summary>
    /// <param name="id"></param>
    public void FlashStarActions(ActionSequence actions)
    {
        if (null != actions && actions.Count > 0)
        {
            FlashStartMsg msg = new FlashStartMsg();
            msg.name = actions.Name;
            
            List<byte> allActions = new List<byte>();
            for (int i = 0, imax = actions.Count; i < imax; ++i)
            {
                MemoryStream DataStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(DataStream);
                CtrlActionMsg tmp = new CtrlActionMsg();
                List<byte> list = new List<byte>();
                Action action = actions[i];
                foreach (KeyValuePair<byte, short> kvp in action.rotas)
                {
                    list.Add(kvp.Key);
                }
                list.Sort();
                tmp.ids = list;
                tmp.sportTime = (ushort)action.sportTime;
                tmp.endTime = (ushort)(action.sportTime + action.waitTime);
                for (int j = 0, jmax = list.Count; j < jmax; ++j)
                {
                    short rota = action.rotas[list[j]];
                    if (rota < 0)
                    {
                        rota = (short)(-rota);
                    }
                    tmp.rotas.Add((byte)rota);
                }
                tmp.Write(writer);
                ProtocolPacket packet = new ProtocolPacket();

                packet.setmCmd((byte)CMDCode.Ctrl_Action);
                byte[] msgBytes = DataStream.ToArray();
                packet.setmParam(msgBytes);
                packet.setmParamLen(msgBytes.Length);
                byte[] rawDatas = packet.packetData();
                allActions.AddRange(rawDatas);
                writer.Close();
                DataStream.Close();
            }
            mFlashWriteActions = allActions.ToArray();
            
            msg.frameCount = (ushort)(mFlashWriteActions.Length / 100);
            if (mFlashWriteActions.Length % 100 != 0)
            {
                msg.frameCount += 1;
            }
            mWriteTotalNum = msg.frameCount;
            mWriteFrameNum = 0;
            NetWork.GetInst().SendMsg(CMDCode.Flash_Start, msg, mac);
        }
    }
    
    /// <summary>
    /// 停止写入
    /// </summary>
    public void FlashStopActions()
    {
        mFlashWriteActions = null;
        mWriteFrameNum = 0;
        FlashStopMsg msg = new FlashStopMsg();
        NetWork.GetInst().SendMsg(CMDCode.Flash_Stop, msg, mac);
    }

    public void ReadAllFlash()
    {
        ReadAllFlashMsg msg = new ReadAllFlashMsg();
        NetWork.GetInst().SendMsg(CMDCode.Read_All_Flash, msg, mac);
    }

    public void PlayFlash(string name)
    {
        PlayFlashMsg msg = new PlayFlashMsg();
        msg.name = name;
        NetWork.GetInst().SendMsg(CMDCode.Play_Flash, msg, mac);
    }
    string tmpRename = string.Empty;
    /// <summary>
    /// 修改主板名字
    /// </summary>
    /// <param name="name"></param>
    public void MotherboardRename(string name)
    {
        ChangeNameMsg msg = new ChangeNameMsg();
        tmpRename = name;
        msg.name = name;
        NetWork.GetInst().SendMsg(CMDCode.Change_Name, msg, mac);
    }
    /// <summary>
    /// 获取主板mcu Id
    /// </summary>
    public void ReadMCUInfo()
    {
        ReadMcuIdMsg msg = new ReadMcuIdMsg();
        NetWork.GetInst().SendMsg(CMDCode.Read_MCU_ID, msg, mac);
    }
    /// <summary>
    /// 读取sn信息
    /// </summary>
    public void ReadSnInfo()
    {
        ReadIcFlashMsg msg = new ReadIcFlashMsg();
        msg.argType = (byte)0x07;
        NetWork.GetInst().SendMsg(CMDCode.Read_IC_Flash, msg, mac);
    }

    public void WriteSn(string sn)
    {
        if (string.IsNullOrEmpty(mDeviceSn))
        {
            WriteIcFlash((byte)0x07, sn);
            mDeviceSn = sn;
        }
    }

    public void FlushFlashInfo()
    {
        CommonMsg msg = new CommonMsg();
        NetWork.GetInst().SendMsg(CMDCode.Flush_IC_Flash, msg, mac);
    }

    public void ActivationRobotSuccess()
    {
        if (!string.IsNullOrEmpty(mMcuId))
        {
            PlayerPrefs.SetInt(mMcuId, 1);
            PlayerPrefs.Save();
        }
    }


    public byte[] mRobotUpdateData;
    ushort mRobotUpdateFrameNum;
    ushort mRobotUpdateTotalNum;
    bool updataSuccessFlag = false;
    bool restartFlag = false;
    //public bool isSystemUpdateFlag = false;
    //public bool isServoUpdateFlag = false;


    public bool RobotBlueUpdate()
    {
        RobotUpdateStartMsg msg = new RobotUpdateStartMsg();
        string path = PlatformMgr.Instance.Robot_System_FilePath;
        
        updataSuccessFlag = false;
        restartFlag = false;
        //创建一个Byte用来存放读取到的文件内容
        byte[] bytes = null;
        if (File.Exists(path))
        {
            msg.fileName = Path.GetFileNameWithoutExtension(path);
            FileStream fs = new FileStream(PlatformMgr.Instance.Robot_System_FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            bytes = new byte[fs.Length];
            try
            {
                fs.Read(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                MyLog.Log("读取文件失败了 = " + PlatformMgr.Instance.Robot_System_FilePath);
                bytes = null;

            }
            fs.Dispose();
            fs.Close();
        }
        else
        {
            msg.fileName = path;
            TextAsset text = Resources.Load<TextAsset>(path);
            if (null != text)
            {
                bytes = text.bytes;
            }
        }
        
        if (null == bytes) return false;
        mRobotUpdateData = bytes;
        msg.frameCount = (ushort)(mRobotUpdateData.Length / 100);
        if (mRobotUpdateData.Length % 100 != 0)
        {
            msg.frameCount += 1;
        }
        mRobotUpdateTotalNum = msg.frameCount;
        mRobotUpdateFrameNum = 0;
        Debuger.Log(string.Format("mRobotUpdateFrameNum = {0} mRobotUpdateTotalNum = {1}", mRobotUpdateFrameNum, mRobotUpdateTotalNum));
        NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Start, msg, mac);
        //isSystemUpdateFlag = true;
        /*WaitPromptMsg.ShowPrompt(LauguageTool.GetIns().GetText("开始升级主板程序"), RobotUpdateOnClick);
        WaitPromptMsg.SetRightBtnText(LauguageTool.GetIns().GetText("取消"));*/
        return true;
    }

    /// <summary>
    /// 停止写入
    /// </summary>
    public void RobotBlueUpdateStop()
    {
        mRobotUpdateData = null;
        mRobotUpdateFrameNum = 0;
        //isSystemUpdateFlag = false;
        RobotUpdateStopMsg msg = new RobotUpdateStopMsg();
        NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Stop, msg, mac);
    }

    /// <summary>
    /// 读取电量
    /// </summary>
    public void ReadSystemPower()
    {
        if (!PlatformMgr.Instance.IsWaitUpdateFlag)
        {
            CommonMsg msg = new CommonMsg();
            NetWork.GetInst().SendMsg(CMDCode.Read_System_Power, msg, mac);
        }
        
    }
    /// <summary>
    /// 开启重复读系统电量
    /// </summary>
    public void StartReadSystemPower()
    {
        ReadSystemPower();
        if (-1 != mReadPowerIndex)
        {
            Timer.Cancel(mReadPowerIndex);
        }
        mReadPowerIndex = Timer.Add(30, 30, 0, ReadSystemPower);
    }


    /// <summary>
    /// 升级某个舵机，0表示升级所有的
    /// </summary>
    /// <param name="id"></param>
    public bool ServoUpdate(byte id)
    {
        ServoUpdateStartMsg msg = new ServoUpdateStartMsg();
        msg.id = id;
        string path = PlatformMgr.Instance.Robot_Servo_FilePath;
        //创建一个Byte用来存放读取到的文件内容
        byte[] bytes = null;
        //byte[] bytes = File.ReadAllBytes(path);
        if (File.Exists(path))
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            bytes = new byte[fs.Length];
            try
            {
                fs.Read(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                MyLog.Log("读取文件失败 =" + path);
                bytes = null;
            }
            fs.Dispose();
            fs.Close();
        }
        else
        {
            TextAsset text = Resources.Load<TextAsset>(path);
            if (null != text)
            {
                bytes = text.bytes;
            }
        }
        
        if (null == bytes)
        {
            return false;
        }
        int mod = bytes.Length % 64;
        if (mod != 0)
        {
            mRobotUpdateData = new byte[bytes.Length + 64 - mod];
        }
        else
        {
            mRobotUpdateData = new byte[bytes.Length];
        }
        Array.Copy(bytes, mRobotUpdateData, bytes.Length);
        //mRobotUpdateData = File.ReadAllBytes(path);
        msg.frameCount = (ushort)(mRobotUpdateData.Length / 100);
        if (mRobotUpdateData.Length % 100 != 0)
        {
            msg.frameCount += 1;
        }
        msg.crc32 = PublicFunction.GetCRC32Str(mRobotUpdateData);
        mRobotUpdateTotalNum = msg.frameCount;
        mRobotUpdateFrameNum = 0;
        Debuger.Log(string.Format("mRobotUpdateFrameNum = {0} mRobotUpdateTotalNum = {1}", mRobotUpdateFrameNum, mRobotUpdateTotalNum));
        NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Start, msg, mac);
        //isServoUpdateFlag = true;
        /*WaitPromptMsg.ShowPrompt(LauguageTool.GetIns().GetText("开始升级舵机程序"), CannelServoUpdateOnClick);
        WaitPromptMsg.SetRightBtnText(LauguageTool.GetIns().GetText("取消"));*/
        return true;
    }
    /// <summary>
    /// 取消舵机升级
    /// </summary>
    public void StopServoUpdate()
    {
        mRobotUpdateData = null;
        mRobotUpdateFrameNum = 0;
        //isServoUpdateFlag = false;
        CommonMsg msg = new CommonMsg();
        NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Stop, msg, mac);
    }

    public void StopAllUpdate()
    {
        /*if (isSystemUpdateFlag)
        {
            isSystemUpdateFlag = false;
            
        }*/
        RobotBlueUpdateStop();
        /*if (isServoUpdateFlag)
        {
            isServoUpdateFlag = false;
            
        }*/
        StopServoUpdate();
    }

    public static ErrorCode CheckSystemUpdate(string version)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            /*if (StepManager.GetIns().OpenOrCloseGuide)
            {//指引的时候不升级
                ret = ErrorCode.Do_Not_Upgrade;
                break;
            }*/
            if (!string.IsNullOrEmpty(PlatformMgr.Instance.Robot_System_Version) && !PlatformMgr.Instance.Robot_System_Version.Equals(version))
            {//检测到有升级文件，且版本不一致
                if (!PlatformMgr.Instance.Robot_System_FilePath.EndsWith(".bin") || File.Exists(PlatformMgr.Instance.Robot_System_FilePath))
                {
                    /*if (PlatformMgr.Instance.PowerData.isAdapter)
                    {//正在充电
                        if (PlatformMgr.Instance.IsChargeProtected)
                        {
                            ret = ErrorCode.Robot_Adapter_Open_Protect;
                        }
                        else
                        {
                            ret = ErrorCode.Robot_Adapter_Close_Protect;
                        }
                        break;
                    }
                    else
                    {
                        
                    }*/
                    if (PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                    {//电量太低，不更新
                        ret = ErrorCode.Robot_Power_Low;
                        break;
                    }
                }
                else
                {
                    MyLog.Log(string.Format("升级文件不存在，Robot_System_Version = {0} Robot_System_FilePath = {1}", PlatformMgr.Instance.Robot_System_Version, PlatformMgr.Instance.Robot_System_FilePath));
                    ret = ErrorCode.Do_Not_Upgrade;
                    break;
                }
            }
            else
            {
                ret = ErrorCode.Do_Not_Upgrade;
                break;
            }
        } while (false);

        return ret;
    }
    public static ErrorCode CheckServoUpdate(ReadMotherboardDataMsgAck msg)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            /*if (StepManager.GetIns().OpenOrCloseGuide)
            {//指引的时候不升级
                ret = ErrorCode.Do_Not_Upgrade;
                Debuger.Log("指引不升级");
                break;
            }*/
            if (!string.IsNullOrEmpty(PlatformMgr.Instance.Robot_Servo_Version) && (!PlatformMgr.Instance.Robot_Servo_Version.Equals(msg.djVersion) || msg.errorVerIds.Count > 0))
            {
                if (!PlatformMgr.Instance.Robot_Servo_FilePath.EndsWith(".bin") || File.Exists(PlatformMgr.Instance.Robot_Servo_FilePath))
                {
                    if (PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                    {//电量太低，不更新
                        ret = ErrorCode.Robot_Power_Low;
                        break;
                    }
                    /*if (PlatformMgr.Instance.PowerData.isAdapter)
                    {//正在充电
                        if (PlatformMgr.Instance.IsChargeProtected)
                        {
                            ret = ErrorCode.Robot_Adapter_Open_Protect;
                        }
                        else
                        {
                            ret = ErrorCode.Robot_Adapter_Close_Protect;
                        }
                        break;
                    }
                    else
                    {
                        if (PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                        {//电量太低，不更新
                            ret = ErrorCode.Robot_Power_Low;
                            break;
                        }
                    }*/
                }
                else
                {
                    MyLog.Log(string.Format("升级文件不存在，Servo_Version = {0} Servo_FilePath = {1}", PlatformMgr.Instance.Robot_Servo_Version, PlatformMgr.Instance.Robot_Servo_FilePath));
                    ret = ErrorCode.Do_Not_Upgrade;
                    break;
                }
            }
            else
            {
                ret = ErrorCode.Do_Not_Upgrade;
                break;
            }
        } while (false);

        return ret;
    }


    //////////////////////////////////////////////////////////////////////////
    //传感器
    /// <summary>
    /// 设置某种传感器的传输状态
    /// </summary>
    /// <param name="sensorType"></param>
    /// <param name="openFlag"></param>
    public void SetSensorIOState(TopologyPartType sensorType, bool openFlag)
    {
        SetSensorIOStateMsg msg = new SetSensorIOStateMsg();
        msg.sensorType = (byte)sensorType;
        msg.openFlag = openFlag;
        msg.ids.Add(0);
        NetWork.GetInst().SendMsg(CMDCode.Set_Sensor_IO_State, msg, mac);

        //LogicCtrl.GetInst().OpenLogicForRobot(this);
        //LogicCtrl.GetInst().CallUnityCmd("jimu://queryInfrared|1");
    }
    /// <summary>
    /// 读取传感器数据
    /// </summary>
    /// <param name="ids"></param>
    public void ReadSensorData(List<byte> ids, TopologyPartType partType, bool readAllFlag)
    {
        if (null != mReadSensorDataDict && mReadSensorDataDict.ContainsKey(partType))
        {
            if (partType == TopologyPartType.Gyro)
            {
                ReadSensorDataMsg msg = new ReadSensorDataMsg();
                msg.sensorData.sensorType = (byte)partType;
                msg.sensorData.ids = ids;
                msg.arg = 1;
                ExtendCMDCode exCmd;
                if (readAllFlag)
                {
                    exCmd = ExtendCMDCode.ReadAllSensorData;
                    mReadSensorDataDict[partType].ReadAllDataMsg(ids);
                }
                else
                {
                    exCmd = ExtendCMDCode.ReadInfraredData + (partType - TopologyPartType.Infrared);
                    mReadSensorDataDict[partType].ReadDataMsg(ids);
                }
                NetWork.GetInst().SendMsg(CMDCode.Read_Sensor_Data, msg, mac, exCmd);
            }
            else if (partType == TopologyPartType.Speaker)
            {
                ReadSensorDataMsg msg = new ReadSensorDataMsg();
                msg.sensorData.sensorType = (byte)partType;
                msg.sensorData.ids = ids;
                mReadSensorDataDict[partType].ReadDataMsg(ids);
                ExtendCMDCode exCmd;
                exCmd = ExtendCMDCode.ReadInfraredData + (partType - TopologyPartType.Infrared);
                NetWork.GetInst().SendMsg(CMDCode.Read_Sensor_Data, msg, mac, exCmd);
            }
            else
            {
                ReadSensorDataOtherMsg msg = new ReadSensorDataOtherMsg();
                SensorBaseData tmp = new SensorBaseData();
                tmp.ids = ids;
                mReadSensorDataDict[partType].ReadDataMsg(ids);
                tmp.sensorType = (byte)partType;
                msg.sensorList.Add(tmp);
                ExtendCMDCode exCmd = ExtendCMDCode.ReadInfraredData + (partType - TopologyPartType.Infrared);
                NetWork.GetInst().SendMsg(CMDCode.Read_Sensor_Data_Other, msg, mac, exCmd);
            }
        }
        else
        {
            Debuger.Log(partType.ToString() + " 传感器不存在");
            switch (partType)
            {
                case TopologyPartType.Infrared:
                    SingletonObject<LogicCtrl>.GetInst().QueryInfraredCallBack(CallUnityResult.failure);
                    break;
                case TopologyPartType.Touch:
                    SingletonObject<LogicCtrl>.GetInst().QueryTouchStatusCallBack(CallUnityResult.failure);
                    break;
                case TopologyPartType.Gyro:
                    if (readAllFlag)
                    {
                        SingletonObject<LogicCtrl>.GetInst().QueryAllSensorCallBack(CallUnityResult.failure);
                    }
                    else
                    {
                        SingletonObject<LogicCtrl>.GetInst().QueryGyroscopeCallBack(CallUnityResult.failure);
                    }
                    
                    break;
            }
        }
        
    }

    public void ReadAllSensorData()
    {
        TopologyPartType[] sensorTypes = PublicFunction.Read_All_Sensor_Type;
        ReadSensorDataOtherMsg msg = new ReadSensorDataOtherMsg();
        for (int i = 0, imax = sensorTypes.Length; i < imax; ++i)
        {
            if (null != mReadSensorDataDict && mReadSensorDataDict.ContainsKey(sensorTypes[i]) && null != mReadSensorDataDict[sensorTypes[i]].ids)
            {
                List<byte> ids = mReadSensorDataDict[sensorTypes[i]].ids;
                SensorBaseData tmp = new SensorBaseData();
                tmp.ids = ids;
                mReadSensorDataDict[sensorTypes[i]].ReadAllDataMsg(ids);
                tmp.sensorType = (byte)sensorTypes[i];
                msg.sensorList.Add(tmp);
            }
        }
        if (msg.sensorList.Count > 0)
        {
            NetWork.GetInst().SendMsg(CMDCode.Read_Sensor_Data_Other, msg, mac, ExtendCMDCode.ReadAllSensorData);
        }
        else
        {
            SingletonObject<LogicCtrl>.GetInst().QueryAllSensorCallBack(CallUnityResult.failure);
        }
    }

    public void SensorInit(List<byte> ids, TopologyPartType partType)
    {
        if (null == mReadSensorDataDict)
        {
            mReadSensorDataDict = new Dictionary<TopologyPartType, ReadSensorDataBase>();
        }
        if (!mReadSensorDataDict.ContainsKey(partType))
        {
            switch (partType)
            {
                case TopologyPartType.Infrared:
                    InfraredData data = new InfraredData(ids);
                    mReadSensorDataDict[TopologyPartType.Infrared] = data;
                    break;
                case TopologyPartType.Touch:
                    TouchData touchData = new TouchData(ids);
                    mReadSensorDataDict[TopologyPartType.Touch] = touchData;
                    break;
                case TopologyPartType.Gyro:
                    GyroData gyroData = new GyroData(ids);
                    mReadSensorDataDict[TopologyPartType.Gyro] = gyroData;
                    break;
                case TopologyPartType.Light:
                    LightData lightData = new LightData(ids);
                    mReadSensorDataDict[TopologyPartType.Light] = lightData;
                    break;
                case TopologyPartType.Speaker:
                    SpeakerData speakerData = new SpeakerData(ids);
                    mReadSensorDataDict[TopologyPartType.Speaker] = speakerData;
                    break;
            }
        }
        SetSensorIOState(partType, true);
    }


    public ReadSensorDataBase GetReadSensorData(TopologyPartType partType)
    {
        if (null != mReadSensorDataDict && mReadSensorDataDict.ContainsKey(partType))
        {
            return mReadSensorDataDict[partType];
        }
        Debuger.Log(partType.ToString() + " 不存在");
        return null;
    }
    /// <summary>
    /// 设置表情
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="lightType"></param>
    /// <param name="color"></param>
    public void SendEmoji(List<byte> ids, SendEmojiDataMsg.LightType lightType, string color, byte times, UInt16 duration)
    {
        SendEmojiDataMsg msg = new SendEmojiDataMsg();
        msg.sensorData.ids = ids;
        msg.sensorData.sensorType = (byte)TopologyPartType.Light;
        msg.lightType = lightType;
        msg.duration = duration;
        msg.times = times;
        msg.rgb = color;
        NetWork.GetInst().SendMsg(CMDCode.Send_Sensor_Data, msg, mac, ExtendCMDCode.SendEmojiData);
    }
    /// <summary>
    /// 设置灯光
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="lightShowData"></param>
    /// <param name="duration"></param>
    public void SendLight(List<byte> ids, List<LightShowData> lightShowData, UInt16 duration)
    {
        SendLightDataMsg msg = new SendLightDataMsg();
        msg.sensorData.sensorType = (byte)TopologyPartType.Light;
        msg.sensorData.ids = ids;
        msg.duration = duration;
        msg.showData = lightShowData;
        NetWork.GetInst().SendMsg(CMDCode.Send_Light_Data, msg, mac);
    }

    /// <summary>
    /// 控制数码管
    /// </summary>
    /// <param name="ids">需要控制的id</param>
    /// <param name="controlType">控制类型</param>
    /// <param name="showNum">需显示的数字位数，1表示个位，2表示十分位，3表示百分位，4表示千分位</param>
    /// <param name="showSubPoint">需要显示的点的位数，1表示个位，2表示十分位，3表示百分位，4表示千分位</param>
    /// <param name="showColon">是否显示冒号</param>
    /// <param name="isNegativeNum">是否是负数</param>
    /// <param name="flickerTimes">闪烁的次数</param>
    /// <param name="flickerTimeout">闪烁或数值变化的频率</param>
    /// <param name="startValue">起始值</param>
    /// <param name="endValue">结束值</param>
    public void SendDigitalTube(List<byte> ids, SendDigitalTubeDataMsg.DigitalTubeControlType controlType, List<byte> showNum, List<byte> showSubPoint, bool showColon, bool isNegativeNum, byte flickerTimes, UInt32 flickerTimeout, UInt32 startValue, UInt32 endValue)
    {
        SendDigitalTubeDataMsg msg = new SendDigitalTubeDataMsg();
        msg.sensorData.sensorType = (byte)TopologyPartType.DigitalTube;
        msg.sensorData.ids = ids;
        msg.controlType = controlType;
        msg.showNum = showNum;
        msg.showSubPoint = showSubPoint;
        msg.showColon = showColon;
        msg.isNegativeNum = isNegativeNum;
        msg.flickerTimes = flickerTimes;
        msg.flickerTimeout = flickerTimeout;
        msg.startValue = startValue;
        msg.endValue = endValue;
        NetWork.GetInst().SendMsg(CMDCode.Send_Sensor_Data, msg, mac, ExtendCMDCode.SendDigitalTubeData);
    }
    /// <summary>
    /// 控制传感器
    /// </summary>
    /// <param name="id"></param>
    /// <param name="partType"></param>
    /// <param name="controlType"></param>
    /// <param name="duration"></param>
    /// <param name="times"></param>
    public void CtrlSensorLED(byte id, TopologyPartType partType, CtrlSensorLEDMsg.ControlType controlType, UInt16 duration, byte times)
    {
        CtrlSensorLEDMsg msg = new CtrlSensorLEDMsg();
        msg.sensorData.sensorType = (byte)partType;
        msg.sensorData.ids.Add(id);
        msg.controlType = controlType;
        msg.duration = duration;
        msg.times = times;
        NetWork.GetInst().SendMsg(CMDCode.Ctrl_Sensor_LED, msg, mac);
    }

    #endregion

    #region 私有函数


    private void ReadSystemVersion()
    {
        CommonMsg msg = new CommonMsg();
        NetWork.GetInst().SendMsg(CMDCode.Read_System_Version, msg, mac);
    }

    private void WriteIcFlash(byte type, string arg)
    {
        WriteIcFlashMsg msg = new WriteIcFlashMsg();
        msg.argType = type;
        msg.arg = arg;
        NetWork.GetInst().SendMsg(CMDCode.Write_IC_Flash, msg, mac);
    }

    

    private void FlashWriteFrame(byte[] acts, int frame, bool isEnd)
    {
        FlashWriteMsg msg = new FlashWriteMsg();
        int len = 100;
        if (isEnd)
        {
            len = acts.Length - 100 * frame;
        }
        byte[] bytes = new byte[len];
        Array.Copy(acts, frame * 100, bytes, 0, len);
        msg.frameNum = (ushort)(frame + 1);
        msg.bytes = bytes;
        if (isEnd)
        {
            NetWork.GetInst().SendMsg(CMDCode.Flash_End, msg, mac);
        }
        else
        {
            NetWork.GetInst().SendMsg(CMDCode.Flash_Write, msg, mac);
        }
        
    }

    private void RobotUpdateFrame(byte[] acts, int frame, bool isEnd)
    {
        RobotUpdateWriteMsg msg = new RobotUpdateWriteMsg();
        int len = 100;
        if (isEnd)
        {
            len = acts.Length - 100 * frame;
        }
        byte[] bytes = new byte[len];
        Array.Copy(acts, frame * 100, bytes, 0, len);
        msg.frameNum = (ushort)(frame + 1);
        msg.bytes = bytes;
        if (isEnd)
        {
            Timer.Add(0.02f, 0, 1, SendRobotUpdateFinish, msg);
            //NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Finish, msg, mac);
        }
        else
        {
            Timer.Add(0.02f, 0, 1, SendRobotUpdateWrite, msg);
            //NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Write, msg, mac);
        }

    }

    void SendRobotUpdateFinish(params object[] args)
    {
        RobotUpdateWriteMsg msg = (RobotUpdateWriteMsg)args[0];
        NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Finish, msg, mac);
    }

    void SendRobotUpdateWrite(params object[] args)
    {
        RobotUpdateWriteMsg msg = (RobotUpdateWriteMsg)args[0];
        NetWork.GetInst().SendMsg(CMDCode.Robot_Update_Write, msg, mac);
    }

    private void ServoUpdateFrame(byte[] acts, int frame, bool isEnd)
    {
        ServoUpdateWriteMsg msg = new ServoUpdateWriteMsg();
        int len = 100;
        if (isEnd)
        {
            len = acts.Length - 100 * frame;
        }
        byte[] bytes = new byte[len];
        Array.Copy(acts, frame * 100, bytes, 0, len);
        msg.frameNum = (ushort)(frame + 1);
        msg.bytes = bytes;
        if (isEnd)
        {
            Timer.Add(0.02f, 0, 1, SendServoUpdateFinish, msg);
        }
        else
        {
            Timer.Add(0.02f, 0, 1, SendServoUpdateWrite, msg);
        }
    }

    void SendServoUpdateFinish(params object[] args)
    {
        ServoUpdateWriteMsg msg = (ServoUpdateWriteMsg)args[0];
        NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Finish, msg, mac);
    }

    void SendServoUpdateWrite(params object[] args)
    {
        ServoUpdateWriteMsg msg = (ServoUpdateWriteMsg)args[0];
        NetWork.GetInst().SendMsg(CMDCode.Servo_Update_Write, msg, mac);
    }

    private void CheckReadBack(ExtendCMDCode exCmd)
    {
        if (0 != mReadBackNum)
        {
            return;
        }
        if (-1 != mReadBackOutTimeIndex)
        {
            Timer.Cancel(mReadBackOutTimeIndex);
            mReadBackOutTimeIndex = -1;
        }
        NetWaitMsg.CloseWait();
        switch (exCmd)
        {
            case ExtendCMDCode.ReadBack:
            case ExtendCMDCode.ReadConnectedAngle:
                {
                    if (mReadBackRotas.Count != mDjData.Count)
                    {//回读失败
                        /*string str = string.Empty;
                        List<byte> ids = mDjData.GetIDList();
                        for (int i = 0, imax = ids.Count; i < imax; ++i)
                        {
                            if (!mReadBackRotas.ContainsKey(ids[i]))
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    str += PublicFunction.Separator_Comma;
                                }
                                str += ids[i];
                            }
                        }
                        str = "[ff0000]" + str + "[-]";*/
                        if (PlatformMgr.Instance.GetBluetoothState())
                        {
                            PromptMsg.ShowDoublePrompt(LauguageTool.GetIns().GetText("HuiDuShuLiangBuYiZhi"), ReadFailOnClick);
                        }
                    }
                    else if (!string.IsNullOrEmpty(mErrorRotaDjStr))
                    {// 有舵机处于死区
                        Dictionary<byte, short> errorRotas = new Dictionary<byte, short>();
                        foreach (KeyValuePair<byte, ushort> kvp in mReadBackRotas)
                        {
                            DuoJiData data = mDjData.GetDjData(kvp.Key);
                            if (!PublicFunction.IsNormalRota(kvp.Value) && (null == data || data.modelType == ServoModel.Servo_Model_Angle))
                            {
                                errorRotas[kvp.Key] = (short)kvp.Value;
                            }
                        }
                        PopReadAngleErrorMsg.ShowReadAngleErrorMsg(errorRotas, ReadFailOnClick);
                    }
                    else
                    {
                        if (exCmd == ExtendCMDCode.ReadConnectedAngle)
                        {
                            SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                            EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
                        }
                        Action nowAction = new Action();
                        //正常赋值
                        foreach (KeyValuePair<byte, ushort> kvp in mReadBackRotas)
                        {
                            DuoJiData data = mDjData.GetDjData(kvp.Key);
                            if (null != data)
                            {
                                if (data.isTurn)
                                {
                                    nowAction.UpdateTurn(kvp.Key, data.turnData);
                                }
                                else
                                {
                                    data.rota = (short)kvp.Value;
                                    data.lastRota = data.rota;
                                    mDjData.UpdateData(data);
                                    nowAction.UpdateRota(kvp.Key, (short)kvp.Value);
                                }
                            }
                            else
                            {
                                Debuger.Log("模型上无此id");
                            }
                        }
                        PowerDownFlag = false;
                        //锁住角度
                        CtrlAction(nowAction, true, false);
                        EventMgr.Inst.Fire(EventID.Read_Back_Msg_Ack_Success, new EventArg(nowAction));
                        EventMgr.Inst.Fire(EventID.UI_Post_Robot_Select_ID);
                    }
                    
                }
                break;
            case ExtendCMDCode.LogicGetPosture:
                {
                    Action nowAction = new Action();
                    //正常赋值
                    foreach (KeyValuePair<byte, ushort> kvp in mReadBackRotas)
                    {
                        DuoJiData data = mDjData.GetDjData(kvp.Key);
                        if (null != data)
                        {
                            if (data.isTurn)
                            {
                                nowAction.UpdateTurn(kvp.Key, data.turnData);
                            }
                            else
                            {
                                data.rota = (short)kvp.Value;
                                data.lastRota = data.rota;
                                mDjData.UpdateData(data);
                                nowAction.UpdateRota(kvp.Key, (short)kvp.Value);
                            }
                        }
                        else
                        {
                            Debuger.Log("模型上无此id");
                        }
                    }
                    if (exCmd == ExtendCMDCode.LogicGetPosture)
                    {
                        SingletonObject<LogicCtrl>.GetInst().GetPostureCallBack(CallUnityResult.success, nowAction);
                    }
                    else
                    {
                        SingletonObject<LogicCtrl>.GetInst().ServoPowerOnCallBack(CallUnityResult.success);
                    }

                    PowerDownFlag = false;
                    //锁住角度
                    CtrlAction(nowAction, true, false);
                }
                break;
            case ExtendCMDCode.ServoPowerOn:
                //正常赋值
                CtrlActionMsg msg = new CtrlActionMsg();
                foreach (KeyValuePair<byte, ushort> kvp in mReadBackRotas)
                {
                    msg.ids.Add(kvp.Key);
                    msg.rotas.Add((byte)kvp.Value);
                    DuoJiData data = mDjData.GetDjData(kvp.Key);
                    if (null != data)
                    {
                        data.rota = (short)kvp.Value;
                        data.lastRota = data.rota;
                        mDjData.UpdateData(data);
                        data.isPowerDown = false;
                    }
                    else
                    {
                        Debuger.Log("模型上无此id");
                    }
                }
                msg.sportTime = (ushort)20;
                msg.endTime = (ushort)(20);
                NetWork.GetInst().SendMsg(CMDCode.Ctrl_Action, msg, mac, ExtendCMDCode.CtrlAction);
                SingletonObject<LogicCtrl>.GetInst().ServoPowerOnCallBack(CallUnityResult.success);
                break;
            case ExtendCMDCode.ServoPowerDown:
                SingletonObject<LogicCtrl>.GetInst().ServoPowerOffCallBack(CallUnityResult.success);
                break;
        }
    }
    void ReadFailOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals(PromptMsg.LeftBtnName))
        {//断开蓝牙
            //PlatformMgr.Instance.DisConnenctBuletooth();
            EventMgr.Inst.Fire(EventID.Read_Back_Msg_Ack_Failed);
            if (mReadBackExCmd == ExtendCMDCode.ReadConnectedAngle)
            {
                SingletonObject<LogicCtrl>.GetInst().CloseBlueSearch();
                EventMgr.Inst.Fire(EventID.Blue_Connect_Finished);
            }
            else if (mReadBackExCmd == ExtendCMDCode.LogicGetPosture)
            {
                SingletonObject<LogicCtrl>.GetInst().GetPostureCallBack(CallUnityResult.failure, null);
            }
        }
        else if (btnName.Equals(PromptMsg.RightBtnName))
        {//重新回读
            ReadBack(mReadBackExCmd);
        }
    }

    void SelfExceptionOnClick(bool confirmFlag)
    {
        if (confirmFlag)
        {
            SelfCheck(false);
            HandShake();
            NetWaitMsg.ShowWait();
            //2秒以后读取初始角度
            Timer.Add(3, 0, 1, ReadMotherboardData);
        }
        else
        {
            mSelfCheckErrorFlag = false;
            SelfCheck(true);
        }
    }

    void SelfCheckErrorOnClick(GameObject obj)
    {
        string btnName = obj.name;
        if (btnName.Equals(PromptMsg.LeftBtnName))
        {//断开蓝牙
            mSelfCheckErrorFlag = false;
            SelfCheck(true);
            //PlatformMgr.Instance.DisConnenctBuletooth();
        }
        else if (btnName.Equals(PromptMsg.RightBtnName))
        {//握手命令
            SelfCheck(false);
            HandShake();
            NetWaitMsg.ShowWait();
            //2秒以后读取初始角度
            Timer.Add(3, 0, 1, ReadMotherboardData);
            //ClientMain.GetInst().WaitTimeInvoke(2, ReadMotherboardData);
        }
    }

    
    void PlayActionsCallBack(object[] obj)
    {
        int index = (int)obj[0];
        index++;
        if (null != mNowPlayActions)
        {
            if (index < mNowPlayActions.Count)
            {
                mNowPlayIndex = index;
                CtrlAction(mNowPlayActions[index], mNowPlayActions[index - 1]);

                /*mNowPlayTime = PublicFunction.GetUnixMs();
                mNextPlayTime = mNowPlayTime + mNowPlayActions[index].AllTime;
                mPlayCallBackIndex = Timer.Add(mNowPlayActions[index].AllTime / 1000.0f, 0, 1, PlayActionsCallBack, index);*/
                if (null != mPlayActionDlgt)
                {
                    mPlayActionDlgt(index, false);
                }
            }
            else if (!mNowPlayActions.IsTurnModel())
            {
                if (null != mPlayActions && mPlayActions.Count > 0)
                {
                    mPlayActions.RemoveAt(0);
                    if (mPlayActions.Count > 0)
                    {
                        mNowPlayIndex = 0;
                        PlayActions(mPlayActions[0]);
                    }
                    else
                    {
                        mNowPlayActions = null;
                    }
                }
                else
                {
                    mNowPlayActions = null;
                }
                if (null != mPlayActionDlgt)
                {
                    mPlayActionDlgt(index, true);
                }
                SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.success);
            }
            else
            {
                SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.success);
                if (null != mPlayActionDlgt)
                {
                    mPlayActionDlgt(index, true);
                }
            }
        }
        else
        {
            SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.success);
        }
        
    }

    /// <summary>
    /// 注册网络回调
    /// </summary>
    public void RegisterNetCallBack()
    {
        ProtocolClient.GetInst().Register(mac, CMDCode.Self_Check, SelfCheckCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_Motherboard_Data, ReadMotherboardCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_Back, ReadBackCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Ctrl_Action, CtrlActionCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Change_ID, ChangeDeviceIdCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Change_Sensor_ID, ChangeSensorIdCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Flash_Start, FlashStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Flash_Write, FlashStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Flash_End, FlashEndCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_All_Flash, ReadAllFlashCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Change_Name, MotherboardRenameMsgAck);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_MCU_ID, ReadMcuMsgCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_IC_Flash, ReadIcFlashMsgCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Write_IC_Flash, WriteIcFlashMsgCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Robot_Update_Start, RobotUpdateStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Robot_Update_Write, RobotUpdateStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Robot_Update_Finish, RobotUpdateFinishedCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Robot_Update_Stop, RobotUpdateStopCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_System_Version, ReadSystemVersionCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Robot_Restart_Update_Start_Ack, RobotUpdateRestartStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Robot_Restart_Update_Finish_Ack, RobotUpdateRestartFinishCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_System_Power, ReadPowerCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Servo_Update_Start, ServoUpdateStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Servo_Update_Write, ServoUpdateStartCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Servo_Update_Finish, ServoUpdateFinishedCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Servo_Update_Stop, ServoUpdateStopCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_Device_Type, ReadDeviceCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_Sensor_Data, ReadSensorDataCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Send_Sensor_Data, SendSensorDataCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Send_Light_Data, SendLightDataCallBack);
        ProtocolClient.GetInst().Register(mac, CMDCode.Read_Sensor_Data_Other, ReadSensorDataOtherCallBack);
    }
    /// <summary>
    /// 自检回调，如设备故障会收到此通知
    /// </summary>
    /// <param name="len"></param>
    /// <param name="br"></param>
    private void SelfCheckCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        if (null == br)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
            return;
        }
        else if (len <= 1)
        {
            return;
        }
        byte result = br.ReadByte();
        switch (result)
        {
            case 1://电量过低/过高
                byte result1 = br.ReadByte();
                if (result1 == 0)
                {//电量过低
                    if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                    {
                        isPowerPromptFlag = true;
                        //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("DianLiangGuoDi"));
                        if (LogicCtrl.GetInst().IsLogicProgramming)
                        {
                            LogicCtrl.GetInst().CommonTipsCallBack(LogicLanguage.GetText("DianLiangGuoDi"), PublicFunction.Show_Error_Time_Space);
                        }
                        else
                        {
                            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DianLiangGuoDi"), PublicFunction.Show_Error_Time_Space);
                        }
                    }
                    //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DianLiangGuoDi"), PublicFunction.Show_Error_Time_Space);
                    //PlatformMgr.Instance.DisConnenctBuletooth();
                }
                else if (result1 == 1)
                {//电量过高
                    if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                    {
                        //PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("DianLiangGuoGao"));
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DianLiangGuoGao"), PublicFunction.Show_Error_Time_Space);
                    }
                    //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText(""), PublicFunction.Show_Error_Time_Space);
                    //PlatformMgr.Instance.DisConnenctBuletooth();
                }
                break;
            case 2://舵机有问题
                {
                    mSelfCheckErrorFlag = true;
                    if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                    {
                        SelfCheckDjErrorAck msg = new SelfCheckDjErrorAck();
                        msg.Read(br);
                        string str = string.Empty;
                        if (msg.turnProtect.Count > 0)
                        {
                            str += string.Format(GetPromptText("舵机转动异常"), ("[ff0000]" + PublicFunction.ListToString(msg.turnProtect) + "[-]"));
                        }
                        if (msg.eProtect.Count > 0)
                        {
                            str += string.Format(GetPromptText("舵机电流异常"), ("[ff0000]" + PublicFunction.ListToString(msg.eProtect) + "[-]"));
                        }
                        if (msg.cProtect.Count > 0)
                        {
                            str += string.Format(GetPromptText("舵机温度异常"), ("[ff0000]" + PublicFunction.ListToString(msg.cProtect) + "[-]"));
                        }
                        if (msg.hfProtect.Count > 0)
                        {
                            str += string.Format(GetPromptText("舵机过压异常"), ("[ff0000]" + PublicFunction.ListToString(msg.hfProtect) + "[-]"));
                        }
                        if (msg.lfProtect.Count > 0)
                        {
                            str += string.Format(GetPromptText("舵机欠压异常"), ("[ff0000]" + PublicFunction.ListToString(msg.lfProtect) + "[-]"));
                        }
                        if (msg.otherProtect.Count > 0)
                        {
                            str += string.Format(GetPromptText("舵机其他异常"), ("[ff0000]" + PublicFunction.ListToString(msg.otherProtect) + "[-]"));
                        }
                        if (msg.encryptProtect.Count > 0)
                        {//熔丝位或加密错误保护
                            if (SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
                            {
                                str += string.Format(LogicLanguage.GetText("舵机已损坏，请更换该舵机"), "[ff0000]" + PublicFunction.ListToString(msg.encryptProtect) + "[-]");
                            }
                            else
                            {
                                str += string.Format(LauguageTool.GetIns().GetText("舵机已损坏，请更换该舵机"), "[ff0000]" + PublicFunction.ListToString(msg.encryptProtect) + "[-]");
                            }
                        }
                        if (LogicCtrl.GetInst().IsLogicProgramming)
                        {
                            /*this.StopNowPlayActions();
                            this.StopAllTurn();*/
                            LogicCtrl.GetInst().ExceptionCallBack(str.Replace("[ff0000]", "").Replace("[-]", ""), SelfExceptionOnClick);
                        }
                        else
                        {
                            PromptMsg.ShowDoublePrompt(str, SelfCheckErrorOnClick);
                        }
                        
                        /*SelfCheckMsgDjErrorAck msg = new SelfCheckMsgDjErrorAck(len);
                        msg.Read(br);
                        string str = PublicFunction.ListToString(msg.errorList);
                        str = "[ff0000]" + str + "[-]";
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiYouWenTi"), str), SelfCheckErrorOnClick);*/
                    }
                    else
                    {
                        Debuger.Log("PromptMsg IsExist");
                    }
                    
                    //PlatformMgr.Instance.DisConnenctBuletooth();
                }
                
                break;
            case 3://舵机版本不一致
                {
                    mSelfCheckErrorFlag = true;
                    if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                    {
                        SelfCheckMsgDjErrorAck msg1 = new SelfCheckMsgDjErrorAck(len);
                        msg1.Read(br);
                        string str1 = PublicFunction.ListToString(msg1.errorList);
                        str1 = "[ff0000]" + str1 + "[-]";
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), str1), SelfCheckErrorOnClick);
                    }
                }
                break;
            case 4://红外异常
                {
                    /*mSelfCheckErrorFlag = true;
                    if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                    {
                        if (LogicCtrl.GetInst().IsLogicProgramming)
                        {
                            / *this.StopNowPlayActions();
                            this.StopAllTurn();* /
                            LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("红外传感器连接异常"), string.Empty), SelfExceptionOnClick);
                        }
                        else
                        {
                            PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("红外传感器连接异常"), string.Empty), SelfCheckErrorOnClick);
                        }
                    }*/
                }
                break;
            case 5://陀螺仪模块异常
                /*mSelfCheckErrorFlag = true;
                if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                {
                    if (LogicCtrl.GetInst().IsLogicProgramming)
                    {
                        LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("陀螺仪传感器连接异常"), string.Empty), SelfExceptionOnClick);
                    }
                    else
                    {
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("陀螺仪传感器连接异常"), string.Empty), SelfCheckErrorOnClick);
                    }
                }*/
                break;
            case 6://触碰异常
                /*mSelfCheckErrorFlag = true;
                if (!PopWinManager.GetInst().IsExist(typeof(PromptMsg)))
                {
                    if (LogicCtrl.GetInst().IsLogicProgramming)
                    {
                        LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("触碰传感器连接异常"), string.Empty), SelfExceptionOnClick);
                    }
                    else
                    {
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("触碰传感器连接异常"), string.Empty), SelfCheckErrorOnClick);
                    }
                }*/
                break;
        }
    }

    string GetPromptText(string key)
    {
        if (LogicCtrl.GetInst().IsLogicProgramming)
        {
            return LogicLanguage.GetText(key);
        }
        return LauguageTool.GetIns().GetText(key);
    }
    /// <summary>
    /// 主板信息回调
    /// </summary>
    /// <param name="len"></param>
    /// <param name="br"></param>
    private void ReadMotherboardCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        if (len < 11 || null == br)
        {
            if (null == br)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("HuoQuZhuBanXinXiShiBai"));
                PlatformMgr.Instance.DisConnenctBuletooth();
                mReSendReadMotherNum = 0;
                ConnectingMsg.HideMsg();
                NetWaitMsg.CloseWait();
                SingletonObject<LogicCtrl>.GetInst().ExceptionRepairResult();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                return;
            }
            mReSendReadMotherNum++;
            if (mReSendReadMotherNum < 2)
            {
                if (len == 1)
                {
                    StopAllUpdate();
                }
                ClientMain.GetInst().WaitTimeInvoke(1, PlatformMgr.Instance.DisConnenctBuletooth);
                NetWaitMsg.ShowWait();
                SearchBluetoothMsg.SetConnectingState(false);
                mReSendReadMotherIndex = Timer.Add(5, 1, 1, ReadMotherboardOutTime);
            }
            else
            {
                SingletonObject<LogicCtrl>.GetInst().ExceptionRepairResult();
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("HuoQuZhuBanXinXiShiBai"));
                PlatformMgr.Instance.DisConnenctBuletooth();
                mReSendReadMotherNum = 0;
                NetWaitMsg.CloseWait();
                ConnectingMsg.HideMsg();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
            }
            return;
        }
        //ReadSystemVersion();
        NetWaitMsg.CloseWait();
        mReSendReadMotherNum = 0;
        ReadMotherboardDataMsgAck msg = new ReadMotherboardDataMsgAck();
        msg.Read(br);
        MotherboardData = msg;
        PlatformMgr.Instance.PowerData.power = msg.power;
        if (RobotManager.GetInst().IsSetDeviceIDFlag)
        {
            TopologyPartType[] sensorAry = PublicFunction.Open_Topology_Part_Type;
            int deviceNum = msg.ids.Count;
            string errorSensor = string.Empty;
            for (int i = 0, imax = sensorAry.Length; i < imax; ++i)
            {
                SensorData sensorData = MotherboardData.GetSensorData(sensorAry[i]);
                if (null != sensorData)
                {
                    deviceNum += sensorData.ids.Count;
                    if (sensorData.errorIds.Count > 0)
                    {
                        errorSensor += PublicFunction.ListToString(sensorData.errorIds);
                    }
                }
            }
            if (msg.errorIds.Count > 0)
            {//有异常舵机
                string str = PublicFunction.ListToString(msg.errorIds);
                PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("ChongFuDuoJiID"), str));
                PlatformMgr.Instance.DisConnenctBuletooth();
                ConnectingMsg.HideMsg();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                return;
            }
            else if (!string.IsNullOrEmpty(errorSensor))
            {//重复的传感器
                PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("传感器ID重复"), errorSensor));
                PlatformMgr.Instance.DisConnenctBuletooth();
                ConnectingMsg.HideMsg();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                return;
            }
            else if (deviceNum < 1)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("WuDuoJi"));
                PlatformMgr.Instance.DisConnenctBuletooth();
                ConnectingMsg.HideMsg();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                return;
            }
            else if (msg.ids.Count > 1)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("ZhiNengXiuGaiYiGe"));
                PlatformMgr.Instance.DisConnenctBuletooth();
                ConnectingMsg.HideMsg();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                return;
            }
            else if (deviceNum > 1)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("只能修改一个设备"));
                PlatformMgr.Instance.DisConnenctBuletooth();
                ConnectingMsg.HideMsg();
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.BluetoothConnectionPage_ConnectionFailed);
                return;
            }
            SearchBluetoothMsg.CloseMsg();
            EventMgr.Inst.Fire(EventID.Set_Device_ID_ReadData_Result, new EventArg(msg));
            EventMgr.Inst.Fire(EventID.BLUETOOTH_MATCH_RESULT, new EventArg(true));
            PlatformMgr.Instance.SaveLastConnectedData(mac);
        }
        else if (mSelfCheckErrorFlag)
        {//自检错误
            SingletonObject<LogicCtrl>.GetInst().ExceptionRepairResult();
            if (msg.errorVerIds.Count > 0)
            {//舵机版本不一致
                /*if (!CheckServoVersion(msg))
                {
                    string str1 = PublicFunction.ListToString(msg.errorVerIds);
                    str1 = "[ff0000]" + str1 + "[-]";
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), str1), SelfCheckErrorOnClick);
                }*/
                if (!LogicCtrl.GetInst().IsLogicProgramming)
                {
                    TopologyBaseMsg.ShowMsg(msg);
                }
                //ServosConStateMsg.ShowMsg(msg);
                return;
            }
            else if (msg.errorIds.Count > 0)
            {//有异常舵机
                string str = PublicFunction.ListToString(msg.errorIds);
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("DuoJiYouWenTi"), str), SelfExceptionOnClick);
                }
                else
                {
                    str = "[ff0000]" + str + "[-]";
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiYouWenTi"), str), SelfCheckErrorOnClick);
                }
                return;
            }
            else
            {
                List<byte> list = mDjData.GetIDList();
                List<byte> errorList = null;
                for (int i = 0, icount = list.Count; i < icount; ++i)
                {
                    if (!msg.ids.Contains(list[i]))
                    {
                        if (null == errorList)
                        {
                            errorList = new List<byte>();
                        }
                        errorList.Add(list[i]);
                    }
                }
                if (null != errorList)
                {
                    string str = PublicFunction.ListToString(errorList);
                    
                    if (LogicCtrl.GetInst().IsLogicProgramming)
                    {
                        LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("DuoJiYouWenTi"), str), SelfExceptionOnClick);
                    }
                    else
                    {
                        str = "[ff0000]" + str + "[-]";
                        PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("DuoJiYouWenTi"), str), SelfCheckErrorOnClick);
                    }
                    return;
                }
            }
            mSelfCheckErrorFlag = false;
            SelfCheck(true);
            TopologyPartType[] partType = PublicFunction.Open_Topology_Part_Type;
            for (int i = 0, imax = partType.Length; i < imax; ++i)
            {
                SensorData sensorData = MotherboardData.GetSensorData(partType[i]);
                if (null != sensorData && sensorData.ids.Count > 0)
                {
                    SensorInit(sensorData.ids, partType[i]);
                }
            }
            return;
        }
        else if (mRetrieveMotherboardFlag)
        {
            SensorData sensorData = MotherboardData.GetSensorData(TopologyPartType.Speaker);
            if (null != sensorData && sensorData.ids.Count == 1 && sensorData.errorIds.Count == 0)
            {
                SensorInit(sensorData.ids, TopologyPartType.Speaker);
                ReadSensorData(sensorData.ids, TopologyPartType.Speaker, false);
            }
            mRetrieveMotherboardFlag = false;
            TopologyBaseMsg.ShowMsg(msg);
            //ServosConStateMsg.ShowMsg(msg);
        }
        else
        {
            if (SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming && !SingletonObject<LogicCtrl>.GetInst().IsLogicOpenSearchFlag)
            {//防止重复发包
                return;
            }
            SensorData sensorData = MotherboardData.GetSensorData(TopologyPartType.Speaker);
            if (null != sensorData && sensorData.ids.Count == 1 && sensorData.errorIds.Count == 0)
            {
                SensorInit(sensorData.ids, TopologyPartType.Speaker);
                ReadSensorData(sensorData.ids, TopologyPartType.Speaker, false);
            }
            
            SearchBluetoothMsg.CloseMsg();
            TopologyBaseMsg.ShowMsg(msg);
            //ServosConStateMsg.ShowMsg(msg);
            return;
        }
        /*
        else
        {
            ServosConStateMsg.ShowMsg(msg);
            return;
            if (msg.errorIds.Count > 0)
            {//有异常舵机
                / *string str = PublicFunction.ListToString(msg.errorIds);
                HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiYouWenTi"), str));* /
                //PlatformMgr.Instance.DisConnenctBuletooth();
            }
            else if (msg.errorVerIds.Count > 0)
            {//舵机版本不一致
                bool flag = true;
                do
                {
                    List<byte> list = GetAllDjData().GetIDList();
                    if (list.Count != msg.ids.Count)
                    {
                        flag = false;
                        //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DuoJiShuLiangBuYiZhi"));
                        PlatformMgr.Instance.DisConnenctBuletooth();
                        break;
                    }
                    for (int i = 0, icount = list.Count; i < icount; ++i)
                    {
                        if (list[i] != msg.ids[i])
                        {
                            flag = false;
                            //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DuoJiIDBuPiPei"));
                            PlatformMgr.Instance.DisConnenctBuletooth();
                            break;
                        }
                    }
                } while (false);
                if (flag)
                {
                    if (CheckServoVersion(msg))
                    {
                        //return;
                    }
                    else
                    {
                        //HUDTextTips.ShowTextTip(string.Format(LauguageTool.GetIns().GetText("DuoJiBanBenBuYiZhi"), PublicFunction.ListToString(msg.errorVerIds)));
                        PlatformMgr.Instance.DisConnenctBuletooth();
                    }
                }
            }
            else
            {
                do
                {
                    Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        List<byte> list = robot.GetAllDjData().GetIDList();
                        if (list.Count != msg.ids.Count)
                        {
                            //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DuoJiShuLiangBuYiZhi"));
                            PlatformMgr.Instance.DisConnenctBuletooth();
                            break;
                        }
                        bool flag = true;
                        for (int i = 0, icount = list.Count; i < icount; ++i)
                        {
                            if (list[i] != msg.ids[i])
                            {
                                //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("DuoJiIDBuPiPei"));
                                PlatformMgr.Instance.DisConnenctBuletooth();
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("蓝牙连接成功"), PublicFunction.Normal_Tips_Color);
                        }
                        //回读机器人的角度
                        //ReadBack();
                    }
                } while (false);
            }
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                PlayerPrefs.SetString(PublicFunction.Last_Connected_Bluetooth, mac);
                PlayerPrefs.Save();
                PlatformMgr.Instance.PowerData.power = msg.power;
                if (CheckSystemVersion(msg.mbVersion))
                {
                    motherboardMsg = msg;
                    //return;
                }
                / *ReadMCUInfo();
                SelfCheck(true);* /
            }
            //MotherboardDataMsg.ShowMotherboardMsg(msg);
            ServosConStateMsg.ShowMsg(msg);
        }*/
    }

    private void ReadMotherboardOutTime()
    {
        mReSendReadMotherIndex = -1;
        PlatformMgr.Instance.ConnenctBluetooth(this.mac, string.Empty);
    }

    private void ReadBackOutTime()
    {
        mReadBackOutTimeIndex = -1;
        mReadBackNum = 0;
        CheckReadBack(mReadBackExCmd);
    }
    private void ReadBackCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                NetWaitMsg.CloseWait();
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                EventMgr.Inst.Fire(EventID.Read_Back_Msg_Ack, new EventArg(false));
                if (exCmd == ExtendCMDCode.LogicGetPosture)
                {
                    SingletonObject<LogicCtrl>.GetInst().GetPostureCallBack(CallUnityResult.failure, null);
                }
                else if (exCmd == ExtendCMDCode.ServoPowerOn)
                {
                    SingletonObject<LogicCtrl>.GetInst().ServoPowerOnCallBack(CallUnityResult.failure);
                }
                else if (exCmd == ExtendCMDCode.ServoPowerDown)
                {
                    SingletonObject<LogicCtrl>.GetInst().ServoPowerOffCallBack(CallUnityResult.failure);
                }
                return;
            }
            else if (len <= 1)
            {//失败
                /*if (mReadBackNum > 0)
                {
                    --mReadBackNum;
                    CheckReadBack();
                }*/
                return;
            }
            ReadBackMsgAck msg = new ReadBackMsgAck();
            msg.Read(br);
            if (msg.result == (byte)0xAA)
            {
                switch (mReadBackExCmd)
                {
                    case ExtendCMDCode.ReadBack:
                    case ExtendCMDCode.ReadConnectedAngle:
                    case ExtendCMDCode.ServoPowerOn:
                    case ExtendCMDCode.LogicGetPosture:
                        {
                            mReadBackRotas[msg.id] = msg.rota;
                            DuoJiData data = mDjData.GetDjData(msg.id);
                            if (null != data && data.modelType == ServoModel.Servo_Model_Angle)
                            {
                                data.isPowerDown = false;
                            }
                            if (!PublicFunction.IsNormalRota(msg.rota) && (null == data || data.modelType == ServoModel.Servo_Model_Angle))
                            {
                                if (!string.IsNullOrEmpty(mErrorRotaDjStr))
                                { 
                                    mErrorRotaDjStr += PublicFunction.Separator_Comma;
                                }
                                mErrorRotaDjStr += msg.id;
                            }
                        }
                        break;
                    case ExtendCMDCode.RobotPowerDown:
                    case ExtendCMDCode.ServoPowerDown:
                        {
                            PowerDownFlag = true;
                            mPowerDownRotas[msg.id] = msg.rota;
                            DuoJiData data = mDjData.GetDjData(msg.id);
                            if (null != data)
                            {
                                data.isPowerDown = true;
                            }
                        }
                        break;
                }
            }
            if (mReadBackNum > 0)
            {
                --mReadBackNum;
                CheckReadBack(mReadBackExCmd);
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
    

    private void CtrlActionCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        switch (exCmd)
        {
            case ExtendCMDCode.CtrlAction:
                {
                    if (null == br)
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                        if (null != mNowPlayActions)
                        {
                            StopNowPlayActions();
                            SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.failure);
                        }
                        return;
                    }
                    else if (len <= 0)
                    {
                        //HUDTextTips.ShowTextTip("execute error!", PublicFunction.Show_Error_Time_Space);
                        if (null != mNowPlayActions)
                        {
                            StopNowPlayActions();
                            SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.failure);
                        }
                        return;
                    }
                    CommonMsgAck msg = new CommonMsgAck();
                    msg.Read(br);
                    if (msg.result == (byte)ErrorCode.Result_OK)
                    {//成功

                    }
                    else
                    {
                        //HUDTextTips.ShowTextTip("execute error!", PublicFunction.Show_Error_Time_Space);
                        if (null != mNowPlayActions)
                        {
                            StopNowPlayActions();
                            SingletonObject<LogicCtrl>.GetInst().PlayActionCallBack(CallUnityResult.failure);
                            if (LogicCtrl.GetInst().IsLogicProgramming)
                            {
                                SingletonObject<LogicCtrl>.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("舵机其他异常"), string.Empty), SelfExceptionOnClick);
                            }
                            else
                            {
                                PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("舵机其他异常"), string.Empty), SelfCheckErrorOnClick);
                            }
                        }
                    }
                }
                break;
            case ExtendCMDCode.CtrlServoMove:
                {
                    if (null == br)
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                        SingletonObject<LogicCtrl>.GetInst().ServoSetCallBack(CallUnityResult.failure);
                        return;
                    }
                    else if (len <= 0)
                    {
                        SingletonObject<LogicCtrl>.GetInst().ServoSetCallBack(CallUnityResult.failure);
                        return;
                    }
                    CommonMsgAck msg = new CommonMsgAck();
                    msg.Read(br);
                    if (msg.result == (byte)ErrorCode.Result_OK)
                    {//成功
                        SingletonObject<LogicCtrl>.GetInst().ServoSetCallBack(CallUnityResult.success);
                    }
                    else
                    {
                        SingletonObject<LogicCtrl>.GetInst().ServoSetCallBack(CallUnityResult.failure);
                    }
                }
                break;
            case ExtendCMDCode.CtrlActionForDjId:
                {
                    if (null == br)
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                    }
                    SingletonObject<LogicCtrl>.GetInst().AdjustServoCallBack();
                }
                break;
        }
        
    }

    private void ChangeDeviceIdCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        if (null == br)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
            return;
        }
        else if (len <= 0)
        {
            return;
        }
        CommonMsgAck msg = new CommonMsgAck();
        msg.Read(br);
        if (msg.result == (byte)ErrorCode.Result_OK)
        {//成功
            NetWaitMsg.ShowWait(2);
            HandShake();
            EventMgr.Inst.Fire(EventID.Set_Device_ID_Msg_Ack, new EventArg(true));
        }
        else
        {
            EventMgr.Inst.Fire(EventID.Set_Device_ID_Msg_Ack, new EventArg(false));
        }
        //PlatformMgr.Instance.DisConnenctBuletooth();
    }

    private void ChangeSensorIdCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        if (null == br)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
            return;
        }
        else if (len <= 0)
        {
            return;
        }
        ChangeSensorIDMsgAck msg = new ChangeSensorIDMsgAck();
        msg.Read(br);
        if (msg.result == (byte)ErrorCode.Result_OK)
        {//成功
            NetWaitMsg.ShowWait(2);
            HandShake();
        }
        EventMgr.Inst.Fire(EventID.Change_Sensor_ID_Msg_Ack, new EventArg(msg));
    }



    private void FlashStartCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                mFlashWriteActions = null;
                mWriteFrameNum = 0;
                return;
            }
            else if (len <= 0)
            {
                HUDTextTips.ShowTextTip("写入失败");
                mFlashWriteActions = null;
                mWriteFrameNum = 0;
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
                if (null != mFlashWriteActions && mWriteFrameNum < mWriteTotalNum)
                {
                    if (mWriteFrameNum == mWriteTotalNum - 1)
                    {
                        FlashWriteFrame(mFlashWriteActions, mWriteFrameNum, true);
                    }
                    else
                    {
                        FlashWriteFrame(mFlashWriteActions, mWriteFrameNum, false);
                    }
                    ++mWriteFrameNum;
                }
                HUDTextTips.ShowTextTip(string.Format("写入第{0}帧", mWriteFrameNum));
            }
            else
            {
                mFlashWriteActions = null;
                mWriteFrameNum = 0;
                HUDTextTips.ShowTextTip("写入失败");
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

    

    private void FlashEndCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            mFlashWriteActions = null;
            mWriteFrameNum = 0;
            if (null == br)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                return;
            }
            else if (len <= 0)
            {
                HUDTextTips.ShowTextTip("写入失败");
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
                PromptMsg.ShowSinglePrompt("写入结束");
            }
            else
            {
                HUDTextTips.ShowTextTip("写入失败");
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

    private void FlashStopCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                return;
            }
            else if (len <= 0)
            {
                HUDTextTips.ShowTextTip("取消失败");
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
                HUDTextTips.ShowTextTip("取消成功");
            }
            else
            {
                HUDTextTips.ShowTextTip("取消失败");
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
    int flashNum = 0;
    List<string> readFlashList = new List<string>();
    private void ReadAllFlashCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                return;
            }
            else if (len <= 0)
            {
                HUDTextTips.ShowTextTip("读取失败");
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            if (len == 1)
            {
                CommonMsgAck msg = new CommonMsgAck();
                msg.Read(br);
                if (msg.result == (byte)ErrorCode.Result_OK)
                {//无动作列表
                    HUDTextTips.ShowTextTip("无动作列表");
                }
                else
                {
                    flashNum = msg.result;
                    readFlashList.Clear();
                }
            }
            else
            {
                ReadAllFlashMsgAck msg = new ReadAllFlashMsgAck(len);
                msg.Read(br);
                readFlashList.Add(msg.name);
                if (readFlashList.Count == flashNum)
                {
                    FlashMsg.OpenFlashMsg(readFlashList);
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

    private void MotherboardRenameMsgAck(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                return;
            }
            else if (len <= 0)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改失败"));
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            if (len == 1)
            {
                CommonMsgAck msg = new CommonMsgAck();
                msg.Read(br);
                if (msg.result == (byte)ErrorCode.Result_OK)
                {//无动作列表
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改成功"), HUDTextTips.Color_Green);
                    PlatformMgr.Instance.MotherboardRename(tmpRename);
                }
                else
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("修改失败"));
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

    private void ReadMcuMsgCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br || len != 13)
            {
                Debuger.Log("获取mcu id 失败");
                return;
            }
            CommonMsgAck result = new CommonMsgAck();
            result.Read(br);
            if (result.result == (byte)ErrorCode.Result_OK)
            {//成功
                ReadMcuIdMsgAck msg = new ReadMcuIdMsgAck();
                msg.Read(br);
                mMcuId = msg.id;
                Debuger.Log("mcu id = " + mMcuId);
                if (!string.IsNullOrEmpty(mMcuId))
                {
                    if (!PlayerPrefs.HasKey(mMcuId))
                    {//未激活
                        ReadSnInfo();
                    }
                }
            }
            else
            {
                Debuger.Log("获取mcu id 失败");
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

    private void ReadIcFlashMsgCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br || len <= 1)
            {
                Debuger.Log("获取IcFlash参数失败");
                mDeviceSn = string.Empty;
            }
            else
            {
                byte result = br.ReadByte();
                if (0 == result)
                {
                    ReadIcFlashSnAck msg = new ReadIcFlashSnAck((byte)(len - 1));
                    msg.Read(br);
                    mDeviceSn = msg.deviceSn;
                }
                else
                {
                    mDeviceSn = string.Empty;
                }
                Debuger.Log("sn = " + mDeviceSn);
            }
            PlatformMgr.Instance.ActivationRobot(mMcuId, mDeviceSn);
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


    private void WriteIcFlashMsgCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br || len <= 0)
            {
                Debuger.Log("写入IcFlash参数失败");
            }
            else
            {
                CommonMsgAck msg = new CommonMsgAck();
                msg.Read(br);
                Debuger.Log("写入IcFlash参数 result =" + msg.result);
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
    private void RobotUpdateStartCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("连接超时"));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("连接超时"), RobotUpdateEndShowMotherOnClick);
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                return;
            }
            else if (len <= 0)
            {
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"), RobotUpdateEndShowMotherOnClick);
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
                if (null != mRobotUpdateData && mRobotUpdateFrameNum < mRobotUpdateTotalNum)
                {
                    if (mRobotUpdateFrameNum == mRobotUpdateTotalNum - 1)
                    {
                        RobotUpdateFrame(mRobotUpdateData, mRobotUpdateFrameNum, true);
                    }
                    else
                    {
                        RobotUpdateFrame(mRobotUpdateData, mRobotUpdateFrameNum, false);
                    }
                    ++mRobotUpdateFrameNum;
                }
                float per = mRobotUpdateFrameNum / (mRobotUpdateTotalNum + 0.0f) * 100;
                int perInt = (int)per;
                if (perInt <= 0) perInt = 0;
                EventMgr.Inst.Fire(EventID.Update_Progress, new EventArg(perInt));
                //WaitPromptMsg.UpdateText(string.Format(LauguageTool.GetIns().GetText("正在升级"), perInt.ToString()));
                //HUDTextTips.ShowTextTip(string.Format("写入第{0}帧", mRobotUpdateFrameNum));
            }
            /*else if (msg.result == 1 || msg.result == 2 || msg.result == 3)
            {//升级失败（空间不够或者没有进入升级模式）
                if (mRobotUpdateFrameNum == 0)
                {
                    if (msg.result == 1)
                    {
                        WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
                    }
                    else
                    {

                    }
                }
            }*/
            else
            {
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            }
        }
        catch (System.Exception ex)
        {
            mRobotUpdateData = null;
            mRobotUpdateFrameNum = 0;
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }



    private void RobotUpdateFinishedCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            mRobotUpdateData = null;
            mRobotUpdateFrameNum = 0;
            if (null == br)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("连接超时"));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("连接超时"));
                return;
            }
            else if (len <= 0)
            {
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("等待升级完成"), LauguageTool.GetIns().GetText("确定"), RobotUpdateEndShowMotherOnClick);
                /*WaitPromptMsg.OnSleepRightBtn();
                DealyOnWakeRightBtn();*/
                PlatformMgr.Instance.SetSendXTState(false);
            }
            else
            {
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            }
        }
        catch (System.Exception ex)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    private void RobotUpdateStopCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                return;
            }
            else if (len <= 0)
            {
                //PlatformMgr.Instance.DisConnenctBuletooth();
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
            }
            else
            {
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

    private void ReadSystemVersionCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br || len <= 0)
            {
                return;
            }
            ReadSystemVersionAck msg = new ReadSystemVersionAck((byte)len);
            msg.Read(br);
            Debuger.Log("ReadSystemVersionCallBack = " + msg.version);
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

    private void RobotUpdateRestartStartCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            restartFlag = true;
            if (null == br || len <= 0)
            {
                //WaitPromptMsg.OnWakeRightBtn();
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
                return;
            }
            //CommonMsgAck msg = new CommonMsgAck();
            //msg.Read(br);
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("开始升级"));
        }
        catch (System.Exception ex)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
            //WaitPromptMsg.OnWakeRightBtn();
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    private void RobotUpdateRestartFinishCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            restartFlag = true;
            //WaitPromptMsg.OnWakeRightBtn();
            if (null == br || len <= 0)
            {
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            EventMgr.Inst.Fire(EventID.Update_Finished, new EventArg(true));
            PlatformMgr.Instance.SetSendXTState(true);
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级完成"));
            updataSuccessFlag = true;
            //isSystemUpdateFlag = false;
        }
        catch (System.Exception ex)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(true));
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }
    public bool canShowPowerFlag = false;
    bool isPowerPromptFlag = false;
    private void ReadPowerCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br || len < 4)
            {
                return;
            }
            PlatformMgr.Instance.PowerData.Read(br);
            //PowerMsg.UpdatePower();
            if (canShowPowerFlag && !PlatformMgr.Instance.PowerData.isAdapter && PlatformMgr.Instance.PowerData.percentage <= 20 && !isPowerPromptFlag)
            {
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().CommonTipsCallBack(string.Format(LogicLanguage.GetText("电量低于20%"), 20), PublicFunction.Show_Error_Time_Space);
                }
                else
                {
                    PromptMsg.ShowSinglePrompt(string.Format(LauguageTool.GetIns().GetText("电量低于20%"), 20));
                }
                
                isPowerPromptFlag = true;
            }
            if (PlatformMgr.Instance.NeedUpdateFlag && !PlatformMgr.Instance.PowerData.isAdapter)
            {//需要升级
                if (PlatformMgr.Instance.PowerData.power < PublicFunction.Update_System_Power_Min)
                {//电量过低，应该断开蓝牙
                    ErrorCode ret = CheckServoUpdate(MotherboardData);
                    if (ErrorCode.Do_Not_Upgrade != ret)
                    {//舵机有升级
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("舵机版本不一致且设备电量过低"));
                        PlatformMgr.Instance.DisConnenctBuletooth();
                    }
                    else
                    {
                        ret = CheckSystemUpdate(MotherboardData.mbVersion);
                        if (ErrorCode.Do_Not_Upgrade != ret)
                        {
                            PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("检测到主板程序有更新且设备电量过低"));
                            PlatformMgr.Instance.DisConnenctBuletooth();
                        }
                        else
                        {
                            PlatformMgr.Instance.NeedUpdateFlag = false;
                        }
                    }
                }
                else
                {
                    if (!PopWinManager.GetInst().IsExist(typeof(TopologyBaseMsg)))
                    {
                        PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("充电完成，开始升级!"), PopGotoUpdateViewOnClick);
                    }
                }
                
            }
            if (LogicCtrl.GetInst().IsLogicProgramming && PlatformMgr.Instance.IsChargeProtected)
            {
                LogicCtrl.GetInst().ChargeProtectedCallBack();
            }
            EventMgr.Inst.Fire(EventID.Read_Power_Msg_Ack);
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

    void PopGotoUpdateViewOnClick(GameObject obj)
    {
        if (obj.name.Equals(PromptMsg.RightBtnName))
        {
            TopologyBaseMsg.ShowMsg(MotherboardData);
            //ServosConStateMsg.ShowMsg(MotherboardData);
        }
    }

    private void ServoUpdateStartCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("连接超时"));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("连接超时"));
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                return;
            }
            else if (len <= 0)
            {
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(false));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
                if (null != mRobotUpdateData && mRobotUpdateFrameNum < mRobotUpdateTotalNum)
                {
                    if (mRobotUpdateFrameNum == mRobotUpdateTotalNum - 1)
                    {
                        ServoUpdateFrame(mRobotUpdateData, mRobotUpdateFrameNum, true);
                    }
                    else
                    {
                        ServoUpdateFrame(mRobotUpdateData, mRobotUpdateFrameNum, false);
                    }
                    ++mRobotUpdateFrameNum;
                }
                float per = mRobotUpdateFrameNum / (mRobotUpdateTotalNum + 0.0f) * 100;
                int perInt = (int)per;
                if (perInt <= 0) perInt = 0;
                EventMgr.Inst.Fire(EventID.Update_Progress, new EventArg(perInt));
                //WaitPromptMsg.UpdateText(string.Format(LauguageTool.GetIns().GetText("正在升级"), perInt.ToString()));
                //WaitPromptMsg.UpdateText(string.Format(LauguageTool.GetIns().GetText("正在升级"), (int)((mRobotUpdateFrameNum / (mRobotUpdateTotalNum + 0.0f)) * 100)));
            }
            else
            {
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(false));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            }
        }
        catch (System.Exception ex)
        {
            mRobotUpdateData = null;
            mRobotUpdateFrameNum = 0;
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(false));
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    private void ServoUpdateFinishedCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("连接超时"));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("连接超时"));
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                return;
            }
            else if (len <= 0)
            {
                EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(false));
                //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
                mRobotUpdateData = null;
                mRobotUpdateFrameNum = 0;
                return;
            }
            if (len == 1)
            {//接收数据成功或者升级成功
                CommonMsgAck msg = new CommonMsgAck();
                msg.Read(br);
                if (msg.result == (byte)ErrorCode.Result_OK)
                {//接收数据成功，进入正式升级
                    //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("等待升级完成"), LauguageTool.GetIns().GetText("确定"), ServoUpdateSuccessOnClick);
                    /*WaitPromptMsg.OnSleepRightBtn();
                    DealyOnWakeRightBtn();*/
                    PlatformMgr.Instance.SetSendXTState(false);
                }
                else if (msg.result == (byte)0xAA)
                {//舵机升级成功
                    //WaitPromptMsg.OnWakeRightBtn();
                    //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级完成"));
                    EventMgr.Inst.Fire(EventID.Update_Finished, new EventArg(false));
                    PlatformMgr.Instance.SetSendXTState(true);
                    //isServoUpdateFlag = false;
                }
            }
            else if (len >= 4)
            {//升级失败
                ServoUpdateFailAck msg = new ServoUpdateFailAck();
                msg.Read(br);
                EventMgr.Inst.Fire(EventID.Update_Fail, new EventArg(false, msg));
                PlatformMgr.Instance.SetSendXTState(true);
                //WaitPromptMsg.OnWakeRightBtn();
                //WaitPromptMsg.UpdateText(string.Format(LauguageTool.GetIns().GetText("舵机升级失败"), PublicFunction.ListToString(msg.servoList)), ServoUpdateFailOnClick);
            }
        }
        catch (System.Exception ex)
        {
            EventMgr.Inst.Fire(EventID.Update_Error, new EventArg(false));
            //WaitPromptMsg.UpdateText(LauguageTool.GetIns().GetText("升级异常"));
            mRobotUpdateData = null;
            mRobotUpdateFrameNum = 0;
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    void DealyOnWakeRightBtn()
    {
        Timer.Add(20, 1, 1, WaitPromptMsg.OnWakeRightBtn);
    }

    private void ServoUpdateStopCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                return;
            }
            else if (len <= 0)
            {
                return;
            }
            CommonMsgAck msg = new CommonMsgAck();
            msg.Read(br);
            if (msg.result == (byte)ErrorCode.Result_OK)
            {//成功
            }
            else
            {
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

    private void ReadDeviceCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {//读取失败
                EventMgr.Inst.Fire(EventID.Connected_Jimu_Result, new EventArg(this, false));
                return;
            }
            else if (len <= 1)
            {
                EventMgr.Inst.Fire(EventID.Connected_Jimu_Result, new EventArg(this, true));
                return;
            }
            ReadDeviceTypeMsgAck msg = new ReadDeviceTypeMsgAck(len);
            msg.Read(br);
            if (msg.name.StartsWith("JIMU") || msg.name.StartsWith("Jimu"))
            {//
                EventMgr.Inst.Fire(EventID.Connected_Jimu_Result, new EventArg(this, true));
            }
            else
            {
                EventMgr.Inst.Fire(EventID.Connected_Jimu_Result, new EventArg(this, false));
            }
        }
        catch (System.Exception ex)
        {
            EventMgr.Inst.Fire(EventID.Connected_Jimu_Result, new EventArg(this, true));
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    void ReadSensorDataCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                if (!SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                }
                if (exCmd == ExtendCMDCode.ReadSpeakerData)
                {
                }
                else
                {
                    ReadSensorDataFinished(exCmd, CallUnityResult.failure);
                }
                return;
            }
            else if (len <= 1)
            {
                if (exCmd == ExtendCMDCode.ReadSpeakerData)
                {
                }
                else
                {
                    ReadSensorDataFinished(exCmd, CallUnityResult.failure);
                }
                return;
            }
            ReadSensorDataMsgAck msg = new ReadSensorDataMsgAck();
            msg.Read(br);
            TopologyPartType sensorType = (TopologyPartType)msg.sensorType;
            if (null != mReadSensorDataDict && mReadSensorDataDict.ContainsKey(sensorType))
            {
                mReadSensorDataDict[sensorType].ReadCallBackMsg(br, len - 1);
                if (exCmd == ExtendCMDCode.ReadSpeakerData)
                {
                    SpeakerData speakerData = (SpeakerData)GetReadSensorData(TopologyPartType.Speaker);
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
                        }
                    }
                }
                else
                {
                    string error = mReadSensorDataDict[sensorType].GetErrorID();
                    if (string.IsNullOrEmpty(error))
                    {
                        ReadSensorDataFinished(exCmd, CallUnityResult.success);
                    }
                    else
                    {
                        ReadSensorDataFinished(exCmd, CallUnityResult.failure);
                        SensorErrorPrompt(sensorType, error);
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

    void PopSpeakerOnClick(GameObject obj)
    {
        if (PromptMsg.RightBtnName.Equals(obj.name))
        {
            PlatformMgr.Instance.ConnectSpeaker(string.Empty);
        }
    }

    void ReadSensorDataFinished(ExtendCMDCode exCmd, CallUnityResult result)
    {
        switch (exCmd)
        {
            case ExtendCMDCode.ReadInfraredData:
                SingletonObject<LogicCtrl>.GetInst().QueryInfraredCallBack(result);
                break;
            case ExtendCMDCode.ReadTouchData:
                SingletonObject<LogicCtrl>.GetInst().QueryTouchStatusCallBack(result);
                break;
            case ExtendCMDCode.ReadGyroData:
                SingletonObject<LogicCtrl>.GetInst().QueryGyroscopeCallBack(result);
                break;
            case ExtendCMDCode.ReadAllSensorData:
                SingletonObject<LogicCtrl>.GetInst().QueryAllSensorCallBack(result);
                break;

        }
    }

    void SendSensorDataCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                if (!SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                }
                LogicSetSensorDataCallBack(exCmd, CallUnityResult.failure);
                return;
            }
            else if (len < 3)
            {
                LogicSetSensorDataCallBack(exCmd, CallUnityResult.failure);
                return;
            }
            else
            {
                SendSensorDataMsgAck msg = new SendSensorDataMsgAck();
                msg.Read(br);
                if (msg.result == 0)
                {
                    LogicSetSensorDataCallBack(exCmd, CallUnityResult.success);
                }
                else
                {
                    
                    LogicSetSensorDataCallBack(exCmd, CallUnityResult.failure);
                    TopologyPartType partType = (TopologyPartType)msg.sensorData.sensorType;
                    SensorErrorPrompt(partType, PublicFunction.ListToString(msg.sensorData.ids));

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

    void LogicSetSensorDataCallBack(ExtendCMDCode cmd, CallUnityResult result)
    {
        if (cmd == ExtendCMDCode.SendEmojiData)
        {
            SingletonObject<LogicCtrl>.GetInst().SetEmojiCallBack(result);
        }
        else if (cmd == ExtendCMDCode.SendDigitalTubeData)
        {
            SingletonObject<LogicCtrl>.GetInst().SetDigitalTubeCallBack(result);
        }
    }

    void SendLightDataCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                if (!SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                }
                SingletonObject<LogicCtrl>.GetInst().SetLEDsCallBack(CallUnityResult.failure);
                return;
            }
            else if (len < 3)
            {
                SingletonObject<LogicCtrl>.GetInst().SetLEDsCallBack(CallUnityResult.failure);
                return;
            }
            SendSensorDataMsgAck msg = new SendSensorDataMsgAck();
            msg.Read(br);
            if (msg.result == 0)
            {
                SingletonObject<LogicCtrl>.GetInst().SetLEDsCallBack(CallUnityResult.success);
            }
            else
            {
                SingletonObject<LogicCtrl>.GetInst().SetLEDsCallBack(CallUnityResult.failure);
                TopologyPartType partType = (TopologyPartType)msg.sensorData.sensorType;
                SensorErrorPrompt(partType, PublicFunction.ListToString(msg.sensorData.ids));
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
    List<byte[]> mReadSensorCacheData;
    void ReadSensorDataOtherCallBack(int len, BinaryReader br, ExtendCMDCode exCmd)
    {
        try
        {
            if (null == br)
            {
                if (!SingletonObject<LogicCtrl>.GetInst().IsLogicProgramming)
                {
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("连接超时"));
                }
                ReadSensorDataFinished(exCmd, CallUnityResult.failure);
                return;
            }
            else if (len <= 2)
            {
                ReadSensorDataFinished(exCmd, CallUnityResult.failure);
                return;
            }
            ReadSensorDataOtherMsgAck msg = new ReadSensorDataOtherMsgAck();
            msg.Read(br);
            if (msg.nowFrame == 1)
            {
                mReadSensorCacheData = new List<byte[]>();
            }
            if (null != mReadSensorCacheData)
            {
                byte[] bytes = br.ReadBytes(len - 2);
                mReadSensorCacheData.Add(bytes);
            }
            if (msg.nowFrame == msg.totalFrame)
            {//数据读取完毕
                int bytesLen = 0;
                for (int i = 0, imax = mReadSensorCacheData.Count; i < imax; ++i)
                {
                    bytesLen += mReadSensorCacheData[i].Length;
                }
                byte[] bytes = new byte[bytesLen];
                int index = 0;
                for (int i = 0, imax = mReadSensorCacheData.Count; i < imax; ++i)
                {
                    Array.Copy(mReadSensorCacheData[i], 0, bytes, index, mReadSensorCacheData[i].Length);
                    index += mReadSensorCacheData[i].Length;
                }
                MemoryStream ms = new MemoryStream(bytes);
                BinaryReader reader = new BinaryReader(ms);

                byte sensorNum = reader.ReadByte();
                CallUnityResult result = CallUnityResult.success;
                for (int i = 0; i < sensorNum; ++i)
                {
                    TopologyPartType sensorType = (TopologyPartType)reader.ReadByte();
                    if (null != mReadSensorDataDict && mReadSensorDataDict.ContainsKey(sensorType))
                    {
                        mReadSensorDataDict[sensorType].ReadCallBackMsg(reader, 0);
                        string error = mReadSensorDataDict[sensorType].GetErrorID();
                        if (!string.IsNullOrEmpty(error))
                        {
                            result = CallUnityResult.failure;
                            SensorErrorPrompt(sensorType, error);
                        }
                    }
                }
                ms.Dispose();
                ms.Close();
                reader.Close();
                ReadSensorDataFinished(exCmd, result);
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

    void SensorErrorPrompt(TopologyPartType partType, string error)
    {
        switch (partType)
        {
            case TopologyPartType.Infrared:
                mSelfCheckErrorFlag = true;
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("红外传感器连接异常"), error), SelfExceptionOnClick);
                }
                else
                {
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("红外传感器连接异常"), error), SelfCheckErrorOnClick);
                }
                break;
            case TopologyPartType.Gyro:
                mSelfCheckErrorFlag = true;
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("陀螺仪传感器连接异常"), error), SelfExceptionOnClick);
                }
                else
                {
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("陀螺仪传感器连接异常"), error), SelfCheckErrorOnClick);
                }
                break;
            case TopologyPartType.Touch:
                mSelfCheckErrorFlag = true;
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("触碰传感器连接异常"), error), SelfExceptionOnClick);
                }
                else
                {
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("触碰传感器连接异常"), error), SelfCheckErrorOnClick);
                }
                break;
            case TopologyPartType.DigitalTube:
                mSelfCheckErrorFlag = true;
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("数码管传感器连接异常"), error), SelfExceptionOnClick);
                }
                else
                {
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("数码管传感器连接异常"), error), SelfCheckErrorOnClick);
                }
                break;
            case TopologyPartType.Gravity:
                break;
            case TopologyPartType.Light:
                mSelfCheckErrorFlag = true;
                if (LogicCtrl.GetInst().IsLogicProgramming)
                {
                    LogicCtrl.GetInst().ExceptionCallBack(string.Format(LogicLanguage.GetText("灯光传感器连接异常"), error), SelfExceptionOnClick);
                }
                else
                {
                    PromptMsg.ShowDoublePrompt(string.Format(LauguageTool.GetIns().GetText("灯光传感器连接异常"), error), SelfCheckErrorOnClick);
                }
                break;
            case TopologyPartType.Speaker:
                break;
            case TopologyPartType.Ultrasonic:
                break;
        }
    }
    #endregion
}