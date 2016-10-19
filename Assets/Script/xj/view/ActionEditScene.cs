using Game.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Scene;

/// <summary>
/// Author:xj
/// FileName:ActionEditScene.cs
/// Description:
/// Time:2015/7/22 11:19:43
/// </summary>
public class ActionEditScene : BaseScene
{
    #region 公有属性
    #endregion

    #region 私有属性
    static string sOpenActionsName = null;
    static string sOpenActionsIcon = null;
    //ShowRobotIDUI mShowRobotDjID;
    ActionEditUI mActionEditUi;
    MoveSecond mMoveSecond;
    #endregion

    #region 公有函数
    public ActionEditScene()
    {
        mResPath = string.Empty;
        mUIList = new List<BaseUI>();
        //mShowRobotDjID = new ShowRobotIDUI(ShowRobotIDType.ShowModelID);
        mActionEditUi = new ActionEditUI();
        mUIList.Add(mActionEditUi);
        //mUIList.Add(mShowRobotDjID);
    }


    public static void OpenActions(string name)
    {
        sOpenActionsName = name;
        sOpenActionsIcon = null;
        SceneMgr.EnterScene(SceneType.EditAction);
    }

    public static void CreateActions(string name, string iconId)
    {
        sOpenActionsName = name;
        sOpenActionsIcon = iconId;
        SceneMgr.EnterScene(SceneType.EditAction);
    }
    public override void UpdateScene()
    {
        base.UpdateScene();
        //mEditOperateUI.Open();
        if (!string.IsNullOrEmpty(sOpenActionsIcon))
        {
            mActionEditUi.CreateActions(sOpenActionsName, sOpenActionsIcon);
            sOpenActionsName = null;
            sOpenActionsIcon = null;
        }
        else if (!string.IsNullOrEmpty(sOpenActionsName))
        {
            mActionEditUi.OpenActions(sOpenActionsName);
            sOpenActionsName = null;
        }
        mActionEditUi.Open();
        //Debug.Log("dfsfsf66666666");
       GameObject robotObj = GameObject.Find("oriGO");
        if (null != robotObj)
        {
            try
            {

                mMoveSecond = robotObj.GetComponent<MoveSecond>();
                if (null == mMoveSecond)
                {
                    mMoveSecond = robotObj.AddComponent<MoveSecond>();
                }
                /*if (null != mMoveSecond)
                {
                    List<string> list = mMoveSecond.GetAllDjName();
                    if (null != list)
                    {
                        List<Transform> djList = new List<Transform>();
                        for (int i = 0, icount = list.Count; i < icount; ++i)
                        {
                            GameObject obj = mMoveSecond.FindGOByName(list[i]);
                            if (null != obj)
                            {
                                djList.Add(obj.transform);
                            }
                        }
                        mShowRobotDjID.InitDuoJi(djList);
                    }

                }*/
            }
            catch (System.Exception ex)
            {
                if (ClientMain.Exception_Log_Flag)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
            
        }
        //mShowRobotDjID.Open();
        
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        if (null != mMoveSecond)
        {
            mMoveSecond.DestroyRobot();
            GameObject.Destroy(mMoveSecond);
        }
    }

    #endregion

    #region 私有函数
    
    #endregion
}