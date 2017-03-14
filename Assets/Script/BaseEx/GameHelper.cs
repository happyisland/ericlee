using UnityEngine;
using System.Collections;
using Game.Platform;

public class GameHelper
{
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

    public static bool SetLabelText(Transform label, string text)
    {
        if (null != label)
        {
            UILabel lb = label.GetComponent<UILabel>();
            if (null != lb)
            {
                lb.text = text;
                return true;
            }
        }
        return false;
    }

    public static bool SetSprite(Transform sprite, string spriteName)
    {
        if (null != sprite)
        {
            UISprite sp = sprite.GetComponent<UISprite>();
            if (null != sp)
            {
                sp.spriteName = spriteName;
                sp.MakePixelPerfect();
                return true;
            }
        }
        return false;
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenPosition PlayTweenPosition(Transform trans, Vector3 to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenPosition tweenPosition = trans.GetComponent<TweenPosition>();
            if (null != tweenPosition)
            {
                PlayTweenPosition(tweenPosition, to, duration);
            }
            else
            {
                trans.localPosition = to;
            }
            return tweenPosition;
        }
        return null;
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenScale PlayTweenScale(Transform trans, Vector3 to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenScale tweenScale = trans.GetComponent<TweenScale>();
            if (null != tweenScale)
            {
                PlayTweenScale(tweenScale, to, duration);
            }
            else
            {
                trans.localScale = to;
            }
            return tweenScale;
        }
        return null;
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenRotation PlayTweenRota(Transform trans, Vector3 to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenRotation tweenRotation = trans.GetComponent<TweenRotation>();
            if (null != tweenRotation)
            {
                PlayTweenRota(tweenRotation, to, duration);
            }
            else
            {
                trans.localEulerAngles = to;
            }
            return tweenRotation;
        }
        return null;
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public static TweenAlpha PlayTweenAlpha(Transform trans, float to, float duration = 0.3f)
    {
        if (null != trans)
        {
            TweenAlpha tweenAlpha = trans.GetComponent<TweenAlpha>();
            if (null != tweenAlpha)
            {
                PlayTweenAlpha(tweenAlpha, to, duration);
            }
            else
            {
                SetTransformAlpha(trans, to);
            }
            return tweenAlpha;
        }
        return null;
    }

    public static void SetTransformAlpha(Transform trans, float alpha)
    {
        UIWidget widget = trans.GetComponent<UIWidget>();
        if (null != widget)
        {
            widget.alpha = alpha;
        }
        else
        {
            UIPanel uiPanel = trans.GetComponent<UIPanel>();
            if (null != uiPanel)
            {
                uiPanel.alpha = alpha;
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
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }


}
