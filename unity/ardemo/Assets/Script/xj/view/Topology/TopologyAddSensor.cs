#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:TopologyAddSensor.cs
/// Description:
/// Time:2017/1/9 19:47:34
/// </summary>
public class TopologyAddSensor : MonoBehaviour
{
    #region 公有属性
    #endregion

    #region 其他属性
    #endregion

    #region 公有函数
    #endregion

    #region 其他函数
    int mAddSensorId = 0;
    int mAddSensorType = 0;
    string[] SensorID = new string[] { "1", "2", "3", "4", "5", "6", "7", "8" };
    string[] SensorType = null;

    void OnGUI()
    {
        TopologyUI topologyUI = TopologyBaseMsg.GetTopologyUI();
        if (null == topologyUI)
        {
            return;
        }
        if (topologyUI.IsEdit())
        {
            if (null == SensorType)
            {
                SensorType = new string[PublicFunction.Open_Topology_Part_Type.Length];
                for (int i = 0, imax = PublicFunction.Open_Topology_Part_Type.Length; i < imax; ++i)
                {
                    SensorType[i] = PublicFunction.Open_Topology_Part_Type[i].ToString();
                }
            }
            GUILayout.BeginArea(new Rect(100, 10, 1000, 200));
            //GUILayout.BeginVertical();
            mAddSensorId = GUILayout.SelectionGrid(mAddSensorId, SensorID, SensorID.Length, GUILayout.Width(20));
            mAddSensorType = GUILayout.SelectionGrid(mAddSensorType, SensorType, 4, GUILayout.Width(400));
            if (GUILayout.Button("增加一个传感器", GUILayout.Width(120)))
            {
                byte id = byte.Parse(SensorID[mAddSensorId]);
                TopologyPartType sensorType = (TopologyPartType)Enum.Parse(typeof(TopologyPartType), SensorType[mAddSensorType]);
                topologyUI.AddIndependentSensor(id, sensorType);
            }
            //GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
    #endregion
}
#endif