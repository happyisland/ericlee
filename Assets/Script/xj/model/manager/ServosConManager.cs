using Game.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ServosConManager.cs
/// Description:
/// Time:2016/4/5 16:26:10
/// </summary>
public class ServosConManager : SingletonObject<ServosConManager>
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<string, ServosConnection> mRobotServosDict = null;
    #endregion

    #region 公有函数
    public ServosConManager()
    {
        mRobotServosDict = new Dictionary<string, ServosConnection>();
    }

    public ServosConnection GetServosConnection(string robotId)
    {
        if (mRobotServosDict.ContainsKey(robotId))
        {
            return mRobotServosDict[robotId];
        }
        return null;
    }

    public void UpdateServosConnection(string robotId, ServosConnection servosConnection)
    {
        mRobotServosDict[robotId] = servosConnection;
    }

    public void ReadServosConnection(string robotName)
    {
        try
        {
            Robot robot = RobotManager.GetInst().GetRobotForName(robotName);
            if (null != robot)
            {
                if (!mRobotServosDict.ContainsKey(robot.ID))
                {//没读取过
                    string robotPath = ResourcesEx.GetRobotPath(robotName);
                    ServosConnection tmpServos = ServosConnection.CreateServos(robotPath);
                    if (null != tmpServos)
                    {
                        mRobotServosDict[robot.ID] = tmpServos;
                        ModelDjData servosData = robot.GetAllDjData();
                        List<byte> servoList = servosData.GetIDList();
                        for (int i = 0, imax = servoList.Count; i < imax; ++i)
                        {
                            servosData.UpdateServoModel(servoList[i], tmpServos.GetServoModel(servoList[i]));
                        }
                    }
                }
            }
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
    #endregion

    #region 其他函数
    
    #endregion
}