using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:MyProjectBuild.cs
/// Description:
/// Time:2016/1/11 10:41:02
/// </summary>
public class MyProjectBuild
{
    #region 公有属性
    #endregion

    #region 其他属性
    static string[] sDefaultAry = new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/default/qiluogan" };
    #endregion

    #region 公有函数


    #endregion

    #region 其他函数


    //在这里找出你当前工程所有的场景文件，假设你只想把部分的scene文件打包 那么这里可以写你的条件判断 总之返回一个字符串数组。
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }
    
    [MenuItem("ProjectBuild/Android/导出社区正式版")]
    public static void BuildForGoogle()
    {
        //打包之前先设置一下 预定义标签， 我建议大家最好 做一些  91 同步推 快用 PP助手一类的标签。 这样在代码中可以灵活的开启 或者关闭 一些代码。
        //因为 这里我是承接 上一篇文章， 我就以sharesdk做例子 ，这样方便大家学习 ，
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, "USE_SHARE");
        if (SetCommunity(false, false, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/RobotAndroid";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(Path.Combine(path, PlayerSettings.productName));
            }
            //path = EditorUtility.SaveFilePanel("选择导出路径", path, "", "");
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
            EditorUtility.RevealInFinder(Path.Combine(path, PlayerSettings.productName));
        }
    }

    [MenuItem("ProjectBuild/Android/导出社区带个人模型版")]
    public static void BuildForGoogleAndData()
    {
        //打包之前先设置一下 预定义标签， 我建议大家最好 做一些  91 同步推 快用 PP助手一类的标签。 这样在代码中可以灵活的开启 或者关闭 一些代码。
        //因为 这里我是承接 上一篇文章， 我就以sharesdk做例子 ，这样方便大家学习 ，
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, "USE_SHARE");
        if (SetCommunity(false, true, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/RobotAndroid";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(Path.Combine(path, PlayerSettings.productName));
            }
            //path = EditorUtility.SaveFilePanel("选择导出路径", path, "", "");
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
            EditorUtility.RevealInFinder(Path.Combine(path, PlayerSettings.productName));
        }
    }

    [MenuItem("ProjectBuild/Android/导出社区测试版")]
    public static void BuildForGoogleTest()
    {
        if (SetCommunity(true,false, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/RobotAndroid";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(Path.Combine(path, PlayerSettings.productName));
            }
            //path = EditorUtility.SaveFilePanel("选择导出路径", path, "", "");
            BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
            EditorUtility.RevealInFinder(Path.Combine(path, PlayerSettings.productName));
        }
        
    }

    [MenuItem("ProjectBuild/Android/导出unity正式版")]
    static void BuildForAndroid()
    {
        if (SetUnity(false, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = "C:/Users/Public/Desktop";
            path = EditorUtility.SaveFilePanel("选择导出路径", path, "Jimu", "apk");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, null);
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, null);
#endif
        }

    }

    [MenuItem("ProjectBuild/Android/导出unity测试版")]
    static void BuildForAndroidTest()
    {
        if (SetUnity(true, BuildTarget.Android))
        {
            SetAndroidSetting();
            string path = "C:/Users/Public/Desktop";
            path = EditorUtility.SaveFilePanel("选择导出路径", path, "Jimu", "apk");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, null);
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, null);
#endif
        }

    }

    [MenuItem("ProjectBuild/IOS/导出社区正式版")]
    public static void BuildForIOS()
    {
        if (SetCommunity(false, false, BuildTarget.iPhone))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Roobt_ios_sq";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iPhone, BuildOptions.None);
                IosFinishedDealFiles(path);
                EditorUtility.RevealInFinder(path);
            }
            
        }
        
    }

    [MenuItem("ProjectBuild/IOS/导出社区带个人模型版")]
    public static void BuildForIOSAndModel()
    {
        if (SetCommunity(false, true, BuildTarget.iPhone))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Roobt_ios_sq";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {

                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iPhone, BuildOptions.None);
                IosFinishedDealFiles(path);
                EditorUtility.RevealInFinder(path);
            }
            
        }
        
    }

    [MenuItem("ProjectBuild/IOS/导出社区测试版")]
    public static void BuildForIOSTest()
    {
        if (SetCommunity(true, false, BuildTarget.iPhone))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Roobt_ios_sq";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iPhone, BuildOptions.None);
                IosFinishedDealFiles(path);
                EditorUtility.RevealInFinder(path);
            }
            
        }
        
    }

    [MenuItem("ProjectBuild/IOS/导出unity正式版")]
    static void BuildForUnityIOS()
    {
        if (SetUnity(false, BuildTarget.iPhone))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Robot_ios_unity";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iPhone, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, null);
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, null);
#endif
        }

    }

    [MenuItem("ProjectBuild/IOS/导出unity测试版")]
    static void BuildForUnityIOSTest()
    {
        if (SetUnity(true, BuildTarget.iPhone))
        {
            SetIphoneSetting();
            string path = Application.dataPath.Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/Assets"));
            path = path.Substring(0, path.LastIndexOf("/"));
            path += "/robot_ios/Robot_ios_unity";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                PublicFunction.DelDirector(path);
            }
            path = EditorUtility.SaveFolderPanel("选择导出路径", path, "");
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iPhone, BuildOptions.None);
                EditorUtility.RevealInFinder(path);
            }
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, null);
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, null);
#endif
        }

    }


    static void DelFolder(string path, string [] notDel, bool delSelf)
    {
        try
        {
            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                for (int i = dirs.Length - 1; i >= 0; --i)
                {
                    string tmpPath = dirs[i].Replace('\\', '/');
                    if (null != notDel)
                    {
                        bool delFlag = true;
                        for (int notIndex = 0, notMax = notDel.Length; notIndex < notMax; ++notIndex)
                        {
                            if (tmpPath.Equals(notDel[notIndex]))
                            {
                                delFlag = false;
                            }
                        }
                        if (delFlag)
                        {
                            DelFolder(tmpPath, notDel, true);
                        }
                    }
                    else
                    {
                        DelFolder(tmpPath, notDel, true);
                    }
                }
                string[] files = Directory.GetFiles(path);
                for (int i = files.Length - 1; i >= 0; --i)
                {
                    File.Delete(files[i]);
                }
                if (delSelf)
                {
                    string tmpPath = path.Replace('\\', '/');
                    bool delFlag = true;
                    for (int notIndex = 0, notMax = notDel.Length; notIndex < notMax; ++notIndex)
                    {
                        if (notDel[notIndex].StartsWith(tmpPath))
                        {
                            delFlag = false;
                        }
                    }
                    if (delFlag)
                    {
                        Directory.Delete(path);
                    }
                    if (File.Exists(path + ".meta"))
                    {
                        File.Delete(path + ".meta");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {

        }
    }

    static bool SetCommunity(bool isTest, bool haveModel, BuildTarget target)
    {
        LoadStartScene();
        if (!CheckDefault.CheckLauguageConfig())
        {
            EditorUtility.DisplayDialog("错误", "多语言配置文件缺少部分翻译", "确定");
            //return false;
        }
#if UNITY_ANDROID
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, null);
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, null);
#endif
        ClientMain.GetInst().useThirdAppFlag = true;
        ClientMain.GetInst().simulationUseThirdAppFlag = false;
        ClientMain.GetInst().debugLogFlag = isTest;
        ClientMain.GetInst().copyDefaultFlag = false;
        /*if (haveModel)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/default", sDefaultAry, true);
        }
        else
        {
            DelFolder(Application.streamingAssetsPath, sDefaultAry, true);
        }*/
        DelFolder(Application.streamingAssetsPath + "/defaultFiles", null, true);
        /*if (target == BuildTarget.Android)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/android" }, true);
        }
        else if (target == BuildTarget.iPhone)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/ios" }, true);
        }*/
        AssetDatabase.Refresh();
        //CheckDefault.CreateDefaultConfigNoDialog();
        
        //AssetDatabase.Refresh();
        return true;
    }

    static bool SetUnity(bool isTest, BuildTarget target)
    {
        LoadStartScene();
        if (!CheckDefault.CheckLauguageConfig())
        {
            EditorUtility.DisplayDialog("错误", "多语言配置文件缺少部分翻译", "确定");
            //return false;
        }
        ClientMain.GetInst().useThirdAppFlag = false;
        ClientMain.GetInst().simulationUseThirdAppFlag = false;
        ClientMain.GetInst().debugLogFlag = isTest;
        ClientMain.GetInst().copyDefaultFlag = true;
        if (isTest)
        {
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "USE_TEST");
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, "USE_TEST");
#endif
        }
        else
        {
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, null);
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, null);
#endif
        }
        DelFolder(Application.streamingAssetsPath + "/defaultFiles", null, true);
        /*if (target == BuildTarget.Android)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/android" }, true);
        }
        else if (target == BuildTarget.iPhone)
        {
            DelFolder(Application.streamingAssetsPath + "/defaultFiles/parts", new string[] { Application.streamingAssetsPath.Replace('\\', '/') + "/defaultFiles/parts/ios" }, true);
        }*/
        AssetDatabase.Refresh();
        /*CheckDefault.CreateDefaultConfigNoDialog();
        AssetDatabase.Refresh();*/
        return true;
    }
    static void SetAndroidSetting()
    {
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
        PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
        PlayerSettings.Android.targetDevice = AndroidTargetDevice.FAT;
    }

    static void SetIphoneSetting()
    {
#if UNITY_IPHONE
        PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, BuildTarget.iPhone);
        PlayerSettings.SetPropertyInt("Architecture", 2, BuildTarget.iPhone);
#endif
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
    }

    /// <summary>
    /// ios打包完以后处理部分文件
    /// </summary>
    static void IosFinishedDealFiles(string path)
    {
        /*string path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets"));
		path = path.Substring(0, path.LastIndexOf("/"));
		path += "/robot_ios/Roobt_ios_sq";*/
        string keyboardSrc = "Classes/Keyboard.mm";
        string keyboardDest = "Classes/UI/Keyboard.mm";
        string[] files = Directory.GetFiles(Path.Combine(path, "Libraries"));
        if (null != files)
        {
            for (int i = 0, imax = files.Length; i < imax; ++i)
            {
                if (files[i].EndsWith(".meta"))
                {
                    File.Delete(files[i]);
                }
            }
        }
        try
        {
            string str = Path.Combine(path, keyboardSrc).Replace("\\", "/");
            string dest = Path.Combine(path, keyboardDest).Replace("\\", "/");
            if (File.Exists(dest))
            {
                File.Delete(dest);
            }
            File.Move(str, dest);
        }
        catch (System.Exception ex)
        {
            Debug.Log("IosFinishedDealFiles error = " + ex.ToString());
        }
    }

    static void LoadStartScene()
    {
        string sceneName = "Assets/Scene/startScene.unity";
        if (!EditorApplication.currentScene.Equals("sceneName"))
        {
            EditorApplication.OpenScene(sceneName);
        }
    }


    /*[PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log(pathToBuiltProject);
    }*/
#endregion
}