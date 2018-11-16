using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:PopReadAngleErrorMsg.cs
/// Description:
/// Time:2016/1/20 10:12:58
/// </summary>
public class PopReadAngleErrorMsg : BasePopWin
{
    #region 公有属性
    #endregion

    #region 其他属性
    GameObject mDuoJiPrefab = null;//舵机的预设
    Transform mGridTrans = null;

    UIScrollView mUIScrollView = null;
    UIPanel mScrollViewPanel = null;
    UIGrid mUIGrid = null;
    UISprite mBgSprite = null;

    UILabel mTitleLabel = null;

    Dictionary<byte, short> mRotaDict = null;
    Dictionary<string, ActionEditUI.DuoJi> mDuoJiDataDict = null;
    ActionEditUI.DuoJi mSelectDuoJi = null;

    Transform mLeftBtnTrans = null;
    Transform mRightBtnTrans = null;
    UIButton mRightBtn = null;


    int DuoJi_Width_Space = 380;
    float mLastSendDjRota = 0;
    Robot mRobot;

    AnimationCurve mAnimationCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(3, 0.3f, 0, 0), new Keyframe(5, 1, 0, 0));
    float mStartTime = 0;
    int mSpeedFactor = 1;//速度因子:按住速度变化的因子数
    int maxFactor = 30;//每秒最大增加的角度

    ButtonDelegate.OnClick mOnBtnClick;

    UIScrollBar mScrollBar;
    #endregion

    #region 公有函数
    public PopReadAngleErrorMsg(Dictionary<byte, short> rotas, ButtonDelegate.OnClick onClick)
    {
        mUIResPath = "Prefab/UI/readbackAngleMsg";
        mDuoJiDataDict = new Dictionary<string, ActionEditUI.DuoJi>();
        isSingle = true;
        mRotaDict = rotas;
        mRobot = RobotManager.GetInst().GetCurrentRobot();
        mOnBtnClick = onClick;
    }

    public static void ShowReadAngleErrorMsg(Dictionary<byte, short> rotas, ButtonDelegate.OnClick onClick)
    {
        object[] args = new object[2];
        args[0] = rotas;
        args[1] = onClick;
        PopWinManager.GetInst().ShowPopWin(typeof(PopReadAngleErrorMsg), args);
    }

    public override void Init()
    {
        base.Init();
    }
    #endregion

    #region 其他函数
    protected override void AddEvent()
    {
        try
        {
            base.AddEvent();
            if (null != mTrans)
            {
                mTitleLabel = GameHelper.FindChildComponent<UILabel>(mTrans, "title/Label");
                if (null != mTitleLabel)
                {
                    mTitleLabel.text = string.Format(LauguageTool.GetIns().GetText("舵机角度死区"), PublicFunction.DuoJi_Min_Show_Rota - PublicFunction.DuoJi_Start_Rota, PublicFunction.DuoJi_Max_Show_Rota - PublicFunction.DuoJi_Start_Rota);
                }
                Transform duoji = mTrans.Find("duoji");
                if (null != duoji)
                {
                    mDuoJiPrefab = duoji.gameObject;
                }

                Transform bg = mTrans.Find("bg");
                if (null != bg)
                {
                    mBgSprite = bg.GetComponent<UISprite>();
                }

                Transform dj = mTrans.Find("dj");
                if (null != dj)
                {
                    mScrollViewPanel = GameHelper.FindChildComponent<UIPanel>(dj, "djlist");
                    if (null != mScrollViewPanel)
                    {
                        mScrollViewPanel.depth = this.mDepth + 2;
                    }
                    mUIScrollView = GameHelper.FindChildComponent<UIScrollView>(dj, "djlist");
                    mGridTrans = dj.Find("djlist/grid");
                    if (null != mGridTrans)
                    {
                        mUIGrid = mGridTrans.GetComponent<UIGrid>();
                        if (null != mUIGrid)
                        {
                            mUIGrid.cellWidth = DuoJi_Width_Space;
                        }
                    }
                }

                Transform btn = mTrans.Find("btn");
                if (null != btn)
                {
                    mLeftBtnTrans = btn.Find("leftBtn");
                    if (null != mLeftBtnTrans)
                    {
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(mLeftBtnTrans, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("取消");
                        }
                    }
                    mRightBtnTrans = btn.Find("rightBtn");
                    if (null != mRightBtnTrans)
                    {
                        mRightBtn = mRightBtnTrans.GetComponent<UIButton>();
                        if (null != mRightBtn)
                        {
                            mRightBtn.OnSleep();
                        }
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(mRightBtnTrans, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("确定");
                        }
                    }
                }

                mScrollBar = GameHelper.FindChildComponent<UIScrollBar>(mTrans, "framescrollbar");

                InitLayout();
                InitDuoJi();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    protected override void OnButtonClick(GameObject obj)
    {
        try
        {
            base.OnButtonClick(obj);
            string name = obj.name;
            if (name.Equals("leftBtn"))
            {//取消
                if (null != mOnBtnClick)
                {
                    mOnBtnClick(obj);
                }
                OnClose();
            }
            else if (name.Equals("rightBtn"))
            {//确定
                if (null != mOnBtnClick)
                {
                    mOnBtnClick(obj);
                }
                OnClose();
            }
            else if (name.StartsWith("dj_"))
            {//舵机
                if (null != mDuoJiDataDict && mDuoJiDataDict.ContainsKey(name))
                {
                    SetSelectDuoJi(mDuoJiDataDict[name]);
                }
                else
                {
                    SetSelectDuoJi(null);
                }
            }
            else if (name.Equals("subBtn"))
            {
                ChangeDuoJiAngle(-1, obj.transform.parent.parent);
            }
            else if (name.Equals("addBtn"))
            {
                ChangeDuoJiAngle(1, obj.transform.parent.parent);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    void UpdateSpeedFactor()
    {
        float cur = mAnimationCurve.Evaluate(Time.time - mStartTime);
        mSpeedFactor = (int)(cur * maxFactor);
        mSpeedFactor = (mSpeedFactor < 1 ? 1 : mSpeedFactor);
    }
    protected override void OnButtonPress(GameObject obj, bool press)
    {
        base.OnButtonPress(obj, press);
        try
        {
            if (press)
            {
                mStartTime = Time.time;
            }
            else
            {
                mStartTime = 0;
                mSpeedFactor = 1;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    protected override void OnDurationPress(GameObject obj)
    {
        base.OnDurationPress(obj);
        
        string name = obj.name;
        if (name.Equals("subBtn"))
        {
            UpdateSpeedFactor();
            ChangeDuoJiAngle(-mSpeedFactor, obj.transform.parent.parent);
        }
        else if (name.Equals("addBtn"))
        {
            UpdateSpeedFactor();
            ChangeDuoJiAngle(mSpeedFactor, obj.transform.parent.parent);
        }
    }

    void SetSelectDuoJi(ActionEditUI.DuoJi selectDj)
    {
        if (selectDj != mSelectDuoJi)
        {
            if (null != mSelectDuoJi)
            {
                //上一次选中的舵机处于显示状态则关闭调节
                mSelectDuoJi.SetAdjustActive(false);
            }
            mSelectDuoJi = selectDj;
            if (null != mSelectDuoJi)
            {
                mSelectDuoJi.SetAdjustActive(true);
            }
        }
    }

    private void InitDuoJi()
    {
        if (null != mRotaDict)
        {
            foreach (KeyValuePair<byte, short> kvp in mRotaDict)
            {
                GameObject obj = GameObject.Instantiate(mDuoJiPrefab) as GameObject;
                if (null != obj)
                {
                    string name = "dj_" + kvp.Key;
                    obj.name = name;
                    obj.SetActive(true);
                    ActionEditUI.DuoJi data = new ActionEditUI.DuoJi(obj.transform, kvp.Key);
                    
                    data.onDragDj = DragSetDuoJiAngle;
                    mDuoJiDataDict[name] = data;
                    data.SetErrorAngle(kvp.Value);
                    obj.transform.localPosition = Vector2.zero;
                    obj.transform.parent = mGridTrans;
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localEulerAngles = Vector2.zero;
                    if (null == mSelectDuoJi)
                    {
                        mSelectDuoJi = data;
                        mSelectDuoJi.SetAdjustActive(true);
                    }
                }
            }
            UIManager.SetButtonEventDelegate(mGridTrans, this.mBtnDelegate);
            if (null != mUIGrid)
            {
                mUIGrid.repositionNow = true;
            }
        }
    }

    void InitLayout()
    {
        int count = 0;
        if (null != mRotaDict)
        {
            count = mRotaDict.Count;
        }
        if (count <= 1)
        {
            return;
        }
        int addWidth = (count - 1) * DuoJi_Width_Space;
        if (addWidth > 600)
        {
            addWidth = 600;
        }
        if (null != mTitleLabel)
        {
            mTitleLabel.width += addWidth;
        }
        if (null != mBgSprite)
        {
            mBgSprite.width += addWidth;
        }
        int rectWidth = 0;
        if (null != mScrollViewPanel)
        {
            Vector4 rect = mScrollViewPanel.finalClipRegion;
            rect.z += addWidth;
            mScrollViewPanel.baseClipRegion = rect;
            rectWidth = (int)rect.z;
        }
        int allWidth = DuoJi_Width_Space * count;
        if (allWidth > rectWidth)
        {
            if (null != mUIScrollView)
            {
                if (null != mScrollBar)
                {
                    mScrollBar.gameObject.SetActive(true);
                    mUIScrollView.horizontalScrollBar = mScrollBar;
                }
                mUIScrollView.MoveAbsolute(new Vector3((allWidth - rectWidth) / 2, 0));
            }
        }
        if (null != mLeftBtnTrans)
        {
            mLeftBtnTrans.localPosition -= new Vector3(addWidth / 2, 0);
        }
        if (null != mRightBtnTrans)
        {
            mRightBtnTrans.localPosition += new Vector3(addWidth / 2, 0);
        }
        /*UISprite line = GameHelper.FindChildComponent<UISprite>(mTrans, "btn/line");
        if (null != line)
        {
            line.width += addWidth;
        }*/
        UISprite barBackground = GameHelper.FindChildComponent<UISprite>(mTrans, "framescrollbar/Background");
        if (null != barBackground)
        {
            barBackground.width += addWidth;
        }
        UISprite barForeground = GameHelper.FindChildComponent<UISprite>(mTrans, "framescrollbar/Foreground");
        if (null != barForeground)
        {
            barForeground.width += addWidth;
        }
    }
    void ChangeDuoJiAngle(int rota, Transform dj)
    {
        if (null != dj)
        {
            ActionEditUI.DuoJi djdata = mDuoJiDataDict[dj.name];
            if (djdata.Angle <= PublicFunction.DuoJi_Min_Rota && rota < 0 || djdata.Angle >= PublicFunction.DuoJi_Max_Rota && rota > 0)
            {
                return;
            }
            djdata.SetAngle(djdata.Angle + rota);
            if (null != mRobot)
            {
                mRobot.GetAllDjData().UpdateData(djdata.Id, (short)djdata.Angle);
                SendRota(djdata.Id, djdata.Angle, true);
            }
            ChangeRightBtnState();
        }
    }
    void DragSetDuoJiAngle(ActionEditUI.DuoJi data, bool finished)
    {
        if (null != mRobot)
        {
            mRobot.GetAllDjData().UpdateData((byte)data.Id, (short)data.Angle);
            SendRota(data.Id, data.Angle, finished);
        }
        ChangeRightBtnState();
    }

    void SendRota(int id, int rota, bool finished)
    {

        if (mRobot.Connected)
        {
            if (finished)
            {
                mLastSendDjRota = 0;
                mRobot.CtrlActionForDjId(id, rota);
            }
            else if (Time.time - mLastSendDjRota >= 0.1f)
            {
                mRobot.CtrlActionForDjId(id, rota);
                mLastSendDjRota = Time.time;
            }
        }
        EventMgr.Inst.Fire(EventID.Adjust_Angle_For_UI, new EventArg(id, rota));
    }

    void ChangeRightBtnState()
    {
        if (null == mDuoJiDataDict)
        {
            if (null != mRightBtn)
            {
                mRightBtn.OnAwake();
            }
            return;
        }
        foreach (KeyValuePair<string, ActionEditUI.DuoJi> kvp in mDuoJiDataDict)
        {
            if (!PublicFunction.IsShowNormalRota(kvp.Value.Angle))
            {
                return;
            }
        }
        if (null != mRightBtn)
        {
            mRightBtn.OnAwake();
        }
    }
    #endregion
}