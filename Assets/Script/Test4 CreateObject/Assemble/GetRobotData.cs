/*
 * 模型名称目录
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Scene;
using Game.Platform;
using Game.UI;
using Game.Event;
using System;
using System.Xml;
using Game.Resource;


public class GetRobotData : SingletonBehaviour<GetRobotData>
{

    public List<GameObject> contents = new List<GameObject>();
    public List<string> createList = new List<string>();
    public List<string> downloadList=new List<string>();
    public List<string> defaultList=new List<string>();

    
    /// <summary>
    /// 加载单个机器人的数据
    /// </summary>
    /// <param name="robotname"></param>
    public void ReadOneRobot(string robotname)
    {        
        string opentype = RobotMgr.DataType(robotname);
        RecordContactInfo.Instance.openType = opentype;
        string filename = RobotMgr.NameNoType(robotname);
        if (RobotMgr.Instance.rbt.ContainsKey(robotname) == false && RecordContactInfo.Instance.HasFile(opentype, filename))
        {
            RobotDataMgr.Instance.ReadMsg(robotname);
            RobotManager.GetInst().CreateOrUpdateRobot(robotname);
        }
    }

    public string AddMoreFile(ResFileType type,string robotname)
    {
        if (type == ResFileType.Type_default)
        {
            RecordContactInfo.Instance.openType = "default";
            if (defaultList.Contains(robotname) == false)
            {
                defaultList.Add(robotname);
                string nameWithType = RobotMgr.NameWithType(robotname, "default");
                RobotDataMgr.Instance.ReadMsg(nameWithType);
                RobotManager.GetInst().CreateOrUpdateRobot(nameWithType);
                return nameWithType;
            }
            
        }
        else if (type == ResFileType.Type_playerdata)
        {
            RecordContactInfo.Instance.openType = "playerdata";
            if (createList.Contains(robotname) == false)
            {
                createList.Add(robotname);
                string nameWithType = RobotMgr.NameWithType(robotname, "playerdata");
                RobotDataMgr.Instance.ReadMsg(nameWithType);
                RobotManager.GetInst().CreateOrUpdateRobot(nameWithType);
                return nameWithType;
            }
        }
        return null;
    }


    //新建物体即新建一个物体的名称按钮
    #region 新建物体
    /// <summary>
    /// 确认生成模型
    /// </summary>
    /// <param name="go"></param>
    /// <param name="robotname"></param>
    public void ConfirmRobotName(GameObject go,string robotname)
    {
        string nameTemp = robotname;
        RecordContactInfo.Instance.openType = "playerdata";
        string namewithtype = RobotMgr.NameWithType(nameTemp,"playerdata");
        if (RobotMgr.Instance.rbt.ContainsKey(namewithtype) == false && nameTemp != "")
        {
            RobotManager.GetInst().IsCreateRobotFlag = true;
#if UNITY_EDITOR
            if (PublicFunction.IsInteger(nameTemp))
            {
                List<byte> list = new List<byte>();
                int count = int.Parse(nameTemp);
                for (byte i = 1; i <= count; ++i)
                {
                    list.Add(i);
                }
                RobotManager.GetInst().IsCreateRobotFlag = false;


                
               CreateGO(list,nameTemp);
            }
            else
            {
                HUDTextTips.ShowTextTip("请输入整数");
            }

#else
            if (PlatformMgr.Instance.GetBluetoothState())
            {
                PlatformMgr.Instance.DisConnenctBuletooth();         
            }
            //else
            {
                //PopWinManager.Inst.ShowPopWin(typeof(ConnenctBluetoothMsg));
                ConnectBluetoothMsg.ShowMsg();
            }
#endif
        }
    }

    /// <summary>
    /// 创建机器人模型
    /// </summary>
    /// <param name="robotid"></param>
    /// <param name="nameTemp"></param>
    public void CreateGO(List<byte> robotid,string nameTemp)
    {
        GameObject loadingSprite = GameObject.Find("MainUIRoot/Loading") as GameObject;
        if (loadingSprite != null) loadingSprite.SetActive(false);

        List<int> djidTemp = new List<int>();
        if(robotid !=null&&robotid.Count>0)
        {
            foreach(byte temp in robotid)
            {
              djidTemp.Add(temp);
            }

            RecordContactInfo.Instance.openType = "playerdata";
            string nameWithType = nameTemp;
            RobotDataMgr.Instance.CreatRobotWithoutModel(nameWithType, djidTemp);   //EditAction
            RobotDataMgr.Instance.SaveRobotMsg(nameWithType);

            RobotManager.GetInst().CreateOrUpdateRobot(nameWithType, RobotMgr.Instance.rbt[nameWithType].id);
            RobotMgr.Instance.rbtnametempt = nameWithType;
            Robot robot = RobotManager.GetInst().GetRobotForID(RobotMgr.Instance.rbt[nameWithType].id);
            if (null != robot)
            {
                Robot createRobot = RobotManager.GetInst().GetCreateRobot();
                robot.ConnectRobotResult(createRobot.Mac, true);
                robot.ShowName = createRobot.ShowName;
                robot.MotherboardData = createRobot.MotherboardData;
                RobotManager.GetInst().SetCurrentRobot(robot);
                //robot.ReadBack();
                ControlData.ClearData();
            }
            RobotMgr.Instance.newRobot = true;

        }
    }

    /// <summary>
    /// 选择玩家机器人
    /// </summary>
    /// <param name="robotName"></param>
    public void SelectRobotPlayer(string robotName)
    {
            RecordContactInfo.Instance.openType = "playerdata";

            SceneMgrTest.Instance.LastScene = SceneType.StartScene;
            string nameType = RobotMgr.NameWithType(robotName, "playerdata");
           // Debug.Log("opentypeown:" + RecordContactInfo.Instance.openTypeTemp);
            SelectRobot(nameType);
    }

    /// <summary>
    /// 选择官方模型
    /// </summary>
    /// <param name="robotName"></param>
    public void SelectRobotDefault(string robotName)
    {
        RecordContactInfo.Instance.openType = "default";
        SceneMgrTest.Instance.LastScene = SceneType.StartScene;
        string nameType = RobotMgr.NameWithType(robotName, "default");
       // Debug.Log("opentypedefault:" + RecordContactInfo.Instance.openTypeTemp);
        SelectRobot(nameType);
    }

    /// <summary>
    /// 选择下载的模型
    /// </summary>
    /// <param name="robotName"></param>
    public void SelectRobotDownload(string robotName)
    {
        RecordContactInfo.Instance.openType = "download";
        string nameType = RobotMgr.NameWithType(robotName, "download");
        SelectRobot(nameType);
    }


    /// <summary>
    /// 选择模型的数据处理
    /// </summary>
    /// <param name="robotName"></param>
    public void SelectRobot(string robotName)    //
    {
        try
        {
            RobotMgr.Instance.rbtnametempt = robotName;

            string opentype = RobotMgr.DataType(robotName);
            string filename = RobotMgr.NameNoType(robotName);
            if (RobotMgr.Instance.rbt.ContainsKey(robotName) == false && RecordContactInfo.Instance.HasFile(opentype, filename))
            {
                RobotDataMgr.Instance.ReadMsg(robotName);

                RobotManager.GetInst().CreateOrUpdateRobot(robotName);
            }

            RobotMgr.Instance.newRobot = false;
            if (!ClientMain.Use_Third_App_Flag)
            {
                if (RobotMgr.Instance.rbt.ContainsKey(robotName))
                {
                    RobotManager.GetInst().ChoiceRobotForName(robotName);
                }
            }
            
            ControlData.ClearData();
            //SceneMgr.EnterScene(SceneType.MainWindow);
            
            RobotMgr.Instance.startNum = true;

        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.Log(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    /// <summary>
    /// 删除模型数据
    /// </summary>
    /// <param name="robotname"></param>
    public void DeleteRobotData(string robotname)
    {
        string nameNoType = RobotMgr.NameNoType(robotname);
        string dataType = RecordContactInfo.Instance.openType;
        RecordContactInfo.Instance.DeleteXmlFile(nameNoType, dataType);
        if (RobotMgr.Instance.rbt.ContainsKey(robotname))
        {
            RobotMgr.Instance.rbt.Remove(robotname);
        }
        RobotManager.GetInst().DeleteRobot(robotname);
    }
    #endregion
}
