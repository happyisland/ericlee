using UnityEngine;
using System.Collections;
/// <summary>
/// 竖杆配置
/// </summary>
public class VsliderSettingUI : BaseUI
{
    bool isChange = false;
    public SliderWidgetData sliderData;  //滑竿数据
    private UISprite selectServo;
    public bool isSelectOtherServo = false;
    public VsliderSettingUI(string widgetID)
    {
        mUIResPath = "Prefabs/VsliderSetting";
        sliderData = new SliderWidgetData();
        CopyData(sliderData,(SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(widgetID));
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        selectServo = null;
        Transform trans = GameObject.Find("VsliderSetting/Cancel").transform;  //cancelBTN
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft,UserdefControllerScene.upSpace,UserdefControllerScene.leftSpace);

        Vector3 pos = trans.position;
        trans = GameObject.Find("VsliderSetting/Label").transform;  //label
        trans.position = new Vector3(0, pos.y, 0);//UIManager.GetWinPos(trans, UIWidget.Pivot.Top,0,UserdefControllerScene.upSpace);
        trans.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("设置竖滑杆主标题");

        trans = GameObject.Find("VsliderSetting/Confirm").transform; // confirmBTN
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight,UserdefControllerScene.rightSpace,UserdefControllerScene.upSpace);

        trans = GameObject.Find("VsliderSetting/bottomBoard").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.Bottom, 0, -51);
        trans.GetComponentInChildren<UISprite>().width = PublicFunction.GetWidth();

        trans = GameObject.Find("VsliderSetting/sliderWiget").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopLeft,346,184);
        if (sliderData.servoID != 0)
            trans.GetChild(1).GetComponent<UILabel>().text = "Servo " + sliderData.servoID;
        trans.GetChild(2).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("切换方向");
        trans = GameObject.Find("VsliderSetting/sliderShow").transform;
        trans.localPosition = UIManager.GetWinPos(trans, UIWidget.Pivot.TopRight, 298, 153);
        trans.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("上推竖杆");

        UserdefControllerScene.InitServoListV(GameObject.Find("VsliderSetting/bottomBoard/Sprite/grid").transform, OnButtonClick);

        Transform p = GameObject.Find("VsliderSetting/bottomBoard/Sprite/grid").transform;

        if (p != null)
        {
            for (int i = 0; i < p.childCount; i++)
            {
                p.GetChild(i).GetChild(2).GetComponent<UISprite>().enabled = false;
                p.GetChild(i).GetChild(3).GetComponent<UISprite>().enabled = false;
                p.GetChild(i).GetChild(4).GetComponent<UISprite>().enabled = false;
                p.GetChild(i).GetChild(5).GetComponent<UISprite>().enabled = false;
                p.GetChild(i).GetChild(6).GetComponent<UISprite>().enabled = false;
            }
        }

        if (sliderData.servoID != 0) //已配置过的
        {
            UILabel selectText = GameObject.Find("VsliderSetting/sliderWiget").transform.GetChild(1).GetComponent<UILabel>();
            selectText.text = LauguageTool.GetIns().GetText("舵机") + " " + sliderData.servoID.ToString();
            //舵机特殊表示
            
            for (int j = 0; j < p.childCount; j++)
            {
                if (p.GetChild(j).GetComponentInChildren<UILabel>().text == selectText.text)
                {
                    selectServo = p.GetChild(j).GetChild(6).GetComponent<UISprite>();
                    selectServo.enabled = true;
                }
            }
        }
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Contains("servo_"))  //舵机被点击
        {
            if (selectServo != null)
            {
                selectServo.enabled = false;
            }
            selectServo = obj.transform.GetChild(6).GetComponent<UISprite>();
            selectServo.enabled = true;
            UILabel text = obj.GetComponentInChildren<UILabel>();
            char[] sp = new char[1];
            sp[0] = ' ';
            byte id = byte.Parse(text.text.Split(sp)[1]);
            if (id != sliderData.servoID)
            {
                sliderData.servoID = id;
                isChange = true;
            }
            GameObject.Find("VsliderSetting/sliderWiget").transform.GetChild(1).GetComponent<UILabel>().text = "Servo " + sliderData.servoID;
        }
        else if (obj.name.Contains("switchDirect"))
        {
            Transform trans = GameObject.Find("VsliderSetting/sliderWiget/vSlider").transform;
            TweenRotation tween = trans.GetComponent<TweenRotation>();
            if (tween == null)
            {
                tween = trans.gameObject.AddComponent<TweenRotation>();
                tween.from = trans.localEulerAngles;
                tween.to = new Vector3(trans.localEulerAngles.x, trans.localEulerAngles.y, trans.localEulerAngles.z - 180);
                tween.duration = 0.3f;
            }

            sliderData.directionDisclock = !sliderData.directionDisclock;
            if (sliderData.directionDisclock)
                tween.PlayForward();
            else
                tween.PlayReverse();

        }
        else if (obj.name.Contains("Confirm"))
        {
            Debug.Log(sliderData.servoID);
            if (sliderData.servoID == 0)
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("舵机配置提示"));
            }
            else
            {
                if (sliderData.directionDisclock != ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId)).directionDisclock)
                    isChange = true;
                if (isChange)
                {
                    CopyData((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId), sliderData);  //确定修改， 
                    UserdefControllerUI.isTotalDataChange = true;
                    isSelectOtherServo = true;
                }
                UserdefControllerScene.Ins.CloseVsliderSettingUI();
                UserdefControllerScene.Ins.BackControllerSettingUI();
            }       
        }
        else if (obj.name.Contains("Cancel"))
        {
            if (sliderData.directionDisclock != ((SliderWidgetData)ControllerManager.GetInst().GetWidgetdataByID(sliderData.widgetId)).directionDisclock)
                isChange = true;
            isSelectOtherServo = false;
            UserdefControllerScene.Ins.CloseVsliderSettingUI();
            UserdefControllerScene.Ins.BackControllerSettingUI();
            //UserdefControllerScene.PopWin(LauguageTool.GetIns().GetText("保存遥控器提示"), DoCancel, isChange);
        }
    }
    /// <summary>
    /// 取消保存
    /// </summary>
    void DoCancel(GameObject obj)
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
    }
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
