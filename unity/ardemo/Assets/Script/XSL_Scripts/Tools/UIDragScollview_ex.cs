using UnityEngine;
using System.Collections;
using Game.Event;

public class UIDragScollview_ex : UIDragScrollView
{
    bool _isSetting;
    bool IsSetting
    {
        set
        {
            _isSetting = value;
            enabled = value;
        }
    }

    void Start()
    {
        IsSetting = true;
        EventMgr.Inst.Regist(EventID.SwitchEdit, OnSettingCallback);
    }

    void OnDestroy()
    {
        EventMgr.Inst.UnRegist(EventID.SwitchEdit, OnSettingCallback);
    }

    void OnSettingCallback(EventArg arg)
    {
        IsSetting = (bool)arg[0];
    }
}
