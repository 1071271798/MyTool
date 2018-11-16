using UnityEngine;
using System.Collections;

public class ModifyControlView : MonoBehaviour {

    public Transform leftTrans;  //左面板
    public Transform topTrans;   //上面板
    public Transform bottomTrans;  //下面板
    public Transform confirmBtn;   //确认按钮
    public Transform deletaBtn;    //删除按钮
    public Transform settingBtn;   //设置按钮
    public Transform stopBtn;      //停止按钮
    public Transform testBtn;
    public Transform backhomeBtn;  //返回主页按钮 
    public Transform backsecondBtn; //返回上级页面按钮
    public Transform lineTrans;    //线框
    public Transform exploreTrans; //侧栏伸缩按钮
    public Transform cancelLeftBtn;    //侧栏缩回按钮
    public Transform cancelBottomBtn;  //底部缩回按钮

    private bool _isSetting;
    public bool IsSetting
    {
        get
        {
            return _isSetting;
        }
        set
        {
            _isSetting = value;
            if (_isSetting) //设置界面
            {
                ShowOrHideTopBoard(true);
                ShowOrHideLeftboard(true);
                exploreTrans.gameObject.SetActive(true);
            }
            else  //遥控页面
            {
                ShowOrHideLines(false);
                ShowOrHideTopBoard(false);
                ShowOrHideLeftboard(false);
                exploreTrans.gameObject.SetActive(false);
            }
        }
    }

	// Use this for initialization
	void Start () {
        InitEnvironment();	
	}
	
	// Update is called once per frame
	void Update () {

    }

    #region  //fuction
    /// <summary>
    /// 返回
    /// </summary>
    /// <param name="go"></param>
    void OnBackbtnClicked(GameObject go)
    { 
        
    }
    void OnSecondbackClicked(GameObject go)
    {
        IsSetting = false;
    }
    void OnConfirmClicked(GameObject go)
    {
        IsSetting = false;
    }
    void OnCancelleftClicked(GameObject go)
    {
        ShowOrHideLeftboard(false);
    }
    void OnCancelbottomClicked(GameObject go)
    {
        ShowOrHideBottmBoard(false);
    }
    int testC = 0;
    void OnTestClicked(GameObject go)
    {
        testC++;
        bool f = testC % 2 == 0 ? true : false;
        ShowOrHideBottmBoard(f);
    }
    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="go"></param>
    void OnSettingbtnClicked(GameObject go)
    {
        IsSetting = true;
    }
    /// <summary>
    /// 伸缩左侧面板
    /// </summary>
    /// <param name="go"></param>
    void OnExplorebtnClicked(GameObject go)
    {
        ShowOrHideLeftboard(!isLeftShow);
    }
    /// <summary>
    /// 网格显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    void ShowOrHideLines(bool isshow)
    {
        if (lineTrans != null)
            lineTrans.gameObject.SetActive(isshow);
    }
    /// <summary>
    /// 左侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isLeftShow = false;
    void ShowOrHideLeftboard(bool isshow)
    {
        if (leftTrans == null || isshow == isLeftShow)
            return;
        isLeftShow = isshow;
        HideOrShowTrans(isshow, leftTrans, directType.left);
        if (cancelLeftBtn != null)
            cancelLeftBtn.GetComponent<BoxCollider>().enabled = isshow;
    }
    /// <summary>
    /// 上侧面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isTopShow = false;
    void ShowOrHideTopBoard(bool isshow)
    {
        if (topTrans == null || isshow == isTopShow)
            return;
        isTopShow = isshow;
        HideOrShowTrans(isshow, topTrans, directType.top);
    }
    /// <summary>
    /// 底部面板显示与隐藏
    /// </summary>
    /// <param name="isshow"></param>
    bool isBottomShow = false;
    void ShowOrHideBottmBoard(bool isshow)
    {
        if (bottomTrans == null || isshow == isBottomShow)
            return;
        isBottomShow = isshow;
        HideOrShowTrans(isshow, bottomTrans, directType.bottom);
        if (cancelBottomBtn != null)
            cancelBottomBtn.GetComponent<BoxCollider>().enabled = isshow;
    }

    void InitEnvironment()
    {
        if (settingBtn != null)
            UIEventListener.Get(settingBtn.gameObject).onClick = OnSettingbtnClicked;
        if (confirmBtn != null)
            UIEventListener.Get(confirmBtn.gameObject).onClick = OnConfirmClicked;
        if (backsecondBtn != null)
            UIEventListener.Get(backsecondBtn.gameObject).onClick = OnSecondbackClicked;
        if (cancelLeftBtn != null)
            UIEventListener.Get(cancelLeftBtn.gameObject).onClick = OnCancelleftClicked;
        if (cancelBottomBtn != null)
            UIEventListener.Get(cancelBottomBtn.gameObject).onClick = OnCancelbottomClicked;
        if (testBtn != null)
            UIEventListener.Get(testBtn.gameObject).onClick = OnTestClicked;
        if (exploreTrans != null)
            UIEventListener.Get(exploreTrans.gameObject).onClick = OnExplorebtnClicked;

        //控件面板初始化
        
    }
    #endregion

    void HideOrShowTrans(bool isShow,Transform trans, directType type , float time = 0.5f, EventDelegate.Callback call = null)
    {
        TweenPosition tp = null;
        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.zero;
        if (trans != null)
        {
            Transform nullTran = null;
            UIWidget temWidget = trans.GetComponentInChildren<UIWidget>();
            temWidget.SetAnchor(nullTran);
            int hh = temWidget.height;
            int ww = temWidget.width;
            hh -= 40;
            ww -= 40;
            MyAnimtionCurve cur1 = new MyAnimtionCurve(MyAnimtionCurve.animationCurveType.position);
            if (trans.GetComponent<TweenPosition>() == null)
            {
                trans.gameObject.AddComponent<TweenPosition>();
                trans.GetComponent<TweenPosition>().from = trans.localPosition;
                if(type == directType.bottom)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y + hh, trans.localPosition.z);
                else if(type == directType.top)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x, trans.localPosition.y - hh, trans.localPosition.z);
                else if(type == directType.left)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x + ww, trans.localPosition.y, trans.localPosition.z);
                else if(type == directType.right)
                    trans.GetComponent<TweenPosition>().to = new Vector3(trans.localPosition.x - ww, trans.localPosition.y, trans.localPosition.z);
                trans.GetComponent<TweenPosition>().animationCurve = cur1.animCurve;
            }
            tp = trans.GetComponent<TweenPosition>();
            tp.duration = time;

            TweenPosition tween = null;
            if (isShow)
            {
                tp.PlayForward();
                if (tween != null)
                    tween.PlayForward();
            }
            else
            {
                tp.PlayReverse();
                if (tween != null)
                    tween.PlayReverse();
            }
        }
    }
    /// <summary>
    /// 方向枚举
    /// </summary>
    enum directType
    { 
        left,
        right,
        top,
        bottom,
    }
}
