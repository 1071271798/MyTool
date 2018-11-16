using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Game.Resource;
using Game.Platform;

public class AnimAssets : MonoBehaviour {

    private static AnimAssets _instance;
    string robotIDTemp;
    string robotNameNoType;
    public static AnimAssets Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AnimAssets();
            }
            return _instance;
        }
    }

    //public void Dispose()
    //{
    //    if (Instance != null) { Destroy(Instance); }
    //}

    public AnimAssets()
    {

        string rbtnametempt;
        if (RobotManager.GetInst().IsCreateRobotFlag)
        {
            rbtnametempt = RobotManager.GetInst().GetCreateRobot().Name;
        }
        else
        {
            rbtnametempt = RobotManager.GetInst().GetCurrentRobot().Name;
        }

        //Debug.Log("rbtnametempt:"+rbtnametempt);

        robotIDTemp = RobotMgr.Instance.FindRobotID(rbtnametempt);
        robotNameNoType = RobotMgr.NameNoType(rbtnametempt);
        animtype = new List<string>();  //动画的source
       allSprites = new List<string>();
       jubuSpritesName = new List<string>();//局部图名称
       innerSprites = new List<string>();//内置在app中的贴图资源
       outAddSprites = new List<string>();   //需要从外部加载的模型
    }

    private List<string> animtype=null;  //动画的source
    private List<string> allSprites = null;
    private List<string> jubuSpritesName = null;//局部图名称
    private Texture textureTT;
    private List<string> innerSprites = null;//内置在app中的贴图资源
    private List<string> outAddSprites = null;   //需要从外部加载的模型
    private bool noJuBu = false;

    /// <summary>
    /// 加载动画数据及一些基本处理
    /// </summary>
    public void FindOriClip()
    {
        //找到内置的图片
        innerSprites = GetInnerTexList.Instance.FindPicType();
        string pathTT = PublicFunction.CombinePath(ResourcesEx.GetCommonPathForNoTypeName(robotNameNoType), "JuBuPic");
        if (System.IO.Directory.Exists(pathTT) == false)
        {
            noJuBu = true;
        }
        //Debug.Log("rbtAnim:" + RobotMgr.Instance.rbtAnim.Count);
        //动画数据
        if (RobotMgr.Instance.rbtAnim.ContainsKey(robotIDTemp) == false)
        {
       
            // RobotMgr.Instance.CreateAnim(robotIDTemp);
            AnimData animDataT = new AnimData();
            AnimBase animb;
            Dictionary<string, string[]> animdata = new Dictionary<string, string[]>();
            if (AnimReadData.Instance != null)
            {
                AnimReadData.Instance.Dispose();
            }
            animdata = AnimReadData.Instance.FindAnimData();

            jubuSpritesName = new List<string>();

            for (int i = 1; i <= animdata.Count; i++)
            {
                animb = new AnimBase();
                string id = i.ToString();
                animb.id = animdata[id][0];
                animb.source = animdata[id][1];
                animb.start = animdata[id][2];
                animb.end = animdata[id][3];
                animb.step = animdata[id][4];
                animb.name = animdata[id][5];
                animb.parts = animdata[id][6];


                animb.djid = animdata[id][7];
                animb.shape = animdata[id][8];
                animb.line = animdata[id][9];
                animb.type = animdata[id][10];

                animb.goname = animdata[id][11];
                animb.pic = animdata[id][12];
                animb.firstPic = animdata[id][13];
                animb.lvdaiNum = animdata[id][14];
                animb.sensorID = animdata[id][15];
                if (animDataT.anims.ContainsKey(id) == false)
                {
                    animDataT.anims.Add(id, animb);
                }

                if (animtype.Contains(animb.type) == false)
                {
                    //Debug.Log("dffdsf:" + animb.type);
                    animtype.Add(animb.type);
                }

                if (allSprites.Contains(animdata[id][6]) == false)
                {
                    allSprites.Add(animdata[id][6]);

                }

                if (jubuSpritesName.Contains(animdata[id][12]) == false)
                {
                   
                    jubuSpritesName.Add(animdata[id][12]);
                }


            }
            RobotMgr.Instance.rbtAnim.Add(robotIDTemp, animDataT);


            if (jubuSpritesName != null && jubuSpritesName.Count > 0)
            {
                jubuSpritesName.Add("background");
            }

            foreach (string temp in allSprites)
            {
                if (innerSprites.Contains(temp) == false && temp != "")
                {
                    if (outAddSprites.Contains(temp) == false)
                    { 
                        outAddSprites.Add(temp);
                    }
                }
            }

            textureTT = Resources.Load("Prefab/Test4/UI/Clean") as Texture;

            if (noJuBu == false)
            {
                StartCoroutine(AddJuBuPic(jubuSpritesName, jubuPCount));
            }
            else
            {
                if (outAddSprites != null && outAddSprites.Count > 0)
                {
                    StartCoroutine(AddOtherPic(outAddSprites, outPCount));
                }
                else
                {
                    AddAnimAssets();
                }
            }
        }

    }


    int jubuPCount = 0;
    /// <summary>
    /// 局部图的处理
    /// </summary>
    /// <param name="pics"></param>
    /// <param name="pcount"></param>
    /// <returns></returns>
    IEnumerator AddJuBuPic(List<string> pics, int pcount)
    {
       
        if (pics.Count > pcount)
        {
            //零件图片
            string pathTemp = "file:///" + PublicFunction.CombinePath(ResourcesEx.GetCommonPathForNoTypeName(robotNameNoType), "JuBuPic") + pics[pcount] + ".png";
            WWW www = new WWW(pathTemp);

            Texture textureT = null;
            yield return www;
            try
            {
                if (www != null && string.IsNullOrEmpty(www.error))
                {
                    //获取Texture
                    textureT = www.texture;
                    //更多操作...    
                    if (textureT != null && RobotMgr.Instance.jubuPics.ContainsKey(pics[pcount]) == false)
                    {

                        //Debug.Log("dfsfdsf:"+pics[jubuPCount]);
                        RobotMgr.Instance.jubuPics.Add(pics[pcount], textureT);
                    }

                }
                else
                {
                    if (textureTT != null && RobotMgr.Instance.jubuPics.ContainsKey(pics[pcount]) == false)
                    {
                        RobotMgr.Instance.jubuPics.Add(pics[pcount], textureTT);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());
            }
            pcount++;
            if (pics.Count == RobotMgr.Instance.jubuPics.Count)
            {
                if (outAddSprites != null && outAddSprites.Count > 0)
                {
                    StartCoroutine(AddOtherPic(outAddSprites, outPCount));
                }
                else
                {
                    AddAnimAssets();
                }

            }
            else if (pcount <= pics.Count)
            {
                //Debug.Log("outNameT:"+outNamT[i]+";00outPics.Count:" + outPics.Count + ";00outNamT.Count:" + outNamT.Count);
                StartCoroutine(AddJuBuPic(pics, pcount));
            }
        }
    }


    int outPCount = 0;
    /// <summary>
    /// 添加其他图片
    /// </summary>
    /// <param name="outNamT"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    IEnumerator AddOtherPic(List<string> outNamT, int i)
    {

        if (outNamT.Count > i)
        {

            //零件图片
            //string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "//partsPic//" + outNamT[i] + ".png";
            string pathTemp = "file:///" + ResourcesEx.persistentDataPath + "/default/" + robotNameNoType + "/partsPic/" + outNamT[i] + ".png";

            WWW www = new WWW(pathTemp);

            Texture textureT = null;
            yield return www;
            try
            {
                if (www != null && string.IsNullOrEmpty(www.error))
                {
                    //获取Texture
                    textureT = www.texture;
                    //更多操作...    
                    if (textureT != null && RobotMgr.Instance.outPics.ContainsKey(outNamT[i]) == false)
                    {
                        RobotMgr.Instance.outPics.Add(outNamT[i], textureT);
                    }

                }
                else
                {
                    if (textureTT != null)
                    {
                        //Debug.Log("sfff:" + outNamT[i]);
                        RobotMgr.Instance.outPics.Add(outNamT[i], textureTT);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());

            }
            i++;
            if (RobotMgr.Instance.outPics.Count == outNamT.Count)
            {
                AddAnimAssets();
            }
            else if (i <= outNamT.Count)
            {
                //Debug.Log("outNameT:"+outNamT[i]+";00outPics.Count:" + outPics.Count + ";00outNamT.Count:" + outNamT.Count);
                StartCoroutine(AddOtherPic(outNamT, i));
            }

        }
    }

    string path1;
    AnimationClip clipTemp;
    /// <summary>
    /// 动画资源的处理
    /// </summary>
    public void AddAnimAssets()
    {
        List<string> RtID = NormalStringData.DefaultRtID();
        //Debug.Log("pics.Count 7777:");
        if (RtID.Contains(robotIDTemp))   //目前只有起落杆的动画
        {

            string opentype = RecordContactInfo.Instance.openType;

            string path = "Prefab/Test4/Anims/" + animtype[0];
            clipTemp = Resources.Load(path) as AnimationClip;

            clipTemp.name = animtype[0];

            if (clipTemp != null)
            {
                //Debug.Log("start press:" + clipTemp.name);
                RobotMgr.Instance.anims.Add(clipTemp.name, clipTemp);
            }

        }
        else
        {
            string opentype = RecordContactInfo.Instance.openType;
            
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/editor/" + robotIDTemp + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/ios/" + robotIDTemp + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/ios/" + robotIDTemp + ".assetbundle";
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                path1 = "file:///" + ResourcesEx.persistentDataPath + "/" + opentype + "/" + robotNameNoType + "/clip/android/" + robotIDTemp + ".assetbundle";
            }

            StartCoroutine(GetClip(robotIDTemp));
        }
    }

    
    /// <summary>
    /// 加载外部动画资源
    /// </summary>
    /// <param name="idtemp"></param>
    /// <returns></returns>
    IEnumerator GetClip(string idtemp)
    {
        clipTemp = null;
        WWW bundle1 = null;
        if (bundle1 != null)
        {
            bundle1.Dispose();

        }

        bundle1 = new WWW(path1);

        yield return bundle1;
        try
        {
            //Debug.Log("anims:" + RobotMgr.Instance.anims.Count);
            foreach (string temp in animtype)
            {
                if (RobotMgr.Instance.anims.ContainsKey(temp) == false)
                {
                    UnityEngine.Object t = bundle1.assetBundle.LoadAsset(temp);
                    //Debug.Log("animclipname:" + temp);
                    clipTemp = GameObject.Instantiate(t) as AnimationClip;
                    clipTemp.name = temp;
                    if (clipTemp != null)
                    {
                        RobotMgr.Instance.anims.Add(temp, clipTemp);
                    }
                    bundle1.assetBundle.Unload(true);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            PlatformMgr.Instance.Log(MyLogType.LogTypeInfo, this.GetType() + "-" + st.GetFrame(0).ToString() + "- error = " + ex.ToString());

        }

    }

}
