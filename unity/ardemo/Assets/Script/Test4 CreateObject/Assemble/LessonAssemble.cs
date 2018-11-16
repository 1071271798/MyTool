using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Resource;
using Game;
using Game.Event;
using Game.Platform;
/// <summary>
/// 功能： 课程新增功能
/// 
/// </summary>
public class LessonAssemble : MonoBehaviour {
    private int introClickCount;//课程指引打击次数
    private GameObject lessonNotice;
    private GameObject lessonbg;
    private Dictionary<string, GameObject> lessongos = new Dictionary<string, GameObject>();
    private Texture bgPic;  //场景背景图片
    private static LessonAssemble _inst;
    private GameObject noticesTemp;
    public static LessonAssemble Instance()
    {
        if(_inst==null)
        {
            _inst=new LessonAssemble();
          
        }
        return _inst;
    }

    void Start()
    {
        EventMgr.Inst.Regist(EventID.ModelLoadFinish, ModelLoadFinish);
        
        Debug.Log("noticesTemp:" + noticesTemp);
    }
    void OnDestroy()
    {
        EventMgr.Inst.UnRegist(EventID.ModelLoadFinish, ModelLoadFinish);
    }
    void ModelLoadFinish(EventArg arg)
    {
        string robotname = RobotMgr.Instance.rbtnametempt;
        string robotNameNoType = RobotMgr.NameNoType(robotname);
        noticesTemp = GameObject.Find("UIRootABL/Camera/Center/LittePanel/Center/LessonParts/notices");
        //延时播放动画
        if (robotNameNoType == "tanceche")
        {
            noticesTemp.transform.Find("tancechenotice1/Sprite").gameObject.SetActive(true);
        }
        if (robotNameNoType == "TankbotPro")
        {
            noticesTemp.transform.Find("TankbotPronotice1/Sprite").gameObject.SetActive(true);
            noticesTemp.transform.Find("TankbotPronotice1/Sprite2").gameObject.SetActive(true);
        }
    }

    public string GetPicName(string rbtName)
    {
        string picName = "";
        if (rbtName == "yuhangyuan" || rbtName == "newxingbao")
        {
            picName = "yuhangyuan";
        }
        else if (rbtName == "tanceche" || rbtName == "newlubao")
        {
            picName = "tanceche";
        }
        else if (rbtName == "TankbotPro" || rbtName == "newtanxing")
        {
            picName = "TankbotPro";
        }
        return picName;
    }

    //加载场景背景图片
    public IEnumerator ChangeCamBG(string rbtName)
    {
        //模型名称的图片
        string picName = GetPicName(rbtName);

        string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "//default//" + rbtName + "//background//" + picName + ".png";

        if (pathTemp != null)
        {
            WWW www = new WWW(pathTemp);

            yield return www;
            try
            {
                if (www != null && string.IsNullOrEmpty(www.error))
                {
                    //获取Texture
                    bgPic = www.texture;
                   
                    GameObject.Find("MainCamera/AssemblePanel/UI Root/Camera/gamebg/tex").GetComponent<UITexture>().mainTexture = bgPic;
                    noticesTemp = GameObject.Find("UIRootABL/Camera/Center/LittePanel/Center/LessonParts/notices");
                    noticesTemp.transform.Find(picName + "notice1").GetComponent<UITexture>().mainTexture = bgPic;
                }

            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());

            }
        }


    }

    /// <summary>
    /// 课程模式搭建指引
    /// </summary>
    /// <param name="bgpic"></param>
    public void StartHandle(string robotName, GameObject lessonPartsT)
    {
        introClickCount = 1;
        lessonNotice = lessonPartsT.transform.Find("notices").gameObject;
        lessonbg = lessonPartsT.transform.Find("lessonbg").gameObject;
        //查找相应的提示
        int childNum = 1;
        foreach (Transform child in lessonNotice.GetComponentInChildren<Transform>())
        {
            string nnameT = "notice" + childNum++;
            lessongos.Add(child.name, child.gameObject);
            if (child.name == "yuhangyuannotice1")
            {
                if (robotName == "yuhangyuan" || robotName == "newxingbao")
                {
                    child.Find("bg1/bg1label").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("startStory1");
                    child.Find("bg1/bg1label2").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("startStory2");
                    child.Find("bg1/bg1label3").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("startStory3");
                    child.Find("bg1/bg1label4").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("startStory4");
                    child.Find("Sprite2/Label1").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("yuhangyuanStory1");
                    child.Find("Sprite2/Label2").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("yuhangyuanStory2");
                }

            }
            else if (child.name == "tancechenotice1")
            {
                if (robotName == "tanceche" || robotName == "newlubao")
                {
                    child.Find("Sprite/Sprite2/Label1").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("tancecheStory1");
                }
            }
            else if (child.name == "TankbotPronotice1")
            {
                if (robotName == "TankbotPro" || robotName == "newtanxing")
                {
                    child.Find("Sprite/Sprite2/Label1").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("TankbotProStory1");
                    child.Find("Sprite2/Sprite2/Label1").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("TankbotProStory2");
                }

            }
            else
            {
                if (robotName == "yuhangyuan" || robotName == "newxingbao")
                {
                    child.Find("Label").GetComponent<UILabel>().text = LauguageTool.GetIns().GetText("lesson" + child.name);//提示显示的文字
                }

            }
        }
        click(lessonNotice);
        UIEventListener.Get(lessonNotice).onClick += click;

    }

    IEnumerator LoadLessonBG(string rbtName,UITexture texture)
    {
        //模型名称的图片
        string picName = GetPicName(rbtName);
        string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "//default//" + rbtName + "//background//" + picName + ".png";

        if (pathTemp != null)
        {
            WWW www = new WWW(pathTemp);

            yield return www;
            try
            {
                if (www != null && string.IsNullOrEmpty(www.error))
                {
                    //获取Texture
                    texture.mainTexture = www.texture;
                }

            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());

            }
        }
    }
     

    public void click(GameObject go)
    {
        string robotname = RobotMgr.Instance.rbtnametempt;
        string robotNameNoType = RobotMgr.NameNoType(robotname);
        if (robotNameNoType == "yuhangyuan" || robotNameNoType == "newxingbao")
        {
            if (introClickCount <= 5)
            {

                ShowIntro();
            }
            else
            {
                lessonNotice.SetActive(false);
                lessonbg.SetActive(false);
            }
        }
        else if (robotNameNoType == "tanceche" || robotNameNoType == "newlubao")
        {
            if (introClickCount <= 1)
            {

                ShowIntro();
            }
            else
            {
                lessonNotice.SetActive(false);
                lessonbg.SetActive(false);
            }
        }
        else if (robotNameNoType == "TankbotPro" || robotNameNoType == "newtanxing")
        {
            if (introClickCount <= 1)
            {

                ShowIntro();
            }
            else
            {
                lessonNotice.SetActive(false);
                lessonbg.SetActive(false);
            }
        }
        else
        {
            lessonNotice.SetActive(false);
            lessonbg.SetActive(false);
        }
        
    }

    /// <summary>
    /// 指引操作
    /// </summary>
    /// <param name="lessonParts">课程控件</param>
    /// <param name="clickCount">点击次数</param>
    ///<param name="translatenots">翻译</param>
    public void ShowIntro()
    {
        string rbtnameNoType = RobotMgr.NameNoType(RobotMgr.Instance.rbtnametempt);
        string nameT = LessonAssemble.Instance().GetPicName(rbtnameNoType);
        string nameTemp2 = nameT + "notice1";
        //显示相应的提示
        foreach(string nameTemp in lessongos.Keys)
        {
            if (nameTemp.Contains("notice" + introClickCount))
            {
                if(introClickCount==1)
                {
                    if(nameTemp==nameTemp2)
                    {
                        if (lessongos[nameTemp2].activeInHierarchy == false)
                        {
                            lessongos[nameTemp2].SetActive(true);
                        }

                        if (rbtnameNoType == "yuhangyuan" || rbtnameNoType == "newxingbao")
                        {

                            if (LauguageTool.GetIns().CurLauguage == LauguageType.Chinese || LauguageTool.GetIns().CurLauguage == LauguageType.HokongChinese)
                            {
                                lessongos[nameTemp2].GetComponent<Animation>().Play("yhyAnimation");
                                Timer.Add(24.2f, 0, 0, Disableyhynotice1);
 
                            }
                            else
                            {
                                lessongos[nameTemp2].GetComponent<Animation>().Play("yhyAnimationqtyy");
                                Timer.Add(35.0f, 0, 0, Disableyhynotice1);                                               
                            }
                        }
                        else if (rbtnameNoType == "tanceche" || rbtnameNoType == "newlubao")
                        {
                            Timer.Add(5.5f, 0, 0, Disableyhynotice1);
                        }
                        else if (rbtnameNoType == "TankbotPro" || rbtnameNoType == "newtanxing")
                        {
                            Timer.Add(7.5f, 0, 0, Disableyhynotice1);
                        }
                    }
                    else
                    {
                        if (lessongos[nameTemp].activeInHierarchy)
                        {
                            lessongos[nameTemp].SetActive(false);
                        }

                    }
                    
                }
                else
                {
                    if (lessongos[nameTemp].activeInHierarchy == false)
                    {
                        lessongos[nameTemp].SetActive(true);
                    }
                }
            }
            else
            {
                if (lessongos[nameTemp].activeInHierarchy)
                {
                    lessongos[nameTemp].SetActive(false);
                }
            }
        }

        introClickCount++;
    }

    public void Disableyhynotice1()
    {

        Debug.Log("close notice:"+Time.realtimeSinceStartup);

        if (introClickCount == 2)
        {
            click(lessonNotice);
        }
        
    }
   
}
