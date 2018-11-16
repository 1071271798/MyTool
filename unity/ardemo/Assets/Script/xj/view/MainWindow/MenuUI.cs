using Game;
using Game.Platform;
using Game.Scene;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Game.Resource;
using Game.Event;

public class MenuUI : BaseUI
{

    private MainMenuType mMenuType;
    private UILabel mTitleLabel;


    private Transform mScrollViewGrid;
    private UIScrollView mScrollView;
    private SpringPanel mSpringPanel;

    private GameObject mItem;
    private Vector2 mItemSize;
    private Vector2 mDeleteSize;

    private GameObject mDragObject;
    private UISprite mDragDeleteSprite;
    private float mDragDistanceAcc;
    private Vector3 mRightTargetPos;


    private string Item_Start = "item_";

    private Robot mRobot;
    private ActionSequence mAction;
    private ActionRunState mActionState;

    private Dictionary<string, Dictionary<string, object>> mProgramDict;
    public MenuUI()
    {
        mUIResPath = "Prefab/UI/menuUI";
    }

    public void SetMenuData(MainMenuType type)
    {
        mMenuType = type;
    }

    protected override void AddEvent()
    {
        try
        {
            PlatformMgr.Instance.MobPageStart(MobClickEventID.P7);
            base.AddEvent();
            EventMgr.Inst.Regist(EventID.Set_Choice_Robot, SetChoiceRobot);
            EventMgr.Inst.Regist(EventID.Refresh_List, RefreshList);
            mRobot = RobotManager.GetInst().GetCurrentRobot();
            if (null != mTrans)
            {
                Transform item = mTrans.Find("item");
                if (null != item)
                {
                    mItem = item.gameObject;
                }
                Transform right = mTrans.Find("right");
                if (null != right)
                {
                    int marginTop = 0;
                    int marginBotton = 0;
                    UISprite bg = GameHelper.FindChildComponent<UISprite>(right, "bg");
                    int width = (int)(PublicFunction.GetWidth() * PublicFunction.Main_Scene_Right_Width);
                    int height = PublicFunction.GetHeight() - marginTop - marginBotton;
                    if (null != bg)
                    {
                        bg.width = width;
                        bg.height = height;
                    }
                    
                    /*Transform title = right.FindChild("title");
                    if (null != title)
                    {
                        Vector2 size = NGUIMath.CalculateRelativeWidgetBounds(title).size;
                        title.localPosition = new Vector2(0, height / 2 - 26 - size.y / 2);
                        mTitleLabel = GameHelper.FindChildComponent<UILabel>(title, "name");
                    }*/
                    Transform btnAdd = right.Find("btnAdd");
                    int addHeight = 100;
                    if (null != btnAdd)
                    {
                        UISprite addBg = GameHelper.FindChildComponent<UISprite>(btnAdd, "Background");
                        if (null != addBg)
                        {
                            addBg.width = width;
                        }
                        BoxCollider box = btnAdd.GetComponent<BoxCollider>();
                        if (null != box)
                        {
                            box.size = new Vector2(width, box.size.y);
                        }
                        UILabel lb = GameHelper.FindChildComponent<UILabel>(btnAdd, "Label");
                        if (null != lb)
                        {
                            lb.text = LauguageTool.GetIns().GetText("新建动作");
                        }
                        GameHelper.SetButtonCentered(btnAdd, 20);
                        Vector2 size = NGUIMath.CalculateRelativeWidgetBounds(btnAdd).size;
                        btnAdd.localPosition = new Vector2(0, -height / 2 + size.y / 2);
                        if (PlatformMgr.Instance.EditFlag)
                        {
                            addHeight = (int)size.y;
                        } else
                        {
                            addHeight = 86;
                        }
                    }

                    if (null != mItem)
                    {//初始化条的数据
                        UISprite line = GameHelper.FindChildComponent<UISprite>(item, "line");
                        int marginX = 40;
                        if (null != line)
                        {
                            line.width = width - marginX * 2;
                        }
                        int textWidth = width;
                        int textLeftWidth = 0;
                        Transform officialIcon = item.Find("officialIcon");
                        if (null != officialIcon)
                        {
                            Vector2 size = NGUIMath.CalculateRelativeWidgetBounds(officialIcon).size;
                            officialIcon.localPosition = new Vector2(-width / 2 + size.x / 2 + 10, 0);
                        }
                        Transform icon = item.Find("icon");
                        if (null != icon)
                        {
                            Vector2 size = NGUIMath.CalculateRelativeWidgetBounds(icon).size;
                            icon.localPosition = new Vector2(-width / 2 + size.x / 2 + marginX, 0);
                            textLeftWidth = (int)size.x + marginX;
                            textWidth -= textLeftWidth;
                        }
                        Transform btnEdit = item.Find("btnEdit");
                        if (null != btnEdit)
                        {
                            Vector2 size = NGUIMath.CalculateRelativeWidgetBounds(btnEdit).size;
                            btnEdit.localPosition = new Vector2(width / 2 - size.x / 2 - marginX - PublicFunction.Iphonex_Add_Offset.x, 0);
                            textWidth -= (int)(size.x + marginX + PublicFunction.Iphonex_Add_Offset.x);
                        }
                        Transform btnDelete = item.Find("btnDelete");
                        if (null != btnDelete)
                        {
                            mDeleteSize = NGUIMath.CalculateRelativeWidgetBounds(btnDelete).size;
                            btnDelete.localPosition = new Vector2(width / 2 + mDeleteSize.x / 2 - PublicFunction.Iphonex_Add_Offset.x, 0);
                            mDeleteSize.x += 12;
                        }
                        Transform name = item.Find("name");
                        if (null != name)
                        {
                            textWidth -= 52;//文字与图标的间隙
                            textLeftWidth += 26;
                            UILabel nameLb = name.GetComponent<UILabel>();
                            if (null != nameLb)
                            {
                                nameLb.width = textWidth;
                            }
                            name.localPosition = new Vector2(-width / 2 + textLeftWidth, 0);
                        }
                        mItemSize = NGUIMath.CalculateRelativeWidgetBounds(item).size;
                        BoxCollider box = item.GetComponent<BoxCollider>();
                        if (null != box)
                        {
                            box.size = new Vector2(mItemSize.x, box.size.y);
                        }
                    }

                    Transform panel = right.Find("panel");
                    if (null != panel)
                    {
                        int topHight = 146;
                        panel.localPosition = new Vector2(0, (addHeight - topHight) / 2);
                        int clipHeight = height - topHight - addHeight;
                        UIPanel uiPanel = panel.GetComponent<UIPanel>();
                        if (null != uiPanel)
                        {
                            uiPanel.baseClipRegion = new Vector4(-PublicFunction.Iphonex_Add_Offset.x / 2, 0, width - PublicFunction.Iphonex_Add_Offset.x, clipHeight);
                        }
                        mScrollView = panel.GetComponent<UIScrollView>();
                        mScrollViewGrid = panel.Find("grid");
                        if (null != mScrollViewGrid)
                        {
                            mScrollViewGrid.localPosition = new Vector3(0, clipHeight / 2);
                        }
                    }

                    mRightTargetPos = UIManager.GetWinPos(right, UIWidget.Pivot.Right);
                    right.localPosition = mRightTargetPos;
                }

                if (null != mItem)
                {
                    mItem.SetActive(false);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    public override void Release()
    {
        base.Release();
        EventMgr.Inst.UnRegist(EventID.Set_Choice_Robot, SetChoiceRobot);
        EventMgr.Inst.UnRegist(EventID.Refresh_List, RefreshList);
        PlatformMgr.Instance.MobPageEnd(MobClickEventID.P7);
    }

    public override void UpdateUI()
    {
        try
        {
            base.UpdateUI();
            mAction = null;
            mActionState = ActionRunState.None;
            InitMenu(mMenuType);
            if (null != mTrans)
            {
                Transform right = mTrans.Find("right");
                if (null != right)
                {
                    right.localPosition = mRightTargetPos;// new Vector2(mRightTargetPos.x + PublicFunction.GetWidth() * 0.4f, mRightTargetPos.y);
                    //GameHelper.PlayTweenPosition(right, mRightTargetPos, 0.6f);
                }
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
            if (null == mRobot)
            {
                SingletonObject<LogicCtrl>.GetInst().CommonTipsCallBack(LogicLanguage.GetText("请重新选择或创建模型"), 1, CommonTipsColor.red);
                return;
            }
            if (mMenuType == MainMenuType.Action_Menu)
            {
                OnActionClick(obj);
            } else
            {
                OnProgramClick(obj);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }

    void OnActionClick(GameObject obj)
    {
        string name = obj.name;
        switch (name)
        {
            case "btnAdd":
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P7_1);
                if (PlatformMgr.Instance.IsChargeProtected)
                {
                    PublicPrompt.ShowChargePrompt(null);
                    return;
                }
                if (null != mRobot)
                {
                    SceneMgrTest.Instance.LastScene = SceneType.EditAction;
                    SetEditActionModel();
                    HideUI();
                    SingletonObject<PublicTools>.GetInst().ResetCameraPos(true);
                    ActionEditScene.CreateActions(string.Empty, string.Empty);
                }
                break;
            case "btnEdit":
                {
                    if (PlatformMgr.Instance.IsChargeProtected)
                    {
                        PublicPrompt.ShowChargePrompt(null);
                        return;
                    }
                    string parentName = obj.transform.parent.name;
                    if (parentName.StartsWith(Item_Start))
                    {
                        SceneMgrTest.Instance.LastScene = SceneType.EditAction;
                        SetEditActionModel();
                        HideUI();
                        string actId = parentName.Substring(Item_Start.Length);
                        SingletonObject<PublicTools>.GetInst().ResetCameraPos(true);
                        ActionEditScene.OpenActions(actId);
                    }
                }
                break;
            case "btnDelete":
                {
                    string parentName = obj.transform.parent.name;
                    if (parentName.StartsWith(Item_Start) && null != mRobot)
                    {
                        string actId = parentName.Substring(Item_Start.Length);
                        mRobot.DeleteActionsForID(actId);
                        RemoveItem(obj.transform.parent);
                    }
                }
                break;
            default:
                {
                    if (name.StartsWith(Item_Start) && null != mRobot)
                    {//动作
                        if (null != mDragObject)
                        {
                            ResetDragObject();
                            return;
                        }
                        if (PlatformMgr.Instance.IsChargeProtected)
                        {
                            PublicPrompt.ShowChargePrompt(null);
                            return;
                        }
                        if (!PlatformMgr.Instance.GetBluetoothState())
                        {
                            SingletonObject<ConnectCtrl>.GetInst().OpenConnectPage(mRobot);
                            return;
                        }
                        string actId = name.Substring(Item_Start.Length);
                        ActionSequence act = mRobot.GetActionsForID(actId);
                        if (null == mAction || mAction != act)
                        {
                            mAction = act;
                            if (null != mAction)
                            {
                                mActionState = ActionRunState.Run;
                                mRobot.StopRunTurn();
                                mRobot.PlayActions(mAction, PlayActionsDelegate);
                            }
                        } else
                        {
                            if (mActionState == ActionRunState.Run)
                            {
                                mRobot.StopNowPlayActions();
                                mActionState = ActionRunState.None;
                                mAction = null;
                                //mRobot.PauseActionsForID(mAction.Id);
                                //mActionState = ActionRunState.Pause;
                            } else if (mActionState == ActionRunState.Pause)
                            {
                                mRobot.ContinueActionsForID(mAction.Id);
                                mActionState = ActionRunState.Run;
                            }
                        }
                    }
                }
                break;
        }
    }

    void OnProgramClick(GameObject obj)
    {
        string name = obj.name;
        switch (name)
        {
            case "btnAdd":
                if (PlatformMgr.Instance.IsChargeProtected)
                {
                    PublicPrompt.ShowChargePrompt(null);
                    return;
                }
                GotoProgram(null);
                break;
            case "btnEdit":
                {
                    if (PlatformMgr.Instance.IsChargeProtected)
                    {
                        PublicPrompt.ShowChargePrompt(null);
                        return;
                    }
                    string parentName = obj.transform.parent.name;
                    if (parentName.StartsWith(Item_Start))
                    {
                        string xmlId = parentName.Substring(Item_Start.Length);
                        GotoProgram(xmlId);
                    }
                }
                break;
            case "btnDelete":
                {
                    string parentName = obj.transform.parent.name;
                    if (parentName.StartsWith(Item_Start) && null != mRobot)
                    {
                        string xmlId = parentName.Substring(Item_Start.Length);
                        if (!string.IsNullOrEmpty(xmlId) && null != mProgramDict && mProgramDict.ContainsKey(xmlId))
                        {
                            string result = Json.Serialize(mProgramDict[xmlId]);
                            PlatformMgr.Instance.CallPlatformFunc(CallPlatformFuncID.deleteProgram, result);
                            RemoveItem(obj.transform.parent);
                        }
                    }
                }
                break;
            default:
                {
                    if (name.StartsWith(Item_Start) && null != mRobot)
                    {//程序
                        if (null != mDragObject)
                        {
                            ResetDragObject();
                            return;
                        }
                        if (PlatformMgr.Instance.IsChargeProtected)
                        {
                            PublicPrompt.ShowChargePrompt(null);
                            return;
                        }
                        string xmlId = name.Substring(Item_Start.Length);
                        GotoProgram(xmlId);
                    }
                }
                break;
        }
    }

    protected override void OnButtonPress(GameObject obj, bool press)
    {
        base.OnButtonPress(obj, press);
        if (!PlatformMgr.Instance.EditFlag)
        {
            return;
        }
        if (!press)
        {//弹起
            if (null != mDragObject)
            {
                if (-mDragObject.transform.localPosition.x >= mDeleteSize.x * 0.9f || mDragDistanceAcc >= 10)
                {
                    Vector2 pos = mDragObject.transform.localPosition;
                    pos.x = -mDeleteSize.x;
                    SpringPosition.Begin(mDragObject, pos, 8);
                } else
                {
                    ResetDragObject();
                }
            }
        }
    }

    protected override void OnButtonDrag(GameObject obj, Vector2 delta)
    {
        base.OnButtonDrag(obj, delta);
        if (!PlatformMgr.Instance.EditFlag)
        {
            return;
        }
        if (MainMenuType.Action_Menu == mMenuType && null != mAction && mActionState == ActionRunState.Run)
        {
            return;
        }
        if (null != mDragObject && obj != mDragObject)
        {
            ResetDragObject();
            return;
        }
        if (obj.name.StartsWith(Item_Start))
        {
            string id = obj.name.Substring(Item_Start.Length);
            if (mMenuType == MainMenuType.Action_Menu && null != mRobot)
            {
                ActionSequence act = mRobot.GetActionsForID(id);
                if (null != act && act.IsOfficial())
                {
                    return;
                }
            } else if (mMenuType == MainMenuType.Program_Menu && null != mProgramDict && mProgramDict.ContainsKey(id) && mProgramDict[id].ContainsKey("isDefault"))
            {
                bool isDefault = false;
                bool.TryParse(mProgramDict[id]["isDefault"].ToString(), out isDefault);
                if (isDefault)
                {
                    return;
                }
            }

        }
        if (IsPanelMove())
        {
            return;
        }
        if (null != mDragObject)
        {
            if (delta.x < 0)
            {
                if (-delta.x > mDragDistanceAcc)
                {
                    mDragDistanceAcc = -delta.x;
                } else
                {
                    mDragDistanceAcc -= (mDragDistanceAcc + delta.x) / 3;
                }
            } else
            {
                mDragDistanceAcc = 0;
            }
            Vector3 pos = mDragObject.transform.localPosition;
            pos = Vector3.Lerp(pos, pos + new Vector3(delta.x, 0) * (0.01f * 65), 0.67f);
            if (pos.x > 0)
            {
                pos.x = 0;
            } else if (pos.x < -mDeleteSize.x)
            {
                //pos.x = -mDeleteSize.x;
                if (pos.x < -2 * mDeleteSize.x)
                {
                    pos.x = -2 * mDeleteSize.x;
                }
                if (null != mDragDeleteSprite)
                {
                    mDragDeleteSprite.width = Mathf.CeilToInt(Mathf.Abs(pos.x));
                }
            }
            mDragObject.transform.localPosition = pos;
        }
        else if (obj.name.StartsWith(Item_Start) && mDragObject == null && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            mDragObject = obj;
            mDragDeleteSprite = GameHelper.FindChildComponent<UISprite>(obj.transform, "btnDelete/Background");
            UIDragScrollView dragScrollView = mDragObject.GetComponent<UIDragScrollView>();
            if (null != dragScrollView)
            {
                dragScrollView.enabled = false;
            }
        }
    }

    private void ResetDragObject()
    {
        Vector2 pos = mDragObject.transform.localPosition;
        pos.x = 0;
        SpringPosition spring = SpringPosition.Begin(mDragObject, pos, 8);
        if (null != spring)
        {
            spring.onFinished = delegate () {
                if (null != mDragDeleteSprite)
                {
                    mDragDeleteSprite.width = (int)mDeleteSize.x;
                    mDragDeleteSprite = null;
                }
            };
        } else
        {
            mDragDeleteSprite = null;
        }
        UIDragScrollView dragScrollView = mDragObject.GetComponent<UIDragScrollView>();
        if (null != dragScrollView)
        {
            dragScrollView.enabled = true;
        }
        mDragObject = null;
        
    }


    private void InitMenu(MainMenuType type)
    {
        if (null != mTrans)
        {
            Transform btnAdd = mTrans.Find("right/btnAdd");
            if (!PlatformMgr.Instance.EditFlag)
            {
                btnAdd.gameObject.SetActive(false);
            }
            UILabel name = GameHelper.FindChildComponent<UILabel>(mTrans, "right/title/name");
            if (null != name)
            {
                name.text = type == MainMenuType.Action_Menu ? LauguageTool.GetIns().GetText("动作列表") : LauguageTool.GetIns().GetText("程序列表");
            }
        }
        SetItem(type);
        RefreshMenuList(true);
    }

    private void RefreshMenuList(bool instant)
    {
        if (null != mScrollViewGrid && null != mItem)
        {
            for (int i = 0, imax = mScrollViewGrid.childCount; i < imax; ++i)
            {
                GameObject.Destroy(mScrollViewGrid.GetChild(i).gameObject);
            }
            mScrollViewGrid.DetachChildren();
            if (mMenuType == MainMenuType.Action_Menu)
            {
                InitActionList();
            }
            else
            {
                InitProgramList();
            }
            ResetQueuePosition(mScrollViewGrid, instant);
            UIManager.SetButtonEventDelegate(mScrollViewGrid, mBtnDelegate);
        }
    }


    private void SetItem(MainMenuType type)
    {
        if (null != mItem)
        {
            Transform item = mItem.transform;
            //UISprite officialIcon = GameHelper.FindChildComponent<UISprite>(item, "officialIcon");
            //UILabel name = GameHelper.FindChildComponent<UILabel>(item, "name");
            //UIButton uiButtion = item.GetComponent<UIButton>();
            Transform btnEdit = item.Find("btnEdit");
            UISprite iconbg = GameHelper.FindChildComponent<UISprite>(item, "icon/bg");
            Transform iconIcon = item.Find("icon/icon");
            Color32 color;
            if (type == MainMenuType.Action_Menu)
            {
                if (null != btnEdit)
                {
                    btnEdit.gameObject.SetActive(true);
                }
                color = new Color32(36, 168, 255, 255);
                if (null != iconIcon)
                {
                    iconIcon.gameObject.SetActive(true);
                }
                if (null != iconbg)
                {
                    iconbg.spriteName = "action_bg";
                }
            }
            else
            {
                if (null != btnEdit)
                {
                    btnEdit.gameObject.SetActive(false);
                }
                color = new Color32(219, 148, 19, 255);
                if (null != iconIcon)
                {
                    iconIcon.gameObject.SetActive(false);
                }
                if (null != iconbg)
                {
                    iconbg.spriteName = "icon_program";
                }
            }
            /*if (null != officialIcon)
            {
                officialIcon.color = color;
                officialIcon.alpha = 0.5f;
            }
            if (null != name)
            {
                name.color = color;
            }*/
            /*if (null != uiButtion)
            {
                uiButtion.pressed = color;
            }*/
        }
        
    }

    private void InitActionList()
    {
        if (null != mRobot)
        {
            List<string> list = mRobot.GetActionsIdList();
            if (null != list)
            {
                for (int i = 0, imax = list.Count; i < imax; ++i)
                {
                    ActionSequence action = mRobot.GetActionsForID(list[i]);
                    if (null != action)
                    {
                        AddAction(action);
                    }
                }
            }
        }
    }
    private void AddAction(ActionSequence action)
    {
        GameObject item = GameObject.Instantiate(mItem) as GameObject;
        item.SetActive(true);
        item.name = Item_Start + action.Id;
        Transform itemTrans = item.transform;
        itemTrans.parent = mScrollViewGrid;
        itemTrans.localPosition = new Vector3(0, 1000);
        itemTrans.localScale = Vector3.one;
        Transform officialIcon = itemTrans.Find("officialIcon");
        if (null != officialIcon)
        {
            if (!action.IsOfficial())
            {
                officialIcon.gameObject.SetActive(false);
            }
        }
        UISprite icon = GameHelper.FindChildComponent<UISprite>(itemTrans, "icon/icon");
        if (null != icon)
        {
            icon.spriteName = action.IconName;
        }
        UILabel name = GameHelper.FindChildComponent<UILabel>(itemTrans, "name");
        if (null != name)
        {
            name.text = action.Name;
        }
    }

    private void InitProgramList()
    {
        if (null != mRobot)
        {
            Dictionary<string, object> arg = new Dictionary<string, object>();
            ResFileType type = ResourcesEx.GetResFileType(RobotMgr.DataType(mRobot.Name));
            arg["modelID"] = RobotMgr.NameNoType(mRobot.Name);
            arg["modelType"] = ((int)type).ToString();
            string json = PlatformMgr.Instance.GetData(PlatformDataType.program, Json.Serialize(arg));
            //string json = "[{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206193408837473\",\"xmlName\":\"交通指挥员\",\"xmlNameLang\":{\"ja\":\"交通整理員\",\"da\":\"Trafikbetjent\",\"tr\":\"Trafik Kontrolcüsü\",\"fr\":\"Contrôleur de la circulation\",\"de\":\"Verkehrslotse\",\"ar\":\"منظِّم حركة المرور\",\"pl\":\"Kontroler ruchu\",\"it\":\"Controller del traffico\",\"pt\":\"Polícia sinaleiro\",\"en\":\"Traffic Controller\",\"zh-hans\":\"交通指挥员\",\"ru\":\"Регулировщик\",\"es\":\"Policía de tráfico\",\"zh-hant\":\"交通指揮員\",\"ko\":\"교통 통제소\"}},{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206193209189621\",\"xmlName\":\"探星1号要抱抱\",\"xmlNameLang\":{\"ja\":\"AstroBot はハグを求める\",\"da\":\"AstroBot ønsker en omfavnelse\",\"tr\":\"AstroBot sarılmak istiyor\",\"fr\":\"AstroBot veut un câlin.\",\"de\":\"AstroBot möchte eine Umarmung\",\"ar\":\"يحتاج AstroBot إلى العناق\",\"pl\":\"AstroBot chce się przytulić\",\"it\":\"AstroBot desidera un abbraccio\",\"pt\":\"O AstroBot quer um abraço\",\"en\":\"AstroBot wants a hug\",\"zh-hans\":\"探星1号要抱抱\",\"ru\":\"AstroBot хочет обняться\",\"es\":\"AstroBot quiere un abrazo\",\"zh-hant\":\"探星1號要抱抱\",\"ko\":\"AstroBot이 포옹을 원합니다.\"}},{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206193010761804\",\"xmlName\":\"探星1号打拳击\",\"xmlNameLang\":{\"ja\":\"AstroBot はボクシングをする\",\"da\":\"AstroBot leger at den bokser\",\"tr\":\"AstroBot boks oynuyor\",\"fr\":\"AstroBot boxe.\",\"de\":\"AstroBot boxt\",\"ar\":\"يلعب AstroBot الملاكمة\",\"pl\":\"AstroBot bawi się w boksera\",\"it\":\"AstroBot gioca a boxe\",\"pt\":\"O AstroBot pratica boxe\",\"en\":\"AstroBot plays boxing\",\"zh-hans\":\"探星1号打拳击\",\"ru\":\"AstroBot играет в бокс\",\"es\":\"AstroBot juega al box\",\"zh-hant\":\"探星1號打拳擊\",\"ko\":\"AstroBot이 복싱을 합니다.\"}},{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206192701607495\",\"xmlName\":\"避障小能手\",\"xmlNameLang\":{\"ja\":\"小さな障害物回避ロボット\",\"da\":\"Undgår små forhindringer\",\"tr\":\"Küçük engel aşıcı\",\"fr\":\"Éviter de petits obstacles\",\"de\":\"Kleiner Hindernisvermeider\",\"ar\":\"متجنب للعوائق الصغيرة\",\"pl\":\"Omijanie małych przeszkód\",\"it\":\"Ausiliario del traffico\",\"pt\":\"Profissional em evitar obstáculos\",\"en\":\"Little obstacle avoider\",\"zh-hans\":\"避障小能手\",\"ru\":\"Малыш, избегающий препятствия\",\"es\":\"Pequeño esquivador de obstáculos\",\"zh-hant\":\"避障小高手\",\"ko\":\"작은 장애물 회피기\"}},{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206184936830459\",\"xmlName\":\"探星1号爱舞蹈\",\"xmlNameLang\":{\"ja\":\"AstroBot はダンスが好き\",\"da\":\"AstroBot elsker at danse\",\"tr\":\"AstroBot dans etmeyi seviyor\",\"fr\":\"AstroBot aime danser.\",\"de\":\"AstroBot tanzt gern\",\"ar\":\"يُحب AstroBot الرقص\",\"pl\":\"AstroBot uwielbia tańczyć\",\"it\":\"AstroBot ama ballare\",\"pt\":\"O AstroBot adora dançar\",\"en\":\"AstroBot loves to dance\",\"zh-hans\":\"探星1号爱舞蹈\",\"ru\":\"AstroBot любит танцевать\",\"es\":\"AstroBot ama bailar\",\"zh-hant\":\"探星1號愛舞蹈\",\"ko\":\"AstroBot이 춤을 좋아합니다.\"}},{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206183911861473\",\"xmlName\":\"探星1号会搬运\",\"xmlNameLang\":{\"ja\":\"AstroBot は物を運ぶことができる\",\"da\":\"AstroBot kan bære ting\",\"tr\":\"AstroBot birşeyleri taşıyabilir\",\"fr\":\"AstroBot peut porter des choses.\",\"de\":\"AstroBot kann Dinge tragen\",\"ar\":\"يستطيع AstroBot حمل الأشياء\",\"pl\":\"AstroBot potrafi przenosić rzeczy\",\"it\":\"AstroBot può trasportare cose\",\"pt\":\"O AstroBot consegue carregar objetos\",\"en\":\"AstroBot can carry things\",\"zh-hans\":\"探星1号会搬运\",\"ru\":\"AstroBot может носить вещи\",\"es\":\"AstroBot puede cargar cosas\",\"zh-hant\":\"探星1號會搬運\",\"ko\":\"AstroBot이 물건을 나를 수 있습니다.\"}},{\"blocklyType\":\"newBlockly\",\"blocklyVersion\":\"3.0.0.5\",\"isDefault\":true,\"modelId\":0,\"xmlId\":\"171206182803989425\",\"xmlName\":\"怒摔百宝箱\",\"xmlNameLang\":{\"ja\":\"怒って宝箱を叩きつける\",\"da\":\"Sætter vredt skattekisten ned\",\"tr\":\"Öfkeyle hazine sandığını düşürür\",\"fr\":\"Lâche méchamment le coffre au trésor\",\"de\":\"Lässt die Schutztruhe wütend fallen\",\"ar\":\"يُلقي صندوق الكنز بغضب\",\"pl\":\"Upuszcza skrzynię skarbów ze złością\",\"it\":\"Fa cadere con rabbia la cassa del tesoro\",\"pt\":\"Enraivecido, deixa cair a arca do tesouro\",\"en\":\"Angrily drops treasure chest\",\"zh-hans\":\"怒摔百宝箱\",\"ru\":\"Злостно роняет сундук с сокровищами\",\"es\":\"Tira el cofre del tesoro con enfado\",\"zh-hant\":\"怒摔百寶箱\",\"ko\":\"보물 상자를 거칠게 내려놓습니다.\"}}]";
            PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, json);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    JsonData data = new JsonData(Json.Deserialize(json));

                    for (int i = 0, imax = data.Count; i < imax; ++i)
                    {
                        Dictionary<string, object> dict = (Dictionary<string, object>)data[i].Dictionary;
                        if (null != dict)
                        {
                            AddProgram(dict);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
            }
        }
    }

    private void AddProgram(Dictionary<string, object> data)
    {
        string xmlId = null;
        bool isDefault = false;
        string xmlName = null;
        if (data.ContainsKey("xmlId"))
        {
            xmlId = data["xmlId"].ToString();
        }
        if (data.ContainsKey("xmlName"))
        {
            xmlName = data["xmlName"].ToString();
        }
        if (data.ContainsKey("isDefault"))
        {
            bool.TryParse(data["isDefault"].ToString(), out isDefault);
        }
        if (!string.IsNullOrEmpty(xmlId))
        {
            if (null == mProgramDict)
            {
                mProgramDict = new Dictionary<string, Dictionary<string, object>>();
            }
            mProgramDict[xmlId] = data;
            GameObject item = GameObject.Instantiate(mItem) as GameObject;
            item.SetActive(true);
            item.name = Item_Start + xmlId;
            Transform itemTrans = item.transform;
            itemTrans.parent = mScrollViewGrid;
            itemTrans.localPosition = new Vector3(0, 1000);
            itemTrans.localScale = Vector3.one;
            Transform officialIcon = itemTrans.Find("officialIcon");
            if (null != officialIcon)
            {
                if (!isDefault)
                {
                    officialIcon.gameObject.SetActive(false);
                }
            }
            UILabel name = GameHelper.FindChildComponent<UILabel>(itemTrans, "name");
            if (null != name)
            {
                name.text = xmlName;
            }
        }

    }

    void ResetQueuePosition(Transform queueTrans, bool instant, bool withinBounds = false, UIScrollView scrollView = null)
    {
        if (null != queueTrans)
        {
            int index = 0;
            for (int i = 0, imax = queueTrans.childCount; i < imax; ++i)
            {
                GameObject obj = queueTrans.GetChild(i).gameObject;
                if (obj.activeSelf)
                {
                    Vector3 targetPos = new Vector3(0, - (1 + mItemSize.y / 2 + index * (mItemSize.y)));
                    if (instant)
                    {
                        obj.transform.localPosition = targetPos;
                    } else
                    {
                        SpringPosition.Begin(obj, targetPos, 8);
                    }
                    ++index;
                }
            }
            if (withinBounds && null != scrollView)
            {
                scrollView.RestrictWithinBounds(false);
            }
        }
    }

    void RemoveItem(Transform item)
    {
        item.parent = null;
        GameObject.Destroy(item.gameObject);
        ResetQueuePosition(mScrollViewGrid, false);
    }

    bool IsPanelMove()
    {
        if (null != mScrollView && null == mSpringPanel)
        {
            mSpringPanel = mScrollView.transform.GetComponent<SpringPanel>();
        }
        if (null != mSpringPanel)
        {
            return mSpringPanel.enabled;
        }
        return false;
    }

    void SetEditActionModel()
    {
        /*GameObject oriGO = GameObject.Find("oriGO");
        if (oriGO != null)
        {
            MoveSecond.Instance.ResetParent();
            MoveSecond.Instance.ResetDJDPPA();
            GameObject.DontDestroyOnLoad(oriGO);
        }*/
    }

    void PlayActionsDelegate(int index, bool finished)
    {
        if (finished)
        {
            mActionState = ActionRunState.None;
            mAction = null;
        }
    }

    public void HideUI()
    {
        /*if (null != mTrans)
        {
            Transform right = mTrans.FindChild("right");
            if (null != right)
            {
                Vector2 rightPos = new Vector2(right.localPosition.x + PublicFunction.GetWidth() * 0.4f, right.localPosition.y);
                GameHelper.PlayTweenPosition(right, rightPos);
            }
        }*/    
    }

    private void GotoProgram(string xmlId)
    {
        if (null != mRobot)
        {
            try
            {
                RobotMgr.Instance.GoToCommunity();
            }
            catch (System.Exception ex)
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, ex.ToString());
            }
            if (ResourcesEx.GetRobotType(mRobot) == ResFileType.Type_default)
            {
            }
            HideUI();
            SceneMgr.EnterScene(SceneType.EmptyScene);

            Timer.Add(0.2f, 1, 1, delegate ()
            {
                if (!string.IsNullOrEmpty(xmlId) && null != mProgramDict && mProgramDict.ContainsKey(xmlId))
                {
                    SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(mRobot, mProgramDict[xmlId]);
                } else
                {
                    SingletonObject<LogicCtrl>.GetInst().OpenLogicForRobot(mRobot);
                }
            });
        }
    }

    void SetChoiceRobot(EventArg arg)
    {
        mRobot = RobotManager.GetInst().GetCurrentRobot();
        RefreshMenuList(false);
    }

    void RefreshList(EventArg arg)
    {
        RefreshMenuList(false);
    }
}

public enum MainMenuType : byte
{
    Action_Menu = 0,
    Program_Menu,
}

public enum ActionRunState : byte
{
    None = 0,
    Run,
    Pause,
}
