using UnityEngine;
using System.Collections;

public class GameHelper
{
    public static object FindChildCompoent(Transform trans, string path, string typeName)
    {
        if (trans != null)
        {

            Transform tmp = trans.Find(path);
            if (tmp != null)
            {
                object obj = tmp.GetComponent(typeName);
                if (obj == null)
                {
                    Debuger.Log(typeName + " Component was not found in the " + path);
                }
                return obj;
            }
            else
            {
                Debuger.Log(path + " was not found in the " + trans + "child");
            }
        }
        else
        {
            Debuger.Log("parent is null,when you find " + path);
        }
        return null;
    }

    public static T FindChildComponent<T>(Transform trans, string path) where T : Component
    {
        if (trans != null)
        {

            Transform tmp = trans.Find(path);
            if (tmp != null)
            {
                T obj = tmp.GetComponent<T>();
                if (obj == null)
                {
                    Debuger.Log(typeof(T).Name + " Component was not found in the " + path);
                }
                return obj;
            }
            else
            {
                Debuger.Log(path + " was not found in the " + trans + "child");
            }
        }
        else
        {
            Debuger.Log("parent is null,when you find " + path);
        }
        return null;
    }

    public static void PlayTweenPosition(TweenPosition tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localPosition;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    public static void PlayTweenScale(TweenScale tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localScale;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    public static void PlayTweenRota(TweenRotation tweens, Vector3 to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                Vector3 from = tweens.transform.localEulerAngles;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }

    public static void PlayTweenAlpha(TweenAlpha tweens, float to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                float from = tweens.value;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }
    public static void PlayMyTweenAlpha(MyTweenAlpha tweens, float to, float duration = 0.3f)
    {
        try
        {
            if (null != tweens)
            {
                tweens.enabled = true;
                float from = tweens.value;
                tweens.ResetToBeginning();
                tweens.duration = duration;
                tweens.from = from;
                tweens.to = to;
                tweens.Play(true);
            }
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }
}
