
using Game.Platform;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;

public class GuideMsg : BasePopWin
{
    static GuideMsg sInst;

    GuideData mGuideData;
    bool mEditFlag;

    Transform mContentTrans;
    Transform mRobotTrans;
    Transform mDialogTrans;
    Transform mDialogBgTrans;
    GameObject mBtnCloseObj;
    GameObject mBtnClickObj;
    GameObject mBtnSkipObj;
    GameObject mSkipPanelObj;
    Vector3 mSkipPanelPos;

    UIWidget.Pivot mPivot;

    Dictionary<GameObject, List<GameObjectRestoreData>> mRestoreDict;

    float mSkipGuideTime;

    public GuideMsg(GuideData data, bool editFlag)
    {
        mUIResPath = "Prefab/UI/guideUI";
        mEditFlag = editFlag;
        mGuideData = data;
        sInst = this;
    }


    public static void ShowMsg(GuideData data, bool editFlag)
    {
        if (null == sInst)
        {
            object[] arg = new object[2];
            arg[0] = data;
            arg[1] = editFlag;
            SingletonObject<PopWinManager>.GetInst().ShowPopWin(typeof(GuideMsg), arg);
        } else
        {
            sInst.RefreshUI(data, true);
        }
    }

    public static void CloseMsg()
    {
        if (null != sInst)
        {
            sInst.OnClose();
        }
    }



    public override void Init()
    {
        base.Init();
        if (null != mGuideData)
        {
            if (!string.IsNullOrEmpty(mGuideData.clientSceneType))
            {
                mUICameraType = eUICameraType.OrthographicOne;
            } else if (mGuideData.unitySceneType == SceneType.Assemble)
            {
                GameObject obj = GameObject.Find("UIRootABL/Camera/Center");
                if (null != obj)
                {
                    mParentTrans = obj.transform;
                }
            }
        }
        
        this.mDepth = 999;
    }

    public override void Release()
    {
        base.Release();
        sInst = null;
    }

    protected override void Close()
    {
        base.Close();
        RestoreGameObjectData();
    }


    protected override void AddEvent()
    { 
        try
        {
            base.AddEvent();
            if (null != mTrans)
            {
                UIPanel panel = GameHelper.FindChildComponent<UIPanel>(mTrans, "panel");
                if (null != panel)
                {
                    panel.depth = mDepth + 2;
                }
                mContentTrans = mTrans.Find("panel/content");
                if (null != mContentTrans)
                {
                    mRobotTrans = mContentTrans.Find("robot");
                    mDialogTrans = mContentTrans.Find("dialog");
                    if (null != mDialogTrans)
                    {
                        mDialogBgTrans = mDialogTrans.Find("bg");
                    }
                }
                UISprite btnClickSprite = GameHelper.FindChildComponent<UISprite>(mTrans, "panel/btnClick");
                if (null != btnClickSprite)
                {
                    mBtnClickObj = btnClickSprite.gameObject;
                    btnClickSprite.width = PublicFunction.GetExtendWidth();
                    btnClickSprite.height = PublicFunction.GetExtendHeight();
                }
                Transform btnClose = mTrans.Find("panel/btnClose");
                if (null != btnClose)
                {
                    mBtnCloseObj = btnClose.gameObject;
                    GameHelper.PlayTweenPosition(btnClose, UIWidget.Pivot.TopLeft, PublicFunction.Back_Btn_Pos);
                }
                
                /*Transform btnSkip = mTrans.FindChild("panel/btnSkip");
                if (null != btnSkip)
                {
                    GameHelper.SetLabelText(btnSkip.FindChild("Label"), LauguageTool.GetIns().GetText("跳过"));
                    mBtnSkipObj = btnSkip.gameObject;
                    GameHelper.PlayTweenPosition(btnSkip, UIWidget.Pivot.TopRight, new Vector2(PublicFunction.GetWidth()*0.26f, PublicFunction.Back_Btn_Pos.y));
                }
                Transform skipPanel = mTrans.Find("panel/skipPanel");
                if (null != skipPanel)
                {
                    mSkipPanelObj = skipPanel.gameObject;
                    Transform panel1 = skipPanel.FindChild("panel");
                    if (null != panel1)
                    {
                        mSkipPanelPos = UIManager.GetWinPos(panel1, UIWidget.Pivot.Bottom);
                        Transform btnSkipAll = panel1.FindChild("btnSkipAll");
                        if (null != btnSkipAll)
                        {
                            UISprite bg = btnSkipAll.GetComponent<UISprite>();
                            if (null != bg)
                            {
                                bg.width = PublicFunction.GetExtendWidth();
                            }
                            GameHelper.SetLabelText(btnSkipAll.FindChild("Label"), LauguageTool.GetIns().GetText("我都会了，全部跳过"));
                        }
                        Transform btnCannel = panel1.FindChild("btnCannel");
                        if (null != btnCannel)
                        {
                            UISprite bg = btnCannel.GetComponent<UISprite>();
                            if (null != bg)
                            {
                                bg.width = PublicFunction.GetExtendWidth();
                            }
                            GameHelper.SetLabelText(btnCannel.FindChild("Label"), LauguageTool.GetIns().GetText("取消"));
                        }
                    }
                    mSkipPanelObj.SetActive(false);
                }*/
                Transform editmode = mTrans.Find("panel/editmode");
                if (null != editmode)
                {
                    editmode.gameObject.SetActive(mEditFlag);
                }
                if (mEditFlag)
                {
                    InitEditUI(editmode);
                }
                RefreshUI(mGuideData, true);
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
            if (name.Equals("btnClose"))
            {
                SingletonObject<GuideManager>.GetInst().GuideFinished();
                OnClose();
            } else if (name.Equals("btnSkip"))
            {
                ShowSkipChoiceUI();
                
            }
            else if (name.Equals("btnClick"))
            {
                GuideData data = SingletonObject<GuideManager>.GetInst().GetNextData();
                if (null != data)
                {
                    bool delChangeFlag = true;
                    if (mGuideData.EqualsChangeList(data))
                    {
                        delChangeFlag = false;
                    }
                    SingletonObject<GuideManager>.GetInst().MoveNextStep();
                    SetCurrentGuide(data, delChangeFlag);
                } else
                {
                    SingletonObject<GuideManager>.GetInst().GuideFinished();
                    OnClose();
                }
            }
            else if (name.Equals("btnSkipAll"))
            {
                SingletonObject<GuideManager>.GetInst().SkipGuide();
                OnClose();
            }
            else if (name.Equals("btnCannel"))
            {
                HideSkipChoiceUI();
            }
            else if (mEditFlag)
            {
                EditClick(name);
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    /*protected override void OnButtonPress(GameObject obj, bool press)
    {
        base.OnButtonPress(obj, press);
        if (obj.name.Equals("btnClick"))
        {
            if (press)
            {
                if (0 == mSkipGuideTime)
                {
                    mSkipGuideTime = Time.time;
                }
            } else
            {
                if (Time.time - mSkipGuideTime >= 3)
                {
                    SingletonObject<GuideManager>.GetInst().SkipGuide();
                    OnClose();
                } else
                {
                    mSkipGuideTime = 0;
                }
            }
        }
    }*/
    private void EditClick(string btnName)
    {
        bool changePos = false;
        UIWidget.Pivot pivot = UIWidget.Pivot.Center;
        switch (btnName)
        {
            case "topLeft":
                pivot = UIWidget.Pivot.TopLeft;
                changePos = true;
                break;
            case "top":
                pivot = UIWidget.Pivot.Top;
                changePos = true;
                break;
            case "topRight":
                pivot = UIWidget.Pivot.TopRight;
                changePos = true;
                break;
            case "left":
                pivot = UIWidget.Pivot.Left;
                changePos = true;
                break;
            case "center":
                pivot = UIWidget.Pivot.Center;
                changePos = true;
                break;
            case "right":
                pivot = UIWidget.Pivot.Right;
                changePos = true;
                break;
            case "bottomLeft":
                pivot = UIWidget.Pivot.BottomLeft;
                changePos = true;
                break;
            case "bottom":
                pivot = UIWidget.Pivot.Bottom;
                changePos = true;
                break;
            case "bottomRight":
                pivot = UIWidget.Pivot.BottomRight;
                changePos = true;
                break;
            case "btnFinished":
                SingletonObject<GuideManager>.GetInst().SaveGuide();
                break;
            case "btnReset":
                SingletonObject<GuideManager>.GetInst().ResetGuide();
                SetCurrentGuide(SingletonObject<GuideManager>.GetInst().GetCurrentData(), true);
                break;
            case "btnPre":
                {
                    SingletonObject<GuideManager>.GetInst().MoveLastStep();
                    SetCurrentGuide(SingletonObject<GuideManager>.GetInst().GetCurrentData(), true);
                }
                break;
            case "btnNext":
                {
                    SingletonObject<GuideManager>.GetInst().MoveNextStep();
                    SetCurrentGuide(SingletonObject<GuideManager>.GetInst().GetCurrentData(), true);
                }
                break;
            case "btnDel":
                if (null != mGuideData)
                {
                    SingletonObject<GuideManager>.GetInst().RemoveCurrentStepData();
                    SetCurrentGuide(SingletonObject<GuideManager>.GetInst().GetCurrentData(), true);
                }
                break;
            case "btnAdd"://增加一步
                mGuideData = null;
                break;
            case "btnSave":
                if (null == mGuideData)
                {
                    AddNewGuide();
                } else
                {
                    ModifyGuide();
                }
                break;
        }
        if (changePos && null != mRobotTrans)
        {
            mPivot = pivot;
            SetRobotPosition(pivot, GetInputVector2(mTrans.Find("panel/editmode/margin")));
        }
    }

    Vector2 GetInputVector2(Transform trans)
    {
        if (null != trans)
        {
            UIInput xInput = GameHelper.FindChildComponent<UIInput>(trans, "offset_x/Input");
            UIInput yInput = GameHelper.FindChildComponent<UIInput>(trans, "offset_y/Input");
            float marginx = 0;
            float marginy = 0;
            if (null != xInput && PublicFunction.IsFloat(xInput.value))
            {
                marginx = float.Parse(xInput.value);
            }
            if (null != yInput && PublicFunction.IsFloat(yInput.value))
            {
                marginy = float.Parse(yInput.value);
            }
            return new Vector2(marginx, marginy);
        }
        return Vector2.zero;
    }

    void SetInputVector2(Transform trans, Vector2 offset)
    {
        if (null != trans)
        {
            UIInput xInput = GameHelper.FindChildComponent<UIInput>(trans, "offset_x/Input");
            UIInput yInput = GameHelper.FindChildComponent<UIInput>(trans, "offset_y/Input");
            if (null != xInput)
            {
                xInput.value = offset.x.ToString();
            }
            if (null != yInput)
            {
                yInput.value = offset.y.ToString();
            }
        }
    }

    private void AddNewGuide()
    {
        mGuideData = GetDeployData();
        SingletonObject<GuideManager>.GetInst().AddGuideData(mGuideData);
        mGuideData = mGuideData.Copy();
    }

    private void ModifyGuide()
    {
        mGuideData = GetDeployData();
        SingletonObject<GuideManager>.GetInst().ModifyGuideData(mGuideData);
        mGuideData = mGuideData.Copy();
    }

    private GuideData GetDeployData()
    {
        GuideData guideData = new GuideData();
        guideData.unitySceneType = SceneMgr.GetCurrentSceneType();
        if (null != SingletonObject<SceneManager>.GetInst().GetCurrentScene())
        {
            guideData.clientSceneType = SingletonObject<SceneManager>.GetInst().GetCurrentScene().GetType().ToString();
        }
        guideData.robotPivot = mPivot;
        guideData.robotMargin = GetInputVector2(mTrans.Find("panel/editmode/margin"));
        if (null != mRobotTrans)
        {
            guideData.robotLocalEulerAngles = mRobotTrans.localEulerAngles;
        }
        if (null != mDialogTrans)
        {
            UILabel lb = GameHelper.FindChildComponent<UILabel>(mDialogTrans, "text");
            if (null != lb)
            {
                guideData.textKey = lb.text.Trim();
            }
        }
        guideData.dialogOffsetPosition = GetInputVector2(mTrans.Find("panel/editmode/dialog"));
        if (null != mDialogBgTrans)
        {
            guideData.dialogLocalEulerAngles = mDialogBgTrans.localEulerAngles;
        }
        return guideData;
    }

    private void SetRobotPosition(UIWidget.Pivot pivot, Vector2 margin)
    {
        Vector3 dialogPos;
        TweenPosition tweenPos = GameHelper.PlayTweenPosition(mRobotTrans, pivot, margin, 0.6f, false);
        if (null != tweenPos)
        {
            dialogPos = tweenPos.to;
        } else
        {
            dialogPos = mRobotTrans.localPosition;
        }
        if (null != mDialogTrans)
        {
            Vector2 offset = GetInputVector2(mTrans.Find("panel/editmode/dialog"));
            dialogPos += new Vector3(offset.x, offset.y);
            GameHelper.PlayTweenPosition(mDialogTrans, dialogPos, 0.6f);
        }
    }
    private void InitEditUI(Transform editmode)
    {
        if (null != editmode)
        {
            GameHelper.PlayTweenPosition(editmode.Find("topLeft"), UIWidget.Pivot.TopLeft, new Vector2(0, 120));
            GameHelper.PlayTweenPosition(editmode.Find("top"), UIWidget.Pivot.Top, new Vector2(0, 120));
            GameHelper.PlayTweenPosition(editmode.Find("topRight"), UIWidget.Pivot.TopRight, new Vector2(0, 120));
            GameHelper.PlayTweenPosition(editmode.Find("left"), UIWidget.Pivot.Left, Vector2.zero);
            GameHelper.PlayTweenPosition(editmode.Find("center"), UIWidget.Pivot.Center, Vector2.zero);
            GameHelper.PlayTweenPosition(editmode.Find("right"), UIWidget.Pivot.Right, Vector2.zero);
            GameHelper.PlayTweenPosition(editmode.Find("bottomLeft"), UIWidget.Pivot.BottomLeft, Vector2.zero);
            GameHelper.PlayTweenPosition(editmode.Find("bottom"), UIWidget.Pivot.Bottom, Vector2.zero);
            GameHelper.PlayTweenPosition(editmode.Find("bottomRight"), UIWidget.Pivot.BottomRight, Vector2.zero);
            GameHelper.PlayTweenPosition(editmode.Find("btnNext"), UIWidget.Pivot.BottomRight, new Vector2(0, PublicFunction.Back_Btn_Pos.y + 80));
            GameHelper.PlayTweenPosition(editmode.Find("btnPre"), UIWidget.Pivot.BottomRight, new Vector2(100, PublicFunction.Back_Btn_Pos.y + 80));
            GameHelper.PlayTweenPosition(editmode.Find("btnReset"), UIWidget.Pivot.BottomLeft, new Vector2(PublicFunction.Back_Btn_Pos.x, PublicFunction.Back_Btn_Pos.y + 100));
            GameHelper.PlayTweenPosition(editmode.Find("btnDel"), UIWidget.Pivot.TopRight, new Vector2(300, PublicFunction.Back_Btn_Pos.y));
            GameHelper.PlayTweenPosition(editmode.Find("btnAdd"), UIWidget.Pivot.TopRight, new Vector2(200, PublicFunction.Back_Btn_Pos.y));
            GameHelper.PlayTweenPosition(editmode.Find("btnSave"), UIWidget.Pivot.TopRight, new Vector2(100, PublicFunction.Back_Btn_Pos.y));
            GameHelper.PlayTweenPosition(editmode.Find("btnFinished"), UIWidget.Pivot.TopRight, new Vector2(0, PublicFunction.Back_Btn_Pos.y ));
            GameHelper.PlayTweenPosition(editmode.Find("margin"), UIWidget.Pivot.TopRight, new Vector2(0, PublicFunction.Back_Btn_Pos.y + 200));
            GameHelper.PlayTweenPosition(editmode.Find("dialog"), UIWidget.Pivot.BottomRight, new Vector2(0, PublicFunction.Back_Btn_Pos.y + 200));
        }
        if (null != mTrans)
        {
            Transform btnClick = mTrans.Find("panel/btnClick");
            if (null != btnClick)
            {
                btnClick.gameObject.SetActive(false);
            }
        }
    }

    private void SetCurrentGuide(GuideData data, bool delChangeFlag)
    {
        if (null != data)
        {
            mGuideData = data.Copy();
        }
        else
        {
            mGuideData = null;
        }
        RefreshUI(mGuideData, delChangeFlag);
    }

    private void RefreshUI(GuideData data, bool delChangeFlag)
    {
        if (delChangeFlag)
        {
            RestoreGameObjectData();
        }
        GuideData initData = data;
        if (null == initData)
        {
            initData = new GuideData();
            initData.dialogOffsetPosition = new Vector3(380, 50);
            initData.textKey = "请开始编辑指引";
            initData.robotPivot = UIWidget.Pivot.Center;
        }
        Vector3 dialogPos;
        if (null != mRobotTrans)
        {
            mRobotTrans.localEulerAngles = initData.robotLocalEulerAngles;
            mRobotTrans.localPosition = UIManager.GetWinPos(mRobotTrans, initData.robotPivot, initData.robotMargin.x, initData.robotMargin.y);
            dialogPos = mRobotTrans.localPosition;
            mRobotTrans.localScale = new Vector3(0.2f, 0.2f, 1);
            GameHelper.PlayTweenScale(mRobotTrans, Vector3.one, 0.65f);
            /*TweenPosition tweenPos = GameHelper.PlayTweenPosition(mRobotTrans, initData.robotPivot, initData.robotMargin);
            if (null != tweenPos)
            {
                dialogPos = tweenPos.to;
            }
            else
            {
                dialogPos = mRobotTrans.localPosition;
            }*/
            if (null != mDialogTrans)
            {
                UILabel lb = GameHelper.FindChildComponent<UILabel>(mDialogTrans, "text");
                int lbHeight = 100;
                if (null != lb)
                {
                    if (mEditFlag)
                    {
                        lb.text = initData.textKey;
                    }
                    else
                    {
                        lb.text = LauguageTool.GetIns().GetText(initData.textKey);
                    }
                    if (!string.IsNullOrEmpty(lb.text))
                    {
                        string finalText = null;
                        lb.Wrap(lb.text, out finalText);
                        string[] lines = finalText.Split('\n');
                        if (null != lines)
                        {
                            lbHeight = lb.fontSize * lines.Length + lb.spacingY * (lines.Length - 1);
                            if (lbHeight < 100)
                            {
                                lbHeight = 100;
                            }
                        }
                        lb.height = lbHeight;
                    }
                }
                if (null != mDialogBgTrans)
                {
                    mDialogBgTrans.localEulerAngles = initData.dialogLocalEulerAngles;
                    UISprite bgSprite = mDialogBgTrans.GetComponent<UISprite>();
                    if (null != bgSprite)
                    {
                        bgSprite.height = lbHeight + 60;
                    }
                    Transform jiao = mDialogBgTrans.Find("jiao");
                    if (null != jiao)
                    {
                        jiao.localPosition = new Vector3(jiao.localPosition.x, -((lbHeight + 60) / 2 + 9f));
                    }
                }
                dialogPos += new Vector3(initData.dialogOffsetPosition.x, initData.dialogOffsetPosition.y);
                mDialogTrans.localPosition = dialogPos;
                mDialogTrans.localScale = new Vector3(1, 0, 1);
                GameHelper.PlayTweenScale(mDialogTrans, Vector3.one, 0.65f);
                //GameHelper.PlayTweenPosition(mDialogTrans, dialogPos, 0.6f);
            }
            if (null != initData.changeList)
            {
                ChangeDepth(initData.changeList);
            }
            if (mEditFlag)
            {
                SetInputVector2(mTrans.Find("panel/editmode/margin"), initData.robotMargin);
                SetInputVector2(mTrans.Find("panel/editmode/dialog"), initData.dialogOffsetPosition);
            }
        }

    }

    private void ChangeDepth(List<DepthChangeData> list)
    {
        if (null != list)
        {
            DepthChangeData depthData = null;
            for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                depthData = list[i];
                GameObject targetObj = FindTargetObj(depthData);
                if (null != targetObj)
                {
                    switch (depthData.changeType)
                    {
                        case DepthChangeType.Change_Camera_Depth:
                            ChangeCameraDepth(targetObj, depthData);
                            break;
                        case DepthChangeType.Change_Widget_Depth:
                            ChangeWidgetDepth(targetObj, depthData);
                            break;
                        case DepthChangeType.Change_Panel_Depth:
                            ChangePanelDepth(targetObj, depthData);
                            break;
                    }
                }

            }
        }
    }

    private void ChangeCameraDepth(GameObject targetObj, DepthChangeData data)
    {
        Camera camera = targetObj.GetComponent<Camera>();
        if (null != camera)
        {
            AddRestoreData(targetObj, new GameObjectRestoreData(data.changeType, false, camera.depth));
            camera.depth = data.depth;
        }
    }

    private void ChangeWidgetDepth(GameObject targetObj, DepthChangeData data)
    {
        UIWidget widget = targetObj.GetComponent<UIWidget>();
        if (null != widget)
        {
            AddRestoreData(targetObj, new GameObjectRestoreData(data.changeType, false, widget.depth));
            widget.depth = data.depth;
        }
    }

    private void ChangePanelDepth(GameObject targetObj, DepthChangeData data)
    {
        UIPanel uiPanel = targetObj.GetComponent<UIPanel>();
        if (null == uiPanel)
        {
            UIPanel parentPanel = NGUITools.FindInParents<UIPanel>(targetObj);
            AddRestoreData(targetObj, new GameObjectRestoreData(data.changeType, true, 0));
            uiPanel = targetObj.AddComponent<UIPanel>();
            uiPanel.depth = mDepth + data.depth;
            if (null != parentPanel)
            {
                uiPanel.clipping = parentPanel.clipping;
                uiPanel.baseClipRegion = parentPanel.baseClipRegion;
            }

        } else
        {
            AddRestoreData(targetObj, new GameObjectRestoreData(data.changeType, false, uiPanel.depth));
            uiPanel.depth = mDepth + data.depth;
        }
        NGUITools.MarkParentAsChanged(targetObj);
    }

    private GameObject FindTargetObj(DepthChangeData data)
    {
        if (!string.IsNullOrEmpty(data.targetPath))
        {
            if (data.findTargetWay == FindTargetWay.Find_Abs_Path)
            {
                return GameObject.Find(data.targetPath);
            } else if (data.findTargetWay == FindTargetWay.Find_Child)
            {
                GameObject obj = GameObject.Find(data.targetPath);
                if (null != obj)
                {
                    Transform trans = obj.transform.GetChild(data.childIndex);
                    if (null != trans)
                    {
                        return trans.gameObject;
                    }
                }
            }
        }
        return null;
    }

    private void AddRestoreData(GameObject obj, GameObjectRestoreData data)
    {
        if (null == mRestoreDict)
        {
            mRestoreDict = new Dictionary<GameObject, List<GameObjectRestoreData>>();
        }
        if (!mRestoreDict.ContainsKey(obj))
        {
            List<GameObjectRestoreData> list = new List<GameObjectRestoreData>();
            mRestoreDict[obj] = list;
        }
        mRestoreDict[obj].Add(data);
    }

    private void RestoreGameObjectData()
    {
        if (null != mRestoreDict)
        {
            GameObjectRestoreData restoreData = null;
            foreach (var kvp in mRestoreDict)
            {
                for (int i = 0, imax = kvp.Value.Count; i < imax; ++i)
                {
                    restoreData = kvp.Value[i];
                    switch (restoreData.changeType)
                    {
                        case DepthChangeType.Change_Camera_Depth:
                            {
                                Camera camera = kvp.Key.GetComponent<Camera>();
                                if (null != camera)
                                {
                                    camera.depth = restoreData.depth;
                                }
                            }
                            break;
                        case DepthChangeType.Change_Panel_Depth:
                            UIPanel uiPanel = kvp.Key.GetComponent<UIPanel>();
                            if (null != uiPanel)
                            {
                                if (restoreData.needDelPanel)
                                {
                                    NGUITools.Destroy(uiPanel);
                                    /*UIWidget widget = kvp.Key.GetComponent<UIWidget>();
                                    if (null != widget)
                                    {
                                        widget.depth = (int)restoreData.depth;
                                    }*/
                                    
                                } else
                                {
                                    uiPanel.depth = (int)restoreData.depth;
                                }
                            }
                            break;
                        case DepthChangeType.Change_Widget_Depth:
                            {
                                UIWidget widget = kvp.Key.GetComponent<UIWidget>();
                                if (null != widget)
                                {
                                    widget.depth = (int)restoreData.depth;
                                }
                            }
                            break;
                    }
                }
                NGUITools.MarkParentAsChanged(kvp.Key);
            }
            mRestoreDict.Clear();
        }
    }


    private void ShowSkipChoiceUI()
    {
        HideGuideGameObject();
        RestoreGameObjectData();
        if (null != mSkipPanelObj)
        {
            Transform panel = mSkipPanelObj.transform.Find("panel");
            if (null != panel)
            {
                panel.localPosition = mSkipPanelPos - new Vector3(0, 300);
                GameHelper.PlayTweenPosition(panel, mSkipPanelPos);
            }
            mSkipPanelObj.SetActive(true);
        }
    }

    private void HideSkipChoiceUI()
    {
        ShowGuideGameObject();
        if (null != mGuideData && null != mGuideData.changeList)
        {
            ChangeDepth(mGuideData.changeList);
        }
        if (null != mSkipPanelObj)
        {
            mSkipPanelObj.SetActive(false);
        }
    }

    private void HideGuideGameObject()
    {
        if (null != mContentTrans)
        {
            mContentTrans.gameObject.SetActive(false);
        }
        if (null != mBtnCloseObj)
        {
            mBtnCloseObj.SetActive(false);
        }
        if (null != mBtnClickObj)
        {
            mBtnClickObj.SetActive(false);
        }
        if (null != mBtnSkipObj)
        {
            mBtnSkipObj.SetActive(false);
        }
    }

    private void ShowGuideGameObject()
    {
        if (null != mContentTrans)
        {
            mContentTrans.gameObject.SetActive(true);
        }
        if (null != mBtnCloseObj)
        {
            mBtnCloseObj.SetActive(true);
        }
        if (null != mBtnClickObj)
        {
            mBtnClickObj.SetActive(true);
        }
        if (null != mBtnSkipObj)
        {
            mBtnSkipObj.SetActive(true);
        }
    }

    private class GameObjectRestoreData
    {
        public bool needDelPanel;
        public float depth;
        public DepthChangeType changeType;
        public GameObjectRestoreData(DepthChangeType changeType, bool delPanel, float depth)
        {
            this.changeType = changeType;
            this.needDelPanel = delPanel;
            this.depth = depth;
        }
    }
}
