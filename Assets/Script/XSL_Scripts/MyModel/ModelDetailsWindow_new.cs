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

public class ModelDetailsWindow_new : MonoBehaviour
{
    bool IsOfficial; //是否是官方
    bool IsCameraState;
    bool IsFirstCamera;
    string RobotName;
    public Transform officialTrans;
    public Transform defaultTrans;
    private Transform Effect_btn;
    private Transform Controller_btn;
    private Transform Logic_btn;
    private Transform BT_button;
    public Transform Back_button;
    private Transform Reset_button;
    private Transform Power_button;
    private Transform Camera_button;
    private Transform Connect_button;
    private Transform Guide_button;
    private Transform ScrollActionItems;
    private Transform FirstCameraEnter;
    //private Transform Resets_button;
    //private Transform Saved_Button;
    //private Transform Stopped_Button;
    private UITexture PhotoTexture;
    private GameObject AddButton;
    private GameObject TakePhotoObj;
    private GameObject PlayAcionTool;
    public UILabel robotName;
    public Camera UICam;
    bool _isBackCommunity;
    bool isBackCommunity
    {
        get
        {
            return _isBackCommunity;

        }
        set
        {
            _isBackCommunity = value;
            if (value)
            {
                GameObjectManage.Ins.ClearData(); //进入社区时 清除图片数据
                PlatformMgr.Instance.Pic_Path = string.Empty;
                //  transform.gameObject.SetActive(false);
            }
        }
    }
    void StartHideLeftIcom()
    {
        StartCoroutine(HideLeftIcom());
    }
    IEnumerator HideLeftIcom()
    {
        Vector3 to = Vector3.zero;
        if (Back_button != null)
        {
            to = Back_button.localPosition;
            Back_button.localPosition = new Vector3(to.x - 160, to.y, to.z);
            Back_button.gameObject.SetActive(false);
        }
        yield return null;
        yield return new WaitForEndOfFrame();

        if (Back_button != null)
        {
            // BT_button.gameObject.SetActive(false);

            Back_button.localPosition = new Vector3(to.x - 160, to.y, to.z);
            Transform nullT = null;
            Back_button.GetComponent<UIWidget>().SetAnchor(nullT);
            Back_button.gameObject.SetActive(true);
            TweenPosition twe = null;
            if (Back_button.GetComponent<TweenPosition>() == null)
            {
                twe = Back_button.gameObject.AddComponent<TweenPosition>();
                twe.from = Back_button.localPosition;
                twe.to = to;
                twe.duration = 0.5f;
            }
            if (twe != null)
                twe.PlayForward();
        }
    }

    /// <summary>
    /// 返回
    /// </summary>
    /// <param name="go"></param>
    public void GoBack(GameObject go)
    {
        isBackCommunity = true;
        RobotMgr.Instance.GoToCommunity();

        //  GameObjectManage.Ins.ClearData(); //进入社区时 清除图片数据
        //   PlatformMgr.Instance.Pic_Path = string.Empty;
        //   transform.gameObject.SetActive(false);
        if (ClientMain.Use_Third_App_Flag)
        {
            SceneMgr.EnterScene(SceneType.EmptyScene);
            PlatformMgr.Instance.BackThirdApp();
        }
        else
        {
            SceneMgr.EnterScene(SceneType.MenuScene);
        }
    }
    //EventDelegate.Callback EnterOtherMode;
    void LogicProgramIN(GameObject go)
    {
        //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("暂不开放"));
        //return;
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedCodingButton);
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {
            if (IsProtected())
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("adpateProtected"));
                return;
            }
            //EnterScene(SceneType.testScene);
            SceneMgr.EnterScene(SceneType.EmptyScene);
            SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(robot);
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
    }
    void EnterScene(SceneType type, int index = 0) //index = 1 表示逻辑编程， 2表示发布
    {
        ShowRightBar(false);
        StartCoroutine(GoToScene(0.5f, type, index));
    }
    IEnumerator GoToScene(float time, SceneType type, int index)
    {
        yield return new WaitForSeconds(time);
        if (type != SceneType.testScene)
            SceneMgr.EnterScene(type);
        else if (index == 2)   //发布
            PlatformMgr.Instance.PublishModel(RobotMgr.NameNoType(RobotManager.GetInst().GetCurrentRobot().Name));
        else if (index == 1)  //逻辑编程
            SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(RobotManager.GetInst().GetCurrentRobot());
    }
    /// <summary>
    /// 点击蓝牙
    /// </summary>
    /// <param name="go"></param>
    public void BlueTooClick(GameObject go)
    {
        if (IsCameraState)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GetBluetoothCurrentState, "");
        }
        PublicPrompt.ShowClickBlueBtnMsg();
    }
    /// <summary>
    /// 复位视角
    /// </summary>
    /// <param name="go"></param>
    public void ResetClick(GameObject go)
    {
        if (null != go)
        {
            Camera.main.GetComponent<CamRotateAroundCircle>().MobClickOriginalPosition();
        }
        Camera.main.GetComponent<CamRotateAroundCircle>().ResetOriState();
    }
    /// <summary>
    /// 动态图纸
    /// </summary>
    /// <param name="go"></param>
    public void BuildClick(GameObject go)
    {
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedBuildModelButton);
        if (RobotManager.GetInst().GetCurrentRobot() == null)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
            return;
        }

        if (RobotMgr.Instance.hideGOs != null && RobotMgr.Instance.hideGOs.Count > 0)
        {
            for (int i = RobotMgr.Instance.hideGOs.Count - 1; i >= 0; i--)
            {
                RobotMgr.Instance.hideGOs[i].SetActive(true);
            }
        }

        GameObject oriGO = GameObject.Find("oriGO");
        if (oriGO != null)
        {
            DontDestroyOnLoad(oriGO);
        }
        SceneMgr.EnterScene(SceneType.Assemble);
    }
    /// <summary>
    /// 动作
    /// </summary>
    /// <param name="go"></param>
    private bool _showPlaylist;
    private bool ShowPlaylist
    {
        get
        {
            return _showPlaylist;
        }
        set
        {
            _showPlaylist = value;
            if (value)                          //动作表
            {
                if (TakePhotoObj.GetComponentInChildren<BoxCollider>() != null)
                    TakePhotoObj.GetComponentInChildren<BoxCollider>().enabled = false;

                if (Back_button != null)
                    UIEventListener.Get(Back_button.gameObject).onClick = BackEffect;
              //  BT_button.gameObject.SetActive(false);
                Power_button.gameObject.SetActive(false);
                robotName.gameObject.SetActive(false);

                ShowRightBar(false);
                /*if (Camera_button != null)
                {
                    Camera_button.gameObject.SetActive(true);
                    UIEventListener.Get(Camera_button.gameObject).onClick = CameraClick;
                }*/

                if (!flagex)
                {
                    ShowBottomBar(true, 0.5f);
                    
                }
                else
                {
                    flagex = false;
                    ShowBottomBar(true, 0.1f);
                    
                }

                //进入动作表时 插上电源了
                if (IsProtected())
                {
                    PublicPrompt.ShowChargePrompt(GoBack);
                }
            }
            else
            {
                if (TakePhotoObj.GetComponentInChildren<BoxCollider>() != null)
                    TakePhotoObj.GetComponentInChildren<BoxCollider>().enabled = true;

                if (Back_button != null)
                    UIEventListener.Get(Back_button.gameObject).onClick = GoBack;
                
                OnBlueConnectResult(new EventArg(PlatformMgr.Instance.GetBluetoothState())); //蓝牙
                robotName.gameObject.SetActive(IsOfficial);

                if (Camera_button != null)
                {
                    Camera_button.gameObject.SetActive(false);
                }
                ShowBottomBar(false);
                if (!flagex1)
                {
                    ShowRightBar(true, 0.5f);
                }
                else
                {
                    flagex1 = false;
                    ShowRightBar(true, 0.1f);
                }
            }
        }
    }
    public void CameraClick(GameObject go)
    {
        IsCameraState = true;

        if (AddButton != null)
        {
            AddButton.gameObject.SetActive(false);
        }

        ScrollActionItems = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList/scrollRect").transform;

        GameObject eeo = null;
        ScrollActionItems.GetComponent<UIPanel>().SetAnchor(eeo);
        //Vector4 rect1 = ScrollActionItems.GetComponent<UIPanel>().baseClipRegion;
        //rect1.x = rect1.x - 149;
        //ScrollActionItems.GetComponent<UIPanel>().baseClipRegion = rect1;

        Vector3 pos3 = ScrollActionItems.localPosition;
        pos3.x = pos3.x - 179;
        ScrollActionItems.localPosition = pos3;

        Vector3 pos1 = Reset_button.localPosition;

        GameObject emp = null;
        Connect_button = BT_button.parent;
        Connect_button.GetComponent<UIWidget>().SetAnchor(emp);
        Connect_button.localPosition = pos1;

        Reset_button.gameObject.SetActive(false);

        Back_button.gameObject.SetActive(false);
        Camera_button.gameObject.SetActive(false);
        GameObject modelObj = GameObject.Find("oriGO");
        PublicFunction.SetLayerRecursively(modelObj, LayerMask.NameToLayer("Default"));

        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GetCameraCurrentState, ""); 
    }
    public void GiveupVideoState(object go)
    {
        IsCameraState = false;

        PlayAcionTool = GameObject.Find("MainUIRoot_new/ModelDetails/Top/toolBars");
        if (PlayAcionTool != null && PlayAcionTool.activeSelf)
        {
            PlayAcionTool.SetActive(false);
        }

        Back_button.gameObject.SetActive(true);
        Camera_button.gameObject.SetActive(true);
        GameObject modelObj = GameObject.Find("oriGO");
        PublicFunction.SetLayerRecursively(modelObj, LayerMask.NameToLayer("Robot"));

        Vector3 pos3 = ScrollActionItems.localPosition;
        pos3.x = pos3.x + 179;
        ScrollActionItems.localPosition = pos3;

        ScrollActionItems.GetComponent<UIPanel>().SetAnchor(GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList"));

        Reset_button.gameObject.SetActive(true);

        Vector3 pos2 = Reset_button.localPosition;

        pos2.y = pos2.y + 118;
        Connect_button.localPosition = pos2;
        Connect_button.GetComponent<UIWidget>().SetAnchor(Reset_button);

        if (AddButton != null)
        {
            AddButton.gameObject.SetActive(true);
        }      
    }
    /// <summary>
    /// 退出蓝牙
    /// </summary>
    /// <param name="go"></param>
    public void ExitCameraBTConnect(EventArg arg)
    {
        if (IsCameraState)
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ExitBluetoothCurrentState, "");
    }

    public void EffectClick(GameObject go)
    {
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedActionListButton);
        if (IsProtected())
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("adpateProtected"));
            return;
        }
        ShowPlaylist = true;

        //StartCoroutine(DisplayCameraGuide(1.0f));
    }
    IEnumerator DisplayCameraGuide(float t)
    {
        yield return new WaitForSeconds(t);
        if (IsFirstCamera) //  读取文件 文件不存在需要开启指引pages
        {
            FirstCameraEnter.gameObject.SetActive(true);
            Guide_button = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter/ConfirmTips").transform;
            UIEventListener.Get(Guide_button.gameObject).onClick = EnterVideoPlay;

            UILabel CameraTips = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter/CameraTips").GetComponent<UILabel>();
            CameraTips.text = LauguageTool.GetIns().GetText("边玩边拍");
            UILabel GuideButtonTips = Guide_button.GetChild(0).GetComponent<UILabel>();
            GuideButtonTips.text = LauguageTool.GetIns().GetText("首次引导按钮");
            StreamWriter sw = System.IO.File.CreateText(GuidePagesConfig);
            sw.Write("{\"instruction1\":\"0\"}");
            sw.Close();
        }
        else
        {
            FirstCameraEnter.gameObject.SetActive(false);
        }
    }
    void BackEffect(GameObject go)
    {
        OnActionClick(null);
        ShowPlaylist = false;
        ResetClick(null);
    }
    public void EnterVideoPlay(GameObject go)
    {
        if (FirstCameraEnter != null && FirstCameraEnter.gameObject.activeSelf)
        {
            FirstCameraEnter.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 遥控器
    /// </summary>
    /// <param name="go"></param>
    public void ControlClick(GameObject go)
    {
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedControllerButton);
        if (RobotManager.GetInst().GetCurrentRobot() == null)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
            return;
        }
        if (IsProtected())
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("adpateProtected"));
            return;
        }
        GameObject oriGO = GameObject.Find("oriGO");
        if (oriGO != null)
        {
            //DoStopAction(oriGO);
            //MoveSecond.Instance.ResetDJDPPA();
            MoveSecond.Instance.ResetParent();
            MoveSecond.Instance.ResetDJDPPA();
            MoveSecond.Instance.RestGOPA();
            MoveSecond.Instance.ChangeParent();
            //PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Arrow"));
            DontDestroyOnLoad(oriGO);
        }
        SceneMgrTest.Instance.LastScene = SceneType.ActionPlay;

        SceneMgr.EnterScene(SceneType.ActionPlay);
        //添加模型背景
        GameObject obj = GameObject.Find("Camera");
        if (obj.GetComponent<MoveSecond>() != null)
            DestroyImmediate(obj.GetComponent<MoveSecond>());
        if (obj.GetComponent<CamRotateAroundCircle>() != null)
            DestroyImmediate(obj.GetComponent<CamRotateAroundCircle>());
    }
    /// <summary>
    /// 社区
    /// </summary>
    /// <param name="go"></param>
    public void CommunityClick(GameObject go)
    {
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            PlatformMgr.Instance.PublishModel(RobotMgr.NameNoType(RobotManager.GetInst().GetCurrentRobot().Name));
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
    }

    public void AddNewAction(GameObject go)
    {
        
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            SceneMgrTest.Instance.LastScene = SceneType.EditAction;

            GameObject oriGO = GameObject.Find("oriGO");
            if (oriGO != null)
            {
                //MoveSecond.Instance.ResetDJDPPA();
                MoveSecond.Instance.ResetParent();
                MoveSecond.Instance.ResetDJDPPA();
                //PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Default"));
                DontDestroyOnLoad(oriGO);
            }
            ActionEditScene.CreateActions(string.Empty, string.Empty);
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
            return;
        }
    }

    /// <summary>
    /// 底部显示或消失
    /// </summary>
    /// <param name="isShow"></param>
    void ShowBottomBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        StartCoroutine(HideOrShowBottomBar(isShow, time, call));
    }
    IEnumerator HideOrShowBottomBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        yield return null;
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        if (time > 0)
            yield return new WaitForSeconds(time);
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;
        Transform trans = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList").transform;
        if (trans != null)
        {
            Transform nullTran = null;
            trans.GetComponent<UIWidget>().SetAnchor(nullTran);
            int hh = trans.GetComponent<UIWidget>().height;
            MyAnimtionCurve cur1 = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            if (trans.GetComponent<TweenPosition>() == null)
            {
                trans.gameObject.AddComponent<TweenPosition>();
                trans.GetComponent<TweenPosition>().from = trans.localPosition;
                trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y + hh, trans.localPosition.z);
                trans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
            }
            tp = trans.GetComponent<TweenPosition>();
            tp.duration = 0.5f;

            TweenPosition tween = null;
            Transform moveTrans = null;
            if (IsOfficial)
            {
                if (Reset_button != null)
                    moveTrans = Reset_button;
            }
            else
            {
                if (BT_button != null)
                    moveTrans = BT_button.parent;
            }
            if (moveTrans != null)
            {
                moveTrans.GetComponent<UIWidget>().SetAnchor(nullTran);
                if (moveTrans.GetComponent<TweenPosition>() == null)
                {
                    moveTrans.gameObject.AddComponent<TweenPosition>();
                    moveTrans.GetComponent<TweenPosition>().from = moveTrans.localPosition;
                    moveTrans.GetComponent<TweenPosition>().to = new Vector3(moveTrans.localPosition.x, moveTrans.localPosition.y + hh - 27, moveTrans.localPosition.z);
                    moveTrans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
                }
                tween = moveTrans.GetComponent<TweenPosition>();
                tween.duration = 0.5f;
            }
            if (call != null)
            {
                tp.AddOnFinished(call);
            }
            if (isShow)
            {
                tp.PlayForward();
                if (tween != null)
                    tween.PlayForward();
            }
            else
            {
                tp.PlayReverse();
                if (tween != null)
                    tween.PlayReverse();
            }
        }

    }

    public void ShowRightBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        StartCoroutine(HideOrShowRightBar(isShow, time, call));  //采用携程，考虑到一开始描点需要时间
    }
    IEnumerator HideOrShowRightBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        if (time > 0)
            yield return new WaitForSeconds(time);
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;
        Transform trans = null;
        if (IsOfficial)
        {
            trans = officialTrans;
        }
        else
        {
            trans = defaultTrans;
        }
        Transform temp = null;
        trans.GetComponent<UIWidget>().SetAnchor(temp);
        from = trans.localPosition;
        if (trans.GetComponent<TweenPosition>() == null)
        {
            trans.gameObject.AddComponent<TweenPosition>();
            to = new Vector3(from.x - 160, from.y, from.z);
            MyAnimtionCurve myCurve = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            trans.GetComponent<TweenPosition>().animationCurve = myCurve.animCurve;
            trans.GetComponent<TweenPosition>().duration = 0.5f;
            trans.GetComponent<TweenPosition>().from = from;
            trans.GetComponent<TweenPosition>().to = to;
        }
        tp = trans.GetComponent<TweenPosition>();
        if (call != null)
        {
            tp.AddOnFinished(call);
        }
        if (isShow)
        {
            tp.PlayForward();
        }
        else
        {
            tp.PlayReverse();
        }

    }

    /// <summary>
    /// 播放动作
    /// </summary>
    /// <param name="go"></param>
    void DoPlayAction(GameObject go)
    {
        ActionLogic.GetIns().OnPlayBtnClicked(go);
        if (!PlatformMgr.Instance.GetBluetoothState()) //未连接的情况
        {
            Animation anim = BT_button.GetComponent<Animation>();
            anim.Play();
        }
    }
    IEnumerator tweenFinshed(TweenPosition t,float tt = 0.3f)
    {
        yield return new WaitForSeconds(tt);
        if (t != null)
            t.PlayReverse();
    }
    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="go"></param>
    void DoStopAction(GameObject go)
    {
        ActionLogic.GetIns().DoStopAction(go);
        bar.transform.GetChild(1).GetChild(0).GetComponent<UISprite>().spriteName = "icon_play";
    }
    /// <summary>
    /// 动作编辑
    /// </summary>
    /// <param name="go"></param>
    void DoEditAction(GameObject go)
    {
        GameObject oriGO = GameObject.Find("oriGO");
        SceneMgrTest.Instance.LastScene = SceneType.EditAction;
        if (oriGO != null)
        {
            
            //MoveSecond.Instance.ResetOriGOPos();
            MoveSecond.Instance.ResetParent();
            MoveSecond.Instance.ResetDJDPPA();

            DontDestroyOnLoad(oriGO);
        }
        string name = ActionLogic.GetIns().GetCurActName();
        if (!string.IsNullOrEmpty(name))
        {
            ActionEditScene.OpenActions(name);
        }
    }

    void PopDeletaWin(GameObject go)
    {
        PublicPrompt.ShowDelateWin(LauguageTool.GetIns().GetText("删除动作提示") + "\"" + ActionLogic.GetIns().GetCurActName() + "\"", DelateActionConfrim);
    }

    void DelateActionConfrim(GameObject go)
    {
        // Destroy(go.transform.parent.parent.gameObject);
        try
        {
            string btnname = go.name;
            if (btnname.Equals(PromptMsg.RightBtnName))
            {
                DoDeletaAction(null);
                OnActionClick(null);
            }
        }
        catch
        { }
    }

    /// <summary>
    /// 删除动作
    /// </summary>
    /// <param name="go"></param>
    void DoDeletaAction(GameObject go)
    {
        ActionLogic.GetIns().DeletaAction();
        OnActionClick(null);
        UpdateActionsData();

        //    StartCoroutine(WaitOneFRAME());
    }

    IEnumerator WaitOneFRAME()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        UpdateActionsData();
    }

    IEnumerator WaitFrameEND(Transform t)
    {
        if (RobotManager.GetInst().GetCurrentRobot() == null)
            yield break;
        yield return new WaitForEndOfFrame();
        List<string> totalActions = RobotManager.GetInst().GetCurrentRobot().GetActionsNameList();
        for (int i = 0; i < totalActions.Count; i++)   //创建结点
        {
            GameObject tobj = Resources.Load("Prefabs/newActionItem1") as GameObject;
            GameObject obj = GameObject.Instantiate(tobj) as GameObject;
            obj.transform.SetParent(t);
            obj.transform.localScale = Vector3.one;
            UISprite sp = obj.transform.GetChild(0).GetComponent<UISprite>();
            if (null != sp)
            {
                ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(name);
                if (null == act)
                {
                    sp.spriteName = "add";
                }
                else
                {
                    sp.spriteName = act.IconName;
                }
                sp.MakePixelPerfect();
            }
        }
        for (int i = 0; i < totalActions.Count; i++)
        {
            string name = totalActions[i];
            ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(name);
            if (null == act)
            {
                t.GetChild(i).GetChild(0).GetComponent<UISprite>().spriteName = "add";
            }
            else
            {
                t.GetChild(i).GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
            }
            t.GetChild(i).GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
            if (name == PublicFunction.Default_Actions_Name)
            {
                name = LauguageTool.GetIns().GetText("FuWei");
            }
            t.GetChild(i).GetComponentInChildren<UILabel>().text = name;
            UIEventListener.Get(t.GetChild(i).gameObject).onClick = OnActionClick;
        }
        t.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 动作文件更新
    /// </summary>
    void UpdateActionsData()
    {
        Transform t = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList/scrollRect").transform;
        for (int i = 0; i < t.childCount; i++)
        {
            Destroy(t.GetChild(i).gameObject);
        }
        StartCoroutine(WaitFrameEND(t));
    }

    void OnActionClick(GameObject go)
    {
        if (go == null)
        {
            ActionLogic.GetIns().DoSelectItem(null);
            bar.SetActive(false);
            return;
        }
        ActionLogic.GetIns().DoSelectItem(go);
        if (RobotManager.GetInst().GetCurrentRobot().IsOfficialForName(go.GetComponentInChildren<UILabel>().text)) //官方动作不可删除 可编辑
        {
            bar.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            bar.transform.GetChild(0).gameObject.SetActive(true);
        }
        StartCoroutine(WaitAMoment());
        if (go.GetComponentInChildren<UILabel>().text == ActionLogic.GetIns().GetNowPlayingActionName())
        {
            bar.transform.GetChild(1).GetChild(0).GetComponent<UISprite>().spriteName = "icon_stop";
        }
        else
        {
            bar.transform.GetChild(1).GetChild(0).GetComponent<UISprite>().spriteName = "icon_play";
        }

    }
    GameObject bar;
    IEnumerator WaitAMoment()
    {
        if (bar == null)
            yield break;
        bar.SetActive(false);
        yield return new WaitForSeconds(0.03f);
        bar.SetActive(true);
        UIEventListener.Get(bar.transform.GetChild(0).gameObject).onClick = PopDeletaWin;
        UIEventListener.Get(bar.transform.GetChild(1).gameObject).onClick = DoPlayAction;
        UIEventListener.Get(bar.transform.GetChild(2).gameObject).onClick = DoStopAction;
        if (IsCameraState)
            bar.transform.GetChild(3).gameObject.SetActive(false);
        else
        {
            bar.transform.GetChild(3).gameObject.SetActive(true);
            UIEventListener.Get(bar.transform.GetChild(3).gameObject).onClick = DoEditAction;
        }
    }

    public static ModelDetailsWindow_new Ins;

    void Awake()
    {
        Ins = this;
        GuidePagesConfig = Application.persistentDataPath + "/instruction1";
    }
    void Start()
    {
        flagex = false;
        flagex1 = true;
        IsCameraState = false;
        EventMgr.Inst.Regist(EventID.UI_MainRightbar_hide, RightBarCallback);
        EventMgr.Inst.Regist(EventID.Set_Choice_Robot, SetRobot);
        EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.Regist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.Regist(EventID.Change_Robot_Name_Back, GetNameChanged);
        EventMgr.Inst.Regist(EventID.Exit_Blue_Connect, ExitCameraBTConnect);
        PlatformMgr.Instance.RegesiterCallUnityDelegate(CallUnityFuncID.GiveupVideoState, GiveupVideoState);
        isBackCommunity = false; // unity or community flag
        if (RecordContactInfo.Instance.openType != "playerdata") //official模型
            IsOfficial = true;
        else  //modify模型 + create
            IsOfficial = false;

        if (!System.IO.File.Exists(GuidePagesConfig)) //  读取文件 文件不存在需要开启指引pages
            IsFirstCamera = true;
        else
            IsFirstCamera = false;

        Transform offGrid = GameObject.Find("MainUIRoot_new/ModelDetails/BottomLeft/grid_Official").transform;
        Transform defaultGrid = GameObject.Find("MainUIRoot_new/ModelDetails/BottomLeft/grid_Default").transform;
        TakePhotoObj = GameObjectManage.Ins.TakePhotoObj;
        Camera_button = GameObject.Find("MainUIRoot_new/ModelDetails/Top/Camera").transform;
        Camera_button.gameObject.SetActive(false);

        FirstCameraEnter = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter").transform;
        FirstCameraEnter.gameObject.SetActive(false);

        if (IsOfficial)   //官方
        {
            officialTrans.gameObject.SetActive(true);
            defaultTrans.gameObject.SetActive(false);
            UIEventListener.Get(officialTrans.GetChild(0).gameObject).onClick = BuildClick;
            officialTrans.GetChild(0).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("搭建");
            Effect_btn = officialTrans.GetChild(1);
            UIEventListener.Get(Effect_btn.gameObject).onClick = EffectClick;
            Effect_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("动作");
            Logic_btn = officialTrans.GetChild(2);
            Controller_btn = officialTrans.GetChild(3);
            UIEventListener.Get(Controller_btn.gameObject).onClick = ControlClick;
            officialTrans.GetChild(3).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("控制器");
            defaultGrid.gameObject.SetActive(false);
            offGrid.gameObject.SetActive(true);
            BT_button = offGrid.GetChild(1).GetChild(0);    //BT
            Power_button = offGrid.GetChild(0);  //power
            Reset_button = offGrid.GetChild(2);
            //Resets_button = offGrid.GetChild(2);

            //Connect_button.gameObject.SetActive(true);
            
            if (robotName != null)
            {
                robotName.text = RobotManager.GetInst().GetCurrentRobot().ShowName;
            }
            TakePhotoObj.SetActive(false);
        }
        else   //自定义
        {
            GameObject ttem = GameObject.Find("MainUIRoot_new/GameObject");  //模型加载完后才能继续操作
            if (ttem != null && ttem.GetComponent<BoxCollider>() != null)
            {
                ttem.GetComponent<BoxCollider>().enabled = false;
            }
            //if (RobotManager.GetInst().IsCreateRobotFlag)   // 创建的模型
            //{
            //    RobotManager.GetInst().GetCurrentRobot();
            //    //CurModelName = RobotManager.GetInst().GetCreateRobot().Name;
            //}
            // TakePhotoObj = GameObject.Find("DontDestroyNode/UIRoot/MainPic/Center/takePhoto");
            //  TakePhotoObj.SetActive(true);
            GameObjectManage.Ins.OnShow();
            officialTrans.gameObject.SetActive(false);
            defaultTrans.gameObject.SetActive(true);
            Effect_btn = defaultTrans.GetChild(0);
            UIEventListener.Get(Effect_btn.gameObject).onClick = EffectClick;
            Effect_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("动作");
            Logic_btn = defaultTrans.GetChild(1);
            Controller_btn = defaultTrans.GetChild(2);
            UIEventListener.Get(Controller_btn.gameObject).onClick = ControlClick;
            Controller_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("控制器");
            UIEventListener.Get(defaultTrans.GetChild(3).gameObject).onClick = CommunityClick;
            defaultTrans.GetChild(3).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("发布");
            defaultGrid.gameObject.SetActive(true);
            offGrid.gameObject.SetActive(false);
            BT_button = defaultGrid.GetChild(1).GetChild(0);    //BT
            Power_button = defaultGrid.GetChild(0);  //power

            if (robotName != null)
                robotName.gameObject.SetActive(false);

            if (TakePhotoObj != null)
            {
                UIEventListener.Get(TakePhotoObj.transform.GetChild(0).gameObject).onClick = CallCamera;  //调用相机
                PhotoTexture = TakePhotoObj.transform.GetChild(0).GetComponent<UITexture>();

                if (PhotoTexture != null)
                {
                    if (PhotoTexture.mainTexture == null)
                    {
                        InitModelPic();
                    }
                }
            }
          //  StartCoroutine(DoAtFrameEnd());
            // ShowPlaylist = false;
        }
        if (BT_button != null)
            UIEventListener.Get(BT_button.gameObject).onClick = BlueTooClick;
        if (Reset_button != null)
            UIEventListener.Get(Reset_button.gameObject).onClick = ResetClick;
        if (Logic_btn != null)
        {
            UIEventListener.Get(Logic_btn.gameObject).onClick = LogicProgramIN;
            Logic_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("逻辑编程");
        }
        AddButton = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList/Button");
        if (AddButton != null)
        {
            UIEventListener.Get(AddButton).onClick = AddNewAction;
        }

        UpdateActionsData();
        bar = GameObject.Find("MainUIRoot_new/ModelDetails/Top/toolBars");
        bar.SetActive(false);
        OnBlueConnectResult(new EventArg(PlatformMgr.Instance.GetBluetoothState())); //蓝牙
        if (PlatformMgr.Instance.GetBluetoothState()) //蓝牙连接时 发送电量信息
        {
            EventMgr.Inst.Fire(EventID.Read_Power_Msg_Ack);
        }

        Transform trans = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList").transform;
        if (trans.GetChild(trans.childCount - 1).GetComponentInChildren<UILabel>() != null)
            trans.GetChild(trans.childCount - 1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("新建动作");

        if (UICam != null) //切换场景时 画面闪烁的问题
        {
            StartCoroutine(ShowLeftFrame());
        }

        if (!System.IO.File.Exists(GuidePagesConfig) && IsOfficial) //  读取文件 文件不存在需要开启指引pages
        {
            // if (IsOfficial)
            // {
            MoveSecond.Instance.OnModelLoadOver = OnModelLoadOver; //模型加载完毕 后显示引导页
            // }
            //else
            //{
            //    OnModelLoadOver();
            //}
            StreamWriter sw = System.IO.File.CreateText(GuidePagesConfig);
            sw.Write("{\"instruction1\":\"0\"}");
            sw.Close();
        }
        else
        {
            FirstGuidePage.GetIns().OnClosePage();
        }
    }
    IEnumerator ShowLeftFrame()
    {
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        if (UICam != null)
            UICam.enabled = true;
    }

    IEnumerator DoAtFrameEnd()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        ShowPlaylist = false;
    }

    public void PreEnterOtherScenes(EventDelegate.Callback call = null)
    {
        if (ShowPlaylist)// 动作表页面
        {
            ShowBottomBar(false, 0, call);
        }
        else  //主页
        {
            ShowRightBar(false, 0, call);
        }
        if (IsOfficial)
        {


        }
        else
        {
            GameObjectManage.Ins.OnExit();
        }
    }

    void OnDestroy()
    {
        DoStopAction(null);
        Ins = null;
        //GameObject oriGO = GameObject.Find("oriGO");
        //if (oriGO != null && !isBackCommunity)
        //{
        //    DontDestroyOnLoad(oriGO);

        //    /*RobotMotion rm=oriGO.GetComponent<RobotMotion>();
        //    Destroy(rm);*/
        //}
        if (isBackCommunity) //社区
        {
            GameObjectManage.Ins.ClearData();
        }
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.UnRegist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.UnRegist(EventID.Set_Choice_Robot, SetRobot);
        EventMgr.Inst.UnRegist(EventID.Change_Robot_Name_Back, GetNameChanged);
        EventMgr.Inst.UnRegist(EventID.UI_MainRightbar_hide, RightBarCallback);
        EventMgr.Inst.UnRegist(EventID.Exit_Blue_Connect, ExitCameraBTConnect);
        //  GameObjectManage.Ins.OnExit();
    }

    #region other
    /// <summary>
    /// 蓝牙连接回调
    /// </summary>
    /// <param name="arg"></param>
    void OnBlueConnectResult(EventArg arg)
    {
        try
        {
            if (RobotManager.GetInst().GetCurrentRobot() == null)
            {
                if (null != BT_button)
                {
                    UISprite sprite = BT_button.GetChild(1).GetComponent<UISprite>();
                    if (null != sprite)
                    {
                        sprite.spriteName = "disconnect";
                        sprite.SetDimensions(50, 50);
                        // sprite.MakePixelPerfect();
                    }
                    if (Power_button != null)
                        Power_button.gameObject.SetActive(false);
                }
                return;
            }

            bool flag = (bool)arg[0];
            string iconName;
            if (flag)
            {
                iconName = "connect";
                if (Power_button != null)
                {
                    Power_button.gameObject.SetActive(true);
                    firstAdapter = false; //断开蓝牙，表示充电结束
                    GetPowerState(null);
                    /*Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "Shape";
                    Power_button.GetChild(1).GetComponent<UISprite>().enabled = true;
                    Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;*/
                    //   EventMgr.Inst.Fire(EventID.Read_Power_Msg_Ack);
                }
            }
            else
            {
                iconName = "disconnect";
                if (Power_button != null)
                {
                    Power_button.gameObject.SetActive(false);
                }
            }
            if (null != BT_button)
            {
                UISprite sprite = BT_button.GetChild(1).GetComponent<UISprite>();
                if (null != sprite)
                {
                    sprite.spriteName = iconName;
                    sprite.SetDimensions(50, 50);
                    //sprite.MakePixelPerfect();
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
    /// 初始化匹配模型跟照片
    /// </summary>
    void InitModelPic()
    {
        StartCoroutine(CaculatePicSize());
    }

    IEnumerator CaculatePicSize()
    {
        yield return null;
        if (PhotoTexture == null)
            yield break;
        PhotoTexture.enabled = false;
        if (!string.IsNullOrEmpty(PlatformMgr.Instance.Pic_Path))
        {
            StartCoroutine(LoadTexture(PlatformMgr.Instance.Pic_Path));
        }
        else
        {
            Texture tempTexture;
            Texture errorPic = Resources.Load<Texture>("pic_error");
            if (errorPic != null)
            {
                tempTexture = errorPic;

                try
                {
                    PhotoTexture.width = tempTexture.width;
                    PhotoTexture.height = tempTexture.height;
                    PhotoTexture.mainTexture = tempTexture;
                }
                catch (System.Exception ex)
                {
                    Debuger.Log(ex.ToString());
                }
            }
        }
        if (TakePhotoObj != null)
        {
            PhotoTexture.enabled = false;
            GameObjectManage.Ins.OnShow();
            //TakePhotoObj.SetActive(true);
        }
        StartCoroutine(WaitSometime());

    }
    IEnumerator WaitSometime()
    {
        yield return null;
        PhotoTexture.enabled = true;
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
            //加载错误图片
            Texture errorPic = Resources.Load<Texture>("pic_error");

            if (errorPic != null)
            {
                try
                {
                    PhotoTexture.width = errorPic.width;
                    PhotoTexture.height = errorPic.height;
                    PhotoTexture.mainTexture = errorPic;
                }
                catch (System.Exception ex)
                {
                    Debuger.Log(ex.ToString());
                }
            }
            yield break;
        }
        Resources.UnloadUnusedAssets();
        WWW www = new WWW(path);
        //while (!www.isDone)
        //{
        //}
        yield return www;
        yield return null;  //拍完照 显示变形
        yield return new WaitForEndOfFrame();
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
                   // Vector2 tVec = new Vector2();
                    tempTexture = errorPic;
                  //  tVec = GetRightsize(tempTexture.width, tempTexture.height);
                    try
                    {
                        /*Transform tnull = null;
                        PhotoTexture.GetComponent<UIWidget>().SetAnchor(tnull);*/
                        PhotoTexture.width = tempTexture.width;
                        PhotoTexture.height = tempTexture.height;
                        PhotoTexture.mainTexture = tempTexture;
                    }
                    catch (System.Exception ex)
                    {
                        Debuger.Log(ex.ToString());
                    }
                }
                yield break;
            }    //success
            tempTexture = www.texture;
            Vector2 ttVec = new Vector2();
            ttVec = GetRightsize(tempTexture.width, tempTexture.height);
            try
            {
                /*Transform tnull = null;
                PhotoTexture.GetComponent<UIWidget>().SetAnchor(tnull);*/
                PhotoTexture.width = (int)ttVec.x;
                PhotoTexture.height = (int)ttVec.y;
                PhotoTexture.mainTexture = tempTexture;
            }
            catch (System.Exception ex)
            {
                Debuger.Log(ex.ToString());
            }
        }
        else
        {
            Debuger.Log("not done " + path);
        }
    }

    Vector2 GetRightsize(int w, int h)
    {
        Vector2 t;
        float screenWidth = PublicFunction.GetWidth();
        float screenHeight = PublicFunction.GetHeight();

        int height = (int)(screenHeight * w / screenWidth);
        if (height >= h)
        {
            t.y = screenHeight;
            t.x = Mathf.CeilToInt(screenHeight * w / h);
        }
        else
        {
            t.y = Mathf.CeilToInt(screenWidth * h / w);
            t.x = screenWidth;
        }
        return t;
        /*UIRoot root = GameObject.FindObjectOfType<UIRoot>();
        Vector2 temp = Vector2.zero;
        if (root != null)
        {
            float s = (float)root.activeHeight / Screen.height;
            int height = Mathf.CeilToInt(Screen.height * s);
            int width = Mathf.CeilToInt(Screen.width * s);
            temp.x = width;
            temp.y = height;
        }
        
        Vector2 t = new Vector2();
        t = Vector2.zero;

        if (w * temp.y > h * temp.x)  //chang
        {
            t.y = (int)temp.y;
            t.x = (int)(h * temp.y / h);
        }
        else   //kuan
        {
            t.x = (int)temp.x;
            t.y = (int)(h * temp.x / w);
        }
        return t;*/
    }

    /// <summary>
    /// 相机弹出框激活
    /// </summary>
    /// <param name="go"></param>
    void CallCamera(GameObject go)
    {
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            PlatformMgr.Instance.Photograph(RobotMgr.NameNoType(RobotManager.GetInst().GetCurrentRobot().Name), PlatformMgr.Instance.Pic_Path);
        }
    }

    void PhotographBack(EventArg arg)
    {
        try
        {
            StartCoroutine(delayExcute(InitModelPic));
            //InitModelPic();
        }
        catch (System.Exception ex)
        {

        }
    }

    IEnumerator delayExcute(EventDelegate.Callback voidDel)
    {
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        voidDel();
    }

    private bool flagex1; //从其它场景直接到默认场景
    private bool flagex;  //从其它场景直接到actionlist
    void RightBarCallback(EventArg arg)
    {
        try
        {
            if ((bool)arg[0]) //show left or botton
            {
                if (RobotMgr.Instance.openActionList == true)
                {
                    RobotMgr.Instance.openActionList = false;
                    flagex = true;
                    ShowPlaylist = true;

                    if (Back_button != null)
                        UIEventListener.Get(Back_button.gameObject).onClick = BackEffect;
                }
                else
                {
                    ShowPlaylist = false;
                    flagex = false;
                    flagex1 = true;
                    // ShowRightBar((bool)arg[0]);
                    if (Back_button != null)
                        UIEventListener.Get(Back_button.gameObject).onClick = GoBack;
                }
            }
        }
        catch (System.Exception ex)
        { }
    }

    /// <summary>
    /// 电量信息回调
    /// </summary>
    /// <param name="arg"></param>
    //static private bool powerFlag = true;  //静态 
    private bool firstAdapter = false;
    void GetPowerState(EventArg arg)
    {
        try
        {
            if (PlatformMgr.Instance.PowerData.isAdapter) //插上适配器
            {
                if (!firstAdapter)  //第一次插入适配器时
                {
                    firstAdapter = true;
                    if (Power_button != null) //充电状态
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "Shape";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = true;
                    }

                    if (PlatformMgr.Instance.IsChargeProtected) //充电保护
                    {
                        //充电保护的情况下在动作表界面插上电源
                        if (ShowPlaylist)// && IsProtected())
                        {
                            PublicPrompt.ShowChargePrompt(GoBack);
                        }
                    }
                    else
                    {

                    }
                }
                if (PlatformMgr.Instance.PowerData.isChargingFinished) //充满
                {
                    if (Power_button != null)
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "charging";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                        //Power_button.GetChild(1).GetComponent<UISprite>().fillAmount = PlatformMgr.Instance.PowerData.percentage * 0.01f;
                    }
                }
            }
            else  //拔下适配器  电量实时反馈
            {
                if (firstAdapter) //拔下电量的那一刻
                {
                    if (Power_button != null)
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "Shape";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = true;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                    }
                }
                firstAdapter = false;

                if (Power_button != null)
                {
                    if (PlatformMgr.Instance.PowerData.percentage > 20) //正常电量
                    {
                        Power_button.GetChild(1).GetComponent<UISprite>().fillAmount = PlatformMgr.Instance.PowerData.percentage * 0.01f;
                    }
                    else  //低电量
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "Battery_no";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                    }
                }
            }
        }
        catch
        { }
    }

    bool IsProtected()
    {
        return PlatformMgr.Instance.PowerData.isAdapter && PlatformMgr.Instance.IsChargeProtected;
    }

    void GoBack()
    {
        BackEffect(null);
    }

    void SetRobot(EventArg arg)
    {
        try
        {
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

    public string GuidePagesConfig;
    void OnModelLoadOver()
    {
        Transform centerTrans = GameObject.Find("MainUIRoot_new/ModelDetails/Center").transform;
        if (centerTrans != null)
        {
            FirstGuidePage.GetIns().Show(0.5f);
             //FirstGuidePage.LoadGuidePage("Prefabs/FirstGuidePage", centerTrans);   
            //UIRoot root = GameObject.FindObjectOfType<UIRoot>();
            //Vector2 temp = Vector2.zero;
            //if (root != null)
            //{
            //    float s = (float)root.activeHeight / Screen.height;
            //    int height = Mathf.CeilToInt(Screen.height * s);
            //    GameObject fgp = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstGuidePage");
            //    GameObject fgpPad = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstGuidePage_pad");

            //    if (height >= 1000) //ipad
            //    {
            //        if (fgpPad != null)
            //        {
            //            FirstGuidePage.LoadGuidePage(fgpPad, centerTrans);
            //            //fgpPa.GetComponent<FirstGuidePage>().enabled =true;
            //        }
            //    }
            //    else
            //    {
            //        if (fgp != null)
            //        {
            //            FirstGuidePage.LoadGuidePage(fgp, centerTrans);
            //            // fgpPad.GetComponent<FirstGuidePage>().enabled = true;
            //        }
            //    }
            //}
        }
    }

    bool isPause;
    bool isFocus;
    void OnEnable()
    {
        isPause = false;
        isFocus = false;
    }

    /// <summary>
    /// 锁屏重启时，先调用Onfocus,后pause
    /// </summary>
    void OnApplicationPause()
    {

#if UNITY_IPHONE || UNITY_ANDROID
        if (!isPause)
        {
            // 强制暂停时，事件
        }
        else
        {
            isFocus = true;
        }
        isPause = true;
#endif
    }

    void OnApplicationFocus()
    {

#if UNITY_IPHONE || UNITY_ANDROID

        if (isFocus)
        {
            // “启动”手机时，事件
            isPause = false;
            isFocus = false;

            if (Application.isMobilePlatform)
            {
                if (!ShowPlaylist)
                { 
                    
                }
            }
        }
        if (isPause)
        {
            isFocus = true;
        }
#endif
    }
    #endregion
}
