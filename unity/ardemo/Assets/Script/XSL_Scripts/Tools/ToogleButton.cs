using UnityEngine;
using System.Collections;

public class ToogleButton : MonoBehaviour {

    public GameObject Thumb;
    public TweenPosition tween;
    public UISprite sprite;
    public Vector3 from;
    public Vector3 to;
    private bool _isOn;
    private bool isOn
    {
        get
        {
            return _isOn;
        }
        set
        {
            _isOn = value;
            if (_isOn)
            {
                tween.from = from;
                tween.to = to;
                tween.PlayForward();
                sprite.spriteName = "btn_open";
            }
            else
            {
                tween.from = to;
                tween.to = from;
                tween.PlayForward();
                sprite.spriteName = "btn_chose";
            }
            Game.Event.EventMgr.Inst.Fire(Game.Event.EventID.SwitchEdit, new Game.Event.EventArg(isOn));
        }
    }

	// Use this for initialization
	void Start () {
        //if (tween != null)
        //{
        //    tween.AddOnFinished(TurnOn);
        //}
        Game.Event.EventMgr.Inst.Regist(Game.Event.EventID.SwitchToogle, SwitchResult);
	}

    void OnClick()
    {
        isOn = !isOn;
    }

    void SwitchResult(Game.Event.EventArg arg)
    {
        isOn = (bool)arg[0];
    }

    //public void ToogleSwitchBtn(GameObject go)
    //{
    //    if (isOn)  // open => close
    //    {
    //        tween.from = from;
    //        tween.to = to;
    //        tween.PlayForward();
    //    }
    //    else
    //    {
    //        tween.from = to;
    //        tween.to = from;
    //        tween.PlayForward();
    //    }
    //}

    //void TurnOn()
    //{
    //    isOn = !isOn;
    //    if (isOn)
    //        sprite.spriteName = "btn_open";
    //    else
    //        sprite.spriteName = "btn_chose";
    //    Game.Event.EventMgr.Inst.Fire(Game.Event.EventID.SwitchEdit, new Game.Event.EventArg(isOn));
    //}
}
