using UnityEngine;
using System.Collections;
/// <summary>
/// 检测轮模式改变频率是否太快  - 竖杆模式
/// </summary>
public class WheelCheck_hslider : MonoBehaviour
{

    public HSliderControl sliderControl;

    float leftT; //
    /// <summary>
    /// 检测改变的频率是否太快
    /// </summary>
    void FixedUpdate()
    {
        if (UserdefControllerUI.isSetting)
            return;
        if (sliderControl == null)
        {
            sliderControl = ControllerManager.GetInst().widgetManager.hSliderManager.GetHSliderByID(gameObject.name);
            if (sliderControl == null)
                return;
        }
        if (!sliderControl.isReady)
            return;
        if (sliderControl.changeFlag)
        {
            sliderControl.changeFlag = false;  //时间拦截
            sliderControl.changeOver = false;
            leftT = Time.time;
        }
        if (Time.time - leftT > 0.01f && !sliderControl.changeOver)
            sliderControl.WheelTurn();
    }
}
