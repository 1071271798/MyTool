// ------------------------------------------------------------------
// Description : 场景管理器
// Author      : oyy
// Date        : 2015-04-24
// Histories   : 
// ------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Game.Platform;

namespace Game.Scene
{
    public enum SceneType : int
    {
        StartScene = 0,
        MenuScene=6,
        MainWindow = 2,
        Assemble = 3,
        EditAction = 4,
        ActionPlay = 5,
      //  CreateModel=6,
        EmptyScene = 1,
        testScene = 7,
    }

    static class SceneMgr
    {
        public static SceneType mCurrentSceneType = SceneType.StartScene;
        public static Type mMyCurrentSceneType = null;

        static SceneMgr()
        {
        }

        public static SceneType GetCurrentSceneType()
        {
            return mCurrentSceneType;
        }
        //跳转场景
        public static void EnterScene(SceneType type)
        {
            EnterScene(type, null, null, null);
        }
        public static void EnterScene(SceneType type, EventDelegate.Callback callback)
        {
            EnterScene(type, null, null, callback);
        }
        public static void EnterScene(SceneType type, Type sceneType)
        {
            EnterScene(type, sceneType, null, null);
        }
        public static void EnterScene(SceneType type, Type sceneType, EventDelegate.Callback callback)
        {
            EnterScene(type, sceneType, null, callback);
        }

        public static void EnterScene(SceneType type, Type sceneType, object[] args)
        {
            EnterScene(type, sceneType, args, null);
        }
        public static void EnterScene(SceneType type, Type sceneType, object[] args, EventDelegate.Callback callback)
        {
            try
            {
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, "EnterScene:" + type + "; mCurrentSceneType:" + mCurrentSceneType + "; EnterSceneType:" + sceneType + "; mMyCurrentSceneType :" + mMyCurrentSceneType);
                if (type == mCurrentSceneType && mMyCurrentSceneType == sceneType)
                {
                    if (null != callback)
                    {
                        callback();
                    }
                    return;
                }
                if (null != mMyCurrentSceneType && mMyCurrentSceneType != sceneType)
                {
                    SingletonObject<SceneManager>.GetInst().CloseCurrentScene();
                }
                if (type == SceneType.Assemble && !PlatformMgr.Instance.IsCourseFlag
                    || type == SceneType.EditAction || type == SceneType.EmptyScene && sceneType == typeof(MainScene))
                {
                    SingletonObject<GameBg>.GetInst().ShowBg();
                }
                else
                {
                    SingletonObject<GameBg>.GetInst().HideBg();
                }
                if (mCurrentSceneType == SceneType.Assemble)
                {
                    //PlatformMgr.Instance.DurationEventEnd(MobClickEventID.BuildPage_StayDuring);
                }

                if (mCurrentSceneType != type)
                {
                    mCurrentSceneType = type;
                    AsyncOperation asyn = Application.LoadLevelAsync((int)type);
                    SingletonBehaviour<ClientMain>.GetInst().StartCoroutine(EnterSceneCallBack(asyn, sceneType, args, callback));
                }
                else
                {
                    GotoMyScene(sceneType, args);
                    if (null != callback)
                    {
                        callback();
                    }
                }

                if (type == SceneType.Assemble)
                {
                    //PlatformMgr.Instance.DurationEventStart(MobClickEventID.BuildPage_StayDuring);
                }

            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
        }

        static IEnumerator EnterSceneCallBack(params object[] obj)
        {
            AsyncOperation asyn = (AsyncOperation)obj[0];
            yield return asyn;
            if (asyn.isDone)
            {
                Type sceneType = (Type)obj[1];
                object[] args = (object[])obj[2];
                EventDelegate.Callback callback = (EventDelegate.Callback)obj[3];
                //SingletonObject<GuideManager>.GetInst().ActiveGuide(GuideTriggerEvent.None);
                GotoMyScene(sceneType, args);
                if (null != callback)
                {
                    callback();
                }
            }
            
        }

        static void GotoMyScene(Type sceneType, object[] args)
        {
            try
            {
                if (null != sceneType)
                {
                    if (mMyCurrentSceneType == sceneType && null != SingletonObject<SceneManager>.GetInst().GetCurrentScene())
                    {
                        SingletonObject<SceneManager>.GetInst().GetCurrentScene().UpdateScene();
                    }
                    else
                    {
                        mMyCurrentSceneType = sceneType;
                        SingletonObject<SceneManager>.GetInst().GotoScene(sceneType, args);
                    }
                }
                else
                {
                    mMyCurrentSceneType = null;
                }
                PlatformMgr.Instance.Log(MyLogType.LogTypeDebug, "GotoMyScene mMyCurrentSceneType = " + mMyCurrentSceneType);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            
        }

    }
}
