using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:Action.cs
/// Description:动作
/// Time:2015/6/26 10:47:41
/// </summary>
public class Action
{
#region 公有属性
    /// <summary>
    /// 动作索引
    /// </summary>
    public int index;
    /// <summary>
    /// 动作运动时间
    /// </summary>
    public int sportTime;
    /// <summary>
    /// 补时，即停留时间
    /// </summary>
    public int waitTime;
    /// <summary>
    /// 动作舵机角度
    /// </summary>
    public Dictionary<byte, short> rotas = null;
    /// <summary>
    /// 处于旋转模式下的舵机
    /// </summary>
    public Dictionary<byte, TurnData> turnDict = null;
    /// <summary>
    /// 最小的舵机id
    /// </summary>
    public byte idMin = 0;
    /// <summary>
    /// 最小舵机id对应的角度
    /// </summary>
    public short idMinRota;
    /// <summary>
    /// 需要显示的舵机列表
    /// </summary>
    public List<byte> showList;

    public int AllTime
    {
        get { return sportTime + waitTime; }
    }
#endregion

#region 私有属性
#endregion
    
#region 公有函数
    public Action()
    {
        Init(0, PublicFunction.Default_Actions_Time, 0);
    }
    public Action(int index)
    {
        Init(index, PublicFunction.Default_Actions_Time, 0);
    }

    public Action(XmlElement xe)
    {
        rotas = new Dictionary<byte, short>();
        turnDict = new Dictionary<byte, TurnData>();
        try
        {
            index = int.Parse(xe.GetAttribute("index"));
            sportTime = int.Parse(xe.GetAttribute("sportTime"));
            waitTime = int.Parse(xe.GetAttribute("waitTime"));
            idMin = byte.Parse(xe.GetAttribute("idMin"));
            idMinRota = short.Parse(xe.GetAttribute("idMinRota"));
            string str = xe.GetAttribute("rotas");
            string[] ary = str.Split(';');
            if (null != ary && ary.Length > 0)
            {
                for (int i = 0, icount = ary.Length; i < icount; ++i)
                {
                    string[] tmp = ary[i].Split('$');
                    if (null != tmp && 2 == tmp.Length)
                    {
                        rotas[byte.Parse(tmp[0])] = short.Parse(tmp[1]);
                    }
                }
            }
            string str1 = xe.GetAttribute("turns");
            if (!string.IsNullOrEmpty(str1))
            {
                string[] ary1 = str1.Split(';');
                if (null != ary1 && ary1.Length > 0)
                {
                    for (int i = 0, icount = ary1.Length; i < icount; ++i)
                    {
                        string[] tmp1 = ary1[i].Split('$');
                        if (null != tmp1 && 2 == tmp1.Length)
                        {
                            string[] tmp2 = tmp1[1].Split(',');
                            if (null != tmp2 && 2 == tmp2.Length)
                            {
                                TurnData data;
                                data.turnDirection = (TurnDirection)(byte.Parse(tmp2[0]));
                                data.turnSpeed = ushort.Parse(tmp2[1]);
                                turnDict[byte.Parse(tmp1[0])] = data;
                            }
                        }
                    }
                }
            }

            string str2 = xe.GetAttribute("show");
            if (!string.IsNullOrEmpty(str2))
            {
                string[] ary2 = str2.Split(';');
                if (null != ary2 && ary2.Length > 0)
                {
                    showList = new List<byte>();
                    for (int i = 0, imax = ary2.Length; i < imax; ++i)
                    {
                        showList.Add(byte.Parse(ary2[i]));
                    }
                }
            }
            /*else
            {
                showList = new List<byte>();
                foreach (KeyValuePair<byte, short> kvp in rotas)
                {
                    showList.Add(kvp.Key);
                }
            }*/
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


    public Action(int index, int sportTime)
    {
        Init(index, sportTime, 0);
    }

    public int GetRota(byte id)
    {
        if (rotas.ContainsKey(id))
        {
            return rotas[id];
        }
        return PublicFunction.DuoJi_Start_Rota;
    }
    public void UpdateData(int index, byte id, int sportTime, int waitTime)
    {
        Init(index, sportTime, waitTime);
    }
    /// <summary>
    /// 修改或添加一个舵机的角度
    /// </summary>
    /// <param name="id"></param>
    /// <param name="rota"></param>
    public void UpdateRota(byte id, short rota)
    {
        if (turnDict.ContainsKey(id))
        {
            turnDict.Remove(id);
        }
        rotas[id] = rota;
         if (idMin == 0 || id < idMin)
         {
             idMin = id;
             idMinRota = rota;
         }
    }
    /// <summary>
    /// 增加或更新连续转动舵机的数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    public void UpdateTurn(byte id, TurnData data)
    {
        if (rotas.ContainsKey(id))
        {
            rotas.Remove(id);
        }
        turnDict[id] = data;
        if (idMin == 0 || id < idMin)
        {
            idMin = id;
        }
    }
    /// <summary>
    /// 增加或更新连续转动舵机的数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dir"></param>
    /// <param name="speed"></param>
    public void UpdateTurn(byte id, TurnDirection dir, int speed)
    {//未修改最小id
        if (rotas.ContainsKey(id))
        {
            rotas.Remove(id);
        }
        TurnData data;
        data.turnDirection = dir;
        data.turnSpeed = (ushort)speed;
        turnDict[id] = data;
    }
    /// <summary>
    /// 修改舵机id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    public void SwitchDuoJiId(byte id, byte targetId)
    {
        if (rotas.ContainsKey(id))
        {
            if (rotas.ContainsKey(targetId))
            {
                short rota = rotas[id];
                rotas[id] = rotas[targetId];
                rotas[targetId] = rota;
            }
            else if (turnDict.ContainsKey(targetId))
            {
                turnDict[id] = turnDict[targetId];
                turnDict.Remove(targetId);
                rotas[targetId] = rotas[id];
                rotas.Remove(id);
            }
            else
            {
                rotas[targetId] = rotas[id];
                rotas.Remove(id);
            }
        }
        else if (turnDict.ContainsKey(id))
        {
            if (turnDict.ContainsKey(targetId))
            {
                TurnData tmpData = turnDict[id];
                turnDict[id] = turnDict[targetId];
                turnDict[targetId] = tmpData;
            }
            else if (rotas.ContainsKey(targetId))
            {
                turnDict[targetId] = turnDict[id];
                turnDict.Remove(id);
                rotas[id] = rotas[targetId];
                rotas.Remove(targetId);
            }
            else
            {
                turnDict[targetId] = turnDict[id];
                turnDict.Remove(id);
            }
        }
        if (targetId < idMin)
        {
            idMin = targetId;
        }
        else if (id == idMin && targetId > idMin)
        {
            idMin = byte.MaxValue;
            foreach (KeyValuePair<byte, short> kvp in rotas)
            {
                if (kvp.Key < idMin)
                {
                    idMin = kvp.Key;
                }
            }
            foreach (KeyValuePair<byte, TurnData> kvp in turnDict)
            {
                if (kvp.Key < idMin)
                {
                    idMin = kvp.Key;
                }
            }
        }
    }

    public void Copy(Action action)
    {
        sportTime = action.sportTime;
        waitTime = action.waitTime;
        idMin = action.idMin;
        idMinRota = action.idMinRota;
        rotas.Clear();
        foreach (KeyValuePair<byte, short> kvp in action.rotas)
        {
            UpdateRota(kvp.Key, kvp.Value);
        }
        turnDict.Clear();
        foreach (KeyValuePair<byte, TurnData> kvp in action.turnDict)
        {
            turnDict[kvp.Key] = kvp.Value;
        }
        if (null != action.showList)
        {
            showList = new List<byte>();
            for (int i = 0, imax = action.showList.Count; i < imax; ++i)
            {
                showList.Add(action.showList[i]);
            }
        }
        else
        {
            showList = null;
        }
    }
    /// <summary>
    /// 判断动作是否有轮模式
    /// </summary>
    /// <returns></returns>

    public bool IsTrunModel()
    {
        if (turnDict.Count > 0)
        {
            return true;
        }
        return false;
    }

    public void AddShowID(int id)
    {
        if (null == showList)
        {
            showList = new List<byte>();
        }
        showList.Add((byte)id);
    }

    public void RomoveShowID(int id)
    {
        if (null != showList)
        {
            showList.Remove((byte)id);
        }
    }

    public void CleanUp()
    {
        idMin = 0;
        idMinRota = 0;
        rotas.Clear();
        turnDict.Clear();
        showList.Clear();
        showList = null;
    }
    /// <summary>
    /// 把数据转换成xml节点
    /// </summary>
    /// <param name="xe"></param>
    /// <returns></returns>
    public XmlElement ConvertToNode(XmlElement xe)
    {
        if (null != xe)
        {
            xe.SetAttribute("index", index.ToString());
            xe.SetAttribute("sportTime", sportTime.ToString());
            xe.SetAttribute("waitTime", waitTime.ToString());
            xe.SetAttribute("idMin", idMin.ToString());
            xe.SetAttribute("idMinRota", idMinRota.ToString());
            xe.SetAttribute("rotas", RotasToString());
            xe.SetAttribute("turns", TurnDataToString());
            xe.SetAttribute("show", ShowListToString());
        }
        return xe;
    }
    

#endregion
    


#region 私有函数
    private void Init(int index, int sportTime, int waitTime)
    {
        this.index = index;
        this.sportTime = sportTime;
        this.waitTime = waitTime;
        rotas = new Dictionary<byte, short>();
        turnDict = new Dictionary<byte, TurnData>();
    }

    private string RotasToString()
    {
        string str = string.Empty;
        foreach (KeyValuePair<byte, short> kvp in rotas)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str += ";";
            }
            str += kvp.Key + "$" + kvp.Value;
        }
        return str;
    }

    private string TurnDataToString()
    {
        string str = string.Empty;
        foreach (KeyValuePair<byte, TurnData> kvp in turnDict)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str += ";";
            }
            str += kvp.Key + "$" + (byte)kvp.Value.turnDirection + "," + kvp.Value.turnSpeed;
        }
        return str;
    }

    private string ShowListToString()
    {
        string str = string.Empty;
        if (null != showList)
        {
            for (int i = 0, imax = showList.Count; i < imax; ++i)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += ";";
                }
                str += showList[i];
            }
        }
        return str;
    }
#endregion

    
}

