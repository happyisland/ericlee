//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Scene;
using Game.UI;
using System.IO;
using Game.Platform;
using Game.Event;
using System.Text;
using Game;

public class ModelDetailsWindow : MonoBehaviour {

    private UIEventListener.VoidDelegate clickSomeObj;
    private string CurModelName;
    private FunType curFun = FunType.none;
    public GameObject ConnectBtn;   //蓝牙图标
    private GameObject ActionsBtn;   //动作
    private GameObject PartsBtn;
    private GameObject ConsoleBtn;
    private GameObject PublishBtn;
    public GameObject SecondWinObj;   //侧栏
    public GameObject ReturnObj;
    public GameObject TakePhotoObj;
    public GameObject ActionToolBars;
    private UITexture PhotoTexture;
    private Texture defaultTexture;
    public UILabel robotName;
    public GameObject ResetSceneBtn;  //场景复位
    private string picFilePath;

    GameObject deletaBtn;
    GameObject playBtn;
    GameObject stopBtn;
    GameObject editBtn;
    GameObject oriGO;

    private List<GameObject> buildObjs;
    private List<GameObject> actionObjs;
    private List<GameObject> controllerObjs;

    private Vector3 oriObjPos;
    Robot mRobot;

    private static bool powerFlag = true;
    bool _isPowerShow;
    bool IsPowerShow   //电量显示
    {
        set
        {
            _isPowerShow = value;
            if (value)  //电量显示
            {
            }
            else // 不显示电量 
            {
                Debug.Log("hide power");
                if (ActionsBtn != null && ConsoleBtn != null)  //电量图标不显示的时候 表示与机器断开
                {
                    powerFlag = true;
                    //ActionsBtn.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
                    //ActionsBtn.GetComponent<BoxCollider>().enabled = true;
                    //ConsoleBtn.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
                    //ConsoleBtn.GetComponent<BoxCollider>().enabled = true;
                }
            }
        }
    }

    void Start()
    {
        if (RecordContactInfo.Instance.openType == "playerdata")
        {
            GameObject loadingSprite = GameObject.Find("MainUIRoot/Loading") as GameObject;
            if (loadingSprite != null)
                loadingSprite.SetActive(false);
        }
        if (StepManager.GetIns().OpenOrCloseGuide)
        {
            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().EditScenesToMain, true));
            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().BuildScenesToMain, true));
        }

        TakePhotoObj = GameObjectManage.Ins.TakePhotoObj;
        EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.Regist(EventID.SetDefaultOver, SetDefaultCallback);
        EventMgr.Inst.Regist(EventID.Set_Choice_Robot, SetRobot);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.Regist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.Regist(EventID.Change_Robot_Name_Back, GetNameChanged);
        curFun = FunType.none;
        if (RobotManager.GetInst().IsCreateRobotFlag)   // 创建的模型
        {
            CurModelName = RobotManager.GetInst().GetCreateRobot().Name;
        }
        else
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            if (null != mRobot)
            {
                CurModelName = mRobot.Name;
                AddMoveSecond();
            }
        }
        //if (Camera.main.GetComponent<MoveSecond>() != null) 
        //{
        //    StartCoroutine(RecordOrigState());
        //}
        Transform p = GameObject.Find("MainUIRoot/ModelDetails/Right/Guanfang").transform;
        Transform p1 = GameObject.Find("MainUIRoot/ModelDetails/Right/Zidingyi").transform;

       
        if (TakePhotoObj != null)
            TakePhotoObj.SetActive(false);
        if (RecordContactInfo.Instance.openType != "playerdata") //official模型
        {
            TakePhotoObj = null;
            p1.gameObject.SetActive(false);
            PartsBtn = p.GetChild(0).gameObject;
            ActionsBtn = p.GetChild(1).gameObject;
            ConsoleBtn = p.GetChild(2).gameObject;
            PublishBtn = null;
            if (robotName != null)
                robotName.gameObject.SetActive(false);
        }
        else  //modify模型 + create
        {
            ResetSceneBtn.SetActive(false);
            p.gameObject.SetActive(false);
            PartsBtn = null;
            ActionsBtn = p1.GetChild(0).gameObject;
            ConsoleBtn = p1.GetChild(1).gameObject;
            PublishBtn = p1.GetChild(2).gameObject;
            InitModelPic();
        }
        
        #region 绑定按钮关系
        if (ConnectBtn != null)
        {
            UIEventListener.Get(ConnectBtn).onClick = DoConnectBtn;
            ConnectBtn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("");
        }
        if (ActionsBtn != null)
        {
            UIEventListener.Get(ActionsBtn).onClick = DoActionsBtn;
            ActionsBtn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("动作");
        }
        if (PartsBtn != null)
        {
            UIEventListener.Get(PartsBtn).onClick = DoPartsBtn;
            PartsBtn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("搭建");
        }
        if (ConsoleBtn != null)
        {
            UIEventListener.Get(ConsoleBtn).onClick = DoCosoleBtn;
            ConsoleBtn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("控制器");
        }
        if (PublishBtn != null)
        {
            UIEventListener.Get(PublishBtn).onClick = DoPublishBtn;
            UILabel text = PublishBtn.GetComponentInChildren<UILabel>();
            if (text != null)
                text.text = LauguageTool.GetIns().GetText("发布");
        }
        if (ReturnObj != null)
        {
            UIEventListener.Get(ReturnObj).onClick = DoReturnBtn;
            UILabel text = ReturnObj.GetComponentInChildren<UILabel>();
            if (text != null)
                text.text = LauguageTool.GetIns().GetText("返回");
        }
        if (SecondWinObj != null)
        {
            if (SecondWinObj.transform.GetChild(1) == null)
                return;
            GameObject button = SecondWinObj.transform.GetChild(1).gameObject;
            UIEventListener.Get(button).onClick = clickSomeObj;  //button的click事件添加
        }
        if (ActionToolBars != null)
        {
            deletaBtn = ActionToolBars.transform.GetChild(0).gameObject;
            playBtn = ActionToolBars.transform.GetChild(1).gameObject;
            stopBtn = ActionToolBars.transform.GetChild(2).gameObject;
            editBtn = ActionToolBars.transform.GetChild(3).gameObject;
            UIEventListener.Get(deletaBtn).onClick = PopDeletaWin;//DoDeletaAction;
            UIEventListener.Get(playBtn).onClick = DoPlayAction;
            UIEventListener.Get(stopBtn).onClick = DoStopAction;
            UIEventListener.Get(editBtn).onClick = DoEditAction;
            ActionToolBars.SetActive(false);
        }
        if (robotName != null && robotName.gameObject.activeSelf)
        {
            if (RobotManager.GetInst().IsCreateRobotFlag)
            {
                robotName.text = RobotManager.GetInst().GetCreateRobot().ShowName;
            }
            else
            {
                robotName.text = mRobot.ShowName;
            }
            
        }
        if (TakePhotoObj != null)
        {
            UIEventListener.Get(TakePhotoObj.transform.GetChild(0).gameObject).onClick = CallCamera;  //调用相机
            PhotoTexture = TakePhotoObj.transform.GetChild(0).GetComponent<UITexture>();
        }
        if (ResetSceneBtn != null)
        {
            UIEventListener.Get(ResetSceneBtn).onClick = ResetScene;
        }
        #endregion
        OnBlueConnectResult(new EventArg(PlatformMgr.Instance.GetBluetoothState()));

        /*if (PlatformMgr.Instance.GetBluetoothState())
        {
            PowerMsg.OpenPower();
        }*/
		
		if (!StepManager.GetIns().OpenOrCloseGuide)
        {
            MoveSecond.Instance.HideGuangfangBtns(); //隐藏官方模型的按钮
        }
    }

    IEnumerator RecordOrigState()
    {
        yield return null;
        //if (Camera.main.GetComponent<MoveSecond>().oriGO != null)
        //{
        //    oriGO = Camera.main.GetComponent<MoveSecond>().oriGO;
        //    oriObjPos = oriGO.transform.localPosition;
        //}
    }

    void OnDestroy()
    {
        Debug.Log("hehheheh");
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.UnRegist(EventID.SetDefaultOver, SetDefaultCallback);
        EventMgr.Inst.UnRegist(EventID.Set_Choice_Robot, SetRobot);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.UnRegist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.UnRegist(EventID.Change_Robot_Name_Back, GetNameChanged);
        PowerMsg.HidePower();
        GameObjectManage.Ins.OnExit();
    }

    void ShowOfficialGUI(bool isOfficial)
    {
        if (isOfficial)
        { 
            
        }
        else
        { 
        
        }
    }

    /// <summary>
    /// 根据模型类型更新主界面
    /// </summary>
    void UpdateModelTypeInfo()
    {
        if (RecordContactInfo.Instance.openType != "playerdata") //官方模型
        {
            
        }
        else  //自定义模型
        { }
    }

    void AddMoveSecond()
    {
        //GameObject camera = GameObject.Find("Camera");
        //if (null != camera)
        //{
        //    MoveSecond ms = camera.GetComponent<MoveSecond>();
        //    if (null != ms)
        //    {
        //        GameObject.Destroy(ms);
        //    }
        //    camera.AddComponent<MoveSecond>();
        //}
    }
    #region  //function
    /// <summary>
    /// 初始化匹配模型跟照片
    /// </summary>
    void InitModelPic()
    {
        StartCoroutine(CaculatePicSize());
    }

    IEnumerator CaculatePicSize()
    {
        yield return null;
        PhotoTexture.enabled = false;
        if (!SecondWinObj.activeSelf) //展开
        {
            PhotoTexture.SetDimensions(860, 464);
            PhotoTexture.transform.localPosition = new Vector3(-590, 20, 0);
        }
        else  //隐藏
        {
            PhotoTexture.SetDimensions(562, 372);
            PhotoTexture.transform.localPosition = new Vector3(-734, 23, 0);
        }
        if (!string.IsNullOrEmpty(PlatformMgr.Instance.Pic_Path))
        {
            StartCoroutine(LoadTexture(PlatformMgr.Instance.Pic_Path));
        }
        else
        {
            int width_e;
            int height_e;
            Texture tempTexture;
            Texture errorPic = Resources.Load<Texture>("pic_error");
            if (errorPic != null)
            {
                tempTexture = errorPic;
                if (PhotoTexture.height * tempTexture.width > (tempTexture.height * PhotoTexture.width))   //长宽匹配
                {
                    width_e = PhotoTexture.width;
                    height_e = (int)(tempTexture.height * (PhotoTexture.width * 1.0f / tempTexture.width));
                }
                else
                {
                    height_e = PhotoTexture.height;
                    width_e = (int)(tempTexture.width * (PhotoTexture.height * 1.0f / tempTexture.height));
                }
                PhotoTexture.width = width_e;
                PhotoTexture.height = height_e;
                PhotoTexture.mainTexture = tempTexture;
            }
        }
        if (TakePhotoObj != null)
        {
            PhotoTexture.enabled = false;
            TakePhotoObj.SetActive(true);
        }
        StartCoroutine(WaitSometime());

    }
    IEnumerator WaitSometime()
    {
        yield return null;
        PhotoTexture.enabled = true;
    }
    /// <summary>
    /// 连接蓝牙入口
    /// </summary>
    /// <param name="obj"></param>
    void DoConnectBtn(GameObject obj)
    {
        ConnectBluetooth(); 
    }

    void SetRobot(EventArg arg)
    {
        try
        {
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            if (null != mRobot)
            {
                CurModelName = mRobot.Name;
                if (robotName != null)
                {
                    robotName.text = mRobot.ShowName;
                }
                AddMoveSecond();
            }
            /*if (RecordContactInfo.Instance.openType == "playerdata") //自定义模型
            {
                InitModelPic();
            }*/
        }
        catch (System.Exception ex)
        {
        	
        }
    }

    void ResetScene(GameObject obj)
    {
        Camera.main.GetComponent<CamRotateAroundCircle>().ResetOriState();
    }

    void PhotographBack(EventArg arg)
    {
        try
        {
            InitModelPic();
        }
        catch (System.Exception ex)
        {
        	
        }
    }

    void GetNameChanged(EventArg arg)
    {
        try
        {
            string name = (string)arg[0];
            robotName.text = name;
        }
        catch (System.Exception ex)
        {

        }
       
    }

    public static void ConnectBluetooth()
    {
        PublicPrompt.ShowClickBlueBtnMsg();
    }
    /// <summary>
    /// 动作入口
    /// </summary>
    /// <param name="obj"></param>
    void DoActionsBtn(GameObject obj)
    {
        if (null != mRobot)
        {
            if (curFun != FunType.Act) //不同之间的切换 一定是打开 
            {
                curFun = FunType.Act;
                IOSecondWindow(true);
            }
            else
                IOSecondWindow(!SecondWinObj.activeSelf);
            if (!SecondWinObj.activeSelf)
                return;
            ActionsBtn.transform.GetChild(2).GetComponent<UISprite>().enabled = true;

            UpdateAllActions(CurModelName);
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
        
    }

    /// <summary>
    /// 零件入口
    /// </summary>
    /// <param name="obj"></param>
    void DoPartsBtn(GameObject obj)
    {
        ExitMainWindow();
        GameObject oriGO = GameObject.Find("oriGO");

        if(oriGO !=null )
        {

            //MoveSecond.Instance.ResetParent();
            //MoveSecond.Instance.ResetDJDPPA();
            //MoveSecond.Instance.Showline();
            DontDestroyOnLoad(oriGO);
           
            /*RobotMotion rm=oriGO.GetComponent<RobotMotion>();
            Destroy(rm);*/
        }
        DoNewBuild(obj);
        /*    暂时没有零件 所以就直接进入搭建
        if (curFun != FunType.Part) //不同之间的切换 一定是打开 
        {
            curFun = FunType.Part;
            IOSecondWindow(true);
        }
        else
            IOSecondWindow(!SecondWinObj.activeSelf);
        if (!SecondWinObj.activeSelf)
            return;
        PartsBtn.transform.GetChild(2).GetComponent<UISprite>().enabled = true;

        UpdateData(null);
         * */
    }
    /// <summary>
    /// 控制器入口
    /// </summary>
    /// <param name="obj"></param>
    void DoCosoleBtn(GameObject obj)
    {
        if (null != mRobot)
        {
            GameObject oriGO = GameObject.Find("oriGO");

            if (oriGO != null)
            {
                //MoveSecond.Instance.ResetParent();
                //MoveSecond.Instance.ResetDJDPPA();
                
                //PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Default"));
                DontDestroyOnLoad(oriGO);
               // Destroy(oriGO);
            }

            if(RobotManager.GetInst().GetCurrentRobot().Connected)
                DoStopAction(obj);   //切换到控制器时 停止动作
            DoNewCosole(obj);
            ExitMainWindow();
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
        
        /*
        if (curFun != FunType.Consl) //不同之间的切换 一定是打开 
        {
            curFun = FunType.Consl;
            IOSecondWindow(true);
        }
        else
            IOSecondWindow(!SecondWinObj.activeSelf);
        if (!SecondWinObj.activeSelf)
            return;
        ConsoleBtn.transform.GetChild(2).GetComponent<UISprite>().enabled = true;

        UpdateData(null);
         * */
    }

    /// <summary>
    /// publish
    /// </summary>
    /// <param name="obj"></param>
    void DoPublishBtn(GameObject obj)
    {
    //    ExitMainWindow();
        if (null != mRobot)
        {
            PlatformMgr.Instance.PublishModel(RobotMgr.NameNoType(mRobot.Name));
           // ExitMainWindow();
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
    }

    void ExitMainWindow()
    {
        if (TakePhotoObj != null)
        {
            TakePhotoObj.SetActive(false);
        }
    }

    void PopDeletaWin(GameObject go)
    {
        GameObject DelateWin = Resources.Load("Prefabs/DelatePopWin") as GameObject;
        GameObject tempWin = Instantiate(DelateWin) as GameObject;
        tempWin.transform.SetParent(transform.parent);
        tempWin.transform.localScale = Vector3.one;
        tempWin.transform.localScale = Vector3.one;
        string tip = LauguageTool.GetIns().GetText("删除动作提示");
        tempWin.transform.GetChild(0).GetChild(0).GetComponentInChildren<UILabel>().text = tip + "\""+ ActionLogic.GetIns().GetCurActName()+"\"";
        UIEventListener.Get(tempWin.transform.GetChild(0).GetChild(1).gameObject).onClick = DelateActionCancel;
        UIEventListener.Get(tempWin.transform.GetChild(0).GetChild(2).gameObject).onClick = DelateActionConfrim;
        tempWin.transform.GetChild(0).GetChild(1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("取消删除");
        tempWin.transform.GetChild(0).GetChild(2).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("确定删除");
    }

    void DelateActionCancel(GameObject go)
    {
        Destroy(go.transform.parent.parent.gameObject);
    }

    void DelateActionConfrim(GameObject go)
    {
        Destroy(go.transform.parent.parent.gameObject);
        DoDeletaAction(null);
    }

    /// <summary>
    /// 删除动作
    /// </summary>
    /// <param name="go"></param>
    void DoDeletaAction(GameObject go)
    {
        ActionLogic.GetIns().DeletaAction();
        UpdateAllActions(CurModelName);
    }
    /// <summary>
    /// 播放动作
    /// </summary>
    /// <param name="go"></param>
    void DoPlayAction(GameObject go)
    {
#if UNITY_EDITOR
        ActionLogic.GetIns().OnPlayBtnClicked(go);
#else
        if (!ActionLogic.GetIns().IsConnect)
        {
            ConnectBtn.GetComponent<Animation>().Play();
        }
        ActionLogic.GetIns().OnPlayBtnClicked(go);
#endif
    }
    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="go"></param>
    void DoStopAction(GameObject go)
    {
        ActionLogic.GetIns().DoStopAction(go);
        playBtn.GetComponent<UISprite>().spriteName = "icon_play";
        if (!ActionLogic.GetIns().IsConnect)
        {
            ConnectBtn.GetComponent<Animation>().Play();
        }
    }
    /// <summary>
    /// 动作编辑
    /// </summary>
    /// <param name="go"></param>
    void DoEditAction(GameObject go)
    {
        GameObject oriGO = GameObject.Find("oriGO");

        if (oriGO != null)
        {
            //MoveSecond.Instance.ResetOriGOPos();
            //MoveSecond.Instance.ResetParent();
            //MoveSecond.Instance.ResetDJDPPA();
            
            DontDestroyOnLoad(oriGO);
        }
        string name = ActionLogic.GetIns().GetCurActName();
        if (!string.IsNullOrEmpty(name))
        {
            ActionEditScene.OpenActions(name);
        }

        if (RobotManager.GetInst().GetCurrentRobot().Connected)
            DoStopAction(go);   //切换到控制器时 停止动作
    }
    /// <summary>
    /// 新建动作
    /// </summary>
    /// <param name="obj"></param>
    void DoNewAction(GameObject obj)   //新建动作
    {
        if (null != mRobot)
        {
            // default
            //if (!RobotManager.GetInst().GetCurrentRobot().HaveDefualtActions() && !StepManager.GetIns().OpenOrCloseGuide) //默认为空时
            //{
            //    HUDTextTips.ShowTextTip("shezhimorendongzuo");
            //    return;
            //}

            GameObject oriGO = GameObject.Find("oriGO");

            if (oriGO != null)
            {
                //MoveSecond.Instance.ResetOriGOPos();
                //MoveSecond.Instance.ResetParent();
                //MoveSecond.Instance.ResetDJDPPA();

                DontDestroyOnLoad(oriGO);
            }
            ActionEditScene.CreateActions(string.Empty, string.Empty);

            if (RobotManager.GetInst().GetCurrentRobot().Connected)
                DoStopAction(obj);
            ExitMainWindow();
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
        
        /*Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {
            CreateActionsUI.ShowMsg(CreateActionsUI.ActionsMsgType.Actions_Msg_Create, robot.ID, null);
        }*/
    }
    /// <summary>
    /// 进入搭建
    /// </summary>
    /// <param name="obj"></param>
    void DoNewBuild(GameObject obj)   //新搭建
    {
        SceneMgr.EnterScene(SceneType.Assemble);
    }
    /// <summary>
    /// 新建控制器
    /// </summary>
    /// <param name="obj"></param>
    void DoNewCosole(GameObject obj)  //新建控制器
    {
      //  SceneMgr.e
        SceneMgr.EnterScene(SceneType.ActionPlay);
    }
    /// <summary>
    /// 返回
    /// </summary>
    /// <param name="obj"></param>
    void DoReturnBtn(GameObject obj)
    {
        GameObject origo = GameObject.Find("oriGO");
        if (origo != null)
        {
            Destroy(origo);
            //SceneMgrTest.Instance.LastScene = SceneType.StartScene;
        }
       

        curFun = FunType.none;
        //if()
        GameObjectManage.Ins.ClearData(); //进入社区时 清除图片数据
        PlatformMgr.Instance.Pic_Path = string.Empty;
        transform.gameObject.SetActive(false);
        if (ClientMain.Use_Third_App_Flag)
        {
            SceneMgr.EnterScene(SceneType.EmptyScene);
            Timer.Add(0.05f, 1, 1, PlatformMgr.Instance.BackThirdApp);
            //StartCoroutine(LoadingWait());
          //  PlatformMgr.Instance.BackThirdApp();
        }
        else
        {
            SceneMgr.EnterScene(SceneType.MenuScene);
        }
    }
    IEnumerator LoadingWait()
    {
        yield return null;
        yield return null;
        PlatformMgr.Instance.BackThirdApp();
    }
    /// <summary>
    /// 打开或者关闭二级窗口
    /// </summary>
    /// <param name="f"></param>
    void IOSecondWindow(bool f) //隐藏或显示2级窗口
    {
        if (SecondWinObj.activeSelf && f) //隐藏之前删除 清理残余数据 
        {
            Transform t = SecondWinObj.GetComponentInChildren<UIGrid>().transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Destroy(t.GetChild(i).gameObject);
            }
        }
        if(ActionsBtn != null)
            ActionsBtn.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
        if(PartsBtn != null)
            PartsBtn.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
        if(ConsoleBtn != null)
            ConsoleBtn.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
        SecondWinObj.SetActive(f);
        if(ActionToolBars != null)
            ActionToolBars.SetActive(false);
        if (!f) //处理  关闭
        {
            if (TakePhotoObj != null)  //用户
            {
                robotName.transform.localPosition += new Vector3(200, 0, 0);
                PhotoTexture.SetDimensions(860, 464);
                PhotoTexture.transform.localPosition = new Vector3(-590, 20, 0);
            }
            else  //屏幕3D模型适应  官方
            {
                oriGO = Camera.main.GetComponent<MoveSecond>().oriGO;
                if (oriGO != null)
                {
                    if (oriGO.transform.localPosition != oriObjPos)
                    {
                        Vector3 tvec = oriGO.transform.position;
                        Vector3 oriGoScreenPoint = Camera.main.WorldToScreenPoint(tvec);
                        tvec = Camera.main.ScreenToWorldPoint(new Vector3(oriGoScreenPoint.x + 130, oriGoScreenPoint.y, oriGoScreenPoint.z));
                        oriGO.transform.position = tvec;
                    }
                }
            }
        }
        else    //打开 根据当前打开的东西窗口配置按钮
        {
            if (TakePhotoObj == null)//默认的模型 为3D
            {
                oriGO = Camera.main.GetComponent<MoveSecond>().oriGO;
                if (oriGO != null)
                {
                    Vector3 tvec = oriGO.transform.position;
                    Vector3 oriGoScreenPoint = Camera.main.WorldToScreenPoint(tvec);  //
                    tvec = Camera.main.ScreenToWorldPoint(new Vector3(oriGoScreenPoint.x - 130, oriGoScreenPoint.y, oriGoScreenPoint.z));
                    oriGO.transform.position = tvec;
                }
            }
            else //自定义的 为图片
            {
                robotName.transform.localPosition -= new Vector3(200, 0, 0);
                PhotoTexture.SetDimensions(562, 372);
                PhotoTexture.transform.localPosition = new Vector3(-734, 23, 0);
            }
            if (transform.GetChild(2) == null)
                return;
            GameObject button = SecondWinObj.transform.GetChild(2).gameObject;
            button.GetComponentInChildren<BoxCollider>().enabled = true;
            switch (curFun)
            { 
                case FunType.Act:
                    if (!RobotManager.GetInst().GetCurrentRobot().HaveDefualtActions()) //默认为空时
                    {
                    //    button.GetComponentInChildren<UIButton>().state = UIButtonColor.State.Disabled;
                    }
                    clickSomeObj = DoNewAction;
                    button.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("新建动作");
                    break;
                case FunType.Part:
                    clickSomeObj = DoNewBuild;
                    button.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("搭建");
                    break;
                case FunType.Consl:
                    clickSomeObj = DoNewCosole;
                    button.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("新建控制器");
                    break;
                default:
                    clickSomeObj = null;
                    break;
            }
            // 这个东西只能在button为active时有效 不然会无效
            UIEventListener.Get(button).onClick = clickSomeObj;  //button的click事件添加
        }
        if (TakePhotoObj != null)   //自定义模型下图片处理
        {
            if (PhotoTexture.mainTexture != null)
            {
                int width_e;
                int height_e;
                Texture tempTexture = PhotoTexture.mainTexture;
                if (PhotoTexture.height * tempTexture.width > (tempTexture.height * PhotoTexture.width))   //长宽匹配
                {
                    width_e = PhotoTexture.width;
                    height_e = (int)(tempTexture.height * (PhotoTexture.width * 1.0f / tempTexture.width));
                }
                else
                {
                    height_e = PhotoTexture.height;
                    width_e = (int)(tempTexture.width * (PhotoTexture.height * 1.0f / tempTexture.height));
                }
                PhotoTexture.width = width_e;
                PhotoTexture.height = height_e;
            }
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
                IsPowerShow = true;
            }
            else
            {
                iconName = "disconnect";
                IsPowerShow = false;
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

    #region 相机功能
    GameObject cameraToolObj;
    /// <summary>
    /// 相机弹出框激活
    /// </summary>
    /// <param name="go"></param>
    void CallCamera(GameObject go)
    {
        Debug.Log("call camera");
        if (null != mRobot)
        {
            PlatformMgr.Instance.Photograph(RobotMgr.NameNoType(mRobot.Name), PlatformMgr.Instance.Pic_Path);
        }
        /*GameObject tempObj = Resources.Load("Prefabs/CameraToolSelect") as GameObject;
        cameraToolObj = Instantiate(tempObj) as GameObject;
        if (cameraToolObj != null)
        {
            cameraToolObj.transform.SetParent(TakePhotoObj.transform.parent);
            cameraToolObj.transform.localScale = Vector3.one;
            cameraToolObj.transform.localPosition = Vector3.one;
            UIEventListener.Get(cameraToolObj.transform.GetChild(0).gameObject).onClick = TakePhoto;  //拍照
            cameraToolObj.transform.GetChild(0).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("拍照");
            UIEventListener.Get(cameraToolObj.transform.GetChild(1).gameObject).onClick = OpenAlbum; //打开相册
            cameraToolObj.transform.GetChild(1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("打开相册");
            UIEventListener.Get(cameraToolObj.transform.GetChild(2).gameObject).onClick = CancelPhoto; //取消
            cameraToolObj.transform.GetChild(2).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("取消");
        }*/
    }

    IEnumerator LoadTexture(string name)
    {
#if UNITY_EDITOR
        string path = "file:///" + name;
#else
        string path = "file://" + name;
#endif
        if (!File.Exists(name))
        {
            Debuger.Log("file doesn't exist");
            yield break;
        }
        Resources.UnloadUnusedAssets();
        WWW www = new WWW(path);
        //while (!www.isDone)
        //{
        //}
        yield return www;
        int width_e;
        int height_e;
        Texture tempTexture;
        if (www.isDone)
        {
            if (null != www.error || www.texture == null)   // error
            {
                if (www.error != null)
                    Debuger.Log("load texture error = " + www.error);
                if (www.texture == null)
                    Debuger.Log("texture is null");
                Texture errorPic = Resources.Load<Texture>("pic_error");

                if (errorPic != null)
                {
                    tempTexture = errorPic;
                    if (PhotoTexture.height * tempTexture.width > (tempTexture.height * PhotoTexture.width))   //长宽匹配
                    {
                        width_e = PhotoTexture.width;
                        height_e = (int)(tempTexture.height * (PhotoTexture.width * 1.0f / tempTexture.width));
                    }
                    else
                    {
                        height_e = PhotoTexture.height;
                        width_e = (int)(tempTexture.width * (PhotoTexture.height * 1.0f / tempTexture.height));
                    }
                    PhotoTexture.width = width_e;
                    PhotoTexture.height = height_e;
                    PhotoTexture.mainTexture = tempTexture;
                }
                yield break;
            }    //success
            tempTexture = www.texture;
            if (PhotoTexture.height * tempTexture.width > (tempTexture.height * PhotoTexture.width))   //长宽匹配
            {
                width_e = PhotoTexture.width;
                height_e = (int)(tempTexture.height * (PhotoTexture.width * 1.0f / tempTexture.width));
            }
            else
            {
                height_e = PhotoTexture.height;
                width_e = (int)(tempTexture.width * (PhotoTexture.height * 1.0f / tempTexture.height));
            }
            try
            {
                PhotoTexture.width = width_e;
                PhotoTexture.height = height_e;
                PhotoTexture.mainTexture = www.texture;
            }
            catch (System.Exception ex)
            {
                Debuger.Log(ex.ToString());
            }
        }
        else
        {
            Debuger.Log("not done "+path);
        }
    }
#endregion
#endregion

#region //界面

    void DiaoDian(GameObject go)
    {
        PublicPrompt.ShowResetPrompt();
    }

    void SetDefaultCallback(EventArg arg)
    {
        HUDTextTips.ShowTextTip("shezhidefaultSuccess");
        //GameObject button = SecondWinObj.transform.GetChild(2).gameObject;
      //  button.GetComponentInChildren<UIButton>().state = UIButtonColor.State.Normal;
    //    button.GetComponentInChildren<BoxCollider>().enabled = true;
    }

    void GetPowerState(EventArg arg)
    {
        try
        {
            if (PlatformMgr.Instance.PowerData.isAdapter) //充电时
            {
                if (PlatformMgr.Instance.IsChargeProtected && powerFlag)  //充电保护 11 => 10
                {
                    powerFlag = false;
                    if (ActionsBtn != null && ConsoleBtn != null)
                    {
                        ActionsBtn.GetComponent<UIButton>().state = UIButtonColor.State.Disabled;
                        ActionsBtn.GetComponent<BoxCollider>().enabled = false;
                        ConsoleBtn.GetComponent<UIButton>().state = UIButtonColor.State.Disabled;
                        ConsoleBtn.GetComponent<BoxCollider>().enabled = false;
                    }
                }
                else if (!PlatformMgr.Instance.IsChargeProtected && !powerFlag) //关闭
                {
                    powerFlag = true;
                    if (ActionsBtn != null && ConsoleBtn != null)
                    {
                        ActionsBtn.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
                        ActionsBtn.GetComponent<BoxCollider>().enabled = true;
                        ConsoleBtn.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
                        ConsoleBtn.GetComponent<BoxCollider>().enabled = true;
                    }
                }
            }
            else //没有充电时
            {
                if (ActionsBtn != null && ConsoleBtn != null && !powerFlag)
                {
                    powerFlag = true;
                    ActionsBtn.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
                    ActionsBtn.GetComponent<BoxCollider>().enabled = true;
                    ConsoleBtn.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
                    ConsoleBtn.GetComponent<BoxCollider>().enabled = true;
                }
            }
            //界面读取电量

        }
        catch (System.Exception ex)
        { }
    }

    UISprite spriteBg = null;
    void OnActionItemClicked(GameObject go)
    {
        if (go != null && go.GetComponentInChildren<UILabel>().text == LauguageTool.GetIns().GetText("FuWei") && !RobotManager.GetInst().GetCurrentRobot().HaveDefualtActions())//如果点击的动作为default并且 default为空时 
        {
#if UNITY_EDITOR
          //  if (ActionLogic.GetIns().IsConnect)
          //  {
                DiaoDian(null);
                if (spriteBg == null)
                {

                    spriteBg = GameObject.Instantiate(go.transform.GetChild(1).GetComponent<UISprite>()) as UISprite;  //选中背景
                    spriteBg.spriteName = "nav";
                    spriteBg.SetDimensions(420, 145);
                }
                spriteBg.transform.SetParent(go.transform);
                spriteBg.transform.localScale = Vector3.one;
                spriteBg.transform.localPosition = new Vector3(82, 0, 0);
                spriteBg.depth = 2;
                spriteBg.enabled = true;
                ActionToolBars.SetActive(false);
#else
            if (ActionLogic.GetIns().IsConnect)
            {
                DiaoDian(null);
                if (spriteBg == null)
                {

                    spriteBg = GameObject.Instantiate(go.transform.GetChild(1).GetComponent<UISprite>()) as UISprite;  //选中背景
                    spriteBg.spriteName = "nav";
                    spriteBg.SetDimensions(420, 145);
                }
                spriteBg.transform.SetParent(go.transform);
                spriteBg.transform.localScale = Vector3.one;
                spriteBg.transform.localPosition = new Vector3(82, 0, 0);
                spriteBg.depth = 2;
                spriteBg.enabled = true;
                ActionToolBars.SetActive(false);
            }
            else
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
            }
#endif
                return;
        }
        if (go!=null && go.GetComponentInChildren<UILabel>().text == LauguageTool.GetIns().GetText("FuWei"))  //复位
        {
            go.GetComponentInChildren<UILabel>().text = PublicFunction.Default_Actions_Name;
        }
        ActionLogic.GetIns().DoSelectItem(go);   //如果是default action 不执行
        if (go != null && go.GetComponentInChildren<UILabel>().text == PublicFunction.Default_Actions_Name)  //复位
        {
            go.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("FuWei");
        }
        ActionToolBars.SetActive(false);
        if (go == null)
        {       
            return;
        }
        if (go.GetComponentInChildren<UILabel>().text == ActionLogic.GetIns().GetNowPlayingActionName())
        {
            playBtn.GetComponent<UISprite>().spriteName = "icon_stop";

        }
        else
        {
            playBtn.GetComponent<UISprite>().spriteName = "icon_play";
        }
        if (spriteBg == null)
        {
            spriteBg = GameObject.Instantiate(go.transform.GetChild(1).GetComponent<UISprite>()) as UISprite;  //选中背景
            spriteBg.spriteName = "nav";
            spriteBg.SetDimensions(420, 145);
        }
        spriteBg.transform.SetParent(go.transform);
        spriteBg.transform.localScale = Vector3.one;
        spriteBg.transform.localPosition = new Vector3(82, 0, 0);
        spriteBg.depth = 2;
        spriteBg.enabled = true;


        StartCoroutine(WaitOneFrame(go));
    }

    IEnumerator WaitOneFrame(GameObject go)
    {
        yield return new WaitForSeconds(0.05f);
        ActionToolBars.SetActive(true);
        if (StepManager.GetIns().OpenOrCloseGuide)
        {
            yield return null;
            EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().WaitSometime, true));
        }
        string teapName = go.GetComponentInChildren<UILabel>().text;
        if (teapName == LauguageTool.GetIns().GetText("FuWei")) //default动作不可删除 不可编辑 和官方动作不可以删除
        {
            deletaBtn.SetActive(false);
            UIEventListener.Get(editBtn).onClick = DiaoDian;
        }
        else if (RobotManager.GetInst().GetCurrentRobot().IsOfficialForName(go.GetComponentInChildren<UILabel>().text)) //官方动作不可删除 可编辑
        {
            deletaBtn.SetActive(false);
            UIEventListener.Get(editBtn).onClick = DoEditAction;
        }
        else    //其它的可删除 可编辑
        {
            deletaBtn.SetActive(true);
            UIEventListener.Get(editBtn).onClick = DoEditAction;
        }
    }

    /// <summary>
    /// 更新所有动作
    /// </summary>
    /// <param name="modelName"></param>
    void UpdateAllActions(string modelName)
    {
        if (null == mRobot)
        {
            return;
        }
        Transform t = SecondWinObj.GetComponentInChildren<UIGrid>().transform;
        for (int i = 0; i < t.childCount; i++)
        {
            Destroy(t.GetChild(i).gameObject);
        }
        DoNextFrameFunction = UpdateActionsData;
        StartCoroutine(DoAtNextFrame(UpdateActionsData));
    }
    /// <summary>
    /// 动作文件更新
    /// </summary>
    void UpdateActionsData()
    {
        Transform t = SecondWinObj.GetComponentInChildren<UIGrid>().transform;
        List<string> totalActions = mRobot.GetActionsNameList();
        if (totalActions.Contains(ModelDetailsTool.forbiddenStr))
        {
            ModelDetailsTool.SpecialFlag = !ModelDetailsTool.SpecialFlag;
            mRobot.DeleteActionsForName(ModelDetailsTool.forbiddenStr);
            totalActions = mRobot.GetActionsNameList();
        }
        #region default_action
        //if (!totalActions.Contains(PublicFunction.Default_Actions_Name))
        //{
        //    totalActions.Insert(0, PublicFunction.Default_Actions_Name);
        //}
        #endregion
        for (int i = 0; i < totalActions.Count; i++)   //创建结点
        {
            GameObject obj = GameObject.Instantiate(ActionLogic.GetIns().actionItem) as GameObject;
            obj.transform.SetParent(t);
            obj.transform.localScale = Vector3.one;
            UISprite sp = obj.GetComponent<UISprite>();
            if (null != sp)
            {
                sp.spriteName = mRobot.GetActionsIconForName(name);
                sp.MakePixelPerfect();
            }
        }
        for (int i = 0; i < totalActions.Count; i++)
        {
            string name = totalActions[i];
            t.GetChild(i).GetComponent<UISprite>().spriteName = mRobot.GetActionsIconForName(name); //通过actionName找到对应的icon
            if (name == PublicFunction.Default_Actions_Name)
            {
                name = LauguageTool.GetIns().GetText("FuWei");
            }
            t.GetChild(i).GetChild(0).GetComponent<UILabel>().text = name;
            //此功能毙掉 点击后弹出一个功能栏
            UIEventListener.Get(t.GetChild(i).gameObject).onClick = OnActionItemClicked;
        }
        OnActionItemClicked(null);
        if (t.childCount < 4)    //scrollview 有bug
        {
            t.GetComponent<UIScrollView>().enabled = false;
        }
        else
        {
            t.GetComponent<UIScrollView>().enabled = true;
        }
      //  t.GetComponent<UIScrollView>().ResetPosition();
        t.GetComponent<UIGrid>().repositionNow = true;
        StartCoroutine(ScrollViewUpdate(t.GetComponent<UIScrollView>()));
    }
    IEnumerator ScrollViewUpdate(UIScrollView t)
    {
        yield return null;
        t.ResetPosition();
      //  t.GetComponent<UIGrid>().repositionNow = true;
    }
    public delegate void DoT();
    public static DoT DoNextFrameFunction;
    public static IEnumerator DoAtNextFrame(DoT doNext)
    {
        yield return null;
        doNext();
    }
#endregion

    enum FunType
    { 
        Act, //actions
        Part, //parts
        Consl,  //console
        none
    }
}

public class ModelDetailsTool
{
    public static bool SpecialFlag = false;
    /// <summary>
    /// orgi类型至少含有orgiObj.getcomptent<uisprite>(） orgri.transtorm.getchild(0).getcomptent<uilabel>()
    /// </summary>
    /// <param name="p"></param>
    /// <param name="orgiObj"></param>
    /// <param name="dataList"></param>
    public static string forbiddenStr = "xiongsonglin";
    public static void BuildItems(Transform p, GameObject orgiObj, int n)
    {
        for (int i = 0; i < n; i++)
        {
            GameObject temp = GameObject.Instantiate(orgiObj) as GameObject;
            temp.transform.SetParent(p);
            temp.transform.localScale = Vector3.one;
          //  temp.transform.GetChild(0).GetComponent<UILabel>().text = dataList[i];
        }
        if (p.GetComponent<UIGrid>() != null)
            p.GetComponent<UIGrid>().repositionNow = true;
    }
}

