using Game.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author:xj
/// FileName:RobotMotion.cs
/// Description:控制机器人
/// Time:2015/7/13 10:29:54
/// </summary>
public class RobotMotion : MonoBehaviour
{
    #region 公有属性
    public Camera mCam = null;
    #endregion

    #region 私有属性
    float x;

    float y;

    float xSpeed = 200;
    
    float ySpeed = 200;
    float zSpeed = 40f;
    float pinchSpeed = 0.02f;
    
    float distance = 0;
    
    float minimumDistance = -3;
    
    float maximumDistance = 3;

    Touch touch;

    float lastDist = 0;


    float curDist = 0;

    float moveLastDist = 0;

    Transform mTrans;
    Transform mCenterTrans;
    Vector3 mTransInitPos;
    Vector3 mTransInitRota;

    //GameObject mBoxObj;
    //BoxCollider mBox;

    bool mPressFlag;
    bool mNeedSendRobotBox;
    Vector3 mLastPos;
    
    #endregion

    #region 公有函数
    #endregion

    #region 私有函数
    void Start()
    {
        EventMgr.Inst.Regist(EventID.Robot_Position_Revert, PositionRevert);
        mTrans = transform;
        mTransInitPos = mTrans.localPosition;
        mTransInitRota = mTrans.localEulerAngles;
        Vector3 pos = Vector3.zero;
        for (int i = 0, icount = mTrans.childCount; i < icount; ++i)
        {
            pos += mTrans.GetChild(i).localPosition;
        }
        if (null == mTrans.Find("CenterObj"))
        {
            GameObject obj = new GameObject("CenterObj");
            mCenterTrans = obj.transform;
            pos = pos / mTrans.childCount;
            mCenterTrans.parent = mTrans;
            if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
            {
                mCenterTrans.localPosition = pos;
            }
        }
        /*mBoxObj = new GameObject("RobotBox");
        mBoxObj.layer = LayerMask.NameToLayer("Robot");
        mBoxObj.transform.position = mCenterTrans.position;
        mBoxObj.transform.localScale = Vector3.one;
        mBox = mBoxObj.AddComponent<BoxCollider>();
        mBox.center = Vector3.zero;
        mBox.size = new Vector3(4000, 4000, 1);*/
        mPressFlag = false;
        mNeedSendRobotBox = true;
    }

    void OnDestroy()
    {
        EventMgr.Inst.UnRegist(EventID.Robot_Position_Revert, PositionRevert);
    }


    bool Raycast(Vector3 inPos)
    {
        if (null == mCam)
        {
            mCam = Camera.main;
            //mCam.gameObject.AddComponent<MoveSecond>();
        }
        // Cast a ray into the screen
        Ray ray = mCam.ScreenPointToRay(inPos);
        RaycastHit hit;
        #if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            mPressFlag = false;
        }
        #else
        int endNum = 0;
        for (int i = 0, icount = Input.touchCount; i < icount; ++i)
        {
            Touch tmpTouch = Input.GetTouch(i);
            if (tmpTouch.phase == TouchPhase.Ended || tmpTouch.phase == TouchPhase.Canceled)
            {
                endNum++;
            }
        }
        if (endNum > 0 && endNum == Input.touchCount)
        {
            mPressFlag = false;
        }
#endif
        /*if (Physics.Raycast(ray, out hit))
        {
            if (null != mBoxObj && hit.collider.gameObject.name.Equals(mBoxObj.name))
            {
                return true;
            }
        }*/
        if (!Physics.Raycast(ray, out hit))
        {
            if (null == hit.collider)
            {
                return true;
            }
        }
        return false;
    }

    void MouseCtrl()
    {
        if (!Raycast(Input.mousePosition))
        {
            return;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            mPressFlag = true;
            mNeedSendRobotBox = false;
            mLastPos = Input.mousePosition;
        }
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && null == RobotEventListener.hoveredObject && null == RobotEventListener.CurrentSelection)
        {
            if (Vector3.Distance(mLastPos, Input.mousePosition) < 2)
            {
                mNeedSendRobotBox = true;
            }
            if (mNeedSendRobotBox)
            {
                EventMgr.Inst.Fire(EventID.Trigger_Robot_Box);
                EventMgr.Inst.Fire(EventID.Cancel_Item_Select);
            }
        }

        if (null != RobotEventListener.CurrentSelectDj)
        {
            return;
        }
        if (mPressFlag && Input.GetMouseButton(0))
        {
            x = Input.GetAxis("Mouse X") * 2;
            y = Input.GetAxis("Mouse Y") * 2;
            mTrans.Translate(Vector3.right * x * Time.deltaTime, Space.World);
            mTrans.Translate(Vector3.up * y * Time.deltaTime, Space.World);
        }
        else if (mPressFlag && Input.GetMouseButton(1))
        {
            x=Input.GetAxis("Mouse X") *xSpeed * 2f;
            y=Input.GetAxis("Mouse Y") *ySpeed * 2f;

            mTrans.RotateAround(mCenterTrans.position, Vector3.up, -x * Time.deltaTime);
            mTrans.RotateAround(mCenterTrans.position, Vector3.right, y * Time.deltaTime);
            
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float ga = Input.GetAxis("Mouse ScrollWheel");
            if (mTrans.localPosition.z > minimumDistance && mTrans.localPosition.z < maximumDistance || mTrans.localPosition.z <= minimumDistance && ga < 0 || mTrans.localPosition.z >= maximumDistance && ga > 0)
            {
                mTrans.Translate(Vector3.forward * -Input.GetAxis("Mouse ScrollWheel") * zSpeed * Time.deltaTime, Space.World);
            }

        }
    }

    void TouchCtrl()
    {
        int beganNum = 0;
        for (int i = 0, icount = Input.touchCount; i < icount; ++i)
        {
            Touch tmpTouch = Input.GetTouch(i);
            if (!Raycast(tmpTouch.position))
            {
                return;
            }
            
            TouchPhase phase = tmpTouch.phase;
            if (phase == TouchPhase.Began)
            {
                beganNum++;
                if (1 == icount)
                {
                    mNeedSendRobotBox = true;
                }
            }
            if (1 == icount)
            {
                if (tmpTouch.deltaPosition.sqrMagnitude > 0)
                {
                    mNeedSendRobotBox = false;
                }
                if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                {
                    if (null == RobotEventListener.hoveredObject && null == RobotEventListener.CurrentSelection && mNeedSendRobotBox)
                    {
                        EventMgr.Inst.Fire(EventID.Trigger_Robot_Box);

                        EventMgr.Inst.Fire(EventID.Cancel_Item_Select);
                    }
                    mNeedSendRobotBox = true;
                }
                
            }
        }
        if (null != RobotEventListener.CurrentSelectDj)
        {
            return;
        }
        if (beganNum > 0 && beganNum == Input.touchCount)
        {
            mPressFlag = true;
        }
        if (mPressFlag && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {

            //One finger touch does orbit

            touch = Input.GetTouch(0);
            

            x = touch.deltaPosition.x * 50;

            y = touch.deltaPosition.y * 50;

            mTrans.RotateAround(mCenterTrans.position, Vector3.up, -x * Time.deltaTime);
            mTrans.RotateAround(mCenterTrans.position, Vector3.right, y * Time.deltaTime);
            /*var rotation = Quaternion.Euler(y, x, 0);
            mTrans.rotation = rotation;*/
        }
        if (mPressFlag && Input.touchCount > 1 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved))
        {

            //Two finger touch does pinch to zoom
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);
            curDist = Vector2.Distance(touch1.position, touch2.position);
            if (curDist > lastDist)
            {
                distance += Vector2.Distance(touch1.deltaPosition, touch2.deltaPosition) * pinchSpeed;
            }
            else
            {
                distance -= Vector2.Distance(touch1.deltaPosition, touch2.deltaPosition) * pinchSpeed;
            }
            lastDist = curDist;

            if (distance <= minimumDistance)
            {
                distance = minimumDistance;
            }
            else if (distance >= maximumDistance)
            {
                distance = maximumDistance;

            }
            var position = new Vector3(mTrans.localPosition.x, mTrans.localPosition.y, -distance);
            mTrans.localPosition = position;
        }
        if (mPressFlag && Input.touchCount > 1 && (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved))
        {
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);
            float curDist = Vector2.Distance(touch1.position, touch2.position);
            if (Math.Abs(curDist - moveLastDist) < 5)
            {
                float x = (touch1.deltaPosition.x + touch2.deltaPosition.x) / 2 * 1.2f;
                float y = (touch1.deltaPosition.y + touch2.deltaPosition.y) / 2 * 1.05f;
                mTrans.Translate(Vector3.right * x * Time.deltaTime, Space.World);
                mTrans.Translate(Vector3.up * y * Time.deltaTime, Space.World);
            }
            moveLastDist = curDist;
        }
    }
    void Update()
    {
        if (null != UICamera.hoveredObject)
        {
            return;
        }
#if UNITY_EDITOR
        MouseCtrl();
#else
        TouchCtrl();
#endif
    }

    void PositionRevert(EventArg arg)
    {
        mTrans.localPosition = mTransInitPos;
        mTrans.localEulerAngles = mTransInitRota;
    }
    #endregion
}