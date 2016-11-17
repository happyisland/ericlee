using Game.Platform;
using Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ClientMain.cs
/// Description:客户端主入口
/// Time:2015/7/21 9:59:17
/// </summary>
public class ClientMain : SingletonBehaviour<ClientMain>
{
    #region 公有属性
    public delegate void Time_CallBack();
    public static bool Exception_Log_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().debugLogFlag; }
    }
    public static bool Use_Third_App_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().useThirdAppFlag; }
    }
    public static bool Simulation_Use_Third_App_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().simulationUseThirdAppFlag; }
    }

    public static bool Copy_Default_Flag
    {
        get { return SingletonBehaviour<ClientMain>.GetInst().copyDefaultFlag; }
    }

    public bool useThirdAppFlag = false;
    public bool simulationUseThirdAppFlag = false;
    public bool debugLogFlag = true;
    public bool copyDefaultFlag = true;

    private bool useTestModelFlag = false;
    public bool UseTestModelFlag
    {
        get
        {
            return useTestModelFlag;
        }
        set
        {
            if (value)
            {
                SceneMgr.EnterScene(SceneType.EmptyScene);
                SceneManager.GetInst().GotoScene(typeof(TestScene));
            }
            else
            {
                SceneMgr.EnterScene(SceneType.MenuScene);
            }
            useTestModelFlag = value;
        }
    }
    #endregion

    #region 私有属性
    #endregion

    #region 公有函数
    public void SetLogState(bool openFlag)
    {
        debugLogFlag = openFlag;
        Debuger.EnableLog = debugLogFlag;
    }

    public void WaitTimeInvoke(float sec, Time_CallBack callBack)
    {
        StartCoroutine(InvokeCallBack(sec, callBack));
    }
    #endregion

    #region 私有函数
    void Start()
    {
        try
        {
            //永不待机
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //锁定30帧
            Application.targetFrameRate = 30;
            //日志开关
            Debuger.EnableLog = debugLogFlag;
            SingletonObject<UIManager>.GetInst().Init();

            SingletonObject<SceneManager>.GetInst().Init();

            SingletonObject<PopWinManager>.GetInst().Init();
            PlatformMgr plat = gameObject.GetComponent<PlatformMgr>();
            if (null == plat)
            {
                plat = gameObject.AddComponent<PlatformMgr>();
            }
            if (Use_Third_App_Flag)
            {
#if !UNITY_IPHONE
                plat.OpenBluetooth();
#endif
            }
            else
            {
                plat.OpenBluetooth();
            }

            DontDestroyOnLoad(gameObject);
#if !UNITY_ANDROID
            DefaultModelCopy defaultCopy = new DefaultModelCopy();
            defaultCopy.CheckDefaultCopy(CopyDefaultModelFineshed);
#else
            if (useThirdAppFlag)
            {
                CopyDefaultModelFineshed(true);
            }
            else
            {
                DefaultModelCopy defaultCopy = new DefaultModelCopy();
                defaultCopy.CheckDefaultCopy(CopyDefaultModelFineshed);
            }
#endif
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

    void CopyDefaultModelFineshed(bool result)
    {
        if (!Use_Third_App_Flag)
        {
            LoadGameBgTexture();
            LauguageTool.GetIns().SetCurLauguage();
            SceneMgr.EnterScene(SceneType.MenuScene);
        }
        else
        {
#if !UNITY_ANDROID
            SceneMgr.EnterScene(SceneType.EmptyScene);
#endif
        }
        PlatformMgr.Instance.PlatformInit();
        if (Simulation_Use_Third_App_Flag)
        {
            PlatformMgr.Instance.GotoScene("{\"modelID\":\"Baby\",\"modelName\":\"Baby\",\"picPath\":\"E:/RobotUnity3D/Assets/StreamingAssets/defaultFiles/data/customize/image/BABY.jpg\",\"modelType\":0}");
        }
    }

    void Update()
    {
        try
        {
            SingletonObject<SceneManager>.GetInst().Update();
            SingletonObject<PopWinManager>.GetInst().Update();
            NetWork.GetInst().Update();
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

    void LateUpdate()
    {
        try
        {
            SingletonObject<SceneManager>.GetInst().LateUpdate();
            SingletonObject<PopWinManager>.GetInst().LateUpdate();
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

    void OnDestroy()
    {
        try
        {
            SingletonObject<UIManager>.GetInst().Dispose();
            PlatformMgr.Instance.PlatformDestroy();
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
    void OnApplicationQuit()
    {
        try
        {
            PlatformMgr.Instance.DisConnenctBuletooth();
            PlatformMgr.Instance.CloseBluetooth();
            MyLog.CloseMyLog();
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

    void OnApplicationFocus(bool focusStatus)
    {
        if (null != PlatformMgr.Instance && PlatformMgr.Instance.GetBluetoothState())
        {
            if (!focusStatus)
            {
                PlatformMgr.Instance.SetSendXTState(false, false);
            }
            else
            {
                PlatformMgr.Instance.SetSendXTState(true);
            }
        }
    }

    bool isLoadGameBg = false;
    public void LoadGameBgTexture()
    {
        if (isLoadGameBg)
        {
            return;
        }
        isLoadGameBg = true;
        UIRoot root = GameHelper.FindChildComponent<UIRoot>(transform, "UI Root");
        if (null != root)
        {
            root.manualHeight = PublicFunction.RootManualHeight;
        }
        GameObject obj = Resources.Load("Prefab/UI/gamebg", typeof(GameObject)) as GameObject;
        if (null != obj)
        {
            GameObject o = UnityEngine.Object.Instantiate(obj) as GameObject;
            o.name = obj.name;
            Transform camera = transform.Find("UI Root/Camera");
            if (null != camera)
            {
                o.transform.parent = camera;
                o.transform.localEulerAngles = Vector3.zero;
                o.transform.localPosition = Vector3.zero;
                o.transform.localScale = Vector3.one;
            }
        }
    }


    public void LoadTexture(string pic, int waitTime)
    {
        if (!string.IsNullOrEmpty(pic))
        {
            GameObject root = GameObject.Find("UIRoot(2D)(2)/UICamera/Center");
            if (null != root)
            {
                Transform test = root.transform.Find("test");
                if (null == test)
                {
                    test = NGUITools.AddChild(root).transform;
                    test.name = "test";
                    test.gameObject.AddComponent<UIPanel>();
                }
                test.localPosition = Vector3.zero;
                test.localScale = Vector3.one;
                UITexture tex1 = AddTex(test, "tex1");
                tex1.transform.localPosition = new Vector3(-200, 0, 0);
                UITexture tex2 = AddTex(test, "tex2");
                tex2.transform.localPosition = new Vector3(200, 0, 0);
                tex1.mainTexture = null;
                tex2.mainTexture = null;
                Resources.UnloadUnusedAssets();
                StartCoroutine(LoadTex(tex1, "file://" + pic, waitTime));
                StartCoroutine(LoadTex(tex2, "file:///" + pic, waitTime));
            }
        }
    }

    UITexture AddTex(Transform parent, string name)
    {
        Transform texTrans = parent.Find(name);
        if (null == texTrans)
        {
            texTrans = NGUITools.AddChild(parent.gameObject).transform;
            texTrans.localScale = Vector3.one;
            texTrans.name = name;
        }
        UITexture tex = texTrans.GetComponent<UITexture>();
        if (null == tex)
        {
            tex = texTrans.gameObject.AddComponent<UITexture>();
            tex.depth = 2;
            tex.material = new Material(Shader.Find("Unlit/Transparent Colored"));
            tex.width = 200;
            tex.height = 200;
        }
        return tex;
    }


    IEnumerator LoadTex(UITexture uiTex, string name, int waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        string path = name;
        
        WWW www = new WWW(path);
        
        yield return www;
        if (www.isDone && null == www.error)
        {
            try
            {
                uiTex.mainTexture = www.texture;
            }
            catch (System.Exception ex)
            {
                Debuger.Log(ex.ToString());
            }
        }
        else
        {
            Debuger.Log("not done " + path);
        }
    }

    IEnumerator InvokeCallBack(float sec, Time_CallBack callBack)
    {
        yield return new WaitForSeconds(sec);
        callBack();
    }


#if USE_TEST
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(300);
        GUILayout.EndVertical();
        if (GUILayout.Button("进入测试模式", GUILayout.Width(100), GUILayout.Height(60)))
        {
            UseTestModelFlag = true;
        }
    }
#endif
#endregion
}