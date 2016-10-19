using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// author: 孙宇
/// describe:摇杆设置界面
/// time: 
/// </summary>
public class JoystickSettingUI :BaseUI
{
    public bool isFirstSetting;
    private UISprite selectServo;
    private int selectServoID;
    private int selectWheelID;
    private bool curSelectServo;
    private Transform tra;

    private JockstickData joyData; //临时数据
    public JockstickData JoyData
    {
        get
        {
            return joyData;
        }
    }
    string widgetID;
    public JoystickSettingUI(string widgetID)
    {
        mUIResPath = "Prefab/UI/control/joystickSettingUI";
        isFirstSetting = false;
        joyData = new JockstickData();
        CopyData(joyData,(JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID));
        if (joyData.type == JockstickData.JockType.none)
            isFirstSetting = true;
        this.widgetID = widgetID;
     //   mTrans.FindChild("settingUI2/Bottom/bottomBoard/bg").GetComponent<UIWidget>().width = PublicFunction.GetWidth();
    }

    public void CopyData(JockstickData data, JockstickData copyD)
    {
        if (copyD != null)
        {
            data.type = copyD.type;
            data.leftUpID = copyD.leftUpID;
            data.leftBottomID = copyD.leftBottomID;
            data.rightBottomID = copyD.rightBottomID;
            data.rightUpID = copyD.rightUpID;
            
        }
    }

    private GameObject settingUI2;
    private GameObject settingUI1;
    private UILabel Title;
    private UILabel Details;
    private Transform bottomTrans;   //底部的舵机面板
    private int wheelType = 0;
    private bool hasJoysickData;
    protected override void AddEvent()
    {
        base.AddEvent();
        curSelectServo = false;
        Transform p = GameObject.Find("joystickSettingUI/settingUI2/Bottom/bottomBoard/Sprite/grid").transform;

        List<byte> turnModelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelModelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();

        if (turnModelList.Count > 0)
        {
            Debug.Log("当前轮模式");
            UserdefControllerScene.InitServoList(p, OnButtonClick, true);
        }
        else if (turnModelList.Count == 0 && engelModelList.Count > 0)
        {
            Debug.Log("当前角度模式");
            UserdefControllerScene.InitServoList(p, null, true);
        }
        else
            Debug.Log("当前无舵机");
        
        
        //UserdefControllerScene.InitServoList(p,null); //舵机列表
        //区分轮模式和角度模式的显示
        //……

        // settingUI1
        Transform trans = GameObject.Find("joystickSettingUI/settingUI1/Up/topLeft").transform;  //top - left
        trans.GetChild(0).localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft, UserdefControllerScene.leftSpace, UserdefControllerScene.upSpace);//UserdefControllerScene.upSpace);

        trans = GameObject.Find("joystickSettingUI/settingUI1/Center/type1").transform; // center - left
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Center, -200);
        trans = GameObject.Find("joystickSettingUI/settingUI1/Center/type2").transform; // center - right
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Center, 200);

        // settingUI2
        trans = GameObject.Find("joystickSettingUI/settingUI2/Up/topLeft").transform;  //top - left
        trans.GetChild(0).localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft, UserdefControllerScene.leftSpace, UserdefControllerScene.upSpace);
        trans = GameObject.Find("joystickSettingUI/settingUI2/Up/topRight").transform;       //top - right
        trans.GetChild(0).localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight, UserdefControllerScene.rightSpace, UserdefControllerScene.upSpace);//, topR.GetChild(0).GetComponent<UIWidget>().width);

        Vector3 po = new Vector3(0,trans.GetChild(0).position.y,0);
        trans = GameObject.Find("joystickSettingUI/Title").transform;  // title
        trans.position = po;//UIManager.GetWinPos(trans, UIWidget.Pivot.Top, 0, UserdefControllerScene.upSpace);
        Title = trans.GetComponent<UILabel>();

        trans = GameObject.Find("joystickSettingUI/settingUI2/Bottom").transform.GetChild(0);  //bottom
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom, 0, -trans.GetComponentInChildren<UIWidget>().height - 11f);//UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom,0,-trans.GetComponentInChildren<UISprite>().height);
        bottomTrans = trans;

        settingUI2 = GameObject.Find("joystickSettingUI/settingUI2");
        settingUI1 = GameObject.Find("joystickSettingUI/settingUI1");
        settingUI2.transform.GetChild(2).GetComponentInChildren<UIWidget>().width = PublicFunction.GetWidth();

        Vector3 po2 = new Vector3(0, Title.transform.position.y + 250, 0);
        Details = GameObject.Find("joystickSettingUI/settingUI2/Details").transform.GetComponent<UILabel>();
        Details.transform.localPosition = po2;
        if (isFirstSetting) // 车子结构选择界面
        {
            hasJoysickData = false;
            settingUI2.SetActive(false);
            Title.text = LauguageTool.GetIns().GetText("select model type");
            settingUI1.transform.GetChild(1).GetChild(0).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("two-wheels");
            settingUI1.transform.GetChild(1).GetChild(1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("four-wheels");
            Details.enabled = false;
            bottomTrans.gameObject.SetActive(false); //隐藏底部面板
        }
        else    // 配置界面
        {
            hasJoysickData = true;
            //settingUI1.SetActive(false);
            
            Title.text = LauguageTool.GetIns().GetText("设置轮子主标题");
            Details.enabled = true;
            string detailtext = LauguageTool.GetIns().GetText("设置轮子副标题");
            string djids = "1";
            Details.text = string.Format(detailtext, djids);
            tra = null;
            if (joyData.type == JockstickData.JockType.twoServo)
            {
                wheelType = 2;
                tra = settingUI2.transform.GetChild(1).GetChild(0);
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                wheelType = 4;
                tra = settingUI2.transform.GetChild(1).GetChild(1);
            }
            else if (joyData.type == JockstickData.JockType.treeServo)
            {
                wheelType = 3;
                tra = settingUI2.transform.GetChild(1).GetChild(2);
            }
            else
            { 
            
            }

            ShowSetting_2();

            tra.gameObject.SetActive(true);
            for (int i = 0; i < tra.childCount; i++)
            {
                if (i == 0) //1号轮子
                {
                    if (joyData.leftUpID != 0)
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoIcon@3x";
                        //tra.GetChild(0).GetChild(2).GetComponent<UISprite>().enabled = true;
                    }
                    else
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoBg2x";
                    }
                }
                else if (i == 1) // 2号轮子
                {
                    if (joyData.rightUpID != 0)
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoIcon@3x";
                    }
                    else
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoBg2x";
                    }
                }
                else if (i == 2)//3号轮子
                {
                    if (joyData.leftBottomID != 0)
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoIcon@3x";
                    }
                    else
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoBg2x";
                    }
                }
                else if (i == 3)//4号轮子
                {
                    if (joyData.rightBottomID != 0)
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoIcon@3x";
                    }
                    else
                    {
                        tra.GetChild(i).GetComponent<UISprite>().spriteName = "servoBg2x";
                    }
                }
            }
            //ShowDuojiList();
            //ShowServosState();
            //bottomTrans.gameObject.SetActive(false); //隐藏底部面板
        }   
    }

    public override void LoadUI()
    {
        base.LoadUI();
    }

    Transform curSelectWheel;
    int wheelID = 0; //正在设置的轮子id
    int SetWheelID
    {
        set
        {
            if (wheelID != value)
            {
                if (curSelectWheel != null)  //切换设置的轮子时 上一个选中状态cancel
                    curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = false;
            }
            wheelID = value;
            if (wheelID != 0) // wheelid 有设置时显示舵机列表
            {
                ShowDuojiList();
                ShowServosState(); // 对应的状态
            }
        }
    }
    /// <summary>
    /// 显示舵机列表
    /// </summary>
    void ShowDuojiList()
    {
        //Debug.Log("hasJoysickData is " + hasJoysickData);
        bottomTrans.gameObject.SetActive(true);
        UserdefControllerUI.HideOrShowTrans(true, bottomTrans, UserdefControllerUI.directType.bottom);
        //
    }
    /// <summary>
    /// 显示舵机列表下的状态
    /// </summary>
    void ShowServosState()
    {
        if (joyData == null)
            return;
        List<byte> takenlist = new List<byte>();
        if (joyData.leftUpID != 0)
            takenlist.Add(joyData.leftUpID);
        if (joyData.rightUpID != 0)
            takenlist.Add(joyData.rightUpID);
        if (joyData.leftBottomID != 0)
            takenlist.Add(joyData.leftBottomID);
        if (joyData.rightBottomID != 0)
            takenlist.Add(joyData.rightBottomID);
        if (joyData.UpID != 0)
            takenlist.Add(joyData.UpID); 
        byte selected = 0;
        if (joyData.type == JockstickData.JockType.treeServo && wheelID == 3)
        {
            if (joyData.UpID != 0)
                selected = joyData.UpID;
        }
        else
        {
            if (wheelID == 1)
                selected = joyData.leftUpID;
            else if (wheelID == 2)
                selected = joyData.rightUpID;
            else if (wheelID == 3)
                selected = joyData.leftBottomID;
            else if (wheelID == 4)
                selected = joyData.rightBottomID;
        }
        ShowTakenState(takenlist, selected);
    }
    /// <summary>
    /// 显示被占用以及当前被选中的状态
    /// </summary>
    void ShowTakenState(List<byte> takenList, byte selected = 0)
    {
        if (selectServo != null)
            selectServo.enabled = false;

        if (takenList == null || takenList.Count == 0)
            return;
        Transform grid = bottomTrans.GetChild(1).GetChild(0);
        //Transform marks = GameObject.Find("UI Root/mark0").transform;
        byte b = 0;
        char[] sp = new char[1];
        sp[0] = ' ';
        for (int i = 0; i < grid.childCount; i++)
        {
            byte.TryParse(grid.GetChild(i).GetComponentInChildren<UILabel>().text.Split(sp)[1], out b);
            grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
            grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
            grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
            grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
            grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
            if (takenList.Contains(b) && selected != b) //被占用的状态
            {
                Debug.Log("leftUpID is " + joyData.leftUpID);
                Debug.Log("rightUpID is " + joyData.rightUpID);
                Debug.Log("leftBottomID is " + joyData.leftBottomID);
                Debug.Log("rightBottomID is " + joyData.rightBottomID);

                if (joyData.leftUpID != 0 && joyData.leftUpID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = true;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    Debug.Log("舵机：" + b + "被占用状态");
                }
                else if (joyData.rightUpID != 0 && joyData.rightUpID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = true;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    Debug.Log("舵机：" + b + "被占用状态");
                }
                else if (joyData.leftBottomID != 0 && joyData.leftBottomID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = true;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    Debug.Log("舵机：" + b + "被占用状态");
                }
                else if (joyData.rightBottomID != 0 && joyData.rightBottomID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = true;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    Debug.Log("舵机：" + b + "被占用状态");
                }
                else
                {
                    Debug.Log("舵机：" + i + "未被激活");
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                }
                
                //grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = true;
                //selectServo = grid.GetChild(i).GetChild(2).GetComponent<UISprite>();
                
                //selectServo.enabled = false;
            }
            else if (selected == b)  //选中的状态 
            {
                //grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                if (joyData.leftUpID != 0 && joyData.leftUpID == b)
                {
                    selectServo = grid.GetChild(i).GetChild(2).GetComponent<UISprite>();
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    selectServo.enabled = true;
                }
                else if (joyData.rightUpID != 0 && joyData.rightUpID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    selectServo = grid.GetChild(i).GetChild(3).GetComponent<UISprite>();
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    selectServo.enabled = true;
                }
                else if (joyData.leftBottomID != 0 && joyData.leftBottomID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    selectServo = grid.GetChild(i).GetChild(4).GetComponent<UISprite>();
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    selectServo.enabled = true;
                }
                else if (joyData.rightBottomID != 0 && joyData.rightBottomID == b)
                {
                    grid.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    selectServo = grid.GetChild(i).GetChild(5).GetComponent<UISprite>();
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    selectServo.enabled = true;
                }
                else
                {
                    selectServo = grid.GetChild(i).GetChild(2).GetComponent<UISprite>();
                    grid.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                    grid.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                    selectServo.enabled = false;
                }
                //selectServo = grid.GetChild(i).GetChild(2).GetComponent<UISprite>();
                selectServoID = b;
                Debug.Log("舵机：" + selectServoID + "被选择状态");
                
            }
        }
    }

    bool isChange = false;
    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        string name = obj.name;
        if (name.Contains("backM"))  //返回 取消修改
        {
            UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
        }
        else if (name.Contains("backS"))
        {
            if (joyData.type == JockstickData.JockType.twoServo)
            {
                if (joyData.leftUpID == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
                else if (joyData.rightUpID == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
                else
                    ShowSetting_1();
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                if (joyData.leftUpID == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
                else if (joyData.rightUpID == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
                else if (joyData.leftBottomID == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
                else if (joyData.rightBottomID == 0)
                    UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("未完成配置提示"), AbandonSetting, isChange);
                else
                    ShowSetting_1();
            }

            //UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("not save wheel tip"), AbandonSetting, isChange);
            if (selectServo != null)
            {
                selectServo.enabled = true;
            }
            //UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("not save Controller tip"), OnSecondbackClicked, isTotalDataChange);
        }
        else if (name.Contains("save")) //保存数据
        {
            if (joyData.type == JockstickData.JockType.twoServo)
            {
                if (joyData.leftUpID == 0)
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("未完成配置提示"));
                else if (joyData.rightUpID == 0)
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("未完成配置提示"));
                else
                {
                    if (isChange)
                    {
                        CopyData((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID), joyData);  //确定修改， 
                        UserdefControllerUI.isTotalDataChange = true;
                    }
                    UserdefControllerScene.Ins.CloseJoystickSettingUI();
                    UserdefControllerScene.Ins.BackControllerSettingUI();
                }
                    //ShowSetting_1();
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                if (joyData.leftUpID == 0)
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("未完成配置提示"));
                else if (joyData.rightUpID == 0)
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("未完成配置提示"));
                else if (joyData.leftBottomID == 0)
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("未完成配置提示"));
                else if (joyData.rightBottomID == 0)
                    HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("未完成配置提示"));
                else
                {
                    if (isChange)
                    {
                        CopyData((JockstickData)ControllerManager.GetInst().GetWidgetdataByID(widgetID), joyData);  //确定修改， 
                        UserdefControllerUI.isTotalDataChange = true;
                    }
                    UserdefControllerScene.Ins.CloseJoystickSettingUI();
                    UserdefControllerScene.Ins.BackControllerSettingUI();
                }
                    //ShowSetting_1();
            }
        }
        else if (name.Contains("type1"))  //双轮模式
        {
            selectWheelID = 1;
            Debug.Log("two-wheels");
            wheelType = 2;
            if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 2)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
            }
            else
            {
                ShowSetting_2();
                if (joyData.type != JockstickData.JockType.twoServo)
                {
                    joyData.type = JockstickData.JockType.twoServo;
                    joyData.leftUpID = 0;
                    joyData.rightUpID = 0;
                    joyData.UpID = 0;
                    joyData.leftBottomID = 0;
                    joyData.rightBottomID = 0;

                    selectWheelID = 1;

                    Transform tra1 = settingUI2.transform.GetChild(1).GetChild(0);

                    if (tra1 != null)
                    {
                        //tra1.GetChild(0).GetChild(2).GetComponent<UISprite>().enabled = true;
                        for (int i = 0; i < tra1.childCount; i++)
                        {
                            if (tra1.GetChild(i).GetComponent<UISprite>() != null && tra1.GetChild(i).GetComponent<UISprite>().spriteName == "servoIcon@3x")
                                tra1.GetChild(i).GetComponent<UISprite>().spriteName = "servoBg2x";
                        }
                        //tra1.GetChild(0).GetChild(2).gameObject.SetActive(true);
                    }

                    isChange = true;

                    Transform grid1 = bottomTrans.GetChild(1).GetChild(0);

                    if (grid1 != null)
                    {
                        for (int i = 0; i < grid1.childCount; i++)
                        {
                                grid1.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                                grid1.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                                grid1.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                                grid1.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                                grid1.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                        }
                    }

                    if (selectServo != null)
                    {
                        selectServo.enabled = false;
                    }
                }
                else
                {
                    if (selectServo != null)
                    {
                        selectServo.enabled = true;
                    }
                }
                settingUI2.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            }
        }
        else if (name.Contains("type2")) //四轮模式
        {
            selectWheelID = 1;
            Debug.Log("four-wheels");
            wheelType = 4;
            if (RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList().Count < 4)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("轮模式舵机数量不足"));
            }
            else
            {
                ShowSetting_2();
                if (joyData.type != JockstickData.JockType.fourServo)
                {
                    joyData.type = JockstickData.JockType.fourServo;
                    joyData.leftUpID = 0;
                    joyData.rightUpID = 0;
                    joyData.UpID = 0;
                    joyData.leftBottomID = 0;
                    joyData.rightBottomID = 0;

                    selectWheelID = 1;

                    Transform tra2 = settingUI2.transform.GetChild(1).GetChild(1);

                    if (tra2 != null)
                    {
                        //Debug.Log("四轮模式预配置！！");
                        //tra2.GetChild(0).GetChild(2).GetComponent<UISprite>().enabled = true;

                        for (int i = 0; i < tra2.childCount; i++)
                        {
                            if (tra2.GetChild(i).GetComponent<UISprite>() != null && tra2.GetChild(i).GetComponent<UISprite>().spriteName == "servoIcon@3x")
                                tra2.GetChild(i).GetComponent<UISprite>().spriteName = "servoBg2x";
                        }
                        //tra2.GetChild(0).GetChild(2).gameObject.SetActive(true);
                    }


                    isChange = true;

                    Transform grid2 = bottomTrans.GetChild(1).GetChild(0);

                    if (grid2 != null)
                    {
                        for (int i = 0; i < grid2.childCount; i++)
                        {
                            grid2.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                            grid2.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                            grid2.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                            grid2.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                            grid2.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
                        }
                    }

                    //ShowDuojiList(false);
                    if (selectServo != null)
                    {
                        selectServo.enabled = false;
                    }
                }
                else
                {
                    if (selectServo != null)
                    {
                        selectServo.enabled = true;
                    }
                }
                settingUI2.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
            }
        }
        else if (name.Contains("type3"))  //三轮模式
        {
            ShowSetting_2();
            if (joyData.type != JockstickData.JockType.treeServo)
            {
                joyData.type = JockstickData.JockType.treeServo;
                joyData.leftUpID = 0;
                joyData.rightUpID = 0;
                joyData.UpID = 0;
                joyData.leftBottomID = 0;
                joyData.rightBottomID = 0;
                isChange = true;
            }
            settingUI2.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
        }
        else if (name.Contains("wheel1"))   // 前左 点击后记录当前设置的轮子，同时背景选中状态
        {
            Debug.Log("wheel1 is selected");
            SetWheelID = 1;
            selectWheelID = 1;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "1");
            curSelectWheel = obj.transform;
            curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("wheel2"))  //前右
        {
            Debug.Log("wheel2 is selected");
            SetWheelID = 2;
            selectWheelID = 2;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "2");
            curSelectWheel = obj.transform;
            curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("wheel3"))   //后左 或第三个轮子
        {
            Debug.Log("wheel3 is selected");
            SetWheelID = 3;
            selectWheelID = 3;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "3");
            curSelectWheel = obj.transform;
            curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("wheel4"))   //后右
        {
            Debug.Log("wheel4 is selected");
            SetWheelID = 4;
            selectWheelID = 4;
            Details.enabled = true;
            Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "4");
            curSelectWheel = obj.transform;
            curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = true;
        }
        else if (name.Contains("servo_"))  //选择对应的舵机 servo_num 
        {
            UILabel text = obj.GetComponentInChildren<UILabel>();
            if (text != null)
            {
                if (obj.transform.GetChild(2).GetComponent<UISprite>().enabled || obj.transform.GetChild(3).GetComponent<UISprite>().enabled
                    || obj.transform.GetChild(4).GetComponent<UISprite>().enabled || obj.transform.GetChild(5).GetComponent<UISprite>().enabled)
                {
                    curSelectServo = true;
                    //obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                }
                else
                {
                    curSelectServo = false;
                }
                char[] sp = new char[1];
                sp[0] = ' ';
                selectServoID = byte.Parse(text.text.Split(sp)[1]);
                Debug.Log("curSelectServo is " + curSelectServo);
                Debug.Log("curSelectedServo is " + selectServoID);

                if (curSelectServo)
                {
                    curSelectServo = false;
                    Debug.Log("curSelectServo is true!!");
                    if (joyData.type == JockstickData.JockType.twoServo)
                    {
                        tra = settingUI2.transform.GetChild(1).GetChild(0);
                    }
                    else if (joyData.type == JockstickData.JockType.fourServo)
                    {
                        tra = settingUI2.transform.GetChild(1).GetChild(1);
                    }
                    if (joyData.leftUpID != 0 && joyData.leftUpID == selectServoID)
                    {
                        tra.GetChild(0).GetComponent<UISprite>().spriteName = "servoBg2x";
                        joyData.leftUpID = 0;
                    }
                    else if (joyData.rightUpID != 0 && joyData.rightUpID == selectServoID)
                    {
                        tra.GetChild(1).GetComponent<UISprite>().spriteName = "servoBg2x";
                        joyData.rightUpID = 0;
                    }
                    else if (joyData.leftBottomID != 0 && joyData.leftBottomID == selectServoID)
                    {
                        tra.GetChild(2).GetComponent<UISprite>().spriteName = "servoBg2x";
                        joyData.leftBottomID = 0;
                    }
                    else if (joyData.rightBottomID != 0 && joyData.rightBottomID == selectServoID)
                    {
                        tra.GetChild(3).GetComponent<UISprite>().spriteName = "servoBg2x";
                        joyData.rightBottomID = 0;
                    }
                }
                if (selectWheelID == 1)
                {
                    //obj.transform.GetChild(2).GetComponent<UISprite>().enabled = true;
                    obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(5).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(6).GetComponent<UISprite>().enabled = false;

                    selectServo = obj.transform.GetChild(2).GetComponent<UISprite>();
                }
                else if (selectWheelID == 2)
                {
                    obj.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
                    //obj.transform.GetChild(3).GetComponent<UISprite>().enabled = true;
                    obj.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(5).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(6).GetComponent<UISprite>().enabled = false;

                    selectServo = obj.transform.GetChild(3).GetComponent<UISprite>();
                }
                else if (selectWheelID == 3)
                {
                    obj.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                    //obj.transform.GetChild(4).GetComponent<UISprite>().enabled = true;
                    obj.transform.GetChild(5).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(6).GetComponent<UISprite>().enabled = false;

                    selectServo = obj.transform.GetChild(4).GetComponent<UISprite>();
                }
                else if (selectWheelID == 4)
                {
                    obj.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                    obj.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                    //obj.transform.GetChild(5).GetComponent<UISprite>().enabled = true;
                    obj.transform.GetChild(6).GetComponent<UISprite>().enabled = false;

                    selectServo = obj.transform.GetChild(5).GetComponent<UISprite>();
                }
                SetWheelServoID(byte.Parse(text.text.Split(sp)[1]));
            }
            if (selectServo != null)  //选中模式
            {
                selectServo.enabled = true;
            }
            Debug.Log("selectWheelID is " + selectWheelID);

            ShowServosState();
            
            //selectServo = obj.transform.GetChild(2).GetComponent<UISprite>();
            
        }
    }
   
    /// <summary>
    /// 丢弃修改
    /// </summary>
    /// <param name="obj"></param>
    void AbandonSetting(GameObject obj)
    {
        try
        {
            if (obj == null) //没有改动时直接退出
            {
                UserdefControllerScene.Ins.CloseJoystickSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
                return;
            }
            string name = obj.name;
            if (name.Equals(PromptMsg.RightBtnName)) //确定丢弃修改
            {
                UserdefControllerScene.Ins.CloseJoystickSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
            }
        }
        catch (System.Exception ex)
        {}
    }
    /// <summary>
    /// 通过结构和轮子设定对应的舵机id
    /// </summary>
    /// <param name="id"></param>
    void SetWheelServoID(int id)
    {
        if (wheelID == 1)
        {
            if (joyData.leftUpID != (byte)id)
            {
                isChange = true;
                joyData.leftUpID = (byte)id;
            }
        }
        else if (wheelID == 2)
        {
            if (joyData.rightUpID != (byte)id)
            {
                isChange = true;
                joyData.rightUpID = (byte)id;
            }
        }
        else if (wheelID == 3 && joyData.type == JockstickData.JockType.fourServo)
        {
            if (joyData.leftBottomID != (byte)id)
            {
                isChange = true;
                joyData.leftBottomID = (byte)id;
            }
        }
        else if (wheelID == 3 && joyData.type == JockstickData.JockType.treeServo)
        {
            if(joyData.UpID != (byte)id)
            {
                isChange = true;
                joyData.UpID = (byte)id;
            }
        }
        else if (wheelID == 4)
        {
            if (joyData.rightBottomID != (byte)id)
            {
                isChange = true;
                joyData.rightBottomID = (byte)id;
            }
        }
        if (curSelectWheel != null)
        {
            curSelectWheel.GetComponent<UISprite>().spriteName = "servoIcon@3x";
        }
    }
    void ShowSetting_1()
    {
        for (int i = 0; i < settingUI2.transform.GetChild(1).childCount; i++)
        {
            settingUI2.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
        }
        settingUI2.SetActive(false);
        settingUI1.SetActive(true);
        Title.text = LauguageTool.GetIns().GetText("select model type");
    }
    void ShowSetting_2()
    {
        settingUI2.SetActive(true);
        settingUI1.SetActive(false);
        Title.text = LauguageTool.GetIns().GetText("设置轮子主标题");

        selectWheelID = 1;

        if (wheelType == 2)
        {
            Transform wheelOne = settingUI2.transform.GetChild(1).GetChild(0).GetChild(0);
            SetWheelID = 1;
            curSelectWheel = wheelOne;
            curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = true;
        }
        else if (wheelType == 4)
        {
            Transform wheelTwo = settingUI2.transform.GetChild(1).GetChild(1).GetChild(0);
            SetWheelID = 1;
            curSelectWheel = wheelTwo;
            curSelectWheel.GetChild(curSelectWheel.childCount - 1).GetComponent<UISprite>().enabled = true;
        }
        
        ShowDuojiList();
        ShowServosState(); // 对应的状态
        Details.enabled = true;
        Details.text = string.Format(LauguageTool.GetIns().GetText("设置轮子副标题"), "1");
    }
}
