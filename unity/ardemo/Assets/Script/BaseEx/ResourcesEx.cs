using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Game.Platform;
using System.Text;

namespace Game.Resource
{
    
    public static class ResourcesEx
    {
        static readonly string[] ResFileTypePath = new string[3] { "default", "playerdata", "download"};

        public readonly static string persistentDataPath = Application.persistentDataPath;

        public readonly static string streamingAssetsPath = Application.streamingAssetsPath;
        
        public static ResFileType GetResFileType(string typeStr)
        {
            if (typeStr.Equals("default"))
            {
                return ResFileType.Type_default;
            }
            else if (typeStr.Equals("playerdata"))
            {
                return ResFileType.Type_playerdata;
            }
            else if (typeStr.Equals("download"))
            {
                return ResFileType.Type_download;
            }
            return ResFileType.Type_playerdata;
        }

        public static string GetFileTypeString(ResFileType type)
        {
            int index = (int)type;
            if (index < 0 || index >= ResFileTypePath.Length)
            {
                return ResFileTypePath[0];
            }
            return ResFileTypePath[index];
        }
        /// <summary>
        /// 获取模型类型
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public static ResFileType GetRobotType(Robot robot)
        {
            if (robot.Name.EndsWith("_default"))
            {
                return ResFileType.Type_default;
            }
            else
            {
                return ResFileType.Type_playerdata;
            }
        }
       
        /// <summary>
        /// 获得动作路径
        /// </summary>
        /// <returns></returns>
        static string GetActionsFloderPath(string robotName)
        {
            string path = PublicFunction.CombinePath(GetRobotPath(robotName), "actions");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        /// <summary>
        /// 获取公用动作的路径
        /// </summary>
        /// <param name="robotName"></param>
        /// <returns></returns>
        static string GetActionsFloderCommonPath(string robotName)
        {
            string path = PublicFunction.CombinePath(GetRobotCommonPath(robotName), "actions");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        static string GetMovesFloderPath(string robotName)
        {
            string path = PublicFunction.CombinePath(GetRobotPath(robotName), "moves");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        static string GetMovesFloderCommonPath(string robotName)
        {
            string path = PublicFunction.CombinePath(GetRobotCommonPath(robotName), "moves");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        /// <summary>
        /// 获得动作的完整路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetActionFilePath(string robotName, string fileName)
        {
            return PublicFunction.CombinePath(GetActionsFloderPath(robotName), fileName) + ".xml";
        }
        /// <summary>
        /// 获取公用动作的路径
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetActionFileCommonPath(string robotName, string fileName)
        {
            return PublicFunction.CombinePath(GetActionsFloderCommonPath(robotName), fileName) + ".xml";
        }

        /// <summary>
        /// 获取某个模型的动作列表
        /// </summary>
        /// <param name="robotName">完整的名字</param>
        /// <returns></returns>
        public static List<string> GetRobotActionsPath(Robot robot)
        {
            try
            {
                List<string> list = new List<string>();
                string userPath = GetActionsFloderPath(robot.Name);
                if (Directory.Exists(userPath))
                {
                    string[] files = Directory.GetFiles(userPath);
                    if (null != files)
                    {
                        list.AddRange(files);
                    }
                }
                ResFileType robotType = ResourcesEx.GetRobotType(robot);
                if (ResFileType.Type_default == robotType)
                {
                    string commonPath = GetActionsFloderCommonPath(robot.Name);

                    if (Directory.Exists(commonPath))
                    {
                        string[] files = Directory.GetFiles(commonPath);
                        if (null != files)
                        {
                            list.AddRange(files);
                        }
                    }
                }

                return list;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                return null;
            }
        }

        public static List<string> GetRobotMovesPath(Robot robot)
        {
            try
            {
                List<string> list = new List<string>();
                string userPath = GetMovesFloderPath(robot.Name);
                if (Directory.Exists(userPath))
                {
                    string[] files = Directory.GetFiles(userPath);
                    if (null != files)
                    {
                        list.AddRange(files);
                    }
                }
                ResFileType robotType = ResourcesEx.GetRobotType(robot);
                if (ResFileType.Type_default == robotType)
                {
                    string commonPath = GetMovesFloderCommonPath(robot.Name);

                    if (Directory.Exists(commonPath))
                    {
                        string[] files = Directory.GetFiles(commonPath);
                        if (null != files)
                        {
                            list.AddRange(files);
                        }
                    }
                }

                return list;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 官方模型公用数据
        /// </summary>
        /// <param name="robotName"></param>
        /// <returns></returns>
        public static string GetRobotCommonPath(string robotName)
        {
            string robotType = RobotMgr.DataType(robotName);
            string name = RobotMgr.NameNoType(robotName);
            return PublicFunction.CombinePath(persistentDataPath, robotType, name);
        }

        public static string GetCommonPathForNoTypeName(string robotName)
        {
            return PublicFunction.CombinePath(persistentDataPath, "default", robotName);
        }
        /// <summary>
        /// 获取官方或者个人用户数据的路径
        /// </summary>
        /// <param name="robotName"></param>
        /// <returns></returns>
        public static string GetRobotPath(string robotName)
        {
            string robotType = RobotMgr.DataType(robotName);
            string name = RobotMgr.NameNoType(robotName);
            string path = PublicFunction.CombinePath(persistentDataPath, "users", GetUserFloder(), robotType, name);
            return path;
        }

        public static string GetRobotPathForNoTypeName(string robotName)
        {
            return PublicFunction.CombinePath(persistentDataPath, "users", GetUserFloder(), "playerdata", robotName);
        }
        /// <summary>
        /// 获取用户的根目录
        /// </summary>
        /// <returns></returns>
        public static string GetUserRootPath()
        {
            string path = PublicFunction.CombinePath(persistentDataPath, "users", GetUserFloder());
            return path;
        }

        public static string GetCommonRootPath()
        {
            string path = PublicFunction.CombinePath(persistentDataPath, "default");
            return path;
        }

        private static string GetUserFloder()
        {
            string str = PlatformMgr.Instance.GetUserData();
            if (string.IsNullOrEmpty(str))
            {
                str = "local";
            }
            return str;
        }

        //获取文件名
        private static string GetFileName(string path)
        {
            int index = path.LastIndexOf(".");
            return index >= 0 ? path.Remove(index) : path;
        }

        public static T Load<T>(string path)
    where T : UnityEngine.Object
        {
            string newPath = GetFileName(path);
            try
            {
                return Resources.Load<T>(newPath);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static UnityEngine.Object Load(string path)
        {
            string newPath = GetFileName(path);
            try
            {
                return Resources.Load(newPath);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public enum XmlNodeType
    {
        head,
        body
    }

    public enum ResFileType
    {
        Type_default = 0,
        Type_playerdata,
        Type_download
    }
}
/// <summary>
/// 操作文件类型
/// </summary>
public enum OperateFileType : byte
{
    Operate_File_Add = 1,
    Operate_File_Change = 2,
    Operate_File_Del = 3,
}
