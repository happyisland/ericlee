// ------------------------------------------------------------------
// Description : 场景管理器
// Author      : oyy
// Date        : 2015-04-24
// Histories   : 
// ------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;

namespace Game.Scene
{
    public enum SceneType : int
    {
        StartScene = 0,
        MenuScene=1,
        MainWindow = 2,
        Assemble = 3,
        EditAction = 4,
        ActionPlay = 5,
      //  CreateModel=6,
        EmptyScene = 6,
        testScene = 7,
    }

    static class SceneMgr
    {
        private static long m_timerId = -1; 

        private static SceneBase sm_currScene;
        public static SceneBase CurrScene { get { return sm_currScene; } }


        private static SceneType sm_enterSceneN;
        public static SceneType EnterSceneN { get { return sm_enterSceneN; } set { sm_enterSceneN = value; } }
        private static Dictionary<SceneType, Func<SceneBase>> sm_sceneCreators;

        public static SceneType mCurrentSceneType = SceneType.StartScene;

        static SceneMgr()
        {
            sm_sceneCreators = new Dictionary<SceneType, Func<SceneBase>>();
        }

        //场景激活时，调用
        public static void ActiveScene(SceneBase scene)
        {
            sm_currScene = scene;
        }

        public static SceneType GetCurrentSceneType()
        {
            return mCurrentSceneType;
        }
        //跳转场景
        public static void EnterScene(SceneType type)
        {
            try
            {
                if (type == SceneType.EmptyScene)
                {
                    RobotMgr.Instance.GoToCommunity();
                }
                if (mCurrentSceneType == SceneType.MainWindow)
                {
                    //先通知主场景播放退出动画
                    mCurrentSceneType = type;
                    if (null != ModelDetailsWindow_new.Ins)
                    {
                        try
                        {
                            ModelDetailsWindow_new.Ins.PreEnterOtherScenes(EnterOtherScenes);
                        }
                        catch (System.Exception ex)
                        {
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                        }
                    }
                }
                else
                {
                    if (sm_currScene != null)
                    {
                        sm_currScene.Dispose();
                    }
                    mCurrentSceneType = type;
                    AsyncOperation asyn = Application.LoadLevelAsync((int)type);
                    if (-1 == m_timerId)
                    {
                        Timer.Cancel(m_timerId);
                    }
                    m_timerId = Timer.Add(0.5f, 0.5f, 1, EnterSceneCallBack, asyn, type);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        static void EnterOtherScenes()
        {
            if (sm_currScene != null)
            {
                sm_currScene.Dispose();
            }
            AsyncOperation asyn = Application.LoadLevelAsync((int)mCurrentSceneType);
            if (-1 == m_timerId)
            {
                Timer.Cancel(m_timerId);
            }
            m_timerId = Timer.Add(0.5f, 0.5f, 1, EnterSceneCallBack, asyn,mCurrentSceneType);
        }

        public static void DisposeCurrent()
        {
            if (sm_currScene != null)
            {
                sm_currScene.Dispose();
            }
        }

        static void EnterSceneCallBack(params object[] obj)
        {
            AsyncOperation asyn = (AsyncOperation)obj[0];
            SceneType type = (SceneType)obj[1];
            if (asyn.isDone)
            {
                m_timerId = -1;
                /*if (sm_sceneCreators.ContainsKey(type))
                {
                    sm_sceneCreators[type]();
                }*/
                if (mCurrentSceneType == SceneType.EditAction)
                {
                    SingletonObject<SceneManager>.GetInst().GotoScene(typeof(ActionEditScene));
                    /*Robot robot = RobotManager.GetInst().GetCurrentRobot();
                    if (null != robot)
                    {
                        SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(robot);
                    }*/
                    //SingletonObject<LogicCtrl>.GetInst().CallUnityCmd("jimu://getPosture");
                }
                else if (mCurrentSceneType == SceneType.ActionPlay)
                {
                    SingletonObject<SceneManager>.GetInst().GotoScene(typeof(UserdefControllerScene));
                }
                else
                {
                    BaseScene scene = SingletonObject<SceneManager>.GetInst().GetCurrentScene();
                    if (scene == null)
                        return;
                    if (scene.GetType() == typeof(ActionEditScene) || scene.GetType() == typeof(UserdefControllerScene))
                    {
                        SingletonObject<SceneManager>.GetInst().CloseCurrentScene();
                    }
                }
            }
        }

    }
}
