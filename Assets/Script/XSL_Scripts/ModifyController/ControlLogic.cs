﻿using UnityEngine;
using System.Collections;

public class ControlLogic
{
    public static int touchCount = 0;
    public static bool actionTouch = false;

    private static ControlLogic ins;
    public static ControlLogic GetIns()
    {
        if (ins == null)
            ins = new ControlLogic();
        return ins;
    }
    private ControlLogic()
    {
    }

    /// <summary>
    /// 播放动作
    /// </summary>
    /// <param name="name"></param>
    public void PlayAction(string actionId)
    {
        if (RobotManager.GetInst().GetCurrentRobot().Connected)
        {
            RobotManager.GetInst().GetCurrentRobot().PlayActionsForID(actionId);
            float durt = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(actionId).GetAllTime();
            durt /= 1000;
            ClientMain.GetInst().StartCoroutine(ActionCirclePlay(durt,actionId));//new Game.Event.EventArg(durt,name));//ActionCirclePlay(durt, name));
        }
        else
        { 
        
        }
    }
    /// <summary>
    /// 动作取消循环播放
    /// </summary>
    /// <param name="name"></param>
    public void CancelRePlay()
    {
        ClientMain.GetInst().gameObject.SetActive(false);//.StopCoroutine("ActionCirclePlay");//(ActionCirclePlay();
        ClientMain.GetInst().gameObject.SetActive(true);
    }
    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="name"></param>
    public void StopAction(string name)
    { 
    
    }

    private IEnumerator ActionCirclePlay(float durTime,string actionId)//Game.Event.EventArg arg)
    {
        if (!actionTouch)
            yield break;
        yield return new WaitForSeconds(durTime);
        PlayAction(actionId);
    }
}