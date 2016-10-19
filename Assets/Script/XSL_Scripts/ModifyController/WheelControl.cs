using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;
using Game.Event;
/// <summary>
/// author: xiongsonglin
/// describe:控件管理类
/// time: 
/// </summary>
public class WidgetManager
{
    public JoyStickContrlManager joyStickManager;
    public VSliderControlManager vSliderManager;
    public static int[] speedArray = new int[] { 0x0080, 0x00EA, 0x0154, 0x01BE, 0x0228, 0x0292 };  //速度档位

    public WidgetManager(ControllerData data)
    {
        if (data != null)
        {
            joyStickManager = new JoyStickContrlManager(data.GetJockList());
            vSliderManager = new VSliderControlManager(data.GetSliderList());
        }
    }

    public void ClearUp()
    {
        joyStickManager.ClearUp();
        vSliderManager.ClearUp();
    }

    /// <summary>
    /// 新增摇杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddJoystickControl(JockstickData data)
    {
        joyStickManager.AddNewStickcontrol(data);
    }
    /// <summary>
    /// 删除摇杆控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void RemoveJoystickControl(string widgetID)
    {
        joyStickManager.RemoveStickcontrol(widgetID);
    }
    /// <summary>
    /// 新增滑竿控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void AddVsliderControl(SliderWidgetData data)
    {
        vSliderManager.AddNewStickcontrol(data);
    }
    /// <summary>
    /// 删除滑竿控件
    /// </summary>
    /// <param name="widgetID"></param>
    public void RemoveVsliderControl(string widgetID)
    {
        vSliderManager.RemoveStickcontrol(widgetID);
    }
    /// <summary>
    /// 新增动作数据
    /// </summary>
    /// <param name="data"></param>
    public void AddActionControl(ActionWidgetData data)
    {
        
    }
    public void RemoveActionControl(string widgetID)
    { }
  //  public void Init()
}

/// <summary>
/// 摇杆控制管理类
/// </summary>
public class JoyStickContrlManager
{
    List<JoyStickControl> joystickList;

    public JoyStickControl GetJoystickByID(string ID)
    {
        if (joystickList != null)
            return joystickList.Find((x) => x.ID == ID);
        else
            return null;
    }

    public void ClearUp()
    {
        joystickList = new List<JoyStickControl>();
    }

    public int JoystickNum()
    {
        return joystickList.Count;
    }

    public JoyStickContrlManager(List<JockstickData> jockdata)
    {
        joystickList = new List<JoyStickControl>(jockdata.Count);
        for (int i = 0; i < jockdata.Count; i++)
        {
            joystickList.Add(new JoyStickControl(jockdata[i]));
        }
    }
    /// <summary>
    /// 新增摇杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddNewStickcontrol(JockstickData data)
    {
        joystickList.Add(new JoyStickControl(data));
    }
    /// <summary>
    /// 删除摇杆控件
    /// </summary>
    /// <param name="widgetid"></param>
    public void RemoveStickcontrol(string widgetid)
    {
        joystickList.Remove(GetJoystickByID(widgetid));
    }
    /// <summary>
    /// 准备就绪
    /// </summary>
    public void ReadyJockControl()
    {
        if (joystickList != null)
        {
            foreach (var tem in joystickList)
            {
                if (tem.joyData != null)
                    tem.isRightSetting = tem.joyData.isOK;
                else
                    tem.isRightSetting = false;
            }
        }
    }
}

/// <summary>
/// 摇杆控制类
/// author:xsl
/// description:
/// </summary>
public class JoyStickControl 
{
    public bool isReady;  //摇杆控件是否可以控制 蓝牙ok，配置ok
    public string ID;  //控件的id 
    public JockstickData joyData;
    private TurnData preLeftWheel;
    private TurnData preRightWheel;
    private TurnData leftWheel;
    private TurnData rightWheel;
    public bool isRightSetting;
    
    private int _leftWheelSpeed1;
    private int _leftWheelSpeed2;
    private int _rightWheelSpeed1;
    private int _rightWheelSpeed2;
    
    // 速度频繁改变时 不予处理
    float leftTime;
    float rightTime;
    //public static bool leftFlag = false;     
    //public static bool rightFlag = false;
    public bool leftFlag = false;     //两个控件的时间过滤不能相互影响 所以要非静态
    public bool rightFlag = false;

    private int leftWheelSpeed1
    {
        get
        {
            return _leftWheelSpeed1;
        }
        set
        {
            if (joyData.type != JockstickData.JockType.treeServo)  //非三轮模式
            {
                if (value != _leftWheelSpeed1 && value != 0) //发生改变 发送命令
                {
                    if (value > 0)
                        leftWheel.turnDirection = TurnDirection.turnByClock;
                    else
                        leftWheel.turnDirection = TurnDirection.turnByDisclock;
                    leftWheel.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];

                    leftFlag = true;
                }
                else if (value != 0) //不发送指令
                {

                }
                else //发送停止指令
                {
                    leftWheel.turnSpeed = 0;
                    leftWheel.turnDirection = TurnDirection.turnStop;

                    leftFlag = true;
                }
            }
            _leftWheelSpeed1 = value;
        }
    }
    private int rightWheelSpeed1
    {
        get
        {
            return _rightWheelSpeed1;
        }
        set
        {
            if (joyData.type != JockstickData.JockType.treeServo) //非三轮模式
            {
                if (value != _rightWheelSpeed1 && value != 0) //发生改变
                {
                    if (value > 0)
                        rightWheel.turnDirection = TurnDirection.turnByClock;
                    else
                        rightWheel.turnDirection = TurnDirection.turnByDisclock;
                    rightWheel.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];

                    rightFlag = true;
                }
                else if (value != 0) //不发送指令
                {
                    //   Debug.Log("右轮速度保持不变！");
                }
                else  //发送停止指令
                {
                    rightWheel.turnSpeed = 0;
                    rightWheel.turnDirection = TurnDirection.turnStop;

                    rightFlag = true;
                }
            }
            _rightWheelSpeed1 = value;
        }
    }
    /// <summary>
    /// 默认左轮id为1，右轮id为2
    /// </summary>
    public JoyStickControl(JockstickData data)
    {
        this.joyData = data;
        this.ID = data.widgetId;
    }

    /// <summary>
    /// 左轮的档位值
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnLeftwheelSpeed(float x, float y, int r)
    {
        float a = (Mathf.Abs(x) - x - 2 * y)/(2*r);
        if (a < 0.1f && a > -0.1f)
        {
            leftWheelSpeed1 = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            leftWheelSpeed1 = 1*zf;
        else if (aa < 0.5f)
            leftWheelSpeed1 = 2*zf;
        else if (aa < 0.7f)
            leftWheelSpeed1 = 3*zf;
        else if (aa < 0.9f)
            leftWheelSpeed1 = 4*zf;
        else
            leftWheelSpeed1 = 5*zf;
    }
    /// <summary>
    /// 右轮的档位值
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnRightwheelSpeed(float x, float y, int r)
    {
        float a = (2 * y - x - Mathf.Abs(x))/(2*r);
        if (a < 0.1f && a > -0.1f)
        {
            rightWheelSpeed1 = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            rightWheelSpeed1 = 1*zf;
        else if (aa < 0.5f)
            rightWheelSpeed1 = 2*zf;
        else if (aa < 0.7f)
            rightWheelSpeed1 = 3*zf;
        else if (aa < 0.9f)
            rightWheelSpeed1 = 4*zf;
        else
            rightWheelSpeed1 = 5*zf;
    }

    public bool leftOver;
    /// <summary>
    /// 左轮数据发送
    /// </summary>
    public void LeftWheelTurn()
    {
        leftOver = true;
        if (preLeftWheel.turnSpeed != leftWheel.turnSpeed || preLeftWheel.turnDirection != leftWheel.turnDirection) //数据不一样时发送
        {
            if (joyData.type == JockstickData.JockType.twoServo)
            {
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(joyData.leftUpID, leftWheel);
                Debug.Log("2lun Left Wheel is " + leftWheel.turnSpeed);
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                Dictionary<byte, TurnData> dict = new Dictionary<byte, TurnData>();
                dict[joyData.leftUpID] = leftWheel;
                dict[joyData.leftBottomID] = leftWheel;
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(dict);
                /*RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(joyData.leftUpID, leftWheel);
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(joyData.leftBottomID, leftWheel);*/
                Debug.Log("4lun Left Wheel is " + leftWheel.turnSpeed);
            }
            preLeftWheel = leftWheel;
        }
    }
    public bool rightOver;
    /// <summary>
    /// 右轮数据发送
    /// </summary>
    public void RightWheelTurn()
    {
        rightOver = true;
        if (rightWheel.turnSpeed != preRightWheel.turnSpeed || rightWheel.turnDirection != preRightWheel.turnDirection) ////数据不一样时发送
        {
            if (joyData.type == JockstickData.JockType.twoServo)
            {
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(joyData.rightUpID, rightWheel);
                Debug.Log("2lun Right Wheel is " + rightWheel.turnSpeed);
            }
            else if (joyData.type == JockstickData.JockType.fourServo)
            {
                Dictionary<byte, TurnData> dict = new Dictionary<byte, TurnData>();
                dict[joyData.rightUpID] = rightWheel;
                dict[joyData.rightBottomID] = rightWheel;
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(dict);
                /*RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(joyData.rightUpID, rightWheel);
                RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(joyData.rightBottomID, rightWheel);*/
                Debug.Log("4lun Right Wheel is " + rightWheel.turnSpeed);
            }
            preRightWheel = rightWheel;
        }
        
    }
}
/// <summary>
/// 横滑竿控制类
/// </summary>
public class HSliderControl
{ 
    
}
/// <summary>
/// 竖滑竿管理类
/// </summary>
public class VSliderControlManager
{
    List<VSliderControl> vSliderList;

    public VSliderControl GetVSliderByID(string id)
    {
        if (vSliderList != null)
        {
            return vSliderList.Find((x) => x.ID == id);
        }
        else
        {
            return null;
        }
    }

    public VSliderControlManager(List<SliderWidgetData> vList)
    {
        vSliderList = new List<VSliderControl>(vList.Count);
        for(int i = 0;i<vList.Count;i++)
        {
            vSliderList.Add(new VSliderControl(vList[i]));
        }
    }

    /// <summary>
    /// 新增摇杆控件
    /// </summary>
    /// <param name="data"></param>
    public void AddNewStickcontrol(SliderWidgetData data)
    {
        vSliderList.Add(new VSliderControl(data));
       // joystickList.Add(new JoyStickControl(data));
    }
    /// <summary>
    /// 删除摇杆控件
    /// </summary>
    /// <param name="widgetid"></param>
    public void RemoveStickcontrol(string widgetid)
    {
        vSliderList.Remove(GetVSliderByID(widgetid));
       // joystickList.Remove(GetJoystickByID(widgetid));
    }

    public void ClearUp()
    {
        vSliderList = new List<VSliderControl>();
    }

    /// <summary>
    /// 准备就绪
    /// </summary>
    public void ReadyVsliderControl()
    {
        if (vSliderList != null)
        {
            foreach (var tem in vSliderList)
            {
                if (tem.sliderData != null)
                {
                    tem.isRightSetting = tem.sliderData.isOK;
                }
                else
                    tem.isRightSetting = false;
            }
        }
    }
}
/// <summary>
/// 竖滑竿控制类
/// </summary>
public class VSliderControl
{
    public bool isReady;  //控件是否可以控制 蓝牙ok，配置ok
    public string ID;
    public SliderWidgetData sliderData;
    private TurnData wheelData;
    private TurnData preWheelData;
    public static bool turnOver;
    public bool isRightSetting;
    public bool changeFlag;
    public bool changeOver;
    private int speedvalue; //速度档位值
    public int Speedvalue                          //档位值发生改变时 记录当前轮模式的数据改变
    {
        get
        {
            return speedvalue;
        }
        set
        {
            if (value != speedvalue && value != 0) //发生改变
            {
                int k = 1;
                if (sliderData.directionDisclock)
                    k = -1;
                else
                    k = 1;
                if (value * k > 0)
                    wheelData.turnDirection = TurnDirection.turnByClock;
                else
                    wheelData.turnDirection = TurnDirection.turnByDisclock;
                wheelData.turnSpeed = (ushort)WidgetManager.speedArray[Mathf.Abs(value)];
                changeFlag = true;  //标记着值发生改变
            }
            else if (value != 0)
            {

            }
            else
            {
                wheelData.turnDirection = TurnDirection.turnStop;
                wheelData.turnSpeed = 0;
                changeFlag = true;
            }
            speedvalue = value;
        }
    }

    public VSliderControl(SliderWidgetData data)
    {
        sliderData = data;
        this.ID = data.widgetId;
    }
    /// <summary>
    /// 坐标与速度的转换
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="r"></param>
    public void TurnWheelSpeed(float x, float y, int r)
    {
        float a = (2 * y - x - Mathf.Abs(x)) / (2 * r);
        if (a < 0.1f && a > -0.1f)
        {
            Speedvalue = 0;
            return;
        }
        int zf = a > 0 ? 1 : -1;
        float aa = Mathf.Abs(a);
        if (aa < 0.3f)
            Speedvalue = 1 * zf;
        else if (aa < 0.5f)
            Speedvalue = 2 * zf;
        else if (aa < 0.7f)
            Speedvalue = 3 * zf;
        else if (aa < 0.9f)
            Speedvalue = 4 * zf;
        else
            Speedvalue = 5 * zf;
    }

    /// <summary>
    /// 发送命令通知硬件
    /// </summary>
    public void WheelTurn()
    {
        changeOver = true;
        if (preWheelData.turnSpeed != wheelData.turnSpeed || preWheelData.turnDirection != wheelData.turnDirection) //数据不一样时发送
        {
            RobotManager.GetInst().GetCurrentRobot().CtrlServoTurn(sliderData.servoID, wheelData);
            preWheelData = wheelData;
        }
    }
}

