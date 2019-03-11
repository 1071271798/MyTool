using UnityEngine;
public class CommonUtils
{
    /// <summary>
    /// 保留几位小数
    /// </summary>
    /// <param name="num"></param>
    /// <param name="keepNum"></param>
    /// <returns></returns>
    public static float KeepDecimal(float num, int keepNum)
    {
        int powNum = (int)Mathf.Pow(10, keepNum);
        return (int)(num * powNum) / (powNum + 0.0f);
    }
    /// <summary>
    /// 求两个角度的平局值
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float AngleAve(float a, float b)
    {
        float tmp = (a + b) / 2;
        if ((a > b ? a - b : b - a) > 180)
        {
            if (tmp >= 180) tmp -= 180;
            else tmp += 180;
        }
        return tmp;
    }
}