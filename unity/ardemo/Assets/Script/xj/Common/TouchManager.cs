using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Author:xj
/// FileName:TouchManager.cs
/// Description:
/// Time:2016/7/12 16:04:06
/// </summary>
public delegate void TouchEventHandler(TouchEventArgs args);
public class TouchManager : MonoBehaviour
{
    public static TouchManager instance;
    private static bool isInited = false;

    public static bool IsInited()
    {
        return (null != instance && isInited);
    }

    public static TouchManager GetInstance()
    {
        return instance;
    }

    public event TouchEventHandler onFirstTouchBegan;
    public event TouchEventHandler onFirstTouchMoved;
    public event TouchEventHandler onFirstTouchEnded;
    public event TouchEventHandler onTwoTouchMoved;

    private Vector2 touchStartPos;
    private bool getTwoTouch;
    private float disBtwTouch;
    private bool isMouseDown;

    public void Awake()
    {
        onFirstTouchBegan = new TouchEventHandler(InitFirstBeganHandler);
        onFirstTouchMoved = new TouchEventHandler(InitFirstMovedHandler);
        onFirstTouchEnded = new TouchEventHandler(InitFirstEndedHandler);
        onTwoTouchMoved = new TouchEventHandler(InitTwoMovedHandler);
        touchStartPos = Vector2.zero;
        getTwoTouch = false;
        isMouseDown = false;
        instance = this;
        isInited = true;
    }

    private void InitFirstBeganHandler(TouchEventArgs args)
    {
        //Debug.Log("InitFirstBeganHandler " + args.ToString());  
    }

    private void InitFirstMovedHandler(TouchEventArgs args)
    {
        //Debug.Log("InitFirstMovedHandler " + args.ToString());  
    }
    private void InitFirstEndedHandler(TouchEventArgs args)
    {
        //Debug.Log("InitFirstEndedHandler " + args.ToString());  
    }
    private void InitTwoMovedHandler(TouchEventArgs args)
    {
        //Debug.Log("InitTwoMovedHandler " + args.ToString());  
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GetTouchEvent();
        }
        else
        {
            GetMouseEvent();
        }
    }

    private void GetTouchEvent()
    {
        if (Input.touchCount == 0)
        {
            getTwoTouch = false;
            return;
        }

        if (null != EventSystem.current && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            getTwoTouch = false;
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch firstTouch = Input.GetTouch(0);
            if (firstTouch.phase == TouchPhase.Began)
            {
                touchStartPos = firstTouch.position;
                onFirstTouchBegan(new TouchEventArgs(touchStartPos, 0));
                return;
            }
            if (getTwoTouch)
            {
                getTwoTouch = false;
                return;
            }

            if (firstTouch.phase == TouchPhase.Moved)
            {
                Vector2 deltaMv = firstTouch.position - touchStartPos;
                onFirstTouchMoved(new TouchEventArgs(touchStartPos, 0));
                touchStartPos = firstTouch.position;
                return;
            }
            if (firstTouch.phase == TouchPhase.Ended)
            {
                onFirstTouchEnded(new TouchEventArgs(touchStartPos, 0));
                return;
            }
            return;
        }
        else if (Input.touchCount == 2)
        {
            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);

            float curDis = UnityHelper.GetGenScreenDis(firstTouch.position, secondTouch.position);
            if (!getTwoTouch)
            {
                getTwoTouch = true;
                disBtwTouch = curDis;
                Debuger.LogError("create disBtwTouch = " + disBtwTouch);
                return;
            }
            if (IsTouchDown(firstTouch) || IsTouchDown(secondTouch))
            {
                float deltaDis = curDis - disBtwTouch;
                onTwoTouchMoved(new TouchEventArgs(Vector2.zero, deltaDis));
                disBtwTouch = curDis;
                return;
            }
            getTwoTouch = true;            
            return;
        }
    }

    private bool IsTouchDown(Touch touch)
    {
        return (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved);
    }

    private bool IsTouchUp(Touch touch)
    {
        return (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended);
    }

    private void GetMouseEvent()
    {
        if (null != EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float wheelValue = Input.GetAxis("Mouse ScrollWheel");
        if (wheelValue != 0)
        {
            onTwoTouchMoved(new TouchEventArgs(Vector2.zero, wheelValue));
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            onFirstTouchBegan(new TouchEventArgs(touchStartPos, 0));
            isMouseDown = true;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            touchStartPos = Input.mousePosition;
            onFirstTouchEnded(new TouchEventArgs(touchStartPos, 0));
            isMouseDown = false;
            return;
        }

        if (Input.GetMouseButton(0))
        {
            if (!isMouseDown) { return; }
            Vector2 deltaMv = ((Vector2)Input.mousePosition) - touchStartPos;
            if (Vector2.zero == deltaMv) { return; }
            onFirstTouchMoved(new TouchEventArgs(touchStartPos, 0));
            touchStartPos = Input.mousePosition;
            return;
        }
    }
}

public class TouchEventArgs : System.EventArgs
{
    public Vector2 touchedPos;
    public float twoMvDis;

    public TouchEventArgs(Vector2 pos, float mvDis)
    {
        touchedPos = UnityHelper.GetGenScreenPos(pos);
        twoMvDis = mvDis;
    }

    public override string ToString()
    {
        return string.Format("touchedPos: {0}\t twoMvDis: {1}", touchedPos, twoMvDis);
    }
}


public class UnityHelper
{
    /// <summary>  
    ///  获取通用的屏幕坐标，即忽略dpi的坐标  
    /// </summary>  
    public static Vector2 GetGenScreenPos(Vector2 originPos)
    {
        Vector2 result = originPos;
        result.x /= (1.000f * Screen.width);
        result.y /= (1.000f * Screen.height);
        return result;
    }
    /// <summary>  
    /// 根据通用屏幕坐标计算出当前的实际屏幕坐标  
    /// </summary>  
    /// <param name="genPos"></param>  
    /// <returns></returns>  
    public static Vector2 GetScreenPosFromGen(Vector2 genPos)
    {
        Vector2 result = genPos;
        result.x *= Screen.width;
        result.y *= Screen.height;
        return result;
    }
    /// <summary>  
    /// 计算通用的屏幕距离，即可以忽略dpi的规范距离  
    /// </summary>  
    /// <param name="pos1"></param>  
    /// <param name="pos2"></param>  
    /// <returns></returns>  
    public static float GetGenScreenDis(Vector2 pos1, Vector2 pos2)
    {
        return Vector2.Distance(GetGenScreenPos(pos1), GetGenScreenPos(pos2));
    }
}