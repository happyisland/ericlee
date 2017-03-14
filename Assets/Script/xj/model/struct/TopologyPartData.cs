using Game.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TopologyPartData.cs
/// Description:拓扑图零件数据
/// Time:2016/7/19 10:36:55
/// </summary>
public class TopologyPartData
{
    #region 公有属性
    /// <summary>
    /// id
    /// </summary>
    public byte id;
    /// <summary>
    /// 零件类型
    /// </summary>
    public TopologyPartType partType;
    /// <summary>
    /// true表示未配置，处于独立队列
    /// </summary>
    public bool isIndependent;
    /// <summary>
    /// 相对位置
    /// </summary>
    public Vector2 localPosition;
    /// <summary>
    /// 记录旋转角度
    /// </summary>
    public Vector3 localEulerAngles;
    /// <summary>
    /// 零件宽度
    /// </summary>
    public int width;
    /// <summary>
    /// 零件长度
    /// </summary>
    public int height;
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    public TopologyPartData()
    {
        id = 0;
        partType = TopologyPartType.Servo;
        isIndependent = false;
        localPosition = Vector3.zero;
        localEulerAngles = Vector3.zero;
        width = 0;
        height = 0;
    }

    public TopologyPartData(TopologyPartData data)
    {
        this.id = data.id;
        this.partType = data.partType;
        this.isIndependent = data.isIndependent;
        this.localPosition = data.localPosition;
        this.localEulerAngles = data.localEulerAngles;
        this.width = data.width;
        this.height = data.height;
    }

    public TopologyPartData(string data)
    {
        try
        {
            string[] ary = data.Split(' ');
            for (int i = 0, imax = ary.Length; i < imax; ++i)
            {
                switch (i)
                {
                    case 0:
                        id = byte.Parse(ary[i]);
                        break;
                    case 1:
                        if (PublicFunction.IsInteger(ary[i]))
                        {
                            byte num = byte.Parse(ary[i]);
                            if (num >= 8)
                            {//兼容以前的数据
                                num += 1;
                            }
                            partType = (TopologyPartType)num;
                        }
                        else
                        {
                            partType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), ary[i]);
                        }
                        break;
                    case 2:
                        isIndependent = bool.Parse(ary[i]);
                        break;
                    case 3:
                        List<float> list = PublicFunction.StringToFloatList(ary[i]);
                        if (list.Count >= 2)
                        {
                            localPosition.x = list[0];
                            localPosition.y = list[1];
                        }
                        break;
                    case 4:
                        List<float> list1 = PublicFunction.StringToFloatList(ary[i]);
                        if (list1.Count >= 3)
                        {
                            localEulerAngles.x = list1[0];
                            localEulerAngles.y = list1[1];
                            localEulerAngles.z = list1[2];
                        }
                        break;
                    case 5:
                        width = int.Parse(ary[i]);
                        break;
                    case 6:
                        height = int.Parse(ary[i]);
                        break;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(id);
        sb.Append(' ');
        sb.Append(partType.ToString());
        sb.Append(' ');
        sb.Append(isIndependent);
        sb.Append(' ');
        sb.Append(localPosition.x);
        sb.Append(PublicFunction.Separator_Comma);
        sb.Append(localPosition.y);
        sb.Append(' ');
        sb.Append(localEulerAngles.x);
        sb.Append(PublicFunction.Separator_Comma);
        sb.Append(localEulerAngles.y);
        sb.Append(PublicFunction.Separator_Comma);
        sb.Append(localEulerAngles.z);
        sb.Append(' ');
        sb.Append(width);
        sb.Append(' ');
        sb.Append(height);
        return sb.ToString();
    }
    #endregion

    #region 其他函数
    #endregion
}
/// <summary>
/// 零件接口数据
/// </summary>
public class PartPortData
{
    public PartPortType portType;
    public GameObject portObj;

    public PartPortData()
    {

    }
}

/// <summary>
/// 拓扑图零件类型
/// </summary>
public enum TopologyPartType : byte
{
    None = 0,
    Infrared = 1,//红外
    Touch = 2,//触碰
    Gyro = 3,//陀螺仪
    Light = 4,//灯光
    Gravity = 5,//重力
    Ultrasonic = 6,//超声
    DigitalTube = 7,//数码管
    Speaker,//蓝牙喇叭
    MainBoard,//主板
    Servo,//舵机
    Line,//直线
    Line_Angle,//拐角
}

/// <summary>
/// 零件接口类型
/// </summary>
public enum PartPortType : byte
{
    Port_Type_Pin_3,
    Port_Type_Pin_4
}