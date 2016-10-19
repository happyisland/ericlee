using UnityEngine;
using System.Collections;

public class NormalTools : MonoBehaviour {

}

/// <summary>
/// 计算工具
/// </summary>
public static class MathTool
{
    /// <summary>
    /// 将string转换成float
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static float StrToFloat(string val)
    {
        float res=0;
        if(val!=null&&val!="")
        {
            res = float.Parse(val);
        }
        return res;
    }

    /// <summary>
    /// 返回运算结果
    /// </summary>
    /// <param name="a"></param>
    /// <param name="sym"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float MathResult(float a, string sym, float b)
    {
        float res = 0;
        switch (sym)
        {
            case "+":
                res = a + b;
                break;
            case "-":
                res = a - b;
                break;
            case "*":
                res = a * b;
                break;
            case "/":
                res = a / b;
                break;
        }

        return res;
    }
}