//----------------------------------------------
//            积木2: xiongsonglin
// 指引步骤的数据结构
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using GuideView;
using Game.Resource;

/// <summary>
/// stepID 步骤的id ; 对话框宽高； 提示语； 相对目标显示偏移量; 提示的类型；对话框是否为目标本身； 如果是对话框相对的提示对象的路径
/// </summary>
public class Step
{
    public int stepID;
    public int lastStepID;
    public string stepWaitID = "";
    public string targetPath;  //目标按钮的路径
    public Vector2 widthAndHeight;    //提示背景的宽高
    public int offset_x;  //x的修正
    public int offset_y;  //y的修正
    public string tipLabel      //提示语  
    {
        get 
        {
            return LauguageTool.GetIns().GetText("guideTip_"+stepID+"_1");
        }
    }
    public string tipBtnLabel    //按钮提示语
    {
        get
        {
            return LauguageTool.GetIns().GetText("guideTip_" + stepID + "_2");
        }
    }
    public TipType tipObjType;
    public string secondPath;

    public Step(string path,string secondPath, Vector2 WvsH, TipType type,int lastID = 0,string stepWaitID = "")
    {
        targetPath = path;
        this.secondPath = secondPath;
        tipObjType = type;
        widthAndHeight = WvsH;
        this.stepWaitID = stepWaitID;
        this.lastStepID = lastID;  //上一步默认为0 表示当前步骤
    }
    public Step()
    {
    }
    /// <summary>
    /// 获取目标按钮路径
    /// </summary>
    /// <returns></returns>
    public string GetTargetPath()
    {
        return targetPath;
    }

    public string GetStepWaitID()
    {
        return stepWaitID;
    }

    public int GetlastStepID()
    {
        if (lastStepID == 0)
            lastStepID = stepID + 1;
        return lastStepID;
    }

    public Vector2 GetOffset()
    {
        Vector2 offset = new Vector2(0, 0);
        offset.x = offset_x;
        offset.y = offset_y;
        return offset;
    }

    //转换成xml
    public XmlStep TurnToXmlStep()
    {
        XmlStep xmlStep = new XmlStep();
        xmlStep.SetStepID(this.stepID);
        xmlStep.SetTargetPath(this.targetPath);
        xmlStep.SetTipType(this.tipObjType);
        xmlStep.SetWH(this.widthAndHeight);
        xmlStep.waitID = this.stepWaitID;
        xmlStep.secondPath = this.secondPath;
        xmlStep.SetlastStepID(lastStepID);
        return xmlStep;
    }
}

/// <summary>
/// 记录每一步的ID,以及目标路径， 提示信息（提示语，按钮提示），提示框的宽高，提示框离目标位置的偏移量，提示框的类型，提示框本身有按钮，还需记录提示框相对的提示按钮的路径
/// </summary>
public class XmlStep
{
    [XmlAttribute]
    public string stepID { get; set; }       //步骤ID
    [XmlElement]
    public string lastStepID;  //上一步骤
    [XmlElement]
    public string waitID;
    [XmlElement]
    public string targetPath;     //目标路径
    [XmlElement]
    public string width;           //int
    [XmlElement]
    public string height;          //int
    [XmlElement]
    public string offset_x;
    [XmlElement]
    public string offset_y;
    [XmlElement]
    public string TipTypeStr;    //只能填写0，1，2，3，4，5，6,7，
    [XmlElement]
    public string secondPath;   //相对的目标路径 一般为空“”；

    public XmlStep()
    {
        stepID = "0";// "步骤ID int";
        waitID = ""; //该步骤是否需要等待
        targetPath = "目标路径 string";
        width = "300";// "提示背景的宽 int";
        height = "200";//提示背景的高 int";
        TipTypeStr = "1";//"提示窗口的类型 只能填写0-6的正整数";
        secondPath = "";
        lastStepID = "0";
        offset_x = "0";
        offset_y = "0";
    }

    #region Get Data function
    public int GetStepID()
    {
        int ID = int.Parse(stepID);
        return ID;
    }
    public int GetlastStepID()
    {
        int id = int.Parse(lastStepID);
        return id;
    }
    public string GetTargetPath()
    {
        return targetPath;
    }
    //提取宽高
    public Vector2 GetWidthHeight()
    {
        int x = int.Parse(width);
        int y = int.Parse(height);
        Vector2 v = new Vector2(x, y);
        return v;
    }

    public TipType GetTipType()
    {
        int len = System.Enum.GetNames(typeof(TipType)).Length;
        int type = int.Parse(TipTypeStr);
        if (type > len || type < 0)
        {
            type = len;
        }
        return (TipType)type;
    }

    /// <summary>
    /// 将xml转换成为step
    /// </summary>
    /// <returns></returns>
    public Step TurnToStep()
    {
        Step step = new Step();
        step.stepID = GetStepID();
        step.targetPath = GetTargetPath();
        step.widthAndHeight = GetWidthHeight();
        step.tipObjType = GetTipType();
        step.stepWaitID = this.waitID;
        step.secondPath = this.secondPath;
        step.lastStepID = GetlastStepID();
        step.offset_x = int.Parse(this.offset_x);
        step.offset_y = int.Parse(this.offset_y);
        return step;

    }

    public XmlStep_new TunToNewStep()
    {
        XmlStep_new newStepxml = new XmlStep_new();
        newStepxml.height = this.height;
        newStepxml.lastStepID = this.lastStepID;
        newStepxml.offset_x = "0";
        newStepxml.offset_y = "0";
        newStepxml.secondPath = this.secondPath;
        newStepxml.stepID = this.stepID;
        newStepxml.targetPath = this.targetPath;
        newStepxml.TipTypeStr = this.TipTypeStr;
        newStepxml.waitID = this.waitID;
        newStepxml.width = this.width;
        return newStepxml;
    }

    #endregion

    #region Set Data function
    public void SetStepID(int id)
    {
        stepID = id.ToString();
    }
    public void SetlastStepID(int id)
    {
        lastStepID = id.ToString();
    }
    public void SetTargetPath(string path)
    {
        targetPath = path;
    }
    public void SetWH(Vector2 WH)
    {
        width = WH.x.ToString();
        height = WH.y.ToString();
    }
    public void SetTipType(TipType tipType)
    {
        TipTypeStr = ((int)tipType).ToString();
    }
    public void SetSecondPath(string path)
    {
        this.secondPath = path;
    }
    #endregion
}

public class XmlStep_new
{
    [XmlAttribute]
    public string stepID { get; set; }       //步骤ID
    [XmlElement]
    public string lastStepID;  //上一步骤
    [XmlElement]
    public string waitID;
    [XmlElement]
    public string targetPath;     //目标路径
    [XmlElement]
    public string width;           //int
    [XmlElement]
    public string height;          //int
    [XmlElement]
    public string offset_x;
    [XmlElement]
    public string offset_y;
    [XmlElement]
    public string TipTypeStr;    //只能填写0，1，2，3，4，5，6,7，
    [XmlElement]
    public string secondPath;   //相对的目标路径 一般为空“”；
}

public class TotalStepXml_new
{
    [XmlElement]
    public List<XmlStep_new> GuideStepsXml;

    public TotalStepXml_new()
    {
        GuideStepsXml = new List<XmlStep_new>();
    }
}

public class TotalStepsXml
{
    [XmlElement]
    public string lastStepID; // 上一次执行的id
    [XmlElement]
    public List<XmlStep> GuideStepsXml;

    public TotalStepsXml()
    {
        GuideStepsXml = new List<XmlStep>();
    }

    public void Clear()
    {
        GuideStepsXml.Clear();
    }

    public bool IsNull()
    {
        if (GuideStepsXml.Count == 0)
            return true;
        else
            return false;
    }

    public void AddStepToEnd(XmlStep step)
    {
        if (step != null)
        {
            step.SetStepID(GuideStepsXml.Count);
            GuideStepsXml.Add(step);
        }
    }

    /// <summary>
    ///得到steplist
    /// </summary>
    /// <returns></returns>
    public List<Step> TurnToStepList()
    {
        List<Step> totalSteps = new List<Step>();
        foreach (var tem in GuideStepsXml)
        {
            totalSteps.Add(tem.TurnToStep());
        }
        return totalSteps;
    }
}

/// <summary>
/// 把xml文件转换解析成 list<step>数据结构
/// </summary>
public class StepManager
{
    public bool OpenOrCloseGuide = false;  //是否开启指引流程
    public bool isTriggle = false;
    public string GuideFilePath;   //配置文件是否开闭的文件路径

    public static StepManager GetIns()
    {
        if (_ins == null)
        {
            _ins = new StepManager();
        }
        return _ins;
    }
    private static StepManager _ins;
    private StepManager()
    {
        GuideFilePath = ResourcesEx.persistentDataPath + "/instruction";

        if (!System.IO.File.Exists(GuideFilePath)) //  读取文件 文件不存在需要开启指引
        {
            OpenOrCloseGuide = true;
        }
        else  // 1为真
        {
            string msg = System.IO.File.OpenText(GuideFilePath).ReadToEnd();;
            Dictionary<string, object> data = (Dictionary<string, object>)LitJson.Json.Deserialize(msg);
            if (data != null)
            {
                string inst = data["instruction"].ToString();
                if (inst == "1")
                {
                    OpenOrCloseGuide = true;
                }
                else
                {
                    OpenOrCloseGuide = false;
                }
            }
            else
            {
                OpenOrCloseGuide = false;
            }
        }
        if (OpenOrCloseGuide)   // 指引开启才会有数据   指引开启时要清空起落杆的动作数据 和 遥控器数据
        {
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null == robot)
            {
                OpenOrCloseGuide = false;
                return;
            }

            TotalStepsXml = new TotalSteps();
            TotalSteps = new List<StepData>();
            LoadStepData();
            isOver = false;
            // WriteStep.WRITE();
        }
        else   //指引关闭时
        { 
            //ActionsManager.GetInst().de
        }
    }

    public void CloseGuide()
    {
        if (OpenOrCloseGuide)
        {
            System.IO.File.Delete(GuideFilePath);
            StreamWriter sw = System.IO.File.CreateText(GuideFilePath);//.Write("{\"instruction\":\"0\"}");
            sw.Write("{\"instruction\":\"0\"}");
            sw.Close();
            OpenOrCloseGuide = false;
            _ins = null;
        }
    }

    public int GetMaxStepID()
    {
        return TotalSteps.Count-1;
    }

    public void ClearStepManager()
    {
        _ins = null;
    }
    //需要等待的步骤名称 
    public string BTSelectStep = "BTstep";
    public string DJmsgConfirm = "DJConfirm";
    public string DefaultHuidu = "DefaultHuidu";
    public string DragDuojiStep = "DragStep";
    public string TurnAroundDuojiStep = "TurnDuojiStep";
    public string DiaodianStep = "DiaodianStep";
    public string EditReadback = "EditReadbackStep";      //编辑动作时掉电回读等待步骤
    public string InputNameStep = "InputStep";
    public string TurnToControlStep = "TurnControlStep";
    public string MainScenesToEdit = "MainToEditStep";
    public string MainScenesToControl = "MainToControlStep";
    public string MainScenesToBuild = "MainToBuildStep";
    public string EditScenesToMain = "EditToMainStep";
    public string BuildScenesToMain = "BuildToMainStep";
    public string EnterTriggle = "EnterTriggle";
    public string ExitTriggle = "ExitTriggle";
    public string WaitSometime = "WaitSometime";
    public string ErrorStep = "ERROR";
    public string SetActionTime = "setActionTime";
    public string DragStepByStep = "dragstepbystep";

    public static string filePath = "E:/guideConfig.xml";

    //private TotalStepsXml TotalStepsXml;
    //public List<Step> TotalSteps;
    private TotalSteps TotalStepsXml;
    public List<StepData> TotalSteps;
    //private 

    #region old step
    /// <summary>
    /// 将step数据添加到totalstepxml数据结构中
    /// </summary>
    /// <param name="step"></param>
    public void AddStepToEnd(Step step,bool isAutoSave)
    {
        if (step != null)
        {
          //  TotalStepsXml.AddStepToEnd(step.TurnToXmlStep());
        }
        if (isAutoSave)
        {
            SaveToFile();
        }
    }

    /// <summary>
    /// 从本地文件读取数据
    /// </summary>
    /// <param name="path"></param>
    private void LoadStepDataFromFile(string path)   //get StepsXml
    {
        TotalSteps.Clear();

        TotalStepsXml = MyMVC.XmlHelper.XmlDeserializeFromFile<TotalSteps>(path, System.Text.Encoding.UTF8);
    }

    public void TurnOldToNew()
    {

    }

    /// <summary>
    /// 从resources中读取数据
    /// </summary>
    private void LoadStepData()
    {
        //WriteStep
#if UNITY_EDITOR
       // ManageObject.Ins.StartCoroutine(LoadText());
        LoadTextFromResource();
#else
        LoadTextFromResource();
#endif
        CurStepData = TotalSteps[0];
    }

    IEnumerator LoadText()
    {
        if (!System.IO.File.Exists(filePath)) //文件不存在 则创建 
        {
            TotalStepsXml = new TotalSteps();
            SaveToFile();
        }
        string tempFilePath = "file://" + filePath;
        WWW www = new WWW(tempFilePath);
        yield return www;
        if (www.isDone)
        {
            string text = www.text;
            Debug.Log("Text:"+text);
            //TextAsset text = Resources.Load("guideConfig", typeof(TextAsset)) as TextAsset;  // GET FILE FROM RESOUCE
            if (TotalStepsXml == null)
                TotalStepsXml = new TotalSteps();
            TotalSteps.Clear();
            TotalStepsXml = MyMVC.XmlHelper.XmlDeserialize<TotalSteps>(text.Trim(), System.Text.Encoding.UTF8);
            TotalSteps = TotalStepsXml.TureToStepList();
        }
    }

    void LoadTextFromResource()
    {
        TextAsset text = Resources.Load("guideConfig", typeof(TextAsset)) as TextAsset;  // GET FILE FROM RESOUCE
        if (TotalStepsXml == null)
            TotalStepsXml = new TotalSteps();
        XmlStep step = new XmlStep();
        TotalSteps.Clear();
        TotalStepsXml = MyMVC.XmlHelper.XmlDeserialize<TotalSteps>(text.text.Trim(), System.Text.Encoding.UTF8);
        TotalSteps = TotalStepsXml.TureToStepList();
    }

    /// <summary>
    /// 将动作文件保存到本地文件
    /// </summary>
    /// <param name="path"></param>
    private void SaveToFile()
    {  
        string str = MyMVC.XmlHelper.XmlSerialize(TotalStepsXml, System.Text.Encoding.UTF8);
        
        FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        fs.SetLength(0);//首先把文件清空了。
        sw.Write(str);//写你的字符串。
        sw.Close();
        //MyMVC.XmlHelper.XmlSerializeToFile(TotalSteps, path, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// 将对象保存到本地文件
    /// </summary>
    /// <param name="path"></param>
    private void SaveToFile(string pathName,object o)
    {
        string str = MyMVC.XmlHelper.XmlSerialize(o, System.Text.Encoding.UTF8);
    //    Debug.Log(str);

        FileStream fs = new FileStream(pathName, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        fs.SetLength(0);//首先把文件清空了。
        sw.Write(str);//写你的字符串。
        sw.Close();
        //MyMVC.XmlHelper.XmlSerializeToFile(TotalSteps, path, System.Text.Encoding.UTF8);
    }

    public static string GetObjPathInScenes(GameObject go)
    {
        if (go != null)
        {
            string path = go.name;
            while (go.transform.parent != null)
            {
                path = go.transform.parent.name +"/"+ path;
                go = go.transform.parent.gameObject;
            }
            return path;
        }
        return "";
    }

    /// <summary>
    /// 初始化对应的tipObj
    /// </summary>
    /// <returns></returns>
    public GameObject InitTipObj()
    {
        GameObject tempObj = null;

        if (CurStepData.tipObjType == TipType.none)
            return tempObj;
		string tempStr = "Prefabs/Tip_" + CurStepData.tipObjType;//System.Enum.GetName(,CurStepData.tipObjType);
        tempObj = Resources.Load(tempStr) as GameObject;
        if (tempObj != null)
        {
            tempObj = GameObject.Instantiate(tempObj) as GameObject;
        }
        return tempObj;
    }
    #endregion 

    #region new step
    private StepData CurStepData;
    /// <summary>
    /// 下一步
    /// </summary>
    public void GoNext()
    {
        IsShowExit = false;
        if (999 == CurStepData.nextStepID)
        {
            isOver = true;
            return;
        }
        if (CurStepData.nextStepID < TotalSteps.Count && CurStepData.nextStepID >= 0)
        {
            int nextID = CurStepData.stepID;
            if (CurStepData.nextStepID != 0)
            {
                nextID = CurStepData.nextStepID;
            }
            else
                nextID++;
            if (CurStepData.nextStepID == 0)
                CurStepData = TotalSteps[nextID];
            else
                CurStepData = TotalSteps[CurStepData.nextStepID];
        }
        else
        {
            CurStepData = null;
        }
    }

    public void GoToStep(int id)
    {
        IsShowExit = false;
        if (id < 0 || id > TotalSteps.Count)
            return;
        CurStepData = TotalSteps[id];
    }

    private bool isOver = false; 
    public bool IsOver()
    {
        return isOver;
    }
    /// <summary>
    /// 上一步
    /// </summary>
    void GoLast()
    {
        IsShowExit = false;
        if (CurStepData.lastStepID < TotalSteps.Count && CurStepData.lastStepID >= 0)
        {
            if (CurStepData.lastStepID == 0)
            {
                CurStepData = TotalSteps[--CurStepData.stepID];
                return;
            }
            CurStepData = TotalSteps[CurStepData.lastStepID];
        }
        else
            CurStepData = null;
    }

    public string GetTiptext1()
    {
        return CurStepData.GetTiptext_1();
    }
    public string GetTiptext2()
    {
        return CurStepData.GetTiptext_2();
    }

    /// <summary>
    /// 执行成功
    /// </summary>
    public void ExicuteSuccess()
    {
        GoNext();
    }
    /// <summary>
    /// 执行失败
    /// </summary>
    public void ExicuteFailed()
    {
        GoLast();
    }
    /// <summary>
    /// 得到当前步骤的waitid
    /// </summary>
    /// <returns></returns>
    public string GetCurStepWaitID()
    {
        if (CurStepData != null)
            return CurStepData.stepWaitID;
        else
        {
            return "";
        }
    }

    public int GetCurStepID()
    {
        return CurStepData.stepID;
    }

    bool IsShowExit; 
    public string GetFlag3()
    {
        if (CurStepData.flag3 == "showExit")
        {
            CurStepData.flag3 = "";
            IsShowExit = true;
        }
        else
        {
            IsShowExit = false;
        }
        return CurStepData.flag3;
    }

    public bool ShowExit()
    {
        GetFlag3();
        return IsShowExit;
    }
    /// <summary>
    /// 得到当前步骤的目标路径
    /// </summary>
    /// <returns></returns>
    public string GetCurStepParth()
    {
        if (CurStepData != null)
            return CurStepData.targetPath;
        else
            return "";
    }
    /// <summary>
    /// 当前步骤是否是nullstep
    /// </summary>
    /// <returns></returns>
    public bool IsNeedTriggle()
    {
        return CurStepData.flag1;
    }
    //public bool IsNeedShow()
    //{
    //    return CurStepData.flag2 == 0;
    //}
    public showType GetShowStyle()
    {
        if (CurStepData.flag2 == 0)
            return showType.scaleWidget;
        else if (CurStepData.flag2 == 1)
            return showType.scaleVsText;
        else if(CurStepData .flag2 == 2)
            return showType.animation;
        else if(CurStepData.flag2 == 3)
            return showType.animVsText;
        else
            return showType.none;
    }
    public Vector2 GetTipOffset()
    {
        Vector2 v = new Vector2(CurStepData.offset_x,CurStepData.offset_y);
        return v;
    }
    #endregion

    public enum showType
    { 
        scaleWidget =0,
        scaleVsText,
        animation,
        animVsText,
        none,
    }
}

//提示物体种类
public enum TipType
{
	left_1 = 0,
	left_2 = 1,
	right_1 = 2,
	right_2 = 3,
	up_1 = 4,
	up_2 = 5,
	down_1 = 6,
	down_2 = 7,
	anim_1 = 8,
	anim_2 = 9,
	none = 10
}
