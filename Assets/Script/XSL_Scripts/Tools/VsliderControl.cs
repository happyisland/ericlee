﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;

[ExecuteInEditMode]
[RequireComponent(typeof(WheelCheck_slider))]
public class VsliderControl : MonoBehaviour
{
    private VSliderControl sliderControl;

    #region Delegate & Event
    public delegate void JoystickEventHandler(VsliderControl vcontrol);
    /// <summary>
    /// 开如
    /// </summary>
    public static event JoystickEventHandler On_JoystickMoveStart;
    /// <summary>
    /// Occurs when the joystick move.
    /// </summary>
    public static event JoystickEventHandler On_JoystickMove;
    /// <summary>
    /// thumb偏离中心位置，并牌按住时，每帧的回调
    /// </summary>
    public static event JoystickEventHandler On_JoystickHolding;
    /// <summary>
    /// Occurs when the joystick stops move
    /// </summary>
    public static event JoystickEventHandler On_JoystickMoveEnd;

    #endregion


    #region   property
    [SerializeField]
    bool isRunInEditor = false;
    [SerializeField]
    private string joystickName = "NguiJoystick";
    public string JoystickName { get { return this.joystickName; } }
    [HideInInspector]
    private bool isLimitInCircle = true;
    public bool IsLimitInCircle { get { return this.isLimitInCircle; } }
    [SerializeField]
    private int height = 400;
    public int Height { get { return this.height; } }
    private int moveheight = 340;
    public int MoveHeight { get { return this.moveheight; } }
    [SerializeField]
    private int width = 124;
    public int Width { get { return this.width; } }

    private int thumbRadio = 44;
    public int ThumbRadio { get { return this.thumbRadio; } }

    [SerializeField]
    private float minAlpha = 0.3f;
    public float MinAlpha { get { return this.minAlpha; } }

    private Vector2 joystickAxis = Vector2.zero;
    /// <summary>
    /// Gets the joystick axis value between -1 & 1...
    /// </summary>
    /// <value>
    /// The joystick axis.
    /// </value>
    public Vector2 JoystickAxis { get { return this.joystickAxis; } }

    private Vector2 lastJoystickAxis = Vector2.zero;
    public Vector2 LastJoystickAxis { get { return this.lastJoystickAxis; } }

    bool isForBid = false;
    /// <summary>
    /// 判断joystick是否被禁用
    /// </summary>
    public bool IsForBid { get { return this.isForBid; } }
    bool isHolding = false;
    public bool IsHolding { get { return this.isHolding; } }
    #endregion

    UIWidget root;
    [SerializeField]
    UISprite bg;
    [SerializeField]
    UISprite thumb;

    void Awake()
    {
        //this.name = this.JoystickName;
        root = this.GetComponent<UIWidget>();
        Init();
    }


    // Update is called once per frame   
    void Update()
    {
        //if (joyControl.isRightSetting)
        //{
        if (isRunInEditor && Application.isEditor && !Application.isPlaying)
        {
            SetSliderstickSize();
        }

        if (!isForBid && isHolding)
        {
            if (On_JoystickHolding != null)
            {
                On_JoystickHolding(this);
            }
        }
        //  }
    }

    void Init()
    {
        // bg.transform.localPosition = Vector3.zero;
        thumb.transform.localPosition = Vector3.zero;
        SetSliderstickSize();
        Lighting(minAlpha);
    }

    #region ngui event
    void OnPress(bool isPressed)
    {
        if (isForBid)
        {
            return;
        }

        int curVsliderServo = 0;
        if (ControllerManager.GetInst().widgetManager.vSliderManager.GetVSliderByID(gameObject.name) != null)
            curVsliderServo = (ControllerManager.GetInst().widgetManager.vSliderManager.GetVSliderByID(gameObject.name)).sliderData.servoID;

        if (curVsliderServo != 0 && RobotManager.GetInst().GetCurrentRobot().GetAllDjData().GetDjData((byte)curVsliderServo).modelType == ServoModel.Servo_Model_Angle)
        {
            Debug.Log("舵机模式错误！！");
            if (isPressed)
            {
                Lighting(1f);
                CalculateJoystickAxis();
                if (On_JoystickMoveStart != null)
                {
                    On_JoystickMoveStart(this);
                }
                isHolding = true;

                // check is useful 
                if (sliderControl == null)
                {
                    sliderControl = ControllerManager.GetInst().widgetManager.vSliderManager.GetVSliderByID(gameObject.name);
                    if (sliderControl == null)
                    {
                        return;
                    }
                }
            }
            else
            {
                CalculateJoystickAxis();
                if (On_JoystickMoveEnd != null)
                {
                    On_JoystickMoveEnd(this);
                }
                thumb.transform.localPosition = Vector3.zero;
                FadeOut(1f, minAlpha);
                isHolding = false;
            }
        }
        else
        {
            if (isPressed)
            {
                Lighting(1f);
                CalculateJoystickAxis();
                if (On_JoystickMoveStart != null)
                {
                    On_JoystickMoveStart(this);
                }
                isHolding = true;

                // check is useful 
                if (sliderControl == null)
                {
                    sliderControl = ControllerManager.GetInst().widgetManager.vSliderManager.GetVSliderByID(gameObject.name);
                    if (sliderControl == null)
                    {
                        return;
                    }
                }
                if (sliderControl.isRightSetting && PlatformMgr.Instance.GetBluetoothState()) // 蓝牙是否连接， 是否配置正确
                {
                    sliderControl.isReady = true;
                }
                else
                {
                    //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("connectRobotTip"));
                    GetComponent<WheelCheck_slider>().enabled = false;
                    sliderControl.isReady = false;
                    return;
                }
                if (sliderControl.isReady)
                {
                    GetComponent<WheelCheck_slider>().enabled = true;
                    sliderControl.TurnWheelSpeed(thumb.transform.localPosition.x, thumb.transform.localPosition.y, (int)(moveheight / 2.0f));
                }
            }
            else
            {
                if (sliderControl != null && sliderControl.isReady)
                {
                    sliderControl.TurnWheelSpeed(0, 0, (int)(moveheight / 2.0f));
                }
                CalculateJoystickAxis();
                if (On_JoystickMoveEnd != null)
                {
                    On_JoystickMoveEnd(this);
                }
                thumb.transform.localPosition = Vector3.zero;
                FadeOut(1f, minAlpha);
                isHolding = false;
            }
        }
    }

    //void OnDragStart ()
    //{
    //    if (isForBid)
    //    {
    //        Debug.Log("joystick is forbid!");
    //        return;
    //    }

    //    Debug.Log("OnDragStart");
    //    Lighting(1f);
    //    CalculateJoystickAxis();
    //    if(On_JoystickMoveStart!=null)
    //    {
    //        On_JoystickMoveStart(this);
    //    }
    //    isHolding = true;
    //    Debug.Log(string.Format("time:{0} - axis:{1}", Time.time, joystickAxis));
    //}

    void OnDrag(Vector2 delta)
    {
        if (isForBid)
        {
            return;
        }
        CalculateJoystickAxis();

        if (sliderControl != null && sliderControl.isReady)
        {
            sliderControl.TurnWheelSpeed(thumb.transform.localPosition.x, thumb.transform.localPosition.y, (int)(moveheight / 2.0f));
        }
        if (On_JoystickMoveStart != null)
        {
         //   On_JoystickMoveStart(this);
        }
    }

    //void OnDragEnd()
    //{
    //    if (isForBid)
    //    {
    //        return;
    //    }
    //    Debug.Log("OnDragEnd");
    //    CalculateJoystickAxis();
    //    if (On_JoystickMoveEnd != null)
    //    {
    //        On_JoystickMoveEnd(this);
    //    }
    //    thumb.transform.localPosition = Vector3.zero;
    //    FadeOut(1f, minAlpha);
    //    isHolding = false;
    //}
    #endregion

    #region utile

    /// <summary>
    /// 计算JoystickAxis
    /// </summary>
    /// <returns></returns>
    void CalculateJoystickAxis()
    {
        Vector3 offset = ScreenPos_to_NGUIPos(UICamera.currentTouch.pos);
        offset -= (transform.localPosition-new Vector3(0, 25, 0));
        if (isLimitInCircle)
        {
            if (offset.magnitude > moveheight * 0.5f)
            {
                offset = offset.normalized * moveheight * 0.5f;
            }
        }
      //  thumb.transform.localPosition = offset;
        if(!UserdefControllerUI.isSetting)
            thumb.transform.localPosition = new Vector3(thumb.transform.localPosition.x, offset.y, thumb.transform.localPosition.z);

        lastJoystickAxis = joystickAxis;
        joystickAxis = new Vector2(joystickAxis.x, offset.y / moveheight);

    }

    /// <summary>
    /// Axis2s the angle.
    /// </summary>
    /// <returns>
    /// The angle.
    /// </returns>
    public float Axis2Angle(bool inDegree = true)
    {
        float angle = Mathf.Atan2(joystickAxis.x, joystickAxis.y);

        if (inDegree)
        {
            return angle * Mathf.Rad2Deg;
        }
        else
        {
            return angle;
        }
    }

    /// <summary>
    /// Axis2s the angle.
    /// </summary>
    /// <returns>
    /// The angle.
    /// </returns>
    public float Axis2Angle(Vector2 axis, bool inDegree = true)
    {
        float angle = Mathf.Atan2(axis.x, axis.y);

        if (inDegree)
        {
            return angle * Mathf.Rad2Deg;
        }
        else
        {
            return angle;
        }
    }



    /// <summary>
    /// 屏幕坐标-->ui坐标
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    Vector3 ScreenPos_to_NGUIPos(Vector3 screenPos)
    {
        Vector3 uiPos = UICamera.currentCamera.ScreenToWorldPoint(screenPos);
        uiPos = UICamera.currentCamera.transform.InverseTransformPoint(uiPos);
        return uiPos;
    }

    /// <summary>
    /// 屏幕坐标-->ngui坐标
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    Vector3 ScreenPos_to_NGUIPos(Vector2 screenPos)
    {
        return ScreenPos_to_NGUIPos(new Vector3(screenPos.x, screenPos.y, 0f));
    }

    /// <summary>
    /// 设置摇杆的大小
    /// </summary>
    /// <param name="radius"></param>
    void SetSliderstickSize()
    {
        root.height = height;
        root.width = width;
        thumb.width = 2 * thumbRadio;
        thumb.height = 2 * thumbRadio;
        //root.width = 2 * radius;
        //root.height = 2 * radius;
        //thumb.width = (int)(30f / 100f * root.width);
        //thumb.height = (int)(30f / 100f * root.height);
    }

    /// <summary>
    /// 点亮摇杆
    /// </summary>
    void Lighting(float alpha)
    {
        //iTween.Stop(this.gameObject, "value");
        //root.alpha = alpha;
    }

    /// <summary>
    /// 渐变摇杆的透明度
    /// </summary>
    void FadeOut(float fromAlpha, float toAlpha)
    {
        //Hashtable itweenArgs = new Hashtable();
        //itweenArgs.Add("easetype", iTween.EaseType.linear);
        //itweenArgs.Add("from", fromAlpha);
        //itweenArgs.Add("to", toAlpha);
        //itweenArgs.Add("time", 0.5f);
        //itweenArgs.Add("onupdate", "OnFadeOutTween");
        //iTween.ValueTo(this.gameObject, itweenArgs);
    }
    void OnFadeOutTween(float value)
    {
        root.alpha = value;
    }

    #endregion



    #region 激活、禁用的控制
    List<string> keys = new List<string>();

    /// <summary>
    /// 禁用
    /// </summary>
    /// <returns>返回值是，取消这个禁用要用到的key</returns>
    public string ForbidJosystick()
    {
        string key = System.Guid.NewGuid().ToString();
        keys.Add(key);
        isForBid = true;
        return key;
    }

    /// <summary>
    /// 启用
    /// </summary>
    /// <param name="key"></param>
    public void ActivizeJosystick(string key)
    {
        if (keys.Contains(key))
        {
            keys.Remove(key);
        }

        isForBid = true;
        if (keys.Count == 0)
        {
            isForBid = false;
        }
    }

    #endregion

}
