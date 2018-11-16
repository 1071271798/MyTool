using Game.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using Game;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:ActionSequence.cs
/// Description:
/// Time:2015/6/26 13:02:49
/// </summary>
public class ActionSequence
{
    #region 公有属性
    /// <summary>
    /// 动作名字
    /// </summary>
    public virtual string Name
    {
        get 
        { 
            if (null == showName)
            {
                return name;
            }
            else
            {
                if (showName.ContainsKey(LauguageTool.GetIns().CurLauguage))
                {
                    if (string.IsNullOrEmpty(showName[LauguageTool.GetIns().CurLauguage]))
                    {
                        if (showName.ContainsKey(LauguageType.English))
                        {
                            return showName[LauguageType.English];
                        }
                    }
                    return showName[LauguageTool.GetIns().CurLauguage];
                }
                else if (showName.ContainsKey(LauguageType.English))
                {
                    return showName[LauguageType.English];
                }
                return name; 
            }
        }
        set
        {
            if (string.IsNullOrEmpty(oldName) && !name.Equals(value))
            {
                oldName = name;
            }
            if (null != showName && showName.ContainsKey(LauguageTool.GetIns().CurLauguage))
            {
                showName[LauguageTool.GetIns().CurLauguage] = value;
            }
            name = value;
        }
    }

    public string ActionName
    {
        get { return name; }
    }
    /// <summary>
    /// 对应的机器人id
    /// </summary>
    public string RobotID
    {
        get { return robotID; }
    }
    /// <summary>
    /// 动作id
    /// </summary>
    public string Id
    {
        get { return id; }
    }

    public int Count
    {
        get 
        {
            if (null != mActions)
            {
                return mActions.Count;
            }
            return 0;
        } 
    }
    /// <summary>
    /// 是否需要保存
    /// </summary>
    public bool NeedSave
    {
        get { return needSave; }
        set { needSave = value; }
    }
    /// <summary>
    /// 动作图标的ID
    /// </summary>
    public string IconID
    {
        get 
        { 
            return icon; 
        }
        set { icon = value; }
    }
    /// <summary>
    /// 动作图片的名字
    /// </summary>
    public virtual string IconName
    {
        get
        {
            return ActionsManager.GetInst().GetActionIconName(icon);
        }
    }
    /// <summary>
    /// 创建时间
    /// </summary>
    public long createTime = 0;

    public float playSpeed = PublicFunction.Default_Play_Speed;
    #endregion

    #region 私有属性
    //showName="0$拉拉;1$lala"
    protected Dictionary<LauguageType, string> showName = null;
    protected string name;
    protected string robotID;
    protected string id;
    protected List<Action> mActions;
    protected bool needSave;//是否需要保存
    protected string icon;
    protected string oldName = null;
    protected bool isOfficial = false;
    #endregion

    #region 公有函数
    public ActionSequence(string robotId)
    {
        name = string.Empty;
        id = CreateID.CreateActionsID();
        robotID = robotId;
        mActions = new List<Action>();
        needSave = false;
        icon = string.Empty;
        createTime = 0;
    }

    public ActionSequence(string name, string robotId, string actionsId, string icon, string showName, bool isofficial)
    {
        this.name = name;
        id = actionsId;
        robotID = robotId;
        mActions = new List<Action>();
        needSave = false;
        this.icon = icon;
        this.isOfficial = isofficial;
        createTime = 0;
        try
        {
            if (!string.IsNullOrEmpty(showName))
            {
                string[] lgstr = showName.Split(';');
                if (null != lgstr)
                {
                    for (int i = 0, imax = lgstr.Length; i < imax; ++i)
                    {
                        string[] nameStr = lgstr[i].Split('$');
                        if (null != nameStr && nameStr.Length == 2)
                        {
                            if (null == this.showName)
                            {
                                this.showName = new Dictionary<LauguageType, string>();
                            }
                            LauguageType lgtype = (LauguageType)int.Parse(nameStr[0]);
                            if (lgtype == LauguageType.Arab)
                            {
                                this.showName[lgtype] = LauguageTool.ConvertArab(nameStr[1]);
                            }
                            else
                            {
                                this.showName[lgtype] = nameStr[1];
                            }
                            
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public ActionSequence(ActionSequence act, Robot robot)
    {
        this.name = act.name;
        this.id = act.Id;
        this.robotID = act.robotID;
        mActions = new List<Action>();
        needSave = act.needSave;
        this.icon = act.icon;
        createTime = act.createTime;
        isOfficial = act.isOfficial;
        for (int i = 0, icount = act.Count; i < icount; ++i)
        {
            Action tmp = new Action();
            tmp.Copy(act[i], robot);
            tmp.index = act[i].index;
            mActions.Add(tmp);
        }
        if (null != act.showName)
        {
            showName = new Dictionary<LauguageType, string>();
            foreach (KeyValuePair<LauguageType, string> kvp in act.showName)
            {
                showName.Add(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// 重新生成动作ID
    /// </summary>
    public void ReCreateID()
    {
        this.id = CreateID.CreateActionsID();
    }

    public Action this[int index]
    {
        get
        {
            if (null != mActions && index >= 0 && index < mActions.Count)
            {
                return mActions[index];
            }
            return null;
        }
        
    }
    /// <summary>
    /// 判断是否是轮模式
    /// </summary>
    /// <returns></returns>
    public bool IsTurnModel()
    {
        int lastIndex = mActions.Count - 1;
        if (lastIndex < 0)
        {
            return false;
        }
        return mActions[lastIndex].IsTrunModel();
    }

    public void AddAction(Action action)
    {
        mActions.Add(action);
    }

    public void InsertAction(int index, Action action)
    {
        if (index < mActions.Count)
        {
            mActions.Insert(index, action);
        }
    }

    public void DelAction(Action action)
    {
        mActions.Remove(action);
    }

    public void ClearActions()
    {
        mActions.Clear();
    }
    /// <summary>
    /// 获取动作列表
    /// </summary>
    /// <returns></returns>
    public List<Action> GetActions()
    {
        return mActions;
    }
    /// <summary>
    /// 修改舵机id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="targetId"></param>
    public void SwitchDuoJiId(byte id, byte targetId)
    {
        try
        {
            for (int i = 0, icount = mActions.Count; i < icount; ++i)
            {
                mActions[i].SwitchDuoJiId(id, targetId);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        //Save();
    }
    /// <summary>
    /// 获取该动作的所有时间
    /// </summary>
    /// <returns>毫秒</returns>
    public int GetAllTime()
    {
        int times = 0;
        for (int i = 0, icount = mActions.Count; i < icount; ++i)
        {
            times += mActions[i].AllTime;
        }
        return times;
    }

    public void Save()
    {
        try
        {
            Robot robot = RobotManager.GetInst().GetRobotForID(robotID);
            if (null != robot)
            {
                string path = ResourcesEx.GetActionFilePath(robot.Name, id);
                bool addFileFlag = true;
                if (File.Exists(path))
                {
                    addFileFlag = false;
                }
                //新建xml实例
                XmlDocument xmlDoc = new XmlDocument();
                //创建根节点，最上层节点
                XmlElement roots = xmlDoc.CreateElement("Roots");
                xmlDoc.AppendChild(roots);
                //添加头
                XmlElement root = xmlDoc.CreateElement("Root");
                roots.AppendChild(root);
                root.SetAttribute("nodeType", "head");
                root.SetAttribute("name", name);
                if (null != showName && showName.Count > 0)
                {
                    string tmpShowName = string.Empty;
                    foreach (KeyValuePair<LauguageType, string> kvp in showName)
                    {
                        if (!string.IsNullOrEmpty(tmpShowName))
                        {
                            tmpShowName += ";";
                        }
                        tmpShowName += (int)kvp.Key + "$" + kvp.Value;
                    }
                    root.SetAttribute("showName", tmpShowName);
                }
                root.SetAttribute("robotID", robotID);
                root.SetAttribute("id", id);
                root.SetAttribute("icon", icon);
                if (0 == createTime && addFileFlag)
                {//不存在
                    createTime = DateTime.Now.Ticks;
                }
                root.SetAttribute("createTime", createTime.ToString());

                //添加内容
                for (int i = 0, icount = mActions.Count; i < icount; ++i)
                {
                    XmlElement node = xmlDoc.CreateElement("Root");
                    node.SetAttribute("nodeType", "body");
                    node = mActions[i].ConvertToNode(node);
                    roots.AppendChild(node);
                }
                //将xml文件保存到本地
                try
                {
                    xmlDoc.Save(path);
                    PlatformMgr.Instance.Log(MyLogType.LogTypeEvent, "保存动作:" + path);
                    if (addFileFlag)
                    {
                        PlatformMgr.Instance.OperateSyncFile(robot.Name, path, OperateFileType.Operate_File_Add);
                    }
                    else
                    {
                        PlatformMgr.Instance.OperateSyncFile(robot.Name, path, OperateFileType.Operate_File_Change);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                    PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                }
                
                if (!string.IsNullOrEmpty(oldName))
                {
                    /*string oldNamePath = ResourcesEx.GetActionsPath(robotName, oldName, type);
                    if (File.Exists(oldNamePath))
                    {
                        File.Delete(oldNamePath);
                    }*/
                    oldName = null;
                }
            }
            
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
        needSave = false;
    }
    //删除文件
    public void DeleteFile()
    {
        try
        {
            Robot robot = RobotManager.GetInst().GetRobotForID(robotID);
            if (null != robot)
            {
                string path = ResourcesEx.GetActionFilePath(robot.Name, name);
                if (!File.Exists(path))
                {
                    path = ResourcesEx.GetActionFilePath(robot.Name, id);
                }
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                        PlatformMgr.Instance.OperateSyncFile(robot.Name, path, OperateFileType.Operate_File_Del);
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                        PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                    }
                    
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    /// <summary>
    /// 是否是同名
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsSameName(string name)
    {
        if (null != showName)
        {
            foreach (KeyValuePair<LauguageType, string> kvp in showName)
            {
                if (name.Equals(kvp.Value))
                {
                    return true;
                }
            }
        }
        if (name.Equals(this.name))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 辅助判断是否是官方动作
    /// </summary>
    /// <returns></returns>
    public bool IsOfficial()
    {
        return isOfficial;
        /*Robot robot = RobotManager.GetInst().GetRobotForID(robotID);
        if (null != robot && robot.Name.EndsWith("_playerdata"))
        {
            return false;
        }
        if (null == showName && !name.Equals(PublicFunction.Default_Actions_Name))
        {
            return false;
        }
        return true;*/
    }

    public void SetOfficial(bool isOfficial)
    {
        this.isOfficial = isOfficial;
    }
    #endregion

    #region 私有函数

    #endregion
}