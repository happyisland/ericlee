using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Event;
/// <summary>
/// Author:xj
/// FileName:InterfaceTestView.cs
/// Description:
/// Time:2016/7/1 12:19:46
/// </summary>
public class InterfaceTestView : BaseUI
{
    enum ViewState
    {
        Menu,
        Logic,
    }

    ViewState mViewState = ViewState.Menu;

    UIInput mLogicInput;
    GameObject mLogicObj;
    GameObject mMenuObj;
    public InterfaceTestView()
    {
        mUIResPath = "Prefab/UI/InterfaceTestView";
    }

    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            Transform logic = mTrans.Find("logic");
            if (null != logic)
            {
                mLogicObj = logic.gameObject;
            }
            Transform grid = mTrans.Find("grid");
            if (null != grid)
            {
                mMenuObj = grid.gameObject;
            }
            Transform BtnCancel = mTrans.Find("BtnCancel");
            if (null != BtnCancel)
            {
                BtnCancel.localPosition = UIManager.GetWinPos(BtnCancel, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
            }
            mLogicInput = GameHelper.FindChildComponent<UIInput>(mTrans, "logic/Input");
        }
        
    }

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        string name = obj.name;
        if (name.Equals("btnlogic"))
        {
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null != robot)
            {
                LogicCtrl.GetInst().OpenLogicForRobot(robot);
                SetViewState(ViewState.Logic);
            }
            else
            {
                Debuger.Log("未选中模型");
            }
        }
        else if (name.Equals("submitBtn"))
        {
            if (null != mLogicInput)
            {
                string text = mLogicInput.value;
                if (!string.IsNullOrEmpty(text) && text.StartsWith(LogicCtrl.GetInst().Logic_Cmd_Start))
                {
                    text = text.Replace("\r", "");
                    if (!LogicCtrl.GetInst().IsLogicProgramming)
                    {
                        
                    }
                    string[] cmds = text.Split('\n');
                    for (int i = 0, imax = cmds.Length; i < imax; ++i)
                    {
                        if (!string.IsNullOrEmpty(cmds[i]))
                        {
                            LogicCtrl.GetInst().CallUnityCmd(cmds[i]);
                        }
                    }
                }
            }
        }
        else if (name.Equals("BtnCancel"))
        {
            if (ViewState.Menu == mViewState)
            {
                OnClose();
                EventMgr.Inst.Fire(EventID.Back_Test_Scene);
            }
            else
            {
                if (mViewState == ViewState.Logic)
                {
                    LogicCtrl.GetInst().CleanUp();
                }
                SetViewState(ViewState.Menu);
            }
        }
    }


    void SetViewState(ViewState state)
    {
        if (state != mViewState)
        {
            SetViewActive(mViewState, false);
            SetViewActive(state, true);
            mViewState = state;
        }
    }

    void SetViewActive(ViewState state, bool active)
    {
        switch (state)
        {
            case ViewState.Menu:
                SetGameObjectActive(mMenuObj, active);
                break;
            case ViewState.Logic:
                SetGameObjectActive(mLogicObj, active);
                break;
        }
    }

    void SetGameObjectActive(GameObject obj, bool active)
    {
        if (null != obj)
        {
            obj.SetActive(active);
        }
    }
}