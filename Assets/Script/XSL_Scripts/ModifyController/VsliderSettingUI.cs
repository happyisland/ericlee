using UnityEngine;
using System.Collections;
using Game.Platform;

/// <summary>
/// 竖杆配置
/// </summary>
public class VsliderSettingUI : BaseUI
{
    bool isChange = false;
    public SliderWidgetData sliderData;  //滑竿数据
    private UISprite selectServo;
    public bool isSelectOtherServo = false;
    public TweenRotation tween;
    public Transform trans3;

    private Transform switchnishizhen;
    private Transform switchshunshizhen;

    public VsliderSettingUI(string widgetID)
    {
        mUIResPath = "Prefabs/VsliderSetting";
        sliderData = new SliderWidgetData();
        CopyData(sliderData,(SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(widgetID));
        sliderData.servoID = (byte)UserdefControllerUI.curVsliderServoNum;
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        selectServo = null;
        Transform trans = GameObject.Find("VsliderSetting/Cancel").transform;  //cancelBTN
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft, 34, 34);

        Vector3 pos = trans.position;
        trans = GameObject.Find("VsliderSetting/mainLabel").transform;  //label
        trans.position = new Vector3(0, pos.y, 0);//UIManager.GetWinPos(trans, UIWidget.Pivot.Top,0,UserdefControllerScene.upSpace);
        trans.GetComponent<UILabel>().text = string.Format(LauguageTool.GetIns().GetText("设置竖杆主标题"), sliderData.servoID);

        //trans = GameObject.Find("VsliderSetting/Confirm").transform; // confirmBTN
        //trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight,UserdefControllerScene.rightSpace,UserdefControllerScene.upSpace);

        trans = GameObject.Find("VsliderSetting/Confirm").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom, 0, 0);
        trans.GetComponent<UISprite>().width = PublicFunction.GetWidth();
        trans.GetChild(0).GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("确定");

        trans = GameObject.Find("VsliderSetting/sliderWiget").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Center, 0, 0);
        /*if (sliderData.directionDisclock)
            trans.GetChild(1).GetComponent<UILabel>().text = "顺时针";
        else
            trans.GetChild(1).GetComponent<UILabel>().text = "逆时针";*/
        trans.GetChild(0).GetChild(1).GetComponentInChildren<UILabel>().text = sliderData.servoID.ToString();

        if (sliderData.directionDisclock)
            trans.GetChild(0).GetChild(2).GetComponent<UISprite>().spriteName = "nishizhen";
        else
            trans.GetChild(0).GetChild(2).GetComponent<UISprite>().spriteName = "shunshizhen";

        trans.GetChild(1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("设为逆时针");
        trans.GetChild(2).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("设为顺时针");
        //trans = GameObject.Find("VsliderSetting/sliderShow").transform;
        //trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight, 298, 153);
        //trans.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("上推竖杆");

        Transform trans2 = GameObject.Find("VsliderSetting/subLabel").transform;  //label
        trans2.localPosition = UIManager.GetWinPos(trans2, UIWidget.Pivot.Top, 0, 120);
        trans2.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("设置竖杆副标题");

        trans3 = GameObject.Find("VsliderSetting/sliderWiget/vSlider/direction").transform;
        tween = trans3.GetComponent<TweenRotation>();

        if (tween == null)
        {
            tween = trans3.gameObject.AddComponent<TweenRotation>();
            tween.from = trans3.localEulerAngles;
            tween.to = new Vector3(trans3.localEulerAngles.x, trans3.localEulerAngles.y, trans3.localEulerAngles.z);
            tween.duration = 0.3f;
        }

        switchnishizhen = GameObject.Find("VsliderSetting/sliderWiget/switchDirectAC").transform;
        switchshunshizhen = GameObject.Find("VsliderSetting/sliderWiget/switchDirectCW").transform;

        if (sliderData.directionDisclock)
        {
            switchnishizhen.GetChild(3).GetComponent<UISprite>().enabled = true;
            switchshunshizhen.GetChild(3).GetComponent<UISprite>().enabled = false;
        }
        else
        {
            switchnishizhen.GetChild(3).GetComponent<UISprite>().enabled = false;
            switchshunshizhen.GetChild(3).GetComponent<UISprite>().enabled = true;
        }

        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Init vslider control setting UI!!");
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Contains("switchDirectAC"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Set nishi direction!!");

            switchnishizhen.GetChild(3).GetComponent<UISprite>().enabled = true;
            switchshunshizhen.GetChild(3).GetComponent<UISprite>().enabled = false;

            if (!sliderData.directionDisclock)
            {
                tween = trans3.gameObject.AddComponent<TweenRotation>();
                tween.from = trans3.localEulerAngles;
                tween.to = new Vector3(trans3.localEulerAngles.x, trans3.localEulerAngles.y + 180, trans3.localEulerAngles.z);
                tween.duration = 0.3f;
            }

            if (!sliderData.directionDisclock)
                tween.PlayForward();
            else
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Can't turn this direction!!");
            
            sliderData.directionDisclock = true;
            //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("配置成功提示"), HUDTextTips.Color_Green);
            //GameObject.Find("VsliderSetting/sliderWiget").transform.GetChild(1).GetComponent<UILabel>().text = "逆时针";

        }
        else if (obj.name.Contains("switchDirectCW"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Set shunshi direction!!");

            switchnishizhen.GetChild(3).GetComponent<UISprite>().enabled = false;
            switchshunshizhen.GetChild(3).GetComponent<UISprite>().enabled = true;

            if (sliderData.directionDisclock)
            {
                tween = trans3.gameObject.AddComponent<TweenRotation>();
                tween.from = trans3.localEulerAngles;
                tween.to = new Vector3(trans3.localEulerAngles.x, trans3.localEulerAngles.y - 180, trans3.localEulerAngles.z);
                tween.duration = 0.3f;
            }
            
            //Debug.Log("now CW direction is " + sliderData.directionDisclock);

            if (sliderData.directionDisclock)
                tween.PlayForward();
            else
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Can't turn this direction!!");

            sliderData.directionDisclock = false;
            //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("配置成功提示"), HUDTextTips.Color_Green);
            //GameObject.Find("VsliderSetting/sliderWiget").transform.GetChild(1).GetComponent<UILabel>().text = "顺时针";

        }
        else if (obj.name.Contains("Confirm"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Confirm save vslider control data!!");

            switchnishizhen.GetChild(3).GetComponent<UISprite>().enabled = false;
            switchshunshizhen.GetChild(3).GetComponent<UISprite>().enabled = false;

            //Debug.Log("curServoNum is " + sliderData.servoID);

            CopyData((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId), sliderData);  //确定修改， 
            UserdefControllerUI.isTotalDataChange = true;
            isSelectOtherServo = true;

            UserdefControllerScene.Ins.CloseVsliderSettingUI();
            UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.vslider_sw);
        }
        else if (obj.name.Contains("Cancel"))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Cancel current vslider control data!!");

            switchnishizhen.GetChild(3).GetComponent<UISprite>().enabled = false;
            switchshunshizhen.GetChild(3).GetComponent<UISprite>().enabled = false;

            if (sliderData.directionDisclock != ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId)).directionDisclock)
                isChange = true;
            //isSelectOtherServo = false;
            UserdefControllerScene.Ins.CloseVsliderSettingUI();
            UserdefControllerScene.Ins.BackControllerSettingUI(UserdefControllerScene.curControlT.vslider_cw);
            //UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("保存遥控器提示"), DoCancel, isChange);
        }
    }
    /// <summary>
    /// 取消保存
    /// </summary>
    /*void DoCancel(GameObject obj)
    {
        try
        {
            if (obj == null)
            {
                UserdefControllerScene.Ins.CloseVsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
                return;
            }
            string name = obj.name;
            if (name.Equals(PromptMsg.LeftBtnName))
            {
                UserdefControllerScene.Ins.CloseVsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
            }
            else if (name.Equals(PromptMsg.RightBtnName))
            {
                if (sliderData.directionDisclock != ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId)).directionDisclock)
                    isChange = true;
                if (isChange)
                {
                    CopyData((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId), sliderData);  //确定修改， 
                    UserdefControllerUI.isTotalDataChange = true;
                }
                UserdefControllerScene.Ins.CloseVsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
            }
        }
        catch (System.Exception ex)
        { }
    }*/
    /// <summary>
    /// 确定保存
    /// </summary>
    void DoOK()
    {
        CopyData((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId),sliderData);
    }
    /// <summary>
    /// 切换方向
    /// </summary>
    void DoSwitch()
    {
    }
    /// <summary>
    /// 选对应的舵机
    /// </summary>
    /// <param name="obj"></param>
    void DoSelect(GameObject obj)
    {

    }
    /// <summary>
    /// 复制数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="copyData"></param>
    void CopyData(SliderWidgetData data, SliderWidgetData copyData)
    {
        if (data != null && copyData != null)
        {
            data.widgetId = copyData.widgetId;
            data.pos_x = copyData.pos_x;
            data.pos_y = copyData.pos_y;
            data.servoID = copyData.servoID;
            data.sType = copyData.sType;
            data.directionDisclock = copyData.directionDisclock;
        }
    }
}
