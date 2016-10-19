using System;
using UnityEngine;
using System.IO;
using Game.Resource;
using System.Text;

/// <summary>
/// Author:xj
/// FileName:MyLog.cs
/// Description:
/// Time:2015/12/29 18:50:51
/// </summary>
public class MyLog
{
    #region 公有属性
    #endregion

    #region 其他属性
    FileStream mFileStream;
    StreamWriter mStreamWriter;
    static MyLog mInst;
    #endregion

    #region 公有函数


    static public bool EnableLog = true;

    static public bool EnableLocalFile = true;



    public MyLog()
    {
        mInst = this;
    }

    public static void CloseMyLog()
    {
        if (null != mInst)
        {
            mInst.mStreamWriter.Dispose();
            mInst.mStreamWriter.Close();
            mInst.mFileStream.Dispose();
            mInst.mFileStream.Close();
            mInst = null;
        }
    }
    static public void Log(object message)
    {
        Log(message, null);
    }


    static public void Log(object message, UnityEngine.Object context)
    {
        if (EnableLog)
        {
            Debug.Log(message, context);
            if (EnableLocalFile)
            {
                MyLogWrite(message.ToString());
            }
        }
    }


    static public void LogError(object message)
    {
        LogError(message, null);
    }


    static public void LogError(object message, UnityEngine.Object context)
    {
        if (EnableLog)
        {
            Debug.LogError(message, context);
            if (EnableLocalFile)
            {
                MyLogWrite(message.ToString());
            }
        }
    }


    static public void LogWarning(object message)
    {
        LogWarning(message, null);
    }


    static public void LogWarning(object message, UnityEngine.Object context)
    {
        if (EnableLog)
        {
            Debug.LogWarning(message, context);
            if (EnableLocalFile)
            {
                MyLogWrite(message.ToString());
            }
        }
    }
    #endregion

    #region 其他函数

    static void MyLogWrite(string text)
    {
        text = DateTime.Now.ToLocalTime() + " " + text;
        if (null == mInst)
        {
            mInst = new MyLog();
            string path = ResourcesEx.persistentDataPath + "/log.txt";
            string readText = string.Empty;
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader sr = new StreamReader(fs);
                    readText = sr.ReadToEnd();
                    sr.Dispose();
                    sr.Close();
                    fs.Dispose();
                    fs.Close();
                }
            }
            mInst.mFileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            mInst.mStreamWriter = new StreamWriter(mInst.mFileStream);
            if (!string.IsNullOrEmpty(readText))
            {
                mInst.mStreamWriter.WriteLine(readText);
            }
        }
        //text += ReadText();
        mInst.mStreamWriter.WriteLine(text);
        mInst.mStreamWriter.Flush();
        //WriteText(text);
    }
    static string ReadText()
    {
        string text = string.Empty;
        try
        {
            string path = ResourcesEx.persistentDataPath + "/log.txt";
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs);
                text = sr.ReadToEnd();
                sr.Dispose();
                sr.Close();
            }
            
        }
        catch (System.Exception ex)
        {
        	
        }
        return text;
    }
    static void WriteText(string text)
    {
        try
        {
            string path = ResourcesEx.persistentDataPath + "/log.txt";
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sr = new StreamWriter(fs);
            sr.Flush();
            sr.WriteLine(text);
            sr.Close();
            sr.Dispose();
        }
        catch (System.Exception ex)
        {

        }
    }
    #endregion
}