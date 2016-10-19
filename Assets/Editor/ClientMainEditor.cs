//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ClientMain), true)]
public class ClientMainEditor : UIWidgetContainerEditor
{
	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

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
        
		serializedObject.ApplyModifiedProperties();
	}
}
