using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.UI;
using Game.Scene;
using System;
using Game.Platform;
/// <summary>
/// author: 孙宇
/// describe:遥控器主界面
/// time: 
/// </summary>
public class UserdefControllerUI : BaseUI
{
    #region  //保护属性
    private UserCotrollerSettingUI setnewUI;
    protected List<BaseUI> mnewUIList;
    protected GameObject sliderToogle;
    protected Transform leftTrans;
    protected Transform topTrans;
    protected Transform bottomTrans;
    protected Transform gridPanel;
    protected Transform widgetsParent;
    protected Transform centerTip;
    protected Transform centerStartTip;
    protected Transform stopNowBtn;
    protected UILabel editTitle;
    protected UILabel editDescribe;
    protected UISprite placedArea;

    private SmartPlace smartPlace;  // 

    private int isExitCollision = 0; //是否碰撞，当拖动时 没有碰撞的情况下绘制可拖动面积
    public static bool isTotalDataChange = false;  // 检测数据是否发生改变
    private bool isActionDataChange = false;
    public bool isFirstSetting = false; //默认进入设置界面
    private bool _isSetting;
    private bool isZeroArea;
    private bool isFirstEdit = false;
    public static bool isSetting; 
    protected bool IsSetting  //遥控器设置
    {
        get
        {
            return _isSetting;
        }
        set
        {
            isSetting = value;
            _isSetting = value;
        //    ShowOrHideLines(value);
            ShowOrHideTopBoard(value);
            ShowOrHideLeftboard(value);
            if (UICamera.current != null)
                UICamera.current.allowMultiTouch = !value;
       //     UICamera.current.allowMultiTouch = !value;
            if (sliderToogle != null)
                sliderToogle.SetActive(value);

            DragdropItemEX dragObj;
            for (int i = 0; i < gridPanel.childCount; i++)
            {
                dragObj = gridPanel.GetChild(i).GetComponent<DragdropItemEX>();
                if(dragObj != null)
                    dragObj.enabled = value;
            }
        }
    }

    private List<string> showActionList;
    #endregion

    #region 公有函数
    public UserdefControllerUI(bool isFirstEnter)
    {
        mUIResPath = "Prefab/UI/control/userdefineControllerUI";
        isTotalDataChange = false;
        isActionDataChange = false;
        isFirstEdit = isFirstEnter;
        //editTitle.enabled = true;

        smartPlace = new SmartPlace();
    }
    #endregion
    #region 私有的
    /// <summary>
    /// 返回
    /// </summary>
    void OnBackbtnClicked()
    {
        //Debug.Log("MainWindow");
        Transform topL = GameObject.Find("userdefineControllerUI/Center/gridPanel").transform;
        if (topL != null && topL.childCount <= 1)
        {
            //Debug.Log("Delete Controller is " + RobotManager.GetInst().GetCurrentRobot().ID);
            ControllerManager.DeletaController(RobotManager.GetInst().GetCurrentRobot().ID);
        }
        
        mTrans.gameObject.SetActive(false);
        SceneMgr.EnterScene(SceneType.MainWindow);
    }
    /// <summary>
    /// 修改取消
    /// </summary>
    /// <param name="go"></param>
    void OnSecondbackClicked(GameObject obj)
    {
        //Debug.Log("define IsSetting is " + IsSetting);
        try
        {
            //Debug.Log("SecondbackClicked is run");
            if (obj == null)
            {
                //centerTip.gameObject.SetActive(true);
                //stopNowBtn.gameObject.SetActive(false);
                IsSetting = false;
                return;
            }
            string btnname = obj.name;
            if (btnname.Equals(PromptMsg.LeftBtnName))
            {
                editTitle.enabled = false;
                editDescribe.enabled = false;
                ShowOrHideLines(false);
                //Debug.Log("btnname is run");
                IsSetting = false;
                //取消修改
                if (isTotalDataChange)
                {
                    //Debug.Log("DataToWidget is run2");
                    ControllerManager.GetInst().CancelCurController();
                    if (gridPanel.childCount > 1)
                    {
                        for (int i = 1; i < gridPanel.childCount; i++)
                            GameObject.Destroy(gridPanel.GetChild(i).gameObject);
                    }
                    DataToWidget();
                }
            }
            else if (btnname.Equals(PromptMsg.RightBtnName))
            {
                editTitle.enabled = false;
                editDescribe.enabled = false;
                ShowOrHideLines(false);
                if (isTotalDataChange)
                {
                    OnConfirmClicked();
                    //DataToWidget();
                }    
            }
            
            //Debug.Log("Cancel Change!!");
            //centerTip.gameObject.SetActive(false);
            //centerStartTip.gameObject.SetActive(false);
        }
        catch
        { }
    }

    /// <summary>
    /// controldata数据显示出来
    /// </summary>
    void DataToWidget()
    {
        //Debug.Log("DataToWidget is run");

        isZeroArea = false;

        smartPlace.Clear();
        smartPlace.SetBgBoard(new Vector4(-gridPanel.GetComponent<UIWidget>().width / 2.0f + (UserdefControllerScene.leftSpace * Screen.width / 1334.0f), gridPanel.GetComponent<UIWidget>().height / 2.0f, gridPanel.GetComponent<UIWidget>().width / 2.0f - (UserdefControllerScene.leftSpace * Screen.width / 1334.0f), -gridPanel.GetComponent<UIWidget>().height / 2.0f));
        //Debug.Log((-Screen.width / 2.0f + (UserdefControllerScene.leftSpace * Screen.width / 1334.0f) + " " + (Screen.height / 2.0f) + " " + (Screen.width / 2.0f - (UserdefControllerScene.leftSpace * Screen.width / 1334.0f)) + " " + (-Screen.height / 2.0f)));

        GameObject actionWidget = Resources.Load("Prefabs/actionWidget") as GameObject;
        GameObject vsliderWidget = Resources.Load("Prefabs/vSlider") as GameObject;
        GameObject hsliderWidget = Resources.Load("") as GameObject;
        GameObject joystickWidget = Resources.Load("Prefabs/joystick") as GameObject;

        Transform leftItems = GameObject.Find("userdefineControllerUI/Left/leftBoard/ContainerLeft/EditScrollview/Grid").transform;

        if (leftItems != null)
        {
            Debug.Log("leftItem is not null!!");
            leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
        }
        
        //Debug.Log("isTotalDataChange is " + isTotalDataChange);
        //将遥控器数据显示出来
        if (ControllerManager.GetInst().TurnShowTypeList() == null)
        {            
            centerTip.gameObject.SetActive(true);
            centerStartTip.gameObject.SetActive(true);
            stopNowBtn.gameObject.SetActive(false);
            //return;
        }
        else
        {
            centerTip.gameObject.SetActive(false);
            centerStartTip.gameObject.SetActive(false);
            stopNowBtn.gameObject.SetActive(true);
        }
        foreach (var tem in ControllerManager.GetInst().TurnShowTypeList())
        {
            GameObject oo = null;
            if (tem.type == ControllerManager.WidgetShowType.widgetType.action)
            {
                oo = GameObject.Instantiate(actionWidget) as GameObject;
                oo.tag = "widget_action"; // 遥控面板的tag跟动作列表里的tag 响应的点击事件不一样
                oo.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
            }
            else if (tem.type == ControllerManager.WidgetShowType.widgetType.vSlider)
            {
                oo = GameObject.Instantiate(vsliderWidget) as GameObject;
                oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
            }
            else if (tem.type == ControllerManager.WidgetShowType.widgetType.joystick)
            {
                oo = GameObject.Instantiate(joystickWidget) as GameObject;
                oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
            }

            if (oo != null)
            {
                oo.transform.SetParent(gridPanel);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = tem.pos;
                oo.name = tem.widgetID;
                GetTCompent.GetCompent<DragdropItemEX>(oo.transform).enabled = false;
                GetTCompent.AddCompent<BoxCollider>(oo.transform);
                ButtonDelegate del = new ButtonDelegate();
                //Debug.Log("onDragdropStart");
                del.onDragdropStart = OnDragdropStart;
                del.onDragdropRelease = OnDragdropRelease;
                del.onDrag = OnButtonDrag;
                del.onClick = OnButtonClick;
                del.onPress = OnButtonPress;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);

                if (tem.type == ControllerManager.WidgetShowType.widgetType.action)  //获取对应的动作图标
                {
                    //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionNm;// 
                    string actionId = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionId;
                    if (actionId != null && actionId != "")
                    {
                        //Debug.Log("actionName is " + actionName);
                        ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId);
                        if (null == act)
                        {
                            oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "add";
                        }
                        else
                        {
                            oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                        }
                        string actionIcon = oo.transform.GetChild(0).GetComponent<UISprite>().spriteName;
                        ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionNm = RobotManager.GetInst().GetCurrentRobot().GetActionNameForIcon(actionIcon);
                        string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(oo.name)).actionNm;
                        //Debug.Log("Get actionIcon is " + actionIcon);
                        //Debug.Log("Get actionName is " + actionName);
                        /*if (actionId != "")
                        {
                            Debug.Log("Get actionId is " + RobotManager.GetInst().GetCurrentRobot().GetActionsIconForID(actionId));
                        }*/
                    }
                }
                //Debug.Log("DataToWidget Add Board");
                smartPlace.AddBoard(new SmartPlace.RectBoard(oo.name, TurnWidgetRect(oo.GetComponentInChildren<UIWidget>())));
            }
        }
        ControllerManager.GetInst().ControllerReady();
    }
    Vector4 TurnWidgetRect(UIWidget widget)
    {
        if (widget == null)
            return Vector4.zero;
        return new Vector4(widget.transform.localPosition.x - widget.width / 2.0f, widget.transform.localPosition.y + widget.height / 2.0f, widget.transform.localPosition.x + widget.width / 2.0f, widget.transform.localPosition.y - widget.height / 2.0f);
    }
    /// <summary>
    /// 遥控器修改确认
    /// </summary>
    /// <param name="go"></param>
    void OnConfirmClicked()
    {
        IsSetting = false;
        //发通知出去  本地遥控器数据更新，动作列表更新
        if (isTotalDataChange)
        {
            ControllerManager.GetInst().SaveCurController();
            ControllerManager.GetInst().ControllerReady();
        }
        if (isActionDataChange)
        {
            //Debug.Log("isActionDataChange is " + isActionDataChange);
            //InitActionList();
        }
        Transform topL = GameObject.Find("userdefineControllerUI/Center/gridPanel").transform;
        if (topL != null && topL.childCount <= 1)
        {
            //Debug.Log("Delete Controller is " + RobotManager.GetInst().GetCurrentRobot().ID);
            ControllerManager.DeletaController(RobotManager.GetInst().GetCurrentRobot().ID);
            centerTip.gameObject.SetActive(true);
            centerStartTip.gameObject.SetActive(true);
            stopNowBtn.gameObject.SetActive(false);
        }
        else
        {
            centerTip.gameObject.SetActive(false);
            centerStartTip.gameObject.SetActive(false);
            stopNowBtn.gameObject.SetActive(true);
        } 
    }
    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="go"></param>
    void OnSettingbtnClicked()
    {
        IsSetting = true;
        isActionDataChange = false;
        isTotalDataChange = false;
    }

    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="go"></param>
    void OnStopbtnClicked(GameObject go)
    {
        ActionLogic.GetIns().DoStopAction(go);
    }
    
    /// <summary>
    /// 伸缩左侧面板
    /// </summary>
    /// <param name="go"></param>
    void OnExplorebtnClicked()
    {     
        ShowOrHideLeftboard(!isLeftShow);
    }
    /// <summary>
    /// 网格显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    void ShowOrHideLines(bool isshow)
    {
        if (gridPanel.GetComponent<UISprite>() != null)
            gridPanel.GetComponent<UISprite>().enabled = isshow;
    }
    /// <summary>
    /// 左侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isLeftShow = false;
    void ShowOrHideLeftboard(bool isshow)
    {
        if (leftTrans == null || isshow == isLeftShow)
            return;
        isLeftShow = isshow;
        if (isshow && isBottomShow)  //底部如果存在则消失
        {
            ShowOrHideBottmBoard(false);
        }
        HideOrShowTrans(isshow, leftTrans, directType.left);
        sliderToogle.transform.GetChild(0).transform.Rotate(new Vector3(0, 180, 0));
       // ClientMain.GetInst().StartCoroutine(DelayOneFrame(isshow));
    }
    IEnumerator DelayOneFrame(bool flag)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        HideOrShowTrans(flag, leftTrans, directType.left);
    }
    /// <summary>
    /// 上侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isTopShow = false;
    void ShowOrHideTopBoard(bool isshow)
    {
        #region  //上边栏弹出 

        #endregion
        if (rightControl != null && rightSetting != null)
        {
            rightSetting.gameObject.SetActive(isshow);
            rightControl.gameObject.SetActive(!isshow);
        }
        topTrans.FindChild("topLeft").gameObject.SetActive(!isshow);
    }
    /// <summary>
    /// 底部面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isBottomShow = false;
    void ShowOrHideBottmBoard(bool isshow)
    {
        if (bottomTrans == null || isshow == isBottomShow)
            return;
        bottomTrans.gameObject.SetActive(true);
        isBottomShow = isshow;
        bottomTrans.GetChild(0).GetComponent<BoxCollider>().enabled = isBottomShow;
        if (isshow && isLeftShow)
            ShowOrHideLeftboard(false);
        HideOrShowTrans(isshow, bottomTrans, directType.bottom, 0.7f, OverHide);
        if (!isshow && curSettingAction != null)  //下弹出框消失时 取消动作选中状态
        {
            //Debug.Log("取消动作选中");
            curSettingAction.GetComponent<UISprite>().spriteName = "Button";
            //Debug.Log("Cancel isBottomShow is " + isBottomShow);
        }
        else if (isshow && curSettingAction != null) //动作选中
        {
            //Debug.Log("动作选中状态");
            //curSettingAction.GetComponent<UISprite>().spriteName = "Button";
            //Debug.Log("Select isBottomShow is " + isBottomShow);
        }
    }
    void OverHide()
    {
        //Debug.Log("OverHide is run!!");
        //Debug.Log("OverHide isActionChange is " + isActionDataChange);
        if (isActionDataChange)
        {
            //Debug.Log("now isActionChange is " + isActionDataChange);
            Transform grid = GameObject.Find("userdefineControllerUI/Bottom/bottomBoard/Sprite/grid").transform;
            for (int i = 0; i < grid.childCount; i++)
            {
                GameObject.Destroy(grid.GetChild(i).gameObject);
            }
            GameObject obj = Resources.Load("Prefabs/newActionItem1") as GameObject;
            GameObject none = GameObject.Instantiate(obj) as GameObject;
            none.transform.SetParent(grid);
            none.transform.localScale = Vector3.one;
            ButtonDelegate del1 = new ButtonDelegate();
            del1.onClick = OnButtonClick;
            GetTCompent.GetCompent<ButtonEvent>(none.transform).SetDelegate(del1);
            if (showActionList != null && showActionList.Count > 0)
            {
                for (int i = 0; i < showActionList.Count; i++)
                {
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.transform.SetParent(grid);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = Vector3.zero;
                    ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(showActionList[i]);
                    if (act == null)
                    {
                        oo.GetComponentInChildren<UILabel>().text = string.Empty;
                        oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "add";
                    }
                    else
                    {
                        oo.GetComponentInChildren<UILabel>().text = act.Name;
                        oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                    }
                    
                    //Debug.Log("action name is " + oo.transform.GetChild(0).GetComponent<UISprite>().spriteName);

                    ButtonDelegate del = new ButtonDelegate();
                    del.onClick = OnButtonClick;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
            grid.GetComponent<UIGrid>().repositionNow = true;
        }
        //else
            //InitActionList();
        //Debug.Log("OverHide isBottomShow is " + isBottomShow);
        bottomTrans.gameObject.SetActive(isBottomShow);
    }
    /// <summary>
    /// 面板伸缩
    /// </summary>
    /// <param name="isShow"></param>
    /// <param name="trans"></param>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <param name="call"></param>
    public static void HideOrShowTrans(bool isShow, Transform trans, directType type, float time = 0.5f, EventDelegate.Callback call = null)
    {
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;
        if (trans != null)
        {
            Transform nullTran = null;
            UIWidget temWidget = trans.GetComponentInChildren<UIWidget>();
            temWidget.SetAnchor(nullTran);
            int hh = temWidget.height;
            int ww = temWidget.width;
            hh -= 40;
            ww -= 40;
            MyAnimtionCurve cur1 = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            if (trans.GetComponent<TweenPosition>() == null)
            {
                trans.gameObject.AddComponent<TweenPosition>();
                trans.GetComponent<TweenPosition>().from = trans.localPosition;
                if (type == directType.bottom)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y + hh, trans.localPosition.z);
                else if (type == directType.top)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y - hh, trans.localPosition.z);
                else if (type == directType.left)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x + ww, trans.localPosition.y, trans.localPosition.z);
                else if (type == directType.right)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x - ww, trans.localPosition.y, trans.localPosition.z);
                trans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
            }
            tp = trans.GetComponent<TweenPosition>();
            tp.duration = time;

            if (isShow)
            {
                tp.PlayForward();

            }
            else
            {
                tp.PlayReverse();
            }
            if(tp != null && call != null)
                tp.AddOnFinished(call);
        }
    }
    /// <summary>
    /// 方向枚举
    /// </summary>
    public enum directType
    {
        left,
        right,
        top,
        bottom,
    }
    /// <summary>
    /// 动作设置入口
    /// </summary>
    private GameObject curSettingAction;
    void OnActionSetting(GameObject obj)
    {
        if (!IsSetting)
        {
            //Debug.Log("IsSetting iees " + IsSetting);
        }
        else
        {
            //Debug.Log("Now IsSetting is " + IsSetting);
            curSettingAction = obj;
            ShowOrHideBottmBoard(true);
        }
    }
    /// <summary>
    /// 设置摇杆
    /// </summary>
    /// <param name="id"></param>
    void OpenJoystickSettingUI(string id)
    {
        UserdefControllerScene.Ins.OpenJoystickSettingUI(id);
     //   OnHide();
    }
    void OpenVsliderSetting(string id)
    {
        UserdefControllerScene.Ins.OpenVsliderSettingUI(id);
    }
    /// <summary>
    /// 初始化动作列表
    /// </summary>
    void  InitActionList()
    {
        Transform grid = bottomTrans.FindChild("Sprite/grid");
        if (grid != null)
        {
            List<string> actions = RobotManager.GetInst().GetCurrentRobot().GetActionsNameList();
            
            //Debug.Log("actions count is " + actions.Count+"Time:"+Time.fixedDeltaTime);
            
            for (int i = 0; i < grid.childCount; i++)
            {
                GameObject.Destroy(grid.GetChild(i).gameObject);
            }

            GameObject obj = Resources.Load("Prefabs/newActionItem1") as GameObject;
            GameObject none = GameObject.Instantiate(obj) as GameObject;
            none.transform.SetParent(grid);
            none.transform.localScale = Vector3.one;
            ButtonDelegate del1 = new ButtonDelegate();
            del1.onClick = OnButtonClick;
            GetTCompent.GetCompent<ButtonEvent>(none.transform).SetDelegate(del1);

            for (int i = 0; i < actions.Count; i++)
            {
                if (!ControllerManager.GetInst().IsActionExist(RobotManager.GetInst().GetCurrentRobot().GetActionsForName(actions[i]).Id))
                {
                    //Debug.Log("IsActionExist is false");
                    if (!showActionList.Contains(RobotManager.GetInst().GetCurrentRobot().GetActionsForName(actions[i]).Id)) //不存在
                    {
                        //Debug.Log("actions is not exist!!");
                        showActionList.Add(RobotManager.GetInst().GetCurrentRobot().GetActionsForName(actions[i]).Id);
                    }
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.transform.SetParent(grid);
                    oo.transform.localScale = Vector3.one;
                    oo.GetComponentInChildren<UILabel>().text = actions[i];          //动作名称
                    
                    UISprite sp = oo.transform.GetChild(0).GetComponent<UISprite>();
                    if (null != sp)
                    {
                        ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(actions[i]);
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
                    ButtonDelegate del = new ButtonDelegate();
                    del.onClick = OnButtonClick;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
            grid.GetComponent<UIGrid>().repositionNow = true;
            UIManager.SetButtonEventDelegate(grid, mBtnDelegate);
        }
    }
    #endregion

    #region other
    private Transform rightSetting;
    private Transform rightControl;
    private Transform leftItems;
    protected override void AddEvent()
    {
        base.AddEvent();
        showActionList = new List<string>();
        //添加
        leftTrans = GameObject.Find("userdefineControllerUI/Left/leftBoard").transform;
        bottomTrans = GameObject.Find("userdefineControllerUI/Bottom/bottomBoard").transform;
        topTrans = GameObject.Find("userdefineControllerUI/Up").transform;
        widgetsParent = GameObject.Find("userdefineControllerUI/Center/widgetsParent").transform;
        gridPanel = GameObject.Find("userdefineControllerUI/Center/gridPanel").transform;
        sliderToogle = GameObject.Find("userdefineControllerUI/Left/leftBoard/ContainerLeft/Backdrop/sliderToogleBtn");
        editTitle = GameObject.Find("userdefineControllerUI/Up/edittitle").GetComponent<UILabel>();
        editDescribe = GameObject.Find("userdefineControllerUI/Up/editdescribe").GetComponent<UILabel>();
        centerTip = GameObject.Find("userdefineControllerUI/StartIcon").transform;
        centerStartTip = GameObject.Find("userdefineControllerUI/StartTip").transform;
        stopNowBtn = GameObject.Find("userdefineControllerUI/Up/topright_control/stopBtn").transform;
        
        #region 位置布局 
        Transform left = leftTrans.GetChild(0);
        Vector3 pos = UIManager.GetWinPos(left, UIWidget.Pivot.Left, -left.GetComponentInChildren<UIWidget>().width-34);
        left.localPosition = pos;
        Transform toplr = topTrans.FindChild("topLeft");
        pos = UIManager.GetWinPos(toplr, UIWidget.Pivot.TopLeft,34, 34);
        toplr.localPosition = pos;
        rightSetting = topTrans.FindChild("topright_setting");
        pos = UIManager.GetWinPos(rightSetting.GetChild(0), UIWidget.Pivot.TopRight,34,34);
        rightSetting.localPosition = pos;
        rightControl = topTrans.FindChild("topright_control");
        pos = UIManager.GetWinPos(rightControl.GetChild(0), UIWidget.Pivot.TopRight, 34, 34);
        rightControl.localPosition = pos;
        pos = UIManager.GetWinPos(editTitle.transform, UIWidget.Pivot.Top, 0, 34);
        editTitle.transform.localPosition = pos;
        //Debug.Log(pos.x+" "+pos.y+" "+pos.z);
        editTitle.text = LauguageTool.GetIns().GetText("设置遥控器页面主标题");
        pos = UIManager.GetWinPos(editDescribe.transform, UIWidget.Pivot.Top, 0, 74);
        editDescribe.transform.localPosition = pos;
        editDescribe.text = LauguageTool.GetIns().GetText("设置遥控器页面副标题");
        Transform bottom = bottomTrans.parent;
        Transform deleta = bottom.GetChild(1);
        pos = UIManager.GetWinPos(bottomTrans, UIWidget.Pivot.Bottom, 0, -bottomTrans.GetComponentInChildren<UIWidget>().height-11f);
        bottomTrans.localPosition = pos;
        pos = UIManager.GetWinPos(deleta, UIWidget.Pivot.BottomRight,34,34);
        deleta.localPosition = pos;
        deleta.gameObject.SetActive(false);  //隐藏

        centerTip.localPosition = UIManager.GetWinPos(centerTip, UIWidget.Pivot.Center, 0, 70);
        centerStartTip.localPosition = UIManager.GetWinPos(centerStartTip, UIWidget.Pivot.Center, 0, -70);
        centerStartTip.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("初始化遥控器提示");
       // topTrans.GetComponentInChildren<UISprite>().width = PublicFunction.GetWidth();
        bottomTrans.GetComponentInChildren<UISprite>().width = PublicFunction.GetWidth();
        leftTrans.GetComponentInChildren<UISprite>().height = PublicFunction.GetHeight();
        gridPanel.GetComponent<UIWidget>().width = PublicFunction.GetWidth();
        gridPanel.GetComponent<UIWidget>().height = PublicFunction.GetHeight();
        placedArea = gridPanel.GetChild(0).GetComponent<UISprite>();
        placedArea.enabled = false;
        IsSetting = false;

        centerTip.gameObject.SetActive(false);
        centerStartTip.gameObject.SetActive(false);
        stopNowBtn.gameObject.SetActive(false);

        if (isFirstEdit)
        {
            isFirstEdit = false;
            ShowOrHideLines(true);
            editTitle.enabled = true;
            editDescribe.enabled = true;
        }
        else
        {
            ShowOrHideLines(false);
            editTitle.enabled = false;
            editDescribe.enabled = false;
        }

        leftItems = GameObject.Find("userdefineControllerUI/Left/leftBoard/ContainerLeft/EditScrollview/Grid").transform;

        if (leftItems != null)
        {
            Debug.Log("leftItem is not null!!");
            leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
            leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;

            // 设置长按反应时间
            leftItems.GetChild(0).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
            leftItems.GetChild(1).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
            leftItems.GetChild(2).GetComponent<UIDragDropItem>().pressAndHoldDelay = 0.1f;
        }

        InitActionList();
        bottomTrans.gameObject.SetActive(false);
        #endregion
    }
    protected override void OnButtonPress(GameObject obj, bool press)
    {
        if (!IsSetting)
        {
            base.OnButtonPress(obj, press);

            //GameObject setBtn0 = GameObject.Find("userdefineControllerUI/Up/topright_control/settingBtn");
            
            //Debug.Log("Now touchCount is " + ControlLogic.touchCount);

            try
            {
                if (press)
                    ControlLogic.touchCount++;
                else
                    ControlLogic.touchCount--;
                if (obj.tag.Contains("widget_action")) //动作之间互斥 ，不可同时按下两个动作
                {
                    string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;

                    if (!PlatformMgr.Instance.GetBluetoothState())  //未连接的情况 模型运动 
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
                    }
                             
                    else if (actionName != "")
                    {
                        if (press)
                        {
                            if (!ControlLogic.actionTouch) //动作被按下时 不响应其它动作
                            {
                                ControlLogic.actionTouch = true;
                                ControlLogic.GetIns().PlayAction(actionName);
                            }
                        }
                        else
                        {
                            ControlLogic.actionTouch = false;
                            ControlLogic.GetIns().CancelRePlay();
                        }
                    }
                }
                if (obj.tag.Contains("widget_vslider") && (!IsSetting)) //动作之间互斥 ，不可同时按下两个动作
                {
                    //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;
                    if (!PlatformMgr.Instance.GetBluetoothState())  //未连接的情况 模型运动 
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
                    }
                }
                if (obj.tag.Contains("widget_joystick") && (!IsSetting)) //动作之间互斥 ，不可同时按下两个动作
                {
                    //string actionName = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionNm;
                    if (!PlatformMgr.Instance.GetBluetoothState())  //未连接的情况 模型运动 
                    {
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
                    }
                }

                BoxCollider setBox = rightControl.GetChild(0).GetComponent<BoxCollider>();
                BoxCollider backBox = GameObject.Find("userdefineControllerUI/Up/topLeft/back").GetComponent<BoxCollider>();
                
                if (ControlLogic.touchCount == 1 || ControlLogic.touchCount == 0)  //遥控器操作时 返回和设置按钮不响应
                {
                    //setBtn0.GetComponent<UIButton>().enabled = true;
                    setBox.enabled = true;
                    backBox.enabled = true;
                }
                else
                {
                    //setBtn0.GetComponent<UIButton>().enabled = false;
                    setBox.enabled = false;
                    backBox.enabled = false;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    //配置控件数据事件
    protected override void OnButtonClick(GameObject obj)
    {
        //Debug.Log("Button Click Event");
        base.OnButtonClick(obj);
        try
        {
            string name = obj.name;
            if(name.Equals("back")) //返回主页
            {
                editTitle.enabled = false;
                ShowOrHideLines(false);
                ActionLogic.GetIns().DoStopAction(obj);
                editDescribe.enabled = false;
                OnBackbtnClicked();
            }
            else if(name.Equals("secBack")) //设置返回
            {
                editTitle.enabled = false;
                ShowOrHideLines(false);
                editDescribe.enabled = false; 
                //centerTip.gameObject.SetActive(false);
                Transform topL2 = GameObject.Find("userdefineControllerUI/Center/gridPanel").transform;
                if (!isTotalDataChange && topL2 != null && topL2.childCount <= 1)
                {
                    //Debug.Log("Delete Controller is " + RobotManager.GetInst().GetCurrentRobot().ID);
                    ControllerManager.DeletaController(RobotManager.GetInst().GetCurrentRobot().ID);
                    centerTip.gameObject.SetActive(true);
                    centerStartTip.gameObject.SetActive(true);
                    stopNowBtn.gameObject.SetActive(false);
                }
                else
                {
                    centerTip.gameObject.SetActive(false);
                    centerStartTip.gameObject.SetActive(false);
                    stopNowBtn.gameObject.SetActive(true);
                }
                UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("保存遥控器提示"), OnSecondbackClicked, isTotalDataChange);
            }
            else if (name.Equals("stopBtn")) //动作停止
            {
                OnStopbtnClicked(obj);
                //Debug.Log("stop");
            }
            else if (name.Equals("settingBtn")) //设置
            {
                editTitle.enabled = true;
                editDescribe.enabled = true;
                ShowOrHideLines(true);
                centerTip.gameObject.SetActive(false);
                centerStartTip.gameObject.SetActive(false);
                stopNowBtn.gameObject.SetActive(false);
                OnSettingbtnClicked();
            }
            else if (name.Equals("confirm")) //确认修改
            {
                editTitle.enabled = false;
                editDescribe.enabled = false;
                ShowOrHideLines(false);
                OnConfirmClicked();
            }
            else if (name.Equals("sliderToogleBtn"))  //伸缩按钮
            {
                OnExplorebtnClicked();
            }
            else if (name.Equals("bg_down"))
            {
                ShowOrHideBottmBoard(false);
            }
            else if (obj.name.Contains("newActionItem"))
            {
                if (curSettingAction != null)
                {
                    //Debug.Log("curSettingAction is not null!!"); 
                    curSettingAction.transform.GetChild(0).GetComponent<UISprite>().spriteName = obj.transform.GetChild(0).GetComponent<UISprite>().spriteName;
                    // controllerdata 数据修改
                    string name1 = obj.GetComponentInChildren<UILabel>().text;
                    if (name1 == "")
                    {
                        name1 = "-1";
                    }
                    //Debug.Log("name1 is " + name1);
                    string name2 = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionNm;
                    //Debug.Log("name2 is "+ name2);
                    string id1;
                    if (name1 == "" || name1 == "-1")
                        id1 = "-1";
                    else
                        id1 = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(name1).Id;
                    //Debug.Log("id1 is " + id1);
                    string id2 = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionId;             

                    
                    //((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionId = RobotManager.GetInst().GetCurrentRobot().GetActionsForName(name2).Id;
                    if (id2 != id1 || name2 != name1)
                    {
                        ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionNm = name1;
                        ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(curSettingAction.name)).actionId = id1;
                        isActionDataChange = true;
                        isTotalDataChange = true;
                        if (id1 == "") //选择了一个空的 
                        {
                            if (!showActionList.Contains(id2))
                                showActionList.Add(id2);
                        }
                        else
                        {
                            if (showActionList.Contains(id1))
                                showActionList.Remove(id1);
                            if (id2 != "" && id2 != "-1" && !showActionList.Contains(id2))
                                showActionList.Add(id2);
                        }
                    }
                    else  //同时为空动作
                    {
                        return;
                    }
                    /*if (name2 != "")
                    {
                        if (!showActionList.Contains(name2))
                            showActionList.Add(name2);
                    }
                    if (name1 != "")
                    {
                        if (showActionList.Contains(name1))
                            showActionList.Remove(name1);
                    }*/
                }
                ShowOrHideBottmBoard(false);
            }
            else if (obj.tag.Contains("widget_action")) //动作被点击
            {
                //Debug.Log("IsSetting is "+IsSetting);
                if (IsSetting && obj.transform.parent.name == "gridPanel")
                {
                    editTitle.enabled = false;
                    editDescribe.enabled = false;
                    //ShowOrHideLines(false);
                    OnActionSetting(obj);
                }
            }
            else if (obj.tag.Contains("widget_vslider")) //竖杆被点击
            {  
                if (IsSetting && obj.transform.parent.name == "gridPanel")
                {
                    editTitle.enabled = true;
                    editDescribe.enabled = true;
                    ShowOrHideLines(true);
                    
                    if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 1)
                    {
                        editTitle.enabled = false;
                        editDescribe.enabled = false;
                        //ShowOrHideLines(false);
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
                    }
                    else
                    {
                        editTitle.enabled = true;
                        editDescribe.enabled = true;
                        ShowOrHideLines(true);
                        OpenVsliderSetting(obj.name);
                    }
                }
            }
            else if (obj.tag.Contains("widget_joystick"))   //摇杆被点击
            {
                if (IsSetting && obj.transform.parent.name == "gridPanel")
                {
                    editTitle.enabled = true;
                    editDescribe.enabled = true;
                    ShowOrHideLines(true);
                    if (RecordContactInfo.Instance.openType == "default")
                    {
                        editTitle.enabled = false;
                        editDescribe.enabled = false;
                        ShowOrHideLines(true);
                        HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("官方模型不可更改配置"));
                    }
                    else
                    {
                        if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 2)
                        {
                            editTitle.enabled = false;
                            editDescribe.enabled = false;
                            //ShowOrHideLines(false);
                            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
                        }
                        else
                        {
                            editTitle.enabled = true;
                            editDescribe.enabled = true;
                            ShowOrHideLines(true);
                            OpenJoystickSettingUI(obj.name);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        { 
        
        }
    }
    /// <summary>
    /// 负责设置状态下控件拖动事件
    /// </summary>
    bool flag1 = false; //拖动前左侧面板是否显示
    bool dragging = false;  //拖动ing 
    Transform dragingTransform; //正在被拖动的控件
    Vector3 finalPosition; //控件最终适应的位置
    bool isColliderCheck = false;
    bool isNewOperate = false;
    protected override void OnDragdropStart(GameObject obj)
    {
        editTitle.enabled = false;
        editDescribe.enabled = false;

        isZeroArea = false;
        //Debug.Log("dragdrop is start");
        base.OnDragdropStart(obj);
        obj.transform.localScale = Vector3.one;
        if (IsSetting && obj.tag.Contains("widget"))
        {
            //Debug.Log("drag is start");
            placedArea.enabled = true;  
            ShowOrHideLines(true);
            Vector4 currentSelect = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
            smartPlace.ChangeCurPos(currentSelect);

            isExitCollision = 0;
            dragging = true;
            dragingTransform = obj.transform;
            GetTCompent.GetCompent<Rigidbody>(obj.transform).useGravity = false;
            GetTCompent.GetCompent<TriggleCheck>(obj.transform).onTriggleEnter = OnEnterCollision;
            GetTCompent.GetCompent<TriggleCheck>(obj.transform).onTriggleExit = OnExitCollision;
            GetTCompent.GetCompent<BoxCollider>(obj.transform).isTrigger = true;

            //Transform leftSelect = GameObject.Find("userdefineControllerUI/Left/leftBoard/ContainerLeft/EditScrollview/Grid").transform;

            bottomTrans.parent.GetChild(1).gameObject.SetActive(true);  //拖动时垃圾桶出现
            if (isBottomShow)
                ShowOrHideBottmBoard(false);
            if (isLeftShow)
            {
                flag1 = true;
                ShowOrHideLeftboard(false);
                
                if (obj.tag.Contains("widget") && obj.transform.parent != gridPanel) // obj为控件库里的物体
                {
                    //Debug.Log("isNewOperate is true");
                    isNewOperate = true;
                    obj.transform.SetParent(gridPanel);
                    //Debug.Log(gridPanel);

                    if (obj.tag.Contains("widget_action"))
                    {
                        obj.transform.GetChild(4).GetComponent<UISprite>().enabled = true;
                        leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = true;
                    }
                    else if (obj.tag.Contains("widget_joystick"))
                    {
                        obj.transform.GetChild(3).GetComponent<UISprite>().enabled = true;
                        leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = true;
                    }
                    else if (obj.tag.Contains("widget_vslider"))
                    {
                        obj.transform.GetChild(3).GetComponent<UISprite>().enabled = true;
                        leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = true;
                    }       

                    if (obj.GetComponent<UIDragScrollView>() != null)
                        GameObject.DestroyImmediate(obj.GetComponent<UIDragScrollView>());

                    /*Vector4 currentIcon = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
                    smartPlace.ChangeCurPos(currentIcon);
                    DrawCanPlacedArea(new Vector2((currentIcon.x + currentIcon.z) / 2.0f, (currentIcon.y + currentIcon.w) / 2.0f), (int)(currentIcon.z - currentIcon.x), (int)(currentIcon.y - currentIcon.w));*/
                }
            }
            else
                flag1 = false;

            if(!isNewOperate)
            {
                //Debug.Log("SetCurRect is ready");
                smartPlace.SetCurRect(obj.name);
            }
            //Debug.Log("Drag isNewOperate is " + isNewOperate);
            if (isNewOperate)
            {
                if (!(smartPlace.IsNewControl(currentSelect)))
                {
                    //Debug.Log("IsNewControl is " + smartPlace.IsNewControl(currentSelect));
                    isZeroArea = true;
                    placedArea.enabled = false;
                    DrawCanPlacedArea(new Vector2(0, 0), 0, 0);
                }
                else
                {
                    //Debug.Log("Drag isNewOperate is true");
                    isZeroArea = false;
                    placedArea.enabled = true;
                    DrawCanPlacedArea(new Vector2((currentSelect.x + currentSelect.z) / 2.0f, (currentSelect.y + currentSelect.w) / 2.0f), (int)(currentSelect.z - currentSelect.x), (int)(currentSelect.y - currentSelect.w));
                }
                
            }
            else
            {
                //Debug.Log("Drag isZeroArea is false");
                isZeroArea = false;
                placedArea.enabled = true;
                DrawCanPlacedArea(new Vector2((currentSelect.x + currentSelect.z) / 2.0f, (currentSelect.y + currentSelect.w) / 2.0f), (int)(currentSelect.z - currentSelect.x), (int)(currentSelect.y - currentSelect.w));
            }
            
        }
    }

    //松开选择的控件放置于界面上
    protected override void OnDragdropRelease(GameObject obj)
    {
        
        base.OnDragdropRelease(obj);
        //ShowOrHideLines(false);

        //ui状态更改
        if (flag1 != isLeftShow)  //拖动前后左侧面板状态还原
            ShowOrHideLeftboard(flag1);

        Vector4 curSelected = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
        //拖动出界判定失败
        if (isNewOperate && smartPlace.IsNewControl(curSelected))
        {
            isZeroArea = false;
        }

        if (leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled)
            leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
        if (leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled)
            leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
        if (leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled)
            leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
        
        
        bottomTrans.parent.GetChild(1).gameObject.SetActive(false);  //拖动时垃圾桶出现
        dragging = false;
        dragingTransform = null;
        //判断是否拖动成功
        if (!IsDragSuccess() || isZeroArea)
        {
            //Debug.Log("isZeroArea is " + isZeroArea);
            if (isNewOperate)
            {
                isNewOperate = false;
            }
            
            placedArea.enabled = false;
            if (isDeleta || isZeroArea)
            {
                isZeroArea = false;
                GameObject.Destroy(obj);
                if (obj.tag.Contains("widget_action") && ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)) != null)
                {
                   // string an = obj.GetComponentInChildren<UILabel>().text;
                    string an = ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).actionId;
                    if (an != "")
                    {
                        if (!showActionList.Contains(an))
                            showActionList.Add(an);
                    }
                    ControllerManager.GetInst().RemoveAction(obj.name);
                    isActionDataChange = true;
                    //leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
                }
                else if (obj.tag.Contains("widget_vslider"))
                {
                    ControllerManager.GetInst().RemoveSliderBar(obj.name);
                    //leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
                }
                else if (obj.tag.Contains("widget_joystick"))
                {
                    ControllerManager.GetInst().RemoveJoystick(obj.name);
                    //leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
                }
                isDeleta = false;
                isTotalDataChange = true;
            }
            return;
        }
        isTotalDataChange = true;

        #region    //判断当前操作类型， 对应修改临时的controllerdata数据
        obj.transform.localPosition = finalPosition;
        if (isNewOperate)  //新建控件
        {
            //Debug.Log("new control");
            //Debuger.Log(ControllerManager.GetInst().widgetManager.joyStickManager.JoystickNum());
            isNewOperate = false;
            obj.name = System.DateTime.Now.ToFileTime().ToString(); //对应的控件id 为创建的时间
            DragdropItemEX dragdropEx = obj.GetComponent<DragdropItemEX>();
            if (dragdropEx != null)    //释放完成后 更改克隆属性，不然会有问题
            {
                dragdropEx.cloneOnDrag = false;
                dragdropEx.restriction = UIDragDropItem.Restriction.None;
            }
            if (obj.tag.Contains("widget_action"))
            {
                ControllerManager.GetInst().NewAction(obj.name, obj.transform.localPosition);
                obj.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                //leftItems.GetChild(0).GetChild(4).GetComponent<UISprite>().enabled = false;
            }
            else if (obj.tag.Contains("widget_vslider"))
            {
                ControllerManager.GetInst().NewSliderBar(obj.name, obj.transform.localPosition);
                obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                //leftItems.GetChild(2).GetChild(3).GetComponent<UISprite>().enabled = false;
            }
            else if (obj.tag.Contains("widget_joystick"))
            {
                if (ControllerManager.GetInst().widgetManager.joyStickManager.JoystickNum() > 0)
                {
                    placedArea.enabled = false;
                    GameObject.Destroy(obj);                        
                    ControllerManager.GetInst().RemoveJoystick(obj.name);
                    //isDeleta = false;
                    isTotalDataChange = true;
                    dragging = false;
                    dragingTransform = null;
                    //leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
                    return;
                }
                else
                {
                    //placedArea.enabled = true;
                    //Debuger.Log(ControllerManager.GetInst().widgetManager.joyStickManager.JoystickNum());
                    ControllerManager.GetInst().NewJoystick(obj.name, obj.transform.localPosition);
                    //Debuger.Log("目前："+ControllerManager.GetInst().widgetManager.joyStickManager.JoystickNum());
                }
                obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                //leftItems.GetChild(1).GetChild(3).GetComponent<UISprite>().enabled = false;
            }
        }
        else  //拖动操作
        {
            //Debug.Log("Now Operate is Drag!!");
            if (obj.tag.Contains("widget_action"))
            {
                ((ActionWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;//.NewAction(obj.name, obj.transform.localPosition);
            }
            else if (obj.tag.Contains("widget_vslider"))
            {
                ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
            }
            else if (obj.tag.Contains("widget_joystick"))
            {
                ((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(obj.name)).localPos = obj.transform.localPosition;
            }
        }
        
        smartPlace.AddBoard(new SmartPlace.RectBoard(obj.name, TurnWidgetRect(obj.GetComponentInChildren<UISprite>())));
        placedArea.enabled = false;
        #endregion

        if (obj.GetComponent<BoxCollider>() != null)
            obj.GetComponent<BoxCollider>().enabled = true;

        if (IsSetting && obj.tag.Contains("widget"))   //拖动结束 删掉碰撞检测功能
        {
            if (obj.GetComponent<Rigidbody>() != null)
                GameObject.Destroy(obj.GetComponent<Rigidbody>());
            if (obj.GetComponent<TriggleCheck>() != null)
                GameObject.Destroy(obj.GetComponent<TriggleCheck>());
            if (isExitCollision != 0)
            {
             //   isExitCollision = 0;
              //  obj.transform.localPosition = finalPosition;
            }
        }

    }
    /// <summary>
    /// 判断是否拖动成功
    /// </summary>
    /// <returns></returns>
    bool IsDragSuccess()
    {
        if (isDeleta)
            return false;
        return true;
    }
    /// <summary>
    /// 初始化遥控器，根据遥控器ID初始化对应的遥控器界面和遥控器内容
    /// </summary>
    public override void Init()
    {
        base.Init();
        //curControllerData 设置界面
      //  DataToWidget();
    }
    public override void LoadUI()
    {
        base.LoadUI();
        if (!ControllerManager.IsControllersNull(RobotManager.GetInst().GetCurrentRobot().ID)) //有遥控器数据
        {
            ClientMain.GetInst().StartCoroutine(DelayExcute());
        }
    }
    IEnumerator DelayExcute()
    {
        //Debug.Log("DataToWidget is run1");
        yield return null;
        yield return new WaitForEndOfFrame();
        DataToWidget();
    }

    //跟随拖拽显示网格
    protected override void OnButtonDrag(GameObject obj, Vector2 delta)
    {
        //isZeroArea = false;
        //Debug.Log("Now IsSetting is " + IsSetting);
        base.OnButtonDrag(obj, delta);
        if (isSetting && obj.tag.Contains("widget") && dragging)
        {
            if (smartPlace.IsEmptyBgboard())
            {
                smartPlace.Clear();
                smartPlace.SetBgBoard(new Vector4(-gridPanel.GetComponent<UIWidget>().width / 2.0f + (UserdefControllerScene.leftSpace * Screen.width / 1334.0f), gridPanel.GetComponent<UIWidget>().height / 2.0f, gridPanel.GetComponent<UIWidget>().width / 2.0f - (UserdefControllerScene.leftSpace * Screen.width / 1334.0f), -gridPanel.GetComponent<UIWidget>().height / 2.0f));
            }
            
            //Debug.Log(TurnWidgetRect(obj.GetComponentInChildren<UIWidget>()).x);
            //DrawCanPlacedArea(new Vector2(0, 0), 0, 0);

            //Debug.Log("dragging is "+dragging);
            Vector4 curSelect = TurnWidgetRect(obj.GetComponentInChildren<UIWidget>());
            smartPlace.ChangeCurPos(curSelect);

            if (smartPlace.IsNewControl(curSelect))
            {
                isZeroArea = false;
                //Debug.Log("IsEmptyable is true");
                placedArea.enabled = true;
                DrawCanPlacedArea(new Vector2((curSelect.x + curSelect.z) / 2.0f, (curSelect.y + curSelect.w) / 2.0f), (int)(curSelect.z - curSelect.x), (int)(curSelect.y - curSelect.w));
            }
            else
            {

                //Debug.Log("can't placed new controller");
            }
            //smartPlace.SetCurRect(new SmartPlace.RectBoard(obj.name
            //smartPlace.SetCurRect(new Vector4(obj.transform.localPosition.x, obj.transform.localPosition.y, obj.GetComponentInChildren<UIWidget>().width, obj.GetComponentInChildren<UIWidget>().height));
            if (smartPlace.IsPlaceable()/* || smartPlace.IsEmptyable()*/)
            {
                isZeroArea = false;
                //Debug.Log("IsPlaceable is true");
                //计算可放置区域大小
                placedArea.enabled = true;
                DrawCanPlacedArea(new Vector2((smartPlace.curRect.board.x + smartPlace.curRect.board.z) / 2.0f, (smartPlace.curRect.board.y + smartPlace.curRect.board.w) / 2.0f), (int)(smartPlace.curRect.board.z - smartPlace.curRect.board.x), (int)(smartPlace.curRect.board.y - smartPlace.curRect.board.w));//(new Vector2(smartPlace.curRect.board.x, smartPlace.curRect.board.y), (int)smartPlace.curRect.board.z, (int)smartPlace.curRect.board.w);
                //smartPlace.AddBoard(new SmartPlace.RectBoard(obj.name, TurnWidgetRect(obj.GetComponentInChildren<UISprite>())));
            }
            else
            {
                //DrawCanPlacedArea(new Vector2(0, 0), 0, 0);
                //Debug.Log("can't placed exist controller");
            }
        }
    }

    /*void FiexedUpdate()
    {
        if (IsSetting && isExitCollision == 0) //设置界面 拖动过程
        {
            if (dragingTransform != null)
            {
                DrawCanPlacedArea(dragingTransform.localPosition, 200, 300);
                //isExitCollision = true;

            }
        }
    }*/
    public override void LateUpdate()
    {
        base.LateUpdate();
        IsDragSuccess();
        if (IsSetting && dragging && isColliderCheck)
        {

            //if(dragingTransform != null)
        }
        //if (IsSetting && isExitCollision == 0) //设置界面 拖动过程
        //{
        //    if (dragingTransform != null)
        //    {
        //        DrawCanPlacedArea(dragingTransform.localPosition, 200, 300);
        //        //isExitCollision = true;
        //    }
        //}
    }
    public override void Open()
    {
        base.Open();
        if (isFirstSetting)
        {
            //Debug.Log("StartCoroutine");
            ClientMain.GetInst().StartCoroutine(DelayOneFrame(OnFirstComing));
        }
    }
    /// <summary>
    /// 默认配置遥控器进来
    /// </summary>
    void OnFirstComing()
    {
        //Debug.Log("IsSetting is true");
        IsSetting = true;
    }
    /// <summary>
    /// 进碰撞
    /// </summary>
    bool isDeleta = false;
    void OnEnterCollision(Collider collider)
    {
        isExitCollision++;
        if (collider.name == "deleta")
        {
            isDeleta = true;
        }
    }
    /// <summary>
    /// 出碰撞
    /// </summary>
    void OnExitCollision(Collider collider)
    {
        isExitCollision--;
        if (collider.name == "deleta")
        {
            isDeleta = false;
        }
    }
    /// <summary>
    /// 绘制可放置的区域
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void DrawCanPlacedArea(Vector2 pos, int width, int height)
    {
        placedArea.transform.localPosition = pos;
        placedArea.width = width;
        placedArea.height = height;
        finalPosition = pos;
    }
    /// <summary>
    /// 滞后一
    /// </summary>
    /// <param name="del"></param>
    /// <returns></returns>
    IEnumerator DelayOneFrame(EventDelegate.Callback del)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        del();
    }
    #endregion
}