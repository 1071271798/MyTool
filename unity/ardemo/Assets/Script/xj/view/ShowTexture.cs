using UnityEngine;

public class ShowTexture : BasePopWin
{
    static ShowTexture sInst;
    static Texture2D sTexture;

    private UITexture mTexture;
    private float mAlpha;
    public ShowTexture(Texture2D texture, float alpha)
    {
        mUIResPath = "Prefab/UI/screenTexture";
        sTexture = texture;
        sInst = this;
        mAlpha = alpha;
    }


    public static void ShowMsg(Texture2D texture, float alpha = 1)
    {
        if (sInst == null)
        {
            object[] args = new object[2];
            args[0] = texture;
            args[1] = alpha;
            PopWinManager.GetInst().ShowPopWin(typeof(ShowTexture), args, 0, null, false);
        } else
        {
            if (!sInst.IsShow)
            {
                sInst.OnShow();
            }
            if (null != texture)
            {
                sInst.SetTexture(texture, alpha);
            }
        }
    }

    public static void CloseMsg()
    {
        if (null != sInst)
        {
            sInst.OnClose();
        }
    }

    public static void HideMsg()
    {
        if (null != sInst)
        {
            sInst.OnHide();
        }
    }

    public static Texture GetTexture2D()
    {
        Texture texture = null;
        if (null != sInst && null != sInst.mTexture && null != sInst.mTexture.mainTexture)
        {
            texture = sInst.mTexture.mainTexture;
        }
        return texture;
    }

    public static void ClearTexture2D()
    {
        if (null != sInst && null != sInst.mTexture && null != sInst.mTexture.mainTexture)
        {
            sInst.mTexture.mainTexture = null;
        }
    }

    public static UITexture GetUITexture()
    {
        if (null != sInst)
        {
            return sInst.mTexture;
        }
        return null;
    }

    public override void Init()
    {
        base.Init();
        mCoverAlpha = 0;
        mAddBox = false;
        SetInitDepth(9999);
    }


    protected override void AddEvent()
    {
        base.AddEvent();
        if (null != mTrans)
        {
            mTexture = GameHelper.FindChildComponent<UITexture>(mTrans, "Texture");
            if (null != mTexture)
            {
                mTexture.width = PublicFunction.GetWidth();
                mTexture.height = PublicFunction.GetHeight();
                mTexture.mainTexture = sTexture;
                mTexture.alpha = mAlpha;
            }
        }
    }

    protected override void Close()
    {
        base.Close();
        sInst = null;
        sTexture = null;
    }

    private void SetTexture(Texture2D tex, float alpha)
    {
        if (null != mTexture)
        {
            mTexture.mainTexture = tex;
            mTexture.alpha = alpha;
        }
    }
}
