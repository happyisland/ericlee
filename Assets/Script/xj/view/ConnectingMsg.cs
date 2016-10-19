using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Resource;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:ConnectingMsg.cs
/// Description:连接页面
/// Time:2016/4/1 13:55:01
/// </summary>
public class ConnectingMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    TweenPosition mViewTweenPosition;
    Transform mContentTrans;
    Dictionary<string, Texture> mTextureDict;
    int mTextureIndex = 0;
    int mTextureIndexMax = 21;
    string mBgTexFont = "left_";
    UITexture mLeftTex;
    UITexture mRightTex;
    float mTime;
    string mBlueName;
    static ConnectingMsg mInst;
    Vector2 mPhoneStartSize;
    Vector2 mPhoneSize;
    Vector2 mPhonePos;
    TweenPosition mPhoneTweenPosition;
    TweenScale mPhoneTweenScale;
    TweenAlpha mBlueTweenAlpha;
    TweenAlpha mDeviceTweenAlpha;
    TweenScale mBlueTweenScale;
    TweenScale mDeviceTweenScale;

    float mConnectingTime;
    #endregion

    #region 公有函数
    public ConnectingMsg(string name, Vector2 size)
    {
        mUIResPath = "Prefab/UI/ConnectingMsg";
        mTextureDict = new Dictionary<string, Texture>();
        mBlueName = name;
        isSingle = true;
        mInst = this;
        mPhoneStartSize = size;
    }


    public static void ShowMsg(string blueName, Vector2 size)
    {
        if (null != mInst)
        {
            mInst.OnShow();
            if (null != mInst.mContentTrans)
            {
                mInst.mBlueName = blueName;
                mInst.mPhoneStartSize = size;
                UILabel label = GameHelper.FindChildComponent<UILabel>(mInst.mContentTrans, "device/Label");
                if (null != label)
                {
                    label.text = blueName;
                }
            }
            mInst.OpenViewAnim();
        }
        else
        {
            object[] args = new object[2];
            args[0] = blueName;
            args[1] = size;
            PopWinManager.GetInst().ShowPopWin(typeof(ConnectingMsg), args);
        }
    }

    public static void CloseMsg()
    {
        if (null != mInst)
        {
            mInst.OnClose();
        }
    }

    public static void HideMsg()
    {
        if (null != mInst)
        {
            mInst.CloseViewAnim();
        }
    }

    public override void Release()
    {
        base.Release();
        mTextureDict.Clear();
        mInst = null;
    }

    public override void Update()
    {
        base.Update();
        if (isShow)
        {
            if (null != mLeftTex && null != mRightTex)
            {
                mTime += Time.deltaTime;
                if (mTime >= mTextureIndex * 0.125f)
                {//每秒8帧
                    mLeftTex.mainTexture = GetBgTextrue(mTextureIndex);
                    mRightTex.mainTexture = GetBgTextrue(mTextureIndex);
                    ++mTextureIndex;
                    if (mTextureIndex > mTextureIndexMax)
                    {
                        mTime = -0.125f;
                        mTextureIndex = 0;
                    }
                }
            }
            mConnectingTime += Time.deltaTime;
            if (mConnectingTime >= 15.0f)
            {
                PlatformMgr.Instance.ConnenctCallBack(string.Empty);
                mConnectingTime = -1000;
            }
        }
    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        base.AddEvent();
        try
        {
            if (null != mTrans)
            {
                mContentTrans = mTrans.Find("content");
                if (null != mContentTrans)
                {
                    UISprite bg = GameHelper.FindChildComponent<UISprite>(mContentTrans, "bg");
                    if (null != bg)
                    {
                        bg.width = PublicFunction.GetWidth() + 4;
                        bg.height = PublicFunction.GetHeight() + 4;
                    }
                    mDeviceTweenAlpha = GameHelper.FindChildComponent<TweenAlpha>(mContentTrans, "device");
                    mBlueTweenAlpha = GameHelper.FindChildComponent<TweenAlpha>(mContentTrans, "blueSp");
                    mDeviceTweenScale = GameHelper.FindChildComponent<TweenScale>(mContentTrans, "device");
                    mBlueTweenScale = GameHelper.FindChildComponent<TweenScale>(mContentTrans, "blueSp");
                    UILabel label = GameHelper.FindChildComponent<UILabel>(mContentTrans, "device/Label");
                    if (null != label)
                    {
                        label.text = mBlueName;
                    }
                    UISprite phone = GameHelper.FindChildComponent<UISprite>(mContentTrans, "phoneSprite");
                    if (null != phone)
                    {
                        mPhoneTweenPosition = phone.gameObject.GetComponent<TweenPosition>();
                        mPhoneTweenScale = phone.gameObject.GetComponent<TweenScale>();
                        mPhoneSize = new Vector2(phone.width, phone.height);
                        mPhonePos = new Vector2(phone.transform.localPosition.x, phone.transform.localPosition.y);
                    }
                    mViewTweenPosition = mContentTrans.GetComponent<TweenPosition>();
                    OpenViewAnim();

                    Transform backbtn = mContentTrans.Find("backbtn");
                    if (null != backbtn)
                    {
                        backbtn.localPosition = UIManager.GetWinPos(backbtn, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
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

    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        try
        {
            string name = obj.name;
            if (name.Equals("backbtn"))
            {
                PlatformMgr.Instance.CannelConnectBluetooth();
                CloseViewAnim();
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




    //////////////////////////////////////////////////////////////////////////
    void OpenViewAnim()
    {
        mConnectingTime = 0;
        if (null != mPhoneTweenPosition)
        {
            mPhoneTweenPosition.transform.localPosition = Vector2.zero;
            GameHelper.PlayTweenPosition(mPhoneTweenPosition, mPhonePos);
            mPhoneTweenPosition.SetOnFinished(OpenViewAnimFinished);
        }
        if (null != mPhoneTweenScale)
        {
            mPhoneTweenScale.transform.localScale = new Vector3(mPhoneStartSize.x / mPhoneSize.x, mPhoneStartSize.y / mPhoneSize.y);
            GameHelper.PlayTweenScale(mPhoneTweenScale, Vector3.one);
        }
        if (null != mBlueTweenAlpha)
        {
            mBlueTweenAlpha.value = 0;
            GameHelper.PlayTweenAlpha(mBlueTweenAlpha, 1);
        }
        if (null != mDeviceTweenAlpha)
        {
            mDeviceTweenAlpha.value = 0;
            GameHelper.PlayTweenAlpha(mDeviceTweenAlpha, 1);
        }
        if (null != mDeviceTweenScale)
        {
            mDeviceTweenScale.transform.localScale = Vector3.zero;
            GameHelper.PlayTweenScale(mDeviceTweenScale, Vector3.one);
        }
        if (null != mBlueTweenScale)
        {
            mBlueTweenScale.transform.localScale = Vector3.zero;
            GameHelper.PlayTweenScale(mBlueTweenScale, Vector3.one);
        }
        /*if (null != mContentTrans)
        {
            mContentTrans.localPosition = new Vector3(PublicFunction.GetWidth(), 0);
        }
        mViewTweenPosition.SetOnFinished(OpenViewAnimFinished);
        GameHelper.PlayTweenPosition(mViewTweenPosition, Vector3.zero);*/
    }

    void CloseViewAnim()
    {
        CloseViewAnimFinished();
        /*mViewTweenPosition.SetOnFinished(CloseViewAnimFinished);
        GameHelper.PlayTweenPosition(mViewTweenPosition, new Vector3(PublicFunction.GetWidth(), 0));*/
    }

    void OpenViewAnimFinished()
    {
        mTextureIndex = 0;
        mTime = 0;
        mLeftTex = GameHelper.FindChildComponent<UITexture>(mContentTrans, "leftTexture");
        if (null != mLeftTex)
        {
            mLeftTex.gameObject.SetActive(true);
            mLeftTex.mainTexture = GetBgTextrue(mTextureIndex);
        }
        mRightTex = GameHelper.FindChildComponent<UITexture>(mContentTrans, "rightTexture");
        if (null != mRightTex)
        {
            mRightTex.gameObject.SetActive(true);
            mRightTex.mainTexture = GetBgTextrue(mTextureIndex);
        }
    }

    void CloseViewAnimFinished()
    {
        OnHide();
        //OnClose();
        if (null != mLeftTex)
        {
            mLeftTex.gameObject.SetActive(false);
        }
        if (null != mRightTex)
        {
            mRightTex.gameObject.SetActive(false);
        }
        SearchBluetoothMsg.ShowMsg();
    }

    Texture GetBgTextrue(int index)
    {
        string name = mBgTexFont + index;
        if (mTextureDict.ContainsKey(name))
        {
            return mTextureDict[name];
        }
        Texture tex = ResourcesEx.Load<Texture>("Texture/BlueConnecting/" + name);
        if (null != tex)
        {
            mTextureDict[name] = tex;
            return tex;
        }
        return null;
    }
    #endregion
}