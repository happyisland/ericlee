﻿using Game.Platform;
using Game.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:PopWinManager.cs
/// Description:弹出窗口管理
/// Time:2015/7/27 9:35:38
/// </summary>
public class PopWinManager : SingletonObject<PopWinManager>
{
    #region 公有属性
    #endregion

    #region 私有属性
    List<BasePopWin> mPopWinList;
    const int Depth_Offset = 5;
    #endregion

    #region 公有函数
    public BasePopWin ShowPopWin(Type popWin, object[] args = null, float alpha = 0.9f)
    {
        try
        {
            /*if (popWin == typeof(ConnenctBluetoothMsg))
            {
                ShowPopWin(typeof(SearchBluetoothMsg));
                return null;
            }*/
            BasePopWin basePopWin;
            if (null == args)
            {
                basePopWin = Activator.CreateInstance(popWin) as BasePopWin;
            }
            else
            {
                basePopWin = Activator.CreateInstance(popWin, args) as BasePopWin;
            }
            if (basePopWin.isSingle)
            {
                ClosePopWin(popWin);
            }
            if (null != basePopWin)
            {
                int dep = 0;
                if (mPopWinList.Count > 0)
                {
                    dep = mPopWinList[mPopWinList.Count - 1].mDepth + Depth_Offset;
                }
                mPopWinList.Add(basePopWin);
                basePopWin.SetDepth(dep);
                basePopWin.Open();
            }
            return basePopWin;
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
        return null;
    }
    public void Init()
    {
        mPopWinList = new List<BasePopWin>();
    }
    public void Update()
    {
        try
        {
            if (null != mPopWinList && mPopWinList.Count > 0)
            {
                for (int i = 0, icount = mPopWinList.Count; i < icount; ++i)
                {
                    if (null != mPopWinList[i])
                    {
                        if (mPopWinList[i].IsOpen)
                        {
                            mPopWinList[i].Update();
                            if (mPopWinList.Count != icount)
                            {//在update里面有删除消息
                                --icount;
                                --i;
                            }
                        }
                    }
                    else
                    {
                        mPopWinList.RemoveAt(i);
                        --icount;
                        --i;
                    }

                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void LateUpdate()
    {
        try
        {
            if (null != mPopWinList && mPopWinList.Count > 0)
            {
                for (int i = 0, icount = mPopWinList.Count; i < icount; ++i)
                {
                    if (null != mPopWinList[i])
                    {
                        if (mPopWinList[i].IsOpen)
                        {
                            mPopWinList[i].LateUpdate();
                        }
                    }
                    else
                    {
                        mPopWinList.RemoveAt(i);
                        --icount;
                        --i;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="popWin"></param>
    public void ClosePopWin(Type popWin)
    {
        try
        {
            if (null != mPopWinList && mPopWinList.Count > 0)
            {
                for (int i = 0, icount = mPopWinList.Count; i < icount; ++i)
                {
                    if (mPopWinList[i].GetType().Equals(popWin))
                    {
                        mPopWinList[i].OnClose();
                        //mPopWinList.RemoveAt(i);
                        --icount;
                        --i;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    public void RemovePopWin(BasePopWin win)
    {
        try
        {
            if (null != mPopWinList)
            {
                mPopWinList.Remove(win);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    /// <summary>
    /// 判断某个弹框是否存在
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsExist(Type type)
    {
        try
        {
            if (null != mPopWinList)
            {
                for (int i = 0, imax = mPopWinList.Count; i < imax; ++i)
                {
                    if (mPopWinList[i].GetType() == type)
                    {
                        return true;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        return false;
    }
    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAll()
    {
        try
        {
            if (null != mPopWinList && mPopWinList.Count > 0)
            {
                for (int i = 0, icount = mPopWinList.Count; i < icount; ++i)
                {
                    mPopWinList[i].OnClose();
                }
                mPopWinList.Clear();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }
    #endregion

    #region 私有函数
    #endregion
}


