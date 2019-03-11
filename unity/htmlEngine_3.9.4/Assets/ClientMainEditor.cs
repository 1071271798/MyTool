//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(ClientMain), true)]
public class ClientMainEditor : Editor
{
    public override void OnInspectorGUI ()
	{
		serializedObject.Update();
        if (GUILayout.Button("输出统计时间", GUILayout.Width(100), GUILayout.Height(24)))
        {
            SingletonObject<TimeStatisticsTool>.GetInst().CalculateAverage();
            SingletonObject<TimeStatisticsTool>.GetInst().CleanUp();
        }
	}
}
