//----------------------------------------------
//            积木2: sunyu
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Scene;
using Game.UI;
using System.IO;
using Game.Platform;
using Game.Event;
using System.Text;
using Game;
using Game.Resource;

public class ModelDetailsWindow_new : MonoBehaviour
{
    bool IsOfficial; //是否是官方
    bool IsCameraState;
    bool IsFirstCamera;
    bool IsFirstVideo;
    bool IsActionScene;
    bool hasSelectAction;
    bool IsCameraExitSS;
    string RobotName;
    public Transform officialTrans;
    public Transform defaultTrans;
    private Transform Effect_btn;
    private Transform Controller_btn;
    private Transform Logic_btn;
    private Transform BT_button;
    public Transform Back_button;
    private Transform Reset_button;
    private Transform Power_button;
    private Transform Camera_button;
    private Transform Play_button;
    private Transform Delete_button;
    private Transform Edit_button;
    private Transform Connect_button;
    private Transform GuideCamera_button;
    private Transform GuideVideo_button;
    private Transform ScrollActionItems;
    private Transform FirstCameraEnter;
    private Transform FirstVideoEnter;
    private Transform Giveup_button;
    private Transform Saved_Button;
    //private Transform StopPlay_Button;
    private Transform StartPlay_Button;

    private Transform CameraBgPanel;
    private Transform CameraAreas;

    private Transform CameraIconTip;
    private Transform CameraGuideLine;
    private Transform CameraTextTips;

    private Transform VideoIconTip;
    private Transform VideoGuideLine2;
    private Transform VideoGuideLine3;
    private Transform VideoGuideLine4;
    private Transform VideoTextTip1;
    private Transform VideoTextTip2;
    private Transform BluetoothIconTip;
    private Transform ActionIconTip;
   
    private UITexture PhotoTexture;
    private GameObject AddButton;
    private GameObject TakePhotoObj;
    private GameObject PlayAcionTool;
    public UILabel robotName;
    public Camera UICam;
    bool _isBackCommunity;
    bool isBackCommunity
    {
        get
        {
            return _isBackCommunity;

        }
        set
        {
            _isBackCommunity = value;
            if (value)
            {
                GameObjectManage.Ins.ClearData(); //进入社区时 清除图片数据
                PlatformMgr.Instance.Pic_Path = string.Empty;
                //  transform.gameObject.SetActive(false);
            }
        }
    }
    void StartHideLeftIcom()
    {
        StartCoroutine(HideLeftIcom());
    }
    IEnumerator HideLeftIcom()
    {
        Vector3 to = Vector3.zero;
        if (Back_button != null)
        {
            to = Back_button.localPosition;
            Back_button.localPosition = new Vector3(to.x - 160, to.y, to.z);
            Back_button.gameObject.SetActive(false);
        }


        yield return null;
        yield return new WaitForEndOfFrame();

        if (Back_button != null)
        {
            // BT_button.gameObject.SetActive(false);

            Back_button.localPosition = new Vector3(to.x - 160, to.y, to.z);
            Transform nullT = null;
            Back_button.GetComponent<UIWidget>().SetAnchor(nullT);
            Back_button.gameObject.SetActive(true);
            TweenPosition twe = null;
            if (Back_button.GetComponent<TweenPosition>() == null)
            {
                twe = Back_button.gameObject.AddComponent<TweenPosition>();
                twe.from = Back_button.localPosition;
                twe.to = to;
                twe.duration = 0.5f;
            }
            if (twe != null)
                twe.PlayForward();
        }
    }

    /// <summary>
    /// 返回
    /// </summary>
    /// <param name="go"></param>
    public void GoBack(GameObject go)
    {
        if (IsActionScene)
            IsActionScene = false;

        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Exit unity scene!!");

        isBackCommunity = true;
        try
        {
            RobotMgr.Instance.GoToCommunity();
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }

        //  GameObjectManage.Ins.ClearData(); //进入社区时 清除图片数据
        //   PlatformMgr.Instance.Pic_Path = string.Empty;
        //   transform.gameObject.SetActive(false);
        if (ClientMain.Use_Third_App_Flag)
        {
            SceneMgr.EnterScene(SceneType.EmptyScene);
            PlatformMgr.Instance.BackThirdApp();
        }
        else
        {
            SceneMgr.EnterScene(SceneType.MenuScene);
        }
    }
    //EventDelegate.Callback EnterOtherMode;
    void LogicProgramIN(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click logic program button!!");
        //HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("暂不开放"));
        //return;
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedCodingButton);
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {
            if (IsProtected())
            {
                HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("adpateProtected"));
                return;
            }
            //EnterScene(SceneType.testScene);
            SceneMgr.EnterScene(SceneType.EmptyScene);
            SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(robot);
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
    }
    void EnterScene(SceneType type, int index = 0) //index = 1 表示逻辑编程， 2表示发布
    {
        ShowRightBar(false);
        StartCoroutine(GoToScene(0.5f, type, index));
    }
    IEnumerator GoToScene(float time, SceneType type, int index)
    {
        yield return new WaitForSeconds(time);
        if (type != SceneType.testScene)
            SceneMgr.EnterScene(type);
        else if (index == 2)   //发布
            PlatformMgr.Instance.PublishModel(RobotMgr.NameNoType(RobotManager.GetInst().GetCurrentRobot().Name));
        else if (index == 1)  //逻辑编程
            SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(RobotManager.GetInst().GetCurrentRobot());
    }
    /// <summary>
    /// 点击蓝牙
    /// </summary>
    /// <param name="go"></param>
    public void BlueTooClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click bluetooth connect!!");

        if (IsCameraState && !PlatformMgr.Instance.IsChargeProtected)
        {
#if UNITY_ANDROID
            StartPlay_Button.GetChild(0).gameObject.SetActive(false);
#endif
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GetBluetoothCurrentState, "");
        }
        PublicPrompt.ShowClickBlueBtnMsg();
    }
    /// <summary>
    /// 复位视角
    /// </summary>
    /// <param name="go"></param>
    public void ResetClick(GameObject go)
    {
        if (null != go)
        {
            Camera.main.GetComponent<CamRotateAroundCircle>().MobClickOriginalPosition();
        }
        Camera.main.GetComponent<CamRotateAroundCircle>().ResetOriState();
    }
    /// <summary>
    /// 动态图纸
    /// </summary>
    /// <param name="go"></param>
    public void BuildClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click build animation button!!");
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedBuildModelButton);
        if (RobotManager.GetInst().GetCurrentRobot() == null)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
            return;
        }

        if (RobotMgr.Instance.hideGOs != null && RobotMgr.Instance.hideGOs.Count > 0)
        {
            for (int i = RobotMgr.Instance.hideGOs.Count - 1; i >= 0; i--)
            {
                RobotMgr.Instance.hideGOs[i].SetActive(true);
            }
        }

        GameObject oriGO = GameObject.Find("oriGO");
        if (oriGO != null)
        {
            DontDestroyOnLoad(oriGO);
        }
        SceneMgr.EnterScene(SceneType.Assemble);
    }
    /// <summary>
    /// 动作
    /// </summary>
    /// <param name="go"></param>
    private bool _showPlaylist;
    private bool ShowPlaylist
    {
        get
        {
            return _showPlaylist;
        }
        set
        {
            _showPlaylist = value;
            if (value)                          //动作表
            {
                if (TakePhotoObj.GetComponentInChildren<BoxCollider>() != null)
                    TakePhotoObj.GetComponentInChildren<BoxCollider>().enabled = false;

                if (Back_button != null)
                    UIEventListener.Get(Back_button.gameObject).onClick = BackEffect;
              //  BT_button.gameObject.SetActive(false);

                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "Display action list!!");

                if (!IsActionScene)
                    IsActionScene = true;

                if (Power_button != null && IsActionScene)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now power icon is hidded for action list is show!!");
                    Power_button.gameObject.SetActive(false);
                }

                robotName.gameObject.SetActive(false);

                ShowRightBar(false);
                if (Camera_button != null && !IsCameraState)
                {
                    Camera_button.gameObject.SetActive(true);
                    UIEventListener.Get(Camera_button.gameObject).onClick = CameraClick;
                }

                if (!flagex)
                {
                    ShowBottomBar(true, 0.5f);
                    
                }
                else
                {
                    flagex = false;
                    ShowBottomBar(true, 0.1f);
                    
                }

                //进入动作表时 插上电源了
                if (IsProtected())
                {
                    PublicPrompt.ShowChargePrompt(GoBack);
                }
            }
            else
            {
                hasSelectAction = false;

                if (TakePhotoObj.GetComponentInChildren<BoxCollider>() != null)
                    TakePhotoObj.GetComponentInChildren<BoxCollider>().enabled = true;

                if (IsActionScene)
                    IsActionScene = false;

                if (Back_button != null)
                    UIEventListener.Get(Back_button.gameObject).onClick = GoBack;
                
                OnBlueConnectResult(new EventArg(PlatformMgr.Instance.GetBluetoothState())); //蓝牙

                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "Hide action list!!");
                //Power_button.gameObject.SetActive(true);

                if (Power_button != null && PlatformMgr.Instance.GetBluetoothState())
                    StartCoroutine(ShowPowerIcons(0.3f));
                

                robotName.gameObject.SetActive(IsOfficial);

                if (Camera_button != null)
                {
                    Camera_button.gameObject.SetActive(false);
                }
                ShowBottomBar(false);
                
                if (!flagex1)
                {
                    ShowRightBar(true, 0.5f);
                }
                else
                {
                    flagex1 = false;
                    ShowRightBar(true, 0.1f);
                }
            }
        }
    }
    public void CameraClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click video button!!");
        IsCameraState = true;
        //IsCameraExitSS = true;

        CameraBgPanel.gameObject.SetActive(true);
        CameraAreas.gameObject.SetActive(false);

        if (AddButton != null)
        {
            AddButton.gameObject.SetActive(false);
        }
        if (Power_button != null)
        {
            Power_button.gameObject.SetActive(false);
        }
        
#if UNITY_ANDROID        
        if (IsFirstVideo)
        {
            FirstVideoEnter.gameObject.SetActive(true);
            VideoIconTip.gameObject.SetActive(true);
            VideoGuideLine2.gameObject.SetActive(true);
            VideoGuideLine3.gameObject.SetActive(true);
            VideoGuideLine4.gameObject.SetActive(true);
            VideoTextTip1.gameObject.SetActive(true);
            VideoTextTip2.gameObject.SetActive(true);
            BluetoothIconTip.gameObject.SetActive(true);
            ActionIconTip.gameObject.SetActive(true);

            CameraAreas.gameObject.SetActive(true);

            GuideVideo_button.gameObject.SetActive(true);
            UIEventListener.Get(GuideVideo_button.gameObject).onClick = EnterVideoPlayScene;

            UILabel VideoTips1 = VideoTextTip1.gameObject.GetComponent<UILabel>();
            VideoTips1.text = LauguageTool.GetIns().GetText("录制视频引导");
            UILabel VideoTips2 = VideoTextTip2.gameObject.GetComponent<UILabel>(); 
            VideoTips2.text = LauguageTool.GetIns().GetText("连接蓝牙运动提示");

            UILabel GuideButtonTips2 = GuideVideo_button.GetChild(0).GetComponent<UILabel>();
            GuideButtonTips2.text = LauguageTool.GetIns().GetText("首次引导按钮");
        } 

        if (hasSelectAction)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "Have selected any action!!");
            Edit_button.gameObject.SetActive(false);
            GameObject eroo = null;
            Play_button.gameObject.GetComponent<UISprite>().SetAnchor(eroo);
            Vector2 pos22 = Play_button.localPosition;
            pos22.x = pos22.x + (261.0f * PublicFunction.GetWidth() / 1334.0f);
            Play_button.localPosition = pos22;

            if (RecordContactInfo.Instance.openType == "default")
            {
                Delete_button.gameObject.SetActive(false);
            }
            else
            {
                Delete_button.gameObject.SetActive(true);
                GameObject ccf = null;
                Delete_button.gameObject.GetComponent<UISprite>().SetAnchor(ccf);
                Vector2 pos24 = Delete_button.localPosition;
                pos24.x = pos24.x + (261.0f * PublicFunction.GetWidth() / 1334.0f);
                Delete_button.localPosition = pos24;
                Delete_button.gameObject.SetActive(false);
            }
        }
#endif
#if UNITY_IPHONE
        if (hasSelectAction)
        {
            Edit_button.gameObject.SetActive(false);
            GameObject ero = null;
            Play_button.gameObject.GetComponent<UISprite>().SetAnchor(ero);
            Vector2 pos22 = Play_button.localPosition;
            pos22.x = pos22.x + (132.0f * PublicFunction.GetWidth() / 1334.0f);
            Play_button.localPosition = pos22;

            if (RecordContactInfo.Instance.openType == "default")
            {
                Delete_button.gameObject.SetActive(false);
            }
            else
            {
                Delete_button.gameObject.SetActive(true);
                GameObject ccf = null;
                Delete_button.gameObject.GetComponent<UISprite>().SetAnchor(ccf);
                Vector2 pos24 = Delete_button.localPosition;
                pos24.x = pos24.x + (132.0f * PublicFunction.GetWidth() / 1334.0f);
                Delete_button.localPosition = pos24;
                Delete_button.gameObject.SetActive(false);
            }
        }       
#endif
#if UNITY_ANDROID
        if (Giveup_button != null)
        {
            Giveup_button.gameObject.SetActive(true);
            UIEventListener.Get(Giveup_button.gameObject).onClick = GiveupCurrentVideo;
            //UIEventListener.Get(Giveup_button.gameObject).onClick = GiveupVideoState;
        }
        if (StartPlay_Button != null)
        {
            StartPlay_Button.gameObject.SetActive(true);
            StartPlay_Button.GetChild(0).gameObject.SetActive(false);
            StartPlay_Button.GetChild(1).gameObject.SetActive(true);
            UIEventListener.Get(StartPlay_Button.gameObject).onClick = PlayingCurrentVideo;
            //UIEventListener.Get(StartPlay_Button.gameObject).onClick = ReplayVideoState;
        }
#endif

        ScrollActionItems = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList/scrollRect").transform;

        GameObject eeo = null;
        ScrollActionItems.GetComponent<UIPanel>().SetAnchor(eeo);

        Vector4 size3 = ScrollActionItems.GetComponent<UIPanel>().baseClipRegion;
        size3.z = size3.z + (200.0f * PublicFunction.GetWidth() / 1334.0f);
        size3.x = size3.x + (100.0f * PublicFunction.GetWidth() / 1334.0f);
        ScrollActionItems.GetComponent<UIPanel>().baseClipRegion = size3;

        Vector3 pos3 = ScrollActionItems.localPosition;
        
        pos3.x = pos3.x - (179.0f * PublicFunction.GetWidth() / 1334.0f);
        //Debug.Log("scrollaction is " + pos3.x);
        ScrollActionItems.localPosition = pos3;

        //ScrollActionItems.GetComponent<UIPanel>().SetAnchor(GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList"));

        if (Reset_button != null)
        {
            Vector3 pos1 = Reset_button.localPosition;
            GameObject emp = null;
            Connect_button = BT_button.parent;
            Connect_button.GetComponent<UIWidget>().SetAnchor(emp);
            Connect_button.localPosition = pos1;

            Reset_button.gameObject.SetActive(false);
        }

        Back_button.gameObject.SetActive(false);
        Camera_button.gameObject.SetActive(false);
        GameObject modelObj = GameObject.Find("oriGO");
        PublicFunction.SetLayerRecursively(modelObj, LayerMask.NameToLayer("Default"));

#if UNITY_IPHONE
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GetCameraCurrentState, ""); 
#endif

#if UNITY_ANDROID
        if (!IsFirstVideo)
        {
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GetCameraCurrentState, ""); 
        }        
#endif
    }
    public void EnterVideoPlayScene(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Enter video scene(only android)!!");
        if (FirstVideoEnter != null && FirstVideoEnter.gameObject.activeSelf)
        {
            IsFirstVideo = false;
            CameraAreas.gameObject.SetActive(false);
            FirstVideoEnter.gameObject.SetActive(false);
#if UNITY_ANDROID
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GetCameraCurrentState, "");
#endif
        }
    }
    public void GiveupCurrentVideo(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Give up video!!");
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.GiveupCurrentVideo, ""); 
    }
    public void GiveupVideoState(object go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Exit video scene!!");

        if (IsCameraState)
        {
            IsCameraExitSS = true;
            IsCameraState = false;
        }
                
        CameraBgPanel.gameObject.SetActive(false);

        if (Power_button != null)
        {
            //StartCoroutine(ShowPowerIcons(0.2f));
            Power_button.gameObject.SetActive(false);
        }

        if (hasSelectAction)
        {
            Edit_button.gameObject.SetActive(true);
            UIEventListener.Get(Edit_button.gameObject).onClick = DoEditAction;
        }        

#if UNITY_ANDROID
        if (Giveup_button != null)
            Giveup_button.gameObject.SetActive(false);
        if (StartPlay_Button != null)
            StartPlay_Button.gameObject.SetActive(false);
        if (Saved_Button != null)        
            Saved_Button.gameObject.SetActive(false);
#endif

        Back_button.gameObject.SetActive(true);
        Camera_button.gameObject.SetActive(true);
        GameObject modelObj = GameObject.Find("oriGO");
        PublicFunction.SetLayerRecursively(modelObj, LayerMask.NameToLayer("Robot"));

        if (Reset_button != null)
        {
            Vector3 pos2 = Reset_button.localPosition;

            pos2.y = pos2.y + (118.0f * PublicFunction.GetHeight() / 750.0f);
            Connect_button.localPosition = pos2;
            Connect_button.GetComponent<UIWidget>().SetAnchor(Reset_button);

            Reset_button.gameObject.SetActive(true);
        }
        
        if (AddButton != null)
        {
            AddButton.gameObject.SetActive(true);
        }        

        Vector4 size3 = ScrollActionItems.GetComponent<UIPanel>().baseClipRegion;
        size3.z = size3.z - (200.0f * PublicFunction.GetWidth() / 1334.0f);
        size3.x = size3.x - (100.0f * PublicFunction.GetWidth() / 1334.0f);
        ScrollActionItems.GetComponent<UIPanel>().baseClipRegion = size3;

        Vector3 pos3 = ScrollActionItems.localPosition;
        if (pos3.x < (-500.0f * PublicFunction.GetWidth() / 1334.0f))
            pos3.x = pos3.x + (179.0f * PublicFunction.GetWidth() / 1334.0f);
        ScrollActionItems.localPosition = pos3;

        ScrollActionItems.GetComponent<UIPanel>().SetAnchor(GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList"));

        //Edit_button.gameObject.SetActive(false);

#if UNITY_ANDROID
        Vector2 pos22 = Play_button.localPosition;
        pos22.x = pos22.x - (261.0f * PublicFunction.GetWidth() / 1334.0f);

        Play_button.localPosition = pos22;
        Play_button.gameObject.GetComponent<UISprite>().SetAnchor(GameObject.Find("MainUIRoot_new"));

        //Play_button.gameObject.SetActive(false);

        if (Delete_button != null)
        {
            Vector2 pos24 = Delete_button.localPosition;
            pos24.x = pos24.x - (261.0f * PublicFunction.GetWidth() / 1334.0f);
            Delete_button.localPosition = pos24;
            Delete_button.gameObject.GetComponent<UISprite>().SetAnchor(GameObject.Find("MainUIRoot_new"));
            if (RecordContactInfo.Instance.openType != "default" && RobotManager.GetInst().GetCurrentRobot().GetActionsIdList().Count != 0 && hasSelectAction)
            {
                Delete_button.gameObject.SetActive(true);
            }
            //Delete_button.gameObject.SetActive(false);           
        }
        ResetClick(null);
#endif

#if UNITY_IPHONE
        Vector2 pos25 = Play_button.localPosition;
        
        pos25.x = pos25.x - (132.0f * PublicFunction.GetWidth() / 1334.0f);
        Play_button.localPosition = pos25;
        Play_button.gameObject.GetComponent<UISprite>().SetAnchor(GameObject.Find("MainUIRoot_new"));

        if (Delete_button != null)
        {
            Vector2 pos27 = Delete_button.localPosition;
            pos27.x = pos27.x - (132.0f * PublicFunction.GetWidth() / 1334.0f);
            Delete_button.localPosition = pos27;
            Delete_button.gameObject.GetComponent<UISprite>().SetAnchor(GameObject.Find("MainUIRoot_new"));
            if (RecordContactInfo.Instance.openType != "default" && RobotManager.GetInst().GetCurrentRobot().GetActionsIdList().Count != 0 && hasSelectAction)
            {
                Delete_button.gameObject.SetActive(true);
            }
            //Delete_button.gameObject.SetActive(false);
        }      
#endif
    }
    public void PlayingCurrentVideo(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Start playing video!!");
        if (StartPlay_Button.GetChild(1).GetComponent<UISprite>().spriteName == "icon_startplay")
        {
            //Debug.Log("start play video!!");
            StartPlay_Button.GetChild(0).gameObject.SetActive(false);
            StartPlay_Button.GetChild(1).gameObject.SetActive(false);
            if (Saved_Button != null)
            {
                Saved_Button.gameObject.SetActive(true);
                Saved_Button.GetChild(0).gameObject.SetActive(true);
                Saved_Button.GetChild(1).gameObject.SetActive(false);
                Saved_Button.GetChild(2).gameObject.SetActive(true);
                //UIEventListener.Get(Saved_Button.gameObject).onClick = SaveCurrentVideo;
            }
            //StartPlay_Button.GetChild(1).GetComponent<UISprite>().spriteName = "icon_playing";
            
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.StartPlayingVideo, ""); 
        }
        /*else if (StartPlay_Button.GetChild(1).GetComponent<UISprite>().spriteName == "icon_playing")
        {
            //Debug.Log("stop play video!!");
            StartPlay_Button.GetChild(1).GetComponent<UISprite>().spriteName = "icon_startplay";
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.StopPlayingVideo, ""); 
        }*/
    }
    public void ReplayVideoState(object go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Replay video!!");
        IsCameraState = true;

        CameraBgPanel.gameObject.SetActive(true);
        CameraAreas.gameObject.SetActive(false);

        if (AddButton != null)
        {
            AddButton.gameObject.SetActive(false);
        }
        if (Power_button != null)
        {
            Power_button.gameObject.SetActive(false);
        }
        if (Camera_button != null)
        {
            Camera_button.gameObject.SetActive(false);
        }

#if UNITY_ANDROID
        CameraClick(null);        
#endif
    }
    public void CurrentStartPlaying(object go)
    {
        /*if (Saved_Button != null)
        {
            Saved_Button.gameObject.SetActive(true);
            Saved_Button.GetChild(0).gameObject.SetActive(true);
            Saved_Button.GetChild(1).gameObject.SetActive(false);
            Saved_Button.GetChild(2).gameObject.SetActive(true);
            //UIEventListener.Get(Saved_Button.gameObject).onClick = SaveCurrentVideo;
        }*/
    }
    public void CurrentSaveVideo(object go)
    {
        if (Saved_Button != null)
        {
            Saved_Button.gameObject.SetActive(true);
            Saved_Button.GetChild(0).gameObject.SetActive(false);
            Saved_Button.GetChild(1).gameObject.SetActive(true);
            Saved_Button.GetChild(2).gameObject.SetActive(false);
            UIEventListener.Get(Saved_Button.gameObject).onClick = SaveCurrentVideo;
        }
        /*if (StartPlay_Button != null)
        {
            StartPlay_Button.GetChild(0).gameObject.SetActive(false);
            StartPlay_Button.GetChild(1).gameObject.SetActive(true);
        }*/    
    }
    public void SaveCurrentVideo(GameObject go)
    {
        PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.SavedCurrentVideo, ""); 
    }
    /// <summary>
    /// 退出蓝牙
    /// </summary>
    /// <param name="go"></param>
    public void ExitCameraBTConnect(EventArg arg)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Exit bluetooth connect in camera scene!!");

        if (IsCameraState && !PlatformMgr.Instance.IsChargeProtected)
        {
            if (Power_button != null)
            {
                Power_button.gameObject.SetActive(false);
            }
            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ExitBluetoothCurrentState, "");
#if UNITY_ANDROID
            if (StartPlay_Button != null)
            {
                StartPlay_Button.gameObject.SetActive(true);
                StartPlay_Button.GetChild(0).gameObject.SetActive(false);
                StartPlay_Button.GetChild(1).gameObject.SetActive(true);
                Saved_Button.gameObject.SetActive(false);
                UIEventListener.Get(StartPlay_Button.gameObject).onClick = PlayingCurrentVideo;
            }           
#endif
        }
        else if (PlatformMgr.Instance.IsChargeProtected)
        {
            if (!Reset_button.gameObject.activeSelf)
                StartCoroutine(ShowResetIcons(0.3f));
        }  
    }

    public void EffectClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click effect button!!");
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedActionListButton);
        if (IsProtected())
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("adpateProtected"));
            return;
        }
        IsActionScene = true;
        ShowPlaylist = true;

        if (IsCameraExitSS)
        {
            IsCameraExitSS = false;           
        }     

        StartCoroutine(DisplayCameraGuide(1.0f));
    }
    IEnumerator DisplayCameraGuide(float t)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "display camera guide!!");
        yield return new WaitForSeconds(t);
        if (IsFirstCamera) //  读取文件 文件不存在需要开启指引pages
        {
            IsFirstCamera = false;
            FirstVideoEnter.gameObject.SetActive(false);
            FirstCameraEnter.gameObject.SetActive(true);
            CameraIconTip.gameObject.SetActive(true);
            CameraGuideLine.gameObject.SetActive(true);
            CameraTextTips.gameObject.SetActive(true);
            GuideCamera_button.gameObject.SetActive(true);
            UIEventListener.Get(GuideCamera_button.gameObject).onClick = EnterCameraPlayScene;

            UILabel CameraTips = CameraTextTips.gameObject.GetComponent<UILabel>();
            CameraTips.text = LauguageTool.GetIns().GetText("边玩边拍");
            UILabel GuideButtonTips = GuideCamera_button.GetChild(0).GetComponent<UILabel>();
            GuideButtonTips.text = LauguageTool.GetIns().GetText("首次引导按钮");
            StreamWriter sw = System.IO.File.CreateText(GuidePagesConfig);
            sw.Write("{\"instruction1\":\"0\"}");
            sw.Close();
        }
        else
        {
            CameraIconTip.gameObject.SetActive(false);
            CameraGuideLine.gameObject.SetActive(false);
            CameraTextTips.gameObject.SetActive(false);
            GuideCamera_button.gameObject.SetActive(false);
            FirstCameraEnter.gameObject.SetActive(false);
            FirstVideoEnter.gameObject.SetActive(false);
        }
    }
    void BackEffect(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click back button in effect scene!!");
        if (IsActionScene)
            IsActionScene = false;
        OnActionClick(null);
        ShowPlaylist = false;
        if (IsCameraExitSS && Reset_button != null && !Reset_button.gameObject.activeSelf)
        {
            //IsCameraExitSS = true;
            StartCoroutine(ShowResetIcons(0.2f));
        }
        
        ResetClick(null);

        //StartCoroutine(ShowResetIcons(0.5f));      
        //Power_button.gameObject.SetActive(true);
    }
    IEnumerator ShowResetIcons(float t)
    {
        yield return new WaitForSeconds(t);
        Reset_button.gameObject.SetActive(true);
        Connect_button.GetComponent<UIWidget>().SetAnchor(Reset_button);
    }
    public void EnterCameraPlayScene(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click sure button in guide panel!!");
        if (FirstCameraEnter != null && FirstCameraEnter.gameObject.activeSelf)
        {
            FirstCameraEnter.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 遥控器
    /// </summary>
    /// <param name="go"></param>
    public void ControlClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click controller button!!");
        PlatformMgr.Instance.MobClickEvent(MobClickEventID.ModelPage_TappedControllerButton);
        if (RobotManager.GetInst().GetCurrentRobot() == null)
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
            return;
        }
        if (IsProtected())
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("adpateProtected"));
            return;
        }
        GameObject oriGO = GameObject.Find("oriGO");
        if (oriGO != null)
        {
            //DoStopAction(oriGO);
            //MoveSecond.Instance.ResetDJDPPA();
            MoveSecond.Instance.ResetParent();
            MoveSecond.Instance.ResetDJDPPA();
            MoveSecond.Instance.RestGOPA();
            MoveSecond.Instance.ChangeParent();
            //PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Arrow"));
            DontDestroyOnLoad(oriGO);
        }
        SceneMgrTest.Instance.LastScene = SceneType.ActionPlay;

        SceneMgr.EnterScene(SceneType.ActionPlay);
        //添加模型背景
        GameObject obj = GameObject.Find("Camera");
        if (obj.GetComponent<MoveSecond>() != null)
            DestroyImmediate(obj.GetComponent<MoveSecond>());
        if (obj.GetComponent<CamRotateAroundCircle>() != null)
            DestroyImmediate(obj.GetComponent<CamRotateAroundCircle>());
    }
    /// <summary>
    /// 社区
    /// </summary>
    /// <param name="go"></param>
    public void CommunityClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click community button!!");
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            PlatformMgr.Instance.PublishModel(RobotMgr.NameNoType(RobotManager.GetInst().GetCurrentRobot().Name));
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
        }
    }

    public void AddNewAction(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click new effect button!!");
        
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            SceneMgrTest.Instance.LastScene = SceneType.EditAction;

            GameObject oriGO = GameObject.Find("oriGO");
            if (oriGO != null)
            {
                //MoveSecond.Instance.ResetDJDPPA();
                MoveSecond.Instance.ResetParent();
                MoveSecond.Instance.ResetDJDPPA();
                //PublicFunction.SetLayerRecursively(oriGO, LayerMask.NameToLayer("Default"));
                DontDestroyOnLoad(oriGO);
            }
            ActionEditScene.CreateActions(string.Empty, string.Empty);
        }
        else
        {
            HUDTextTips.ShowTextTip(LauguageTool.GetIns().GetText("请重新选择或创建模型"));
            return;
        }
    }

    /// <summary>
    /// 底部显示或消失
    /// </summary>
    /// <param name="isShow"></param>
    void ShowBottomBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        StartCoroutine(HideOrShowBottomBar(isShow, time, call));
    }
    IEnumerator HideOrShowBottomBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        yield return null;
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        if (time > 0)
            yield return new WaitForSeconds(time);
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;
        Transform trans = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList").transform;
        if (trans != null)
        {
            Transform nullTran = null;
            trans.GetComponent<UIWidget>().SetAnchor(nullTran);
            int hh = trans.GetComponent<UIWidget>().height;
            MyAnimtionCurve cur1 = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            if (trans.GetComponent<TweenPosition>() == null)
            {
                trans.gameObject.AddComponent<TweenPosition>();
                trans.GetComponent<TweenPosition>().from = trans.localPosition;
                trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y + hh, trans.localPosition.z);
                trans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
            }
            tp = trans.GetComponent<TweenPosition>();
            tp.duration = 0.5f;

            TweenPosition tween = null;
            Transform moveTrans = null;
            if (IsOfficial)
            {
                if (Reset_button != null)
                    moveTrans = Reset_button;
            }
            else
            {
                if (BT_button != null)
                    moveTrans = BT_button.parent;
            }
            if (moveTrans != null)
            {
                moveTrans.GetComponent<UIWidget>().SetAnchor(nullTran);
                if (moveTrans.GetComponent<TweenPosition>() == null)
                {
                    moveTrans.gameObject.AddComponent<TweenPosition>();
                    moveTrans.GetComponent<TweenPosition>().from = moveTrans.localPosition;
                    moveTrans.GetComponent<TweenPosition>().to = new Vector3(moveTrans.localPosition.x, moveTrans.localPosition.y + hh - 27, moveTrans.localPosition.z);
                    moveTrans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
                }
                tween = moveTrans.GetComponent<TweenPosition>();
                tween.duration = 0.5f;
            }
            if (call != null)
            {
                tp.AddOnFinished(call);
            }
            if (isShow)
            {
                tp.PlayForward();
                if (tween != null)
                    tween.PlayForward();
            }
            else
            {
                tp.PlayReverse();
                if (tween != null)
                    tween.PlayReverse();
            }
        }

    }

    public void ShowRightBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        StartCoroutine(HideOrShowRightBar(isShow, time, call));  //采用携程，考虑到一开始描点需要时间
    }
    IEnumerator HideOrShowRightBar(bool isShow, float time = 0, EventDelegate.Callback call = null)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        if (time > 0)
            yield return new WaitForSeconds(time);
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;
        Transform trans = null;
        if (IsOfficial)
        {
            trans = officialTrans;
        }
        else
        {
            trans = defaultTrans;
        }
        Transform temp = null;
        trans.GetComponent<UIWidget>().SetAnchor(temp);
        from = trans.localPosition;
        if (trans.GetComponent<TweenPosition>() == null)
        {
            trans.gameObject.AddComponent<TweenPosition>();
            to = new Vector3(from.x - 160, from.y, from.z);
            MyAnimtionCurve myCurve = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            trans.GetComponent<TweenPosition>().animationCurve = myCurve.animCurve;
            trans.GetComponent<TweenPosition>().duration = 0.5f;
            trans.GetComponent<TweenPosition>().from = from;
            trans.GetComponent<TweenPosition>().to = to;
        }
        tp = trans.GetComponent<TweenPosition>();
        if (call != null)
        {
            tp.AddOnFinished(call);
        }
        if (isShow)
        {
            tp.PlayForward();
        }
        else
        {
            tp.PlayReverse();
        }

    }

    /// <summary>
    /// 播放动作
    /// </summary>
    /// <param name="go"></param>
    void DoPlayAction(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Play robot action!!");
        RobotManager.GetInst().GetCurrentRobot().StopRunTurn();
        ActionLogic.GetIns().OnPlayBtnClicked(go);
        if (!PlatformMgr.Instance.GetBluetoothState()) //未连接的情况
        {
            Animation anim = BT_button.GetComponent<Animation>();
            anim.Play();
        }
    }
    IEnumerator tweenFinshed(TweenPosition t,float tt = 0.3f)
    {
        yield return new WaitForSeconds(tt);
        if (t != null)
            t.PlayReverse();
    }
    /// <summary>
    /// 动作停止
    /// </summary>
    /// <param name="go"></param>
    void DoStopAction(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Stop robot action!!");
        ActionLogic.GetIns().DoStopAction(go);
        Play_button.GetChild(0).GetComponent<UISprite>().spriteName = "icon_play";
    }
    /// <summary>
    /// 动作编辑
    /// </summary>
    /// <param name="go"></param>
    void DoEditAction(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click edit button out of camera scene!!");
        GameObject oriGO = GameObject.Find("oriGO");
        SceneMgrTest.Instance.LastScene = SceneType.EditAction;
        
        if (oriGO != null)
        {
            
            //MoveSecond.Instance.ResetOriGOPos();
            MoveSecond.Instance.ResetParent();
            MoveSecond.Instance.ResetDJDPPA();

            DontDestroyOnLoad(oriGO);
        }
        string id = ActionLogic.GetIns().GetCurActId();
        if (!string.IsNullOrEmpty(id))
        {
            ActionEditScene.OpenActions(id);
        }
    }

    void PopDeletaWin(GameObject go)
    {
        Robot robot = RobotManager.GetInst().GetCurrentRobot();
        if (null != robot)
        {
            ActionSequence act = robot.GetActionsForID(ActionLogic.GetIns().GetCurActId());
            if (null != act)
            {
                PublicPrompt.ShowDelateWin(LauguageTool.GetIns().GetText("删除动作提示") + "\"" + act.Name + "\"", DelateActionConfrim);
            }
        }
        
    }

    void DelateActionConfrim(GameObject go)
    {
        // Destroy(go.transform.parent.parent.gameObject);
        try
        {
            string btnname = go.name;
            if (btnname.Equals(PromptMsg.RightBtnName))
            {
                DoDeletaAction(null);
                OnActionClick(null);
            }
        }
        catch
        { }
    }

    /// <summary>
    /// 删除动作
    /// </summary>
    /// <param name="go"></param>
    void DoDeletaAction(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Delete selected action!!");
        ActionLogic.GetIns().DeletaAction();
        OnActionClick(null);
        UpdateActionsData();

        //    StartCoroutine(WaitOneFRAME());
    }

    IEnumerator WaitOneFRAME()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        UpdateActionsData();
    }

    IEnumerator WaitFrameEND(Transform t)
    {
        if (RobotManager.GetInst().GetCurrentRobot() == null)
            yield break;
        yield return new WaitForEndOfFrame();
        List<string> totalActions = RobotManager.GetInst().GetCurrentRobot().GetActionsIdList();
        GameObject tobj = Resources.Load("Prefabs/newActionItem1") as GameObject;
        for (int i = 0; i < totalActions.Count; i++)   //创建结点
        {
            GameObject obj = GameObject.Instantiate(tobj) as GameObject;
            obj.transform.SetParent(t);
            obj.transform.localScale = Vector3.one;
            obj.name = totalActions[i];
            UISprite sp = obj.transform.GetChild(0).GetComponent<UISprite>();
            if (null != sp)
            {
                ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(totalActions[i]);
                if (null == act)
                {
                    sp.spriteName = "add";
                }
                else
                {
                    sp.spriteName = act.IconName;
                }
                sp.MakePixelPerfect();
            }
        }
        for (int i = 0; i < totalActions.Count; i++)
        {
            string id = totalActions[i];
            ActionSequence act = RobotManager.GetInst().GetCurrentRobot().GetActionsForID(id);
            if (null == act)
            {
                t.GetChild(i).GetChild(0).GetComponent<UISprite>().spriteName = "add";
                t.GetChild(i).GetComponentInChildren<UILabel>().text = string.Empty;
            }
            else
            {
                t.GetChild(i).GetChild(0).GetComponent<UISprite>().spriteName = act.IconName;
                t.GetChild(i).GetComponentInChildren<UILabel>().text = act.Name;
            }
            t.GetChild(i).GetChild(0).GetComponent<UISprite>().MakePixelPerfect();
            /*if (name == PublicFunction.Default_Actions_Name)
            {
                name = LauguageTool.GetIns().GetText("FuWei");
            }*/
            
            UIEventListener.Get(t.GetChild(i).gameObject).onClick = OnActionClick;
        }
        t.GetComponent<UIGrid>().repositionNow = true;
    }
    /// <summary>
    /// 动作文件更新
    /// </summary>
    void UpdateActionsData()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Update action list!!");
        Transform t = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList/scrollRect").transform;
        for (int i = 0; i < t.childCount; i++)
        {
            Destroy(t.GetChild(i).gameObject);
        }
        StartCoroutine(WaitFrameEND(t));
    }

    void OnActionClick(GameObject go)
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Click any action icon!!");
        if (go == null)
        {
            hasSelectAction = false;
            ActionLogic.GetIns().DoSelectItem(null);
            Play_button.gameObject.SetActive(false);
            Edit_button.gameObject.SetActive(false);
            Delete_button.gameObject.SetActive(false);
            return;
        }
        //hasSelectAction = true;
        ActionLogic.GetIns().DoSelectItem(go);
        if (RobotManager.GetInst().GetCurrentRobot().IsOfficialForId(go.name)) //官方动作不可删除 可编辑
        {
            Delete_button.gameObject.SetActive(false);
        }
        else
        {
            Delete_button.gameObject.SetActive(true);
        }
        RobotManager.GetInst().GetCurrentRobot().StopRunTurn();
        StartCoroutine(WaitAMoment());

        if (go.name == ActionLogic.GetIns().GetNowPlayingActionId() && PlatformMgr.Instance.GetBluetoothState())
        {
            Play_button.GetChild(0).GetComponent<UISprite>().spriteName = "icon_stop";
        }
        else
        {
            Play_button.GetChild(0).GetComponent<UISprite>().spriteName = "icon_play";
        }

        /*if (!hasSelectAction)
        {
            hasSelectAction = true;
        }*/     
    }
    //GameObject bar;
    IEnumerator WaitAMoment()
    {
        //GameObject goo = null;
        if (Play_button == null && Edit_button == null && Delete_button == null)
            yield break;
        //Play_button.gameObject.SetActive(false);
        //Edit_button.gameObject.SetActive(false);
        //Delete_button.gameObject.SetActive(false);
        //yield return new WaitForSeconds(0.05f);
        Play_button.gameObject.SetActive(true);
        //Edit_button.gameObject.SetActive(true);
        //Delete_button.gameObject.SetActive(true);
        if (Delete_button != null)
        {
            UIEventListener.Get(Delete_button.gameObject).onClick = PopDeletaWin;
        }       
        UIEventListener.Get(Play_button.gameObject).onClick = DoPlayAction;

        DoPlayAction(Play_button.gameObject);
        //UIEventListener.Get(bar.transform.GetChild(2).gameObject).onClick = DoStopAction;

        if (IsCameraState)
        {
            Edit_button.gameObject.SetActive(false);
            //Debug.Log("hasSelectAction is " + hasSelectAction);

            Delete_button.gameObject.SetActive(true);
            GameObject ccf = null;
            Delete_button.gameObject.GetComponent<UISprite>().SetAnchor(ccf);
            Vector2 pos24 = Delete_button.localPosition;
            pos24.x = pos24.x + (132.0f * PublicFunction.GetWidth() / 1334.0f);
            Delete_button.localPosition = pos24;
            Delete_button.gameObject.SetActive(false);

            if (!hasSelectAction)
            {
                hasSelectAction = true;
                GameObject ero = null;
                Play_button.gameObject.GetComponent<UISprite>().SetAnchor(ero);
                Vector2 pos22 = Play_button.localPosition;
#if UNITY_ANDROID
                pos22.x = pos22.x + (261.0f * PublicFunction.GetWidth() / 1334.0f);
#endif
#if UNITY_IPHONE
                pos22.x = pos22.x + (132.0f * PublicFunction.GetWidth() / 1334.0f);
#endif
                Play_button.localPosition = pos22;

                /*if (RecordContactInfo.Instance.openType == "default")
                {
                    Delete_button.gameObject.SetActive(false);
                }
                else
                {
                    Delete_button.gameObject.SetActive(true);
                    GameObject ccf = null;
                    Delete_button.gameObject.GetComponent<UISprite>().SetAnchor(ccf);
                    Vector2 pos24 = Delete_button.localPosition;
                    pos24.x = pos24.x + (132.0f * PublicFunction.GetWidth() / 1334.0f);
                    Delete_button.localPosition = pos24;
                    Delete_button.gameObject.SetActive(false);
                }*/
            }
            
        }
        else
        {
            hasSelectAction = true;
            Edit_button.gameObject.SetActive(true);
            UIEventListener.Get(Edit_button.gameObject).onClick = DoEditAction;
        }
    }

    public static ModelDetailsWindow_new Ins;

    void Awake()
    {
        Ins = this;
        GuidePagesConfig = ResourcesEx.persistentDataPath + "/instruction1";
    }
    void Start()
    {
        flagex = false;
        flagex1 = true;
        IsCameraState = false;
        IsActionScene = false;
        hasSelectAction = false;
        EventMgr.Inst.Regist(EventID.UI_MainRightbar_hide, RightBarCallback);
        EventMgr.Inst.Regist(EventID.Set_Choice_Robot, SetRobot);
        EventMgr.Inst.Regist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.Regist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.Regist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.Regist(EventID.Change_Robot_Name_Back, GetNameChanged);
        EventMgr.Inst.Regist(EventID.Exit_Blue_Connect, ExitCameraBTConnect);
        PlatformMgr.Instance.RegesiterCallUnityDelegate(CallUnityFuncID.GiveupVideoState, GiveupVideoState);
        PlatformMgr.Instance.RegesiterCallUnityDelegate(CallUnityFuncID.CurrentStartPlaying, CurrentStartPlaying);
        //PlatformMgr.Instance.RegesiterCallUnityDelegate(CallUnityFuncID.CurrentStopPlaying, CurrentStopPlaying);
        PlatformMgr.Instance.RegesiterCallUnityDelegate(CallUnityFuncID.CurrentSaveVideo, CurrentSaveVideo);
        PlatformMgr.Instance.RegesiterCallUnityDelegate(CallUnityFuncID.ReplayVideoState, ReplayVideoState);
        isBackCommunity = false; // unity or community flag
        if (RecordContactInfo.Instance.openType != "playerdata") //official模型
            IsOfficial = true;
        else  //modify模型 + create
            IsOfficial = false;

        if (!System.IO.File.Exists(GuidePagesConfig)) //  读取文件 文件不存在需要开启指引pages
        {
            IsFirstCamera = true;
            IsFirstVideo = true;
        }
        else
        {
            IsFirstCamera = false;
            IsFirstVideo = false;
        }

        Transform offGrid = GameObject.Find("MainUIRoot_new/ModelDetails/BottomLeft/grid_Official").transform;
        Transform defaultGrid = GameObject.Find("MainUIRoot_new/ModelDetails/BottomLeft/grid_Default").transform;
        TakePhotoObj = GameObjectManage.Ins.TakePhotoObj;
        Camera_button = GameObject.Find("MainUIRoot_new/ModelDetails/Top/Camera").transform;
        Camera_button.gameObject.SetActive(false);

        Giveup_button = GameObject.Find("MainUIRoot_new/ModelDetails/TopLeft/giveup").transform;
        Giveup_button.gameObject.SetActive(false);

        StartPlay_Button = GameObject.Find("MainUIRoot_new/ModelDetails/Right/playingIcon").transform;

        Saved_Button = StartPlay_Button.GetChild(2);
        Saved_Button.gameObject.SetActive(false);

        StartPlay_Button.gameObject.SetActive(false);

        FirstCameraEnter = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter").transform;
        FirstVideoEnter = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter").transform;

        CameraIconTip = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter/CameraIcons").transform;
        CameraIconTip.gameObject.SetActive(false);

        CameraGuideLine = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter/GuideLine").transform;
        CameraGuideLine.gameObject.SetActive(false);

        CameraTextTips = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter/CameraTips").transform;
        CameraTextTips.gameObject.SetActive(false);

        GuideCamera_button = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstCameraEnter/ConfirmTips").transform;
        GuideCamera_button.gameObject.SetActive(false);

        VideoIconTip = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/VideoIcons").transform;
        VideoIconTip.gameObject.SetActive(false);

        VideoGuideLine2 = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/GuideLine2").transform;
        VideoGuideLine2.gameObject.SetActive(false);

        VideoGuideLine3 = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/GuideLine3").transform;
        VideoGuideLine3.gameObject.SetActive(false);

        VideoGuideLine4 = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/GuideLine4").transform;
        VideoGuideLine4.gameObject.SetActive(false);

        VideoTextTip1 = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/VideoTips1").transform;
        VideoTextTip1.gameObject.SetActive(false);

        VideoTextTip2 = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/VideoTips2").transform;
        VideoTextTip2.gameObject.SetActive(false);

        BluetoothIconTip = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/BlueToothIcons").transform;
        BluetoothIconTip.gameObject.SetActive(false);

        ActionIconTip = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/ActionIcons").transform;
        ActionIconTip.gameObject.SetActive(false);

        GuideVideo_button = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstVideoEnter/ConfirmTips").transform;
        GuideVideo_button.gameObject.SetActive(false);

        FirstCameraEnter.gameObject.SetActive(false);
        FirstVideoEnter.gameObject.SetActive(false);

        CameraBgPanel = GameObject.Find("MainUIRoot_new/ModelDetails/Center/CameraBg").transform;
        CameraAreas = CameraBgPanel.GetChild(1);
        CameraAreas.gameObject.SetActive(false);
        CameraBgPanel.gameObject.SetActive(false);

        IsCameraExitSS = false;

        if (IsOfficial)   //官方
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Select official model!!");
            officialTrans.gameObject.SetActive(true);
            defaultTrans.gameObject.SetActive(false);
            UIEventListener.Get(officialTrans.GetChild(0).gameObject).onClick = BuildClick;
            officialTrans.GetChild(0).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("搭建");
            Effect_btn = officialTrans.GetChild(1);
            UIEventListener.Get(Effect_btn.gameObject).onClick = EffectClick;
            Effect_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("动作");
            Logic_btn = officialTrans.GetChild(2);
            Controller_btn = officialTrans.GetChild(3);
            UIEventListener.Get(Controller_btn.gameObject).onClick = ControlClick;
            officialTrans.GetChild(3).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("控制器");
            defaultGrid.gameObject.SetActive(false);
            offGrid.gameObject.SetActive(true);
            BT_button = offGrid.GetChild(1).GetChild(0);    //BT
            Power_button = offGrid.GetChild(0);  //power
            Reset_button = offGrid.GetChild(2);
            //Resets_button = offGrid.GetChild(2);

            //Connect_button.gameObject.SetActive(true);

            if (robotName != null)
            {
                robotName.text = RobotManager.GetInst().GetCurrentRobot().ShowName;
            }
            TakePhotoObj.SetActive(false);
  
        }
        else   //自定义
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "Select custom model!!");
            GameObject ttem = GameObject.Find("MainUIRoot_new/GameObject");  //模型加载完后才能继续操作
            if (ttem != null && ttem.GetComponent<BoxCollider>() != null)
            {
                ttem.GetComponent<BoxCollider>().enabled = false;
            }
            //if (RobotManager.GetInst().IsCreateRobotFlag)   // 创建的模型
            //{
            //    RobotManager.GetInst().GetCurrentRobot();
            //    //CurModelName = RobotManager.GetInst().GetCreateRobot().Name;
            //}
            // TakePhotoObj = GameObject.Find("DontDestroyNode/UIRoot/MainPic/Center/takePhoto");
            //  TakePhotoObj.SetActive(true);
            GameObjectManage.Ins.OnShow();
            officialTrans.gameObject.SetActive(false);
            defaultTrans.gameObject.SetActive(true);
            Effect_btn = defaultTrans.GetChild(0);
            UIEventListener.Get(Effect_btn.gameObject).onClick = EffectClick;
            Effect_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("动作");
            Logic_btn = defaultTrans.GetChild(1);
            Controller_btn = defaultTrans.GetChild(2);
            UIEventListener.Get(Controller_btn.gameObject).onClick = ControlClick;
            Controller_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("控制器");
            UIEventListener.Get(defaultTrans.GetChild(3).gameObject).onClick = CommunityClick;
            defaultTrans.GetChild(3).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("发布");
            defaultGrid.gameObject.SetActive(true);
            offGrid.gameObject.SetActive(false);
            BT_button = defaultGrid.GetChild(1).GetChild(0);    //BT
            Power_button = defaultGrid.GetChild(0);  //power

            if (robotName != null)
                robotName.gameObject.SetActive(false);

            if (TakePhotoObj != null)
            {
                UIEventListener.Get(TakePhotoObj.transform.GetChild(0).gameObject).onClick = CallCamera;  //调用相机
                PhotoTexture = TakePhotoObj.transform.GetChild(0).GetComponent<UITexture>();

                if (PhotoTexture != null)
                {
                    if (PhotoTexture.mainTexture == null)
                    {
                        InitModelPic();
                    }
                }
            }
          //  StartCoroutine(DoAtFrameEnd());
            // ShowPlaylist = false;
        }
        if (BT_button != null)
            UIEventListener.Get(BT_button.gameObject).onClick = BlueTooClick;
        if (Reset_button != null)
            UIEventListener.Get(Reset_button.gameObject).onClick = ResetClick;
        if (Logic_btn != null)
        {
            UIEventListener.Get(Logic_btn.gameObject).onClick = LogicProgramIN;
            Logic_btn.GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("逻辑编程");
        }
        AddButton = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList/Button");
        if (AddButton != null)
        {
            UIEventListener.Get(AddButton).onClick = AddNewAction;
        }

        UpdateActionsData();
        //bar = GameObject.Find("MainUIRoot_new/ModelDetails/Top/toolBars");
        //Vector2 posBar = bar.transform.localPosition;
        //posBar.x = posBar.x * PublicFunction.GetWidth() / 1334.0f;
        //posBar.y = posBar.y * PublicFunction.GetHeight() / 750.0f;
        //bar.transform.localPosition = posBar;
        //bar.SetActive(false);
        Play_button = GameObject.Find("MainUIRoot_new/ModelDetails/Top/play").transform;
        Play_button.gameObject.SetActive(false);
        Edit_button = GameObject.Find("MainUIRoot_new/ModelDetails/Top/edit").transform;
        Edit_button.gameObject.SetActive(false);
        Delete_button = GameObject.Find("MainUIRoot_new/ModelDetails/Top/delta").transform;
        Delete_button.gameObject.SetActive(false);

        OnBlueConnectResult(new EventArg(PlatformMgr.Instance.GetBluetoothState())); //蓝牙
        if (PlatformMgr.Instance.GetBluetoothState()) //蓝牙连接时 发送电量信息
        {
            GetPowerState(null);
        }

        Transform trans = GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList").transform;
        if (trans.GetChild(trans.childCount - 1).GetComponentInChildren<UILabel>() != null)
            trans.GetChild(trans.childCount - 1).GetComponentInChildren<UILabel>().text = LauguageTool.GetIns().GetText("新建动作");


        if (UICam != null) //切换场景时 画面闪烁的问题
        {
            StartCoroutine(ShowLeftFrame());
        }

        if (!System.IO.File.Exists(GuidePagesConfig) && IsOfficial) //  读取文件 文件不存在需要开启指引pages
        {
            // if (IsOfficial)
            // {
            MoveSecond.Instance.OnModelLoadOver = OnModelLoadOver; //模型加载完毕 后显示引导页
            // }
            //else
            //{
            //    OnModelLoadOver();
            //}
            StreamWriter sw = System.IO.File.CreateText(GuidePagesConfig);
            sw.Write("{\"instruction1\":\"0\"}");
            sw.Close();
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "xian shi yin dao!!");
        }
        else
        {
            FirstGuidePage.GetIns().OnClosePage();
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "关闭引导页!!");
        }

        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "enter scene!!");
    }
    IEnumerator ShowLeftFrame()
    {
        yield return new WaitForSeconds(0.05f);
        if (UICam != null)
            UICam.enabled = true;
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "ShowLeftFrame!!");
    }

    IEnumerator DoAtFrameEnd()
    {
        yield return new WaitForSeconds(0.05f);
        ShowPlaylist = false;
    }

    public void PreEnterOtherScenes(EventDelegate.Callback call = null)
    {
        if (ShowPlaylist)// 动作表页面
        {
            ShowBottomBar(false, 0, call);
        }
        else  //主页
        {
            ShowRightBar(false, 0, call);
        }
        if (IsOfficial)
        {


        }
        else
        {
            GameObjectManage.Ins.OnExit();
        }
    }

    void OnDestroy()
    {
        DoStopAction(null);
        Ins = null;
        //GameObject oriGO = GameObject.Find("oriGO");
        //if (oriGO != null && !isBackCommunity)
        //{
        //    DontDestroyOnLoad(oriGO);

        //    /*RobotMotion rm=oriGO.GetComponent<RobotMotion>();
        //    Destroy(rm);*/
        //}
        if (isBackCommunity) //社区
        {
            GameObjectManage.Ins.ClearData();
        }
        EventMgr.Inst.UnRegist(EventID.Read_Power_Msg_Ack, GetPowerState);
        EventMgr.Inst.UnRegist(EventID.BLUETOOTH_MATCH_RESULT, OnBlueConnectResult);
        EventMgr.Inst.UnRegist(EventID.Photograph_Back, PhotographBack);
        EventMgr.Inst.UnRegist(EventID.Set_Choice_Robot, SetRobot);
        EventMgr.Inst.UnRegist(EventID.Change_Robot_Name_Back, GetNameChanged);
        EventMgr.Inst.UnRegist(EventID.UI_MainRightbar_hide, RightBarCallback);
        EventMgr.Inst.UnRegist(EventID.Exit_Blue_Connect, ExitCameraBTConnect);
        //  GameObjectManage.Ins.OnExit();
    }

    #region other
    /// <summary>
    /// 蓝牙连接回调
    /// </summary>
    /// <param name="arg"></param>
    void OnBlueConnectResult(EventArg arg)
    {
        try
        {
            if (RobotManager.GetInst().GetCurrentRobot() == null)
            {
                if (null != BT_button)
                {
                    UISprite sprite = BT_button.GetChild(1).GetComponent<UISprite>();
                    if (null != sprite)
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now any robot is disconnect!");

                        sprite.spriteName = "disconnect";
                        sprite.SetDimensions(50, 50);
                        // sprite.MakePixelPerfect();
                    }
                    if (Power_button != null && Power_button.gameObject.activeSelf)
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now power icon is hidded for current robot is null!");
                        Power_button.gameObject.SetActive(false);
                    }
                }
                return;
            }

            bool flag = (bool)arg[0];
            string iconName;
            //StartCoroutine(ShowPowerIcons(0.5f));
            if (flag)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now any bluetooth is connect!");
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now isactionscene is " + IsActionScene);

                iconName = "connect";
                if (Power_button != null && !IsActionScene)
                {
                    StartCoroutine(ShowPowerIcons(0.3f));
                    //Power_button.gameObject.SetActive(true);
                    firstAdapter = false; //断开蓝牙，表示充电结束
                    GetPowerState(null);
                    /*Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "Shape";
                    Power_button.GetChild(1).GetComponent<UISprite>().enabled = true;
                    Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;*/
                    //   EventMgr.Inst.Fire(EventID.Read_Power_Msg_Ack);
                }
            }
            else
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now any bluetooth is disconnect!");

                iconName = "disconnect";
                if (Power_button != null && Power_button.gameObject.activeSelf)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now power icon is hidded for current bluetooth is disconnect!");
                    Power_button.gameObject.SetActive(false);
                }
                if (Play_button != null && Play_button.GetChild(0).GetComponent<UISprite>().spriteName == "icon_stop")
                {
                    Play_button.GetChild(0).GetComponent<UISprite>().spriteName = "icon_play";
                }
            }
            if (null != BT_button)
            {
                UISprite sprite = BT_button.GetChild(1).GetComponent<UISprite>();
                if (null != sprite)
                {
                    sprite.spriteName = iconName;
                    sprite.SetDimensions(50, 50);
                    //sprite.MakePixelPerfect();
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
    IEnumerator ShowPowerIcons(float t)
    {
        yield return new WaitForSeconds(t);

        if (!IsActionScene && PlatformMgr.Instance.GetBluetoothState())
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "now bluetooth is smkkkk!!");
            Power_button.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 初始化匹配模型跟照片
    /// </summary>
    void InitModelPic()
    {
        try
        {
            StartCoroutine(CaculatePicSize());
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "InitModelPic 模型主页图片异常 error=" + ex.ToString());
        }
    }

    IEnumerator CaculatePicSize()
    {
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "CaculatePicSize 模型主页图片开始加载");
        yield return null;
        if (PhotoTexture == null)
            yield break;
        PhotoTexture.enabled = false;
        if (!string.IsNullOrEmpty(PlatformMgr.Instance.Pic_Path))
        {
            StartCoroutine(LoadTexture(PlatformMgr.Instance.Pic_Path));
        }
        else
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "CaculatePicSize 图片路径为空，加载默认图片");
            Texture tempTexture;
            Texture errorPic = Resources.Load<Texture>("pic_error");
            if (errorPic != null)
            {
                tempTexture = errorPic;

                try
                {
                    PhotoTexture.width = tempTexture.width;
                    PhotoTexture.height = tempTexture.height;
                    PhotoTexture.mainTexture = tempTexture;
                }
                catch (System.Exception ex)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, ex.ToString());
                }
            }
        }
        if (TakePhotoObj != null)
        {
            PhotoTexture.enabled = false;
            GameObjectManage.Ins.OnShow();
            //TakePhotoObj.SetActive(true);
        }
        StartCoroutine(WaitSometime());
        PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "CaculatePicSize 模型主页图片加载完成");
    }
    IEnumerator WaitSometime()
    {
        yield return null;
        PhotoTexture.enabled = true;
    }

    IEnumerator LoadTexture(string name)
    {
#if UNITY_EDITOR
        string path = "file:///" + name;
#else
        string path = "file://" + name;
#endif
        if (!File.Exists(name))
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "File doesn't exist!!");
            //加载错误图片
            Texture errorPic = Resources.Load<Texture>("pic_error");

            if (errorPic != null)
            {
                try
                {
                    PhotoTexture.width = errorPic.width;
                    PhotoTexture.height = errorPic.height;
                    PhotoTexture.mainTexture = errorPic;
                }
                catch (System.Exception ex)
                {
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
                }
            }
            yield break;
        }
        Resources.UnloadUnusedAssets();
        WWW www = new WWW(path);
        //while (!www.isDone)
        //{
        //}
        yield return www;
        yield return null;  //拍完照 显示变形
        yield return new WaitForEndOfFrame();
        Texture tempTexture;
        if (www.isDone)
        {
            if (null != www.error || www.texture == null)   // error
            {
                if (www.error != null)
                    //Debuger.Log("load texture error = " + www.error);
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "load texture error = " + www.error);
                if (www.texture == null)
                    //Debuger.Log("texture is null");
                    PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "texture is null");
                Texture errorPic = Resources.Load<Texture>("pic_error");

                if (errorPic != null)
                {
                   // Vector2 tVec = new Vector2();
                    tempTexture = errorPic;
                  //  tVec = GetRightsize(tempTexture.width, tempTexture.height);
                    try
                    {
                        /*Transform tnull = null;
                        PhotoTexture.GetComponent<UIWidget>().SetAnchor(tnull);*/
                        PhotoTexture.width = tempTexture.width;
                        PhotoTexture.height = tempTexture.height;
                        PhotoTexture.mainTexture = tempTexture;
                    }
                    catch (System.Exception ex)
                    {
                        PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
                    }
                }
                yield break;
            }    //success
            tempTexture = www.texture;
            Vector2 ttVec = new Vector2();
            ttVec = GetRightsize(tempTexture.width, tempTexture.height);
            try
            {
                /*Transform tnull = null;
                PhotoTexture.GetComponent<UIWidget>().SetAnchor(tnull);*/
                PhotoTexture.width = (int)ttVec.x;
                PhotoTexture.height = (int)ttVec.y;
                PhotoTexture.mainTexture = tempTexture;
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
            }
        }
        else
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "not done " + path);
        }
    }

    Vector2 GetRightsize(int w, int h)
    {
        Vector2 t;
        float screenWidth = PublicFunction.GetWidth();
        float screenHeight = PublicFunction.GetHeight();

        int height = (int)(screenHeight * w / screenWidth);
        if (height >= h)
        {
            t.y = screenHeight;
            t.x = Mathf.CeilToInt(screenHeight * w / h);
        }
        else
        {
            t.y = Mathf.CeilToInt(screenWidth * h / w);
            t.x = screenWidth;
        }
        return t;
    }

    /// <summary>
    /// 相机弹出框激活
    /// </summary>
    /// <param name="go"></param>
    void CallCamera(GameObject go)
    {
        if (null != RobotManager.GetInst().GetCurrentRobot())
        {
            PlatformMgr.Instance.Photograph(RobotMgr.NameNoType(RobotManager.GetInst().GetCurrentRobot().Name), PlatformMgr.Instance.Pic_Path);
        }
    }

    void PhotographBack(EventArg arg)
    {
        try
        {
            //StartCoroutine(delayExcute(InitModelPic));
            //InitModelPic();
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "PhotographBack error = " + ex.ToString());
        }
    }

    IEnumerator delayExcute(EventDelegate.Callback voidDel)
    {
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        voidDel();
    }

    private bool flagex1; //从其它场景直接到默认场景
    private bool flagex;  //从其它场景直接到actionlist
    void RightBarCallback(EventArg arg)
    {
        try
        {
            if ((bool)arg[0]) //show left or botton
            {
                if (RobotMgr.Instance.openActionList == true)
                {
                    RobotMgr.Instance.openActionList = false;
                    flagex = true;

                    IsActionScene = true;

                    ShowPlaylist = true;                    

                    if (Back_button != null)
                        UIEventListener.Get(Back_button.gameObject).onClick = BackEffect;
                }
                else
                {
                    IsActionScene = false;
                    ShowPlaylist = false;
                    flagex = false;
                    flagex1 = true;

                    // ShowRightBar((bool)arg[0]);
                    if (Back_button != null)
                        UIEventListener.Get(Back_button.gameObject).onClick = GoBack;
                }
            }
        }
        catch (System.Exception ex)
        { 
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }

    /// <summary>
    /// 电量信息回调
    /// </summary>
    /// <param name="arg"></param>
    //static private bool powerFlag = true;  //静态 
    private bool firstAdapter = false;
    void GetPowerState(EventArg arg)
    {
        try
        {
            if (PlatformMgr.Instance.IsChargeProtected) //充电保护
            {
                //充电保护的情况下在动作表界面插上电源
                if (ShowPlaylist)// && IsProtected())
                {
#if UNITY_ANDROID
                    GuideVideo_button.gameObject.SetActive(false);
                    Saved_Button.gameObject.SetActive(false);
                    StartPlay_Button.gameObject.SetActive(false);
#endif
                    Back_button.gameObject.SetActive(true);
                    Camera_button.gameObject.SetActive(false);
                    GameObject modelObj2 = GameObject.Find("oriGO");
                    PublicFunction.SetLayerRecursively(modelObj2, LayerMask.NameToLayer("Robot"));

                    //StartCoroutine(ShowResetIcons(0.5f));
                    CameraBgPanel.gameObject.SetActive(false);
                    PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.ExitPlayVideoMode, "");
                    PublicPrompt.ShowChargePrompt(GoBack);
                }
                if (IsCameraState)
                {
                    IsCameraExitSS = true;
                    IsCameraState = false;

                    if (AddButton != null)
                    {
                        AddButton.gameObject.SetActive(true);
                    }

                    Vector4 size3 = ScrollActionItems.GetComponent<UIPanel>().baseClipRegion;
                    size3.z = size3.z - (200.0f * PublicFunction.GetWidth() / 1334.0f);
                    size3.x = size3.x - (100.0f * PublicFunction.GetWidth() / 1334.0f);
                    ScrollActionItems.GetComponent<UIPanel>().baseClipRegion = size3;

                    //GameObject ddo = null;
                    //ScrollActionItems.GetComponent<UIPanel>().SetAnchor(ddo);

                    Vector3 pos3 = ScrollActionItems.localPosition;

                    if (pos3.x < (-500.0f * PublicFunction.GetWidth() / 1334.0f))
                        pos3.x = pos3.x + (179.0f * PublicFunction.GetWidth() / 1334.0f);
                    ScrollActionItems.localPosition = pos3;

                    ScrollActionItems.GetComponent<UIPanel>().SetAnchor(GameObject.Find("MainUIRoot_new/ModelDetails/Bottom/ActionList"));
                }
            }

            if (PlatformMgr.Instance.PowerData.isAdapter) //插上适配器
            {
                if (!firstAdapter)  //第一次插入适配器时
                {
                    firstAdapter = true;
                    if (Power_button != null) //充电状态
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "power1";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = true;
                    }
                }
                if (PlatformMgr.Instance.PowerData.isChargingFinished) //充满
                {
                    if (Power_button != null)
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "fullCharge";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                        //Power_button.GetChild(1).GetComponent<UISprite>().fillAmount = PlatformMgr.Instance.PowerData.percentage * 0.01f;
                    }
                }
            }
            else  //拔下适配器  电量实时反馈
            {
                if (firstAdapter) //拔下电量的那一刻
                {
                    if (Power_button != null)
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "power1";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = true;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                    }
                }
                firstAdapter = false;

                if (Power_button != null)
                {
                    if (PlatformMgr.Instance.PowerData.percentage > 20) //正常电量
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "power1";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = true;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(1).GetComponent<UISprite>().fillAmount = PlatformMgr.Instance.PowerData.percentage * 0.01f;
                    }
                    else  //低电量
                    {
                        Power_button.GetChild(0).GetComponent<UISprite>().spriteName = "low battery";
                        Power_button.GetChild(1).GetComponent<UISprite>().enabled = false;
                        Power_button.GetChild(2).GetComponent<UISprite>().enabled = false;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }
    }

    bool IsProtected()
    {
        return PlatformMgr.Instance.PowerData.isAdapter && PlatformMgr.Instance.IsChargeProtected;
    }

    void GoBack()
    {
        if (IsCameraState)
        {
            GiveupCurrentVideo(null);
            IsCameraExitSS = true;
        }
        BackEffect(null);
        //Game.Scene.SceneMgr.EnterScene(Game.Scene.SceneType.MainWindow);
    }

    void SetRobot(EventArg arg)
    {
        try
        {
        }
        catch (System.Exception ex)
        {

        }
    }

    void GetNameChanged(EventArg arg)
    {
        try
        {
            string name = (string)arg[0];
            robotName.text = name;
        }
        catch (System.Exception ex)
        {
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
        }

    }

    public string GuidePagesConfig;
    void OnModelLoadOver()
    {
        Transform centerTrans = GameObject.Find("MainUIRoot_new/ModelDetails/Center").transform;
        if (centerTrans != null)
        {
            FirstGuidePage.GetIns().Show(0.3f);
             //FirstGuidePage.LoadGuidePage("Prefabs/FirstGuidePage", centerTrans);   
            //UIRoot root = GameObject.FindObjectOfType<UIRoot>();
            //Vector2 temp = Vector2.zero;
            //if (root != null)
            //{
            //    float s = (float)root.activeHeight / Screen.height;
            //    int height = Mathf.CeilToInt(Screen.height * s);
            //    GameObject fgp = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstGuidePage");
            //    GameObject fgpPad = GameObject.Find("MainUIRoot_new/ModelDetails/Center/FirstGuidePage_pad");

            //    if (height >= 1000) //ipad
            //    {
            //        if (fgpPad != null)
            //        {
            //            FirstGuidePage.LoadGuidePage(fgpPad, centerTrans);
            //            //fgpPa.GetComponent<FirstGuidePage>().enabled =true;
            //        }
            //    }
            //    else
            //    {
            //        if (fgp != null)
            //        {
            //            FirstGuidePage.LoadGuidePage(fgp, centerTrans);
            //            // fgpPad.GetComponent<FirstGuidePage>().enabled = true;
            //        }
            //    }
            //}
            PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "显示引导页!!");
        }
    }

    bool isPause;
    bool isFocus;
    void OnEnable()
    {
        isPause = false;
        isFocus = false;
    }

    /// <summary>
    /// 锁屏重启时，先调用Onfocus,后pause
    /// </summary>
    void OnApplicationPause()
    {

#if UNITY_IPHONE || UNITY_ANDROID
        if (!isPause)
        {
            // 强制暂停时，事件
        }
        else
        {
            isFocus = true;
        }
        isPause = true;
#endif
    }

    void OnApplicationFocus()
    {

#if UNITY_IPHONE || UNITY_ANDROID

        if (isFocus)
        {
            // “启动”手机时，事件
            isPause = false;
            isFocus = false;

            if (Application.isMobilePlatform)
            {
                if (!ShowPlaylist)
                { 
                    
                }
            }
        }
        if (isPause)
        {
            isFocus = true;
        }
#endif
    }
    #endregion
}
