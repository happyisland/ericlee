using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;
/// <summary>
/// Author:xj
/// FileName:AttendanceEditor.cs
/// Description:
/// Time:2016/10/22 17:19:37
/// </summary>
public class AttendanceEditor : EditorWindow
{
    #region 公有属性
    #endregion

    #region 其他属性
    string id = "80040";
    string startDay;
    string endDay;
    Dictionary<int, float> workTimeDict;
    #endregion

    #region 公有函数
    [MenuItem("MyTool/查询考勤")]
    public static void OpenCopyFilesEditor()
    {
        AttendanceEditor windows = EditorWindow.GetWindow<AttendanceEditor>(true, "AttendanceEditor");
        windows.position = new Rect(400, 300, 500, 550);
    }

    public AttendanceEditor()
    {
        id = "80040";
        startDay = "1";
        endDay = "31";
        workTimeDict = new Dictionary<int, float>();
    }
    #endregion

    #region 其他函数
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("工号", GUILayout.Width(100));
        id = GUILayout.TextField(id, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("起始日期(天)", GUILayout.Width(100));
        startDay = GUILayout.TextField(startDay, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("结束日期(天)", GUILayout.Width(100));
        endDay = GUILayout.TextField(endDay, GUILayout.Width(200));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("查询", GUILayout.Width(100)))
        {
            int start = int.Parse(startDay);
            int end = int.Parse(endDay);
            if (end > start && start > 0)
            {
                GetAttendanceTime(id, start, end);
            }

        }
        GUILayout.EndVertical();
    }

    void GetAttendanceTime(string id, int start, int end)
    {
        string result = PublicFunction.HttpGet("http://10.10.1.60/kaoqin/Default.aspx", string.Format("sLibID={0}", id));
        if (!string.IsNullOrEmpty(result))
        {
            string regex = "<font color=\"#\\d+\">\\d+年\\d+月\\d+日 \\d+时\\d+分\\d+秒</font></td><td><font color=\"#\\d+\">\\d+年\\d+月\\d+日 \\d+时\\d+分\\d+秒</font>";
            MatchCollection matchs = Regex.Matches(result, regex);
            if (null != matchs)
            {
                foreach (Match mc in matchs)
                {
                    string str = mc.Value.Trim();
                    string timeRegex = "\\d+年\\d+月\\d+日 \\d+时\\d+分\\d+秒";
                    MatchCollection timeMatchs = Regex.Matches(str, timeRegex);
                    TimeData data = GetWorkTime(timeMatchs);
                    if (null != data)
                    {
                        workTimeDict[data.day] = data.time;
                    }
                }
                float workTime = 0;
                foreach (var kvp in workTimeDict)
                {
                    if (kvp.Key >= start && kvp.Key <= end)
                    {
                        workTime += kvp.Value;
                    }
                }
                EditorUtility.DisplayDialog("提示", string.Format("工作时间为{0}", workTime), "确定");
            }

        }
    }

    TimeData GetWorkTime(MatchCollection matchs)
    {
        if (matchs.Count != 2)
        {
            return null;
        }
        DateTime start = DateTime.Parse(matchs[0].Value.Trim().Replace("年", "-").Replace("月", "-").Replace("日", "").Replace("时", ":").Replace("分", ":").Replace("秒", ""));
        DateTime end = DateTime.Parse(matchs[1].Value.Trim().Replace("年", "-").Replace("月", "-").Replace("日", "").Replace("时", ":").Replace("分", ":").Replace("秒", ""));
        TimeData data = new TimeData();
        data.day = start.Day;
        TimeSpan span = end - start;
        if (span.TotalMilliseconds <= 1)
        {
            data.time = 0;
            return data;
        }
        else if (start.Hour >= 19)
        {
            data.time = 0;
            return data;
        }
        data.time = (float)span.TotalHours;
        DateTime tmp1 = new DateTime(start.Year, start.Month, start.Day, 12, 0, 0);
        DateTime tmp2 = new DateTime(start.Year, start.Month, start.Day, 13, 30, 0);
        if (start <= tmp1 && end >= tmp2)
        {
            data.time -= 1.5f;
        }
        else if (start >= tmp1 && end <= tmp2)
        {
            data.time = 0;
        }
        else if (start >= tmp1)
        {
            data.time -= (float)(tmp2 - start).TotalHours;
        }
        else if (start <= tmp1 && end >= tmp1 && end <= tmp2)
        {
            data.time -= (float)(end - tmp1).TotalHours;
        }
        if (end.Hour > 19 || end.Hour == 19 && end.Minute >= 30)
        {
            data.time -= 0.5f;
        }
        else if (end.Hour == 19)
        {
            data.time -= end.Minute / 60.0f;
        }
        return data;
    }
    #endregion

    public class TimeData
    {
        public int day;
        public float time;
        public TimeData()
        {

        }
    }
}