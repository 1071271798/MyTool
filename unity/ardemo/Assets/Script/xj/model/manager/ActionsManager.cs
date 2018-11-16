using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Game.Resource;
using System.Xml;
using System.Text;
using Game.Platform;

/// <summary>
/// Author:xj
/// FileName:ActionsManager.cs
/// Description:
/// Time:2015/7/8 13:33:56
/// </summary>
public class ActionsManager
{
    #region 公有属性
    #endregion

    #region 私有属性
    static ActionsManager mInst = null;
    /// <summary>
    /// Dictionary<机器人id, Dictionary<动作id, ActionSequence>>
    /// </summary>
    Dictionary<string, Dictionary<string, ActionSequence>> mAllActions = null;
    /// <summary>
    /// 所有机器人的默认动作
    /// </summary>
    Dictionary<string, ActionSequence> mAllDefaultActions = null;
    /// <summary>
    /// 动作图标
    /// </summary>
    ActionIconList mActionIconList = null;
    /// <summary>
    /// 图标id，图标信息
    /// </summary>
    Dictionary<string, ActionIcon> mActionIconDict = null;
    /// <summary>
    /// 官方动作
    /// </summary>
    //Dictionary<string, int> mOfficialActions = null;
    #endregion

    #region 公有函数
    public static ActionsManager GetInst()
    {
        if (null == mInst)
        {
            mInst = new ActionsManager();
        }
        return mInst;
    }

    public void CleanUp()
    {
        if (null != mAllActions)
        {
            mAllActions.Clear();
        }
        if (null != mAllDefaultActions)
        {
            mAllDefaultActions.Clear();
        }
    }
    /// <summary>
    /// 读取某个模型的动作
    /// </summary>
    /// <param name="robotName"></param>
    /// <param name="type"></param>
    public void ReadActions(Robot robot)
    {
        try
        {
            mAllActions.Remove(robot.ID);
            List<string> files = ResourcesEx.GetRobotActionsPath(robot);
            if (null != files && files.Count > 0)
            {
                files.Sort(delegate (string a, string b)
                {
                    FileInfo aFileInfo = new FileInfo(a);
                    FileInfo bFileInfo = new FileInfo(b);
                    if (aFileInfo.CreationTimeUtc.Ticks > bFileInfo.CreationTimeUtc.Ticks)
                    {
                        return 1;
                    }
                    else if (aFileInfo.CreationTimeUtc.Ticks < bFileInfo.CreationTimeUtc.Ticks)
                    {
                        return -1;
                    }
                    return 0;
                });
                for (int i = 0, icount = files.Count; i < icount; ++i)
                {
                    ActionSequence actions = ReadXml(files[i], robot);
                    if (null != actions)
                    {
                        AddRobotActions(actions.RobotID, actions);
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
    /// 获取动作信息
    /// </summary>
    /// <param name="robotId">机器人id</param>
    /// <param name="actionId">动作id</param>
    /// <returns></returns>
    public ActionSequence GetActionForID(string robotId, string actionId)
    {
        if (mAllActions.ContainsKey(robotId) && mAllActions[robotId].ContainsKey(actionId))
        {
            return mAllActions[robotId][actionId];
        }
        if (mAllDefaultActions.ContainsKey(robotId) && mAllDefaultActions[robotId].Id.Equals(actionId))
        {
            return mAllDefaultActions[robotId];
        }
        return null;
    }
    /// <summary>
    /// 获取动作信息
    /// </summary>
    /// <param name="robotId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public ActionSequence GetActionForName(string robotId, string name)
    {
        if (mAllDefaultActions.ContainsKey(robotId) && name.Equals(mAllDefaultActions[robotId].Name))
        {//默认动作
            return mAllDefaultActions[robotId];
        }
        else if (mAllActions.ContainsKey(robotId))
        {
            foreach (KeyValuePair<string, ActionSequence> kvp in mAllActions[robotId])
            {
                if (kvp.Value.Name.Equals(name))
                {
                    return kvp.Value;
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 通过机器人id获取所有动作
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public Dictionary<string, ActionSequence> GetRobotActions(string robotId)
    {
        Dictionary<string, ActionSequence> dict = null;
        if (mAllDefaultActions.ContainsKey(robotId))
        {
            if (null == dict)
            {
                dict = new Dictionary<string, ActionSequence>();
            }
            dict.Add(mAllDefaultActions[robotId].Id, mAllDefaultActions[robotId]);
        }
        if (mAllActions.ContainsKey(robotId))
        {
            if (null == dict)
            {
                dict = new Dictionary<string, ActionSequence>();
            }
            foreach (KeyValuePair<string, ActionSequence> kvp in mAllActions[robotId])
            {
                dict.Add(kvp.Key, kvp.Value);
            }
        }
        return dict;
    }
    /// <summary>
    /// 新增一个动作
    /// </summary>
    /// <param name="robId"></param>
    /// <param name="acts"></param>
    public void AddRobotActions(string robId, ActionSequence acts)
    {
        if (acts.ActionName.Equals(PublicFunction.Default_Actions_Name))
        {//默认动作
            mAllDefaultActions[robId] = acts;
            if (mAllActions.ContainsKey(robId) && mAllActions[robId].ContainsKey(acts.Id))
            {
                mAllActions[robId].Remove(acts.Id);
            }
        }
        else
        {
            if (mAllDefaultActions.ContainsKey(robId) && mAllDefaultActions[robId].Id.Equals(acts.Id))
            {
                return;
            }
            if (mAllActions.ContainsKey(robId))
            {
                if (mAllActions[robId].ContainsKey(acts.Id))
                {
                    /*Robot robot = RobotManager.GetInst().GetRobotForID(robId);
                    if (null != robot)
                    {//防止以前版本存在同id的动作文件
                        ResFileType type = ResourcesEx.GetResFileType(RobotMgr.DataType(robot.Name));
                        string robotName = RobotMgr.NameNoType(robot.Name);
                        string path = ResourcesEx.GetActionsPath(robotName, acts.Id, type);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }*/
                    mAllActions[robId][acts.Id] = acts;
                }
                else
                {
                    mAllActions[robId].Add(acts.Id, acts);
                }
            }
            else
            {
                Dictionary<string, ActionSequence> tmpDict = new Dictionary<string, ActionSequence>();
                tmpDict.Add(acts.Id, acts);
                mAllActions.Add(robId, tmpDict);
            }
        }
        
    }
    /// <summary>
    /// 删除机器人所有动作
    /// </summary>
    /// <param name="robId"></param>
    public void DeleteRobotAllActions(string robId)
    {
        if (mAllDefaultActions.ContainsKey(robId))
        {
            mAllDefaultActions[robId].DeleteFile();
            mAllDefaultActions.Remove(robId);
        }
        if (mAllActions.ContainsKey(robId))
        {
            Dictionary<string, ActionSequence> dict = mAllActions[robId];
            foreach (KeyValuePair<string, ActionSequence> kvp in dict)
            {
                kvp.Value.DeleteFile();
            }
            dict.Clear();
            mAllActions.Remove(robId);
        }
    }
    /// <summary>
    /// 删除某个动作
    /// </summary>
    /// <param name="robId"></param>
    /// <param name="actId"></param>
    public void DeleteRobotActions(string robId, string actId)
    {
        if (mAllDefaultActions.ContainsKey(robId) && mAllDefaultActions[robId].Id.Equals(actId))
        {
            mAllDefaultActions[robId].DeleteFile();
            mAllDefaultActions.Remove(robId);
        }
        if (mAllActions.ContainsKey(robId) && mAllActions[robId].ContainsKey(actId))
        {
            mAllActions[robId][actId].DeleteFile();
            mAllActions[robId].Remove(actId);
        }
    }
    /// <summary>
    /// 获取机器人的某个动作的所有时间
    /// </summary>
    /// <param name="robId">机器人id</param>
    /// <param name="actsId">动作</param>
    /// <returns>总时间，毫秒</returns>
    public int GetActionsTimeForID(string robId, string actsId)
    {
        if (mAllActions.ContainsKey(robId) && mAllActions[robId].ContainsKey(actsId))
        {
            return mAllActions[robId][actsId].GetAllTime();
        }
        if (mAllDefaultActions.ContainsKey(robId) && mAllDefaultActions[robId].Id.Equals(actsId))
        {
            return mAllDefaultActions[robId].GetAllTime();
        }
        return 0;
    }
    /// <summary>
    /// 通过动作名字获取动作时长
    /// </summary>
    /// <param name="robId"></param>
    /// <param name="actsName"></param>
    /// <returns></returns>
    public int GetActionsTimeForName(string robId, string actsName)
    {
        if (mAllDefaultActions.ContainsKey(robId) && actsName.Equals(mAllDefaultActions[robId].Name))
        {
            return mAllDefaultActions[robId].GetAllTime();
        }
        if (mAllActions.ContainsKey(robId))
        {
            Dictionary<string, ActionSequence> acts = mAllActions[robId];
            foreach (KeyValuePair<string, ActionSequence> kvp in acts)
            {
                if (0 == kvp.Value.Name.CompareTo(actsName))
                {
                    return kvp.Value.GetAllTime();
                }
            }
        }
        return 0;
    }



    /// <summary>
    /// 检查动作名字
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ErrorCode CheckActionsName(string robotId, string name, string actId)
    {
        ErrorCode ret = ErrorCode.Result_OK;
        do 
        {
            if (string.IsNullOrEmpty(name))
            {
                ret = ErrorCode.Result_Name_Empty;
                break;
            }
            StringBuilder sb = new StringBuilder(name);
            int emptyNum = 0;
            for (int i = 0, imax = sb.Length; i < imax; ++i)
            {
                if (sb[i] != ' ')
                {
                    break;
                }
                else
                {
                    ++emptyNum;
                }
            }
            if (emptyNum == sb.Length)
            {//全是空
                ret = ErrorCode.Result_Name_Empty;
                break;
            }
            /*if (name.Equals(PublicFunction.Default_Actions_Name) || name.Equals(PublicFunction.Default_Actions_Name_CN))
            {//默认名字
                if (mAllDefaultActions.ContainsKey(robotId) && !mAllDefaultActions[robotId].Id.Equals(actId))
                {
                    ret = ErrorCode.Result_Name_Exist;
                    break;
                }
            }
            else */if (mAllActions.ContainsKey(robotId))
            {
                Dictionary<string, ActionSequence> dict = mAllActions[robotId];
                if (!string.IsNullOrEmpty(actId) && dict.ContainsKey(actId) && dict[actId].Name.Equals(name))
                {//原动作修改且未改名字则不需要检测重名
                    break;
                }
                foreach (KeyValuePair<string, ActionSequence> kvp in dict)
                {
                    if (kvp.Value.IsSameName(name))
                    {
                        ret = ErrorCode.Result_Name_Exist;
                        break;
                    }
                }
                if (ErrorCode.Result_Name_Exist == ret)
                {//已存在同名动作
                    break;
                }
            }
        } while (false);
        return ret;
    }
    /// <summary>
    /// 获取机器人动作的Icon列表的使用情况
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public Dictionary<string, int> GetRobotActionsIconIdDict(string robotId)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        if (mAllDefaultActions.ContainsKey(robotId))
        {
            if (!string.IsNullOrEmpty(mAllDefaultActions[robotId].IconID))
            {
                if (dict.ContainsKey(mAllDefaultActions[robotId].IconID))
                {
                    ++dict[mAllDefaultActions[robotId].IconID];
                }
                else
                {
                    dict[mAllDefaultActions[robotId].IconID] = 1;
                }
            }
        }
        if (mAllActions.ContainsKey(robotId))
        {
            foreach (KeyValuePair<string, ActionSequence> kvp in mAllActions[robotId])
            {
                if (!string.IsNullOrEmpty(kvp.Value.IconID))
                {
                    if (dict.ContainsKey(kvp.Value.IconID))
                    {
                        ++dict[kvp.Value.IconID];
                    }
                    else
                    {
                        dict[kvp.Value.IconID] = 1;
                    }
                }
            }
        }
        return dict;
    }
    
    public string GetNewActionsIconID(string robotId)
    {
        List<ActionIcon> actionIconList = mActionIconList.iconList;
        Dictionary<string, int> iconDict = GetRobotActionsIconIdDict(robotId);
        for (int i = 0, imax = actionIconList.Count; i < imax; ++i)
        {
            if (actionIconList[i].useCount > 0 && (!iconDict.ContainsKey(actionIconList[i].id) || iconDict[actionIconList[i].id] < actionIconList[i].useCount))
            {
                return actionIconList[i].id;
            }
        }
        return string.Empty;
    }
    /// <summary>
    /// 获取动作id列表
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public List<string> GetActionsIDList(string robotId)
    {
        List<string> list = new List<string>();
        if (mAllActions.ContainsKey(robotId))
        {
            foreach (KeyValuePair<string, ActionSequence> kvp in mAllActions[robotId])
            {
                list.Add(kvp.Key);
            }
            list.Sort(new ActionsComparer(mAllActions[robotId]));
            /*for (int i = 0, imax = list.Count; i < imax; ++i)
            {
                Debuger.Log(string.Format("actions list name = {0}, createTime = {1}", mAllActions[robotId][list[i]].Name, mAllActions[robotId][list[i]].createTime));
            }*/
            if (mAllDefaultActions.ContainsKey(robotId))
            {
                if (list.Count > 0)
                {
                    list.Insert(0, mAllDefaultActions[robotId].Id);
                }
                else
                {
                    list.Add(mAllDefaultActions[robotId].Id);
                }
            }
        }
        else
        {
            if (mAllDefaultActions.ContainsKey(robotId))
            {
                if (list.Count > 0)
                {
                    list.Insert(0, mAllDefaultActions[robotId].Id);
                }
                else
                {
                    list.Add(mAllDefaultActions[robotId].Id);
                }
            }
        }
        return list;
    }
    /// <summary>
    /// 获取机器人动作的名字列表
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public List<string> GetActionsNameList(string robotId)
    {
        List<string> list = new List<string>();
        List<string> idList = GetActionsIDList(robotId);
        for (int i = 0, imax = idList.Count; i < imax; ++i)
        {
            ActionSequence asq = GetActionForID(robotId, idList[i]);
            if (null != asq)
            {
                list.Add(asq.Name);
            }
        }
        return list;
    }


    /// <summary>
    /// 通过id获取动作图标
    /// </summary>
    /// <param name="iconId"></param>
    /// <returns></returns>
    public ActionIcon GetActionIcon(string iconId)
    {
        if (mActionIconDict.ContainsKey(iconId))
        {
            return mActionIconDict[iconId];
        }
        return null;
    }
    /// <summary>
    /// 通过图标id获取动作图标的名字
    /// </summary>
    /// <param name="iconId"></param>
    /// <returns></returns>
    public string GetActionIconName(string iconId)
    {
        if (mActionIconDict.ContainsKey(iconId))
        {
            return mActionIconDict[iconId].iconName;
        }
        return PublicFunction.Default_Actions_Icon_Name;
    }
    /// <summary>
    /// 获取所有的动作图标
    /// </summary>
    /// <returns></returns>
    public List<ActionIcon> GetActionIconList()
    {
        return mActionIconList.iconList;
    }
    /// <summary>
    /// 是否有默认动作
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public bool HaveDefaultAction(string robotId)
    {
        if (mAllDefaultActions.ContainsKey(robotId))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 获取默认动作
    /// </summary>
    /// <param name="robotId"></param>
    /// <returns></returns>
    public ActionSequence GetDefaultAction(string robotId)
    {
        if (mAllDefaultActions.ContainsKey(robotId))
        {
            return mAllDefaultActions[robotId];
        }
        return null;
    }
    /// <summary>
    /// 创建默认动作
    /// </summary>
    /// <param name="robotId"></param>
    /// <param name="action"></param>
    public void CreateDefaultAction(string robotId, Action action)
    {
        action.sportTime = 1000;
        if (mAllDefaultActions.ContainsKey(robotId))
        {
            mAllDefaultActions[robotId].ClearActions();
            mAllDefaultActions[robotId].AddAction(action);
        }
        else
        {
            ActionSequence act = new ActionSequence(robotId);
            act.Name = PublicFunction.Default_Actions_Name;
            act.IconID = PublicFunction.Default_Actions_Icon_ID;
            act.AddAction(action);
            mAllDefaultActions[robotId] = act;
        }
        mAllDefaultActions[robotId].Save();
    }
    /// <summary>
    /// 添加官方动作id
    /// </summary>
    /// <param name="actId"></param>
    /*public void AddOfficial(string actId)
    {
        mOfficialActions[actId] = 1;
    }*/
    /// <summary>
    /// 判断动作是否是官方动作
    /// </summary>
    /// <param name="actId"></param>
    /// <returns></returns>
    /*public bool IsOfficial(string actId)
    {
        if (mOfficialActions.ContainsKey(actId))
        {
            return true;
        }
        return false;
    }*/

    /*public void SaveOfficialActions()
    {
        try
        {
            string path = Path.Combine(ResourcesEx.persistentDataPath, "officialActions.xml");
            
            //新建xml实例
            XmlDocument xmlDoc = new XmlDocument();
            //创建根节点，最上层节点
            XmlElement roots = xmlDoc.CreateElement("Roots");
            xmlDoc.AppendChild(roots);
            
            foreach (KeyValuePair<string, int> kvp in mOfficialActions)
            {
                XmlElement node = xmlDoc.CreateElement("Root");
                node.SetAttribute("actid", kvp.Key);
                roots.AppendChild(node);
            }
            //将xml文件保存到本地
            xmlDoc.Save(path);
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }*/

    #endregion

    #region 私有函数
    private ActionsManager()
    {
        mAllActions = new Dictionary<string, Dictionary<string, ActionSequence>>();
        mAllDefaultActions = new Dictionary<string, ActionSequence>();
        //mOfficialActions = new Dictionary<string, int>();
        //ReadAllActions();
        mActionIconList = new ActionIconList();
        mActionIconList.LoadXml();
        mActionIconDict = new Dictionary<string, ActionIcon>();
        ActionIcon tmp;
        for (int i = 0, imax = mActionIconList.iconList.Count; i < imax; ++i)
        {
            tmp = mActionIconList.iconList[i];
            mActionIconDict[tmp.id] = tmp;
        }
        //ReadOfficialActionsXml();
    }

    /*public void ReadOfficialActionsXml()
    {
        try
        {
            string path = Path.Combine(ResourcesEx.persistentDataPath, "officialActions.xml");
            if (!File.Exists(path))
            {//文件不存在
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            XmlNode node = xmlRoot.FirstChild;
            mOfficialActions.Clear();
            while (null != node)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                XmlElement xe = (XmlElement)node;
                string actid = xe.GetAttribute("actid");
                AddOfficial(actid);
                node = node.NextSibling;
            }
            xmlDoc = null;
        }
        catch (System.Exception ex)
        {
            if (ClientMain.Exception_Log_Flag)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                Debuger.LogError(this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }
    }*/



    public static ActionSequence ReadXml(string path, Robot robot)
    {
        try
        {
            if (!File.Exists(path))
            {//文件不存在
                return null;
            }
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            XmlNode node = xmlRoot.FirstChild;
            if (null == node)
            {
                return null;
            }
            ActionSequence actions = null;
            bool isOfficial = false;
            if (path.StartsWith(ResourcesEx.GetRobotCommonPath(robot.Name)))
            {
                isOfficial = true;
            }
            while (null != node)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                XmlElement xe = (XmlElement)node;
                string nodeType = xe.GetAttribute("nodeType");

                if (nodeType.Equals(Game.Resource.XmlNodeType.head.ToString()))
                {
                    string name = xe.GetAttribute("name");
                    string robotID = robot.ID;// xe.GetAttribute("robotID");
                    string id = xe.GetAttribute("id");
                    string icon = xe.GetAttribute("icon");
                    string create = xe.GetAttribute("createTime");
                    string showName = xe.GetAttribute("showName");
                    actions = new ActionSequence(name, robotID, id, icon, showName, isOfficial);
                    if (string.IsNullOrEmpty(create))
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        if (null != fileInfo)
                        {
                            actions.createTime = fileInfo.CreationTimeUtc.Ticks;
                        }
                    }
                    else
                    {
                        actions.createTime = long.Parse(create);
                    }
                }
                else
                {
                    Action ac = new Action(xe, robot);
                    if (null != ac)
                    {
                        if (null == ac.GetShowID())
                        {//兼容以前的老版本
                            if (actions.Count == 0)
                            {
                                byte minId = byte.MaxValue;
                                foreach (KeyValuePair<byte, short> kvp in ac.rotas)
                                {
                                    DuoJiData tmpData = robot.GetAnDjData(kvp.Key);
                                    if (null != tmpData && tmpData.modelType == ServoModel.Servo_Model_Angle)
                                    {
                                        if (kvp.Value != PublicFunction.DuoJi_Start_Rota)
                                        {
                                            ac.AddShowID(kvp.Key);
                                        }
                                        if (kvp.Key < minId)
                                        {
                                            minId = kvp.Key;
                                        }
                                    }
                                    
                                }
                                if ((ac.GetShowID() == null || ac.GetShowID().Count == 0) && minId != byte.MaxValue)
                                {
                                    ac.AddShowID(minId);
                                }
                            }
                            else
                            {
                                Action lastAct = actions[actions.Count - 1];
                                foreach (KeyValuePair<byte, short> kvp in ac.rotas)
                                {
                                    DuoJiData tmpData = robot.GetAnDjData(kvp.Key);
                                    if (null != tmpData && tmpData.modelType == ServoModel.Servo_Model_Angle)
                                    {
                                        if (lastAct.GetRota(kvp.Key) != kvp.Value)
                                        {
                                            ac.AddShowID(kvp.Key);
                                        }
                                    }
                                }
                                if (null == ac.GetShowID() || ac.GetShowID().Count == 0)
                                {
                                    List<byte> lastShow = lastAct.GetShowID();
                                    if (null != lastShow)
                                    {
                                        for (int i = 0, imax = lastShow.Count; i < imax; ++i)
                                        {
                                            ac.AddShowID(lastShow[i]);
                                        }
                                    }
                                }
                            }

                        }
                        actions.AddAction(ac);
                    }
                }
                node = node.NextSibling;
            }
            return actions;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo,"ActionsManager -" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        return null;
    }
    #endregion


    public class ActionsComparer : IComparer<string>
    {
        private int mTimeAsc = 1;    //根据时间升序排列
        private Dictionary<string, ActionSequence> actionDict;
        public ActionsComparer(Dictionary<string, ActionSequence> actions)
        {
            actionDict = actions;
        }

        /*传入参数1表示升序，-1降序，正数为升序，负数为降序，传1/-1统一*/
        /*sort默认是升序，即Compare()返回小于0表示x小于y，x排前面*/
        public ActionsComparer(Dictionary<string, ActionSequence> actions, int timeAsc)
        {
            if (timeAsc < 0)
            {
                mTimeAsc = -1;
            }
            else
            {
                mTimeAsc = 1;
            }
            actionDict = actions;
        }

        /*负值表示<（降序）,正值表示>（升序），0表示=*/
        public int Compare(string x, string y)
        {
            if (actionDict[x].IsOfficial() && !actionDict[y].IsOfficial())
            {
                return -mTimeAsc;
            }
            if (!actionDict[x].IsOfficial() && actionDict[y].IsOfficial())
            {
                return mTimeAsc;
            }
            if (actionDict[x].createTime < actionDict[y].createTime)
            {
                return -mTimeAsc;
            }
            else if (actionDict[x].createTime > actionDict[y].createTime)
            {
                return mTimeAsc;
            }
            return 0;
        }
    }
}