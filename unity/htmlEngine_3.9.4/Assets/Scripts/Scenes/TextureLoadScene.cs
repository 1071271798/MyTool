using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextureLoadScene : BaseScene
{
    UIScrollView mScrollView;
    Transform mGrid;
    GameObject mImgItem;

    string mLoadTexturePath = Application.streamingAssetsPath + "/Textures";

    List<string> mTexturePathList;

    Dictionary<string, TextureData> mTextureDict;

    public TextureLoadScene()
    {
        mResPath = "textureLoadScene";
    }

    public override void FirstOpen()
    {
        try
        {
            base.FirstOpen();
            if (null != mTrans)
            {
                FindTextureList();
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
                Transform texture = mTrans.Find("texture");
                if (null != texture)
                {
                    mImgItem = texture.gameObject;
                }
                Transform top = mTrans.Find("top");
                if (null != top)
                {
                    GameHelper.SetPosition(top, UIWidget.Pivot.Top, new Vector2(0, 20));
                }
                mTextureDict = new Dictionary<string, TextureData>();
                for (int i = 0, imax = mTexturePathList.Count; i < imax; ++i)
                {
                    TextureData data = new TextureData();
                    data.path = mTexturePathList[i];
                    data.name = "img_" + i;
                    AddTextureItem(data, i);
                    mTextureDict[mTexturePathList[i]] = data;
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
            SingletonObject<TimeStatisticsTool>.GetInst().EventStart("Load_Res_Texture");
            mCompleteCount = 0;
            foreach (var item in mTextureDict)
            {
                if (null != item.Value.uiText)
                {
                    SingletonBehaviour<ResourcesLoad>.GetInst().Load(item.Key, LoadResType.Load_Res_Texture, delegate (object[] arg) {
                        if (null != arg && arg.Length > 0 && arg[0].GetType() == typeof(Texture2D))
                        {
                            item.Value.uiText.mainTexture = (Texture2D)arg[0];
                        }
                        CompleteOneTexture();
                    });
                }
            }
            
        } else if (obj.name.Equals("Btn_IO_Single"))
        {
            SingletonObject<TimeStatisticsTool>.GetInst().EventStart("Load_Res_Texture");
            mCompleteCount = 0;
            foreach (var item in mTextureDict)
            {
                if (null != item.Value.uiText)
                {
                    SingletonBehaviour<ResourcesLoad>.GetInst().LoadSingleThread(item.Key, LoadResType.Load_Res_Texture, delegate (object[] arg)
                    {
                        if (null != arg && arg.Length > 0 && arg[0].GetType() == typeof(Texture2D))
                        {
                            item.Value.uiText.mainTexture = (Texture2D)arg[0];
                        }
                        CompleteOneTexture();
                    });
                }
            }

        }
        else if (obj.name.Equals("Btn_WWW"))
        {
            SingletonObject<TimeStatisticsTool>.GetInst().EventStart("Load_Res_Texture");
            mCompleteCount = 0;
            foreach (var item in mTextureDict)
            {
                if (null != item.Value.uiText)
                {
                    SingletonBehaviour<ResourcesLoad>.GetInst().WWWLoad(item.Key, LoadResType.Load_Res_Texture, delegate (object[] arg)
                    {
                        if (null != arg && arg.Length > 0 && arg[0].GetType() == typeof(Texture2D))
                        {
                            item.Value.uiText.mainTexture = (Texture2D)arg[0];
                        }
                        CompleteOneTexture();
                    });
                }
            }
        } else if (obj.name.Equals("Btn_Unload"))
        {
            SingletonBehaviour<ResourcesLoad>.GetInst().UnLoad(LoadResType.Load_Res_Texture);
        }
    }

    void CompleteOneTexture()
    {
        ++mCompleteCount;
        if (mTextureDict.Count == mCompleteCount)
        {
            SingletonObject<TimeStatisticsTool>.GetInst().EventFinished("Load_Res_Texture", delegate (long time)
            {
                Debug.Log(time);
            });
        }
    }
    //////////////////////////////////////////////////////////////////////////
    void FindTextureList()
    {
        if (Directory.Exists(mLoadTexturePath))
        {
            mTexturePathList = new List<string>();
            string[] files = Directory.GetFiles(mLoadTexturePath);
            for (int i = files.Length - 1; i >= 0; --i)
            {
                if (!files[i].EndsWith(".meta"))
                {
                    mTexturePathList.Add(PublicFunction.ConvertSlashPath(files[i]));
                }
            }
        }
    }

    void AddTextureItem(TextureData data, int index)
    {
        data.itemTrans = GameObject.Instantiate(mImgItem).transform;
        data.uiText = GameHelper.FindChildComponent<UITexture>(data.itemTrans, "img");
        data.itemTrans.parent = mGrid;
        data.itemTrans.localEulerAngles = Vector3.zero;
        data.itemTrans.localScale = Vector3.one;
        data.itemTrans.name = data.name;
        int width = 100;
        int height = 100;
        int space = 20;
        int row = (PublicFunction.GetWidth() - 200) / (width + space);
        data.uiText.width = width;
        data.uiText.height = height;
        data.itemTrans.localPosition = new Vector3(width / 2.0f + index % row * (width + space), -height / 2.0f - index / row * (height + space), 0);
        GameHelper.SetLabelText(data.itemTrans.Find("Label"), data.name);
        BoxCollider box = data.itemTrans.GetComponent<BoxCollider>();
        if (null != box)
        {
            box.size = new Vector3(width, height);
        }
        data.itemTrans.gameObject.SetActive(true);
    } 


    ItemObjectEx CreateItemObjectCallback(GameObject obj)
    {
        ItemObjectEx objEx = new ItemObjectEx();
        objEx.itemObj = obj;
        objEx.childObj = obj.transform.Find("img").gameObject;
        objEx.childObj1 = obj.transform.Find("Label").gameObject;
        UIManager.SetButtonEventDelegate(objEx.itemObj.transform, mBtnDelegate);
        return objEx;
    }


    //////////////////////////////////////////////////////////////////////////
    private class TextureData
    {
        public string path;
        public string name;
        public UITexture uiText;
        public Transform itemTrans;

        public TextureData()
        {

        }
    }
}