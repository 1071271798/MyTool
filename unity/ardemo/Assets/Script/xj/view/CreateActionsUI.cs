using System;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Game.Event;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:CreateActionsUI.cs
/// Description:
/// Time:2015/10/20 16:08:54
/// </summary>
public class CreateActionsUI : BasePopWin
{
    #region 公有属性
    public delegate void OnCloseDelegate();
    public enum ActionsMsgType
    {
        Actions_Msg_Create,
        Actions_Msg_Save,
        Actions_Msg_SaveAs,
    }
    #endregion

    #region 其他属性
    UILabel mTitleText;
    //UISprite mIconSprite;
    UIInput mNameInput;
    UIButton mBtnOk;
    UIButton mBtnSaveAs;
    GameObject mSelectIcon;
    GameObject mItem;
    Transform mGridTrans;
    UIGrid mGrid;

    ActionsMsgType mMsgType;
    string mRobotId;
    ActionSequence mActions;
    OnCloseDelegate onClose;
    bool isOfficial;

    UISprite mInputBg;

    TweenPosition mBtnBackTweenPosition;
    TweenPosition mBtnOkTweenPosition;

    TweenAlpha mUITweenAlpha;
    TweenAlpha mIconListTweenAlpha;

    bool mSaveClickFlag = false;
    #endregion

    #region 公有函数
    public CreateActionsUI(ActionsMsgType type, string robotId, ActionSequence actions, OnCloseDelegate close)
    {
        mUIResPath = "Prefab/UI/CreateAction";
        onClose = close;
        mMsgType = type;
        mRobotId = robotId;
        mActions = actions;
        isSingle = true;
        if (null != mActions && mActions.IsOfficial())
        {//官方动作，不能编辑
            isOfficial = true;
        }
        else
        {
            isOfficial = false;
        }
    }


    public static void ShowMsg(ActionsMsgType type, string robotId, ActionSequence actions, OnCloseDelegate close = null)
    {
        object[] args = new object[4];
        args[0] = type;
        args[1] = robotId;
        args[2] = actions;
        args[3] = close;
        PopWinManager.Inst.ShowPopWin(typeof(CreateActionsUI), args);
    }
    #endregion

    #region 其他函数
    public override void Init()
    {
        base.Init();
    }
    protected override void AddEvent()
    {
        try
        {
            base.AddEvent();
            if (null == mTrans)
            {
                return;
            }
            mUITweenAlpha = mTrans.GetComponent<TweenAlpha>();
            Transform title = mTrans.Find("title");
            if (null != title)
            {
                title.localPosition = UIManager.GetWinPos(title, UIWidget.Pivot.Top, 0, 65);
                mTitleText = GameHelper.FindChildComponent<UILabel>(title, "Label");
                if (null != mTitleText)
                {
                    if (mMsgType == ActionsMsgType.Actions_Msg_Create)
                    {
                        mTitleText.text = LauguageTool.GetIns().GetText("新建动作");
                    }
                    else if (mMsgType == ActionsMsgType.Actions_Msg_SaveAs)
                    {
                        mTitleText.text = LauguageTool.GetIns().GetText("另存为");
                    }
                    else
                    {
                        mTitleText.text = LauguageTool.GetIns().GetText("保存动作");
                    }
                }
            }

            Transform bg = mTrans.Find("bg");
            if (null != bg)
            {
                UISprite bgSp = bg.GetComponent<UISprite>();
                if (null != bgSp)
                {
                    bgSp.width = PublicFunction.GetExtendWidth();
                    bgSp.height = PublicFunction.GetExtendHeight();
                }
            }

            Transform btnCancel = mTrans.Find("BtnCancel");
            if (null != btnCancel)
            {
                mBtnBackTweenPosition = btnCancel.GetComponent<TweenPosition>();
                Vector3 pos = UIManager.GetWinPos(btnCancel, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y);
                //Vector3 canelPos = btnCancel.localPosition;
                btnCancel.localPosition = pos - new Vector3(300, 0, 0);
                GameHelper.PlayTweenPosition(mBtnBackTweenPosition, pos, 0.6f);
                //btnCancel.localPosition = canelPos;
            }
            /*Transform btnsaveas = mTrans.Find("btnsaveas");
            if (null != btnsaveas)
            {
                mBtnSaveAs = btnsaveas.GetComponent<UIButton>();
                if (null != mBtnSaveAs)
                {
                    mBtnSaveAs.OnSleep();
                }
                Vector3 saveaspos = btnsaveas.localPosition;
                saveaspos.x = UIManager.GetWinPos(btnsaveas, UIWidget.Pivot.Right, 45).x;
                btnsaveas.localPosition = saveaspos;
            }*/
            
            Transform action = mTrans.Find("action");
            if (null != action)
            {
                action.localPosition = new Vector3(0, (0.5f - 0.267f) * PublicFunction.GetHeight());

                /*mBtnOk = GameHelper.FindChildComponent<UIButton>(action, "BtnOk");
                UILabel btnLb = GameHelper.FindChildComponent<UILabel>(action, "BtnOk/Label");
                if (null != btnLb)
                {
                    btnLb.text = LauguageTool.GetIns().GetText("确定");
                }*/
                mNameInput = GameHelper.FindChildComponent<UIInput>(action, "Input");
                if (null != mNameInput)
                {
                    mInputBg = GameHelper.FindChildComponent<UISprite>(mNameInput.transform, "Background");
                    mNameInput.onSelect = OnInputSelect;
                    mNameInput.onValidate = OnValidate;
                    mNameInput.defaultText = LauguageTool.GetIns().GetText("请输入动作名");
                    if (null == mActions)
                    {
                        //mNameInput.value = mNameInput.defaultText;
                    }
                    else
                    {
                        if (LauguageTool.IsArab(mActions.Name))
                        {
                            mNameInput.value = LauguageTool.ConvertArab(mActions.Name);
                        }
                        else
                        {
                            mNameInput.value = mActions.Name;
                        }
                        
                    }
                }

                
                //mIconSprite = GameHelper.FindChildComponent<UISprite>(action, "icon/icon");
            }
            Transform tips = mTrans.Find("tips");
            if (null != tips)
            {
                tips.localPosition = new Vector3(0, (0.5f - 0.41f) * PublicFunction.GetHeight());
                UILabel promptlb = GameHelper.FindChildComponent<UILabel>(tips, "Label");
                if (null != promptlb)
                {
                    promptlb.text = LauguageTool.GetIns().GetText("为您的动作选择一个图标");
                }

            }
            Transform item = mTrans.Find("item");
            if (null != item)
            {
                mItem = item.gameObject;
            }
            Transform iconList = mTrans.Find("iconList");
            if (null != iconList)
            {
                mIconListTweenAlpha = iconList.GetComponent<TweenAlpha>();
                UIPanel panel = iconList.GetComponent<UIPanel>();
                /*if (null != panel)
                {
                    */panel.depth = mDepth + 1;
                    Vector4 rect = panel.finalClipRegion;
                    rect.z = PublicFunction.GetWidth() - PublicFunction.Back_Btn_Pos.x * 2;
                    
                //}
                //Vector3 pos = iconList.localPosition;
                //iconList.localPosition = new Vector3(pos.x, UIManager.GetWinPos(iconList, UIWidget.Pivot.Bottom, 0, 110).y, pos.z);
                mGridTrans = iconList.Find("grid");
                if (null != mGridTrans)
                {
                    int cellWidth = 200;
                    UIGrid grid = mGridTrans.GetComponent<UIGrid>();
                    if (null != grid)
                    {
                        cellWidth = (int)grid.cellWidth;
                    }
                    Vector3 pos = mGridTrans.localPosition;
                    //pos.y = -PublicFunction.GetHeight() / 2 + 260;
                    pos.x = -PublicFunction.GetWidth() / 2 + cellWidth / 2;
                    rect.y = pos.y;
                    panel.baseClipRegion = rect;
                    mGridTrans.localPosition = new Vector3(pos.x, (0.5f - 0.6f) * PublicFunction.GetHeight()); ;
                    mGrid = mGridTrans.GetComponent<UIGrid>();
                }

                Transform drag = mTrans.Find("drag");
                if (null != drag)
                {
                    UISprite sp = drag.GetComponent<UISprite>();
                    if (null != sp)
                    {
                        sp.width = PublicFunction.GetWidth();
                    }
                }
            }

            Transform btnOk = mTrans.Find("BtnOk");
            if (null != btnOk)
            {
                mBtnOkTweenPosition = btnOk.GetComponent<TweenPosition>();
                Vector3 pos = UIManager.GetWinPos(btnOk, UIWidget.Pivot.Bottom, 0, 60);
                btnOk.localPosition = pos - new Vector3(0, 300);
                GameHelper.PlayTweenPosition(mBtnOkTweenPosition, pos, 0.6f);
                mBtnOk = btnOk.GetComponent<UIButton>();
                /*BoxCollider box = btnOk.GetComponent<BoxCollider>();
                if (null != box)
                {
                    Vector2 boxSize = box.size;
                    boxSize.x = PublicFunction.GetExtendWidth() + 4;
                    box.size = boxSize;
                    UISprite btnSprite = GameHelper.FindChildComponent<UISprite>(btnOk, "Background");
                    if (null != btnSprite)
                    {
                        btnSprite.width = (int)boxSize.x;
                    }
                }*/
                UILabel btnLb = GameHelper.FindChildComponent<UILabel>(btnOk, "Label");
                if (null != btnLb)
                {
                    btnLb.text = LauguageTool.GetIns().GetText("确定");
                }
            }

            string iconName = InitIconList();
            if (ActionsMsgType.Actions_Msg_SaveAs != mMsgType && null != mActions && !string.IsNullOrEmpty(mActions.IconID))
            {
                iconName = mActions.IconName;
            }
            ChangeBtnState();
            //SetNowIcon(iconName);
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
            if (name.Equals("BtnOk") || name.Equals("btnsaveas"))
            {
                mSaveClickFlag = true;
                bool saveAsFlag = name.Equals("btnsaveas");
                if (ActionsMsgType.Actions_Msg_SaveAs == mMsgType)
                {
                    saveAsFlag = true;
                }
                string actName = string.Empty;
                string actId = string.Empty;
                if (null != mNameInput)
                {
                    if (LauguageTool.IsArab(mNameInput.value))
                    {
                        actName = LauguageTool.ConvertArab(mNameInput.value);
                    }
                    else
                    {
                        actName = mNameInput.value;
                    }
                }

                if (!saveAsFlag && null != mActions)
                {//另存为会新建id
                    actId = mActions.Id;
                }
                ErrorCode ret = ActionsManager.GetInst().CheckActionsName(mRobotId, actName, actId);
                if (ErrorCode.Result_OK == ret)
                {
                    string iconName = string.Empty;
                    if (null != mSelectIcon)
                    {
                        iconName = mSelectIcon.name;
                    }
                    switch (mMsgType)
                    {
                        case ActionsMsgType.Actions_Msg_Create:
                            if (!saveAsFlag)
                            {
                                ActionEditScene.CreateActions(actName, iconName);
                                //EventMgr.Inst.Fire(EventID.UI_Create_Actions, new EventArg(actName, iconName));
                            }
                            OpenCloseAnim();
                            break;
                        case ActionsMsgType.Actions_Msg_Save:
                        case ActionsMsgType.Actions_Msg_SaveAs:
                            if (null != mActions && !actName.Equals(mActions.Name) && !iconName.Equals(mActions.IconID))
                            {
                                saveAsFlag = true;
                            }
                            if (isOfficial && !saveAsFlag)
                            {
                                PromptMsg.ShowSinglePrompt(LauguageTool.GetIns().GetText("官方动作不能修改"));
                            }
                            else
                            {                              
                                EventMgr.Inst.Fire(EventID.UI_Save_Actions, new EventArg(actName, iconName, saveAsFlag));
                                OpenCloseAnim();
                            }
                            
                            break;
                    }
                    
                }
                else if (ErrorCode.Result_Name_Empty == ret)
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("名字为空"), 1, CommonTipsColor.red);
                }
                else if (ErrorCode.Result_Name_Exist == ret)
                {
                    SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("重复名字"), 1, CommonTipsColor.red);
                }
            }
            else if (name.Equals("BtnCancel"))
            {
                OpenCloseAnim();
            }
            else if (name.StartsWith("icon_"))
            {
                SetSelectIcon(obj, true);
                //SetNowIcon(ActionsManager.GetInst().GetActionIconName(name));
                ChangeBtnState();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    protected override void Close()
    {
        try
        {
            base.Close();
            if (mSaveClickFlag && null != onClose)
            {
                onClose();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void SetLabelColor(UIButton btn, float a)
    {
        UILabel lb = GameHelper.FindChildComponent<UILabel>(btn.transform, "Label");
        if (null != lb)
        {
            lb.alpha = a;
        }
    }
    void ChangeBtnState()
    {
        do 
        {
            if (null == mSelectIcon || null == mNameInput)
            {
                if (null != mBtnOk)
                {
                    mBtnOk.OnSleep();
                    //SetLabelColor(mBtnOk, 0.5f);
                }
                if (null != mBtnSaveAs)
                {
                    mBtnSaveAs.OnSleep();
                }
                break;
            }
            /*if (!mIconSprite.gameObject.activeSelf)
            {
                if (null != mBtnOk)
                {
                    mBtnOk.OnSleep();
                }
                if (null != mBtnSaveAs)
                {
                    mBtnSaveAs.OnSleep();
                }
                break;
            }*/
            string actName = string.Empty;
            if (LauguageTool.IsArab(mNameInput.value))
            {
                actName = LauguageTool.ConvertArab(mNameInput.value);
            }
            else
            {
                actName = mNameInput.value;
            }
            if (string.IsNullOrEmpty(actName) || actName.Equals(mNameInput.defaultText))
            {
                if (null != mBtnOk)
                {
                    mBtnOk.OnSleep();
                    //SetLabelColor(mBtnOk, 0.5f);
                }
                if (null != mBtnSaveAs)
                {
                    mBtnSaveAs.OnSleep();
                }
                break;
            }
            if (null != mBtnOk)
            {
                if (mMsgType == ActionsMsgType.Actions_Msg_SaveAs)
                {
                    if (null != mActions && !mActions.IconID.Equals(mSelectIcon.name) && !actName.Equals(mActions.Name))
                    {//动作存在，且改变了图片和名字
                        mBtnOk.OnAwake();
                        //SetLabelColor(mBtnOk, 1);
                    }
                    else
                    {
                        mBtnOk.OnSleep();
                        //SetLabelColor(mBtnOk, 0.5f);
                    }
                }
                else
                {
                    mBtnOk.OnAwake();
                    //SetLabelColor(mBtnOk, 1);
                    /*if (StepManager.GetIns().OpenOrCloseGuide)
                    {
                        EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().InputNameStep, true));
                    }*/
                }
            }
            
            /*if (null != mBtnOk)
            {
                if (isOfficial)
                {
                    mBtnOk.OnSleep();
                }
                else
                {
                    mBtnOk.OnAwake();
                    if (StepManager.GetIns().OpenOrCloseGuide)
                    {
                        EventMgr.Inst.Fire(EventID.GuideNeedWait, new EventArg(StepManager.GetIns().InputNameStep, true));
                    }
                }
                
            }
            if (null != mBtnSaveAs && mMsgType == ActionsMsgType.Actions_Msg_Save)
            {
                if (null != mActions && !mActions.IconID.Equals(mSelectIcon.name) && !mNameInput.value.Equals(mActions.Name))
                {//动作存在，且改变了图片和名字
                    mBtnSaveAs.OnAwake();
                }
                else
                {
                    mBtnSaveAs.OnSleep();
                }
            }*///修改另存为逻辑时屏蔽
        } while (false);
        
    }

    void OnInputSelect(bool isSelect, GameObject obj)
    {
        try
        {
            if (!isSelect)
            {
                ChangeBtnState();
                /*if (null != mInputBg)
                {
                    mInputBg.color = Color.white;
                }*/
            }
            else
            {
                /*if (null != mInputBg)
                {
                    mInputBg.color = new Color32(57, 197, 233, 255);
                }*/
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    byte tmpLenght = 0;
    char OnValidate(string text, int charIndex, char addedChar)
    {
        if (charIndex == 0)
        {
            tmpLenght = 0;
        }
        if (addedChar == '$' || addedChar == ';' || addedChar == '|' || addedChar == ',')
        {
            return (char)0;
        }
        if (tmpLenght >= PublicFunction.Action_Name_Lenght_Max)
        {//限制长度
            return (char)0;
        }
        if (Convert.ToInt32(addedChar) >= Convert.ToInt32(Convert.ToChar(128)))
        {//中文字符
            if (tmpLenght + 2 > PublicFunction.Action_Name_Lenght_Max)
            {
                return (char)0;
            }
            tmpLenght += 2;
        }
        else
        {
            ++tmpLenght;
        }        
        return addedChar;
    }


    string InitIconList()
    {
        string iconName = string.Empty;
        if (null != mGridTrans && null != mGrid && null != mItem)
        {
            bool findNewFlag = false;
            Dictionary<string, int> iconDict = ActionsManager.GetInst().GetRobotActionsIconIdDict(mRobotId);
            List<ActionIcon> actionIconList = ActionsManager.GetInst().GetActionIconList();
            int nowIndex = -1;
            for (int i = 0, imax = actionIconList.Count; i < imax; ++i)
            {
                GameObject obj = GameObject.Instantiate(mItem) as GameObject;
                if (null != obj)
                {
                    string name = actionIconList[i].id;
                    obj.SetActive(true);
                    obj.name = name;
                    bool grey = false;
                    if (!findNewFlag && (null == mActions || string.IsNullOrEmpty(mActions.IconID) || ActionsMsgType.Actions_Msg_SaveAs == mMsgType) && actionIconList[i].useCount > 0 && (!iconDict.ContainsKey(name) || iconDict[name] < actionIconList[i].useCount))
                    {//新建动作选择默认图标
                        findNewFlag = true;
                        SetSelectIcon(obj, true);
                        nowIndex = i;
                        iconName = actionIconList[i].iconName;
                    }
                    else if (mMsgType == ActionsMsgType.Actions_Msg_Save && null != mActions && mActions.IconID.Equals(actionIconList[i].id))
                    {//当前动作的图片
                        SetSelectIcon(obj, true);
                        nowIndex = i;
                    }
                    else if (actionIconList[i].useCount > 0 && iconDict.ContainsKey(name) && iconDict[name] >= actionIconList[i].useCount)
                    {
                        grey = true;
                    }
                    SetIcon(obj, actionIconList[i].iconName, grey);
                    
                    obj.transform.parent = mGridTrans;
                    obj.transform.localEulerAngles = Vector3.zero;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;
                }
            }
            
            mGrid.repositionNow = true;
            UIManager.SetButtonEventDelegate(mTrans, this.mBtnDelegate);

            if (-1 != nowIndex)
            {
                Timer.Add(0.1f, 1, 1, MoveSelectFont, nowIndex);
            }
        }
        return iconName;
    }

    void MoveSelectFont(params object[] args)
    {
        if (null != mGridTrans)
        {
            UIScrollView scrollView = mGridTrans.parent.GetComponent<UIScrollView>();
            if (null != scrollView)
            {
                int cellWidth = 200;
                UIGrid grid = mGridTrans.GetComponent<UIGrid>();
                if (null != grid)
                {
                    cellWidth = (int)grid.cellWidth;
                }
                scrollView.MoveRelative(new Vector3(-cellWidth * (int)args[0], 0));
                scrollView.RestrictWithinBounds(true);
            }
        }
        
    }



    void SetIcon(GameObject obj, string icon, bool grey)
    {
        UIButton btn = obj.GetComponent<UIButton>();
        if (null != btn)
        {
            if (grey)
            {
                btn.OnSleep();
            }
        }
        UISprite iconSp = GameHelper.FindChildComponent<UISprite>(obj.transform, "icon");
        if (null != iconSp)
        {
            iconSp.spriteName = icon;
            iconSp.width = 120;
            iconSp.height = 120;
            if (grey)
            {
                iconSp.alpha = 0.5f;
                //iconSp.OnGrey();
            }
            else
            {
                iconSp.alpha = 1;
            }
        }
    }

    void SetSelectIcon(GameObject obj, bool select)
    {
        if (null != mSelectIcon&&select)
        {
            SetSelectIcon(mSelectIcon, false);
        }
        if (select)
        {
            mSelectIcon = obj;
        }
        UISprite bg = GameHelper.FindChildComponent<UISprite>(obj.transform, "bg");
        UISprite shadow = GameHelper.FindChildComponent<UISprite>(obj.transform, "bg/shadow");
        UISprite icon = GameHelper.FindChildComponent<UISprite>(obj.transform, "icon");
        if (null != bg && null != shadow && null != icon)
        {
            if (select)
            {
                bg.width = 186;
                bg.height = 186;
                shadow.width = 186;
                shadow.height = 186;
                icon.width = 116;
                icon.height = 116;
            }
            else
            {
                bg.width = 160;
                bg.height = 160;
                shadow.width = 160;
                shadow.height = 160;
                icon.width = 100;
                icon.height = 100;
            }
            
        }
    }

    void SetNowIcon(string spriteName)
    {
        /*if (null != mIconSprite)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                mIconSprite.gameObject.SetActive(false);
            }
            else
            {
                if (!mIconSprite.gameObject.activeSelf)
                {
                    mIconSprite.gameObject.SetActive(true);
                }
                mIconSprite.spriteName = spriteName;
                mIconSprite.MakePixelPerfect();
            }
        }*/
        ChangeBtnState();
    }


    void OpenCloseAnim()
    {
        /*if (null != mBtnBackTweenPosition && null != mBtnOkTweenPosition)
        {
            GameHelper.PlayTweenPosition(mBtnBackTweenPosition, mBtnBackTweenPosition.transform.localPosition - new Vector3(300, 0), 0.6f);
            GameHelper.PlayTweenPosition(mBtnOkTweenPosition, mBtnOkTweenPosition.transform.localPosition - new Vector3(0, 150), 0.6f);
            GameHelper.PlayTweenAlpha(mUITweenAlpha, 0, 0.6f); 
            GameHelper.PlayTweenAlpha(mIconListTweenAlpha, 0, 0.6f);
            Timer.Add(0.6f, 1, 1, OnClose);

        }
        else*/
        {
            OnClose();
        }
        
    }
    #endregion
}