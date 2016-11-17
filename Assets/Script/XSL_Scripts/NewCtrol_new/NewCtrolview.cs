//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Event;
using Game.Scene;

public class NewCtrolview : MonoBehaviour {

    public static string ControllerID;    //控制器ID
    public string SelectActionName;       //被选中的动作名称
    public Transform TotalActList;        //全部动作挂载的结点
    public Transform UserActList;         //用户动作挂载的结点            
    public GameObject item2;
    public GameObject EditedAction; //正在编辑的动作
    public GameObject EditWinObj;   //编辑动作窗口
    public GameObject ControlWinObj;  //动作控制窗口
    public GameObject CancelBtn;      //动作编辑取消按钮
    public GameObject OkBtn;          //动作编辑OK按钮
    public GameObject BackBtn;        //退出控制器按钮
    public GameObject ConnectBtn;     //蓝牙连接按钮
    public GameObject StopActionBtn;   //动作停止按钮
    private bool IsEdit;              //是否是编辑界面
    public UIPopupList playModeSelect;

	// Use this for initialization
	void Start () {
        if (playModeSelect != null)
        {
            playModeSelect.transform.parent.gameObject.SetActive(ModelDetailsTool.SpecialFlag);
        }
        EventMgr.Inst.Regist(EventID.SwitchEdit, EnterEditMode);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        ControllerID = "_ACONTROLLER";
        UpdateUserActions();
        if (playModeSelect != null)
        {
           // playModeSelect.onChange = new List<EventDelegate>(OnPlayModeSelect);
            EventDelegate.Add(playModeSelect.onChange, OnPlayModeSelect);
        }
        //if (SwitchBtn != null)
        //{
        // //   UIEventListener.Get(SwitchBtn.gameObject).onClick = 
        //    UIEventListener.Get(SwitchBtn.gameObject).onClick = 
        //}
        if (CancelBtn != null)
        {
            UIEventListener.Get(CancelBtn).onClick = OnCancelClicked;
        }
        if (OkBtn != null)
        {
            UIEventListener.Get(OkBtn).onClick = OnOkClicked;
        }
        if (BackBtn != null)
        {
            UIEventListener.Get(BackBtn).onClick = OnBackClicked;
        }
        if (ConnectBtn != null)
        {
            UIEventListener.Get(ConnectBtn).onClick = OnConnectClicked;
            if (RobotManager.GetInst().GetCurrentRobot().Connected)
            {
                ConnectBtn.GetComponentInChildren<UISprite>().spriteName = "connect";
            }
            else
            {
                ConnectBtn.GetComponentInChildren<UISprite>().spriteName = "disconnect";
            }
            ConnectBtn.GetComponentInChildren<UISprite>().MakePixelPerfect();
        }
        if (StopActionBtn != null)
        {
            UIEventListener.Get(StopActionBtn).onClick = OnStopClicked;
        }
        InitEnvironment();
        if (ActionLogic.GetIns().IsPlayListNull())   //初次进来如果没有动作默认配置
        {
            IsEdit = true;
        //    EventMgr.Inst.Fire(EventID.SwitchEdit,
           // EnterEditMode(new EventArg(true));
            EventMgr.Inst.Fire(EventID.SwitchToogle,new EventArg(IsEdit));
        }
        else
        {
            IsEdit = false;
         //   EnterEditMode(new EventArg(false));
            EventMgr.Inst.Fire(EventID.SwitchToogle, new EventArg(IsEdit));
        }

        //MoveSecond.Instance.GOInControl();
	}

    void OnDestroy()
    {
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
    }

    void OnPlayModeSelect()
    {
        ActionPlayModel mode = ActionPlayModel.none;
        switch (playModeSelect.value.ToString())
        {
            case "None":
                mode = ActionPlayModel.none;
                break;
            case "Random":
                mode = ActionPlayModel.random;
                break;
            case "Sequence":
                mode = ActionPlayModel.sequence;
                break;
            case "SequenceCircle":
                mode = ActionPlayModel.sequenceCircle;
                break;
            case "SingleCircle":
                mode = ActionPlayModel.singleCircle;
                break;
            default:
                mode = ActionPlayModel.none;
                break;
        }
        ActionLogic.GetIns().CurPlayMode = mode;
    }

    void InitEnvironment()
    {
        if (UserActList == null)
            return;
        for (int i = 0; i < UserActList.childCount; i++)
        {
            UIEventListener.Get(UserActList.GetChild(i).gameObject).onClick = OnActionClicked;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flag"></param>
    bool OnceFlag = false;
    private void EnterEditMode(EventArg arg)
    {
        IsEdit = (bool)arg[0];
        if (IsEdit)  //编辑
        {
            if (!OnceFlag)
            {
                OnceFlag = !OnceFlag;
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("编辑状态提示"), HUDTextTips.Color_Green, 2f);
            }
            if (ActionLogic.GetIns().IsConnect)  //进入编辑状态，停止动作
            {
                ActionLogic.GetIns().DoStopAction(null);
                if (curOperateAction != null)
                {
                    curOperateAction.GetComponent<UISprite>().spriteName = "icon_control";
                }
                playModeSelect.value = "None";
            }
            if (!StepManager.GetIns().OpenOrCloseGuide) //非指引模式下
            {
                for (int i = 0; i < UserActList.childCount; i++)
                {
                    UserActList.GetChild(i).GetComponent<Animation>().Play();
                    UserActList.GetChild(i).GetComponent<Animation>().wrapMode = WrapMode.Loop;
                }
            }
            StopActionBtn.GetComponentInChildren<UISprite>().enabled = false;
            playModeSelect.gameObject.SetActive(false);

            if (StepManager.GetIns().OpenOrCloseGuide)//GuideViewBase.OpenOrCloseGuide)   //教程 步骤没有成功
            {   
                EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().TurnToControlStep, false));
            }
        }
        else
        {
            StopActionBtn.GetComponentInChildren<UISprite>().enabled = true;
            playModeSelect.gameObject.SetActive(true);
            for (int i = 0; i < UserActList.childCount; i++)
            {
                UserActList.GetChild(i).GetComponent<Animation>().wrapMode = WrapMode.Once;
            }
            if (StepManager.GetIns().OpenOrCloseGuide)//GuideViewBase.OpenOrCloseGuide)   //如果是教程，通知这个步骤已经完成
            {
                EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().TurnToControlStep,true));
            }
        }
    }

    int index = -1;
    private GameObject curOperateAction = null;
    private string SelectActionIcon;
    void OnActionClicked(GameObject go)
    {
        if (IsEdit)  //编辑状态下的点击
        {
            index = go.transform.GetSiblingIndex();       //记录当前点击动作的序列号
            SelectActionName = go.GetComponentInChildren<UILabel>().text;
            SelectActionIcon = go.transform.GetChild(0).GetComponent<UISprite>().spriteName;
            ShowEditWin();
        }
        else    //运行状态下的点击
        {
            bool flager = false;
            if (go.GetComponentInChildren<UILabel>().text == LauguageTool.GetIns().GetText("FuWei"))  //翻译
            {
                flager = true;
                go.GetComponentInChildren<UILabel>().text = PublicFunction.Default_Actions_Name;
            }
            ActionLogic.GetIns().DoTouchItem_New(go);
            if (flager)
            {
                go.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("FuWei");
            }
            curOperateAction = go;
            if (!ActionLogic.GetIns().IsConnect)
                ConnectBtn.GetComponent<Animation>().Play();
        }
    }

    void ShowEditWin()
    {
        if (EditWinObj == null || ControlWinObj == null)
            return;
        EditWinObj.SetActive(true);
        ControlWinObj.SetActive(false);
        hehe = UpdateAllActions;
        StartCoroutine(DoNextFrame(hehe));  //更新所有的动作
        
        EditedAction.GetComponentInChildren<UILabel>().text = SelectActionName;
        EditedAction.transform.GetChild(0).GetComponent<UISprite>().spriteName = SelectActionIcon; //
        EditedAction.transform.GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
    }

    EventDelegate.Callback hehe;
    IEnumerator DoNextFrame(EventDelegate.Callback hehe)
    {
        for (int i = 0; i < TotalActList.childCount; i++)
        {
            Destroy(TotalActList.GetChild(i).gameObject);
        }
        yield return null;
        hehe();
    }

    void ShowContrWin()
    {
        if (EditWinObj == null || ControlWinObj == null)
            return;
        EditWinObj.SetActive(false);
        ControlWinObj.SetActive(true);
        if (RobotManager.GetInst().GetCurrentRobot().Connected)
        {
            ConnectBtn.GetComponentInChildren<UISprite>().spriteName = "connect";
            ConnectBtn.GetComponentInChildren<UISprite>().MakePixelPerfect();
        }
        UpdateUserActions();
    }

    /// <summary>
    /// 更新用户动作表
    /// </summary>
    void UpdateUserActions()
    {
        ActionLogic.GetIns().ClearPlayList();

        if (ControlData.GetIns().curActionData == null)
            return;
        bool flag = false;
        for (int j = 0; j < UserActList.childCount; j++)     //用户的动作表 
        {
            string str = ControlData.GetIns().curActionData.ActionList[j];
            if (RobotManager.GetInst().GetCurrentRobot().GetActionsNameList().Contains(str))
            {
                if (str == PublicFunction.Default_Actions_Name)  //复位动作翻译
                {
                    str = LauguageTool.GetIns().GetText("FuWei");
                }
                UserActList.GetChild(j).GetComponentInChildren<UILabel>().text = str;
                ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(str);
                if (null == act)
                {
                    UserActList.GetChild(j).GetChild(0).GetComponent<UISprite>().spriteName = "add";
                }
                else
                {
                    UserActList.GetChild(j).GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                }
                ActionLogic.GetIns().AddToPlayList(UserActList.GetChild(j).gameObject);
            }
            else
            {
                flag = true;
                ControlData.GetIns().curActionData.ActionList[j] = "";
                UserActList.GetChild(j).GetComponentInChildren<UILabel>().text = "";
                UserActList.GetChild(j).GetChild(0).GetComponent<UISprite>().spriteName = "null";
            }
            UserActList.GetChild(j).GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
        }
        if (flag)
        {
            ControlData.GetIns().SaveToXml(ControlData.GetIns().curActionData);
        }
    }

    /// <summary>
    /// 更新所有的动作列表
    /// </summary>
    void UpdateAllActions()
    {
        List<string> totalActions = RobotManager.GetInst().GetCurrentRobot().GetActionsNameList();

        ModelDetailsTool.BuildItems(TotalActList, item2, totalActions.Count+1);  //多创建一个空的图标
        for (int i = 0; i < totalActions.Count; i++)   //第一个留空白图标
        {
            TotalActList.GetChild(i+1).GetComponentInChildren<UILabel>().text = totalActions[i];
            if (totalActions[i] == PublicFunction.Default_Actions_Name)  //复位中英文切换
            {
                TotalActList.GetChild(i + 1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("FuWei");
            }
            ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(totalActions[i]);
            if (null == act)
            {
                TotalActList.GetChild(i + 1).GetChild(0).GetComponent<UISprite>().spriteName = "add";
            }
            else
            {
                TotalActList.GetChild(i + 1).GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
            }
            TotalActList.GetChild(i+1).GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
            if (ControlData.GetIns().curActionData.ActionList.Contains(totalActions[i])) //如果该动作已经包含在
            {
                //UserActList.GetChild(i).gameObject.GetComponent<UIButtonColor>().defaultColor = 
            }
            UIEventListener.Get(TotalActList.GetChild(i+1).gameObject).onClick = OnActionSelect;
        }
        UIEventListener.Get(TotalActList.GetChild(0).gameObject).onClick = OnActionSelect;
        TotalActList.GetChild(0).GetChild(0).GetComponent<UISprite>().spriteName = "null";
        TotalActList.GetChild(0).GetComponentInChildren<UILabel>().text = "";
    }

    /// <summary>
    /// 选中动作列表中的某个动作
    /// </summary>
    /// <param name="go"></param>
    private GameObject lastSelectObj = null;
    void OnActionSelect(GameObject go)
    {
        if (lastSelectObj != go)
        {
            if (lastSelectObj != null)
            {
                //lastSelectObj.transform.GetComponent<UISprite>().spriteName = "actionBg1";
                Destroy(lastSelectObj.transform.GetChild(lastSelectObj.transform.childCount - 1).gameObject);
            }
            lastSelectObj = go;
            //Sprite sp = new Sprite();
            GameObject tt = Resources.Load("Prefabs/actionItemSelected") as GameObject;
            tt = Instantiate(tt) as GameObject;
            UISprite tobj = tt.GetComponent<UISprite>();
           // for(int i = 0;i<tobj.GetComponents<
            tobj.name = "BeSelected";
            tobj.spriteName = "btn_action_sel";
            tobj.MakePixelPerfect();
            tobj.transform.SetParent(lastSelectObj.transform);
            tobj.transform.localScale = Vector3.one;
            tobj.transform.localPosition = Vector3.zero;
            //sp.name = "btn_action_sel";
            
           // lastSelectObj.transform.GetComponent<UISprite>().spriteName = "btn_actions_sel";

        }
        string selectName = go.GetComponentInChildren<UILabel>().text;
        
        EditedAction.GetComponentInChildren<UILabel>().text = selectName;
        EditedAction.transform.GetChild(0).GetComponent<UISprite>().spriteName = go.transform.GetChild(0).GetComponent<UISprite>().spriteName; //
        EditedAction.transform.GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
        SelectActionName = selectName;   
    }
    /// <summary>
    /// ok button
    /// </summary>
    /// <param name="go"></param>
    void OnOkClicked(GameObject go)
    {
        if (SelectActionName == "")
            ControlData.GetIns().RemoveControllerAction(index);
        else
        {
            if (SelectActionName == LauguageTool.GetIns().GetText("FuWei"))  //界面上的复位翻译
            {
                SelectActionName = PublicFunction.Default_Actions_Name;
            }
            ControlData.GetIns().curActionData.ActionList[index] = SelectActionName;
            ControlData.GetIns().SaveToXml(ControlData.GetIns().curActionData);
        }
        ControlData.GetIns().UpdateActionData(); //更新数据 接着等待一帧
        StartCoroutine(ModelDetailsWindow.DoAtNextFrame(UpdateControllerData));
    }

    void UpdateControllerData()
    {
        ShowContrWin();
        EventMgr.Inst.Fire(EventID.SwitchToogle, new EventArg(IsEdit));
    }
    /// <summary>
    /// cancel button
    /// </summary>
    /// <param name="go"></param>
    void OnCancelClicked(GameObject go)
    {
        ShowContrWin();
        EventMgr.Inst.Fire(EventID.SwitchToogle, new EventArg(IsEdit));
    }
    /// <summary>
    /// back
    /// </summary>
    /// <param name="go"></param>
    void OnBackClicked(GameObject go)
    {
        GameObject oriGO = GameObject.Find("oriGO");
        if(oriGO!=null)
        {
            DontDestroyOnLoad(oriGO);
            SceneMgrTest.Instance.LastScene = SceneType.ActionPlay;
        }

        BackBtn.SetActive(false);
        if(RobotManager.GetInst().GetCurrentRobot().Connected)
            OnStopClicked(null);  //返回之前停止运动
        Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
    }
    /// <summary>
    /// connect
    /// </summary>
    /// <param name="go"></param>
    void OnConnectClicked(GameObject go)
    { 
        ModelDetailsWindow.ConnectBluetooth();
    }

    void OnStopClicked(GameObject go)
    {
        if (ActionLogic.GetIns().IsConnect)
        {
            ActionLogic.GetIns().DoStopAction(go);
            if (curOperateAction != null)
            {
                curOperateAction.GetComponent<UISprite>().spriteName = "icon_control";
            }
            playModeSelect.value = "None";
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
            ConnectBtn.GetComponent<Animation>().Play();
        }
    }

    void OnBlueConnectResult(EventArg arg)
    {
        try
        {
            bool flag = (bool)arg[0];
            string iconName;
            if (flag)
            {
                iconName = "connect";
            }
            else
            {
                iconName = "disconnect";
            }
            if (null != ConnectBtn)
            {
                UISprite sprite = ConnectBtn.GetComponentInChildren<UISprite>();
                if (null != sprite)
                {
                    sprite.spriteName = iconName;
                    sprite.MakePixelPerfect();
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
