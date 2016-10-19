using UnityEngine;
using System.Collections;

public class PressForGuide : MonoBehaviour {

    void Start()
    { }
    bool flag = false;

    void OnPress(bool pressed)
    {
        Debug.Log("i'm pressed");
        if (StepManager.GetIns().OpenOrCloseGuide && !flag)
        {
            flag = true;
            GuideViewBase.Ins.StepSuccess();
        }
    }
}
