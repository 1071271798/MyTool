//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(CircleScrollBar))]
public class UICircleScrollBarEditor : UIProgressBarEditor
{
	protected override void DrawLegacyFields ()
	{
		CircleScrollBar sb = target as CircleScrollBar;
        serializedObject.Update();
		float val = EditorGUILayout.Slider("Value", sb.value, 0f, 1f);
		float alpha = EditorGUILayout.Slider("Alpha", sb.alpha, 0f, 1f);

        NGUIEditorTools.DrawProperty("StartAngle", serializedObject, "mRealStartAngle");
        NGUIEditorTools.DrawProperty("AngleRange", serializedObject, "mAngleRange");
        NGUIEditorTools.DrawProperty("Radius", serializedObject, "mRadius");

        if (GUILayout.Button("Refresh", GUILayout.Width(100), GUILayout.Height(20)))
        {
            sb.OnRefresh();
        }
		if (sb.value != val ||
			sb.alpha != alpha)
		{
			NGUIEditorTools.RegisterUndo("Scroll Bar Change", sb);
			sb.value = val;
			sb.alpha = alpha;
			NGUITools.SetDirty(sb);
		}
	}
}
