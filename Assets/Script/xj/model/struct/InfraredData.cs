using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:InfraredData.cs
/// Description:红外的数据
/// Time:2016/8/6 15:50:45
/// </summary>
public class InfraredData : ReadSensorDataBase
{
    #region 公有属性

    #endregion

    #region 其他属性
    Dictionary<byte, UInt16> dataDict;
    #endregion

    #region 公有函数
    public InfraredData()
    {
        dataDict = new Dictionary<byte, UInt16>();
    }

    public InfraredData(List<byte> ids) : base(ids)
    {
        dataDict = new Dictionary<byte, UInt16>();
        for (int i = 0, imax = ids.Count; i < imax; ++i)
        {
            dataDict[ids[i]] = 0;
        }
    }


    public override object GetReadResult()
    {
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
        if (readIds[0] == 0)
        {
            foreach (KeyValuePair<byte, UInt16> kvp in dataDict)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["id"] = kvp.Key;
                dict["result"] = kvp.Value;
                result.Add(dict);
            }
        }
        else
        {
            for (int i = 0, imax = readIds.Count; i < imax; ++i)
            {
                if (dataDict.ContainsKey(readIds[i]))
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict["id"] = readIds[i];
                    dict["result"] = dataDict[readIds[i]];
                    result.Add(dict);
                }
            }
        }
        return result;
    }

    public override string GetOnlyTypeReadResult()
    {
        StringBuilder sb = new StringBuilder();
        if (readIds[0] == 0)
        {
            foreach (KeyValuePair<byte, UInt16> kvp in dataDict)
            {
                if (sb.Length > 0)
                {
                    sb.Append(PublicFunction.Separator_Comma);
                }
                sb.Append(kvp.Value);
            }
        }
        else
        {
            for (int i = 0, imax = readIds.Count; i < imax; ++i)
            {
                if (sb.Length > 0)
                {
                    sb.Append(PublicFunction.Separator_Comma);
                }
                if (dataDict.ContainsKey(readIds[i]))
                {
                    sb.Append(dataDict[readIds[i]]);
                }
                else
                {
                    sb.Append(0);
                }
            }
        }
        return sb.ToString();
    }

    public override void ReadCallBackMsg(BinaryReader br)
    {
        
        try
        {
            base.ReadCallBackMsg(br);
            if (null != backIds)
            {
                for (int i = 0, imax = backIds.Count; i < imax; ++i)
                {
                    ReadInfraredDataMsgAck msg = new ReadInfraredDataMsgAck();
                    msg.Read(br);
                    if (dataDict.ContainsKey(backIds[i]))
                    {
                        dataDict[backIds[i]] = msg.arg;
                    }
                }
            }
            if (null != errIds)
            {
                for (int i = 0, imax = errIds.Count; i < imax; ++i)
                {
                    if (dataDict.ContainsKey(errIds[i]))
                    {
                        dataDict[errIds[i]] = 0;
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

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UInt16 GetData(byte id)
    {
        if (dataDict.ContainsKey(id))
        {
            return dataDict[id];
        }
        return 0;
    }

    #endregion

    #region 其他函数
    #endregion
}