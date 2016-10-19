using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:ReadSensorDataBase.cs
/// Description:读取传感器数据
/// Time:2016/10/8 10:22:35
/// </summary>
public class ReadSensorDataBase
{
    #region 公有属性
    #endregion

    #region 其他属性
    protected List<byte> ids;
    protected List<byte> readIds;

    protected List<byte> errIds;
    protected List<byte> backIds;
    #endregion

    #region 公有函数

    public ReadSensorDataBase()
    {
    }

    public ReadSensorDataBase(List<byte> ids)
    {
        this.ids = new List<byte>();
        this.ids.AddRange(ids);
    }

    public virtual void ReadDataMsg(List<byte> ids)
    {
        readIds = ids;
    }
    
    public virtual void ReadCallBackMsg(BinaryReader br)
    {
        try
        {
            byte errid = br.ReadByte();
            byte id = br.ReadByte();
            errIds = new List<byte>();
            backIds = new List<byte>();
            for (int i = 0; i < 8; ++i)
            {
                if (errid != 0 && ((errid & (1 << i)) > 0))
                {
                    errIds.Add((byte)(i + 1));
                }
                if (id != 0 && ((id & (1 << i)) > 0))
                {
                    backIds.Add((byte)(i + 1));
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
    

    public virtual object GetReadResult()
    {
        return string.Empty;
    }

    public virtual string GetOnlyTypeReadResult()
    {
        return string.Empty;
    }
    #endregion

    #region 其他函数
    #endregion
}