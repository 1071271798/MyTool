
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:SceneManager.cs
/// Description:
/// Time:2015/7/21 16:54:56
/// </summary>
public class SceneManager : SingletonObject<SceneManager>
{
    #region 公有属性
    #endregion

    #region 私有属性
    BaseScene mCurrentScene = null;
    Type mLastScene = null;
    #endregion

    #region 公有函数
    public void Init()
    {
        
    }
    /// <summary>
    /// 场景跳转
    /// </summary>
    /// <param name="scene">进入的场景</param>
    /// <param name="args">该场景构造函数的参数</param>
    public void GotoScene(Type scene, object[] args = null)
    {
        try
        {
            Debug.Log( "GotoScene = " + scene.ToString());
            CloseCurrentScene();
            BaseScene baseScene;
            if (null == args)
            {
                baseScene = Activator.CreateInstance(scene) as BaseScene;
            }
            else
            {
                baseScene = Activator.CreateInstance(scene, args) as BaseScene;
            }
            if (null != baseScene)
            {
                baseScene.Open();
                mCurrentScene = baseScene;
            }
            //SingletonObject<GuideManager>.GetInst().ActiveGuide(GuideTriggerEvent.None);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    public BaseScene GetCurrentScene()
    {
        return mCurrentScene;
    }
    /// <summary>
    /// 关闭当前场景
    /// </summary>
    public void CloseCurrentScene()
    {
        try
        {
            if (null != mCurrentScene)
            {
                mLastScene = mCurrentScene.GetType();
                mCurrentScene.Close();
                mCurrentScene = null;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void Update()
    {
        try
        {
            if (null != mCurrentScene)
            {
                mCurrentScene.Update();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
        
    }

    public void LateUpdate()
    {
        try
        {
            if (null != mCurrentScene)
            {
                mCurrentScene.LateUpdate();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            Debug.LogError( this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
        }
    }
    #endregion

    #region 私有函数
    #endregion
}
