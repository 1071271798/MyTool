using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Landoky
{
    class AutoAddKeyFrameWindow : EditorWindow
    {
        /// 动画
        private AnimationClip motion;
        /// 起始
        private float start = 0;
        /// 补帧间隔
        private float interval = 20;
        /// 结束
        private float end = -1;
        /// 总长度
        private float length = 0.0f;

        [MenuItem("Tools/AutoAddKeyFrameWindow")]
        static void Open()
        {
            var win = GetWindow<AutoAddKeyFrameWindow>();
            win.title = "自动补帧工具";
            win.Show();
        }

        private void OnGUI()
        {
            motion = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("动画文件"), motion, typeof(AnimationClip), false);
            EditorGUILayout.FloatField(new GUIContent("总时间长度: "), length);

            if (motion != null)
            {
                start = EditorGUILayout.FloatField("起始帧(秒)", start);
                if (start < 0)
                {
                    start = 0;
                }
                interval = EditorGUILayout.FloatField("补帧间隔(秒)", interval);
                if (interval <= 0)
                {
                    interval = 20;
                }
                end = EditorGUILayout.FloatField("结束帧(秒)", end);
                if (end < 0)
                {
                    end = float.MaxValue;
                }
                if (GUILayout.Button("自动补帧"))
                {
                    float totalTime = motion.length;

                    EditorCurveBinding[] binding = AnimationUtility.GetCurveBindings(motion);
                    foreach (var editorCurveBinding in binding)
                    {
                        Debug.Log(editorCurveBinding.path + ";propertyName:" + editorCurveBinding.propertyName);
                        var curve = AnimationUtility.GetEditorCurve(motion, editorCurveBinding);
                        for (float time = start; time < end && time < totalTime; time += interval)
                        {
                            bool add = true;
                            foreach (var keyframe in curve.keys)
                            {
                                if (keyframe.time == time)
                                {
                                    add = false;
                                    break;
                                }
                            }
                            if (!add)
                            {
                                continue;
                            }
                            float value = curve.Evaluate(time);
                            curve.AddKey(new Keyframe(time, value, 0, 0));
                        }
                        AnimationUtility.SetEditorCurve(motion, editorCurveBinding, curve);
                    }
                    //AssetDatabase.CreateAsset(motion, assetPath);
                    AssetDatabase.Refresh();
                }

				if (GUILayout.Button("自动删帧"))
				{
					float totalTime = motion.length;
					
					EditorCurveBinding[] binding = AnimationUtility.GetCurveBindings(motion);
					foreach (var editorCurveBinding in binding)
					{
						var curve = AnimationUtility.GetEditorCurve(motion, editorCurveBinding);
						for (int i = 0, imax = curve.keys.Length; i < imax; ++i)
						{
							if (curve[i].time >= start && curve[i].time < end)
							{
								curve.RemoveKey(i);
								--i;
								--imax;
							}
						}
						AnimationUtility.SetEditorCurve(motion, editorCurveBinding, curve);
					}
					//AssetDatabase.CreateAsset(motion, assetPath);
					AssetDatabase.Refresh();
				}
            }
        }
    }
}
