using UnityEngine;
using Game.Scene;
using System.Collections;
using System.Collections.Generic;

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
    public UserdefControllerScene()
    {
        mUIList = new System.Collections.Generic.List<BaseUI>();
        Ins = this;
    }

    /// <summary>
    /// 创建新遥控器
    /// </summary>
    /// <param name="name"></param>
    public void CreateNewController(string name)
    {
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
        mUIList.Remove(joySettingUI);
        joySettingUI.OnClose();
    }
    /// <summary>
    /// 打开滑竿设置页面
    /// </summary>
    /// <param name="id"></param>
    public void OpenVsliderSettingUI(string id)
    {
        vsliderSettingUI = new VsliderSettingUI(id);
        mUIList.Add(vsliderSettingUI);
        controllerUI.OnHide();
        UpdateScene();
    }
    /// <summary>
    /// 关闭滑竿设置页面
    /// </summary>
    public void CloseVsliderSettingUI()
    {
        mUIList.Remove(vsliderSettingUI);
        vsliderSettingUI.OnClose();

    }

    public void BackControllerSettingUI()
    {
        if (controllerUI != null)
            controllerUI.OnShow();
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
                    oo.GetComponentInChildren<UILabel>().text = "Servo " + turnList[i].ToString();//djl[i].ToString();
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
                    oo.GetComponentInChildren<UILabel>().text = "Servo " + engelList[i].ToString();//djl[i].ToString();
                    oo.GetComponent<UISprite>().spriteName = "servoIconN3x";
                    oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_jdN3x";
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
                    oo.GetComponentInChildren<UILabel>().text = "Servo " + engelList[i].ToString();//djl[i].ToString();
                    oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_jdS3x";
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
                    oo.GetComponentInChildren<UILabel>().text = "Servo " + turnList[i].ToString();//djl[i].ToString();
                    oo.GetComponent<UISprite>().spriteName = "servoIconN3x";
                    oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_lN3x";
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
    public static void InitServoListV(Transform p, ButtonDelegate.OnClick call = null)
    {
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
                oo.GetComponentInChildren<UILabel>().text = "Servo " + engelList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoIconN3x";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_jdN3x";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = null;
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
                oo.GetComponentInChildren<UILabel>().text = "Servo " + turnList[i].ToString();//djl[i].ToString();
                oo.GetComponent<UISprite>().spriteName = "servoIcon@3x";
                oo.transform.GetChild(0).GetComponent<UISprite>().spriteName = "servoModel_lS3x";
                ButtonDelegate del = new ButtonDelegate();
                if (call != null)
                    del.onClick = call;
                GetTCompent.GetCompent<ButtonEvent>(oo.transform).SetDelegate(del);
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
