using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnimLoadScene : BaseScene
{
    UIScrollView mScrollView;
    Transform mGrid;
    GameObject mItem;

    string mLoadPath = Application.streamingAssetsPath + "/Animation";

    List<string> mFilesPathList;

    Dictionary<string, ItemData> mDataDict;

    public AnimLoadScene()
    {
        mResPath = "animLoadScene";
    }

    public override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                FindFilesList();
                Transform list = mTrans.Find("list");
                if (null != list)
                {
                    mScrollView = list.GetComponent<UIScrollView>();
                    mGrid = list.Find("grid");
                    if (null != mGrid)
                    {
                        mGrid.transform.localPosition = new Vector2(-(PublicFunction.GetWidth() - 200) / 2.0f, (PublicFunction.GetHeight() - 200) / 2.0f - 60);
                    }
                    UIPanel uiPanel = list.GetComponent<UIPanel>();
                    if (null != uiPanel)
                    {
                        uiPanel.baseClipRegion = new Vector4(0, -60, PublicFunction.GetWidth() - 200, PublicFunction.GetHeight() - 200);
                    }
                }
                Transform item = mTrans.Find("item");
                if (null != item)
                {
                    mItem = item.gameObject;
                }
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    GameHelper.SetPosition(top, UIWidget.Pivot.Top, new Vector2(0, 20));
                }
                mDataDict = new Dictionary<string, ItemData>();
                for (int i = 0, imax = mFilesPathList.Count; i < imax; ++i)
                {
                    ItemData data = new ItemData();
                    data.path = mFilesPathList[i];
                    data.name = "item_" + i;
                    AddItem(data, i);
                    mDataDict[mFilesPathList[i]] = data;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    int mCompleteCount = 0;
    protected override void OnButtonClick(GameObject obj)
    {
        base.OnButtonClick(obj);
        if (obj.name.Equals("Btn_IO_Mu"))
        {
            foreach (var item in mDataDict)
            {
                item.Value.Destroy();
            }
            SingletonObject<TimeStatisticsTool>.GetInst().EventStart("Load_Res_Anim");
            mCompleteCount = 0;
            foreach (var item in mDataDict)
            {
                SingletonBehaviour<ResourcesLoad>.GetInst().Load(item.Key, LoadResType.Load_Single_Asset_Bundle, delegate (object[] arg)
                {
                    if (null != arg && arg.Length > 0)
                    {
                        foreach (var assetItem in arg)
                        {
                            AnimationClip assetObj = assetItem as AnimationClip;
                            item.Value.AddClip(assetObj, assetObj.name);
                        }
                    }
                    CompleteOneAsset();
                });
            }

        }
        else if (obj.name.Equals("Btn_IO_Single"))
        {
            foreach (var item in mDataDict)
            {
                item.Value.Destroy();
            }
            SingletonObject<TimeStatisticsTool>.GetInst().EventStart("Load_Res_Anim");
            mCompleteCount = 0;
            foreach (var item in mDataDict)
            {
                SingletonBehaviour<ResourcesLoad>.GetInst().LoadSingleThread(item.Key, LoadResType.Load_Single_Asset_Bundle, delegate (object[] arg)
                {
                    if (null != arg && arg.Length > 0)
                    {
                        foreach (var assetItem in arg)
                        {
                            AnimationClip assetObj = assetItem as AnimationClip;
                            item.Value.AddClip(assetObj, assetObj.name);
                        }
                    }
                    CompleteOneAsset();
                });
            }

        }
        else if (obj.name.Equals("Btn_WWW"))
        {
            foreach (var item in mDataDict)
            {
                item.Value.Destroy();
            }
            SingletonObject<TimeStatisticsTool>.GetInst().EventStart("Load_Res_Anim");
            mCompleteCount = 0;
            foreach (var item in mDataDict)
            {
                SingletonBehaviour<ResourcesLoad>.GetInst().WWWLoad(item.Key, LoadResType.Load_Single_Asset_Bundle, delegate (object[] arg)
                {
                    if (null != arg && arg.Length > 0)
                    {
                        foreach (var assetItem in arg)
                        {
                            AnimationClip assetObj = assetItem as AnimationClip;
                            item.Value.AddClip(assetObj, assetObj.name);
                        }
                    }
                    CompleteOneAsset();
                });
            }
        }
        else if (obj.name.Equals("Btn_Unload"))
        {
            foreach (var item in mDataDict)
            {
                item.Value.Destroy();
            }
            SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(LoadResType.Load_Single_Asset_Bundle);
        }
    }

    void CompleteOneAsset()
    {
        ++mCompleteCount;
        if (mDataDict.Count == mCompleteCount)
        {
            SingletonObject<TimeStatisticsTool>.GetInst().EventFinished("Load_Res_Anim", delegate (long time)
            {
                Debug.Log(time);
            });
        }
    }
    //////////////////////////////////////////////////////////////////////////
    void FindFilesList()
    {
        if (Directory.Exists(mLoadPath))
        {
            mFilesPathList = new List<string>();
            string[] files = Directory.GetFiles(mLoadPath);
            for (int i = files.Length - 1; i >= 0; --i)
            {
                if (!files[i].EndsWith(".meta"))
                {
                    mFilesPathList.Add(PublicFunction.ConvertSlashPath(files[i]));
                }
            }
        }
    }

    void AddItem(ItemData data, int index)
    {
        data.itemTrans = GameObject.Instantiate(mItem).transform;
        data.objTrans = data.itemTrans.Find("obj");
        data.animation = data.objTrans.GetComponent<Animation>();
        data.itemTrans.parent = mGrid;
        data.itemTrans.localEulerAngles = Vector3.zero;
        data.itemTrans.localScale = Vector3.one;
        data.itemTrans.name = data.name;
        int width = 100;
        int height = 100;
        int space = 20;
        int row = (PublicFunction.GetWidth() - 200) / (width + space);
        data.itemTrans.localPosition = new Vector3(width / 2.0f + index % row * (width + space), -height / 2.0f - index / row * (height + space), 0);
        GameHelper.SetLabelText(data.itemTrans.Find("Label"), data.name);
        BoxCollider box = data.itemTrans.GetComponent<BoxCollider>();
        if (null != box)
        {
            box.size = new Vector3(width, height);
        }
        data.itemTrans.gameObject.SetActive(true);
    }

    //////////////////////////////////////////////////////////////////////////
    private class ItemData
    {
        public string path;
        public string name;
        public Transform objTrans;
        public Transform itemTrans;
        public Animation animation;
        public List<string> clipNames;

        public ItemData()
        {
            clipNames = new List<string>();
        }

        public void AddClip(AnimationClip clip, string clipName)
        {
            animation.AddClip(clip, clipName);
            clipNames.Add(clipName);
        }

        public void Destroy()
        {
            if (null != animation && null != clipNames && clipNames.Count > 0)
            {
                for (int i = 0, imax = clipNames.Count; i < imax; ++i)
                {
                    AnimationClip clip = animation.GetClip(clipNames[i]);
                    animation.RemoveClip(clip);
                }
                clipNames.Clear();
            }
        }
    }
}