//----------------------------------------------
//            积木2: xiongsonglin
// 指引流程框架
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Event;

public class GuideViewBase : MonoBehaviour {
    public static GuideViewBase Ins;
    public Transform tipTrans;
    public GameObject exitGuideBTn;
    public GameObject cover;

    public bool IsCloseGuide;      //关闭教程
    public bool IsNext;    //下一步
    public bool IsFalse;  //该步骤失败

    public static bool flagger = false;   //防止每次回来一直创建

    public void Awake()
    {
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null == robot)
        {
            flagger = true;
        }
        if (!flagger && StepManager.GetIns().OpenOrCloseGuide)
        {
            flagger = !flagger;
            DontDestroyOnLoad(gameObject); // 不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool OnceFlag = false;
    public static int ixt = 0;
    public static bool IsModelLoadOver = false;  //模型是否加载完成
	// Use this for initialization
	void Start () {
        //指引开启， 每次回到主界面接收消息
        if (StepManager.GetIns().OpenOrCloseGuide)
        {
            Ins = this;
            EventMgr.Inst.Regist(EventID.GuideNeedWait, WaitCallback);
          //  OpenGuide();//打开指引 
            if (!OnceFlag)
            {
                IsModelLoadOver = false;   //
                StartGuide();
                OnceFlag = true;
               // EventMgr.Inst.Fire(EventID.);
                Game.Platform.PlatformMgr.Instance.DisConnenctBuletooth();
                UIEventListener.Get(exitGuideBTn).onClick = OnCloseGuide;
                exitGuideBTn.SetActive(true);
                exitGuideBTn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("退出指引");
            }
        }
	}

    void Update()
    {
        #region  //指引辅助工具
        /*
        if (IsReadyWriteInto)   //stepxi写入xml
        {
            WriteStepIntoFile();
            IsReadyWriteInto = false;
        }
        if (IsLoadTipObj) //挂载对应的tipobj
        {
            GameObject tempObj = Step.CreateTipObjByType(CurType);
            if (tempObj != null)
            {
                tempObj = Instantiate(tempObj) as GameObject;
                tempObj.transform.SetParent(tipTrans);
                tempObj.transform.localScale = Vector3.one;
                tempObj.transform.localPosition = Vector3.zero;
            }
            IsLoadTipObj = false;
        }
        if (IsOpenGuide)
        {
            StartGuide();
            IsOpenGuide = false;
        }
        if (IsCloseGuide)
        {
            CloseGuide();
            IsCloseGuide = false;
        }
         * */

        #endregion
        if (IsNext)
        {
            IsNext = false;
            for (int i = 0; i < tipTrans.childCount; i++) //清掉当前数据 
            {
                Destroy(tipTrans.GetChild(i).gameObject);
            }
         //   StepManager.GetIns().GoNext();
            StepSuccess();
        }
        if (IsCloseGuide)
        {
            IsCloseGuide = false;
            CloseGuide();
        }
    }

    void LateUpdate()
    {
        if (StepManager.GetIns().OpenOrCloseGuide)
        {
            if (moveObject != null)
            {
                if (moveObject.GetComponent<UILabel>() == null)  //no text
                    addObj.transform.position = moveObject.transform.position;
            }
        }
    }

    #region //写指引
    public GameObject targetObject;
    public bool isNullStep;
    public GameObject tipObject;
    public TipType tipType;
    public int lastStep;
    public int nextStep;
    public bool flag1;
    public string flag2;
    public string flag3;
   // public type
    #endregion

    /// <summary>
    /// 指引开启 只会执行一次
    /// </summary>
    void StartGuide()
    {
        //挂载了指引的物体
       StepManager.GetIns();

        StartCoroutine(WaitFrame());
    }

    IEnumerator WaitFrame()
    {
        yield return null;
        yield return null;
        yield return null;
        ShowCurTip();
     //   StepManager.GetIns().GetStepByIndex(curStepIndex).ShowTipObj(tipTrans);
        UICamera.genericEventHandler = gameObject;   //所有的事件将流入这里
    }

    public void OnCloseGuide(GameObject obj)
    {
        CloseGuide();
        if (addObj != null)
        {
            Destroy(addObj);
        }
        if (cover != null)
            Destroy(cover);

    }

    /// <summary>
    /// 指引关闭
    /// </summary>
    void CloseGuide()
    {
        EventMgr.Inst.Fire(EventID.GuideShutdown);
        for (int i = 0; i < tipTrans.childCount; i++)
        {
            Destroy(tipTrans.GetChild(i).gameObject);
        }
        ClearTipObj();
        UICamera.genericEventHandler = null;   //事件将不再截取
        StepManager.GetIns().CloseGuide();
        if (exitGuideBTn != null)
            exitGuideBTn.SetActive(false);
    }

    #region 回调处理
    void OnClick()
    { }
    public bool isTriggle;
    private GameObject triggleObj;
    void OnClick(EventReceive erg)  //do
    {
        if (erg.go == exitGuideBTn)  //退出按钮
        {
            erg.go.SendMessage("OnClick", erg.obj, SendMessageOptions.DontRequireReceiver);
            return;
        }
        for (int i = 0; i < tipTrans.childCount; i++) //清掉当前数据 
        {
            Destroy(tipTrans.GetChild(i).gameObject);
        }
        string tstr = StepManager.GetIns().GetCurStepParth();
        GameObject targetObj = GameObject.Find(tstr); //目标物体
        bool flag1 = false;
        if (tstr.Contains("#"))
        {
            char[] ch = new char[1];
            ch[0] = '#';
            string[] aa = tstr.Split(ch);
            targetObj = GameObject.Find(aa[0]);
            if (aa[1] == "") // a/b/c# 此情况表示 需要判断parent
                flag1 = true;
            else
            {
                int childID = int.Parse(aa[1]);
                if (childID != -1 && targetObj != null && targetObj.transform.childCount >= childID)  // 
                {
                    targetObj = targetObj.transform.GetChild(childID).gameObject;
                }
            }
        }
        if (erg.go == targetObj || erg.go.transform.parent != null && erg.go.transform.parent.gameObject == targetObj && flag1) //找到目标
        {
            exitGuideBTn.SetActive(false);
            if (IsModelLoadOver)
            {
                string waitID = StepManager.GetIns().GetCurStepWaitID();
                if (waitID == "")
                {
                    StepSuccess();
                }
                erg.go.SendMessage("OnClick", erg.obj, SendMessageOptions.DontRequireReceiver); // 执行当前步骤
            }
            
            //exitGuideBTn.SetActive(false);
        }
    }

    /// <summary>
    /// 指引恢复后 触发下一步继续
    /// </summary>
    public void ShowAfterTriggle()
    {
        tipTrans.gameObject.SetActive(true);
        //exitGuideBTn.SetActive(true);
    }

    void ErrorWin()
    {
        for (int i = 0; i < tipTrans.childCount; i++) //清掉当前数据 
        {
            Destroy(tipTrans.GetChild(i).gameObject);
        }
        GameObject errorUI = Resources.Load("Prefabs/Tip_BDialog_up") as GameObject;
        errorUI = GameObject.Instantiate(errorUI) as GameObject;
        errorUI.transform.SetParent(tipTrans);
        errorUI.transform.localScale = Vector3.one;
        UISprite dialog_bg = errorUI.transform.GetChild(0).GetComponent<UISprite>();
        dialog_bg.width = 500;
        dialog_bg.height =380;
        UILabel tip = dialog_bg.transform.GetChild(0).GetComponent<UILabel>();
        tip.text = LauguageTool.GetIns().GetText("指引异常提示");
        GameObject targetBtn = null;
        if (dialog_bg.transform.childCount > 1)
        {
            targetBtn = dialog_bg.transform.GetChild(1).gameObject;
            targetBtn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("退出指引");
            UICamera.genericEventHandler = null;
            UIEventListener.Get(targetBtn).onClick = OnErrorGuideClicked;
        }
    }

    private GameObject overObj;
    void SuccessWin()
    {
        overObj = Resources.Load("Prefabs/GuideOver") as GameObject;
        //exitGuideBTn.SetActive(false);
        overObj = Instantiate(overObj) as GameObject;
        overObj.transform.SetParent(transform);
        overObj.transform.localScale = Vector3.one;
        overObj.transform.localPosition = Vector3.zero;
        UILabel[] lables = overObj.GetComponentsInChildren<UILabel>();
        lables[0].text = LauguageTool.GetIns().GetText("指引完成");
        lables[1].text = LauguageTool.GetIns().GetText("返回主界面");
        UICamera.genericEventHandler = null;
        cover.SetActive(false); 
        UIEventListener.Get(lables[1].transform.parent.gameObject).onClick = OnOverGuideClicked;
    }

    void OnOverGuideClicked(GameObject go)
    {
        overObj.SetActive(false);
        Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
        OnCloseGuide(go);
    }

    void OnErrorGuideClicked(GameObject go)
    {
        CloseGuide();
    }

    public void StepSuccess()
    {
        flag_step_text = false; //每次成功重置该步骤的。。。
        if (addObj != null)
        {
            Destroy(addObj);
        }
        addObj = null;
        StepManager.GetIns().ExicuteSuccess();
        if (StepManager.GetIns().IsOver())
        {
            SuccessWin();
            return;
        }
        if (StepManager.GetIns().GetCurStepWaitID() != "" && StepManager.GetIns().GetCurStepParth() == "") //NULL STEP
        {
            return;
        }
        if (StepManager.GetIns().IsNeedTriggle())
        {
            isTriggle = true;
            UICamera.genericEventHandler = null;
            tipTrans.gameObject.SetActive(false);
            //exitGuideBTn.SetActive(false);
            cover.SetActive(false);
            return;
        }
        ShowCurTip();
    }

    public void EndTriggleStep()
    {
        GuideViewBase.Ins.isTriggle = false;
        UICamera.genericEventHandler = GuideViewBase.Ins.gameObject;
        //exitGuideBTn.SetActive(true);
        if (cover != null)
            cover.SetActive(!isTriggle);
    }

    void StepFail()
    {
        if (tipTrans.childCount != 0)
        {
            for (int i = 0; i < tipTrans.childCount; i++)
            {
                Destroy(tipTrans.GetChild(i).gameObject);
            }
        }
        if (addObj != null)
        {
            Destroy(addObj);
        }
        StepManager.GetIns().ExicuteFailed();
        ShowCurTip();
    //    StepManager.GetIns().GetLastStepByIndex(curStepIndex).ShowTipObj(tipTrans);
    }

    /// <summary>
    /// show triggle step tip
    /// </summary>
    /// <param name="f"></param>
    public void ShowCurTip(bool f)
    {
        if (cover != null)
            cover.SetActive(true);
        StartCoroutine(ShowAfterSometime(f));
        exitGuideBTn.SetActive(StepManager.GetIns().ShowExit());
    }

    public void ClearTipObj()
    {
        if (addObj != null)
            Destroy(addObj);
        if (isTriggle)
        {
            cover.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    GameObject moveObject;
    public void ShowCurTip()
    {
        if (cover != null)
            cover.SetActive(!isTriggle);
        StartCoroutine(ShowAfterSometime(false));
        exitGuideBTn.SetActive(StepManager.GetIns().ShowExit());
    }
    bool flag_step_text = false;
    IEnumerator ShowAfterSometime(bool f)
    {
        moveObject = null;
        if (StepManager.GetIns().IsNeedTriggle() && !f || StepManager.GetIns().GetShowStyle() == StepManager.showType.none) //null step or does't need show
        {
            yield break;
        }
        GameObject tempObj;
        string tstr = StepManager.GetIns().GetCurStepParth();
        int childID = -1;
        if (tstr.Contains("#"))
        {
            char[] ch = new char[1];
            ch[0] = '#';
            string[] aa = tstr.Split(ch);
            tstr = aa[0];
            if (aa[1] != "") 
                childID = int.Parse(aa[1]);
        }
        yield return null;
        yield return null;
        
        if (addObj != null)
        {
            Destroy(addObj);
        }
        tempObj = GameObject.Find(tstr);
        if (tempObj == null)
            yield break;

        if (StepManager.GetIns().GetFlag3() != "")//flag3 不为空 特殊处理
        {
            if (!StepManager.GetIns().GetFlag3().Contains("000text"))
                moveObject = tempObj;
                //moveObject = tempObj.GetComponentInChildren<UILabel>().gameObject;
           // Debug.Log(moveObject.name + "==");
        }
        if (StepManager.GetIns().GetShowStyle() == StepManager.showType.scaleWidget || StepManager.GetIns().GetShowStyle() == StepManager.showType.scaleVsText)
        {
            if (childID != -1 && tempObj != null && tempObj.transform.childCount >= childID)  // 
            {
                tempObj = tempObj.transform.GetChild(childID).gameObject;
            }
            UIWidget tempwidget1 = tempObj.GetComponentInChildren<UIWidget>();

            if (tempwidget1 != null)
            {
                CreateTipObj(tempObj);
            }

            if (StepManager.GetIns().GetShowStyle() == StepManager.showType.scaleVsText)
            {
                GameObject tipTextObj = StepManager.GetIns().InitTipObj();
                if (tipTextObj != null)
                {
                    if (tipTextObj.GetComponentInChildren<UILabel>() != null)
                    {
                        UILabel[] labels = tipTextObj.GetComponentsInChildren<UILabel>();

                        if (labels.Length != 0)
                        {
                            string[] tip1s = new string[2];
                            string tip1 = StepManager.GetIns().GetTiptext1();
                            bool tipflag = false;
                            if (tip1.Contains("@"))  //包含@时表示步骤失败需要更新提示内容
                            {
                                tipflag = true;
                                tip1s = tip1.Split('@');
                            }
                            if (tipflag)
                            {
                                if (!flag_step_text) //第一次
                                {
                                    flag_step_text = true;
                                    labels[0].text = tip1s[0];
                                }
                                else
                                {
                                    labels[0].text = tip1s[1];
                                }
                            }
                            else
                            {
                                labels[0].text = tip1;
                            }
                            
                            //if (StepManager.GetIns().GetCurStepID() == 20)
                            //{
                            //    if (!flag_step_text)   //第20步骤
                            //    {
                            //        flag_step_text = true;
                            //        labels[0].text = StepManager.GetIns().GetTiptext1(); //第一次
                            //    }
                            //    else
                            //        labels[0].text = StepManager.GetIns().GetTiptext2(); //第二次
                            //}
                            //else
                            //    labels[0].text = StepManager.GetIns().GetTiptext1();
                            if (labels.Length == 2)
                                labels[1].text = StepManager.GetIns().GetTiptext2();
                        }
                    }
                    tipTextObj.transform.SetParent(addObj.transform);
                    tipTextObj.transform.localScale = Vector3.one;

                    tipTextObj.transform.localPosition = StepManager.GetIns().GetTipOffset();
                }
            }
        }
        else if (StepManager.GetIns().GetShowStyle() == StepManager.showType.animation) //动画
        {
            cover.SetActive(false);
            GameObject tipTextObj = StepManager.GetIns().InitTipObj();
            if (tipTextObj != null)
            {
                tipTextObj.transform.SetParent(transform);
                tipTextObj.transform.localScale = Vector3.one;
                tipTextObj.transform.position = tempObj.transform.position;
                tipTextObj.transform.SetParent(transform);
                tipTextObj.transform.localScale = Vector3.one;

                //    tipTextObj.transform.localPosition = StepManager.GetIns().GetTipOffset();
                addObj = tipTextObj;
                addObj.transform.position = tempObj.transform.position;
            }
        }
        else if (StepManager.GetIns().GetShowStyle() == StepManager.showType.animVsText) //动画加文字
        {
            GameObject tipTextObj = StepManager.GetIns().InitTipObj();
            if (tipTextObj != null)
            {
                tipTextObj.transform.SetParent(transform);
                tipTextObj.transform.localScale = Vector3.one;
                tipTextObj.transform.position = tempObj.transform.position;
                UILabel[] labels = tipTextObj.GetComponentsInChildren<UILabel>();
                if (labels.Length != 0)
                {
                    labels[0].text = StepManager.GetIns().GetTiptext1();
                    if (labels.Length == 2)
                        labels[1].text = StepManager.GetIns().GetTiptext2();
                }
                tipTextObj.transform.SetParent(transform);
                tipTextObj.transform.localScale = Vector3.one;

                //    tipTextObj.transform.localPosition = StepManager.GetIns().GetTipOffset();
                addObj = tipTextObj;
                addObj.transform.position = tempObj.transform.position;
            }
        }
        if (addObj != null && StepManager.GetIns().GetFlag3().Contains("000text"))
        {
            addObj.GetComponentInChildren<UILabel>().gameObject.SetActive(false);
        }
    }

    public GameObject addObj;
    void CreateTipObj(GameObject tempObj)
    {
        addObj = GameObject.Instantiate(tempObj) as GameObject;
        addObj.layer = LayerMask.NameToLayer("guideUI");
        for (int i = 0; i < addObj.transform.childCount; i++)
            addObj.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("guideUI");
        
        GameObject emptyTrans = new GameObject();
        emptyTrans.layer = LayerMask.NameToLayer("guideUI");
        addObj.transform.SetParent(emptyTrans.transform);
        addObj.transform.localScale = Vector3.one;
        addObj.transform.position = Vector3.zero;
        addObj = emptyTrans;
        UIWidget tempwidget = addObj.GetComponentInChildren<UIWidget>();
        addObj.transform.position = tempObj.transform.position;
        if (addObj.GetComponentInChildren<UIButton>() != null)
            Destroy(addObj.GetComponentInChildren<UIButton>());
        if (addObj.GetComponentInChildren<BoxCollider>() != null)    //隐藏碰撞区
        {
            addObj.GetComponentInChildren<BoxCollider>().enabled = false;
        }
        addObj.transform.SetParent(transform);
        addObj.transform.localScale = Vector3.one;
        #region   tweenScale
        /*
        TweenScale scaleT = tempwidget.GetComponent<TweenScale>();
        if (scaleT == null)
        {
            scaleT = tempwidget.gameObject.AddComponent<TweenScale>();
        }
        scaleT.from = Vector3.one;
        scaleT.to = new Vector3(1.3f, 1.3f, 1.3f);
        scaleT.style = UITweener.Style.PingPong;
         */
        #endregion
        #region  iconShake
        //  Animation anim = tempwidget.GetComponent<Animation>();
        //  if (anim == null)
        //  {
        //      anim = tempwidget.gameObject.AddComponent<Animation>();
        //  }
        //  AnimationClip animClip = Resources.Load("IconShake") as AnimationClip;
        ////  anim.AddClip(animClip,"shake");
        //  anim.clip = animClip;
        //  anim.wrapMode = WrapMode.PingPong;
        //  anim.Play();
        #endregion
        //GameObject cover = Instantiate(Resources.Load("Prefabs/cover")) as GameObject;
        //cover.transform.SetParent(tempwidget.transform);
        //cover.transform.localScale = Vector3.one;
        //cover.transform.localPosition = Vector3.zero;
    }

    void WaitCallback(EventArg arg)
    {
        string id = (string)arg[0];
        bool flag = (bool)arg[1];
        if (id == StepManager.GetIns().ErrorStep)  //蓝牙未开启
        {
            if (cover != null)
                cover.SetActive(false);
            StepManager.GetIns().isTriggle = true;
            UICamera.genericEventHandler = null;
            return;
        }
        if (id != StepManager.GetIns().GetCurStepWaitID())
            return;
        if (flag)  //步骤成功 跳入下一步
        {
            StepSuccess();
        }
        else
        {
            StepFail();
        }
        #region old
        /*
        if (id != StepManager.GetIns().GetStepByIndex(curStepIndex).GetStepWaitID())  // 不是要等待的id
            return;
        if (flag)  //要等待的执行成功 进入下一步
        {
            StepSuccess();
        }
        else   //回到上一步
        {
            StepFail();
        }
         * */
        #endregion
    }
    #endregion
    /// <summary>
    /// 重新进入蓝牙指引
    /// </summary>
    public void EndTriggleStep_BT()
    {
        EndTriggleStep();
        StepManager.GetIns().GoToStep(7);
        ShowCurTip();
    }

    void GuideEvent(EventReceive erg)
    {
        if (erg.funcName.Equals("OnClick"))
        {
            if(erg.go != null)
                OnClick(erg);
        }
        else
        {
            if(StepManager.GetIns().GetCurStepWaitID() != "" )
            {
                if (erg.go.name == StepManager.GetIns().GetFlag3()) //dian
                {
                    erg.go.SendMessage(erg.funcName, erg.obj, SendMessageOptions.DontRequireReceiver);
                }
                else if (StepManager.GetIns().GetFlag3() == "" || StepManager.GetIns().GetFlag3().Contains("000text"))  //000text时input 没有label显示
                {
                    if(erg.go.name != "dian" && erg.go.name !="bg")  //拖动舵机到时间轴的步骤，舵机拖动前后名称不一样
                        erg.go.SendMessage(erg.funcName, erg.obj, SendMessageOptions.DontRequireReceiver);
                }
            }
            //else if (StepManager.GetIns().GetCurStepID() == 16)
            //{
            //    if(erg.go.name == "dian")
            //        erg.go.SendMessage(erg.funcName, erg.obj, SendMessageOptions.DontRequireReceiver);
            //}
            //else if (StepManager.GetIns().GetCurStepID() == 18)
            //{
            //    if (erg.go.name == "Foreground")
            //        erg.go.SendMessage(erg.funcName, erg.obj, SendMessageOptions.DontRequireReceiver);
            //}
        }
    }

    void OnDestroy()
    {
        EventMgr.Inst.UnRegist(EventID.GuideNeedWait,WaitCallback);
    }
}

public class EventReceive
{
    public string funcName;
    public GameObject go;
    public object obj;

    public EventReceive(string funcName, GameObject go, object obj)
    {
        this.funcName = funcName;
        this.go = go;
        this.obj = obj;
    }
}

public delegate void DelegateCallback(bool flag);  //回调
public class EventReceiveCallback
{
    public GameObject go;   //接收消息的目标物体
    public DelegateCallback Callback;
    public object obj; //消息的参数
    public EventReceiveCallback(GameObject go, object obj, DelegateCallback callback)
    {
        this.go = go;
        this.obj = obj;
        this.Callback = callback;
    }
}

