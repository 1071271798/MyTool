using UnityEngine;
using System.Collections;
using Game.Scene;
using Game.Platform;

//delegate void CameraController();
public class CamRotateAroundCircle : MonoBehaviour
{
    private Vector3 oriPos;
    private Vector3 oriAng;
    private float oriSize;

    private Vector3 oriPosTarget;

    //����ϵ��  
    private float distance = 0.0f;
    //���һ����ƶ��ٶ�  
    private float xSpeed = 250.0f;
    private float ySpeed = 120.0f;
    //��������ϵ��  
    private float yMinLimit = -360;
    private float yMaxLimit = 360;
    //����ͷ��λ��  
    private float x = 0.0f;
    private float y = 0.0f;
    //��¼��һ���ֻ�����λ���ж��û�������Ŵ�����С����
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;
    //���ڰ󶨲��������
    private Transform target;

    public static CamRotateAroundCircle _instance;

    private Vector3 scToWorPos;//��Ļʵʱλ��

    public bool canControl = true;     //�Ƿ���Կ��ƽ���
    private bool canMove = false;

    public Vector3 centerPos;
    public GameObject center;
    public Vector3 camEgler;

    float xTemp;
    float yTemp;

    float defaultField;
    Vector2 lastPosition;
    Vector2 lastPosition1;

    //GameObject cameramr;
    void Awake()
    {
        //centerPos = new Vector3(0, 1.0f, -0.68f);
        if (SceneMgr.GetCurrentSceneType() != SceneType.Assemble)
        {
            centerPos = new Vector3(0, 1.0f, -0.54f);
        }
        else
        {
            centerPos = new Vector3(0, 1.0f, -0.68f);
        }

        Vector3 rotatTemp = new Vector3(0, 90.0f, 0);
        Quaternion tempquat = Quaternion.Euler(rotatTemp);
        this.transform.rotation = tempquat;

        defaultField = Camera.main.fieldOfView;
    }

    void Start()
    {
        center = GameObject.Find("MVCenter");

        if (SceneMgr.GetCurrentSceneType() != SceneType.Assemble)
        {

            this.GetComponent<Camera>().rect = new Rect(0, 0, 0.6f, 1);
        }
        else
        {
            this.GetComponent<Camera>().rect = new Rect(0, 0, 1, 1);
        }

        if (center == null)
        {
            center = Resources.Load("Prefab/Test4/ScensePrefab/Center") as GameObject;
            center = GameObject.Instantiate(center, centerPos, Quaternion.identity) as GameObject;
            center.GetComponentInChildren<MeshRenderer>().enabled = false;
            center.name = "MVCenter";
            DontDestroyOnLoad(center);
        }

        if (RobotMgr.Instance.newRobot == false)
        {
            center.transform.position = centerPos;
        }
        else
        {
            center.transform.position = new Vector3(-0.46f, 0, -2.68f);
        }

        target = center.transform;

        distance = 10.0f;

        _instance = this;

        //Debug.Log("campos:"+);

        this.GetComponent<Camera>().fieldOfView = 60;

        oriPos = this.transform.position;
        oriAng = this.transform.eulerAngles;
        oriSize = this.GetComponent<Camera>().fieldOfView;

        oriPosTarget = target.transform.position;

        camEgler = this.GetComponent<Camera>().transform.localEulerAngles;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        xTemp = x;
        yTemp = y;

        if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble)
        {
            AddEvent(JMSimulatorOnly.Instance.btns["Refresh"]);
            // JMSimulatorOnly.Instance.EnterSceneHide();
        }

    }

    public void AddEvent(GameObject btn)
    {
        if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble)
        {

            //GameObject btn = GameObject.Find("UI Root/Camera/Left/Refresh");
            UIEventListener.Get(btn).onClick += OnResetCam;

        }
    }

    void Update()
    {
        //if (canControl)
        //{
        CameraControl();
        //}
    }

    /// <summary>
    /// reset
    /// </summary>
    bool isResetOdder = false;
    public void ResetOriState()
    {
        //  this.transform.position = oriPos;
        //  this.transform.eulerAngles = oriAng;
        //  this.camera.fieldOfView = oriSize;

        //  target.transform.position = oriPosTarget;
        //  target.transform.position = oriPosTarget;

        //  this.camera.fieldOfView = 60;
        // // this.camera.transform.localEulerAngles = camEgler;
        ////  isResetOdder = true;
        //  y = camEgler.x;
        //  x = camEgler.y;
        ResetCam(this.gameObject);
    }

    public float minfov = 15f;
    public float maxfov = 90f;
    public float sensitivity = 1f;

    public void CameraControl()
    {
        if (null != UICamera.hoveredObject)
        {
            if (UICamera.hoveredObject.name != "CameraMR")
            {
                return;
            }
        }
#if UNITY_EDITOR
        MouseOperation();
#endif

#if UNITY_IPHONE
            TouchOperation();
#endif
#if UNITY_ANDROID
        TouchOperation();
#endif

    }

    public void MobClickOriginalPosition(GameObject obj = null)
    {
        if (RecordContactInfo.Instance.openType != "default")
        {
            return;
        }
        bool originalPosition = true;
        if (RobotMgr.Instance.newRobot == false)
        {
            if (center != null)
            {
                if (Vector3.Distance(center.transform.position, centerPos) > 0.0001f)
                {
                    originalPosition = false;
                }
            }
            else
            {
                GameObject center1 = GameObject.Find("MVCenter");
                if (center1 != null)
                {
                    if (Vector3.Distance(center1.transform.position, centerPos) > 0.00001f)
                    {
                        originalPosition = false;
                    }
                }

            }
        }
        else
        {
            Vector3 pos = new Vector3(-0.46f, 0, -2.68f);
            if (Vector3.Distance(center.transform.position, pos) > 0.0001f)
            {
                originalPosition = false;
            }
        }
        if (Mathf.Abs(Camera.main.fieldOfView - defaultField) > 0.00001f)
        {
            originalPosition = false;
        }
        if (target)
        {
            var rotation1 = Quaternion.Euler(yTemp, xTemp, 0);

            var position = rotation1 * new Vector3(0.0f, 0.0f, -10.0f) * 0.3f + target.position;
            if (Vector3.Distance(position, this.transform.position) > 0.0001f)
            {
                originalPosition = false;
            }
        }
    }

    void OnResetCam(GameObject go)
    {
        MobClickOriginalPosition();
        ResetCam(go);
    }
    //�������λ
    public void ResetCam(GameObject go)
    {

        if (RobotMgr.Instance.newRobot == false)
        {
            if (center != null)
            {
                center.transform.position = centerPos;
            }
            else
            {
                center = GameObject.Find("MVCenter");

                if (center != null)
                {
                    center.transform.position = centerPos;
                }

            }
        }
        else
        {
            Vector3 pos = new Vector3(-0.46f, 0, -2.68f);
            center.transform.position = pos;
        }
        x = xTemp;
        y = yTemp;
        Camera.main.fieldOfView = defaultField;
        distance = 10.0f;
        ConPos();
    }

    public void MouseOperation()
    {
        scToWorPos = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            lastPosition = Input.mousePosition;
        }
        //ƽ����Ļ
        if (Input.GetMouseButton(0) && Input.touchCount == 0)
        {
            //���ݴ��������X��Yλ��  
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        
        //��ת��Ļ
        ScreenPosControl();

        //�������������
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {

            float fov = Camera.main.fieldOfView;
            fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity * 10;
            fov = Mathf.Clamp(fov, minfov, maxfov);
            Camera.main.fieldOfView = fov;
        }

        if (target)
        {
            //�����������λ��
            if (y >= 360 || y <= -360)
            {
                y = 0;
            }
            //y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);

            var position = rotation * new Vector3(0.0f, 0.0f, -distance) * 0.5f + target.position;
            transform.rotation = rotation;
            transform.position = position;
            
            if (Input.GetMouseButtonUp(0) && Vector2.Distance(lastPosition, Input.mousePosition) > 0.01f || Input.GetMouseButtonUp(1) && Vector2.Distance(lastPosition, Input.mousePosition) > 0.01f)
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P6_1);
            }
        }
    }

    public void TouchOperation()
    {
        #region test
        //��Ļ��ת
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                lastPosition = Input.GetTouch(0).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {

                ////���ݴ��������X��Yλ��  
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                if (target)
                {
                    //�����������λ��
                    if (y >= 360 || y <= -360)
                    {
                        y = 0;
                    }
                    // y = ClampAngle(y, yMinLimit, yMaxLimit);
                    var rotation = Quaternion.Euler(y, x, 0);
                    //��ǰ����:
                    transform.rotation = rotation;
                    //transform.position = position;
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended && Vector2.Distance(lastPosition, Input.GetTouch(0).position) > 0.01f)
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P6_1);
            }

        }
        else if (Input.touchCount == 2)     //�жϴ�������Ϊ��㴥��  
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                lastPosition = Input.GetTouch(0).position;
            }
            if (Input.GetTouch(1).phase == TouchPhase.Began)
            {
                lastPosition1 = Input.GetTouch(1).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                //�������ǰ���㴥�����λ��  
                var tempPosition1 = Input.GetTouch(0).position;
                var tempPosition2 = Input.GetTouch(1).position;
                float posDistance = Mathf.Sqrt((tempPosition1.x - tempPosition2.x) * (tempPosition1.x - tempPosition2.x) + (tempPosition1.y - tempPosition2.y) * (tempPosition1.y - tempPosition2.y));

                #region ˫ָ����
                //����������Ϊ�Ŵ󣬷��ؼ�Ϊ��С  
                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                {
                    //�Ŵ�ϵ������3�Ժ���������Ŵ�  
                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
                    if (distance > 3.0f)
                    {
                        distance -= 0.2f;
                    }
                }
                else
                {
                    //��Сϴ������18.5�����������С  
                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
                    if (distance < 80)
                    {
                        distance += 0.2f;
                    }
                }
                #endregion

                #region test   ˫ָƽ��
                float delta_x = Input.GetAxis("Mouse X") * 0.02f;
                float delta_y = Input.GetAxis("Mouse Y") * 0.02f;
                Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
                if ((-270 < y && y < -90) || (90 < y && y < 270))
                {
                    transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + transform.localPosition;
                    target.transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + target.transform.localPosition;
                }
                else
                {
                    transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.localPosition;
                    target.transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + target.transform.localPosition;
                }
                if (target)
                {
                    //�����������λ��
                    if (y >= 360 || y <= -360)
                    {
                        y = 0;
                    }
                    //y = ClampAngle(y, yMinLimit, yMaxLimit);
                    var rotation1 = Quaternion.Euler(y, x, 0);

                    var position = rotation1 * new Vector3(0.0f, 0.0f, -distance) * 0.3f + target.position;
                    transform.position = position;
                }
                #endregion

                //������һ�δ������λ�ã����ڶԱ�  
                oldPosition1 = tempPosition1;
                oldPosition2 = tempPosition2;
            }
            else if ((Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Stationary) || (Input.GetTouch(0).phase == TouchPhase.Stationary && Input.GetTouch(1).phase == TouchPhase.Moved))
            {

            }
            else if ((Input.GetTouch(0).phase == TouchPhase.Ended && Input.GetTouch(1).phase == TouchPhase.Ended && (Vector2.Distance(lastPosition, Input.GetTouch(0).position) > 0.01f ||  Vector2.Distance(lastPosition1, Input.GetTouch(1).position) > 0.01f)))
            {
                PlatformMgr.Instance.MobClickEvent(MobClickEventID.click_P6_1);
            }
        }
        #endregion
    }

    //��λ
    public void ConPos()
    {
        if (target)
        {
            //�����������λ��
            if (y >= 360 || y <= -360)
            {
                y = 0;
            }
            var rotation1 = Quaternion.Euler(y, x, 0);

            var position = rotation1 * new Vector3(0.0f, 0.0f, -distance) * 0.3f + target.position;
            this.transform.position = position;
        }
    }


    bool isMove(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        var lengdis = Mathf.Abs(leng1 - leng2);
        if (lengdis <= 10.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //����������Ϊ�Ŵ󣬷��ؼ�Ϊ��С
    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //����������һ�δ��������λ���뱾�δ��������λ�ü�����û�������
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));

        if (leng1 < leng2)
        {

            //�Ŵ�����
            return true;
        }
        else
        {
            //��С����
            return false;
        }
    }



    //ͨ������ƶ���Ļλ��
    public void ScreenPosControl()
    {
        #region mouse Control screenPos
        scToWorPos = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);

        if (Input.GetMouseButton(1))
        {

            float delta_x = Input.GetAxis("Mouse X") * 0.02f;
            float delta_y = Input.GetAxis("Mouse Y") * 0.02f;
            Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            if ((-270 < y && y < -90) || (90 < y && y < 270))
            {
                transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + transform.localPosition;
                target.transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + target.transform.localPosition;
            }
            else
            {
                transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.localPosition;
                target.transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + target.transform.localPosition;
            }
            
        }

        #endregion
    }


    //Update����һ�����ý����Ժ����������������������λ��
    void LateUpdate()
    {
        // targetΪ���ǰ󶨵����������������ת�Ĳ�����
        if (target)
        {
            //�����������λ��
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);

            var position = rotation * new Vector3(0.0f, 0.0f, -distance) * 0.3f + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 10;
        if (angle > 360)
            angle -= 10;
        return Mathf.Clamp(angle, min, max);
    }

}