using UnityEngine;
using System.Collections;
using Game.Scene;

public class UserCotrollerSettingUI : BaseUI
{
    public UserCotrollerSettingUI()
    {
        mUIResPath = "Prefab/UI/control/userdefineControllerSetting";
    }

    protected override void FirstOpen()
    {
        base.FirstOpen();
    }

    protected override void Close()
    {
        base.Close();
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        Transform topL = GameObject.Find("userdefineControllerSetting/Up/topLeft").transform;
        topL.GetChild(0).localPosition = UIManager.GetWinPos(topL, UIWidget.Pivot.TopLeft,UserdefControllerScene.leftSpace,UserdefControllerScene.upSpace);//, topL.GetChild(0).GetComponentInChildren<UIWidget>().width);
        Transform topR = GameObject.Find("userdefineControllerSetting/Up/topright").transform;
        topR.GetChild(0).localPosition = UIManager.GetWinPos(topR, UIWidget.Pivot.TopRight, UserdefControllerScene.rightSpace, UserdefControllerScene.upSpace);//, topR.GetChild(0).GetComponent<UIWidget>().width);
        Transform centerIcon = GameObject.Find("userdefineControllerSetting/Center/settingTip").transform;
        centerIcon.localPosition = UIManager.GetWinPos(centerIcon, UIWidget.Pivot.Center, 0, 70);
        Transform centerTip = GameObject.Find("userdefineControllerSetting/Center/settingTip2").transform;
        centerTip.localPosition = UIManager.GetWinPos(centerTip, UIWidget.Pivot.Center, 0, -70);
        centerTip.GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("初始化遥控器提示");
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            if (obj.name.Contains("backM"))
            {
                GoBack();
            }
            else if (obj.name.Contains("settingC"))
            {
                DoSetting();
            }
        }
        catch (System.Exception ex)
        { }
    }

    void GoBack()
    {
        mTrans.gameObject.SetActive(false);
        SceneMgr.EnterScene(SceneType.MainWindow);
    }

    void DoSetting()
    {
        OnClose();
        UserdefControllerScene.Ins.CreateNewController("");
    }
}
