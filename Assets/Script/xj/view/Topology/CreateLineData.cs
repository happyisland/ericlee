using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:CreateLineData.cs
/// Description:生成连线数据
/// Time:2016/9/21 13:56:52
/// </summary>
public class CreateLineData
{
    #region 公有属性
    public int offsetRange = 20;
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    public List<TopologyPartData> CreateLine(Transform obj, Transform otherObj)
    {
        List<TopologyPartData> list = new List<TopologyPartData>();
        return list;
    }
    #endregion

    #region 其他函数
    /// <summary>
    /// 生成水平直线
    /// </summary>
    /// <returns></returns>
    Vector2 CreateHorizontalLine(out TopologyPartData data, Vector2 pos, Vector2 otherPos)
    {
        data = new TopologyPartData();
        data.partType = TopologyPartType.Line;
        return pos;
    }
    /// <summary>
    /// 生成垂直直线
    /// </summary>
    /// <returns></returns>
    Vector2 CreateVerticalLine()
    {
        return Vector2.zero;
    }
    /// <summary>
    /// 生成拐角
    /// </summary>
    /// <returns></returns>
    Vector2 CreateAngleLine()
    {
        return Vector2.zero;
    }
    #endregion
}