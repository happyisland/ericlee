//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using Game.Event;
using System;
using Game.Platform;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(ClientMain), true)]
public class ClientMainEditor : UIWidgetContainerEditor
{
    int eventIdIndex = 0;
    string callUnityArg = string.Empty;
    int callUnityIndex = 0;
    string[] callUnityIdAry = null;
    string[] callUnityCache = null;
    string[] eventIdAry = null;
    string[] eventIdCache = null;
    string findCallUnity = string.Empty;
    string findEventId = string.Empty;
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
        if (null == callUnityIdAry)
        {
            Array arr = Enum.GetValues(typeof(CallUnityFuncID));
            callUnityIdAry = new string[arr.Length];
            callUnityCache = new string[arr.Length];
            for (int i = 0, imax = arr.Length; i < imax; ++i)
            {
                callUnityIdAry[i] = arr.GetValue(i).ToString();
                callUnityCache[i] = arr.GetValue(i).ToString();
            }
        }
        if (null == eventIdAry)
        {
            Array arr = Enum.GetValues(typeof(EventID));
            eventIdAry = new string[arr.Length];
            eventIdCache = new string[arr.Length];
            for (int i = 0, imax = arr.Length; i < imax; ++i)
            {
                eventIdAry[i] = arr.GetValue(i).ToString();
                eventIdCache[i] = arr.GetValue(i).ToString();
            }
        }
        NGUIEditorTools.DrawProperty("使用社区", serializedObject, "useThirdAppFlag");
        NGUIEditorTools.DrawProperty("模拟使用社区", serializedObject, "simulationUseThirdAppFlag");
        //NGUIEditorTools.DrawProperty("复制默认文件", serializedObject, "copyDefaultFlag");
        ClientMain clientMain = target as ClientMain;
        if (null != clientMain)
        {
            if (clientMain.debugLogFlag)
            {
                if (GUILayout.Button("关闭日志", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.SetLogState(false);
                }
            }
            else
            {
                if (GUILayout.Button("打开日志", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.SetLogState(true);
                }
            }
            if (clientMain.UseTestModelFlag)
            {
                if (GUILayout.Button("退出测试模式", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.UseTestModelFlag = false;
                }
            }
            else
            {
                if (GUILayout.Button("进入测试模式", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    clientMain.UseTestModelFlag = true;
                }
            }
			if (GUILayout.Button("打开测试标志", GUILayout.Width(100), GUILayout.Height(24)))
			{
#if UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "USE_TEST");
#elif UNITY_IPHONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iPhone, "USE_TEST");
#endif
			}
        }
        LauguageType lgType = LauguageTool.GetIns().CurLauguage;
        LauguageType tagType = lgType + 1;
        if (tagType == LauguageType.Ohter)
        {
            tagType = LauguageType.Chinese;
        }
        else if (tagType == LauguageType.Korean)
        {
            tagType = LauguageType.German;
        }
        if (GUILayout.Button("切换成" + tagType.ToString(), GUILayout.Width(100), GUILayout.Height(24)))
        {
            LauguageTool.GetIns().CurLauguage = tagType;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("EventID", GUILayout.Width(100));
        findEventId = GUILayout.TextField(findEventId, GUILayout.Width(60));
        if (GUILayout.Button("搜索", GUILayout.Width(40)))
        {
            List<string> cmdList = new List<string>();
            for (int i = 0, imax = eventIdAry.Length; i < imax; ++i)
            {
                string cmd = eventIdAry[i].ToString();
                if (string.IsNullOrEmpty(findEventId) || cmd.Contains(findEventId) || findEventId.Contains(cmd))
                {
                    cmdList.Add(cmd);
                }
            }
            eventIdCache = new string[cmdList.Count];
            eventIdIndex = 0;
            for (int i = 0, imax = cmdList.Count; i < imax; ++i)
            {
                eventIdCache[i] = cmdList[i];
            }
        }
        eventIdIndex = EditorGUILayout.Popup(eventIdIndex, eventIdCache/*, GUILayout.Width(200)*/);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(24)))
        {
            if (-1 != eventIdIndex)
            {
                EventID id = (EventID)Enum.Parse(typeof(EventID), eventIdCache[eventIdIndex]);
                EventMgr.Inst.Fire(id);
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("CallUnityFuncID", GUILayout.Width(100));
        findCallUnity = GUILayout.TextField(findCallUnity, GUILayout.Width(60));
        if (GUILayout.Button("搜索", GUILayout.Width(40)))
        {
            List<string> cmdList = new List<string>();
            for (int i = 0, imax = callUnityIdAry.Length; i < imax; ++i)
            {
                string cmd = callUnityIdAry[i].ToString();
                if (string.IsNullOrEmpty(findCallUnity) || cmd.Contains(findCallUnity) || findCallUnity.Contains(cmd))
                {
                    cmdList.Add(cmd);
                }
            }
            callUnityCache = new string[cmdList.Count];
            callUnityIndex = 0;
            for (int i = 0, imax = cmdList.Count; i < imax; ++i)
            {
                callUnityCache[i] = cmdList[i];
            }
        }
        callUnityIndex = EditorGUILayout.Popup(callUnityIndex, callUnityCache/*, GUILayout.Width(200)*/);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("参数", GUILayout.Width(100));
        
        callUnityArg = GUILayout.TextField(callUnityArg, /*GUILayout.Width(300), */GUILayout.Height(20));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("执行", GUILayout.Width(100), GUILayout.Height(24)))
        {
            if (callUnityIndex >= 0)
            {
                CallUnityFuncID id = (CallUnityFuncID)Enum.Parse(typeof(CallUnityFuncID), callUnityCache[callUnityIndex]);
                PlatformMgr.Instance.TestCallUnityFunc(id, callUnityArg);
            }
        }

        serializedObject.ApplyModifiedProperties();
	}
}
