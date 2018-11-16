using Game.Platform;
using Game.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:ServosConManager.cs
/// Description:
/// Time:2016/4/5 16:26:10
/// </summary>
public class ServosConManager : SingletonObject<ServosConManager>
{
    #region 公有属性
    #endregion

    #region 其他属性
    Dictionary<string, ServosConnection> mRobotServosDict = null;
    #endregion

    #region 公有函数
    public ServosConManager()
    {
        mRobotServosDict = new Dictionary<string, ServosConnection>();
    }

    public ServosConnection GetServosConnection(string robotId)
    {
        if (mRobotServosDict.ContainsKey(robotId))
        {
            return mRobotServosDict[robotId];
        }
        return null;
    }

    public void UpdateServosConnection(string robotId, ServosConnection servosConnection)
    {
        mRobotServosDict[robotId] = servosConnection;
    }

    public ServosConnection CreateRobotConnection(Robot robot)
    {
        ServosConnection servosConnection = new ServosConnection();
        if (null != robot.MotherboardData)
        {
            servosConnection.MainboardType = robot.MotherboardData.GetMainboardType();
        } else
        {
            servosConnection.MainboardType = TopologyPartType.MainBoard;
        }
        List<byte> servosList = robot.GetAllDjData().GetIDList();
        TopologyPartData partData = null;
        for (int i = 0, imax = servosList.Count; i < imax; ++i)
        {
            partData = new TopologyPartData();
            partData.partType = TopologyPartType.Servo;
            partData.isIndependent = true;
            servosConnection.AddTopologyPartData(partData);
            DuoJiData data = robot.GetAllDjData().GetDjData(servosList[i]);
            if (null != data)
            {
                servosConnection.UpdateServoModel(data.id, data.modelType);
            }
        }
        if (null != robot.MotorsData)
        {
            List<byte> motors = robot.MotorsData.GetIds();
            if (null != motors && motors.Count > 0)
            {
                for (int i = 0, imax = motors.Count; i < imax; ++i)
                {
                    partData = new TopologyPartData();
                    partData.partType = TopologyPartType.Motor;
                    partData.isIndependent = true;
                    servosConnection.AddTopologyPartData(partData);
                }
            }
        }
        if (null != robot.MotherboardData)
        {
            TopologyPartType[] openSensor = PublicFunction.Open_Topology_Part_Type;
            SensorData sensorData = null;
            for (int i = 0, imax = openSensor.Length; i < imax; ++i)
            {
                sensorData = robot.MotherboardData.GetSensorData(openSensor[i]);
                if (null != sensorData && sensorData.ids.Count > 0)
                {
                    for (int sensorIndex = 0, sensorMax = sensorData.ids.Count; sensorIndex < sensorMax; ++sensorIndex)
                    {
                        partData = new TopologyPartData();
                        partData.partType = openSensor[i];
                        partData.isIndependent = true;
                        servosConnection.AddTopologyPartData(partData);
                    }
                }
            }
        }
        servosConnection.Save(robot);
        UpdateServosConnection(robot.ID, servosConnection);
        return servosConnection;
    }

    public void ReadServosConnection(Robot robot)
    {
        try
        {

            ResFileType robotType = ResourcesEx.GetRobotType(robot);
            string robotPath = string.Empty;
            robotPath = ResourcesEx.GetRobotPath(robot.Name);
            ServosConnection tmpServos = ServosConnection.CreateServos(robotPath);
            if (robotType == ResFileType.Type_default)
            {
                robotPath = ResourcesEx.GetRobotCommonPath(robot.Name);
                ServosConnection defaultCon = ServosConnection.CreateServos(robotPath);
                if (null == tmpServos || defaultCon.FileModifyTime > tmpServos.FileModifyTime)
                {
                    tmpServos = defaultCon;
                }
            }
            if (null != tmpServos)
            {
                mRobotServosDict[robot.ID] = tmpServos;
                ModelDjData servosData = robot.GetAllDjData();
                List<byte> servoList = servosData.GetIDList();
                for (int i = 0, imax = servoList.Count; i < imax; ++i)
                {
                    servosData.UpdateServoModel(servoList[i], tmpServos.GetServoModel(servoList[i]));
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }


    public override void CleanUp()
    {
        base.CleanUp();
        mRobotServosDict.Clear();
    }
    #endregion

    #region 其他函数

    #endregion
}