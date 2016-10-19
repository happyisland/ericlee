using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Linq;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:MyTime.cs
/// Description:
/// Time:2015/12/2 10:40:13
/// </summary>

public class MyTime : SingletonObject<MyTime>
{
    public delegate void TimeCallBack(params object[] args);
    public class TimeData
    {
        public float time;
        float callTime;
        TimeCallBack callBack;
        public bool isPlaying;
        object[] args;
        public TimeData(float callTime, TimeCallBack ack, params object[] args)
        {
            time = 0;
            this.callTime = callTime;
            this.callBack = ack;
            this.args = args;
            isPlaying = true;
        }

        public bool TryCall()
        {
            if (time >= callTime)
            {
                if (null != callBack)
                {
                    callBack(args);
                }
                return true;
            }
            return false;
        }
    }

    List<TimeData> mTimeList = null;
    List<TimeData> mDelList = null;
    public MyTime()
    {
        mDelList = new List<TimeData>();
    }

    public void Update()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mDelList.Count; i < imax; ++i)
            {
                mTimeList.Remove(mDelList[i]);
            }
        }
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                if (mTimeList[i].isPlaying)
                {
                    mTimeList[i].time += Time.fixedDeltaTime;
                    if (mTimeList[i].TryCall())
                    {
                        mDelList.Add(mTimeList[i]);
                        /*if (mTimeList.Count != imax)
                        {
                            break;
                        }
                        mTimeList.RemoveAt(i);
                        --imax;
                        --i;*/
                    }
                }
            }
        }
    }

    public void PauseTime()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                mTimeList[i].isPlaying = false;
            }
        }
    }

    public void ContinueTime()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                mTimeList[i].isPlaying = true;
            }
        }
    }


    public void StopTime()
    {
        if (null != mTimeList)
        {
            for (int i = 0, imax = mTimeList.Count; i < imax; ++i)
            {
                mDelList.Add(mTimeList[i]);
            }
        }
    }



    public void Add(float start, TimeCallBack ack, params object [] args)
    {
        TimeData data = new TimeData(start, ack, args);
        if (null == mTimeList)
        {
            mTimeList = new List<TimeData>();
        }
        mTimeList.Add(data);
    }
}