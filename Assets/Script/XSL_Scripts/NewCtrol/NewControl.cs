using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Event;
using Game.Platform;

public class NewControl : MonoBehaviour {

    public static string ControllerID;
    public GameObject SettingBtn;
    public GameObject StopActionBtn;
    public GameObject OkBtn;
    public GameObject BackBtn;
 //   public GameObject ConnectBtn;
    public Transform UserActionTrans;
    public Transform OtherActionTrans;
    private GameObject curOperateAction = null;
    public Camera UICam;
    public Transform OpenTestBTN;

    private bool _isEdit;
    public bool IsEdit
    {
        get
        {
            return _isEdit;
        }
        set
        {
            _isEdit = value;
            OkBtn.SetActive(value);
            //ConnectBtn.SetActive(!value);
            StopActionBtn.SetActive(!value);
            SettingBtn.SetActive(!value);
            string bgName = "";
            for (int i = 0; i < UserActionTrans.parent.GetChild(1).childCount; i++)
            {
                UserActionTrans.parent.GetChild(1).GetChild(i).GetComponent<UISprite>().enabled = !value;
            }
            if (value)
            {
                UIEventListener.Get(BackBtn).onClick = OnCancelClick;
                bgName = "circular_sel";
            }
            else
            {
                UIEventListener.Get(BackBtn).onClick = OnBackClick;
                bgName = "180";
            }
            for (int i = 0; i < UserActionTrans.childCount; i++)
            {
                UserActionTrans.GetChild(i).GetComponent<UISprite>().spriteName = bgName;
                UserActionTrans.GetChild(i).GetComponent<UISprite>().SetDimensions(180, 180);
                //if(bgName == "circular_sel")
                //{
                //    UserActionTrans.GetChild(i).GetComponent<UISprite>().SetDimensions(198,198);
                //}
                //else
                //{
                //    UserActionTrans.GetChild(i).GetComponent<UISprite>().SetDimensions(180,180);
                //}
            }
            if (mainwindow != null && downSlider != null) //不锁定
            {
                Vector3 from1 = mainwindow.localPosition; ;
                Vector3 from2 = downSlider.localPosition; ;
                Vector3 to1 = Vector3.zero;
                Vector3 to2 = Vector3.zero;
                if (value) //
                {
                    to1 = new Vector3(mainwindow.localPosition.x, mainwindow.localPosition.y + 90, mainwindow.localPosition.z);
                    to2 = new Vector3(downSlider.localPosition.x, downSlider.localPosition.y + 159, downSlider.localPosition.z);
                    if (mainwindow.GetComponent<TweenPosition>() != null)
                    {
                        from1 = mainwindow.GetComponent<TweenPosition>().to;
                        to1 = mainwindow.GetComponent<TweenPosition>().from;
                        DestroyImmediate(mainwindow.GetComponent<TweenPosition>());
                    }
                    //if (mainwindow.GetComponent<TweenScale>() != null)
                    //{
                    //    DestroyImmediate(mainwindow.GetComponent<TweenScale>());
                    //}
                    if (downSlider.GetComponent<TweenPosition>() != null)
                    {
                        from2 = downSlider.GetComponent<TweenPosition>().to;
                        to2 = downSlider.GetComponent<TweenPosition>().from;
                        DestroyImmediate(downSlider.GetComponent<TweenPosition>());
                    }
                    downSlider.gameObject.AddComponent<TweenPosition>();
                    mainwindow.gameObject.AddComponent<TweenPosition>();
                  //  mainwindow.gameObject.AddComponent<TweenScale>();
                    mainwindow.GetComponent<TweenPosition>().from = from1;
                    mainwindow.GetComponent<TweenPosition>().to = to1;
                 //   mainwindow.GetComponent<TweenScale>().from = Vector3.one;
                 //   mainwindow.GetComponent<TweenScale>().to = new Vector3(0.95f, 0.95f, 0.95f);
                    downSlider.GetComponent<TweenPosition>().from = from2;
                    downSlider.GetComponent<TweenPosition>().to = to2;
                    //mainwindow.localPosition = new Vector3(mainwindow.localPosition.x, mainwindow.localPosition.y + 61, mainwindow.localPosition.z);
                    //downSlider.localPosition = new Vector3(downSlider.localPosition.x, downSlider.localPosition.y + 176, downSlider.localPosition.z);
                    //mainwindow.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

                    
                }
                else
                {
                    to1 = new Vector3(mainwindow.localPosition.x, mainwindow.localPosition.y - 90, mainwindow.localPosition.z);
                    to2 = new Vector3(downSlider.localPosition.x, downSlider.localPosition.y - 159, downSlider.localPosition.z);
                    if (mainwindow.GetComponent<TweenPosition>() != null)
                    {
                        from1 = mainwindow.GetComponent<TweenPosition>().to;
                        to1 = mainwindow.GetComponent<TweenPosition>().from;
                        DestroyImmediate(mainwindow.GetComponent<TweenPosition>());
                    }
                    //if (mainwindow.GetComponent<TweenScale>() != null)
                    //{
                    //    DestroyImmediate(mainwindow.GetComponent<TweenScale>());
                    //}
                    if (downSlider.GetComponent<TweenPosition>() != null)
                    {
                        from2 = downSlider.GetComponent<TweenPosition>().to;
                        to2 = downSlider.GetComponent<TweenPosition>().from;
                        DestroyImmediate(downSlider.GetComponent<TweenPosition>());
                    }
                    downSlider.gameObject.AddComponent<TweenPosition>();
                    mainwindow.gameObject.AddComponent<TweenPosition>();
                   // mainwindow.gameObject.AddComponent<TweenScale>();
                    mainwindow.GetComponent<TweenPosition>().from = from1;
                    mainwindow.GetComponent<TweenPosition>().to = to1;
                  //  mainwindow.GetComponent<TweenScale>().from = new Vector3(0.95f, 0.95f, 0.95f);
                 //   mainwindow.GetComponent<TweenScale>().to = Vector3.one;
                    downSlider.GetComponent<TweenPosition>().from = from2;
                    downSlider.GetComponent<TweenPosition>().to = to2;

                    //mainwindow.transform.localPosition = new Vector3(mainwindow.localPosition.x, mainwindow.localPosition.y - 61, mainwindow.localPosition.z);
                    //downSlider.localPosition = new Vector3(downSlider.localPosition.x, downSlider.localPosition.y - 176, downSlider.localPosition.z);
                    //mainwindow.transform.localScale = Vector3.one;
                }
                mainwindow.GetComponent<TweenPosition>().duration = 0.3f;
                //   mainwindow.GetComponent<TweenScale>().duration = 0.3f;
                downSlider.GetComponent<TweenPosition>().duration = 0.4f;
            }
            StartCoroutine(WaitOneFrame(value));
        }
    }

    IEnumerator WaitOneFrame(bool value)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        EventMgr.Inst.Fire(EventID.SwitchEdit, new EventArg(value));
    }

    void OnTestAtals(GameObject go)
    {
        Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.testScene);
    }

    Transform mainwindow;
    Transform downSlider;
    void Start()
    {
        //MyAnimtionCurve cur1 = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
        //if (SettingBtn != null)
        //{
        //    TweenPosition tween = GetTCompent.GetCompent<TweenPosition>(SettingBtn.transform);
        //    tween.animationCurve = cur1.animCurve;
        //    tween.from = SettingBtn.transform.localPosition;
        //    tween.to = new Vector3(tween.from.x - 255, tween.from.y, tween.from.z);
        //    tween.duration = 0.5f;
        //}

        if (OpenTestBTN != null)
        {
            UIEventListener.Get(OpenTestBTN.gameObject).onClick = OnTestAtals;
        }
        EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, GetPowerState);
        ControllerID = "_ACONTROLLER";
        UpdateModelActions();
        IsEdit = false;
        if (SettingBtn != null)
            UIEventListener.Get(SettingBtn).onClick = OnSetting;
        if (OkBtn != null)
            UIEventListener.Get(OkBtn).onClick = OnOkClick;
        if (StopActionBtn != null)
            UIEventListener.Get(StopActionBtn).onClick = OnStopPlay;
        //if (ConnectBtn != null)
        //    UIEventListener.Get(ConnectBtn).onClick = OnConnectClick;
        if(GameObject.Find("UIRoot/BackGround/Mainwindow") != null)
            mainwindow = GameObject.Find("UIRoot/BackGround/Mainwindow").transform;
        if (GameObject.Find("UIRoot/BackGround/ContainerDown") != null)
            downSlider = GameObject.Find("UIRoot/BackGround/ContainerDown").transform;

        if (ActionLogic.GetIns().IsConnect) //
        {
            EventMgr.Inst.Fire(EventID.Read_Power_Msg_Ack);
        }

        if (UICam != null)
        {
            StartCoroutine(ShowLeftFrame());
        }
    }

    IEnumerator ShowLeftFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        if (UICam != null)
            UICam.enabled = true;
    }

    void OnDestroy()
    {
        GameObject cam = GameObject.Find("Camera");
        if (cam != null && cam.tag == "MainCamera")
            DestroyImmediate(GameObject.Find("Camera"));

        GameObject oriT = GameObject.Find("OriGO");
        if (oriT != null)
        {
            DontDestroyOnLoad(oriT);
        }
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, GetPowerState);
    }

    #region
    void OnSetting(GameObject go)
    {
        Transform t = null;
		mainwindow.GetComponent<UIWidget> ().SetAnchor (t);
		downSlider.GetComponent<UIWidget> ().SetAnchor (t);
        IsEdit = true;
        UpdateLeftActions();
    }
    /// <summary>
    /// 动作播放
    /// </summary>
    /// <param name="go"></param>
    void OnActionPlay(GameObject go)
    {
        if (IsEdit)  //编辑状态下的点击
        {
            
        }
        else    //运行状态下的点击
        {
            if (go != null)
            {
            ActionLogic.GetIns().DoTouchItem_New(go);  //有状态和时间信息
            if (ActionLogic.GetIns().IsConnect)  //播放的效果
            {
                //ConnectBtn.GetComponent<Animation>().Play();
                go.transform.GetChild(go.transform.childCount - 1).gameObject.SetActive(true);
                TweenScale scale1 = go.transform.parent.GetComponent<TweenScale>();
                TweenScale scale2 = go.transform.GetChild(go.transform.childCount - 1).GetComponent<TweenScale>();
                TweenAlpha color1 = go.transform.GetChild(go.transform.childCount - 1).GetComponent<TweenAlpha>();
                if (scale1 != null)
                {
                    scale1.ResetToBeginning();
                    scale1.PlayForward();
                }
                if (scale2 != null)
                {
                    scale2.ResetToBeginning();
                    scale2.PlayForward();
                }
                if (color1 != null)
                {
                    color1.ResetToBeginning();
                    color1.PlayForward();
                }
            }
            }
        }
    }
    /// <summary>
    /// 停止播放
    /// </summary>
    /// <param name="go"></param>
    void OnStopPlay(GameObject go)
    {
        if (ActionLogic.GetIns().IsConnect)
        {
            ActionLogic.GetIns().DoStopAction(go);
            if (curOperateAction != null)
            {
                curOperateAction.GetComponent<UISprite>().spriteName = "icon_control";
            }
           // playModeSelect.value = "None";
        }
        else
        {
           // HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
        }
    }

    /// <summary>
    /// setting 确定 写入数据
    /// </summary>
    void OnOkClick(GameObject go)
    {
        string str = "";
        IsEdit = false;
        ActionLogic.GetIns().ClearPlayList();
        for (int i = 0; i < UserActionTrans.childCount; i++)
        {
            str = "";
            if (UserActionTrans.GetChild(i).childCount != 0)
            {
                str = UserActionTrans.GetChild(i).GetComponentInChildren<UILabel>().text;
                ActionLogic.GetIns().AddToPlayList(UserActionTrans.GetChild(i).GetChild(0).gameObject);
            }
            ControlData.GetIns().curActionData.ActionList[i] = str;
            #region  
            #endregion
        }
        ControlData.GetIns().SaveToXml(ControlData.GetIns().curActionData);

     //   UpdateModelActions();
    }

    /// <summary>
    /// setting 取消
    /// </summary>
    /// <param name="go"></param>
    void OnCancelClick(GameObject go)
    {
        IsEdit = false;
        UpdateModelActions();
    }

    void OnBackClick(GameObject go)
    {
        RobotMgr.Instance.openActionList = false;
        BackBtn.SetActive(false);
        if (RobotManager.GetInst().GetCurrentRobot().Connected)
            OnStopPlay(null);  //返回之前停止运动
        Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
    }

    void OnConnectClick(GameObject go)
    {
        //ModelDetailsWindow.ConnectBluetooth();
    }

    /// <summary>
    /// 更新模型的动作表,start函数里面调用
    /// </summary>
    void UpdateModelActions()
    {
        ActionLogic.GetIns().ClearPlayList();

        if (ControlData.GetIns().curActionData == null)
            return;
        for (int k = 0; k < UserActionTrans.childCount; k++)
        {
            if (UserActionTrans.GetChild(k).childCount > 0)
                Destroy(UserActionTrans.GetChild(k).GetChild(0).gameObject);
        }
        bool flag = false;
        for (int j = 0; j < UserActionTrans.childCount; j++)     //用户的动作表 
        {
            string str = ControlData.GetIns().curActionData.ActionList[j];
            
            if (RobotManager.GetInst().GetCurrentRobot().GetActionsIdList().Contains(str))  //模型动作包含
            {
                GameObject item = Resources.Load("Prefabs/newActionItem") as GameObject;   //创建对应图标
                item = Instantiate(item) as GameObject;
                item.transform.SetParent(UserActionTrans.GetChild(j));
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = Vector3.zero;
                UIEventListener.Get(item).onClick = OnActionPlay;
                item.transform.GetComponentInChildren<UILabel>().text = str;
                ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(str);
                if (null == act)
                {
                    item.transform.GetChild(0).GetComponent<UISprite>().spriteName = "add";
                }
                else
                {
                    item.transform.GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                }
                item.transform.GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
                //BG enable false
                item.transform.GetChild(item.transform.childCount - 2).GetComponent<UISprite>().enabled = false;
                ActionLogic.GetIns().AddToPlayList(UserActionTrans.GetChild(j).gameObject);
            }
            else //不包含 被删除了
            {
                flag = true;
                ControlData.GetIns().curActionData.ActionList[j] = "";
                if (UserActionTrans.GetChild(j).childCount > 1)
                {
                    for (int i = 0; i < UserActionTrans.GetChild(j).childCount; i++)
                    {
                        Destroy(UserActionTrans.GetChild(j).GetChild(i).gameObject);
                        ActionLogic.GetIns().RemoveFromPlayList(UserActionTrans.GetChild(j).GetChild(i).gameObject);
                    }
                }
                //UserActionTrans.GetChild(j).GetComponentInChildren<UILabel>().text = "";
                //UserActionTrans.GetChild(j).GetChild(0).GetComponent<UISprite>().spriteName = "null";
            }
         //   UserActionTrans.GetChild(j).GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
        }
        if (flag)
        {
            ControlData.GetIns().SaveToXml(ControlData.GetIns().curActionData);
        }
    }
    void UpdateLeftActions()
    {
        for (int i = 0; i < OtherActionTrans.childCount; i++)
        {
            Destroy(OtherActionTrans.GetChild(i).gameObject);
        }
        List<string> totalActions = RobotManager.GetInst().GetCurrentRobot().GetActionsIdList();
        string tem = "";
        for (int i = 0; i < totalActions.Count; i++)
        {
            tem = totalActions[i];
            if (!ActionLogic.GetIns().IsIdExist(tem))
            {
                GameObject item = Resources.Load("Prefabs/newActionItem") as GameObject;   //创建对应图标
                item = Instantiate(item) as GameObject;
                item.transform.SetParent(OtherActionTrans);
                item.transform.localScale = Vector3.one;
                UIEventListener.Get(item).onClick = OnActionPlay;
                item.name = tem;
                ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(tem);
                if (null == act)
                {
                    item.transform.GetChild(0).GetComponent<UISprite>().spriteName = "add";
                    item.transform.GetComponentInChildren<UILabel>().text = string.Empty;
                }
                else
                {
                    item.transform.GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                    item.transform.GetComponentInChildren<UILabel>().text = act.Name;
                }
                item.transform.GetChild(0).GetComponent<UISprite>().MakePixelPerfect();

            }
        }
        OtherActionTrans.GetComponent<UIGrid>().repositionNow = true;
    }
    #endregion

    void GetPowerState(EventArg arg)
    {
        try
        {
            if (PlatformMgr.Instance.PowerData.isAdapter) //充电时
            {
                if (PlatformMgr.Instance.IsChargeProtected)  //充电保护 11 => 10
                {
                   // PublicPrompt.ShowChargePrompt(AutoSaveActions);
                   // Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
                    PublicPrompt.ShowChargePrompt(GoHome);
                }
            }
        }
        catch (System.Exception ex)
        { }
    }

    void GoHome()
    {
        Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
    }

    void PreEnterOtherScenes()
    { 
    
    }
}
