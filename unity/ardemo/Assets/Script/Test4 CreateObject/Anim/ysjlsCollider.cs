/*
 * 
 * 作用：如果发生碰撞，则尺寸变大
 * 
 */

using UnityEngine;
using System.Collections;

public class ysjlsCollider : MonoBehaviour
{

    GameObject mc = null;
    GameObject tool;
    GameObject parentGO;
    Vector3 oriPos;
    Vector3 oriRotate;

	// Use this for initialization
	void Start () {
        Debug.Log("0000dgdg");
        BoxCollider bc=this.GetComponent<BoxCollider>();
        if(bc ==null)
        {
            bc=this.gameObject.AddComponent<BoxCollider>();

        }
        bc.size = new Vector3(0.3f,0.25f,0.05f);
        bc.isTrigger = true;
        CharacterController ccer=this.gameObject.AddComponent<CharacterController>();
        ccer.radius = 0.05f;
        mc = GameObject.Find("oriGO/mc");
        parentGO = GameObject.Find("m79_001");
        tool = GameObject.Find("ysjlstool1");
        oriPos = tool.transform.position;
        oriRotate = tool.transform.eulerAngles;

	}

    /// <summary>
    /// 碰撞触发发生时
    /// </summary>
    /// <param name="temp"></param>
    void OnTriggerEnter(Collider temp)
    {
        Debug.Log("temp:" + temp.gameObject.name);
        if (temp.gameObject.name == "ysjlstool1" && RobotMgr.Instance.colliderCount < RobotMgr.Instance.finalCount)
        {
          RobotMgr.Instance.colliderCount++;
          Debug.Log("temp:" + temp.gameObject.name + ";" + RobotMgr.Instance.colliderCount);

          if (RobotMgr.Instance.colliderCount == RobotMgr.Instance.finalCount)
            {
                if (tool!=null)
              {
                  Debug.Log("leave true");
                  tool.transform.parent = parentGO.transform;
                  RobotMgr.Instance.leave = true;
              }
                
            }
        }

        
    }

    /// <summary>
    /// 碰撞触发退出时
    /// </summary>
    /// <param name="temp"></param>
    void OnTriggerExit(Collider temp)
    {
        if (temp.gameObject.name == "ysjlstool1" && RobotMgr.Instance.colliderCount>0)
        {
 
            RobotMgr.Instance.colliderCount--;
            Debug.Log("OnTriggerExit:" + RobotMgr.Instance.colliderCount);

            if (RobotMgr.Instance.colliderCount < RobotMgr.Instance.finalCount)
            {
                Debug.Log("leave 000");
                if (RobotMgr.Instance.leave)
                {
                    tool.transform.parent = null;
                    tool.transform.parent = mc.transform;
                    Debug.Log("leave");
                    tool.transform.eulerAngles = oriRotate;
                    Vector3 nowPos = tool.transform.position;
                    Invoke("pos1", 0.2f);
                    RobotMgr.Instance.leave = false;
                }
            }
        }

    
    }

    /// <summary>
    /// y轴位置移动
    /// </summary>
    void pos1()
    {
        Vector3 nowPos = tool.transform.position;
        Move<TweenPosition>(tool, new Vector3(nowPos.x, nowPos.y, nowPos.z), new Vector3(nowPos.x, oriPos.y, nowPos.z), "Position");
        Invoke("pos2", 0.2f);
    }

    /// <summary>
    /// x,z轴位置移动
    /// </summary>
    void pos2()
    {
        Vector3 nowPos = tool.transform.position;
        Move<TweenPosition>(tool, nowPos, oriPos, "Position");
    }


    /// <summary>
    /// 坐标，角度偏移
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="now"></param>
    /// <param name="ori"></param>
    /// <param name="t"></param>
    public void Move<T>(GameObject go, Vector3 now, Vector3 ori, string t) where T : UITweener
    {

        T tp = go.GetComponent<T>();

        if (tp == null)
        {
            tp = go.AddComponent<T>();
        }

        tp.duration = 0.1f;

        if (t == "Position")
        {
            TweenPosition tt = tp as TweenPosition;

            tt.worldSpace = true;
            tt.from = now;
            tt.to = ori;
        }
        else if (t == "Rotate")
        {
            TweenRotation tt = tp as TweenRotation;

            tt.from = now;
            tt.to = ori;
        }

        tp.PlayForward();
    }
}
