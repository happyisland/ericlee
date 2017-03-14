//#define  Close_Robot
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Game.Event;
using Game.Scene;
using Game.Resource;

public class MoveSecondOld : MonoBehaviour
{
    public static MoveSecondOld Instance;

 
    #region 变量
    public GameObject oriGO;

    public GameObject arrow;
    private Vector3 objPos;
    private Vector3 objAngle;
    private Vector3 objScale;

    List<string> typetemp;
    public Dictionary<string, GameObject> prefabgos = new Dictionary<string, GameObject>();
   // public Dictionary<string, int> gonamnum = new Dictionary<string, int>();
    private int nameNum=1;
    private GameObject tempgo;

    public Dictionary<string,DPBOX> alldpbox=new Dictionary<string,DPBOX>();//舵盘和舵机主体旋转时需要的父物体模型--只用舵机才使用该属性
    Dictionary<string, DJClass> djIntGO = new Dictionary<string, DJClass>();   //舵机的名称，舵机类<内部模型名称，模型材质>

    List<GameObject> lines = new List<GameObject>();  //所有线


    Dictionary<string, Vector3> goPosAll = new Dictionary<string, Vector3>();  //生成模型的所有坐标
    Dictionary<string, Vector3> goAngleAll = new Dictionary<string, Vector3>();  //生成模型的所有角度
    Dictionary<string, Vector3> goScaleAll = new Dictionary<string, Vector3>();  //生成模型的所有尺寸

    private string goPos;   //物体的位置
    private string goAngle;  //物体的角度
    private string goScale;  //物体的角度
    private GameObject newt;
    private string newtName;

    private List<string> goName = new List<string>();  //所有物体的名字

    public GameObject mainCamera;
    public GameObject camera1;
    private GameObject arrowCamera;    //显示旋转箭头的相机
    private GameObject arrowCameraPreb;

    public GameObject duojiGO;     //选中的物体为duoji物体
    public GameObject dpTemp;    //duoji上duopan的替代物，用于获取选择后dp的角度
    public GameObject firstobj;

    public Vector3 tempangle;
    public string robotName;

    //已经生成的物体
    public List<GameObject> AddedAllGOs = new List<GameObject>();

    Dictionary<string, int> mDjInitRota = null;
    public List<GameObject> alldj=new List<GameObject>();  //所有的舵机物体-实物
    
    Dictionary<string, int> mDjEulerDict = new Dictionary<string, int>();

    Dictionary<string, float> mDjRotaDict = new Dictionary<string, float>();

    Dictionary<string, SelectDuoJiData> mSelectDjData = new Dictionary<string, SelectDuoJiData>();
    //public SelectDuoJiData mSelectData = new SelectDuoJiData();
    #endregion

    public Vector3 oriPos;  //oriGO的默认坐标
    public Vector3 oriAngle;//oriGO的默认角度
    public Vector3 oriScale;//oriGO的默认尺寸

    public UILabel testlabel;

    GameObject loadingSprite; //加载动画

    float t1;
    List<string> storeParts = new List<string>();//内置的零件
    List<string> outParts=new List<string>();  //需要从后台下载加载的零件
    List<string> innerParts = new List<string>();  //模型需要的内置的零件

  public GameObject connect;   //mainScene中的connect按钮
  public GameObject Guanfang;//mainScene中的Guanfang按钮

    bool createStart = true;
    int gonameCount;//所有零件的个数
    int outStartN=0;
    int officalGoN = 0;   //官方模型GO开始的数字
    void Awake()
    {
        oriScale = new Vector3(1,1,1);
        robotName = RobotMgr.Instance.rbtnametempt;
         t1= Time.realtimeSinceStartup;

        #region test
        InitiateScript inscript = new InitiateScript();
        inscript.init();
        loadingSprite = GameObject.Find("MainUIRoot/Loading") as GameObject;
       
        #endregion
        MoveStart2nd(robotName);

        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null == robot)
        {
            return;
        }
        if (ResFileType.Type_playerdata == ResourcesEx.GetResFileType(RobotMgr.DataType(robot.Name)))
        {
            if(loadingSprite != null)
                loadingSprite.SetActive(false);
        }
        if (ResFileType.Type_default == ResourcesEx.GetResFileType(RobotMgr.DataType(robot.Name)))
        {
             arrow = Resources.Load("Prefab/Test4/arrow") as GameObject;
             arrowCameraPreb = Resources.Load("Prefab/Test4/ArrowCamera") as GameObject;
             if (SceneMgrTest.Instance.LastScene==SceneType.StartScene)
             {
                 Init();
             }
            else
             {
                 CreateTheGO(robotName);
             }
        }

    }


    void Start()
    {

        Instance = this;

        if (SceneMgr.mCurrentSceneType == SceneType.EditAction)
        {

        }
        else if (SceneMgr.mCurrentSceneType == SceneType.Assemble)
        {
            SceneMgrTest.Instance.LastScene = SceneType.Assemble;
        }
    }

    void OnEnable()
    {
#if !Close_Robot
        EventMgr.Inst.Regist(EventID.Ctrl_Robot_Action, CtrlAction);
        EventMgr.Inst.Regist(EventID.Read_Start_Rota_Ack, ReadStartRotaAck);
        EventMgr.Inst.Regist(EventID.Adjust_Angle_For_UI, AdjustAngleForUI);
	        EventMgr.Inst.Regist(EventID.Read_Back_Msg_Ack_Success, OnReadBackAck);
#endif
    }
    void OnDisable ()
    {
#if !Close_Robot
        EventMgr.Inst.UnRegist(EventID.Ctrl_Robot_Action, CtrlAction);
        EventMgr.Inst.UnRegist(EventID.Read_Start_Rota_Ack, ReadStartRotaAck);
        EventMgr.Inst.UnRegist(EventID.Adjust_Angle_For_UI, AdjustAngleForUI);
        EventMgr.Inst.UnRegist(EventID.Read_Back_Msg_Ack_Success, OnReadBackAck);
#endif
    }
    public void DestroyRobot()
    {
        if (null != oriGO)
        {
            //Destroy(oriGO);
        }
    }

    //隐藏官方模型的按钮
    public void HideGuangfangBtns()
    {
        if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow && ResFileType.Type_default == ResourcesEx.GetResFileType(RobotMgr.DataType(robotName)))
        {

            connect = GameObject.Find("MainUIRoot/ModelDetails/TopRight/connect");
            Guanfang = GameObject.Find("MainUIRoot/ModelDetails/Right/Guanfang");

            if (SceneMgrTest.Instance.LastScene == SceneType.StartScene)
            {
                if (connect != null)
                {
                    connect.SetActive(false);
                }

                if (Guanfang != null)
                {
                    Guanfang.SetActive(false);
                }
            }

        }
    }

    public string path;
    public void FindOriComponent(int outN)
    {
        if(outN<innerParts.Count)
        {
			if (prefabgos.ContainsKey(innerParts[outN]) == false)
            {

				StartCoroutine(GetInnerParts(innerParts[outN],0.001f,outN));
            }
        }
    }

    //获取内置parts
    IEnumerator GetInnerParts(string tem, float t3, int num)
    {
        yield return new WaitForSeconds(t3);

        tempgo = Resources.Load("Prefab/Test4/GOPrefabs/" + tem) as GameObject;
 

        if (prefabgos.ContainsKey(tem)==false)
        {
            //PublicFunction.SetLayerRecursively(tempgo, LayerMask.NameToLayer("Robot"));
            prefabgos.Add(tem, tempgo);
        }

        if (prefabgos.Count == innerParts.Count)
        {
            if(outParts.Count== 0)
            {

                string robotType = RobotMgr.DataType(robotName);

                if (robotType == "default")
                {
                    CreateTheGO(robotName);
                }

            }
            else
            {
                AddOutParts();
            }
            
        }

        num++;
        FindOriComponent(num);
    }


    //加载外部parts
    public void AddOutParts()
    {

        foreach (string temp in outParts)
        {
            if (prefabgos.ContainsKey(temp) == false)
            {              
                string path1 = "";
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/editor/" + temp + ".assetbundle";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/ios/" + temp + ".assetbundle";
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/ios/" + temp + ".assetbundle";
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    path1 = "file:///" + ResourcesEx.persistentDataPath + "/parts/android/" + temp + ".assetbundle";

                }
                StartCoroutine(GetOutParts(temp, path1));
            }
        }
    }
    //获取外置的parts
    IEnumerator GetOutParts(string tem,string path)
    {

       WWW bundle1  = new WWW(path);
        yield return bundle1;

        try
        {
            UnityEngine.Object t = bundle1.assetBundle.mainAsset;

            tempgo = t as GameObject;
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }

        }

        if(prefabgos.ContainsKey(tem)==false)
        {

            //PublicFunction.SetLayerRecursively(tempgo, LayerMask.NameToLayer("Robot"));
            prefabgos.Add(tem, tempgo);
        }

        
     

        if (prefabgos.Count == typetemp.Count)
        {
            string robotType = RobotMgr.DataType(robotName);

            if (robotType == "default")
            {

                 CreateTheGO(robotName);
            }
        }
     
        bundle1.assetBundle.Unload(false);
    }

    void Init()
    {
 
        typetemp = RobotMgr.Instance.FindAllGOTypes(robotName);

        storeParts = PartsDataRead.Instance.FindPartsType();

        foreach(string temp in typetemp)
        {
            if(storeParts.Contains(temp)==false&& outParts.Contains(temp) == false)
            {
                outParts.Add(temp);
            }

            else if(storeParts.Contains(temp) == true && innerParts.Contains(temp) == false)
            {
                //Debug.Log("add inner:"+temp);

                innerParts.Add(temp);
            }
        }

        FindOriComponent(outStartN);


    }
    /// <summary>
    /// 获取舵机的初始数据
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, int> GetDjInitData()
    {
        try
        {
            if (null != mDjInitRota)
            {
                Dictionary<int, int> data = new Dictionary<int, int>();
                foreach (KeyValuePair<string, int> kvp in mDjInitRota)
                {
                    int id = RobotMgr.Instance.FinddjIDBydjNam(robotName, kvp.Key);// PublicFunction.GetDuoJiId(kvp.Key);
                    data[id] = PublicFunction.DuoJi_Start_Rota;
                }
                return data;
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
        
        return null;
    }

    //添加相机脚本CamRotateAroundCircle
    public void AddCamRAC()
    {
        if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
        {
            mainCamera = GameObject.Find("Camera");

        }
        else
        {
            mainCamera = GameObject.Find("MainCamera");
        }
        if (null != mainCamera)
        {
            mainCamera.transform.position = new Vector3(-3.0f, 1.0f, -0.6799996f);
            Vector3 rotatTemp = new Vector3(13.0f, 99.0f, 0);
            Quaternion tempquat = Quaternion.Euler(rotatTemp);
            mainCamera.transform.rotation = tempquat;
            Camera cam = mainCamera.GetComponent<Camera>();
            cam.farClipPlane = 2000;
            CamRotateAroundCircle rotaCricle = mainCamera.GetComponent<CamRotateAroundCircle>();
            if (null == rotaCricle)
            {
                rotaCricle = mainCamera.AddComponent<CamRotateAroundCircle>();
            }
        }
    }

    //创建完整的物体
    public void CreateTheGO(string robotname)
    {
        try
        {
            AddCamRAC();

            bool needProduce = false;
            if (null != mainCamera)
            {
                foreach (Transform child in mainCamera.GetComponentInChildren<Transform>())
                {
                    if (child.name == "Camera")
                    {
                        needProduce = true;
                    }
                }
            }
            if (needProduce == false)
            {
                arrowCamera = UnityEngine.GameObject.Instantiate(arrowCameraPreb, mainCamera.transform.position, Quaternion.identity) as GameObject;
                arrowCamera.name = "Camera";
                arrowCamera.transform.parent = mainCamera.transform;
                arrowCamera.transform.localEulerAngles = Vector3.zero;
            }

            oriGO = GameObject.Find("oriGO");

            if (oriGO == null)
            {

                string nameNoType = RobotMgr.NameNoType(robotName);
                string[] x = RecordContactInfo.Instance.FindPosModel(nameNoType);
                oriPos = RobotMgr.StringToVector(x[0]);

                oriAngle = RobotMgr.StringToVector(x[1]);
                oriScale = RobotMgr.StringToVector(x[2]);

                oriGO = new GameObject();
                oriGO.name = "oriGO";

                oriGO.transform.position = Vector3.zero;
                oriGO.transform.localEulerAngles = Vector3.zero;

                oriGO.transform.position = oriPos;
                oriGO.transform.localEulerAngles = oriAngle;

                goName = RobotMgr.Instance.FindAllGOName(robotname);

                StartCoroutine(ProduceModel(robotname, officalGoN,0.01f));

            }
            else
            {

                string nameNoType = RobotMgr.NameNoType(robotName);
                string[] x = RecordContactInfo.Instance.FindPosModel(nameNoType);
                oriPos = RobotMgr.StringToVector(x[0]);

                oriAngle = RobotMgr.StringToVector(x[1]);
                oriScale = RobotMgr.StringToVector(x[2]);


                oriGO.transform.position = oriPos;
                oriGO.transform.localEulerAngles = oriAngle;


                foreach (Transform child in oriGO.GetComponentInChildren<Transform>())
                {

                    if (child.name != "CenterObj")
                    {
                        if (AddedAllGOs.Contains(child.gameObject) == false)
                        {
                            AddedAllGOs.Add(child.gameObject);
                        }
                        //GTemp模型的局部整体
                        if (child.name.Contains("line") || child.name.Contains("lnA") || child.name.Contains("lnB") || child.name.Contains("GTemp"))
                        {
                            lines.Add(child.gameObject);
                            
                            if (child.name.Contains("ln"))
                            {

                                RobotMgr.ChangeJointsName(child.gameObject);
                            }
                        }
                    }
                }
                CreateModelFinish();
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
        
        if(RecordContactInfo.Instance.openType == "playerdata")
        {
            oriGO.SetActive(false);
        }
    }
      //模型进入控制器的处理
    public void GOInControl()
    {
        oriGO = GameObject.Find("oriGO");
        string nameNoType = RobotMgr.NameNoType(robotName);
        string[] x = RecordContactInfo.Instance.FindPosModel(nameNoType);
        oriPos = RobotMgr.StringToVector(x[0]);

        oriAngle = RobotMgr.StringToVector(x[1]);
        oriScale = RobotMgr.StringToVector(x[2]);


        oriGO.transform.position = oriPos;
        oriGO.transform.localEulerAngles = oriAngle;
        if (oriGO != null)
        {
            foreach (Transform child in oriGO.GetComponentInChildren<Transform>())
            {

                if (child.name != "CenterObj")
                {
                    if (AddedAllGOs.Contains(child.gameObject) == false)
                    {
                        AddedAllGOs.Add(child.gameObject);
                    }

                }
            }
        }

        oriGO.transform.localScale = oriScale;
        #region 模型父子关系
        if (SceneMgr.GetCurrentSceneType() != SceneType.Assemble)
        {
            RestGOPA();

            Vector3 pos = Vector3.zero;
            Transform trans = oriGO.transform;
            PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Robot"));


            //查找舵机位置图标
            RobotMgr.CloseDJPosShow(AddedAllGOs);

            //找到所有舵机

            alldj = RobotMgr.Instance.FindAllDJGO(AddedAllGOs);

            //找到所有舵机的舵盘和主体旋转的父物体
            foreach (GameObject djTemp in alldj)
            {
                GameObject dpgo = null;
                GameObject boxgo = null;

                RobotMgr.Instance.FindDPBOXGO(robotName, djTemp, out dpgo, out boxgo);
                if (dpgo != null && alldpbox.ContainsKey(djTemp.name)==false)
                {
                    DPBOX dpbox = new DPBOX();
                    dpbox.dp = dpgo;
                    dpbox.dplocalPos = dpgo.transform.localPosition;
                    dpbox.dplocalAngle = dpgo.transform.localEulerAngles;

                    dpbox.box = boxgo;
                    dpbox.boxlocalPos = boxgo.transform.localPosition;
                    dpbox.boxlocalAngle = boxgo.transform.localEulerAngles;

                    alldpbox.Add(djTemp.name, dpbox);
                }
            }

            if (alldpbox != null && alldpbox.Count > 0)
            {
                RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
            }

            Hideline();
        }
        #endregion
    }

    //
    public void CreateModelFinish()
    {
        //oriGO.transform.position = oriPos;
        //oriGO.transform.localEulerAngles = oriAngle;

        oriGO.transform.localScale = oriScale;
        #region 模型父子关系
        if (SceneMgr.GetCurrentSceneType() != SceneType.Assemble)
        {
            RestGOPA();

            Vector3 pos = Vector3.zero;
            Transform trans = oriGO.transform;
            PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Robot"));


            //查找舵机位置图标
            RobotMgr.CloseDJPosShow(AddedAllGOs);

            //找到所有舵机

            alldj = RobotMgr.Instance.FindAllDJGO(AddedAllGOs);

            //找到所有舵机的舵盘和主体旋转的父物体
            foreach (GameObject djTemp in alldj)
            {
                GameObject dpgo = null;
                GameObject boxgo = null;

                RobotMgr.Instance.FindDPBOXGO(robotName, djTemp, out dpgo, out boxgo);
                if (dpgo != null)
                {
                    DPBOX dpbox = new DPBOX();
                    dpbox.dp = dpgo;
                    dpbox.dplocalPos = dpgo.transform.localPosition;
                    dpbox.dplocalAngle = dpgo.transform.localEulerAngles;

                    dpbox.box = boxgo;
                    dpbox.boxlocalPos = boxgo.transform.localPosition;
                    dpbox.boxlocalAngle = boxgo.transform.localEulerAngles;

                    alldpbox.Add(djTemp.name, dpbox);
                }
            }

            //变换父子关系
            //if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
            //{
                if (alldpbox != null && alldpbox.Count > 0)
                {
                    RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
                }
            //}

            Hideline();
        }
        #endregion

        if (loadingSprite!=null)
        {
            loadingSprite.SetActive(false);
            GuideViewBase.IsModelLoadOver = true;
         //   EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg("StartGuide", true));
        }

        if (!StepManager.GetIns().OpenOrCloseGuide)
        {
            if (connect != null)
            {

                connect.SetActive(true);
            }

            if (Guanfang != null)
            {
                Guanfang.SetActive(true);
            }
        }
    }

    //模型生成过程
    IEnumerator ProduceModel(string robotname,int num,float t)   //List<string> goNameTemp,
    {
        yield return new WaitForSeconds(t);
        gonameCount = goName.Count;
        if (num < gonameCount)
        {
            //Debug.Log("num:"+num);
            string robotid = RobotMgr.Instance.rbt[robotname].id;

            if (goName[num] != null)
            {
                string goType = RobotMgr.Instance.rbt[robotname].gos[goName[num]].goType;

                RobotMgr.Instance.FindGOPosAngle(robotname, goName[num], out goPos, out goAngle, out goScale);

                objPos = StringToVector(goPos);

                objAngle = StringToVector(goAngle);
                objScale = StringToVector(goScale);
                if (goPosAll.ContainsKey(goName[num]) == false)
                {

                    goPosAll.Add(goName[num], objPos);
                    goAngleAll.Add(goName[num], objAngle);
                    goScaleAll.Add(goName[num], objScale);
                }

                if (prefabgos.ContainsKey(goType))
                {

                    ProduceOfficalGO(prefabgos[goType], objPos, out newt, goName[num], objAngle, num, objScale);
                }

                //gonamnum[goType]++;
            }
        }
        //    }
        //}
    }


    //创建模型新
    public void CreateGOByStep(string robotname,string gotype)
    {
        //try
        //{
        //    if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
        //    {
        //        mainCamera = GameObject.Find("Camera");
        //    }
        //    else
        //    {
        //        mainCamera = GameObject.Find("MainCamera");
        //    }
        //    if (null != mainCamera)
        //    {
        //        mainCamera.transform.position = new Vector3(-3.0f, 1.0f, -0.6799996f);
        //        Vector3 rotatTemp = new Vector3(13.0f, 99.0f, 0);
        //        Quaternion tempquat = Quaternion.Euler(rotatTemp);
        //        mainCamera.transform.rotation = tempquat;
        //        Camera cam = mainCamera.GetComponent<Camera>();
        //        cam.farClipPlane = 2000;
        //        CamRotateAroundCircle rotaCricle = mainCamera.GetComponent<CamRotateAroundCircle>();
        //        if (null == rotaCricle)
        //        {
        //            rotaCricle = mainCamera.AddComponent<CamRotateAroundCircle>();
        //        }
        //    }

        //    if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
        //    {
        //        GameObject camera = GameObject.Find("Camera");
        //        if (null != camera)
        //        {
        //            CamRotateAroundCircle rotaCricle = camera.GetComponent<CamRotateAroundCircle>();
        //            if (null == rotaCricle)
        //            {
        //                rotaCricle = camera.AddComponent<CamRotateAroundCircle>();
        //            }
        //        }
        //    }

        //    bool needProduce = false;
        //    foreach (Transform child in mainCamera.GetComponentInChildren<Transform>())
        //    {
        //        if (child.name == "Camera")
        //        {
        //            needProduce = true;
        //        }
        //    }
        //    if (needProduce == false)
        //    {
        //        arrowCamera = UnityEngine.GameObject.Instantiate(arrowCameraPreb, mainCamera.transform.position, Quaternion.identity) as GameObject;
        //        arrowCamera.name = "Camera";
        //        arrowCamera.transform.parent = mainCamera.transform;
        //        arrowCamera.transform.localEulerAngles = Vector3.zero;
        //    }

        //    oriGO = GameObject.Find("oriGO");

        //    if(createStart)
        //    {
        //        string nameNoType = RobotMgr.NameNoType(robotName);
        //        string[] x = RecordContactInfo.Instance.FindPosModel(nameNoType);
        //        oriPos = RobotMgr.StringToVector(x[0]);

        //        oriAngle = RobotMgr.StringToVector(x[1]);

        //        oriGO = new GameObject();
        //        oriGO.name = "oriGO";

        //        oriGO.transform.position = Vector3.zero;
        //        oriGO.transform.localEulerAngles = Vector3.zero;

        //        goName = RobotMgr.Instance.FindAllGOName(robotname);
        //        gonameCount = goName.Count;
        //        Debug.Log("goName:"+goName.Count);
        //        createStart = false;
        //    }

        //    if (AddedAllGOs.Count < goName.Count)
        //    {
        //        if (goName != null && goName.Count > 0)
        //        {

        //            for (int i = goName.Count - 1; i >= 0; i--)
        //            {

        //                string robotid = RobotMgr.Instance.rbt[robotname].id;

        //                if (goName[i] != null)
        //                {
        //                    string goType = RobotMgr.Instance.rbt[robotname].gos[goName[i]].goType;
                            
        //                    if(goType==gotype)
        //                    {
        //                        RobotMgr.Instance.FindGOPosAngle(robotname, goName[i], out goPos, out goAngle, out goScale);

        //                        objPos = StringToVector(goPos);

        //                        objAngle = StringToVector(goAngle);
        //                        objScale = StringToVector(goScale);
        //                        if (goPosAll.ContainsKey(goName[i]) == false)
        //                        {
        //                            goPosAll.Add(goName[i], objPos);
        //                            goAngleAll.Add(goName[i], objAngle);
        //                        }

        //                        ProduceGO(prefabgos[goType], objPos, out newt, goName[i], objAngle);
        //                    }
                           
        //                }

        //                goName.Remove(goName[i]);
        //            }
        //        }
        //    }

        //    //if (oriGO == null)
        //    //{



        //    //}
        //    //else
        //    //{

        //    //    foreach (Transform child in oriGO.GetComponentInChildren<Transform>())
        //    //    {

        //    //        if (child.name != "CenterObj")
        //    //        {
        //    //            if (AddedAllGOs.Contains(child.gameObject) == false)
        //    //            {
        //    //                AddedAllGOs.Add(child.gameObject);
        //    //            }

        //    //        }
        //    //    }

        //    //}

        //    Debug.Log("finale");
        //    Debug.Log("gonameCount:" + gonameCount + ";AddedAllGOs.Count:"+ AddedAllGOs.Count);
        //    if (AddedAllGOs.Count == gonameCount)
        //    {


        //        oriGO.transform.position = oriPos;
        //        oriGO.transform.localEulerAngles = oriAngle;

        //        SettingColor(robotName, AddedAllGOs);
        //        if (SceneMgr.GetCurrentSceneType() != SceneType.Assemble)
        //        {
        //            RestGOPA();

        //            Vector3 pos = Vector3.zero;
        //            Transform trans = oriGO.transform;
        //            PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Robot"));


        //            //查找舵机位置图标
        //            RobotMgr.CloseDJPosShow(AddedAllGOs);

        //            //找到所有舵机

        //            alldj = RobotMgr.Instance.FindAllDJGO(AddedAllGOs);


        //            //找到所有舵机的舵盘和主体旋转的父物体
        //            foreach (GameObject djTemp in alldj)
        //            {
        //                GameObject dpgo = null;
        //                GameObject boxgo = null;

        //                RobotMgr.Instance.FindDPBOXGO(robotName, djTemp, out dpgo, out boxgo);
        //                if (dpgo != null)
        //                {
        //                    DPBOX dpbox = new DPBOX();
        //                    dpbox.dp = dpgo;
        //                    dpbox.dplocalPos = dpgo.transform.localPosition;
        //                    dpbox.dplocalAngle = dpgo.transform.localEulerAngles;

        //                    dpbox.box = boxgo;
        //                    dpbox.boxlocalPos = boxgo.transform.localPosition;
        //                    dpbox.boxlocalAngle = boxgo.transform.localEulerAngles;

        //                    alldpbox.Add(djTemp.name, dpbox);
        //                }
        //            }
        //            Debug.Log("   time9:" + Time.realtimeSinceStartup);
        //            //变换父子关系
        //            if (alldpbox != null && alldpbox.Count > 0)
        //            {
        //                RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
        //            }
        //            Debug.Log("   time10:" + Time.realtimeSinceStartup);

        //            Hideline();
        //        }

        //        if (AddedAllGOs.Count == gonameCount)
        //        {
        //            loadingSprite.SetActive(false);
        //        }
        //    }
        //}
        //catch (System.Exception ex)
        //{
        //    if (ClientMain.Exception_Log_Flag)
        //    {
        //        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
        //        Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        //    }
        //}

        //if (RecordContactInfo.Instance.openType == "playerdata")
        //{
        //    oriGO.SetActive(false);
        //}
    }

    Dictionary<string, Texture> djIDTexture = new Dictionary<string, Texture>();
    //生成官方模型GO
    public void ProduceOfficalGO(GameObject prefabgo, Vector3 oPos, out GameObject newt, string nameT, Vector3 oAngle, int num,Vector3 oScale)
    {
        try
        {
            newt = UnityEngine.Object.Instantiate(prefabgo, oPos, Quaternion.identity) as GameObject;

            if (newt!=null)
            {
                newt.name = nameT;
                if (oriGO!=null)
                {
                   newt.transform.parent = oriGO.transform;
                }
                
                newt.transform.localPosition = oPos;
                newt.transform.localEulerAngles = oAngle;
                newt.transform.localScale = oScale;
            
                newtName = nameT;


                SettingColorByOne(robotName, newt);
                if (prefabgo.name == "seivo")
                {
                    int djIDTemp = RobotMgr.Instance.rbt[robotName].gos[nameT].djID;
                    RobotMgr.Instance.ShowID(newt, djIDTemp, djIDTexture);
                }

                //GTemp模型的局部整体
                if (nameT.Contains("line") || nameT.Contains("lnA") || nameT.Contains("lnB") || nameT.Contains("GTemp"))
                {
                    lines.Add(newt);

                    if (nameT.Contains("ln"))
                    {

                        RobotMgr.ChangeJointsName(newt);
                    }
                }
          

                if (nameT.Contains("line"))
                {
                    lines.Add(newt);
                }

                if (AddedAllGOs.Contains(newt) == false)
                {
                    AddedAllGOs.Add(newt);
                }
                else
                {
                    Destroy(newt);
                }
            }


            //if(AddedAllGOs.Count ==gonameCount)
            //{
            //    SettingColor(robotName, AddedAllGOs);
            //}

            num++;

            if (num < gonameCount)
            {
                
               /// ProduceModel(robotName, num);
                StartCoroutine(ProduceModel(robotName, num, 0.001f));
            }
            else
            {
                //Debug.Log("Addedgo:"+AddedAllGOs.Count);
                CreateModelFinish();
            }
        }
        catch (System.Exception ex)
        {
            newt = null;
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    //生成物体
    public void ProduceGO(GameObject prefabgo, Vector3 oPos, out GameObject newt, string nameT, Vector3 oAngle)
    {
        try
        {
            newt = UnityEngine.Object.Instantiate(prefabgo, oPos, Quaternion.identity) as GameObject;
              
            newt.name = nameT;
            newt.transform.eulerAngles = oAngle;
            newt.transform.parent = oriGO.transform;
            newtName = nameT;

            if (prefabgo.name == "seivo")
            {
                //RobotMgr.Instance.ShowDJID(newt, robotName);
            }

            if (nameT.Contains("line"))
            {
                lines.Add(newt);
            }

           // Debug.Log("AddedAllGOs.Count 01:"+ AddedAllGOs.Count+"  newt:"+newt);
            if (AddedAllGOs.Contains(newt) == false)
            {
                AddedAllGOs.Add(newt);
               // Debug.Log("AddedAllGOs.Count 02:" + AddedAllGOs.Count);
            }
            else
            {  
                Destroy(newt);
            }
        }
        catch (System.Exception ex)
        {
            newt = null;
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }
    
    //创建旋转箭头
    GameObject arrowgo;
    public void CreateArrow(GameObject tgo)
    {
        try
        {
            string t1type = RobotMgr.Instance.rbt[robotName].gos[tgo.name].goType;

            if (t1type == "seivo")
            {
                arrowgo = UnityEngine.GameObject.Instantiate(arrow, tgo.transform.position, Quaternion.identity) as GameObject;
                arrowgo.transform.right = tgo.transform.right;
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

    //删除旋转箭头
    public void DestroyArrow()
    {
        try
        {
            if (null != arrowgo)
            {
                UnityEngine.GameObject.Destroy(arrowgo);
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

    //将字符串转换为Vector3
    public Vector3 StringToVector(string vect)
    {
        Vector3 newVect = Vector3.zero;
        try
        {
            string[] num = vect.Split(new char[] { '(', ',', ')' });

            //Convert.ToSingle()将字符转换为float
            newVect = new Vector3(Convert.ToSingle(num[1]), Convert.ToSingle(num[2]), Convert.ToSingle(num[3]));
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        return newVect;
    }
    //*****动作控制模块初始化时，需要的数据
    void MoveStart2nd(string nam)
    {
        try
        {
            mDjInitRota = RobotMgr.Instance.FindDJData(robotName);
            
            if (null != mDjInitRota)
            {
                foreach (KeyValuePair<string, int> kvp in mDjInitRota)
                {
                    mDjEulerDict[kvp.Key] = kvp.Value;
                    mDjRotaDict[kvp.Key] = PublicFunction.DuoJi_Start_Rota;
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

    public List<string> GetAllDjName()
    {
        if (null != mDjInitRota)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, int> kvp in mDjInitRota)
            {
                list.Add(kvp.Key);
            }
            return list;
        }
        return null;
    }


    //通过名字查找物体
    public GameObject FindGOByName(string nam)
    {

        for (int i = 0; i < AddedAllGOs.Count; i++)
        {
            if (AddedAllGOs[i].name == nam)
            {
                return AddedAllGOs[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 把舵机的角度转换成模型上的舵盘角度
    /// </summary>
    /// <param name="name"></param>
    /// <param name="djRota"></param>
    /// <returns></returns>
    float ConvertToEulerAngles(string name, float djRota)
    {
        if (null != mDjInitRota && mDjInitRota.ContainsKey(name))
        {
            float euler = djRota + mDjInitRota[name] - PublicFunction.DuoJi_Start_Rota;
            if (euler < 0)
            {
                euler += 360;
            }
            return euler;
        }
        return djRota;
    }
    /// <summary>
    /// 把模型上的舵盘角度转换成舵机的角度
    /// </summary>
    /// <param name="name"></param>
    /// <param name="eulerAngles"></param>
    /// <returns></returns>
    float ConvertToDjRota(string name, float eulerAngles)
    {
        if (null != mDjInitRota && mDjInitRota.ContainsKey(name))
        {
            if (eulerAngles < 0)
            {
                eulerAngles += 360;
            }
            float rota = eulerAngles + PublicFunction.DuoJi_Start_Rota - mDjInitRota[name];
            if (rota < 0)
            {
                rota += 360;
            }
            return rota;
        }
        return eulerAngles;
    }
    void ReadStartRotaAck(EventArg arg)
    {
        try
        {
            DuoJiData data = (DuoJiData)arg[0];
            string name = RobotMgr.Instance.FindDJBydjID(robotName, data.id);//PublicFunction.GetDuoJiName(data.id);
            if (mDjRotaDict.ContainsKey(name))
            {
                mDjRotaDict[name] = data.rota;
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

    void CtrlAction(EventArg arg)
    {
        try
        {
            Robot robot = RobotManager.GetInst().GetCurrentRobot();
            if (null == robot)
            {
                return;
            }
            if (ResFileType.Type_default != ResourcesEx.GetResFileType(RobotMgr.DataType(robot.Name)))
            {
                return;
            }
            Action action = (Action)arg[0];
            if (null == action)
            {
                return;
            }
            foreach (KeyValuePair<byte, short> kvp in action.rotas)
            {
                string name = RobotMgr.Instance.FindDJBydjID(robotName, kvp.Key);//PublicFunction.GetDuoJiName(kvp.Key);
                short rota = kvp.Value;
                if (rota < 0)
                {
                    rota = (short)(-rota);
                }

                RotateTo(name, rota, action.sportTime);
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

    void AdjustAngleForUI(EventArg arg)
    {
        try
        {
            int id = (int)arg[0];
            int rota = (int)arg[1];
            string name = RobotMgr.Instance.FindDJBydjID(robotName, id);//PublicFunction.GetDuoJiName(kvp.Key);
            RotateTo(name, rota);
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

    void OnReadBackAck(EventArg arg)
    {
        try
        {
            CtrlAction(arg);
            /*DuoJiData data = (DuoJiData)arg[0];
            if (null != data)
            {
                string name = RobotMgr.Instance.FindDJBydjID(robotName, data.id);//PublicFunction.GetDuoJiName(data.id);
                GameObject obj = FindGOByName(name);
                if (null != obj)
                {
                    DefaultZone(obj);
                    RotateTo(mSelectData.selectObj, data.rota, true);
                    ClearZones();
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

    public void RotateTo(string djName, int a, int duration = 0)
    {
        try
        {

            if (duration > 0)
            {
                if (!mSelectDjData.ContainsKey(djName))
                {
                    mSelectDjData[djName] = new SelectDuoJiData();
                }
                mSelectDjData[djName].djName = djName;
                mSelectDjData[djName].selectObj = FindRotateGO(djName);
                mSelectDjData[djName].duration = duration;
                mSelectDjData[djName].Play(mDjRotaDict[djName], a, duration / 1000.0f);
            }
            else
            {
                if (mDjRotaDict.ContainsKey(djName))
                {
                    float rota = a - mDjRotaDict[djName];
                    if (Math.Abs(rota) > 0.1f)
                    {
                        Rotate(djName, FindRotateGO(djName), rota);
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

    //模型旋转
    public int Rotate(string djName, GameObject tselected, float a)
    {
        float euler = 0;
        try
        {
            if (null != tselected)
            {
                euler = mDjRotaDict[djName] + a;
                if (euler < PublicFunction.DuoJi_Min_Rota)
                {
                    euler = PublicFunction.DuoJi_Min_Rota;
                    a = euler - mDjRotaDict[djName];
                }
                else if (euler > PublicFunction.DuoJi_Max_Rota)
                {
                    euler = PublicFunction.DuoJi_Max_Rota;
                    a = euler - mDjRotaDict[djName];
                }

                if (RobotMgr.isDJDP(robotName, djName) == false)
                {

                    tselected.transform.Rotate(new Vector3(a, 0, 0));
                   // Debug.Log("djname:" + djName + ";djzhuti" + ";rota:");
                }
                else
                {
                    tselected.transform.Rotate(new Vector3(-a, 0, 0));
                    //Debug.Log("djname:" + djName + ";duoji" + ";rota:");
                }

                
                mDjRotaDict[djName] += a;
                mDjEulerDict[djName] = PublicFunction.Rounding(euler);



                
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
        
        return PublicFunction.Rounding(euler);
    }

    void Update()
    {
        try
        {
            if (mSelectDjData.Count > 0)
            {
                List<string> dellist = null;
                foreach (KeyValuePair<string, SelectDuoJiData> kvp in mSelectDjData)
                {
                    if (kvp.Value.Update())
                    {
                        if (null == dellist)
                        {
                            dellist = new List<string>();
                        }
                        dellist.Add(kvp.Key);
                    }
                    if (mDjRotaDict.ContainsKey(kvp.Key))
                    {
                        float offset = kvp.Value.rota - mDjRotaDict[kvp.Key];
                        if (Mathf.Abs(offset) > 0.1f)
                        {
                            Rotate(kvp.Value.djName, kvp.Value.selectObj, offset);
                        }
                    }
                    
                }
                if (null != dellist)
                {
                    for (int i = 0, imax = dellist.Count; i < imax; ++i)
                    {
                        mSelectDjData.Remove(dellist[i]);
                    }
                    dellist.Clear();
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
    //改变父子关系
    public GameObject ChangeRelation2nd(GameObject a1, GameObject a2)
    {
        a2.transform.parent = null;
        a1.transform.parent = a2.transform;
        return a2;
    }

    GameObject ject;
    //找到子物体
    public GameObject FindChild(GameObject t1, int number)
    {
        foreach (Transform m in t1.transform.GetComponentInChildren<Transform>())
        {
            if (m.name == number.ToString())
            {
                ject = m.gameObject;
            }
        }
        return ject;
    }
    List<string> childGO = new List<string>();
    public List<GameObject> posGO = new List<GameObject>();


    //设置模型颜色
    public static void SettingColorByOne(string robotname, GameObject goTemp)
    {
        try
        {
                if (goTemp.activeInHierarchy)
                {

                    string nameTemp = goTemp.name;
                    if (nameTemp.Contains("lnA") == false && nameTemp.Contains("lnB") == false&&nameTemp.Contains("line") == false && nameTemp.Contains("onoff") == false)
                    {
                        string colorTempL = RobotMgr.Instance.rbt[robotname].gos[nameTemp].color;

                        Color colorTemp = RobotMgr.StringToColor(colorTempL);
                        RobotMgr.SettingColor(goTemp, colorTemp);
                    }
                    else
                    {
                        RobotMgr.SettingLineColor(goTemp);
                    }

                }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    //设置模型颜色
    public static void SettingColor(string robotname,List<GameObject> AddedGOs)
    {
        try
        {
            for (int i = 0; i < AddedGOs.Count;i++ )
            {
                if(AddedGOs[i].activeInHierarchy)
                {

                    string nameTemp = AddedGOs[i].name;
                    //Debug.Log("color:"+nameTemp);
                    if(nameTemp.Contains("lnA") == false && nameTemp.Contains("lnB") == false&&nameTemp.Contains("line") == false && nameTemp.Contains("onoff") == false)
                    {
                        string colorTempL = RobotMgr.Instance.rbt[robotname].gos[nameTemp].color;

                        Color colorTemp = RobotMgr.StringToColor(colorTempL);
                        RobotMgr.SettingColor(AddedGOs[i], colorTemp);
                    }
                    else
                    {
                        RobotMgr.SettingLineColor(AddedGOs[i]);
                    }
                   
                }

            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    //确认生成物体后，使模型的shader为Diffuse
    public static void ChangeDiffuse(GameObject t, Color ori)
    {
        try
        {
            if (t != null)
            {
                RobotMgr.ChangeDiffuse(t, ori);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

    }



    #region djid

    //获取所有djid
    public List<int> FindDJID(string robotname)
    {
        List<int> dict = new List<int>();
        try
        {
            dict = RobotMgr.Instance.FindDJID(robotname);
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        
        return dict;
    }
    //*****通过duiji的name找duiji的id
    public int FinddjIDBydjNam(string robotname, string djname)
    {
        int djid = 0;
        try
        {
            djid = RobotMgr.Instance.FinddjIDBydjNam(robotname, djname);
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        
        return djid;
    }

    //通过duiji的id找duiji的name
    public string FindDJBydjID(string robotname, int djidTemp)
    {
        string djname = string.Empty;
        try
        {
            RobotMgr.Instance.FindDJBydjID(robotname, djidTemp);
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        
        return djname;
    }

     //修改duiji的id
    public void ReviseDJID(string robotname, int oriid, int newid)
    {
        try
        {
            RobotMgr.Instance.ReviseDJID(robotname, oriid, newid);
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

    #region 选旋转物体
    public GameObject FindRotateGO(string pickgo)
    {
        GameObject rotateGo = null;
        try
        {
            int isdp = RobotMgr.Instance.rbt[robotName].gos[pickgo].isDP;
            if (isdp == 1)
            {
                rotateGo = alldpbox[pickgo].dp;
            }
            else
            {
                rotateGo = alldpbox[pickgo].box;
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        return rotateGo;
    }
    

    
    #endregion


    #region  创建无模型机器人 
    //创建完整的物体
    public void CreateRobotWithoutGO(string robotname)
    {
        try
        {
            if (SceneMgr.GetCurrentSceneType() == SceneType.MainWindow)
            {
                mainCamera = GameObject.Find("Camera");
            }
            else
            {
                mainCamera = GameObject.Find("MainCamera");
            }
            bool needProduce = false;
            foreach (Transform child in mainCamera.GetComponentInChildren<Transform>())
            {
                if (child.name == "Camera")
                {
                    needProduce = true;
                }
            }
            if (needProduce == false)
            {
                arrowCamera = UnityEngine.GameObject.Instantiate(arrowCameraPreb, mainCamera.transform.position, Quaternion.identity) as GameObject;
                arrowCamera.name = "Camera";
                arrowCamera.transform.parent = mainCamera.transform;
                arrowCamera.transform.localEulerAngles = Vector3.zero;
            }

            oriGO = GameObject.Find("oriGO");
            if (oriGO == null)
            {
                oriGO = new GameObject();
                oriGO.name = "oriGO";
            }
            oriGO.transform.position = Vector3.zero;
            oriGO.transform.localEulerAngles = Vector3.zero;

            // RecordContactInfo.Instance.FindAllGOName2nd(goName);
            goName = RobotMgr.Instance.FindAllGOName(robotname);

            if (goName != null && goName.Count > 0)
            {

                foreach (string goN in goName)
                {
                    //string goType = RecordContactInfo.Instance.FindPickGOType2nd(goN);
                    if (goN != null)
                    {
                        string goType = RobotMgr.Instance.rbt[robotname].gos[goN].goType;

                        // RecordContactInfo.Instance.FindGOPosAngle2nd(goN, out goPos, out goAngle);
                        RobotMgr.Instance.FindGOPosAngle(robotname, goN, out goPos, out goAngle,out goScale);

                        objPos = StringToVector(goPos);

                        objAngle = StringToVector(goAngle);

                        objScale = StringToVector(goScale);

                        

                        ProduceGO(prefabgos["seivo"], objPos, out newt, goN, objAngle);
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
        oriGO.SetActive(false);
        //return oriGO;
    }
    #endregion

    //返回到主场景
    void OnReturnClicked(GameObject go)
    {
        // DontDestroyOnLoad(uir);
        SceneMgr.EnterScene(SceneType.MainWindow);
    }

    //设置模型父子关系
    public void ChangeParent()
    {
        RobotMgr.Instance.ChangeParent(robotName, alldj, AddedAllGOs, alldpbox);
    }

    //重置模型的父子关系
    public void ResetParent()
    {
        RobotMgr.Instance.ResetParent(robotName, oriGO);

    }
    //恢复舵机或者舵盘的坐标和角度
    public void ResetDJDPPA()
    {
        List<string> dpboxName = new List<string>();
        foreach (string key in alldpbox.Keys)
        {
            dpboxName.Add(key);
        }
        for (int i = dpboxName.Count - 1; i >= 0; i--)
        {
            alldpbox[dpboxName[i]].dp.transform.localPosition = alldpbox[dpboxName[i]].dplocalPos;
            alldpbox[dpboxName[i]].dp.transform.localEulerAngles = alldpbox[dpboxName[i]].dplocalAngle;

            alldpbox[dpboxName[i]].box.transform.localPosition = alldpbox[dpboxName[i]].boxlocalPos;
            alldpbox[dpboxName[i]].box.transform.localEulerAngles = alldpbox[dpboxName[i]].boxlocalAngle;

        }


        for (int i = 0; i < AddedAllGOs.Count; i++)
        {
            string goname = AddedAllGOs[i].name;

            if (goPosAll.ContainsKey(goname))
            {
                AddedAllGOs[i].transform.localPosition = goPosAll[goname];
                AddedAllGOs[i].transform.localEulerAngles = goAngleAll[goname];
            }
        }
        //Debug.Log("AddedAllGOs:" + AddedAllGOs.Count);
    }

    //重置oriGO的位置和角度
    public void ResetOriGOPos()
    {
        oriGO.transform.position = oriPos;

        oriGO.transform.localEulerAngles = oriAngle;
    }

    public Transform FindCenter()
    {
        Transform center = null;
        foreach (Transform child in oriGO.GetComponentInChildren<Transform>())
        {
            if (child.name == "CenterObj")
            {
                center = child;
            }
        }
        return center;
    }

    //重置goPosAll，goAngleAll，存储相对位置坐标和角度坐标
    public void RestGOPA()
    {
        goPosAll.Clear();
        goAngleAll.Clear();

        if (AddedAllGOs != null)
        {
            foreach (GameObject keyGO in AddedAllGOs)
            {

                string keytemp = keyGO.name;
                Vector3 posTemp=keyGO.transform.localPosition;
                Vector3 angTemp=keyGO.transform.localEulerAngles;

                goPosAll.Add(keytemp,posTemp);
                goAngleAll.Add(keytemp,angTemp);
            }

        }
    }

    //隐藏线
    public void Hideline()
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if(lines[i].name!="line8")
            {
               lines[i].SetActive(false);
            }
            
        }
    }

    //显示线
    public void Showline()
    {
        for (int i = lines.Count - 1; i >= 0;i-- )
        {
            if (lines[i].name != "line8")
            {
                lines[i].SetActive(true);
            }
        }
    }
}

/// <summary>
/// 选中舵机后返回的物体，和旋转的方向
/// </summary>
public class SelectDuoJiDataOld
{
    public string djName;
    public float rota;
    public float fromRota;
    public int targetRota;
    public GameObject selectObj;
    public float duration;
    public float time;
    public bool isPlaying;

    public SelectDuoJiDataOld()
    {

    }

    public void Play(float rota, int target, float duration)
    {
        this.rota = rota;
        this.fromRota = rota;
        this.targetRota = target;
        this.duration = duration;
        time = 0;
        isPlaying = true;
    }

    public bool Update()
    {
        if (isPlaying)
        {
            time += Time.deltaTime;
            float val = time / duration;
            val = Mathf.Clamp01(val);
            rota = Mathf.Lerp(fromRota, targetRota, val);
            if (Mathf.Abs(rota - targetRota) <= 0.01f)
            {
                rota = targetRota;
                isPlaying = false;
                return true;
            }
        }
        return false;
    }
}


