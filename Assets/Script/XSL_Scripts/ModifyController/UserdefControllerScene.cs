using UnityEngine;
using Game.Scene;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;

public class UserdefControllerScene : BaseScene {

    public static UserdefControllerScene Ins;
    public static int leftSpace = 20;  //左边距
    public static int rightSpace = 20; //右边距
    public static int upSpace = 20;    //上边距 
    public static int downSpace = 20;  //下边距 
    static string controllerName;
    string controllerUIpath;  //ui场景路径
    private UserdefControllerUI controllerUI;
    private UserCotrollerSettingUI setUI;
    private JoystickSettingUI joySettingUI;
    private VsliderSettingUI vsliderSettingUI;
    private HsliderSettingUI hsliderSettingUI;
    private int vsliderServoID;
    private int hsliderServoID;
    private string selectWidgetID;
    public UserdefControllerScene()
    {
        mUIList = new System.Collections.Generic.List<BaseUI>();
        Ins = this;
    }

    public enum curControlT
    {
        joystick_w,
        vslider_sw,
        vslider_cw,
        hslider_sw,
        hslider_cw,
    }

    /// <summary>
    /// 创建新遥控器
    /// </summary>
    /// <param name="name"></param>
    public void CreateNewController(string name)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Create new controller!!");

        if (setUI != null)
            mUIList.Remove(setUI);
        if (controllerUI == null)
        {
            //Debug.Log("isFirstSetting is true");
            controllerUI = new UserdefControllerUI(true);
          //  controllerUI.IsSetting = true;
            controllerUI.isFirstSetting = true;
            mUIList.Add(controllerUI);
            
         //   controllerUI.IsSetting = true;
        }
        UpdateScene();
    }
    /// <summary>
    /// 打开摇杆设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenJoystickSettingUI(string id)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Open joystick setting UI!!");

        joySettingUI = new JoystickSettingUI(id);
        if (joySettingUI.JoyData.type == JockstickData.JockType.none) //widget 为null
        {
            joySettingUI.isFirstSetting = true;
        }
        else  //widget不为null
        {
            joySettingUI.isFirstSetting = false;
        }
        mUIList.Add(joySettingUI);
        controllerUI.OnHide();
        UpdateScene();
    }
    /// <summary>
    /// 关闭摇杆设置页面
    /// </summary>
    public void CloseJoystickSettingUI()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Close joystick setting UI!!");

        mUIList.Remove(joySettingUI);
        joySettingUI.OnClose();
    }
    /// <summary>
    /// 打开滑杆设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenVsliderSettingUI(string id)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Open vslider setting UI!!");

        vsliderSettingUI = new VsliderSettingUI(id);
        mUIList.Add(vsliderSettingUI);
        controllerUI.OnHide();
        UpdateScene();
    }
    /// <summary>
    /// 关闭滑杆设置页面
    /// </summary>
    public void CloseVsliderSettingUI()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Close vslider setting UI!!");

        selectWidgetID = vsliderSettingUI.sliderData.widgetId;
        /*if (vsliderSettingUI.isSelectOtherServo)
            vsliderServoID = vsliderSettingUI.sliderData.servoID;
        else
            vsliderServoID = 0;*/
        vsliderServoID = vsliderSettingUI.sliderData.servoID;
        mUIList.Remove(vsliderSettingUI);
        vsliderSettingUI.OnClose();
    }
    /// <summary>
    /// 打开横杆设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenHsliderSettingUI(string id)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Open hslider setting UI!!");

        hsliderSettingUI = new HsliderSettingUI(id);
        mUIList.Add(hsliderSettingUI);
        controllerUI.OnHide();
        UpdateScene();
    }
    /// <summary>
    /// 关闭横杆设置页面
    /// </summary>
    public void CloseHsliderSettingUI()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Close hslider setting UI!!");

        selectWidgetID = hsliderSettingUI.hsliderData.widgetId;
        /*if (hsliderSettingUI.isSelectOtherServo)
            hsliderServoID = hsliderSettingUI.hsliderData.servoID;
        else
            hsliderServoID = 0;*/
        hsliderServoID = hsliderSettingUI.hsliderData.servoID;
        mUIList.Remove(hsliderSettingUI);
        hsliderSettingUI.OnClose();
    }

    public void BackControllerSettingUI(curControlT controlT)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Back controller setting scene!!");

        if (controllerUI != null)
            controllerUI.OnShow();
        //Debug.Log("ReEnter is Controller");
        Transform gridPanelC = GameObject.Find("userdefineControllerUI/Center/gridPanel").transform;

        if (controlT == curControlT.vslider_sw || controlT == curControlT.hslider_sw)
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("配置成功提示"), HUDTextTips.Color_Green);

        // 竖杆操作
        if (gridPanelC != null && gridPanelC.childCount > 1 && vsliderSettingUI != null && controlT == curControlT.vslider_sw)
        {
            for (int i = 1; i < gridPanelC.childCount; i++)
            {
                if (gridPanelC.GetChild(i).tag.Contains("widget_vslider") && gridPanelC.GetChild(i).name == selectWidgetID)
                {
                    gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("舵机") + " " + vsliderServoID.ToString();

                    if (((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(i).name)).directionDisclock)
                    {
                        gridPanelC.GetChild(i).GetChild(5).GetComponent<UISprite>().spriteName = "nishi";
                        gridPanelC.GetChild(i).GetChild(6).GetComponent<UISprite>().spriteName = "shunshi";
                    }
                    else
                    {
                        gridPanelC.GetChild(i).GetChild(5).GetComponent<UISprite>().spriteName = "shunshi";
                        gridPanelC.GetChild(i).GetChild(6).GetComponent<UISprite>().spriteName = "nishi";
                    }
                    
                    for (int j = 1; j < gridPanelC.childCount; j++)
                    {
                        if (gridPanelC.GetChild(j).tag.Contains("widget_vslider") && gridPanelC.GetChild(j).name != selectWidgetID && gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text == gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text)
                        {
                            gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text = "";
                            ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).servoID = 0;
                            ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).directionDisclock = true;
                        }        
                    }
                }
            }
        }
        // 横杆操作
        if (gridPanelC != null && gridPanelC.childCount > 1 && hsliderSettingUI != null && controlT == curControlT.hslider_sw)
        {
            for (int i = 1; i < gridPanelC.childCount; i++)
            {
                if (gridPanelC.GetChild(i).tag.Contains("widget_hslider") && gridPanelC.GetChild(i).name == selectWidgetID)
                {
                    gridPanelC.GetChild(i).GetChild(1).GetChild(0).transform.localPosition = Vector3.zero;
                    gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("舵机") + " " + hsliderServoID.ToString();
                    gridPanelC.GetChild(i).GetChild(5).GetComponent<UILabel>().text = hsliderSettingUI.hsliderData.min_angle.ToString() + "°";
                    gridPanelC.GetChild(i).GetChild(6).GetComponent<UILabel>().text = hsliderSettingUI.hsliderData.max_angle.ToString() + "°";
                    for (int j = 1; j < gridPanelC.childCount; j++)
                    {
                        if (gridPanelC.GetChild(j).tag.Contains("widget_hslider") && gridPanelC.GetChild(j).name != selectWidgetID && gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text == gridPanelC.GetChild(i).GetChild(4).GetComponent<UILabel>().text)
                        {
                            gridPanelC.GetChild(j).GetChild(4).GetComponent<UILabel>().text = "";
                            gridPanelC.GetChild(j).GetChild(1).GetChild(0).transform.localPosition = Vector3.zero;
                            ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).servoID = 0;
                            ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).min_angle = -118;
                            ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).max_angle = 118;
                            gridPanelC.GetChild(j).GetChild(5).GetComponent<UILabel>().text = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).min_angle.ToString() + "°";
                            gridPanelC.GetChild(j).GetChild(6).GetComponent<UILabel>().text = ((HSliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(gridPanelC.GetChild(j).name)).max_angle.ToString() + "°";
                        }
                    }
                }
            }
        }
        UpdateScene();
    }

    /// <summary>
    /// 场景更新
    /// </summary>
    public override void UpdateScene()
    {
        base.UpdateScene();
        //if(controllerUI != null)
        //    controllerUI.Open();
        foreach (var tem in mUIList)
        {
            tem.Open();
        }
    }
    public override void Open()
    {
        //mUIList.Clear();
        if (ControllerManager.IsControllersNull(RobotManager.GetInst().GetCurrentRobot().ID)) //没有遥控器的时候 设置界面
        {
            //Debug.Log("Open Setting");
            //CreateNewController("");  //创建一个没命名的遥控器
            setUI = new UserCotrollerSettingUI();
            if (setUI != null)
            {
                mUIList.Add(setUI);
            }   
        }
        else //进入遥控器列表界面
        {
            //Debug.Log("Restart Setting");
            controllerUI = new UserdefControllerUI(false);
            if (controllerUI != null)
            {
                mUIList.Add(controllerUI);
            }            
        }
        base.Open();
       // ControllerManager.WriteControllerByID();
    }
    public override void Close()
    {
        base.Close();
        //释放遥控器数据 同时停止所有可能的动作
        ControllerManager.GetInst().CleanUp();
    }

    #region  public static
    /// <summary>
    /// 初始化舵机列表（摇杆）
    /// </summary>
    public static void InitServoList(Transform p, ButtonDelegate.OnClick call = null,bool isTurnFirst = true)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Init joystick servo list!!");

        GameObject obj = Resources.Load<GameObject>("prefabs/servo_");
        List<byte> turnList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();
        if (isTurnFirst)
        {
            //Debug.Log("轮模式优先");
            if (turnList.Count > 0)
            {
                //Debug.Log("轮列表非空");
                for (int i = 0; i < turnList.Count; i++)
                {
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.transform.SetParent(p);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = Vector3.zero;
                    oo.GetComponentInChildren<UILabel>().text = turnList[i].ToString();//djl[i].ToString();
                    ButtonDelegate del = new ButtonDelegate();
                    if (call != null)
                        del.onClick = call;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
            if (engelList.Count > 0)
            {
                //Debug.Log("角度列表非空");
                for (int i = 0; i < engelList.Count; i++)
                {
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.transform.SetParent(p);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = Vector3.zero;
                    oo.GetComponentInChildren<UILabel>().text = engelList[i].ToString();//djl[i].ToString();
                    oo.GetComponent<UISprite>().spriteName = "servoAngleNo";
                    oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "Clean";
                    ButtonDelegate del = new ButtonDelegate();
                    if (call != null)
                        del.onClick = null;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
        }
        else
        {
            //Debug.Log("角度模式优先");
            if (engelList.Count > 0)
            {
                //Debug.Log("角度列表非空");
                for (int i = 0; i < engelList.Count; i++)
                {
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.transform.SetParent(p);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = Vector3.zero;
                    oo.GetComponentInChildren<UILabel>().text = engelList[i].ToString();//djl[i].ToString();
                    oo.GetComponent<UISprite>().spriteName = "servoAngleEnable";
                    oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "Clean";
                    ButtonDelegate del = new ButtonDelegate();
                    if (call != null)
                        del.onClick = call;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
            if (turnList.Count > 0)
            {
                //Debug.Log("轮列表非空");
                for (int i = 0; i < turnList.Count; i++)
                {
                    GameObject oo = GameObject.Instantiate(obj) as GameObject;
                    oo.transform.SetParent(p);
                    oo.transform.localScale = Vector3.one;
                    oo.transform.localPosition = Vector3.zero;
                    oo.GetComponentInChildren<UILabel>().text = turnList[i].ToString();//djl[i].ToString();
                    oo.GetComponent<UISprite>().spriteName = "servoTurnNo";
                    oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "Clean";
                    ButtonDelegate del = new ButtonDelegate();
                    if (call != null)
                        del.onClick = call;
                    GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
                }
            }
        }
        p.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 初始化舵机列表（竖杆）
    /// </summary>
    public static void InitServoListV(Transform p, int curServo, ButtonDelegate.OnClick call = null)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Init vslider servo list!!");

        GameObject obj = Resources.Load<GameObject>("prefabs/servo_");
        
        List<byte> turnList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();

        //Debug.Log("轮列表 is "+turnList.Count);
        //Debug.Log("角度列表 is " + engelList.Count);
        
        if (engelList.Count > 0)
        {
            //Debug.Log("竖杆角度列表非空");
            for (int i = 0; i < engelList.Count; i++)
            {
                /*GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.GetComponentInChildren<UILabel>().text = "Servo " + engelList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoAngleNo";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_jdN3x";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = null;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);*/
             }
        }
        if (turnList.Count > 0)
        {
            //Debug.Log("竖杆轮列表非空");
            for (int i = 0; i < turnList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.GetComponentInChildren<UILabel>().text = turnList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoTurnEnable";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "Clean";
                oo.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(5).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(6).GetComponent<UISprite>().enabled = false;

                if (oo.transform.GetComponentInChildren<UILabel>().text == curServo.ToString())
                {
                    oo.transform.GetChild(6).GetComponent<UISprite>().enabled = true;
                }
                    
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        p.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 初始化舵机列表（横杆）
    /// </summary>
    public static void InitServoListH(Transform p, int curServo, ButtonDelegate.OnClick call = null)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Init hslider servo list!!");

        GameObject obj = Resources.Load<GameObject>("prefabs/servo_");
        List<byte> turnList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetTurnList();
        List<byte> engelList = RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetAngleList();

        if (engelList.Count > 0)
        {
            //Debug.Log("角度列表非空");
            for (int i = 0; i < engelList.Count; i++)
            {
                GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.GetComponentInChildren<UILabel>().text = engelList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoAngleEnable";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "Clean";
                oo.transform.GetChild(2).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(3).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(4).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(5).GetComponent<UISprite>().enabled = false;
                oo.transform.GetChild(6).GetComponent<UISprite>().enabled = false;

                if (oo.transform.GetComponentInChildren<UILabel>().text == curServo.ToString())
                {
                    oo.transform.GetChild(6).GetComponent<UISprite>().enabled = true;
                }
                    
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
            }
        }
        if (turnList.Count > 0)
        {
            //Debug.Log("轮列表非空");
            for (int i = 0; i < turnList.Count; i++)
            {
                /*GameObject oo = GameObject.Instantiate(obj) as GameObject;
                oo.transform.SetParent(p);
                oo.transform.localScale = Vector3.one;
                oo.transform.localPosition = Vector3.zero;
                oo.GetComponentInChildren<UILabel>().text = "Servo " + turnList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoTurnNo";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_lN3x";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = null;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);*/
            }
        }
        p.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 确认取消的提示框 如果数据未发生改变时 不弹出提示框
    /// </summary>
    /// <param name="str"></param>
    /// <param name="onclick"></param>
    public static void PopWin(string str, ButtonDelegate.OnClick onclick,bool isChange)
    {
        if (isChange)
        {
            PublicPrompt.ShowDelateWin(str, onclick);
        }
        else
        {
            onclick(null);
        }
    }
    #endregion
}
