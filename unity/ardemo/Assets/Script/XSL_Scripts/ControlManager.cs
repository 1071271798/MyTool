using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MyMVC;
using MyData;

public class ControlManager : MonoBehaviour
{
    /*
    #region 文件保存
    private string XmlFilePath;
	public bool isChange = false;
    #endregion

    #region   界面元素
    public GameObject windowsEdit;  //编辑窗口
	public GameObject windowsRun;   //运行窗口
	public GameObject windowsEdit_none;  //编辑状态下默认窗口
	public GameObject windowsRun_none; //运行状态下默认窗口
	public GameObject secondEditWindow; // 编辑状态的二级窗口
	public Transform modelGrid;        //模型列
	public Transform animGrid_run;     //run窗口下的动作列
	public Transform animGrid_edit;    //edit窗口下的动作列
	public Transform animList_run;     //run窗口下的动作序列
	public Transform animList_edit;    //edit窗口下的动作序列
	private windowID curSubWindow;  //当前运行的子窗口
	private windowID preSubWindow;  // 前一个运行的子窗口
    private Color normalColor = Color.white; 
    private Color highColor = Color.yellow;   //选中的颜色
    #endregion

    #region    界面按钮
    //Button
	public GameObject OnRunWindow_btn;   //运行
	public GameObject OnEditWindow_btn;  //编辑
	public GameObject OnAddAnim_btn;     //添加动作
	public GameObject OnImportModel_btn;  //导入模型
	public GameObject OnPause_btn;        // 暂停(已取消)
	public GameObject OnStop_btn;         // 停止（已取消）
	public GameObject OnPlayList_btn;     // 播放列表按钮
	public GameObject OnConnect_btn;      //连接与断开
	public GameObject OnSetting_btn;      //设置
    public GameObject OnReturn_btn;      //返回主界面
    #endregion

    #region 维护当前的界面数据
	private RunState _curRunState;
	private RunState CurRunState  // 
	{
		get
		{
			return _curRunState;
		}
		set
		{
			_curRunState = value;
			switch(_curRunState)
			{
			case RunState.Run:
				if(CurOperateAnim != null)
					CurOperateAnim.transform.GetChild(2).GetComponent<UISprite>().spriteName = "pause_ico";
				OnPlayList_btn.GetComponent<UISprite>().spriteName = "play_ico";
				break;
			case RunState.notRun:
				if(CurOperateAnim != null)
					CurOperateAnim.transform.GetChild(2).GetComponent<UISprite>().spriteName = "play_ico";
				OnPlayList_btn.GetComponent<UISprite>().spriteName = "play_ico";
				break;
			case RunState.Run_list:
				if(CurOperateAnim != null)
					CurOperateAnim.transform.GetChild(2).GetComponent<UISprite>().spriteName = "play_ico";
				break;
			case RunState.Pause_list:
				if(CurOperateAnim != null)
					CurOperateAnim.transform.GetChild(2).GetComponent<UISprite>().spriteName = "play_ico";
				break;
			case RunState.Pause:
				break;
			}
		}
	}

	private GameObject _curOperateModel;  //当前正在操作的model
	public GameObject CurOperateModel   //当前模型改变时 自动更新对应的动作列表
	{
		get
		{
			return _curOperateModel;
		}
		set
		{
            if(_curOperateModel != null)
                _curOperateModel.GetComponent<UISprite>().color = normalColor;

			if(isChange)   // 当前模型切换时 如果模型数据被改变 则重新保存
            {
                SavaObjToXml(_curOperateModel, XmlFilePath);
				isChange = false;
			}
            if (value == _curOperateModel || value == null) //更换值无效时 
            {
                return;
            }
            //
			_curOperateModel = value;

            // 更换模型时 当前机器人跟着改变
            isConnect = false; // 断开连接
            string robotID = _curOperateModel.GetComponent<ModelCell>().robotID;
            CurRobot = RobotManager.GetInst().GetRobot(robotID);

			// 是否先从本地获取数据 填充数据结构
			string modelName = _curOperateModel.transform.GetChild(0).GetComponent<UILabel>().text;
			ModelCell curModelcell = _curOperateModel.GetComponent<ModelCell>();
            curModelcell.CellData.modelID = curModelcell.robotID;
			if(CurRobot != null)
				curModelcell.CellData.totalActionList = CurRobot.GetActionsNameList();  //当前机器的人的可用动作导入到celldata中                                                                                                                                                                                           
			UpdateAnims();

			UpdateSignals();
            _curOperateModel.GetComponent<UISprite>().color = highColor;
		}
	}
	private GameObject _curOperateAnim;    //当前操作的动作
	public GameObject CurOperateAnim       //当前操作对象被改变时 对应的动作编辑窗口也相应被改
	{
		get
		{
			return _curOperateAnim;
		}
		set
		{
			_curOperateAnim = value;
			UpdateEditAnims();
		}
	}

    public Robot CurRobot;
    //信号
    private bool _isConnect;
    private bool isConnect
    {
        get
        {
            return _isConnect;
        }
        set
        {
            _isConnect = value;
            if (_isConnect)
            { 
                //如果连接成功
            }
        }
    }
    
    #endregion 

    /// <summary>
	/// 单例
	/// </summary>
	/// <returns>The instance.</returns>
	private static ControlManager _instance;
	static public ControlManager GetInstance()
	{
		if (_instance == null) {
						return null;
				} else 
						return _instance;
	}

    #region unity 回调函数
    void Awake()
	{
		_instance = this;

	}
	// Use this for initialization
	void Start () {

        preSubWindow = windowID.none;
       // GameObject.Find("Camera").transform.localEulerAngles = new Vector3(0, 0, 90);
        XmlFilePath = Application.persistentDataPath;
		InitEnvironment ();
	}

	// Update is called once per frame
	void Update () {
	
		CheckWindowChange ();  //检测窗口

        if (Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Escape)))
        {
            Application.Quit();
        }
        if (Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Home)))
        {
            Application.Quit();
        }
	}
	void OnDestory()
	{
		if (isChange) {
			SavaObjToXml(_curOperateModel, XmlFilePath);
			isChange = false;		
		}

		_instance = null;
	}
    #endregion

    #region 界面之间的切换
    void ReturnMainWindow(GameObject go)
    {
        if(isChange)
            SavaObjToXml(_curOperateModel, XmlFilePath);
        MainWindow.LoadLevel("mainScene");
    }
    /// <summary>
    /// 检测窗口改变
    /// </summary>
    void CheckWindowChange()
    {
        if (preSubWindow != curSubWindow)
        {
            SwitchWindow(curSubWindow);
            preSubWindow = curSubWindow;
        }
    }
    /// <summary>
    /// 根据窗口id配置对应窗口内容
    /// </summary>
    /// <param name="id">Identifier.</param>
    void SwitchWindow(windowID id)
    {
        HideAnimWindow();
        switch (id)
        {
            case windowID.run_window:
                if (windowsRun != null)
                {
                    windowsRun.SetActive(true);
                    UpdateAnims();
                }
                else
                    Debug.Log("null object of windowsRun");
                break;
            case windowID.edit_window:
                if (windowsEdit != null)
                {
                    windowsEdit.SetActive(true);
                    UpdateAnims();
                }
                else
                    Debug.Log("null object of windowsEdit");
                break;
            case windowID.run_none:
                if (windowsRun_none != null)
                    windowsRun_none.SetActive(true);
                else
                    Debug.Log("null object of windowsRun_none");
                break;
            case windowID.edit_none:
                if (windowsEdit_none != null)
                    windowsEdit_none.SetActive(true);
                else
                    Debug.Log("null object of windowsEdit_none");
                break;
            case windowID.none:
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 切换运行动作界面
    /// </summary>
    void DoRunWindowSwitch(GameObject oo)
    {
        if (CurOperateModel != null)
            curSubWindow = windowID.run_window;
        else
            curSubWindow = windowID.run_none;
    }
    /// <summary>
    /// 切换编辑动作界面
    /// </summary>
    void DoEditWindowSwitch(GameObject oo)
    {
        if (CurOperateModel != null)
            curSubWindow = windowID.edit_window;
        else
            curSubWindow = windowID.edit_none;
    }

    #endregion

    #region 播放控制
    /// <summary>
	/// 暂停
	/// </summary>
	void DoPause(GameObject oo)
	{
		CurRunState = RunState.Pause;
		//根据运行状态更改小图标 三角形 或者两竖
		Debug.Log ("PauseButton clicked");
	}
	/// <summary>
	/// 播放
	/// </summary>
	void DoPlay()
	{
		CurRunState = RunState.Run;
		//根据运行状态更改小图标 三角形 或者两竖
		Debug.Log ("playButton Clicked");
	}
	/// <summary>
	/// 结束播放时被调用
	/// </summary>
	void OnOverPlay()
	{
		CurRunState = RunState.notRun;
	}
	/// <summary>
	/// 停止
	/// </summary>
	void DoStop(GameObject oo)
	{
		CurRunState = RunState.notRun;
		//根据运行状态更改小图标 三角形 或者两竖
		Debug.Log ("StopButton clicked");
	}
    #endregion

    /// <summary>
	/// 模型导入接口
	/// </summary>
	public void DoImportModel(GameObject oo)
	{
		// test data
		GameObject modelItem = Resources.Load ("Prefab/UI/modelitem_x") as GameObject;
		GameObject temp = GameObject.Instantiate (modelItem) as GameObject;
		temp.transform.parent = modelGrid;
		temp.transform.localScale = Vector3.one;
		UIEventListener.Get (temp).onClick = DoSelectModel;
		modelGrid.GetComponent<UIGrid> ().repositionNow = true;
	} 

    #region 动作单元接口
    /// <summary>
    /// 点击动作
    /// </summary>
    public void DoTouchAnim(GameObject obj)
    {
        if (CurOperateAnim != null && CurOperateAnim != obj) //如果点击其它动作时 
        {
            if (CurRunState == RunState.Run)  //如果上一个动作未结束
            {
                CurRunState = RunState.notRun;
                // 结束操作 ---------------------------------
            }
            if (curSubWindow == windowID.run_window)
                CurOperateAnim.transform.GetChild(2).GetComponent<UISprite>().spriteName = "play_ico";
        }
        CurOperateAnim = obj;
        switch (curSubWindow)
        {
            case windowID.run_window:

                PlayAnim(obj);
                break;
            case windowID.edit_window:
              //  Debug.Log("编辑动作");
                EditAnim(obj);
                break;
            case windowID.run_none:
                break;
            case windowID.edit_none:
                break;
            default:
                break;
        }
    }
    /// <summary>
	/// 新加动作
	/// </summary>
	public void DoAddAnim(GameObject oo)
	{
        BindComponentsToAction(animGrid_edit, CreatNewAnim(animGrid_edit, new IdAndName("-1", "-1")));        
        if (CurOperateModel != null)
        {

        }
		SortAnimGrid (animGrid_edit);
	}
	/// <summary>
	/// 创建新的anim
	/// </summary>
	/// <param name="parent">Parent.</param>
	Transform CreatNewAnim(Transform parent)
	{
		GameObject animItem = Resources.Load ("Prefab/UI/animItem_x") as GameObject;
		if(curSubWindow == windowID.run_window)
			animItem = Resources.Load ("Prefab/UI/animItem_xx") as GameObject;
		GameObject temp = GameObject.Instantiate (animItem) as GameObject;
		UIEventListener.Get (temp).onClick = DoTouchAnim;
		temp.transform.parent = parent;
		temp.transform.localScale = Vector3.one;
		return temp.transform;
	}
	/// <summary>
	/// 创建新的带名称的anim
	/// </summary>
	/// <param name="parent">Parent.</param>
	/// <param name="name">Name.</param>
    Transform CreatNewAnim(Transform parent, IdAndName idn)
    {
        GameObject animItem = Resources.Load("Prefab/UI/actionItem") as GameObject;
        GameObject temp = GameObject.Instantiate(animItem) as GameObject;
        temp.transform.parent = parent;
        temp.transform.localScale = Vector3.one;
        temp.transform.GetChild(0).GetComponent<UILabel>().text = idn.ID_Name;
        temp.transform.GetChild(1).GetComponent<UILabel>().text = idn.ID;
        return temp.transform;
    }
	/// <summary>
	/// 移除动作 更新对应模型的数据
	/// </summary>
	public void RemoveAnim(GameObject obj)
	{
		string str = CurOperateAnim.transform.GetChild (1).GetComponent<UILabel> ().text;  //获取唯一标识符id
		CurOperateModel.GetComponent<ModelCell> ().CellData.RemoveInCurlist (str);
		GameObject.DestroyImmediate (CurOperateAnim);
		SortAnimGrid (animGrid_edit);
		ShowOrHideSecondWindow (false);
        isChange = true;
	}
	/// <summary>
	/// 编辑动作
	/// </summary>
	public void EditAnim(GameObject obj)
	{
		// 编辑动作窗口  动作窗口：显示当前动作名称，动作列表
		ActionEditWindow editW = secondEditWindow.GetComponent<ActionEditWindow> ();
		editW.actionNm.text = obj.transform.GetChild (0).GetComponent<UILabel> ().text;
		//editW.
		for (int i = 0; i< editW.canSelectActions.childCount; i++) {
			Destroy(editW.canSelectActions.GetChild(i).gameObject);		
		}
      //  List<string> actionList = CurRobot.GetActionsNameList();
		ModelCellData_x cell = CurOperateModel.GetComponent<ModelCell> ().CellData;
		foreach (var tem in cell.totalActionList) {
			if(!cell.curActionDic.ContainsKey(tem))   // 遍历获取到目前可用的动作列表
			{
				GameObject ob = Resources.Load("Prefab/UI/actionCell") as GameObject;
				GameObject oo = Instantiate(ob) as GameObject;
				oo.transform.parent = editW.canSelectActions;
				oo.transform.localScale = Vector3.one;
				oo.transform.GetChild(0).GetComponent<UILabel>().text = tem;   // 动作名称
			}
		}
		ShowOrHideSecondWindow (true);
		editW.canSelectActions.GetComponent<UIGrid> ().repositionNow = true;
	}
    /// <summary>
    /// 播放动作
    /// </summary>
    void PlayAnim(GameObject obj)
    {
        if (CurRunState != RunState.Run)
        {
            Debug.Log("播放被点击啦");
            CurRunState = RunState.Run;
            
            if (isConnect) //连接成功时 播放动作
            {
                string actionID = CurOperateAnim.transform.GetChild(1).GetComponent<UILabel>().text;
            //    CurRobot.PlayActionsForName(actionID);
                Debug.Log("我连接成功并且开始播放"+actionID+"动作啦");

                //CurRobot.PlayActionsForID(actionID);
                CurRobot.PlayActionsForName(actionID);
            }
        }
        else
        {
            Debug.Log("暂停或者结束被点击啦");
            CurRunState = RunState.notRun;

        }
    }
    /// <summary>
    /// 确认修改动作 更新modelCell中对应的数据， 有个问题，modelCell数据的关联性是否会被打断？ 名字与对应动作的关联？ 是否取别名？
    /// </summary>
    public void DoCertainChange(GameObject obj)
    {
        ActionEditWindow temp = secondEditWindow.GetComponent<ActionEditWindow>();
        //先修改界面数据 ，在更新内存数据 最后写入本地
        UILabel id = CurOperateAnim.transform.GetChild(1).GetComponent<UILabel>();
        UILabel name = CurOperateAnim.transform.GetChild(0).GetComponent<UILabel>();
        ModelCell curModelCell = CurOperateModel.GetComponent<ModelCell>();    //跟新数据到modelcell类中
        if (curModelCell.CellData.curActionDic.ContainsKey(id.text))   //如果是修改当前存在的 则先删掉 然后再存新的
        {
            curModelCell.CellData.curActionDic.Remove(id.text);
            isChange = true;
        }
        name.text = temp.actionNm.text;
        if (temp.curSelectAction != null)   //当前有选中
        {
            id.text = temp.curSelectAction.GetChild(0).GetComponent<UILabel>().text;    //弹出界面的内容赋值到当前操作的内容上

            if (id.text != "-1")   //如果是新增动作，初始化时的id都是-1 
            {   //如果修改过id  并且有效
                bool f = curModelCell.CellData.AddToCurList(new IdAndName(id.text, name.text));
                isChange = true;
            }
            else
            {
                Debug.Log("no changes occur");
                //给出提示		
            }
        }
        else
        {
            //没有选择动作时 
            Debug.Log("只修改了昵称");
            if (id.text == "-1" || id.text == "")  //修改昵称不成功的情况
            {
                DestroyImmediate(CurOperateAnim);
            }
            else
                isChange = true;
        }
        ShowOrHideSecondWindow(false);
    }
    /// <summary>
    /// 取消修改
    /// </summary>
    /// <param name="obj">Object.</param>
    public void DoCancelChange(GameObject obj)
    {
        ShowOrHideSecondWindow(false);
    }
    /// <summary>
    /// 对anim列表进行排序
    /// </summary>
    void SortAnimGrid(Transform parent)
    {
        UIGrid grid = parent.GetComponent<UIGrid>();
        if (grid != null)
        {
            grid.repositionNow = true;
        }
    }
    IEnumerator SortAtBack()
    {
    //    Debug.Log("sort ");
        yield return null;
        SortAnimGrid(animGrid_run);
        SortAnimGrid(animGrid_edit);
        SortAnimGrid(animList_run);
        SortAnimGrid(animList_edit);
    }
    #endregion

	/// <summary>
	/// 显示或隐藏二级动作编辑窗口
	/// </summary>
	/// <param name="f">If set to <c>true</c> f.</param>
	void ShowOrHideSecondWindow(bool f)
	{
		if (secondEditWindow != null) {
						secondEditWindow.SetActive (f);
				} else
						Debug.Log ("null objcet binded");
	}
	/// <summary>
	/// 选择模型
	/// </summary>
	public void DoSelectModel(GameObject oo)
	{
		// 根据oo找到对应的xml文件中的数据段 然后倒入到定义好的结构中
		//。。。

		CurOperateModel = oo;
		Debug.Log ("select model");
	}
	/// <summary>
	/// 连接机器人
	/// </summary>
	/// <param name="oo">Oo.</param>
	public void DoConnect(GameObject oo)
	{
		if (CurOperateModel != null) 
		{
			UISprite connectSprite = oo.GetComponent<UISprite>();
			UILabel connectLabel = oo.GetComponentInChildren<UILabel>();
			if(!isConnect)
			{
				isConnect = true;
				connectSprite.spriteName = "";  //显示连接图标
				connectLabel.text = "Connectted";
				// 连接
			}
			else
			{
				isConnect = false;
				connectSprite.spriteName = "";  //显示断开图标
				connectLabel.text = "DisConnectted";
				//断开
			}
		}
		else 
		{
			Debug.Log("请先选择一个要连接的机器人");	
		}
	}
	/// <summary>
	/// 设置
	/// </summary>
	public void DoSettting(GameObject oo)
	{
		Debug.Log ("do setting");
	}

	/// <summary>
	/// 动作列表的控制
	/// </summary>
	void PlayActionList(GameObject oo)
	{
		if (CurRunState != RunState.Run_list) {
						CurRunState = RunState.Run_list;
				}
		else 
			CurRunState = RunState.Pause_list;
		if (CurRunState == RunState.Run_list) {
			oo.GetComponent<UISprite> ().spriteName = "pause_ico";
				} else if (CurRunState == RunState.Pause_list) {
			oo.GetComponent<UISprite>().spriteName = "play_ico";		
		}
	}
	
	/// <summary>
	/// 更新所有的动作
	/// </summary>
	void UpdateAnims()
	{
		if (CurOperateModel != null)
        {
            #region 清空结点
            int count1 =count1 = animGrid_run.childCount;
			int count2 = animGrid_edit.childCount;
			int count3 = animList_run.childCount;
			int count4 = animList_edit.childCount;
			for(int i =0; i<count1; i++)   //这样删除会不会有问题？？？？？？？？？？？？？
			{
				Destroy(animGrid_run.GetChild(i).gameObject);
			}
			for(int i = 0; i<count2; i++)
			{
				Destroy(animGrid_edit.GetChild(i).gameObject);
			}
			for(int i = 0; i<count3; i++)
			{
				Destroy(animList_run.GetChild(i).gameObject);
			}
			for(int i = 0; i< count4; i++)
			{
				Destroy(animList_edit.GetChild(i).gameObject);
            }
            #endregion

            Dictionary<string,string> temDic = CurOperateModel.GetComponent<ModelCell>().CellData.curActionDic;   //当前模型的可用动作字典
			List<IdAndName> temList = CurOperateModel.GetComponent<ModelCell>().CellData.combineList;             //当前模型动作序列组合列表
			IdAndName tt = new IdAndName();

			foreach(var temKey in temDic.Keys)  //遍历动作字典 给runActions 和editActions集合下赋值
			{
				tt.ID = temKey;
				tt.ID_Name = temDic[temKey];
				if(tt.ID == "-1")
					continue;
				CreatNewAnim(animGrid_run,tt);   
				CreatNewAnim(animGrid_edit,tt);
			}
			foreach(var te in temList)  // 给动作序列赋值
			{
				CreatNewAnim(animList_run,te);
				CreatNewAnim(animList_edit,te);
			}
            BindActionCompents();
            // ------------------------------------ 生成结点数据 

            //if(curSubWindow == windowID.run_window)   //为animlist_run下结点 配置不同属性
            //{
            //    for(int i = 0; i< animList_run.childCount; i++)
            //    {
            //        Transform oo = animList_run.GetChild(i);
            //       // Debug.Log("ee");
            //        MyDragdropItem mydragItem = oo.GetComponent<MyDragdropItem>();

            //        if(mydragItem != null)
            //        {
            //            Debug.Log("uu");
            //            mydragItem.restriction = UIDragDropItem.Restriction.Vertical;
            //            mydragItem.cloneOnDrag = false;
            //            mydragItem.dragRemove = true;
            //        }
            //    }
            //}
			StartCoroutine(SortAtBack());
		}
	}

    void BindActionCompents()
    { 
        int n1 = animGrid_run.childCount;
        int n2 = animList_run.childCount;
        int n3 = animGrid_edit.childCount;
        int n4 = animList_edit.childCount;

        for (int i = 0; i < n1; i++)  // runActions
        { 
            GameObject obj = animGrid_run.GetChild(i).gameObject;
            if (obj.GetComponent<MyDragdropItem>() == null) obj.AddComponent<MyDragdropItem>();
            obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
            obj.GetComponent<MyDragdropItem>().restriction = UIDragDropItem.Restriction.PressAndHold;
            obj.GetComponent<MyDragdropItem>().cloneOnDrag = true;
            obj.GetComponent<MyDragdropItem>().dragRemove = false;
            UIEventListener.Get(obj).onClick = DoTouchAnim;
        }
        for (int i = 0; i < n2; i++)  //runListActions
        {
            GameObject obj = animList_run.GetChild(i).gameObject;
            if (obj.GetComponent<MyDragdropItem>() == null) obj.AddComponent<MyDragdropItem>();
            obj.GetComponent<MyDragdropItem>().restriction = UIDragDropItem.Restriction.Vertical;
            obj.GetComponent<MyDragdropItem>().cloneOnDrag = false;
            obj.GetComponent<MyDragdropItem>().dragRemove = true;
            obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
        }
        for (int i = 0; i < n3; i++) // editActions
        {
            GameObject obj = animGrid_edit.GetChild(i).gameObject;
            if(obj.GetComponent<EditDragDrop>() == null) obj.AddComponent<EditDragDrop>();
            obj.GetComponent<EditDragDrop>().cloneOnDrag = false;
            obj.GetComponent<EditDragDrop>().restriction = UIDragDropItem.Restriction.PressAndHold;
            obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
            UIEventListener.Get(obj).onClick = DoTouchAnim;
        }
        for (int i = 0; i < n4; i++)  //editListActions
        {
            GameObject obj = animList_edit.GetChild(i).gameObject;
            if (obj.GetComponent<EditDragDrop>() == null) obj.AddComponent<EditDragDrop>();
            obj.GetComponent<EditDragDrop>().restriction = UIDragDropItem.Restriction.Vertical;
            obj.GetComponent<EditDragDrop>().cloneOnDrag = false;
            obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
        }
    }
    /// <summary>
    /// 给特定的结点绑定对应的组件
    /// </summary>
    /// <param name="parent"></param>
    void BindActionCompent(Transform parent)
    {
        if (parent == animGrid_run)
        {
            int n1 = animGrid_run.childCount;
            for (int i = 0; i < n1; i++)  // runActions
            {
                GameObject obj = animGrid_run.GetChild(i).gameObject;
                if(obj.GetComponent<MyDragdropItem>() == null)  obj.AddComponent<MyDragdropItem>();
                obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
                obj.GetComponent<MyDragdropItem>().restriction = UIDragDropItem.Restriction.PressAndHold;  //按住然后拖动方式
                obj.GetComponent<UIDragDropItem>().cloneOnDrag = true;  //克隆
                obj.GetComponent<MyDragdropItem>().dragRemove = false;  //拖动到外时不删
                UIEventListener.Get(obj).onClick = DoTouchAnim;
            }
        }
        else if (parent == animList_run)
        {
            int n2 = animList_run.childCount;
            for (int i = 0; i < n2; i++)  //runListActions
            {
                GameObject obj = animList_run.GetChild(i).gameObject;
                if (obj.GetComponent<MyDragdropItem>() == null)  obj.AddComponent<MyDragdropItem>();
                obj.GetComponent<MyDragdropItem>().restriction = UIDragDropItem.Restriction.Vertical;  //竖直方向拖动
                obj.GetComponent<MyDragdropItem>().cloneOnDrag = false;   //非克隆方式
                obj.GetComponent<MyDragdropItem>().dragRemove = true;     //拖到外面时删除
                obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
            }
        }
        else if (parent == animGrid_edit)
        {
            int n3 = animGrid_edit.childCount;
            for (int i = 0; i < n3; i++) // editActions
            {
                GameObject obj = animGrid_edit.GetChild(i).gameObject;
                if (obj.GetComponent<EditDragDrop>() == null) obj.AddComponent<EditDragDrop>();
                obj.GetComponent<EditDragDrop>().cloneOnDrag = false;
                obj.GetComponent<EditDragDrop>().restriction = UIDragDropItem.Restriction.PressAndHold;
                obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
                UIEventListener.Get(obj).onClick = DoTouchAnim;
            }
        }
        else if (parent == animList_edit)
        {
            int n4 = animList_edit.childCount;
            for (int i = 0; i < n4; i++)  //editListActions
            {
                GameObject obj = animList_edit.GetChild(i).gameObject;
                if (obj.GetComponent<EditDragDrop>() == null) obj.AddComponent<EditDragDrop>();
                obj.GetComponent<EditDragDrop>().restriction = UIDragDropItem.Restriction.Vertical;
                obj.GetComponent<EditDragDrop>().cloneOnDrag = false;
                obj.GetComponent<UIDragScrollView>().scrollView = obj.transform.parent.GetComponent<UIScrollView>();
            }
        }
    }
    /// <summary>
    /// 给特action绑定对应的组件
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="actionCell"></param>
    public void BindComponentsToAction(Transform parent, Transform actionCell)
    {
        if (parent == animGrid_run)
        {
            if (actionCell.GetComponent<MyDragdropItem>() == null)  actionCell.gameObject.AddComponent<MyDragdropItem>();
            actionCell.GetComponent<MyDragdropItem>().restriction = UIDragDropItem.Restriction.PressAndHold;  //竖直方向拖动
            actionCell.GetComponent<MyDragdropItem>().cloneOnDrag = true;   //克隆方式
            actionCell.GetComponent<MyDragdropItem>().dragRemove = false;     //拖到外面时不删
            actionCell.GetComponent<UIDragScrollView>().scrollView = actionCell.parent.GetComponent<UIScrollView>();
            UIEventListener.Get(actionCell.gameObject).onClick = DoTouchAnim;
        }
        else if (parent == animList_run)
        {
            if (actionCell.GetComponent<MyDragdropItem>() == null)  actionCell.gameObject.AddComponent<MyDragdropItem>();
            actionCell.GetComponent<MyDragdropItem>().restriction = UIDragDropItem.Restriction.Vertical;  //竖直方向拖动
            actionCell.GetComponent<MyDragdropItem>().cloneOnDrag = false;   //非克隆方式
            actionCell.GetComponent<MyDragdropItem>().dragRemove = true;     //拖到外面时删除
            actionCell.GetComponent<UIDragScrollView>().scrollView = actionCell.parent.GetComponent<UIScrollView>();
        }
        else if (parent == animGrid_edit)
        {
            if (actionCell.GetComponent<EditDragDrop>() == null)  actionCell.gameObject.AddComponent<EditDragDrop>();
            actionCell.GetComponent<EditDragDrop>().restriction = UIDragDropItem.Restriction.PressAndHold;  //按住拖动
            actionCell.GetComponent<EditDragDrop>().cloneOnDrag = false;  //非克隆
            actionCell.GetComponent<UIDragScrollView>().scrollView = actionCell.parent.GetComponent<UIScrollView>();
            UIEventListener.Get(actionCell.gameObject).onClick = DoTouchAnim;
        }
        else if (parent == animList_edit)
        {
            if (actionCell.GetComponent<EditDragDrop>() == null)  actionCell.gameObject.AddComponent<EditDragDrop>();
            actionCell.GetComponent<EditDragDrop>().restriction = UIDragDropItem.Restriction.Vertical;
            actionCell.GetComponent<EditDragDrop>().cloneOnDrag = false;  //非克隆
            actionCell.GetComponent<UIDragScrollView>().scrollView = actionCell.transform.parent.GetComponent<UIScrollView>();
        }
    }
    /// <summary>
    /// 根据数据更新特定的grid显示 0,1,2,3 分别代表4个grid结点
    /// </summary>
    /// <param name="ID"></param>
    public void UpdateSubGrid(int ID)
    {
        Transform tt = null;
        List<IdAndName> tempList = new List<IdAndName>();
        if (ID == 0)  // 更新run anims 绑定mydragitem
        {
            tt = animGrid_run;
            Dictionary<string,string> temdic = CurOperateModel.GetComponent<ModelCell>().CellData.curActionDic;
            foreach (var temKey in temdic.Keys)
            {
                IdAndName idn = new IdAndName(temKey, temdic[temKey]);
                tempList.Add(idn);
            }
        }
        else if (ID == 1)  //更新run_list 绑定mydragitem 并且设置对应的值
        {
            tt = animList_run;
            tempList = CurOperateModel.GetComponent<ModelCell>().CellData.combineList;
        }
        else if (ID == 2)  // 更新edit_list 绑定
        {
            tt = animGrid_edit;
            Dictionary<string, string> temdic = CurOperateModel.GetComponent<ModelCell>().CellData.curActionDic;
            foreach (var temKey in temdic.Keys)
            {
                IdAndName idn = new IdAndName(temKey, temdic[temKey]);
                tempList.Add(idn);
            }
        }
        else if (ID == 3)
        {
            tt = animList_edit;
            tempList = CurOperateModel.GetComponent<ModelCell>().CellData.combineList;
        }
        if (tt != null)
        {
            for (int i = 0; i < tempList.Count; i++)
            {
                CreatNewAnim(tt, tempList[i]);
            }
        }

        if (curSubWindow == windowID.run_window)   //为animlist_run下结点 配置不同属性
        {
            for (int i = 0; i < animList_run.childCount; i++)
            {
                Transform oo = animList_run.GetChild(i);
                // Debug.Log("ee");
                MyDragdropItem mydragItem = oo.GetComponent<MyDragdropItem>();

                if (mydragItem != null)
                {
                    Debug.Log("uu");
                    mydragItem.restriction = UIDragDropItem.Restriction.Vertical;
                    mydragItem.cloneOnDrag = false;
                    mydragItem.dragRemove = true;
                }
            }
        }

        StartCoroutine(SortAtBack());
    }

    public void UpdateCurData(int ID)
    {
        StartCoroutine(UpdateCurDataNextFrame(ID));
    }
    /// <summary>
    /// 根据界面更新数据 延缓一帧执行 是因为很多删除操作是下一帧开始删除的 
    /// </summary>
    /// <param name="ID"></param>
    IEnumerator UpdateCurDataNextFrame(int ID)
    {
        yield return null;
        isChange = true;
        if (ID == -1) //更新所有数据
        { 
            
        }
        else if (ID == 0)
        {
            Dictionary<string,string> tt = CurOperateModel.GetComponent<ModelCell>().CellData.curActionDic;//.Clear();
            tt.Clear();
            int n = animGrid_run.childCount;
            for (int i = 0; i < n; i++)
            {
                IdAndName idn = GetAnimData(animGrid_run.GetChild(i));
                tt.Add(idn.ID, idn.ID_Name);
            }
        }
        else if (ID == 1)
        {
            List<IdAndName> tt = CurOperateModel.GetComponent<ModelCell>().CellData.combineList;
            tt.Clear();
            int n = animList_run.childCount;
            for (int i = 0; i < n; i++)
            {
                tt.Add(GetAnimData(animList_run.GetChild(i)));
            }
        }
        else if (ID == 2)
        {
            Dictionary<string, string> tt = CurOperateModel.GetComponent<ModelCell>().CellData.curActionDic;//.Clear();
            tt.Clear();
            int n = animGrid_edit.childCount;
            for (int i = 0; i < n; i++)
            {
                IdAndName idn = GetAnimData(animGrid_edit.GetChild(i));
                tt.Add(idn.ID, idn.ID_Name);
            }
        }
        else if (ID == 3)
        {
            List<IdAndName> tt = CurOperateModel.GetComponent<ModelCell>().CellData.combineList;
            tt.Clear();
            int n = animList_edit.childCount;
            for (int i = 0; i < n; i++)
            {
                tt.Add(GetAnimData( animList_edit.GetChild(i)));
            }
        }
    }

    IdAndName GetAnimData(Transform oo)
    {
        IdAndName tt = new IdAndName();
        try
        {
            tt.ID_Name = oo.GetChild(0).GetComponent<UILabel>().text;
            tt.ID = oo.GetChild(1).GetComponent<UILabel>().text;
        }
        catch (System.Exception)
        {
            throw;
        }
        return tt;
    }

	/// <summary>
	/// 更新模型依赖的信号
	/// </summary>
	void UpdateSignals()
	{
		isConnect = false;
			CurRunState = RunState.notRun;
		CurOperateAnim = null;
		isChange = false;
	}
	/// <summary>
	/// 更新编辑中的动作
	/// </summary>
	void UpdateEditAnims()
	{
		if (secondEditWindow != null) {
			if(secondEditWindow.GetComponent<ActionEditWindow>() != null)
			{

			}
		}
	}
	/// <summary>
	/// 隐藏animwindow
	/// </summary>
	void HideAnimWindow()
	{
		if (windowsEdit != null) {
			windowsEdit.SetActive(false);		
		}
		if (windowsRun != null) {
			windowsRun.SetActive(false);		
		}
		if (windowsEdit_none != null) {
			windowsEdit_none.SetActive(false);		
		}
		if (windowsRun_none != null) {
			windowsRun_none.SetActive(false);		
		}
	}

    #region 初始化环境
    /// <summary>
	/// 初始化
	/// </summary>
	void InitEnvironment()
	{
        InitModelTable();

		// 模型item绑定事件
        for (int i = 0; i < modelGrid.childCount; i++ )
        {
            UIEventListener.Get(modelGrid.GetChild(i).gameObject).onClick = DoSelectModel;
        }

		BindButton ();

		LoadAllObjsFromXml ();
	}
	/// <summary>
	/// 初始化模型列表  通过加载robotManager 获取到模型列表
	/// </summary>
	void InitModelTable()
	{
		//清空模型列表
		for (int i= 1; i<modelGrid.childCount; i++) {
			Destroy(modelGrid.GetChild(i).gameObject);
		}
		List<string> robotNameList = RobotManager.GetInst ().GetAllRobot ();
		if (robotNameList.Count != 0) 
		{
			for(int i = 0;i<robotNameList.Count; i++)
			{
                GameObject modelItem = Resources.Load("Prefab/UI/modelitem_x") as GameObject;
				GameObject oo = Instantiate (modelItem) as GameObject;
                oo.transform.parent = modelGrid;
                oo.transform.localScale = Vector3.one;
                ModelCell robotCell = oo.GetComponent<ModelCell>();
                Robot robot = RobotManager.GetInst().GetRobot(robotNameList[i]);  // 通过id获取robot数据 然后给数据赋值
                robotCell.robotID = robot.ID;
                robotCell.RobotNM = robot.Name;
                robotCell.CellData.totalActionList = robot.GetActionsNameList();
                // 常规的颜色
                oo.GetComponent<UISprite>().color = normalColor;
			}
            modelGrid.GetComponent<UIGrid>().repositionNow = true;
            CurOperateModel = modelGrid.GetChild(0).gameObject;
            curSubWindow = windowID.run_window;    //第一次进去时的窗口
		}
	}

	/// <summary>
	/// 绑定按钮
	/// </summary>
	void BindButton()
	{
		if (OnRunWindow_btn != null) 
		{
			UIEventListener.Get(OnRunWindow_btn).onClick = DoRunWindowSwitch;
		}
		if (OnEditWindow_btn != null) 
		{
			UIEventListener.Get(OnEditWindow_btn).onClick = DoEditWindowSwitch;
		}
		if (OnAddAnim_btn != null) 
		{
			UIEventListener.Get(OnAddAnim_btn).onClick = DoAddAnim;
		}
		if (OnPause_btn != null) 
		{
			UIEventListener.Get(OnPause_btn).onClick = DoPause;
		}
		if (OnStop_btn != null) 
		{
			UIEventListener.Get(OnStop_btn).onClick = DoStop;
		}
		if (OnImportModel_btn != null) 
		{
			UIEventListener.Get(OnImportModel_btn).onClick = DoImportModel;
		}
		if (OnPlayList_btn != null) 
		{
			UIEventListener.Get(OnPlayList_btn).onClick = PlayActionList;		
		}
		if (OnConnect_btn != null) 
		{
			UIEventListener.Get (OnConnect_btn).onClick = DoConnect;
		}
        if (OnReturn_btn != null)
        {
            UIEventListener.Get(OnReturn_btn).onClick = ReturnMainWindow;
        }
	}
    #endregion

    /// <summary>
	/// 检测为空的对象
	/// </summary>
	void CheckNull()
	{
		if (windowsEdit == null) {
			Debug.Log ("null object");
				}
		if (windowsRun == null) {
			Debug.Log("null object");		
		}

	}

    #region 数据存储
    /// <summary>
	/// 保存当前的模型到xml中
	/// </summary>
	/// <param name="path">Path.</param>
	public void SavaObjToXml(GameObject obj, string path)
	{
        if (obj == null)
            return;
        ModelCell modelCell = obj.GetComponent<ModelCell>();
		ModelCellData_x cell1 = obj.GetComponent<ModelCell> ().CellData;
		path += modelCell.robotID;
		path += ".xml";
		ModelCellSerialize cellSerialize = new ModelCellSerialize{RobotID = modelCell.robotID, RobotName = cell1.modelNm};
        // ModelCellData_x中不再需要保持可用动作列表
		foreach (var tem in cell1.curActionDic.Keys) {
          //  Debug.Log(tem + "--==");
			cellSerialize.ActionList.Add(new IdAndName(tem,cell1.curActionDic[tem]));
		}
		cellSerialize.ActionCombine = cell1.combineList;
		//cell1
		string str = XmlHelper.XmlSerialize (cellSerialize, Encoding.UTF8);

	//	Debug.Log (str);

		FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
		StreamWriter sw = new StreamWriter(fs);
		fs.SetLength(0);//首先把文件清空了。
		sw.Write(str);//写你的字符串。
		sw.Close();
	}

	/// <summary>
	/// 从xml中反序列化到对象中绑定的类
	/// </summary>
	/// <param name="path">Path.</param>
	public void LoadObjFromXml(GameObject obj, string path)
	{
        if (!File.Exists(path))
        {
            Debug.Log(path+"文件不存在");
            return;
        }
  //      Debug.Log("导入的文件名："+path);
		ModelCellSerialize cellSerialize = XmlHelper.XmlDeserializeFromFile<ModelCellSerialize> (path, Encoding.UTF8);
		ModelCell cell = obj.GetComponent<ModelCell> ();
		cell.name = cellSerialize.RobotName;
		cell.CellData.combineList = cellSerialize.ActionCombine;
		foreach (var item in cellSerialize.ActionList) {
			cell.CellData.curActionDic.Add(item.ID, item.ID_Name);
		}
	}

	/// <summary>
	/// 从本地导入xml到所有模型对象
	/// </summary>
	void LoadAllObjsFromXml()
	{
		for (int i = 0; i< modelGrid.childCount; i++) {
			GameObject oo = modelGrid.GetChild(i).gameObject;
            string fileName = oo.GetComponent<ModelCell>().robotID;
			fileName += ".xml";
			LoadObjFromXml(modelGrid.GetChild(i).gameObject, XmlFilePath+fileName);		
		}
    }
    #endregion
     
    */
}

/*
//窗口类型
enum windowID  
{
	run_window,        //运行界面的动作窗口
	edit_window,      //编辑界面的动作窗口
	run_none,        // 运行界面空窗口
	edit_none,      // 编辑界面空窗口
	none           //默认窗口 
}
//运行状态枚举
enum RunState
{
	notRun,
	Run,
	Run_list,
	Pause,
	Pause_list
}

*/